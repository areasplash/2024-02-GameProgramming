using UnityEngine;
using Unity.Collections;

using System;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
	using UnityEditor;
#endif



[Serializable] struct EntityDrawData {
	public Vector3 position;
	public Vector4 rotation;
	public Vector3 scale;

	public Vector2 tiling;
	public Vector2 offset;
	public Color   color;
	public float   intensity;
}

[Serializable] struct ShadowDrawData {
	public Vector3 position;
	public Vector4 rotation;
	public Vector3 scale;
}



public class DrawManager : MonoSingleton<DrawManager> {

	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] Mesh       m_EntityMesh;
	[SerializeField] Material   m_EntityMaterial;
	[SerializeField] AtlasMapSO m_EntityAtlasMap;

	[SerializeField] Mesh       m_ShadowMesh;
	[SerializeField] Material   m_ShadowMaterial;



	static Mesh EntityMesh {
		get   =>  Instance? Instance.m_EntityMesh : default;
		set { if (Instance) Instance.m_EntityMesh = value; }
	}
	static Material EntityMaterial {
		get   =>  Instance? Instance.m_EntityMaterial : default;
		set { if (Instance) Instance.m_EntityMaterial = value; }
	}
	static AtlasMapSO EntityAtlasMap {
		get   =>  Instance? Instance.m_EntityAtlasMap : default;
		set { if (Instance) Instance.m_EntityAtlasMap = value; }
	}

	static Mesh ShadowMesh {
		get   =>  Instance? Instance.m_ShadowMesh : default;
		set { if (Instance) Instance.m_ShadowMesh = value; }
	}
	static Material ShadowMaterial {
		get   =>  Instance? Instance.m_ShadowMaterial : default;
		set { if (Instance) Instance.m_ShadowMaterial = value; }
	}



	#if UNITY_EDITOR
		[CustomEditor(typeof(DrawManager))] public class DrawManagerEditor : ExtendedEditor {
			public override void OnInspectorGUI() {
				Begin("Draw Manager");

				LabelField("Entity", EditorStyles.boldLabel);
				EntityMesh     = ObjectField("Entity Mesh",      EntityMesh);
				EntityMaterial = ObjectField("Entity Material",  EntityMaterial);
				EntityAtlasMap = ObjectField("Entity Atlas Map", EntityAtlasMap);
				Space();

				LabelField("Shadow", EditorStyles.boldLabel);
				ShadowMesh     = ObjectField("Shadow Mesh",     ShadowMesh);
				ShadowMaterial = ObjectField("Shadow Material", ShadowMaterial);
				Space();

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

	static HashMap<int, int        > entitySizeMap = new HashMap<int, int        >();
	static HashMap<int, TextureData> entityDataMap = new HashMap<int, TextureData>();

	static void ReadEntityAtlasMap() {
		if (!EntityAtlasMap) return;
		entitySizeMap.Clear();
		entityDataMap.Clear();
		float pixelPerUnit = UIManager.PixelPerUnit;

		foreach (var pair in EntityAtlasMap.AtlasMap) {
			// EntityType_MotionType_Direction_Index_Duration
			string[] split = pair.Key.Split('_');
			if (split.Length != 5) continue;

			bool match = true;
			match &= Enum.TryParse(split[0], out EntityType entityType);
			match &= Enum.TryParse(split[1], out MotionType motionType);
			match &=  int.TryParse(split[2], out int direction);
			match &=  int.TryParse(split[3], out int index);
			match &=  int.TryParse(split[4], out int duration);
			if (!match) continue;

			int[] key = new int[5];
			key[0] = 0;
			key[1] = key[0] + ((((int)entityType + 1) & 0xFF) << 24);
			key[2] = key[1] + ((((int)motionType + 1) & 0xFF) << 16);
			key[3] = key[2] + ((((int)direction  + 1) & 0xFF) <<  8);
			key[4] = key[3] + ((((int)index      + 1) & 0xFF) <<  0);
			
			for (int k = 4 - 1; -1 < k; k--) {
				if (!entitySizeMap.ContainsKey(key[k])) entitySizeMap.Add(key[k], 0);
				entitySizeMap[key[k]]++;
				if (k == 0 || entitySizeMap.ContainsKey(key[k - 1])) break;
			}
			if (!entitySizeMap.ContainsKey(key[4])) entitySizeMap[key[4]] = duration;
			if (1 < entitySizeMap[key[3]]) entitySizeMap[key[4]] += entitySizeMap[key[4] - 1];
			entityDataMap.Add(key[4], pair.Value);
		}
	}

	static int GetEntitySize(
		EntityType entityType = (EntityType)(-1),
		MotionType motionType = (MotionType)(-1),
		int        direction  = -1,
		int         index     = -1
	) => entitySizeMap.TryGetValue(
		((((int)entityType + 1) & 0xFF) << 24) |
		((((int)motionType + 1) & 0xFF) << 16) |
		((((int)direction  + 1) & 0xFF) <<  8) |
		((((int)index      + 1) & 0xFF) <<  0),
		out int count) ? count : 0;
	
	static TextureData GetEntityData(
		EntityType entityType,
		MotionType motionType,
		int        direction,
		int        index
	) => entityDataMap.TryGetValue(
		((((int)entityType + 1) & 0xFF) << 24) |
		((((int)motionType + 1) & 0xFF) << 16) |
		((((int)direction  + 1) & 0xFF) <<  8) |
		((((int)index      + 1) & 0xFF) <<  0),
		out TextureData data) ? data : new TextureData();



	static float GetYaw(Quaternion quaternion) {
		float y = 0.0f + 2.0f * (quaternion.y * quaternion.w + quaternion.x * quaternion.z);
		float x = 1.0f - 2.0f * (quaternion.y * quaternion.y + quaternion.z * quaternion.z);
		return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
	}

	static void GetDirection(float relativeYaw, int numDirections, out int direction, out bool xFlip) {
		xFlip = false;
		int i = 0;
		int yaw = (int)Mathf.Repeat(relativeYaw / 360f * 256f, 256 - 1);
		switch (numDirections) {
			case  1: i = (yaw +  0) / 128; if (0 < i) { i =  1 - i; xFlip = true; } break;
			case  2: i = (yaw +  0) / 128;                                          break;
			case  3: i = (yaw + 32) /  64; if (2 < i) { i =  4 - i; xFlip = true; } break;
			case  4: i = (yaw + 32) /  64;                                          break;
			case  5:
			case  6:
			case  7: i = (yaw + 16) /  32; if (4 < i) { i =  8 - i; xFlip = true; } break;
			case  8: i = (yaw + 16) /  32;                                          break;
			case  9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15: i = (yaw +  8) /  16; if (8 < i) { i = 16 - i; xFlip = true; } break;
			case 16: i = (yaw +  8) /  16;                                          break;
		}
		direction = i;
	}

	static int GetIndex(int count, float value, Func<int, int> func) {
		int m = 0;
		int l = 0;
		int r = count - 1;
		while (l <= r) {
			m = (l + r) / 2;
			if      (value < func(m - 1)) r = m - 1;
			else if (func(m + 0) < value) l = m + 1;
			else break;
		}
		return m;
	}



	public static void DrawEntity(Vector3 position,
		EntityType entityType) {
		DrawEntity(position, entityType, Time.time, Color.white, 0f);
	}
	public static void DrawEntity(Vector3 position,
		EntityType entityType, float offset) {
		DrawEntity(position, entityType, offset, Color.white, 0f);
	}
	public static void DrawEntity(Vector3 position,
		EntityType entityType, float offset, Color color, float intensity = 0f) {
		Quaternion camera = CameraManager.Rotation;

		int count = GetEntitySize(entityType, default, default);
		int total = GetEntitySize(entityType, default, default, count - 1);
		int value = (int)(offset * 1000) % (total == 0 ? 1 : total);
		int index = GetIndex(count, value, i => GetEntitySize(entityType, default, default, i));

		TextureData data = GetEntityData(entityType, default, default, index);
		float pixel = 1f / UIManager.PixelPerUnit;
		entityBatcher.Add(new EntityDrawData() {
			position = position,
			rotation = new Vector4(camera.x, camera.y, camera.z, camera.w),
			scale    = new Vector3(data.size.x * pixel, data.size.y * pixel, 1),

			tiling    = data.tiling,
			offset    = data.offset,
			color     = color,
			intensity = intensity,
		});
	}

	public static void DrawEntity(Vector3 position, Quaternion rotation,
		EntityType entityType, MotionType motionType) {
		DrawEntity(position, rotation, entityType, motionType, Time.time, Color.white, 0f);
	}
	public static void DrawEntity(Vector3 position, Quaternion rotation,
		EntityType entityType, MotionType motionType, float offset) {
		DrawEntity(position, rotation, entityType, motionType, offset, Color.white, 0f);
	}
	public static void DrawEntity(Vector3 position, Quaternion rotation,
		EntityType entityType, MotionType motionType, float offset,
		Color color, float intensity = 0f) {
		Quaternion camera = CameraManager.Rotation;

		float relativeYaw   = GetYaw(rotation) - GetYaw(camera);
		int   numDirections = GetEntitySize(entityType, motionType);
		GetDirection(relativeYaw, numDirections, out int direction, out bool xflip);

		int count = GetEntitySize(entityType, motionType, direction);
		int total = GetEntitySize(entityType, motionType, direction, count - 1);
		int value = (int)(offset * 1000) % (total == 0 ? 1 : total);
		int index = GetIndex(count, value, i => GetEntitySize(entityType, motionType, direction, i));

		TextureData data = GetEntityData(entityType, motionType, direction, index);
		float pixel = 1f / UIManager.PixelPerUnit;
		entityBatcher.Add(new EntityDrawData() {
			position = position,
			rotation = new Vector4(camera.x, camera.y, camera.z, camera.w),
			scale    = new Vector3(data.size.x * pixel, data.size.y * pixel, 1),

			tiling    = data.tiling * (xflip ? new Vector2(-1, 1) : Vector2.one),
			offset    = data.offset + (xflip ? new Vector2(data.tiling.x, 0) : Vector2.zero),
			color     = color,
			intensity = intensity,
		});
	}

	public static void DrawShadow(Vector3 position) {
		DrawShadow(position, Quaternion.identity, Vector3.one);
	}
	public static void DrawShadow(Vector3 position, Quaternion rotation) {
		DrawShadow(position, rotation, Vector3.one);
	}
	public static void DrawShadow(Vector3 position, Quaternion rotation, Vector3 scale) {
		shadowBatcher.Add(new ShadowDrawData() {
			position = position,
			rotation = new Vector4(rotation.x, rotation.y, rotation.z, rotation.w),
			scale    = scale,
		});
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	void Start() => ReadEntityAtlasMap();

	static GPUBatcher<EntityDrawData> entityBatcher;
	static GPUBatcher<ShadowDrawData> shadowBatcher;

	void OnEnable () {
		entityBatcher = new GPUBatcher<EntityDrawData>(EntityMaterial, EntityMesh, 0);
		shadowBatcher = new GPUBatcher<ShadowDrawData>(ShadowMaterial, ShadowMesh, 0);
		entityBatcher.param.layer = LayerMask.NameToLayer("Entity");
		shadowBatcher.param.layer = LayerMask.NameToLayer("Entity");
		entityBatcher.param.receiveShadows = false;
		shadowBatcher.param.receiveShadows = false;
		entityBatcher.param.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		shadowBatcher.param.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
	}

	void OnDisable() {
		entityBatcher.Dispose();
		shadowBatcher.Dispose();
	}

	void LateUpdate() {
		entityBatcher.Draw ();
		entityBatcher.Clear();
		shadowBatcher.Draw ();
		shadowBatcher.Clear();
	}
}



public struct NativeList<T> : IDisposable where T : unmanaged {

	// ================================================================================================
	// Fields
	// ================================================================================================

	NativeArray<T> narray;



	public T this[int index] {
		get => narray[index];
		set {
			narray[index] = value;
			Length = Mathf.Max(Length, index + 1);
		}
	}

	public int Capacity {
		get => narray.Length;
		set {
			value  = Mathf.Max(value, 4);
			Length = Mathf.Min(value, Length);
			NativeArray<T> narrayTemp = new NativeArray<T>(value, Allocator.Persistent);
			if (0 < Length) NativeArray<T>.Copy(narray, narrayTemp, Length);
			narray.Dispose();
			narray = narrayTemp;
		}
	}

	public int Length { get; private set; }



	// ================================================================================================
	// Methods
	// ================================================================================================

	public NativeList(int capacity = 64) {
		narray = new NativeArray<T>(Mathf.Max(capacity, 4), Allocator.Persistent);
		Length = 0;
	}

	public void Dispose() => narray.Dispose();



	public NativeArray<T> GetArray() => narray;

	public void Add(T value) => Insert(Length, value);

	public void Insert(int index, T value) {
		if (Capacity < index + 1) Capacity = Mathf.Max(Capacity + 1, Capacity * 2);
		if (0 < Length - index) NativeArray<T>.Copy(narray, index, narray, index + 1, Length - index);
		narray[index] = value;
		Length += 1;
	}

	public void AddRange(NativeList<T> list) => InsertRange(Length, list);

	public void InsertRange(int index, NativeList<T> list) {
		int i = list.Length;
		if (Capacity < Length + i) Capacity = Mathf.Max(Capacity + i, Capacity * 2);
		if (0 < Length - index) NativeArray<T>.Copy(narray, index, narray, index + i, Length - index);
		NativeArray<T>.Copy(list.GetArray(), 0, narray, index, i);
		Length += i;
	}

	public void RemoveAt(int index) => RemoveRange(index, 1);

	public void RemoveRange(int index, int count) {
		int i = Mathf.Min(count, Length - index);
		NativeArray<T>.Copy(narray, index + i, narray, index, Length - index - i);
		Length -= i;
	}

	public void Clear() => Length = 0;
}



public class GPUBatcher<T> : IDisposable where T : unmanaged {

	const GraphicsBuffer.Target Args       = GraphicsBuffer.Target.IndirectArguments;
	const GraphicsBuffer.Target Structured = GraphicsBuffer.Target.Structured;



	// ================================================================================================
	// Fields
	// ================================================================================================

	Mesh renderMesh;
	int  stride;
	int  propID;

	NativeList<int> narrayArgs;
	GraphicsBuffer  bufferArgs;
	NativeList<T>   narrayStructured;
	GraphicsBuffer  bufferStructured;

	public RenderParams param;

	int i;
	int j;



	public int Capacity {
		get => narrayStructured.Capacity;
		set => narrayStructured.Capacity = value;
	}

	public int Length => narrayStructured.Length;



	// ================================================================================================
	// Methods
	// ================================================================================================

	public GPUBatcher(Material material, Mesh mesh, int submesh) {
		renderMesh = mesh;
		stride     = Marshal.SizeOf<T>();
		propID     = Shader.PropertyToID($"_{typeof(T).Name}");

		narrayArgs = new NativeList<int>(5) {
			[0] = (int)mesh.GetIndexCount(submesh),
			[1] = 0,
			[2] = (int)mesh.GetIndexStart(submesh),
			[3] = (int)mesh.GetBaseVertex(submesh),
			[4] = 0
		};
		bufferArgs = new GraphicsBuffer(Args, narrayArgs.Capacity, sizeof(int));
		bufferArgs.SetData(narrayArgs.GetArray(), 0, 0, narrayArgs.Length);

		narrayStructured = new NativeList<T>(64);
		bufferStructured = new GraphicsBuffer(Structured, narrayStructured.Capacity, stride);
		bufferStructured.SetData(narrayStructured.GetArray(), 0, 0, narrayStructured.Length);

		param = new RenderParams(material) {
			worldBounds       = new Bounds(Vector3.zero, Vector3.one * 1024),
			matProps          = new MaterialPropertyBlock(),
			shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
			receiveShadows    = true
		};
		param.matProps.SetBuffer(propID, bufferStructured);
	}

	public void Dispose() {
		narrayArgs.Dispose();
		bufferArgs.Release();
		narrayStructured.Dispose();
		bufferStructured.Release();
	}



	public void Add(T value) => Insert(Length, value);
	
	public void Insert(int index, T value) {
		narrayStructured.Insert(index, value);
		i = Mathf.Min(i, index );
		j = Mathf.Max(j, Length);
	}

	public void AddRange(NativeList<T> value) => InsertRange(Length, value);

	public void InsertRange(int index, NativeList<T> value) {
		narrayStructured.InsertRange(index, value);
		i = Mathf.Min(i, index );
		j = Mathf.Max(j, Length);
	}

	public void RemoveAt(int index) => RemoveRange(index, 1);

	public void RemoveRange(int index, int count) {
		narrayStructured.RemoveRange(index, count);
		i = Mathf.Min(i, index );
		j = Mathf.Max(j, Length);
	}

	public void Clear() {
		narrayStructured.Clear();
		i = 0;
		j = 0;
	}

	public void Draw() {
		if (Length == 0) return;
		if (narrayArgs[1] != Length) {
			narrayArgs[1]  = Length;
			bufferArgs.SetData(narrayArgs.GetArray(), 0, 0, narrayArgs.Length);
		}
		if (bufferStructured.count != Capacity) {
			bufferStructured.Release();
			bufferStructured = new GraphicsBuffer(Structured, Capacity, stride);
			param.matProps.SetBuffer(propID, bufferStructured);
			i = 0;
			j = Length;
		}
		if (i < j) {
			bufferStructured.SetData(narrayStructured.GetArray(), i, i, j - i);
			i = Length;
			j = 0;
		}
		Graphics.RenderMeshIndirect(in param, renderMesh, bufferArgs);
	}
}
