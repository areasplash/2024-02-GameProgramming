using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
	using static CameraManager;
#endif



// ====================================================================================================
// Camera Manager Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(CameraManager)), CanEditMultipleObjects]
	public class CameraManagerEditor : Editor {

		SerializedProperty m_MainCamera;
		SerializedProperty m_FadeCamera;

		void OnEnable() {
			m_MainCamera = serializedObject.FindProperty("m_MainCamera");
			m_FadeCamera = serializedObject.FindProperty("m_FadeCamera");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change Camera Manager Properties");
			Space();
			LabelField("Camera", EditorStyles.boldLabel);
			PropertyField(m_MainCamera);
			PropertyField(m_FadeCamera);
			Space();
			LabelField("Camera Properties", EditorStyles.boldLabel);
			RenderTextureSize = Vector2IntField("Render Texture Size", RenderTextureSize);
			FieldOfView       = FloatField     ("Field Of View",       FieldOfView);
			OrthographicSize  = FloatField     ("Orthographic Size",   OrthographicSize);
			Projection        = Slider         ("Projection",          Projection, 0, 1);
			BeginHorizontal();
			{
				PrefixLabel(" ");
				GUIStyle l = new GUIStyle(GUI.skin.label) { alignment  = TextAnchor.MiddleLeft  };
				GUIStyle r = new GUIStyle(GUI.skin.label) { alignment  = TextAnchor.MiddleRight };
				GUIStyle s = new GUIStyle(GUI.skin.label) { fixedWidth = 50 };
				GUILayout.Label("< Perspective ", l);
				GUILayout.Label("Orthographic >", r);
				GUILayout.Label(" ", s);
			}
			EndHorizontal();
			Space();
			Target         = ObjectField ("Target", Target, typeof(GameObject), true) as GameObject;
			TargetPosition = Vector3Field("Target Position", TargetPosition);
			TargetDistance = Slider      ("Target Distance", TargetDistance, 0, 256);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Camera Manager
// ====================================================================================================

public class CameraManager : MonoSingleton<CameraManager> {

	// Fields

	[SerializeField] Camera     m_MainCamera;
	[SerializeField] Camera     m_FadeCamera;

	[SerializeField] Vector2Int m_RenderTextureSize = Vector2Int.zero;
	[SerializeField] float      m_FieldOfView       = 60.00f;
	[SerializeField] float      m_OrthographicSize  = 11.25f;
	[SerializeField] float      m_Projection        = 01.00f;

	[SerializeField] GameObject m_Target;
	[SerializeField] Vector3    m_TargetPosition;
	[SerializeField] float      m_TargetDistance = 36.0f;



	// Properties

	static Camera MainCamera => Instance? Instance.m_MainCamera : default;
	static Camera FadeCamera => Instance? Instance.m_FadeCamera : default;

	public static Vector3 Rotation {
		get =>    Instance? Instance.transform.eulerAngles : default;
		set { if (Instance) Instance.transform.eulerAngles = value; }
	}

	static RenderTexture texture;

	public static Vector2Int RenderTextureSize {
		get => Instance? Instance.m_RenderTextureSize : default;
		set {
			if (!Instance) return;
			Instance.m_RenderTextureSize = value;
			texture = MainCamera? MainCamera.targetTexture : null;
			if (texture) {
				texture.Release();
				texture.width  = value.x;
				texture.height = value.y;
				texture.Create();
			}
			texture = FadeCamera? MainCamera.targetTexture : null;
			if (texture) {
				texture.Release();
				texture.width  = value.x;
				texture.height = value.y;
				texture.Create();
			}
			Projection = Projection;
		}
	}

	public static float FieldOfView {
		get => Instance? Instance.m_FieldOfView : default;
		set {
			if (!Instance) return;
			Instance.m_FieldOfView = value;
			if (MainCamera) MainCamera.fieldOfView = value;
			if (FadeCamera) FadeCamera.fieldOfView = value;
			Projection = Projection;
		}
	}

	public static float OrthographicSize {
		get => Instance? Instance.m_OrthographicSize : default;
		set {
			if (!Instance) return;
			Instance.m_OrthographicSize = value;
			if (MainCamera) MainCamera.orthographicSize = value;
			if (FadeCamera) FadeCamera.orthographicSize = value;
			Projection = Projection;
		}
	}

	public static float Projection {
		get => Instance? Instance.m_Projection : default;
		set {
			if (!MainCamera) return;
			Instance.m_Projection = value;
			float aspect =  (float)RenderTextureSize.x / RenderTextureSize.y;
			float left   = -OrthographicSize * aspect;
			float right  =  OrthographicSize * aspect;
			float bottom = -OrthographicSize;
			float top    =  OrthographicSize;
			float nearClipPlane = MainCamera.nearClipPlane;
			float  farClipPlane = MainCamera. farClipPlane;

			Matrix4x4 a = Matrix4x4.Perspective(FieldOfView, aspect, nearClipPlane, farClipPlane);
			Matrix4x4 b = Matrix4x4.Ortho( left, right, bottom, top, nearClipPlane, farClipPlane);
			Matrix4x4 m = MainCamera.projectionMatrix;
			for (int i = 0; i < 16; i++) m[i] = Mathf.Lerp(a[i], b[i], value);
			if (MainCamera) MainCamera.projectionMatrix = m;
			if (FadeCamera) FadeCamera.projectionMatrix = m;
		}
	}

	public static float TargetDistance {
		get => Instance? Instance.m_TargetDistance : default;
		set {
			if (!Instance) return;
			Instance.m_TargetDistance = value;
			Vector3 position = Target ? Target.transform.position : TargetPosition;
			position += Instance.transform.forward * -value;
			if (MainCamera) MainCamera.transform.position = position;
			if (FadeCamera) FadeCamera.transform.position = position;
		}
	}

	public static GameObject Target {
		get =>    Instance? Instance.m_Target : default;
		set { if (Instance) Instance.m_Target = value; }
	}

	public static Vector3 TargetPosition {
		get =>    Instance? Instance.m_TargetPosition : default;
		set { if (Instance) Instance.m_TargetPosition = value; }
	}



	// Methods

	//static float   shakeStrength;
	static float   shakeDuration;
	//static Vector3 shakeDirection;

	public static void ShakeCamera(float strength, float duration) {
		if (!Instance) return;
		//shakeStrength  = strength;
		shakeDuration  = duration;
	}

	void FixedUpdate() {
		if (0 < shakeDuration) {
			shakeDuration = Mathf.Max(0, shakeDuration - Time.fixedDeltaTime);
			// shake
		}
	}

	

	public static Ray ScreenPointToRay(Vector3 position) {
		if (!MainCamera) return new Ray();
		float multiplier = RenderTextureSize.x / Screen.width;
		Vector3 viewport = MainCamera.ScreenToViewportPoint(position * multiplier);
		return MainCamera.ViewportPointToRay(viewport);
	}

	

	// Cycle

	void Start() => Projection = Projection;


}
