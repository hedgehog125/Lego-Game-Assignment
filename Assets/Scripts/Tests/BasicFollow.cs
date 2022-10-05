using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tests {
	public class BasicFollow : MonoBehaviour {
		[SerializeField] private Transform m_target;

		private Vector3 offset;
		private void Awake() {
			offset = transform.position - m_target.position;
		}
		private void LateUpdate() {
			transform.position = m_target.position + offset;
		}
	}
}
