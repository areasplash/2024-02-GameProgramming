using UnityEngine;



public class MonoSingleton<T> : MonoBehaviour where T : Object {

	// ================================================================================================
	// Fields
	// ================================================================================================

	static T instance;

	public static T Instance => instance ??= FindAnyObjectByType<T>();



	// ================================================================================================
	// Methods
	// ================================================================================================

	protected virtual void Awake() {
		if (Instance.Equals(this)) {
			if (!transform.parent) DontDestroyOnLoad(gameObject);
		}
		else Destroy(gameObject);
	}

	protected virtual void OnApplicationQuit() => instance = null;
}
