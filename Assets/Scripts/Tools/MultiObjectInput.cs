using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tools {
	[RequireComponent(typeof(PlayerInput))]
	public class MultiObjectInput : MonoBehaviour {
		private PlayerInput input;
		private void Awake() {
			input = GetComponent<PlayerInput>();
			Debug.Log(input.actionEvents[0].actionName);
		}
	}
}
