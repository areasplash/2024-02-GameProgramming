using UnityEngine;
using UnityEngine.UI;

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
		SerializedProperty m_MainRawImage;
		SerializedProperty m_FadeRawImage;
		SerializedProperty m_Target;
		SerializedProperty m_TargetPosition;

		CameraManager I => target as CameraManager;

		string[] layerNames;
		string[] LayerNames => layerNames ??= GetLayerNames();
		string[] GetLayerNames() {
			string[] names = new string[32];
			for (int i = 0; i < 32; i++) names[i] = LayerMask.LayerToName(i);
			return names;
		}

		void OnEnable() {
			m_MainCamera     = serializedObject.FindProperty("m_MainCamera");
			m_FadeCamera     = serializedObject.FindProperty("m_FadeCamera");
			m_MainRawImage   = serializedObject.FindProperty("m_MainRawImage");
			m_FadeRawImage   = serializedObject.FindProperty("m_FadeRawImage");
			m_Target         = serializedObject.FindProperty("m_Target");
			m_TargetPosition = serializedObject.FindProperty("m_TargetPosition");
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
			LabelField("Camera Transition", EditorStyles.boldLabel);
			I.DefaultMask    = MaskField("Default Mask",    I.DefaultMask,  LayerNames);
			I.ExteriorMask   = MaskField("Exterior Mask",   I.ExteriorMask, LayerNames);
			I.InteriorMask   = MaskField("Interior Mask",   I.InteriorMask, LayerNames);
			I.TransitionTime = Slider   ("Transition Time", I.TransitionTime, 0, 3);
			PropertyField(m_MainRawImage);
			PropertyField(m_FadeRawImage);
			Space();
			LabelField("Camera Tracking Controls", EditorStyles.boldLabel);
			PropertyField(m_Target);
			PropertyField(m_TargetPosition);
			I.TargetDistance = Slider("Target Distance", I.TargetDistance, 0, 256);
			BeginHorizontal();
			{
				PrefixLabel("Freeze Position");
				I.FreezePosition[0] = ToggleLeft("X", I.FreezePosition[0], GUILayout.Width(28));
				I.FreezePosition[1] = ToggleLeft("Y", I.FreezePosition[1], GUILayout.Width(28));
				I.FreezePosition[2] = ToggleLeft("Z", I.FreezePosition[2], GUILayout.Width(28));
			}
			EndHorizontal();
			BeginHorizontal();
			{
				PrefixLabel("Freeze Rotation");
				I.FreezeRotation[0] = ToggleLeft("X", I.FreezeRotation[0], GUILayout.Width(28));
				I.FreezeRotation[1] = ToggleLeft("Y", I.FreezeRotation[1], GUILayout.Width(28));
				I.FreezeRotation[2] = ToggleLeft("Z", I.FreezeRotation[2], GUILayout.Width(28));
			}
			EndHorizontal();
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

	[SerializeField] Vector2Int m_RenderTextureSize = Vector2Int.one;
	[SerializeField] float      m_FieldOfView       = 60.00f;
	[SerializeField] float      m_OrthographicSize  = 11.25f;
	[SerializeField] float      m_Projection        = 01.00f;

	[SerializeField] int      m_CommonLayer    = 0;
	[SerializeField] int      m_ExteriorLayer  = 0;
	[SerializeField] int      m_InteriorLayer  = 0;
	[SerializeField] float    m_TransitionTime = 0.5f;
	[SerializeField] RawImage m_MainRawImage   = null;
	[SerializeField] RawImage m_FadeRawImage   = null;

	[SerializeField] GameObject m_Target         = null;
	[SerializeField] Vector3    m_TargetPosition = Vector3.zero;
	[SerializeField] float      m_TargetDistance = 36.0f;
	[SerializeField] bool[]     m_FreezePosition = new bool[3];
	[SerializeField] bool[]     m_FreezeRotation = new bool[3];



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
			value.x = Mathf.Max(1, value.x);
			value.y = Mathf.Max(1, value.y);
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
			value = Mathf.Clamp(value, 1, 179);
			m_FieldOfView = value;
			if (MainCamera) MainCamera.fieldOfView = value;
			if (FadeCamera) FadeCamera.fieldOfView = value;
			Projection = Projection;
		}
	}

	public float OrthographicSize {
		get => m_OrthographicSize;
		set {
			value = Mathf.Max(0.01f, value);
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

	

	public int DefaultMask {
		get => m_CommonLayer;
		set => m_CommonLayer = value;
	}

	public int ExteriorMask {
		get => m_ExteriorLayer;
		set => m_ExteriorLayer = value;
	}

	public int InteriorMask {
		get => m_InteriorLayer;
		set => m_InteriorLayer = value;
	}

	public int CurrentMask { get; private set; }

	public float TransitionTime {
		get => m_TransitionTime;
		set => m_TransitionTime = value;
	}

	RawImage MainRawImage => m_MainRawImage;
	RawImage FadeRawImage => m_FadeRawImage;




	public GameObject Target {
		get => m_Target;
		set => m_Target = value;
	}

	public Vector3 TargetPosition {
		get => m_TargetPosition;
		set => m_TargetPosition = value;
	}

	public float TargetDistance {
		get => m_TargetDistance;
		set {
			m_TargetDistance = value;
			if (MainCamera) MainCamera.transform.localPosition = new Vector3(0, 0, -value);
			if (FadeCamera) FadeCamera.transform.localPosition = new Vector3(0, 0, -value);
		}
	}

	public bool[] FreezePosition {
		get => m_FreezePosition;
		set => m_FreezePosition = value;
	}

	public bool[] FreezeRotation {
		get => m_FreezeRotation;
		set => m_FreezeRotation = value;
	}



	// Methods

	public Ray ScreenPointToRay(Vector3 position) {
		if (MainCamera) {
			float multiplier = RenderTextureSize.x / Screen.width;
			Vector3 viewport = MainCamera.ScreenToViewportPoint(position * multiplier);
			return MainCamera.ViewportPointToRay(viewport);
		}
		return default;
	}

	

	static float   shakeStrength;
	static float   shakeDuration;
	static Vector3 shakeDirection;

	public void ShakeCamera(float strength, float duration) {
			shakeStrength = strength;
			shakeDuration = duration;
	}

	void UpdateCameraShake() {
		if (0 < shakeDuration) {
			shakeDuration = Mathf.Max(0, shakeDuration - Time.fixedDeltaTime);
			shakeDirection = Random.insideUnitSphere;
			MainCamera.transform.position += shakeDirection * shakeStrength;
			FadeCamera.transform.position += shakeDirection * shakeStrength;
		}
	}



	// Lifecycle

	Vector3 position;

	void UpdateTransform() {
		if (Target) TargetPosition = Target.transform.position;
		if (!FreezePosition[0] || !FreezePosition[1] || !FreezePosition[2]) {
			Vector3 a = transform.position;
			Vector3 b = TargetPosition;
			if (!FreezePosition[0]) a.x = b.x;
			if (!FreezePosition[1]) a.y = b.y;
			if (!FreezePosition[2]) a.z = b.z;
			transform.position = a;
		}
		if (!FreezeRotation[0] || !FreezeRotation[1] || !FreezeRotation[2]) {
			Vector3 direction = (TargetPosition - transform.position).normalized;
			if (direction != Vector3.zero) {
				Vector3 a = transform.eulerAngles;
				Vector3 b = Quaternion.LookRotation(direction).eulerAngles;
				if (!FreezeRotation[0]) a.x = b.x;
				if (!FreezeRotation[1]) a.y = b.y;
				if (!FreezeRotation[2]) a.z = b.z;
				transform.eulerAngles = a;
			}
		}
		if (transform.position != position) {
			float pixelPerUnit = UIManager.I.PixelPerUnit;
			Vector3 positionInversed = transform.InverseTransformDirection(transform.position);
			positionInversed.x = Mathf.Round(positionInversed.x * pixelPerUnit) / pixelPerUnit;
			positionInversed.y = Mathf.Round(positionInversed.y * pixelPerUnit) / pixelPerUnit;
			positionInversed.z = Mathf.Round(positionInversed.z * pixelPerUnit) / pixelPerUnit;
			//transform.position = position = transform.TransformDirection(positionInversed);
		}
	}



	int currentMask;

	void BeginDetectLayer() {
		currentMask = DefaultMask;
	}

	void DetectLayer(Collider collider) {
		if (collider.isTrigger) currentMask |= 1 << collider.gameObject.layer;
	}

	void EndDetectLayer() {
		if ((currentMask & ExteriorMask) == 0 && (currentMask & InteriorMask) == 0) {
			currentMask |= ExteriorMask;
		}
		CurrentMask = currentMask;
	}

	void UpdateLayer() {
		if (FadeRawImage.color.a == 0) {
			if (MainCamera.cullingMask != currentMask) FadeCamera.cullingMask = currentMask;
		}
		if (FadeCamera.cullingMask != 0) {
			float delta = Time.fixedDeltaTime / TransitionTime;
			float alpha = Mathf.Clamp01(FadeRawImage.color.a + delta);
			FadeRawImage.color = new Color(1, 1, 1, alpha);
		}
		if (FadeRawImage.color.a == 1) {
			MainCamera.cullingMask = FadeCamera.cullingMask;
			FadeCamera.cullingMask = 0;
			FadeRawImage.color = new Color(1, 1, 1, 0);
		}
	}



	void Start() => Projection = Projection;

	void LateUpdate() {
		UpdateTransform();
	}

	void FixedUpdate() {
		UpdateCameraShake();

		EndDetectLayer();
		UpdateLayer();
		BeginDetectLayer();
	}

	void OnTriggerStay(Collider collider) {
		DetectLayer(collider);
	}
}
