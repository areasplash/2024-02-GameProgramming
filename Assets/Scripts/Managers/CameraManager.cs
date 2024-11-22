using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

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
		SerializedProperty m_MainRawImage;
		SerializedProperty m_FadeRawImage;
		SerializedProperty m_AbsoluteLayer;
		SerializedProperty m_ExteriorLayer;
		SerializedProperty m_InteriorLayer;
		SerializedProperty m_Target;
		SerializedProperty m_TargetPosition;

		void OnEnable() {
			m_MainCamera     = serializedObject.FindProperty("m_MainCamera");
			m_FadeCamera     = serializedObject.FindProperty("m_FadeCamera");
			m_MainRawImage   = serializedObject.FindProperty("m_MainRawImage");
			m_FadeRawImage   = serializedObject.FindProperty("m_FadeRawImage");
			m_AbsoluteLayer  = serializedObject.FindProperty("m_AbsoluteLayer");
			m_ExteriorLayer  = serializedObject.FindProperty("m_ExteriorLayer");
			m_InteriorLayer  = serializedObject.FindProperty("m_InteriorLayer");
			m_Target         = serializedObject.FindProperty("m_Target");
			m_TargetPosition = serializedObject.FindProperty("m_TargetPosition");
		}

		string[] layerNames;
		string[] LayerNames {
			get {
				if (layerNames == null) {
					layerNames = new string[32];
					for (int i = 0; i < 32; i++) layerNames[i] = LayerMask.LayerToName(i);
				}
				return layerNames;
			}
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
			LabelField("Camera Transition", EditorStyles.boldLabel);
			m_AbsoluteLayer.intValue = MaskField("Absolute Layer",  AbsoluteLayer, LayerNames);
			m_ExteriorLayer.intValue = MaskField("Exterior Layer", ExteriorLayer, LayerNames);
			m_InteriorLayer.intValue = MaskField("Interior Layer", InteriorLayer, LayerNames);
			TransitionTime = Slider   ("Transition Time", TransitionTime, 0, 3);
			PropertyField(m_MainRawImage);
			PropertyField(m_FadeRawImage);
			Space();
			LabelField("Camera Tracking Controls", EditorStyles.boldLabel);
			PropertyField(m_Target);
			PropertyField(m_TargetPosition);
			TargetDistance = Slider("Target Distance", TargetDistance, 0, 256);
			BeginHorizontal();
			{
				PrefixLabel("Freeze Position");
				FreezePosition[0] = ToggleLeft("X", FreezePosition[0], GUILayout.Width(28));
				FreezePosition[1] = ToggleLeft("Y", FreezePosition[1], GUILayout.Width(28));
				FreezePosition[2] = ToggleLeft("Z", FreezePosition[2], GUILayout.Width(28));
			}
			EndHorizontal();
			BeginHorizontal();
			{
				PrefixLabel("Freeze Rotation");
				FreezeRotation[0] = ToggleLeft("X", FreezeRotation[0], GUILayout.Width(28));
				FreezeRotation[1] = ToggleLeft("Y", FreezeRotation[1], GUILayout.Width(28));
				FreezeRotation[2] = ToggleLeft("Z", FreezeRotation[2], GUILayout.Width(28));
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

	[SerializeField] int        m_AbsoluteLayer     = 0;
	[SerializeField] int        m_ExteriorLayer     = 0;
	[SerializeField] int        m_InteriorLayer     = 0;
	[SerializeField] float      m_TransitionTime    = 0.5f;
	[SerializeField] RawImage   m_MainRawImage      = null;
	[SerializeField] RawImage   m_FadeRawImage      = null;

	[SerializeField] GameObject m_Target            = null;
	[SerializeField] Vector3    m_TargetPosition    = Vector3.zero;
	[SerializeField] float      m_TargetDistance    = 36.0f;
	[SerializeField] bool[]     m_FreezePosition    = new bool[3];
	[SerializeField] bool[]     m_FreezeRotation    = new bool[3];



	// Properties

	public static Quaternion Rotation {
		get   =>  Instance? Instance.transform.rotation : default;
		set { if (Instance) Instance.transform.rotation = value; }
	}

	public static Vector3 EulerRotation {
		get   =>  Instance? Instance.transform.eulerAngles : default;
		set { if (Instance) Instance.transform.eulerAngles = value; }
	}



	static Camera MainCamera => Instance? Instance.m_MainCamera : default;
	static Camera FadeCamera => Instance? Instance.m_FadeCamera : default;
	static Camera MainCameraDirect => Instance.m_MainCamera;
	static Camera FadeCameraDirect => Instance.m_FadeCamera;

	static RenderTexture MainTexture => MainCamera? MainCameraDirect.targetTexture : default;
	static RenderTexture FadeTexture => FadeCamera? FadeCameraDirect.targetTexture : default;
	static RenderTexture MainTextureDirect => MainCameraDirect.targetTexture;
	static RenderTexture FadeTextureDirect => FadeCameraDirect.targetTexture;



	public static Vector2Int RenderTextureSize {
		get => Instance? Instance.m_RenderTextureSize : default;
		set {
			value.x = Mathf.Max(1, value.x);
			value.y = Mathf.Max(1, value.y);
			if (Instance) Instance.m_RenderTextureSize = value;
			if (MainTexture) {
				MainTextureDirect.Release();
				MainTextureDirect.width  = value.x;
				MainTextureDirect.height = value.y;
				MainTextureDirect.Create();
			}
			if (FadeTexture) {
				FadeTextureDirect.Release();
				FadeTextureDirect.width  = value.x;
				FadeTextureDirect.height = value.y;
				FadeTextureDirect.Create();
			}
			Projection = Projection;
		}
	}

	public static float FieldOfView {
		get => Instance? Instance.m_FieldOfView : default;
		set {
			value = Mathf.Clamp(value, 1, 179);
			if (Instance) Instance.m_FieldOfView = value;
			if (MainCamera) MainCameraDirect.fieldOfView = value;
			if (FadeCamera) FadeCameraDirect.fieldOfView = value;
			Projection = Projection;
		}
	}

	public static float OrthographicSize {
		get => Instance? Instance.m_OrthographicSize : default;
		set {
			value = Mathf.Max(0.01f, value);
			if (Instance) Instance.m_OrthographicSize = value;
			if (MainCamera) MainCameraDirect.orthographicSize = value;
			if (FadeCamera) FadeCameraDirect.orthographicSize = value;
			Projection = Projection;
		}
	}

	static float NearClipPlane => MainCamera? MainCameraDirect.nearClipPlane : default;
	static float  FarClipPlane => MainCamera? MainCameraDirect. farClipPlane : default;

	public static float Projection {
		get => Instance? Instance.m_Projection : default;
		set {
			float aspect =  (float)RenderTextureSize.x / RenderTextureSize.y;
			float left   = -OrthographicSize * aspect;
			float right  =  OrthographicSize * aspect;
			float bottom = -OrthographicSize;
			float top    =  OrthographicSize;

			Matrix4x4 a = Matrix4x4.Perspective(FieldOfView, aspect, NearClipPlane, FarClipPlane);
			Matrix4x4 b = Matrix4x4.Ortho (left, right, bottom, top, NearClipPlane, FarClipPlane);
			Matrix4x4 projection = MainCamera? MainCameraDirect.projectionMatrix : default;
			for (int i = 0; i < 16; i++) projection[i] = Mathf.Lerp(a[i], b[i], value);

			if (Instance) Instance.m_Projection = value;
			if (MainCamera) MainCameraDirect.projectionMatrix = projection;
			if (FadeCamera) FadeCameraDirect.projectionMatrix = projection;
		}
	}



	public static int AbsoluteLayer => Instance? Instance.m_AbsoluteLayer : default;
	public static int ExteriorLayer => Instance? Instance.m_ExteriorLayer : default;
	public static int InteriorLayer => Instance? Instance.m_InteriorLayer : default;

	public static float TransitionTime {
		get   =>  Instance? Instance.m_TransitionTime : default;
		set { if (Instance) Instance.m_TransitionTime = value; }
	}

	static RawImage MainRawImage => Instance? Instance.m_MainRawImage : default;
	static RawImage FadeRawImage => Instance? Instance.m_FadeRawImage : default;



	public static GameObject Target {
		get   =>  Instance? Instance.m_Target : default;
		set { if (Instance) Instance.m_Target = value; }
	}

	public static Vector3 TargetPosition {
		get   =>  Instance? Instance.m_TargetPosition : default;
		set { if (Instance) Instance.m_TargetPosition = value; }
	}

	public static float TargetDistance {
		get => Instance? Instance.m_TargetDistance : default;
		set {
			if (Instance) Instance.m_TargetDistance = value;
			if (MainCamera) MainCameraDirect.transform.localPosition = new Vector3(0, 0, -value);
			if (FadeCamera) FadeCameraDirect.transform.localPosition = new Vector3(0, 0, -value);
		}
	}

	public static bool[] FreezePosition {
		get   =>  Instance? Instance.m_FreezePosition : default;
		set { if (Instance) Instance.m_FreezePosition = value; }
	}

	public static bool[] FreezeRotation {
		get   =>  Instance? Instance.m_FreezeRotation : default;
		set { if (Instance) Instance.m_FreezeRotation = value; }
	}



	// Methods

	public static Ray ScreenPointToRay(Vector3 position) {
		if (MainCamera) {
			float multiplier = (float)RenderTextureSize.x / Screen.width;
			Vector3 viewport = MainCameraDirect.ScreenToViewportPoint(position * multiplier);
			return MainCameraDirect.ViewportPointToRay(viewport);
		}
		return default;
	}

	public static Vector3 GetPixelated(Vector3 position) {
		if (Instance) {
			float pixelPerUnit = UIManager.Instance.PixelPerUnit;
			Vector3 positionInversed = Instance.transform.InverseTransformDirection(position);
			positionInversed.x = Mathf.Round(positionInversed.x * pixelPerUnit) / pixelPerUnit;
			positionInversed.y = Mathf.Round(positionInversed.y * pixelPerUnit) / pixelPerUnit;
			positionInversed.z = Mathf.Round(positionInversed.z * pixelPerUnit) / pixelPerUnit;
			return Instance.transform.TransformDirection(positionInversed);
		}
		else return position;
	}

	

	static float shakeStrength;
	static float shakeDuration;
	static bool  shakeVertical;

	public static void ShakeCamera(float strength, float duration, bool vertical = true) {
		shakeStrength = strength;
		shakeDuration = duration;
		shakeVertical = vertical;
	}

	static void UpdateCameraShake() {
		if (0 < shakeDuration) {
			shakeDuration = Mathf.Max(0, shakeDuration - Time.fixedDeltaTime);
			//shakeVertical = Random.insideUnitSphere;
			//MainCamera.transform.position += shakeDirection * shakeStrength;
			//FadeCamera.transform.position += shakeDirection * shakeStrength;
		}
	}



	// Lifecycle

	Vector3    position;
	Quaternion rotation;



	void Start() => Projection = Projection;

	void OnEnable() {
		InitTrigger();
	}

	void LateUpdate() {
		UpdateCameraShake();
		/*
		if (Target) TargetPosition = Target.transform.position;
		if (!FreezePosition[0] || !FreezePosition[1] || !FreezePosition[2]) {
			float distance = Vector3.Distance(transform.position, TargetPosition);
			if (1 / UIManager.Instance.PixelPerUnit * Mathf.Sqrt(3) < distance || transform.rotation != rotation) {
				Vector3 a = transform.position;
				Vector3 b = TargetPosition;
				if (!FreezePosition[0]) a.x = b.x;
				if (!FreezePosition[1]) a.y = b.y;
				if (!FreezePosition[2]) a.z = b.z;
				transform.position = a;
			}
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
		if (transform.rotation != rotation) {
			rotation = transform.rotation;
		}
		else if (transform.position != position) {
			position = transform.position = GetPixelated(transform.position);
		}
		*/
		if (Target) TargetPosition = Target.transform.position;
		transform.position = TargetPosition;
	}



	public static int CullingMask { get; private set; }

	List<Collider> layers       = new List<Collider>();
	bool           layerChanged = false;
	int            layerMask    = 0;

	void InitTrigger() {
		layers.Clear();
		layerChanged = true;
		layerMask = Utility.GetLayerMaskAtPoint(transform.position, gameObject);
		if (layerMask == 0) layerMask |= ExteriorLayer;
		CullingMask = layerMask | AbsoluteLayer;
	}

	void OnTriggerEnter(Collider collider) {
		if (collider.isTrigger) {
			layers.Add(collider);
			layerChanged = true;
		}
	}

	void OnTriggerExit(Collider collider) {
		if (collider.isTrigger) {
			layers.Remove(collider);
			layerChanged = true;
		}
	}

	void EndTrigger() {
		if (layerChanged) {
			layerChanged = false;
			layerMask = 0;
			for (int i = 0; i < layers.Count; i++) layerMask |= 1 << layers[i].gameObject.layer;
			if (layerMask == 0) layerMask |= ExteriorLayer;
			CullingMask = layerMask | AbsoluteLayer;
		}
	}

	void FixedUpdate() {
		EndTrigger();
		if (FadeCamera.cullingMask == 0) {
			if (MainCamera.cullingMask != CullingMask) {
				FadeCamera.cullingMask  = CullingMask;
			}
		}
		else {
			float delta = Time.deltaTime / TransitionTime;
			float alpha = Mathf.Clamp01(FadeRawImage.color.a + delta);
			FadeRawImage.color = new Color(1, 1, 1, alpha);
			if (alpha == 1) {
				MainCamera.cullingMask = FadeCamera.cullingMask;
				FadeCamera.cullingMask = 0;
				FadeRawImage.color = new Color(1, 1, 1, 0);
			}
		}
	}
}
