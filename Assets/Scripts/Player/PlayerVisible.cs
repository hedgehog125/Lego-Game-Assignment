using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player {
	public class PlayerVisible : MonoBehaviour {
		[SerializeField] private GameObject m_halo;
		public void Render(Player player) {
			m_halo.SetActive(player.Stunned);
			if (player.Ducking) {
				transform.localScale = Vector3.one / 2;
			}
			else {
				transform.localScale = Vector3.one;
			}
		}
	}
}
