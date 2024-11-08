using UnityEngine;



public class MonoSingleton<T> : MonoBehaviour where T : Object {

	static T instance;

	public static T Instance => instance? instance : instance = FindAnyObjectByType<T>();
    
	protected virtual void Awake() {
		if (instance == null || instance == this) {
			instance = this as T;
			DontDestroyOnLoad(gameObject);
		}
		else Destroy(gameObject);
	}
}
