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
	public class GameManagerEditor : Editor {

		SerializedProperty m_MainCamera;
		SerializedProperty m_FadeCamera;
		SerializedProperty m_Target;
		SerializedProperty m_TargetPosition;

		CameraManager I => target as CameraManager;

		void OnEnable() {
			m_MainCamera     = serializedObject.FindProperty("m_MainCamera");
			m_FadeCamera     = serializedObject.FindProperty("m_FadeCamera");
			m_Target         = serializedObject.FindProperty("m_Target");
			m_TargetPosition = serializedObject.FindProperty("m_TargetPosition");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Space();
			LabelField("Camera", EditorStyles.boldLabel);
			PropertyField(m_MainCamera);
			PropertyField(m_FadeCamera);
			Space();
			LabelField("Camera Properties", EditorStyles.boldLabel);
			I.renderTextureSize = Vector2IntField("Render Texture Size", I.renderTextureSize);
			I.fieldOfView       = FloatField     ("Field Of View",       I.fieldOfView);
			I.orthographicSize  = FloatField     ("Orthographic Size",   I.orthographicSize);
			I.projection        = Slider         ("Projection",          I.projection, 0, 1);
			BeginHorizontal();
			PrefixLabel(" ");
			GUIStyle l = new GUIStyle(GUI.skin.label) { alignment  = TextAnchor.MiddleLeft  };
			GUIStyle r = new GUIStyle(GUI.skin.label) { alignment  = TextAnchor.MiddleRight };
			GUIStyle s = new GUIStyle(GUI.skin.label) { fixedWidth = 50 };
			GUILayout.Label("< Perspective ", l);
			GUILayout.Label("Orthographic >", r);
			GUILayout.Label(" ", s);
			EndHorizontal();
			Space();
			PropertyField(m_Target);
			if (!m_Target.objectReferenceValue) PropertyField(m_TargetPosition);
			I.targetDistance = FloatField("Target Distance", I.targetDistance);
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

	[SerializeField] Camera m_MainCamera;
	[SerializeField] Camera m_FadeCamera;

	[SerializeField] Vector2Int m_RenderTextureSize = Vector2Int.one;
	[SerializeField] float      m_FieldOfView       = 60.0f;
	[SerializeField] float      m_OrthographicSize  = 22.5f;
	[SerializeField] float      m_Projection        =  0.0f;

	[SerializeField] GameObject m_Target;
	[SerializeField] Vector3    m_TargetPosition;
	[SerializeField] float      m_TargetDistance = 36.0f;



	// Properties

	public Camera mainCamera { get => m_MainCamera; set => m_MainCamera = value; }
	public Camera fadeCamera { get => m_FadeCamera; set => m_FadeCamera = value; }

	public Vector2Int renderTextureSize {
		get => m_RenderTextureSize;
		set {
			if (mainCamera && mainCamera.targetTexture) {
				mainCamera.targetTexture.Release();
				mainCamera.targetTexture.width  = value.x;
				mainCamera.targetTexture.height = value.y;
				mainCamera.targetTexture.Create();
			}
			if (fadeCamera && fadeCamera.targetTexture) {
				fadeCamera.targetTexture.Release();
				fadeCamera.targetTexture.width  = value.x;
				fadeCamera.targetTexture.height = value.y;
				fadeCamera.targetTexture.Create();
			}
		}
	}

	public float fieldOfView {
		get => m_FieldOfView;
		set {
			m_FieldOfView = value;
			if (mainCamera) mainCamera.fieldOfView = m_FieldOfView;
			if (fadeCamera) fadeCamera.fieldOfView = m_FieldOfView;
		}
	}

	public float orthographicSize {
		get => m_OrthographicSize;
		set {
			m_OrthographicSize = value;
			if (mainCamera) mainCamera.orthographicSize = orthographicSize;
			if (fadeCamera) fadeCamera.orthographicSize = orthographicSize;
			projection = projection;
		}
	}

	public float projection {
		get => m_Projection;
		set {
			if (!mainCamera) return;
			m_Projection = value;
			float aspect = (float)mainCamera.targetTexture.width / mainCamera.targetTexture.height;
			float left   = -orthographicSize * aspect;
			float right  =  orthographicSize * aspect;
			float bottom = -orthographicSize;
			float top    =  orthographicSize;
			float nearClipPlane = mainCamera.nearClipPlane;
			float farClipPlane  = mainCamera.farClipPlane;

			Matrix4x4 a = Matrix4x4.Perspective(fieldOfView, aspect, nearClipPlane, farClipPlane);
			Matrix4x4 b = Matrix4x4.Ortho(left, right, bottom, top,  nearClipPlane, farClipPlane);
			Matrix4x4 m = mainCamera.projectionMatrix;
			for (int i = 0; i < 16; i++) m[i] = Mathf.Lerp(a[i], b[i], projection);
			if (mainCamera) mainCamera.projectionMatrix = m;
			if (fadeCamera) fadeCamera.projectionMatrix = m;
		}
	}

	public float targetDistance {
		get => m_TargetDistance;
		set {
			m_TargetDistance = value;
			Vector3 position = targetPosition + transform.forward * -m_TargetDistance;
			if (mainCamera) mainCamera.transform.position = position;
			if (fadeCamera) fadeCamera.transform.position = position;
		}
	}

	public GameObject target {
		get => m_Target;
		set => m_Target = value;
	}

	public Vector3 targetPosition {
		get => m_Target ? m_Target.transform.position : m_TargetPosition;
		set => m_TargetPosition = value;
	}



	// Methods

	public static Ray ScreenPointToRay(Vector3 position) {
		return Instance ? Instance.ScreenPointToRay_Internal(position) : new Ray();
	}
	Ray ScreenPointToRay_Internal(Vector3 position) {
		float multiplier = mainCamera.targetTexture.width / Screen.width;
		Vector3 viewport = mainCamera.ScreenToViewportPoint(position * multiplier);
		return mainCamera.ViewportPointToRay(viewport);
	}

	

	// Cycle

	void LateUpdate() => projection = projection;
}
