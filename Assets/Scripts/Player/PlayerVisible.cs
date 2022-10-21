using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundPlayer = Sound.SoundPlayer;

namespace Player {
	public class PlayerVisible : MonoBehaviour {
		[Header("Objects and references")]
		[SerializeField] private GameObject m_halo;
		[SerializeField] private GameObject m_cameraFollowPoint;

		[Header("Misc")]
		[SerializeField] private int m_haloWaitTime;


		private int haloWaitTick;

		private float halfDuckHeightDiff;

		private Animator anim;
		private SoundPlayer snds;
		public void Init(float _halfDuckHeightDiff) {
			anim = GetComponent<Animator>();
			snds = GetComponent<SoundPlayer>();

			halfDuckHeightDiff = _halfDuckHeightDiff;
		}

		public void RenderState(Player player) {
			if (player.Stunned) {
				if (! (m_halo.activeSelf || player.Dead)) {
					if (haloWaitTick == m_haloWaitTime) {
						m_halo.SetActive(true);
						haloWaitTick = 0;
					}
					else {
						haloWaitTick++;
					}
				}
			}
			else {
				m_halo.SetActive(false);
			}
			m_cameraFollowPoint.transform.localPosition = new Vector3(0, player.Ducking? halfDuckHeightDiff : 0, 0);

			anim.SetBool("Ducking", player.Ducking);
		}
		public void OnDie() {
			snds.Play(1);
			snds.Play(2);
		}

		public void OnStun() {
			snds.Play(0);
		}
	}
}
