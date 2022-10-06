using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CmCam : MonoBehaviour {
	private CinemachineVirtualCamera cam;
	private void Awake() {
		cam = GetComponent<CinemachineVirtualCamera>();
	}

	public void LoopPosition(Vector3 moveAmount) {
		cam.ForceCameraPosition(transform.position + moveAmount, transform.rotation);
	}
}
