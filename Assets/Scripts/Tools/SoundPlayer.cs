using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sound {
	public class SoundPlayer : MonoBehaviour {
		[SerializeField] private int m_maxSources = 10;
		[SerializeField] private List<SoundData> m_sounds;

		private AudioSource[] playingSources;

		private SoundData[] sounds;
		private void Awake() {
			sounds = m_sounds.ToArray();
			playingSources = new AudioSource[m_maxSources];
		}

		private void FixedUpdate() {
			for (int i = 0; i < playingSources.Length; i++) {
				AudioSource source = playingSources[i];
				if (source == null) continue;

				if (! source.isPlaying) {
					playingSources[i] = null;
					Destroy(source);
				}
			}
		}

		public AudioSource Play(int id) {
			SoundData sound = sounds[id];
			if (sound == null) throw new System.Exception("Unknown sound");

			AudioSource source = null;
			for (int i = 0; i < playingSources.Length; i++) {
				source = playingSources[i];
				if (source == null) {
					source = gameObject.AddComponent<AudioSource>();
					playingSources[i] = source;
					break;
				}
			}

			if (source == null) return null;

			source.clip = sound.clip;
			source.volume = sound.volume;
			source.priority = sound.priority;
			source.loop = sound.loop;
			source.Play();

			return source;
		}
	}
}
