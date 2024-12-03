using UnityEngine;
using Unity.Collections;

using System;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
	using UnityEditor;
#endif



[Serializable] struct CreatureData {
	public Vector3    position;
	public Quaternion rotation;
	public Vector3    scale;

	public Vector2 tiling;
	public Vector2 offset;
	public Color   color;
	public float   intensity;
}

[Serializable] struct ParticleData {
	public Vector3    position;
	public Quaternion rotation;
	public Vector3    scale;

	public Vector2 tiling;
	public Vector2 offset;
	public Color   color;
	public float   intensity;
}

[Serializable] struct ShadowOnlyData {
	public Vector3 position;
	public Vector4 rotation;
	public Vector3 scale;
}



public class DrawManager : MonoSingleton<DrawManager> {

	// ================================================================================================
	// Fields
	// ================================================================================================

	[SerializeField] Mesh       m_QuadMesh;
	[SerializeField] Material   m_CreatureMaterial;
	[SerializeField] Material   m_ParticleMaterial;
	[SerializeField] AtlasMapSO m_CreatureAtlasMap;
	[SerializeField] AtlasMapSO m_ParticleAtlasMap;

	[SerializeField] Mesh     m_SphereMesh;
	[SerializeField] Material m_ShadowOnlyMaterial;



	static Mesh QuadMesh {
		get   =>  Instance? Instance.m_QuadMesh : default;
		set { if (Instance) Instance.m_QuadMesh = value; }
	}

	static Material CreatureMaterial {
		get   =>  Instance? Instance.m_CreatureMaterial : default;
		set { if (Instance) Instance.m_CreatureMaterial = value; }
	}
	static Material ParticleMaterial {
		get   =>  Instance? Instance.m_ParticleMaterial : default;
		set { if (Instance) Instance.m_ParticleMaterial = value; }
	}

	static AtlasMapSO CreatureAtlasMap {
		get   =>  Instance? Instance.m_CreatureAtlasMap : default;
		set { if (Instance) Instance.m_CreatureAtlasMap = value; }
	}
	static AtlasMapSO ParticleAtlasMap {
		get   =>  Instance? Instance.m_ParticleAtlasMap : default;
		set { if (Instance) Instance.m_ParticleAtlasMap = value; }
	}



	static Mesh SphereMesh {
		get   =>  Instance? Instance.m_SphereMesh : default;
		set { if (Instance) Instance.m_SphereMesh = value; }
	}
	static Material ShadowOnlyMaterial {
		get   =>  Instance? Instance.m_ShadowOnlyMaterial : default;
		set { if (Instance) Instance.m_ShadowOnlyMaterial = value; }
	}



