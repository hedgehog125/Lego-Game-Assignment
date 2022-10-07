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

		[Header("Jumping, ducking and falling")]
		public float jumpAmount;

		public float fastFallAccceleration;
		public float duckHeight;
		public int duckTime;

		[Header("Death")]
		public float slowMoSpeed;
		public float deathKnockbackAmount;
		public float deathRotateAmount;
		public float deathSpeedMaintainance;
	}
}
