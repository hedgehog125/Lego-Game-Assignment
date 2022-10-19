using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Environment {
	[CreateAssetMenu]
	public class GeneratorData : ScriptableObject {
		[Header("Frequencies")]
		public Vector2Int rockCount;

		[Header("Misc")]
		public int clearStartLength;
	}
}
