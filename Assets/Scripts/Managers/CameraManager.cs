using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Camera Manager
// ====================================================================================================

public class CameraManager : MonoSingleton<CameraManager> {

	// Fields

	[SerializeField] Camera     mainCamera        = null;
	[SerializeField] Camera     fadeCamera        = null;

	[SerializeField] float      fieldOfView       = 60.0f;
	[SerializeField] float      orthographicSize  = 22.5f;
	[SerializeField] bool       pixelPerfect      = true;
	[SerializeField] Vector2    resolution        = new Vector2(640, 360);
	[SerializeField] Vector2    reference         = new Vector2(640, 360);
	[SerializeField] float      pixelPerUnit      = 16.0f;

	[SerializeField] GameObject target            = null;
	[SerializeField] Vector3    targetPosition    = new Vector3(0, 0, 0);
	[SerializeField] float      cameraArmLength   = 36.0f;
	[SerializeField] float      projection        = 00.0f;



	// Properties

	public Camera MainCamera { get => mainCamera; set => mainCamera = value; }
	public Camera FadeCamera { get => fadeCamera; set => fadeCamera = value; }

	public float FieldOfView {
		get => fieldOfView;
		set {
			fieldOfView = value;
			if (MainCamera) MainCamera.fieldOfView = fieldOfView;
			if (FadeCamera) FadeCamera.fieldOfView = fieldOfView;
		}
	}

	public float OrthographicSize {
		get => orthographicSize;
		set {
			orthographicSize = value;
			if (MainCamera) MainCamera.orthographicSize = OrthographicSize;
			if (FadeCamera) FadeCamera.orthographicSize = OrthographicSize;
			Projection = Projection;
		}
	}

	public bool PixelPerfect {
		get => pixelPerfect;
		set {
			pixelPerfect = value;
			Resolution = Resolution;
			Projection = Projection;
		}
	}

	public Vector2 Reference {
		get => reference;
		set {
			reference = value;
			Resolution = Resolution;
		}
	}

	public Vector2 Resolution {
		get => resolution;
		set {
			if (resolution != value) {
				resolution  = value;
				Screen.SetResolution((int)resolution.x, (int)resolution.y, Screen.fullScreen);
			}
			Vector2 size = resolution;
			if (PixelPerfect) {
				int multiplier = 1;
				while (true) {
					bool xMatch = reference.x * multiplier < resolution.x;
					bool yMatch = reference.y * multiplier < resolution.y;
					if (xMatch && yMatch) multiplier++;
					else break;
				}
				size.x = Mathf.Ceil(resolution.x / multiplier);
				size.y = Mathf.Ceil(resolution.y / multiplier);
			}
			bool flag = MainCamera?.targetTexture;
			if (flag) flag &= MainCamera.targetTexture.width  != (int)size.x;
			if (flag) flag &= MainCamera.targetTexture.height != (int)size.y;
			if (flag) {
				MainCamera.targetTexture.Release();
				FadeCamera.targetTexture.Release();
				MainCamera.targetTexture.width  = (int)size.x;
				FadeCamera.targetTexture.width  = (int)size.x;
				MainCamera.targetTexture.height = (int)size.y;
				FadeCamera.targetTexture.height = (int)size.y;
				MainCamera.targetTexture.Create();
				FadeCamera.targetTexture.Create();
				OrthographicSize = size.y / 2 / PixelPerUnit;
			}
		}
	}

	public float PixelPerUnit {
		get => pixelPerUnit;
		set {
			pixelPerUnit = value;
			OrthographicSize = OrthographicSize;
		}
	}

	public GameObject Target {
		get => target;
		set => target = value;
	}

	public Vector3 TargetPosition {
		get => target ? target.transform.position : targetPosition;
		set => targetPosition = value;
	}

	public float CameraArmLength {
		get => cameraArmLength;
		set {
			cameraArmLength = value;
			Vector3 position = TargetPosition + transform.forward * -cameraArmLength;
			if (MainCamera) MainCamera.transform.position = position;
			if (FadeCamera) FadeCamera.transform.position = position;
		}
	}

