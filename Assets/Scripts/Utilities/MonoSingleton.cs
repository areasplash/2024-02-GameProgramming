using UnityEngine;



// ====================================================================================================
// Mono Singleton
// ====================================================================================================

public class MonoSingleton<T> : MonoBehaviour where T : Object {

	static T instance;

	public static T Instance => instance ??= FindAnyObjectByType<T>();

	protected virtual void Awake() {
		if (Instance == this) DontDestroyOnLoad(gameObject);
		else Destroy(gameObject);
	}

	protected virtual void OnApplicationQuit() => instance = null;
}
