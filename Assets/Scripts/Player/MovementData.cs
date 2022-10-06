using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player {
	[CreateAssetMenu]
	public class MovementData : ScriptableObject {
		[Header("Lanes")]
		public float laneWidth;
		public float laneSwitchMaxSpeed;
		public float laneSwitchAcceleration;
		public int maxLaneSwitchTime;
		public float slowDistanceBeforeLane;
		public float slowDistanceMaintainance;
		public float laneSlowMinSpeed;

		[Header("Jumping and falling")]
		public float jumpAmount;
		public float fastFallAccceleration;
	}
}