	public float Projection {
		get => projection;
		set {
			if (!MainCamera) return;
			projection = value;
			float aspect = (float)MainCamera.targetTexture.width / MainCamera.targetTexture.height;
			float left   = -OrthographicSize * aspect;
			float right  =  OrthographicSize * aspect;
			float bottom = -OrthographicSize;
			float top    =  OrthographicSize;
			float nearClipPlane = MainCamera.nearClipPlane;
			float farClipPlane  = MainCamera.farClipPlane;

			Matrix4x4 a = Matrix4x4.Perspective(FieldOfView, aspect, nearClipPlane, farClipPlane);
			Matrix4x4 b = Matrix4x4.Ortho(left, right, bottom, top,  nearClipPlane, farClipPlane);
			Matrix4x4 m = MainCamera.projectionMatrix;
			for (int i = 0; i < 16; i++) m[i] = Mathf.Lerp(a[i], b[i], Projection);
			if (MainCamera) MainCamera.projectionMatrix = m;
			if (FadeCamera) FadeCamera.projectionMatrix = m;
		}
	}



	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(CameraManager)), CanEditMultipleObjects]
		public class GameManagerEditor : Editor {
			CameraManager i => target as CameraManager;

			T ObjectField<T>(string label, T obj) where T : Object {
				return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), true);
			}

			public override void OnInspectorGUI() {
				Space();
				LabelField("Camera", EditorStyles.boldLabel);
				i.MainCamera       = ObjectField ("Main Camera",       i.MainCamera      );
				i.FadeCamera       = ObjectField ("Fade Camera",       i.FadeCamera      );

				Space();
				LabelField("Settings", EditorStyles.boldLabel);
				i.FieldOfView      = FloatField  ("Field Of View",     i.FieldOfView     );
				i.OrthographicSize = FloatField  ("Orthographic Size", i.OrthographicSize);
				i.PixelPerfect     = Toggle      ("Pixel Perfect",     i.PixelPerfect    );
				i.Resolution       = Vector2Field("Resolution",        i.Resolution      );
				i.Reference        = Vector2Field("Reference",         i.Reference       );
				i.PixelPerUnit     = FloatField  ("Pixel Per Unit",    i.PixelPerUnit    );

				Space();
				i.Target           = ObjectField ("Target",             i.Target         );
				i.TargetPosition   = Vector3Field("Target Position",   i.TargetPosition  );
				i.CameraArmLength  = Slider      ("Focus Distance",    i.CameraArmLength, 0, 256);
				i.Projection       = Slider      ("Projection",        i.Projection,      0,   1);

				if (GUI.changed) EditorUtility.SetDirty(target);
			}
		}
	#endif



	// Methods

	public static Vector3 GetPixelated(Vector3 position) {
		position = Instance.transform.InverseTransformDirection(position);
		position.x = Mathf.Round(position.x * Instance.PixelPerUnit) / Instance.PixelPerUnit;
		position.y = Mathf.Round(position.y * Instance.PixelPerUnit) / Instance.PixelPerUnit;
		position.z = Mathf.Round(position.z * Instance.PixelPerUnit) / Instance.PixelPerUnit;
		return Instance.transform.TransformDirection(position);
	}

	public static Ray ScreenPointToRay(Vector3 position) {
		float multiplier = Instance.MainCamera.targetTexture.width / Instance.Resolution.x;
		Vector3 viewport = Instance.MainCamera.ScreenToViewportPoint(position * multiplier);
		return Instance.MainCamera.ViewportPointToRay(viewport);
	}

	

	// Cycle

	void Start() => Resolution = Resolution;

	void LateUpdate() {
		// 추후 최적화 필요
		if (transform.hasChanged) {
			CameraArmLength = CameraArmLength;
			transform.hasChanged = false;
		}
		if (Screen.width != Resolution.x || Screen.height != Resolution.y) {
			Resolution = new Vector2(Screen.width, Screen.height);
		}
		Projection = Projection;
	}
}
