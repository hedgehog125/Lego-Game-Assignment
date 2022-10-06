using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools {	
	public static class Util {
		public static float MaxAbsolute(float value, float limit) {
			if (value > 0) return Mathf.Min(value, limit);
			else return Mathf.Max(value, -limit);
		}
	}
}

