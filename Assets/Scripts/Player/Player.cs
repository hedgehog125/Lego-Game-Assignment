using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Environment;
using Cinemachine;

public class Player : MonoBehaviour {
	[Header("Objects and references")]
	[SerializeField] private ObstacleGenerator m_obstacleScript;
	[SerializeField] private CmCam m_cmCam;

	[Header("")]
	[SerializeField] private Difficulty m_difficulty;
	[SerializeField] private float m_loopAfter;

	private int lane = 1;
	private float moved;
	private float speed;

	private Rigidbody rb;
	private void Awake() {
		rb = GetComponent<Rigidbody>();
		speed = m_difficulty.startSpeed;
	}

	private void FixedUpdate() {
		LoopPosition();

		Vector3 vel = rb.velocity;
		vel.z = speed;
		rb.velocity = vel;

		speed += m_difficulty.speedupRate;
		if (speed > m_difficulty.maxSpeed) speed = m_difficulty.maxSpeed;
	}

	private void LoopPosition() {
		int multiple = Mathf.FloorToInt(transform.position.z / m_loopAfter);
		Vector3 moveAmount = new Vector3(0, 0, -(multiple * m_loopAfter));

		transform.position += moveAmount;
		m_cmCam.LoopPosition(moveAmount);
		m_obstacleScript.LoopPosition(moveAmount);
	}
}
