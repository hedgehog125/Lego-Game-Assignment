using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player {
	public class PlayerVisible : MonoBehaviour {
		[Header("Objects and references")]
		[SerializeField] private GameObject m_halo;
		[SerializeField] private GameObject m_cameraFollowPoint;

		[Header("SFX")]
		[SerializeField] private AudioSource m_bonkSound;
		[SerializeField] private float m_stunVolume;
		[SerializeField] private AudioSource m_deathSound;

		private float halfDuckHeightDiff;

		private Animator anim;
		public void Init(float _halfDuckHeightDiff) {
			anim = GetComponent<Animator>();

			halfDuckHeightDiff = _halfDuckHeightDiff;
		}

		public void RenderState(Player player) {
			m_halo.SetActive(player.Stunned);
			m_cameraFollowPoint.transform.localPosition = new Vector3(0, player.Ducking? halfDuckHeightDiff : 0, 0);

			anim.SetBool("Ducking", player.Ducking);
		}
		public void OnDie() {
			m_bonkSound.Stop();
			m_bonkSound.volume = 1;
			m_bonkSound.Play();

			m_deathSound.Play();
		}

		public void OnStun() {
			m_bonkSound.Stop();
			m_bonkSound.volume = m_stunVolume;
			m_bonkSound.Play();
		}
	}
}
