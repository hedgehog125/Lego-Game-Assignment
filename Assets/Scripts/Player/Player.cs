using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Environment;
using Util = Tools.Util;

namespace Player {
	public class Player : MonoBehaviour {
		[Header("Objects and references")]
		[SerializeField] private ObstacleGenerator m_obstacleScript;
		[SerializeField] private CmCam m_cmCam;
		[SerializeField] private PlayerVisible vis;

		[Header("")]
		[SerializeField] private DifficultyData m_difficulty;
		[SerializeField] private MovementData m_movement;
		[SerializeField] private float m_loopAfter;


		private int lane = 1;
		private int switchingToLane = -1;
		private int switchLaneTick;
		private bool laneSwitchFailed;
		private float[] lanes;

		private int stunTick;
		private int duckTick;
		private bool fastFalling;

		private int[] stuckTicks = new int[2];
		private Vector3 lastPos;

		private RigidbodyConstraints normalContraints;
		private float standHeight;
		private float halfDuckHeightDiff;

		private float distanceMoved;
		private float speed;

		// Used by PlayerVisible
		public bool Stunned { get; private set; }
		public bool Ducking { get; private set; }
		public bool Dead { get; private set; }

		private CapsuleCollider col;
		private Rigidbody rb;
		private void Awake() {
			col = GetComponent<CapsuleCollider>();
			rb = GetComponent<Rigidbody>();

			speed = m_difficulty.startSpeed;
			lanes = new float[] {
				-m_movement.laneWidth,
				0,
				m_movement.laneWidth
			};
			lastPos = transform.position;

			normalContraints = rb.constraints;
			standHeight = col.height;
			halfDuckHeightDiff = (standHeight - m_movement.duckHeight) / 2;
		}

		private enum SwipeAction {
			None,
			Left,
			Right,
			Jump,
			Duck
		}
		private SwipeAction queuedInput = SwipeAction.None;
		private void OnLeft(InputValue input) {
			if (! input.isPressed) return;
			QueueLaneSwitch(false);
		}
		private void OnRight(InputValue input) {
			if (! input.isPressed) return;
			QueueLaneSwitch(true);
		}
		private void QueueLaneSwitch(bool isRight) {
			if (switchingToLane != -1) {
				queuedInput = isRight? SwipeAction.Right : SwipeAction.Left;
				return;
			}

			if (isRight) {
				if (lane != 2) {
					switchingToLane = lane + 1;
				}
			}
			else {
				if (lane != 0) {
					switchingToLane = lane - 1;
				}
			}
		}

		private bool jumpInput;
		private void OnJump(InputValue input) {
			if (! input.isPressed) {
				jumpInput = false;
				return;
			}

			if (jumpInput) queuedInput = SwipeAction.Jump;
			else jumpInput = true;
		}
		private bool duckInput;
		private void OnDuck(InputValue input) {
			if (! input.isPressed) {
				duckInput = false;
				return;
			}

			if (duckInput) queuedInput = SwipeAction.Duck;
			else duckInput = true;
		}

		private void FixedUpdate() {
			Vector3 vel = rb.velocity;
			Vector3 pos = transform.position;

			if (Dead) {
				vel.x *= m_movement.deathSpeedMaintainance;
				vel.z *= m_movement.deathSpeedMaintainance;
			}
			else {
				CheckStuckTick(pos);
				bool onGround = DetectGroundTick();
				LoopPositionTick(ref pos);
				LanesTick(ref pos, ref vel);
				JumpTick(ref vel, onGround);
				ProcessDuckTick(onGround, ref pos, ref vel);
				DeathPlaneTick(pos, ref vel);
				ProcessStunTick();
				CheckCrashedTick(ref vel);
				if (! Dead) {
					vel.z = speed;
					distanceMoved += speed;

					speed = Mathf.Min(speed + m_difficulty.speedupRate, m_difficulty.maxSpeed);
				}
			}
			rb.velocity = vel;
			transform.position = pos;
			
			vis.Render(this);
		}

		private void CheckStuckTick(Vector3 pos) {
			if (
				Mathf.Abs(pos.x - lastPos.x) > 0.015f
				|| switchingToLane == -1 // Only count it if changing lanes
			) {
				stuckTicks[0] = 0;
			}
			else {
				stuckTicks[0]++;
			}
			if (Mathf.Abs(pos.z - lastPos.z) > 0.015f) {
				stuckTicks[1] = 0;
			}
			else {
				stuckTicks[1]++;
			}

			lastPos = pos;
		}
		private bool DetectGroundTick() {
			Vector3 offset = new Vector3(0, -(col.height / 2) + 0.025f, 0);
			Vector3 bottom = col.bounds.center + offset;
			return Physics.Raycast(bottom, Vector3.down, 0.05f);
		}
		private void LoopPositionTick(ref Vector3 pos) {
			int multiple = Mathf.FloorToInt(pos.z / m_loopAfter);
			if (multiple == 0) return;

			Vector3 moveAmount = new Vector3(0, 0, -(multiple * m_loopAfter));

			pos += moveAmount;
			m_cmCam.LoopPosition(moveAmount);
			m_obstacleScript.LoopPosition(moveAmount);
		}

