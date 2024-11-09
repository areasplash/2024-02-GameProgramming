using UnityEngine;



// ====================================================================================================
// Mono Singleton
// ====================================================================================================

public class MonoSingleton<T> : MonoBehaviour where T : Object {

	static T i;

	public static T I => i? i : i = FindAnyObjectByType<T>();

	protected virtual void Awake() {
		if (i == null || i == this) {
			i = this as T;
			DontDestroyOnLoad(gameObject);
		}
		else Destroy(gameObject);
	}
}
