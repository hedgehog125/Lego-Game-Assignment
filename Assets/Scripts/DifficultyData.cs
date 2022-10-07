using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DifficultyData : ScriptableObject {
	[Header("Speed")]
	public float startSpeed;
	public float maxSpeed;
	public float speedupRate;

	[Header("")]
	public int stunTime;
}
