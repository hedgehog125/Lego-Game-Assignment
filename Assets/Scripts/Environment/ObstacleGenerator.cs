using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Environment {
	[RequireComponent(typeof(MeshCollider))]
	[RequireComponent(typeof(Rigidbody))]
	public class ObstacleGenerator : MonoBehaviour {
		[SerializeField] private List<ObstacleData> obstacles;

		private MeshCollider col;
		private void Awake() {
			col = GetComponent<MeshCollider>();
		}

		public void LoopPosition(Vector3 moveAmount) {
			Debug.Log("TODO");
		}
	}
}
