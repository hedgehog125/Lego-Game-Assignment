using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools {
	[RequireComponent(typeof(Rigidbody))]
	public class ConstantAxisSpeed : MonoBehaviour {
		private enum Axis {
			X,
			Y,
			Z
		}
		[SerializeField] private Axis m_axis;
		[SerializeField] private float m_amount;

		private Rigidbody rb;
		private void Awake() {
			rb = GetComponent<Rigidbody>();
		}

		private void FixedUpdate() {
			Vector3 vel = rb.velocity;
			if (m_axis == Axis.X) vel.x = m_amount;
			else if (m_axis == Axis.Y) vel.y = m_amount;
			else vel.z = m_amount;

			rb.velocity = vel;
		}
	}
}