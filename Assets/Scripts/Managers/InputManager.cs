using UnityEngine;



public class InputManager : MonoSingleton<InputManager> {

	public GameObject test;

	Vector3 mousePosition;
	Vector3 eulerAngles;

	void Update() {
		Vector3 viewport = CameraManager.Instance.Camera.ScreenToViewportPoint(Input.mousePosition);
		if (CameraManager.Instance.PixelPerfect) {
			float aspect = (float)Screen.width / Screen.height / 1.77778f;
			if (1 < aspect) viewport.y /= aspect;
			if (aspect < 1) viewport.x /= aspect;
		}
		Ray ray = CameraManager.Instance.Camera.ViewportPointToRay(viewport);
		//Debug.Log(viewport + " " + Screen.width + " " + Screen.height);
		//Ray ray = CameraManager.Instance.Camera.ScreenPointToRay(Input.mousePosition);
		bool isHit = Physics.Raycast(ray, out RaycastHit hit);
		Debug.DrawRay(ray.origin, isHit ? hit.point - ray.origin : ray.direction * 1000, Color.red);
		if (Input.GetKeyDown(KeyCode.Mouse0) && test && isHit) {
			Instantiate(test, hit.point, Quaternion.identity);
		}
		if (Input.GetKeyDown(KeyCode.Mouse1)) {
			mousePosition = Input.mousePosition;
			eulerAngles = CameraManager.Instance.transform.eulerAngles;
		}
		if (Input.GetKey(KeyCode.Mouse1)) {
			CameraManager.Instance.transform.rotation = Quaternion.Euler(
				eulerAngles.x,
				eulerAngles.y + (Input.mousePosition.x - mousePosition.x) * 1f,
				eulerAngles.z);
		}
		if (Input.mouseScrollDelta.y != 0) {
			float value = CameraManager.Instance.OrthographicSize * Mathf.Pow(2, -Input.mouseScrollDelta.y);
			CameraManager.Instance.OrthographicSize = Mathf.Clamp(value, 5.625f, 45f);
		}
	}
}
