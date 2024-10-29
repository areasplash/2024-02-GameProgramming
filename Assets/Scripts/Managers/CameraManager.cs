using UnityEngine;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Camera Manager
// ====================================================================================================

public class CameraManager : MonoSingleton<CameraManager> {

	[SerializeField] Camera mainCamera;
	[SerializeField] Camera viewCamera;
	[SerializeField] Camera fadeCamera;

	[SerializeField] bool  pixelPerfect     =   true;
	[SerializeField] float orthographicSize =  22.5f;
	[SerializeField] float test             =   0.0f;
	[SerializeField] float focusDistance    =  40.0f;

	[SerializeField] GameObject target;
	[SerializeField] Vector3    targetPosition;



	public Camera Camera => PixelPerfect ? viewCamera : mainCamera;

	public bool PixelPerfect {
		get => pixelPerfect;
		set {
			pixelPerfect = value;
			if (mainCamera) mainCamera.cullingMask = PixelPerfect ? 0 : int.MaxValue;
			if (viewCamera) viewCamera.cullingMask = PixelPerfect ? int.MaxValue : 0;
			if (fadeCamera) fadeCamera.cullingMask = PixelPerfect ? int.MaxValue : 0;
		}
	}

	public float OrthographicSize {
		get => orthographicSize;
		set {
			orthographicSize = value;
			if (mainCamera) mainCamera.orthographicSize = OrthographicSize;
			if (viewCamera) viewCamera.orthographicSize = OrthographicSize;
			if (fadeCamera) fadeCamera.orthographicSize = OrthographicSize;
			Test = Test;
		}
	}

	public float Test {
		get => test;
		set {
			test = value;
			float aspect = PixelPerfect ? (float)viewTexture.width / viewTexture.height : mainCamera.aspect;
			float fov    = mainCamera.fieldOfView;
			float near   = mainCamera.nearClipPlane;
			float far    = mainCamera.farClipPlane;
			float left   = -OrthographicSize * aspect;
			float right  =  OrthographicSize * aspect;
			float bottom = -OrthographicSize;
			float top    =  OrthographicSize;
			Matrix4x4 ortho       = Matrix4x4.Ortho(left, right, bottom, top, near, far);
			Matrix4x4 perspective = Matrix4x4.Perspective(fov, aspect, near, far);
			Matrix4x4 matrix4x4   = mainCamera.projectionMatrix;
			for (int i = 0; i < 16; i++) matrix4x4[i] = Mathf.Lerp(ortho[i], perspective[i], Test);
			if (mainCamera) mainCamera.projectionMatrix = matrix4x4;
			if (viewCamera) viewCamera.projectionMatrix = matrix4x4;
			if (fadeCamera) fadeCamera.projectionMatrix = matrix4x4;
		}
	}

	public float FocusDistance {
		get => focusDistance;
		set {
			focusDistance = value;
			transform.position = TargetPosition + transform.forward * -FocusDistance;
		}
	}

	public Vector3 TargetPosition => target ? target.transform.position : targetPosition;

	

	[SerializeField] RenderTexture viewTexture;
	[SerializeField] RenderTexture fadeTexture;
	[SerializeField] RectTransform viewImage;
	[SerializeField] RectTransform fadeImage;

	Vector2 resolution = new Vector2(640, 360);
	Vector2 reference  = new Vector2(640, 360);

	public Vector2 Resolution {
		get => resolution;
		set {
			resolution = value;
			int multiplier = 1;
			while (true) {
				bool xMatch = reference.x * multiplier < resolution.x * 0.75f;
				bool yMatch = reference.y * multiplier < resolution.y * 0.75f;
				if (xMatch && yMatch) multiplier++;
				else break;
			}
			Vector2 temp;
			temp.x = Mathf.Ceil(resolution.x / multiplier);
			temp.y = Mathf.Ceil(resolution.y / multiplier);
			
			Screen.SetResolution((int)resolution.x, (int)resolution.y, Screen.fullScreen);
			viewTexture.Release();
			fadeTexture.Release();
			viewTexture.width  = (int)temp.x;
			fadeTexture.width  = (int)temp.x;
			viewTexture.height = (int)temp.y;
			fadeTexture.height = (int)temp.y;
			viewTexture.Create();
			fadeTexture.Create();
			viewImage.sizeDelta = temp;
			fadeImage.sizeDelta = temp;
			Debug.Log("Resolution: " + resolution + " " + temp + " " + multiplier);
			Debug.Log(viewTexture.width / viewTexture.height);
			Test = Test;
		}
	}



