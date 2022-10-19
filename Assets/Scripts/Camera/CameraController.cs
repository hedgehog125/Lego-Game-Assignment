using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;

public class CameraController : MonoBehaviour {
	[Header("Objects and references")]
	[SerializeField] private CinemachineVirtualCamera cm;

	[Header("")]
	[SerializeField] private float m_deadZoomSpeed;
	[SerializeField] private float m_minDeadFOV;

	private bool playerIsDead;

	private float normalFOV;

	private Volume vol;
	private void Awake() {
		vol = GetComponent<Volume>();

		normalFOV = cm.m_Lens.FieldOfView;
	}

	public void RenderState(Player.Player player) {
		playerIsDead = player.Dead;
		if (player.Dead) {
			vol.enabled = true;
		}
		else {
			vol.enabled = false;
			cm.m_Lens.FieldOfView = normalFOV;
		}
	}
	public void LoopPosition(Vector3 moveAmount) {
		cm.ForceCameraPosition(transform.position + moveAmount, transform.rotation);
	}

	private void Update() {
		if (playerIsDead) {
			cm.m_Lens.FieldOfView = Mathf.Max(cm.m_Lens.FieldOfView - (m_deadZoomSpeed * Time.deltaTime), m_minDeadFOV);
		}
	}
}