		private void LanesTick(ref Vector3 pos, ref Vector3 vel) {
			if (switchingToLane == -1) {
				pos.x = lanes[lane];
			}
			else {
				float targetX = lanes[switchingToLane];
				bool isRight = switchingToLane > lane;
				if (isRight? pos.x > targetX : pos.x < targetX) { // Reached target
					FinishLaneSwitch(ref pos, ref vel, targetX, isRight);
				}
				else {
					bool withinSlowDistance = Mathf.Abs(pos.x - targetX) < m_movement.slowDistanceBeforeLane;
					if (switchLaneTick == m_movement.maxLaneSwitchTime || (stuckTicks[0] >= 3 && (! withinSlowDistance))) { // Reverse
						if (laneSwitchFailed) { // Just teleport if it's gone wrong twice
							FinishLaneSwitch(ref pos, ref vel, targetX, isRight);
						}
						else {
							(switchingToLane, lane) = (lane, switchingToLane);
							switchLaneTick = 0;
							vel.x = lanes[switchingToLane] > pos.x?
								m_movement.laneSwitchAcceleration
								: -m_movement.laneSwitchAcceleration
							;
							laneSwitchFailed = true;

							Stun(ref vel);
						}
					}
					else {
						if (
							withinSlowDistance
							&& queuedInput == SwipeAction.None // Increase the speed if there's another input
						) {
							vel.x *= m_movement.slowDistanceMaintainance;
							if (Mathf.Abs(vel.x) < m_movement.laneSlowMinSpeed) {
								if (isRight) vel.x = m_movement.laneSlowMinSpeed;
								else vel.x = -m_movement.laneSlowMinSpeed;
							}
						}
						else {
							if (isRight) vel.x += m_movement.laneSwitchAcceleration;
							else vel.x -= m_movement.laneSwitchAcceleration;
							vel.x = Util.MaxAbsolute(vel.x, m_movement.laneSwitchMaxSpeed);
						}

						switchLaneTick++;
					}
				}

				if (switchingToLane == -1) {
					if (queuedInput == SwipeAction.Left || queuedInput == SwipeAction.Right) {
						QueueLaneSwitch(queuedInput == SwipeAction.Right);
						queuedInput = SwipeAction.None;
					}
				}
			}
		}
		private void FinishLaneSwitch(ref Vector3 pos, ref Vector3 vel, float targetX, bool isRight) {
			bool notContinuing = isRight?
				queuedInput != SwipeAction.Right
				: queuedInput != SwipeAction.Left
			;
			if (notContinuing) {
				pos.x = targetX;
			}
			vel.x = 0;
			lane = switchingToLane;

			switchingToLane = -1;
			switchLaneTick = 0;
			laneSwitchFailed = false;
		}

		private void JumpTick(ref Vector3 vel, bool onGround) {
			if (! jumpInput) {
				if (queuedInput == SwipeAction.Jump) {
					jumpInput = true;
					queuedInput = SwipeAction.None;
				}
			}

			if (jumpInput && onGround) { // TODO: buffer if not on ground, maybe differently to normal so 2 things can be buffered?
				vel.y = m_movement.jumpAmount;
				jumpInput = false;
				fastFalling = false;
			}
		}
		private void ProcessDuckTick(bool onGround, ref Vector3 pos, ref Vector3 vel) {
			if (! duckInput) {
				if (queuedInput == SwipeAction.Duck) {
					duckInput = true;
					queuedInput = SwipeAction.None;
				}
			}

			if (Ducking) {
				if (onGround) fastFalling = false;
				if (fastFalling) {
					vel.y -= m_movement.fastFallAccceleration;
				}

				if (duckTick == m_movement.duckTime) {
					Ducking = false;
					fastFalling = false;
					duckTick = 0;
					col.height = standHeight;
					pos.y += halfDuckHeightDiff;
				}
				else {
					duckTick++;
				}
			}
			else {
				if (duckInput) {
					if (! onGround) fastFalling = true;

					Ducking = true;
					col.height = m_movement.duckHeight;
					pos.y -= halfDuckHeightDiff;
				}
			}
		}
		private void ProcessStunTick() {
			if (stunTick == m_difficulty.stunTime) {
				Stunned = false;
				stunTick = 0;
			}
			else {
				stunTick++;
			}
		}
		private void CheckCrashedTick(ref Vector3 vel) {
			if (switchingToLane != -1) return;

			if (stuckTicks[1] >= 3) {
				GameOver(ref vel);
			}
		}
		private void DeathPlaneTick(Vector3 pos, ref Vector3 vel) {
			if (pos.y < -20) GameOver(ref vel);
		}

		private void Stun(ref Vector3 vel) {
			if (Stunned) {
				GameOver(ref vel);
				return;
			}
			Stunned = true;
		}
		private void GameOver(ref Vector3 vel) {
			Dead = true;

			rb.constraints = RigidbodyConstraints.None;
			vel = new Vector3(0, 0, -m_movement.deathKnockbackAmount);
			rb.angularVelocity = new Vector3(-m_movement.deathRotateAmount, 0, 0);
			Time.timeScale = m_movement.slowMoSpeed;
			Debug.Log("Game over");
		}
	}
}
