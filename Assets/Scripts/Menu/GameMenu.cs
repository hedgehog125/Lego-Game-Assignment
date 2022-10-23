using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using RendererController = Rendering.RendererController;

namespace Menu {
	public class GameMenu : MonoBehaviour {
		[SerializeField] private Player.Player m_player;
		[SerializeField] private RendererController m_rendererController;
		[SerializeField] private string m_menuScene;
		[SerializeField] private int m_deathTextDelay;

		[Header("Internal")]
		[SerializeField] private GameObject m_gameOverText;
		[SerializeField] private TextMeshProUGUI m_scoreText;

		private int deathTextTick;

		private bool tapped;
		private void OnTap(InputValue input) {
			if (deathTextTick != m_deathTextDelay) return;

			if (tapped) { // Wait until it's released to fix some issues
				if (! input.isPressed) GoToMainMenu();
			}
			else {
				if (input.isPressed) tapped = true;
			}
		}

		private void FixedUpdate() {
			if (m_player.Dead) {
				if (deathTextTick == m_deathTextDelay) {
					m_gameOverText.SetActive(true);
				}
				else {
					deathTextTick++;
				}
			}

			m_scoreText.text = $"Score: {m_player.Score.ToString().PadLeft(7, '0')}";
		}

		private void GoToMainMenu() {
			Time.timeScale = 1;
			m_rendererController.ResetDeath();

			SceneManager.LoadScene(m_menuScene);
		}
	}
}