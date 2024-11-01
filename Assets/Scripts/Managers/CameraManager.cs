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
			CameraManager I => target as CameraManager;

			T ObjectField<T>(string label, T obj) where T : Object {
				return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), true);
			}

			public override void OnInspectorGUI() {
				Space();
				LabelField("Camera", EditorStyles.boldLabel);
				I.MainCamera       = ObjectField ("Main Camera",       I.MainCamera      );
				I.FadeCamera       = ObjectField ("Fade Camera",       I.FadeCamera      );

				Space();
				LabelField("Settings", EditorStyles.boldLabel);
				I.FieldOfView      = FloatField  ("Field Of View",     I.FieldOfView     );
				I.OrthographicSize = FloatField  ("Orthographic Size", I.OrthographicSize);

				Space();
				I.Target           = ObjectField ("Target",             I.Target         );
				I.TargetPosition   = Vector3Field("Target Position",   I.TargetPosition  );
				I.CameraArmLength  = Slider      ("Focus Distance",    I.CameraArmLength, 0, 256);
				I.Projection       = Slider      ("Projection",        I.Projection,      0,   1);

				if (GUI.changed) EditorUtility.SetDirty(target);
			}
		}
	#endif



	// Methods

	public static Ray ScreenPointToRay(Vector3 position) {
		float multiplier = Instance.MainCamera.targetTexture.width / UIManager.Instance.Resolution.x;
		Vector3 viewport = Instance.MainCamera.ScreenToViewportPoint(position * multiplier);
		return Instance.MainCamera.ViewportPointToRay(viewport);
	}

	

	// Cycle

	void LateUpdate() => Projection = Projection;
}
