using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Camera Manager Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(CameraManager)), CanEditMultipleObjects]
	public class CameraManagerEditor : Editor {

		SerializedProperty m_MainCamera;
		SerializedProperty m_FadeCamera;
		SerializedProperty m_Target;

		CameraManager I => target as CameraManager;

		void OnEnable() {
			m_MainCamera = serializedObject.FindProperty("m_MainCamera");
			m_FadeCamera = serializedObject.FindProperty("m_FadeCamera");
			m_Target     = serializedObject.FindProperty("m_Target");
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
			I.RenderTextureSize = Vector2IntField("Render Texture Size", I.RenderTextureSize);
			I.FieldOfView       = FloatField     ("Field Of View",       I.FieldOfView);
			I.OrthographicSize  = FloatField     ("Orthographic Size",   I.OrthographicSize);
			I.Projection        = Slider         ("Projection",          I.Projection, 0, 1);
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
			PropertyField(m_Target);
			I.TargetPosition = Vector3Field("Target Position", I.TargetPosition);
			I.TargetDistance = Slider      ("Target Distance", I.TargetDistance, 0, 256);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Camera Manager
// ====================================================================================================

public class CameraManager : MonoSingleton<CameraManager> {

	// Serialized Fields

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

	Camera MainCamera => m_MainCamera;
	Camera FadeCamera => m_FadeCamera;

	public Vector3 Rotation {
		get => transform.eulerAngles;
		set => transform.eulerAngles = value;
	}

	public Vector2Int RenderTextureSize {
		get => m_RenderTextureSize;
		set {
			m_RenderTextureSize = value;
			if (MainCamera && MainCamera.targetTexture) {
				MainCamera.targetTexture.Release();
				MainCamera.targetTexture.width  = value.x;
				MainCamera.targetTexture.height = value.y;
				MainCamera.targetTexture.Create();
			}
			if (FadeCamera && FadeCamera.targetTexture) {
				FadeCamera.targetTexture.Release();
				FadeCamera.targetTexture.width  = value.x;
				FadeCamera.targetTexture.height = value.y;
				FadeCamera.targetTexture.Create();
			}
			Projection = Projection;
		}
	}

	public float FieldOfView {
		get => m_FieldOfView;
		set {
			m_FieldOfView = value;
			if (MainCamera) MainCamera.fieldOfView = value;
			if (FadeCamera) FadeCamera.fieldOfView = value;
			Projection = Projection;
		}
	}

	public float OrthographicSize {
		get => m_OrthographicSize;
		set {
			m_OrthographicSize = value;
			if (MainCamera) MainCamera.orthographicSize = value;
			if (FadeCamera) FadeCamera.orthographicSize = value;
			Projection = Projection;
		}
	}

	public float Projection {
		get => m_Projection;
		set {
			m_Projection = value;
			float aspect =  (float)RenderTextureSize.x / RenderTextureSize.y;
			float left   = -OrthographicSize * aspect;
			float right  =  OrthographicSize * aspect;
			float bottom = -OrthographicSize;
			float top    =  OrthographicSize;
			float nearClipPlane = MainCamera.nearClipPlane;
			float  farClipPlane = MainCamera. farClipPlane;

			Matrix4x4 a = Matrix4x4.Perspective(FieldOfView, aspect, nearClipPlane, farClipPlane);
			Matrix4x4 b = Matrix4x4.Ortho( left, right, bottom, top, nearClipPlane, farClipPlane);
			Matrix4x4 projection = MainCamera.projectionMatrix;
			for (int i = 0; i < 16; i++) projection[i] = Mathf.Lerp(a[i], b[i], value);
			if (MainCamera) MainCamera.projectionMatrix = projection;
			if (FadeCamera) FadeCamera.projectionMatrix = projection;
		}
	}



	public float TargetDistance {
		get => m_TargetDistance;
		set {
			m_TargetDistance = value;
			Vector3 position = Target ? Target.transform.position : TargetPosition;
			position += transform.forward * -value;
			if (MainCamera) MainCamera.transform.position = position;
			if (FadeCamera) FadeCamera.transform.position = position;
		}
	}

	public GameObject Target {
		get => m_Target;
		set => m_Target = value;
	}

	public Vector3 TargetPosition {
		get => m_TargetPosition;
		set => m_TargetPosition = value;
	}



	// Cached Variables

	static float   shakeStrength;
	static float   shakeDuration;
	static Vector3 shakeDirection;



	// Methods

	public void ShakeCamera(float strength, float duration) {
			shakeStrength = strength;
			shakeDuration = duration;
	}

	void UpdateCamera() {
		if (0 < shakeDuration) {
			shakeDuration = Mathf.Max(0, shakeDuration - Time.fixedDeltaTime);
			shakeDirection = Random.insideUnitSphere;
			MainCamera.transform.position += shakeDirection * shakeStrength;
			FadeCamera.transform.position += shakeDirection * shakeStrength;
		}
	}



	public Ray ScreenPointToRay(Vector3 position) {
		if (MainCamera) {
			float multiplier = RenderTextureSize.x / Screen.width;
			Vector3 viewport = MainCamera.ScreenToViewportPoint(position * multiplier);
			return MainCamera.ViewportPointToRay(viewport);
		}
		return default;
	}

	

	// Lifecycle
	
	void Start() => Projection = Projection;
	void FixedUpdate() => UpdateCamera();
}
