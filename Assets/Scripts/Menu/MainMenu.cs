using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Menu {
	public class MainMenu : MonoBehaviour {
		[SerializeField] private string m_playScene;

		private void OnTap(InputValue input) {
			if (! input.isPressed) return;

			SceneManager.LoadScene(m_playScene);
		}
	}
}