using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class CameraManager : MonoSingleton<CameraManager> {

	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] Camera m_MainCamera;
	[SerializeField] Camera m_FadeCamera;

	[SerializeField] Vector2Int m_RenderTextureSize = Vector2Int.one;
	[SerializeField] float      m_FieldOfView       = 60.00f;
	[SerializeField] float      m_OrthographicSize  = 11.25f;
	[SerializeField] float      m_Projection        = 01.00f;

	[SerializeField] RawImage m_MainRawImage;
	[SerializeField] RawImage m_FadeRawImage;
	[SerializeField] int      m_AbsoluteLayer  = 0;
	[SerializeField] int      m_ExteriorLayer  = 0;
	[SerializeField] int      m_InteriorLayer  = 0;
	[SerializeField] float    m_TransitionTime = 0.2f;

	[SerializeField] GameObject m_Target;
	[SerializeField] Vector3    m_TargetPosition = Vector3.zero;
	[SerializeField] float      m_TargetDistance = 36.0f;
	[SerializeField] bool[]     m_FreezePosition = new bool[3];
	[SerializeField] bool[]     m_FreezeRotation = new bool[3];



	public static Quaternion Rotation {
		get   =>  Instance? Instance.transform.rotation : default;
		set { if (Instance) Instance.transform.rotation = value; }
	}
	public static Vector3 EulerRotation {
		get   =>  Instance? Instance.transform.eulerAngles : default;
		set { if (Instance) Instance.transform.eulerAngles = value; }
	}



	static Camera MainCamera {
		get   =>  Instance? Instance.m_MainCamera : default;
		set { if (Instance) Instance.m_MainCamera = value; }
	}
	static Camera FadeCamera {
		get   =>  Instance? Instance.m_FadeCamera : default;
		set { if (Instance) Instance.m_FadeCamera = value; }
	}
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



	static RawImage MainRawImage {
		get   =>  Instance? Instance.m_MainRawImage : default;
		set { if (Instance) Instance.m_MainRawImage = value; }
	}
	static RawImage FadeRawImage {
		get   =>  Instance? Instance.m_FadeRawImage : default;
		set { if (Instance) Instance.m_FadeRawImage = value; }
	}

	public static int AbsoluteLayer {
		get   =>  Instance? Instance.m_AbsoluteLayer : default;
		set { if (Instance) Instance.m_AbsoluteLayer = value; }
	}
	public static int ExteriorLayer {
		get   =>  Instance? Instance.m_ExteriorLayer : default;
		set { if (Instance) Instance.m_ExteriorLayer = value; }
	}
	public static int InteriorLayer {
		get   =>  Instance? Instance.m_InteriorLayer : default;
		set { if (Instance) Instance.m_InteriorLayer = value; }
	}

	public static float TransitionTime {
		get   =>  Instance? Instance.m_TransitionTime : default;
		set { if (Instance) Instance.m_TransitionTime = value; }
	}



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



	#if UNITY_EDITOR
		[CustomEditor(typeof(CameraManager))] class CameraManagerEditor : ExtendedEditor {
			public override void OnInspectorGUI() {
				Begin("Camera Manager");

				LabelField("Camera", EditorStyles.boldLabel);
				MainCamera = ObjectField("Main Camera", MainCamera);
				FadeCamera = ObjectField("Fade Camera", FadeCamera);
				Space();

				LabelField("Camera Properties", EditorStyles.boldLabel);
				RenderTextureSize = Vector2IntField("Render Texture Size", RenderTextureSize);
				FieldOfView       = FloatField     ("Field Of View",       FieldOfView);
				OrthographicSize  = FloatField     ("Orthographic Size",   OrthographicSize);
				Projection        = Slider         ("Projection",          Projection, 0, 1);
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

				LabelField("Layer Transition", EditorStyles.boldLabel);
				MainRawImage   = ObjectField("Main Raw Image",  MainRawImage);
				FadeRawImage   = ObjectField("Fade Raw Image",  FadeRawImage);
				AbsoluteLayer  = MaskField  ("Absolute Layer",  AbsoluteLayer);
				ExteriorLayer  = MaskField  ("Exterior Layer",  ExteriorLayer);
				InteriorLayer  = MaskField  ("Interior Layer",  InteriorLayer);
				TransitionTime = Slider     ("Transition Time", TransitionTime, 0, 3);
				Space();

				LabelField("Target Tracking", EditorStyles.boldLabel);
				Target         = ObjectField ("Target", Target);
				TargetPosition = Vector3Field("Target Position", TargetPosition);
				TargetDistance = Slider      ("Target Distance", TargetDistance, 0, 256);
				BeginHorizontal();
				PrefixLabel("Freeze Position");
				FreezePosition[0] = ToggleLeft("X", FreezePosition[0], GUILayout.Width(28));
				FreezePosition[1] = ToggleLeft("Y", FreezePosition[1], GUILayout.Width(28));
				FreezePosition[2] = ToggleLeft("Z", FreezePosition[2], GUILayout.Width(28));
				EndHorizontal();
				BeginHorizontal();
				PrefixLabel("Freeze Rotation");
				FreezeRotation[0] = ToggleLeft("X", FreezeRotation[0], GUILayout.Width(28));
				FreezeRotation[1] = ToggleLeft("Y", FreezeRotation[1], GUILayout.Width(28));
				FreezeRotation[2] = ToggleLeft("Z", FreezeRotation[2], GUILayout.Width(28));
				EndHorizontal();
				Space();
				
				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	public static Ray ScreenPointToRay(Vector3 position) {
		if (MainCamera) {
			float multiplier = (float)RenderTextureSize.x / Screen.width;
			Vector3 viewport = MainCameraDirect.ScreenToViewportPoint(position * multiplier);
			return MainCameraDirect.ViewportPointToRay(viewport);
		}
		return default;
	}

	static RaycastHit[] hits = new RaycastHit[16];

	public static bool TryRaycast(Vector3 position, out Vector3 hit) {
		if (MainCamera) {
			Ray ray = ScreenPointToRay(position);
			QueryTriggerInteraction query = QueryTriggerInteraction.Ignore;
			int num = Physics.RaycastNonAlloc(ray, hits, 1000, -5, query);
			for (int i = 0; i < num; i++) {
				Transform transform = hits[i].collider.transform;
				if (Utility.TryGetComponentInParent(transform, out MeshRenderer renderer)) {
					if (renderer.material.name.Equals("TerrainMaterial (Instance)")) {
						hit = hits[i].point;
						return true;
					}
				}
			}
		}
		hit = default;
		return false;
	}

	public static Vector3 GetPixelated(Vector3 position) {
		if (Instance) {
			float pixelPerUnit = UIManager.PixelPerUnit;
			Vector3 positionInversed = Instance.transform.InverseTransformDirection(position);
			positionInversed.x = Mathf.Round(positionInversed.x * pixelPerUnit) / pixelPerUnit;
			positionInversed.y = Mathf.Round(positionInversed.y * pixelPerUnit) / pixelPerUnit;
			positionInversed.z = Mathf.Round(positionInversed.z * pixelPerUnit) / pixelPerUnit;
			return Instance.transform.TransformDirection(positionInversed);
		}
		else return position;
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	public static int CullingMask { get; private set; }

	List<Collider> layers       = new List<Collider>();
	bool           layerChanged = false;
	int            layerMask    = 0;

	void InitTrigger() {
		layers.Clear();
		layerChanged = true;
		layerMask = Utility.GetLayerMaskAtPoint(transform.position, transform);
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
			layerMask &= ~AbsoluteLayer;
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



	void Start() => Projection = Projection;

	void OnEnable() {
		InitTrigger();
	}

	Vector3    position;
	Quaternion rotation;

	void LateUpdate() {
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
}
