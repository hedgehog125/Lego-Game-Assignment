using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Environment;
using Cinemachine;

public class Player : MonoBehaviour {
	[Header("Objects and references")]
	[SerializeField] private ObstacleGenerator m_obstacleScript;
	[SerializeField] private CinemachineVirtualCamera m_cmCam;

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
		if (transform.position.z > m_loopAfter) LoopPosition();

		Vector3 vel = rb.velocity;
		vel.z = speed;
		rb.velocity = vel;

		speed += m_difficulty.speedupRate;
		if (speed > m_difficulty.maxSpeed) speed = m_difficulty.maxSpeed;
	}

	private void LoopPosition() {
		Vector3 moveAmount = new Vector3(0, 0, -transform.position.z);

		Vector3 pos = transform.position;
		pos.z = 0f;
		transform.position = pos;

		m_cmCam.ForceCameraPosition(m_cmCam.transform.position + moveAmount, m_cmCam.transform.rotation);

		m_obstacleScript.LoopPosition(moveAmount);
	}
}
