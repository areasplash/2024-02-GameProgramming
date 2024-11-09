using UnityEngine;
using System;



// ====================================================================================================
// Animation Type
// ====================================================================================================

[Serializable] public enum AnimationType {
	Idle,
	Moving,
	Attacking,
	Dead,
}



// ====================================================================================================
// Creature Type
// ====================================================================================================

[Serializable] public enum CreatureType {
	None,
	Player,
}



// ====================================================================================================
// Creature
// ====================================================================================================

public class Creature : MonoBehaviour {

	void Update() {
		if (transform.hasChanged) {
			transform.GetChild(0).position = UIManager.I.GetPixelated(transform.position);
			transform.hasChanged = false;
		}
	}
	
}
