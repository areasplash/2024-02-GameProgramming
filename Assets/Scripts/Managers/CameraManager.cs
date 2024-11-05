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

	public Vector2Int renderTextureSize {
		get => m_RenderTextureSize;
		set {
			if (m_MainCamera?.targetTexture) {
				m_MainCamera.targetTexture.Release();
				m_MainCamera.targetTexture.width  = value.x;
				m_MainCamera.targetTexture.height = value.y;
				m_MainCamera.targetTexture.Create();
			}
			if (m_FadeCamera?.targetTexture) {
				m_FadeCamera.targetTexture.Release();
				m_FadeCamera.targetTexture.width  = value.x;
				m_FadeCamera.targetTexture.height = value.y;
				m_FadeCamera.targetTexture.Create();
			}
		}
	}

	public float fieldOfView {
		get => m_FieldOfView;
		set {
			m_FieldOfView = value;
			if (m_MainCamera) m_MainCamera.fieldOfView = m_FieldOfView;
			if (m_FadeCamera) m_FadeCamera.fieldOfView = m_FieldOfView;
		}
	}

	public float orthographicSize {
		get => m_OrthographicSize;
		set {
			m_OrthographicSize = value;
			if (m_MainCamera)m_MainCamera.orthographicSize = orthographicSize;
			if (m_FadeCamera)m_FadeCamera.orthographicSize = orthographicSize;
			projection = projection;
		}
	}

	public float projection {
		get => m_Projection;
		set {
			if (!m_MainCamera) return;
			m_Projection = value;
			float aspect = (float)m_MainCamera.targetTexture.width / m_MainCamera.targetTexture.height;
			float left   = -orthographicSize * aspect;
			float right  =  orthographicSize * aspect;
			float bottom = -orthographicSize;
			float top    =  orthographicSize;
			float nearClipPlane = m_MainCamera.nearClipPlane;
			float farClipPlane  = m_MainCamera.farClipPlane;

			Matrix4x4 a = Matrix4x4.Perspective(fieldOfView, aspect, nearClipPlane, farClipPlane);
			Matrix4x4 b = Matrix4x4.Ortho(left, right, bottom, top,  nearClipPlane, farClipPlane);
			Matrix4x4 m = m_MainCamera.projectionMatrix;
			for (int i = 0; i < 16; i++) m[i] = Mathf.Lerp(a[i], b[i], projection);
			if (m_MainCamera) m_MainCamera.projectionMatrix = m;
			if (m_FadeCamera) m_FadeCamera.projectionMatrix = m;
		}
	}

	public float targetDistance {
		get => m_TargetDistance;
		set {
			m_TargetDistance = value;
			Vector3 position = target ? target.transform.position : targetPosition;
			position += transform.forward * -m_TargetDistance;
			if (m_MainCamera) m_MainCamera.transform.position = position;
			if (m_FadeCamera) m_FadeCamera.transform.position = position;
		}
	}

	public GameObject target {
		get => m_Target;
		set => m_Target = value;
	}

	public Vector3 targetPosition {
		get => m_TargetPosition;
		set => m_TargetPosition = value;
	}



	// Methods

	public static Ray ScreenPointToRay(Vector3 position) {
		return Instance ? Instance.ScreenPointToRay_Internal(position) : new Ray();
	}
	Ray ScreenPointToRay_Internal(Vector3 position) {
		float multiplier = m_MainCamera.targetTexture.width / Screen.width;
		Vector3 viewport = m_MainCamera.ScreenToViewportPoint(position * multiplier);
		return m_MainCamera.ViewportPointToRay(viewport);
	}

	

	// Cycle

	void LateUpdate() => projection = projection;
}
