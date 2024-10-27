using UnityEngine;



// ====================================================================================================
// Mono Singleton
// ====================================================================================================

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour {

	static T instance;

	public static T Instance {
		get => instance ??= FindAnyObjectByType<T>();
		private set => instance = value;
	}

	void Awake() {
		if (Instance == this) DontDestroyOnLoad(gameObject);
		else Destroy(gameObject);
	}
}
