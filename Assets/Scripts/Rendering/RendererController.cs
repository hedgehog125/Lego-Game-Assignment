using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

namespace Rendering {
	public class RendererController : MonoBehaviour {
		[SerializeField] private bool inGame = true;
		[SerializeField] private Player.Player m_player;
		[SerializeField] private float m_deathResReduction;

		private Resolution normalRes;
		private FullScreenMode normalFullScreenMode;
		private bool playerWasDead;
		private void Awake() {
			normalRes = Screen.currentResolution;
			normalFullScreenMode = Screen.fullScreenMode;

			Application.targetFrameRate = 60;
		}

		private void FixedUpdate() {
			if (inGame) {
				if (m_player.Dead != playerWasDead) {
					if (m_player.Dead) {
						Screen.SetResolution(
							Mathf.FloorToInt(normalRes.width / m_deathResReduction),
							Mathf.FloorToInt(normalRes.height / m_deathResReduction),
							normalFullScreenMode
						);
					}
					else {
						ResetDeath();
					}
					playerWasDead = m_player.Dead;
				}
			}
		}

		public void ResetDeath() {
			Screen.SetResolution(normalRes.width, normalRes.height, normalFullScreenMode);
		}
	}
}