	void Start() => Test = Test;

	void LateUpdate() {
		if (transform.hasChanged) {
			transform.position = TargetPosition + transform.forward * -FocusDistance;
			mainCamera.transform.position = GetPixelated(transform.position);
			viewCamera.transform.position = GetPixelated(transform.position);
			fadeCamera.transform.position = GetPixelated(transform.position);
			transform.hasChanged = false;
		}
		Vector2 resolution = new Vector2(Screen.width, Screen.height);
		if (Resolution != resolution) Resolution = resolution;
	} 



	const int PixelPerUnit = 16;

	public static Vector3 GetPixelated(Vector3 position) {
		position = Instance.transform.InverseTransformDirection(position);
		position.x = Mathf.Round(position.x * PixelPerUnit) / PixelPerUnit;
		position.y = Mathf.Round(position.y * PixelPerUnit) / PixelPerUnit;
		position.z = Mathf.Round(position.z * PixelPerUnit) / PixelPerUnit;
		return Instance.transform.TransformDirection(position);
	}

/*
	public static Vector3 GetPixelated(Vector3 position) {
		position.x = Mathf.Round(position.x * PixelPerUnit) / PixelPerUnit;
		position.y = Mathf.Round(position.y * PixelPerUnit) / PixelPerUnit;
		position.z = Mathf.Round(position.z * PixelPerUnit) / PixelPerUnit;
		return position;
	}

	public static Vector3 GetPixelated(Vector3 position, ref Vector3 prevProjected) {
		Vector3 nextProjected = Instance.transform.InverseTransformDirection(position);
		Vector3 prevPixelated = GetPixelated(prevProjected);
		Vector3 nextPixelated = GetPixelated(nextProjected);

		float xDelta = nextProjected.x - prevProjected.x;
		float yDelta = nextProjected.y - prevProjected.y;
		switch (Mathf.Atan2(yDelta, xDelta) * Mathf.Rad2Deg) {
			case float angle0 when  -22.5f < angle0 && angle0 <=   22.5f:
			case float angle1 when  157.5f < angle1 || angle1 <= -157.5f:
				nextPixelated.y = prevPixelated.y;
				break;
			case float angle0 when   67.5f < angle0 && angle0 <=  112.5f:
			case float angle1 when -112.5f < angle1 && angle1 <= - 67.5f:
				nextPixelated.x = prevPixelated.x;
				break;
		}
		prevProjected = nextProjected;
		return Instance.transform.TransformDirection(nextPixelated);
	}
*/
}



// ====================================================================================================
// Game Manager Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(CameraManager))]
	public class GameManagerEditor : Editor {
		public override void OnInspectorGUI() {
			serializedObject.Update();
			CameraManager i = (CameraManager)target;

			Space();
			PropertyField(serializedObject.FindProperty("mainCamera"));
			PropertyField(serializedObject.FindProperty("viewCamera"));
			PropertyField(serializedObject.FindProperty("fadeCamera"));

			Space();
			PropertyField(serializedObject.FindProperty("viewTexture"));
			PropertyField(serializedObject.FindProperty("fadeTexture"));
			PropertyField(serializedObject.FindProperty("viewImage"));
			PropertyField(serializedObject.FindProperty("fadeImage"));

			Space();
			i.PixelPerfect     = Toggle("Pixel Perfect",     i.PixelPerfect);
			i.OrthographicSize = Slider("Orthographic Size", i.OrthographicSize, 0, 256);
			i.Test             = Slider("Test",              i.Test,             0,   1);
			i.FocusDistance    = Slider("Focus Distance",    i.FocusDistance,    0, 256);

			Space();
			PropertyField(serializedObject.FindProperty("target"        ));
			PropertyField(serializedObject.FindProperty("targetPosition"));

			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif
