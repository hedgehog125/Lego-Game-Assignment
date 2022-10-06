using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Environment;
using Util = Tools.Util;
using static UnityEditor.PlayerSettings;

namespace Player {
	public class Player : MonoBehaviour {
		[Header("Objects and references")]
		[SerializeField] private ObstacleGenerator m_obstacleScript;
		[SerializeField] private CmCam m_cmCam;

		[Header("")]
		[SerializeField] private DifficultyData m_difficulty;
		[SerializeField] private MovementData m_movement;
		[SerializeField] private float m_loopAfter;

		private int lane = 1;
		private int switchingToLane = -1;
		private int switchLaneTick;
		private bool laneSwitchFailed;
		private float[] lanes;

		private int[] stuckTicks = new int[2];
		private Vector3 lastPos;

		private float distanceMoved;
		private float speed;

		private Rigidbody rb;
		private void Awake() {
			rb = GetComponent<Rigidbody>();

			speed = m_difficulty.startSpeed;
			lanes = new float[] {
				-m_movement.laneWidth,
				0,
				m_movement.laneWidth
			};
			lastPos = transform.position;
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

		private void FixedUpdate() {
			Vector3 vel = rb.velocity;

			CheckStuckTick();
			LoopPositionTick();
			LanesTick(ref vel);

			vel.z = speed;

			distanceMoved += speed;

			speed = Mathf.Min(speed + m_difficulty.speedupRate, m_difficulty.maxSpeed);
			rb.velocity = vel;
		}

		private void CheckStuckTick() {
			Vector3 pos = transform.position;
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
		private void LoopPositionTick() {
			int multiple = Mathf.FloorToInt(transform.position.z / m_loopAfter);
			if (multiple == 0) return;

			Vector3 moveAmount = new Vector3(0, 0, -(multiple * m_loopAfter));

			transform.position += moveAmount;
			m_cmCam.LoopPosition(moveAmount);
			m_obstacleScript.LoopPosition(moveAmount);
		}

		private void LanesTick(ref Vector3 vel) {
			Vector3 pos = transform.position;

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
					if (switchLaneTick == m_movement.maxLaneSwitchTime || stuckTicks[0] >= 3) { // Reverse
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
						}
					}
					else {
						if (
							Mathf.Abs(pos.x - targetX) < m_movement.slowDistanceBeforeLane
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
					if (queuedInput != SwipeAction.None) {
						QueueLaneSwitch(queuedInput == SwipeAction.Right);
						queuedInput = SwipeAction.None;
					}
				}
			}

			transform.position = pos;
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
	}
}
