using UnityEngine;



public class InputManager : MonoSingleton<InputManager> {

	Vector3 mousePosition;
	Vector3 eulerAngles;

	void Update() {
		if (Input.GetKeyDown(KeyCode.Mouse0)) {
			Ray ray = CameraManager.ScreenPointToRay(Input.mousePosition);
			// 나중엔 보이는 것만 검출하도록 수정
			if (Physics.Raycast(ray, out RaycastHit hit)) {
				Debug.Log(hit.point);
			}
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