	#if UNITY_EDITOR
		[CustomEditor(typeof(DrawManager))] public class DrawManagerEditor : ExtendedEditor {
			public override void OnInspectorGUI() {
				Begin("Draw Manager");

				LabelField("Material", EditorStyles.boldLabel);
				QuadMesh         = ObjectField("Quad Mesh",          QuadMesh);
				CreatureMaterial = ObjectField("Creature Material",  CreatureMaterial);
				ParticleMaterial = ObjectField("Particle Material",  ParticleMaterial);
				CreatureAtlasMap = ObjectField("Creature Atlas Map", CreatureAtlasMap);
				ParticleAtlasMap = ObjectField("Particle Atlas Map", ParticleAtlasMap);
				Space();

				LabelField("Shadow", EditorStyles.boldLabel);
				SphereMesh         = ObjectField("Sphere Mesh",          SphereMesh);
				ShadowOnlyMaterial = ObjectField("Shadow Only Material", ShadowOnlyMaterial);
				Space();

				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================

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



	static HashMap<int, int>          creatureSizeMap = new HashMap<int, int>();
	static HashMap<int, int>          particleSizeMap = new HashMap<int, int>();
	static HashMap<int, CreatureData> creatureDataMap = new HashMap<int, CreatureData>();
	static HashMap<int, ParticleData> particleDataMap = new HashMap<int, ParticleData>();

	static int GetCreatureSize(
		CreatureType  creatureType  = (CreatureType )(-1),
		AnimationType animationType = (AnimationType)(-1),
		int           direction     = -1,
		int           index         = -1
	) => creatureSizeMap.TryGetValue(
		((((int)creatureType  + 1) & 0xFF) << 24) |
		((((int)animationType + 1) & 0xFF) << 16) |
		((((int)direction     + 1) & 0xFF) <<  8) |
		((((int)index         + 1) & 0xFF) <<  0),
		out int count) ? count : 0;
	
	static CreatureData GetCreatureData(
		CreatureType  creatureType,
		AnimationType animationType,
		int           direction,
		int           index
	) => creatureDataMap.TryGetValue(
		((((int)creatureType  + 1) & 0xFF) << 24) |
		((((int)animationType + 1) & 0xFF) << 16) |
		((((int)direction     + 1) & 0xFF) <<  8) |
		((((int)index         + 1) & 0xFF) <<  0),
		out CreatureData data) ? data : new CreatureData();

	static int GetParticleSize(
		ParticleType particleType = (ParticleType)(-1),
		int          index        = -1
	) => particleSizeMap.TryGetValue(
		((((int)particleType + 1) & 0xFF) << 24) |
		((((int)index        + 1) & 0xFF) << 16),
		out int count) ? count : 0;
	
	static ParticleData GetParticleData(
		ParticleType particleType,
		int          index
	) => particleDataMap.TryGetValue(
		((((int)particleType + 1) & 0xFF) << 24) |
		((((int)index        + 1) & 0xFF) << 16),
		out ParticleData data) ? data : new ParticleData();



	static void LoadCreatureMap() {
		if (!CreatureAtlasMap) return;
		creatureSizeMap.Clear();
		creatureDataMap.Clear();
		float pixelPerUnit = UIManager.PixelPerUnit;

		if (CreatureAtlasMap) foreach (var pair in CreatureAtlasMap.AtlasMap) {
			// CreatureType_AnimationType_Direction_Index_Duration
			string[] split = pair.Key.Split('_');
			if (split.Length != 5) continue;

			bool match = true;
			match &= Enum.TryParse(split[0], out CreatureType creatureType);
			match &= Enum.TryParse(split[1], out AnimationType animationType);
			match &=  int.TryParse(split[2], out int direction);
			match &=  int.TryParse(split[3], out int index);
			match &=  int.TryParse(split[4], out int duration);
			if (!match) continue;

			int[] key = new int[5];
			key[0] = 0;
			key[1] = key[0] + ((((int)creatureType  + 1) & 0xFF) << 24);
			key[2] = key[1] + ((((int)animationType + 1) & 0xFF) << 16);
			key[3] = key[2] + ((((int)direction     + 1) & 0xFF) <<  8);
			key[4] = key[3] + ((((int)index         + 1) & 0xFF) <<  0);
			
			for (int k = 4 - 1; -1 < k; k--) {
				if (!creatureSizeMap.ContainsKey(key[k])) creatureSizeMap.Add(key[k], 0);
				creatureSizeMap[key[k]]++;
				if (k == 0 || creatureSizeMap.ContainsKey(key[k - 1])) break;
			}
			if (!creatureSizeMap.ContainsKey(key[4])) creatureSizeMap[key[4]] = duration;
			if (1 < creatureSizeMap[key[3]]) creatureSizeMap[key[4]] += creatureSizeMap[key[4] - 1];

			creatureDataMap.Add(key[4], new CreatureData() {
				position  = new Vector3(0, 0, 0),
				rotation  = new Quaternion(0, 0, 0, 1),
				scale     = new Vector3(pair.Value.size.x, pair.Value.size.y, 1) / pixelPerUnit,

				tiling    = new Vector2(pair.Value.tiling.x, pair.Value.tiling.y),
				offset    = new Vector2(pair.Value.offset.x, pair.Value.offset.y),
				color     = Color.white,
				intensity = 0f,
			});
		}
	}

	static void LoadParticleMap() {
		if (!ParticleAtlasMap) return;
		particleSizeMap.Clear();
		particleDataMap.Clear();
		float pixelPerUnit = UIManager.PixelPerUnit;

		if (ParticleAtlasMap) foreach (var pair in ParticleAtlasMap.AtlasMap) {
			// ParticleType_Index_Duration
			string[] split = pair.Key.Split('_');
			if (split.Length != 3) continue;

			bool match = true;
			match &= Enum.TryParse(split[0], out ParticleType particleType);
			match &=  int.TryParse(split[1], out int index);
			match &=  int.TryParse(split[2], out int duration);
			if (!match) continue;

			int[] key = new int[3];
			key[0] = 0;
			key[1] = key[0] + ((((int)particleType + 1) & 0xFF) << 24);
			key[2] = key[1] + ((((int)index        + 1) & 0xFF) << 16);

			for (int k = 2 - 1; -1 < k; k--) {
				if (!particleSizeMap.ContainsKey(key[k])) particleSizeMap.Add(key[k], 0);
				particleSizeMap[key[k]]++;
				if (k == 0 || particleSizeMap.ContainsKey(key[k - 1])) break;
			}
			if (!particleSizeMap.ContainsKey(key[2])) particleSizeMap[key[2]] = duration;
			if (1 < particleSizeMap[key[1]]) particleSizeMap[key[2]] += particleSizeMap[key[2] - 1];

			particleDataMap.Add(key[2], new ParticleData() {
				position  = new Vector3(0, 0, 0),
				rotation  = new Quaternion(0, 0, 0, 1),
				scale     = new Vector3(pair.Value.size.x, pair.Value.size.y, 1) / pixelPerUnit,

				tiling    = new Vector2(pair.Value.tiling.x, pair.Value.tiling.y),
				offset    = new Vector2(pair.Value.offset.x, pair.Value.offset.y),
				color     = Color.white,
				intensity = 0f,
			});
		}
	}



	static GPUBatcher<CreatureData>   creatureBatcher;
	static GPUBatcher<ParticleData>   particleBatcher;
	static GPUBatcher<ShadowOnlyData> shadowOnlyBatcher;

	static void ConstructGPUBatcher() {
		creatureBatcher   = new GPUBatcher<CreatureData  >(CreatureMaterial,   QuadMesh,   0);
		particleBatcher   = new GPUBatcher<ParticleData  >(ParticleMaterial,   QuadMesh,   0);
		shadowOnlyBatcher = new GPUBatcher<ShadowOnlyData>(ShadowOnlyMaterial, SphereMesh, 0);
		creatureBatcher  .param.layer = LayerMask.NameToLayer("Entity");
		particleBatcher  .param.layer = LayerMask.NameToLayer("Entity");
		shadowOnlyBatcher.param.layer = LayerMask.NameToLayer("Entity");
		creatureBatcher  .param.receiveShadows = false;
		particleBatcher  .param.receiveShadows = false;
		shadowOnlyBatcher.param.receiveShadows = false;
		creatureBatcher  .param.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		particleBatcher  .param.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		shadowOnlyBatcher.param.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
	}

	static void DestructGPUBatcher() {
		creatureBatcher  ?.Dispose();
		particleBatcher  ?.Dispose();
		shadowOnlyBatcher?.Dispose();
	}

	static Func<int, int> func;

	static void DrawCreature() {
		float cameraYaw = GetYaw(CameraManager.Rotation);
		foreach (Creature creature in Creature.GetList()) {
			CreatureType  creatureType  = creature.CreatureType;
			AnimationType animationType = creature.AnimationType;

			float relativeYaw   = GetYaw(creature.transform.rotation) - cameraYaw;
			int   numDirections = GetCreatureSize(creatureType, animationType);
			GetDirection(relativeYaw, numDirections, out int direction, out bool xflip);

			int count = GetCreatureSize(creatureType, animationType, direction);
			int total = GetCreatureSize(creatureType, animationType, direction, count - 1);
			int value = (int)(creature.Offset * 1000) % total;
			func = i => GetCreatureSize(creatureType, animationType, direction, i);
			int index = GetIndex(count, value, func);

			CreatureData data = GetCreatureData(creatureType, animationType, direction, index);
			data.position = creature.transform.position;
			data.rotation = CameraManager.Rotation;
			if (xflip) {
				data.offset.x += data.tiling.x;
				data.tiling.x *= -1;
			}
			data.color.a  = creature.Opacity;

			creatureBatcher.Add(data);
			HitboxData hitbox = NavMeshManager.GetHitboxData(creature.HitboxType);
			shadowOnlyBatcher.Add(new ShadowOnlyData() {
				position = creature.transform.position,
				rotation = new Vector4(0, 0, 0, 1),
				scale    = new Vector3(hitbox.radius * 2, hitbox.height, hitbox.radius * 2),
			});
		}
		creatureBatcher  ?.Draw ();
		shadowOnlyBatcher?.Draw ();
		creatureBatcher  ?.Clear();
		shadowOnlyBatcher?.Clear();
	}

	static void DrawParticle() {
		float cameraYaw = GetYaw(CameraManager.Rotation);
		foreach (Particle particle in ParticleManager.GetList()) {
			ParticleType particleType = particle.ParticleType;

			int count = GetParticleSize(particleType);
			int total = GetParticleSize(particleType, count - 1);
			int value = (int)(particle.Offset * 1000) % total;
			func = i => GetParticleSize(particleType, i);
			int index = GetIndex(count, value, func);

			ParticleData data = GetParticleData(particleType, index);
			data.position = particle.Position;
			data.rotation = CameraManager.Rotation;
			data.color.a  = particle.Opacity;

			particleBatcher.Add(data);
		}
		particleBatcher?.Draw ();
		particleBatcher?.Clear();
	}



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	void OnEnable () => ConstructGPUBatcher();
	void OnDisable() =>  DestructGPUBatcher();

	void Start() {
		LoadCreatureMap();
		LoadParticleMap();
	}

	void LateUpdate() {
		DrawCreature();
		DrawParticle();
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
