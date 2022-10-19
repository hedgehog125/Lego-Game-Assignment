using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools {	
	public static class Util {
		public static float MaxAbsolute(float value, float limit) {
			if (value > 0) return Mathf.Min(value, limit);
			else return Mathf.Max(value, -limit);
		}
		public static float LoopAngle(float angle) { // From https://stackoverflow.com/questions/47680017/how-to-limit-angles-in-180-180-range-just-like-unity3d-inspector
			angle %= 360;
			angle = angle > 180? angle - 360 : angle;
			return angle;
		}
	}
}

