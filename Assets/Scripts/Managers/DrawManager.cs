using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;



// ====================================================================================================
// Native List
// ====================================================================================================

public class NativeList<T> : IDisposable where T : struct {

	// Fields

	NativeArray<T> narray;
	int            length;



	// Properties

	public T this[int index] {
		get => narray[index];
		set {
			narray[index] = value;
			length = Mathf.Max(length, index + 1);
		}
	}

	public int Length => length;

	public int Capacity {
		get => narray.Length;
		set {
			value  = Mathf.Max(value, 4);
			length = Mathf.Min(value, length);
			NativeArray<T> narrayTemp = new NativeArray<T>(value, Allocator.Persistent);
			if (0 < length) NativeArray<T>.Copy(narray, narrayTemp, length);
			narray.Dispose();
			narray = narrayTemp;
		}
	}



	// Constructor, Destructor

	public NativeList(int capacity = 64) => Capacity = capacity;

	public void Dispose() => narray.Dispose();



	// Method

	public NativeArray<T> GetArray() => narray;

	public void Add(T value) => Insert(length, value);

	public void Insert(int index, T value) {
		if (Capacity < index + 1) Capacity = Mathf.Max(Capacity + 1, Capacity * 2);
		NativeArray<T>.Copy(narray, index, narray, index + 1, length - index);
		narray[index] = value;
		length += 1;
	}

	public void AddRange(NativeList<T> list) => InsertRange(length, list);

	public void InsertRange(int index, NativeList<T> list) {
		int i = list.Length;
		if (Capacity < length + i) Capacity = Mathf.Max(Capacity + i, Capacity * 2);
		NativeArray<T>.Copy(narray, index, narray, index + i, length - index);
		NativeArray<T>.Copy(list.GetArray(), 0, narray, index, i);
		length += i;
	}

	public void RemoveAt(int index) => RemoveRange(index, 1);

	public void RemoveRange(int index, int count) {
		int i = Mathf.Min(count, length - index);
		NativeArray<T>.Copy(narray, index + i, narray, index, length - index - i);
		length -= i;
	}

	public void Clear() => length = 0;
}



// ====================================================================================================
// GPU Batcher
// ====================================================================================================

public class GPUBatcher<T> : IDisposable where T : unmanaged {

	// Constants

	const GraphicsBuffer.Target Args       = GraphicsBuffer.Target.IndirectArguments;
	const GraphicsBuffer.Target Structured = GraphicsBuffer.Target.Structured;



	// Fields

	Mesh meshCached;

	int stride;
	int propID;

	NativeList<int> narrayArgs;
	GraphicsBuffer  bufferArgs;
	NativeList<T>   narrayStructured;
	GraphicsBuffer  bufferStructured;

	public RenderParams param;

	int i;
	int j;



	// Property

	public int Length => narrayStructured.Length;

	public int Capacity {
		get => narrayStructured.Capacity;
		set => narrayStructured.Capacity = value;
	}



	// Constructor, Destructor

	public GPUBatcher(Material material, Mesh mesh, int submesh) {
		meshCached = mesh;

		stride = Marshal.SizeOf<T>();
		propID = Shader.PropertyToID($"_{typeof(T).Name}");

		narrayArgs = new NativeList<int>(5) {
			[0] = (int)mesh.GetIndexCount(submesh),
			[1] = 0,
			[2] = (int)mesh.GetIndexStart(submesh),
			[3] = (int)mesh.GetBaseVertex(submesh),
			[4] = 0
		};
		bufferArgs = new GraphicsBuffer(Args, narrayArgs.Capacity, sizeof(int));
		bufferArgs.SetData(narrayArgs.GetArray(), 0, 0, narrayArgs.Length);

		narrayStructured = new NativeList<T>();
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
		bufferStructured.Release();
		narrayStructured.Dispose();
	}



	// Method

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
		Graphics.RenderMeshIndirect(in param, meshCached, bufferArgs);
	}
}



// ====================================================================================================
// Draw Manager
// ====================================================================================================

public class DrawManager : MonoSingleton<DrawManager> {

	[Header("Material")]
	[SerializeField] Material creatureMaterial;
	[SerializeField] Material particleMaterial;
	[SerializeField] Material terrainMaterial;

	[Header("Mesh")]
	[SerializeField] Mesh sphereMesh;
	[SerializeField] Mesh quadMesh;

	[Header("Atlas Map")]
	[SerializeField] AtlasMapSO creatureAtlasMap;
	[SerializeField] AtlasMapSO particleAtlasMap;
	[SerializeField] AtlasMapSO terrainAtlasMap;



	GPUBatcher<CreatureData> creatureBatcher;
	GPUBatcher<ParticleData> particleBatcher;
	GPUBatcher<TerrainData > terrainBatcher;



	struct CreatureData {
		public Vector3 position;
		public Vector4 rotation;
		public Vector3 scale;

		public Vector2 tiling;
		public Vector2 offset;
		public Vector3 color;
		public float   emission;
		public float   alpha;
	}
	HashMap<int, int         > creatureSizeMap = new HashMap<int, int         >();
	HashMap<int, CreatureData> creatureDataMap = new HashMap<int, CreatureData>();

	struct ParticleData {
		public Vector3 position;
		public Vector3 scale;

		public Vector2 tiling;
		public Vector2 offset;
		public Vector3 color;
		public float   emission;
		public float   alpha;
	}
	HashMap<int, int         > particleSizeMap = new HashMap<int, int         >();
	HashMap<int, ParticleData> particleDataMap = new HashMap<int, ParticleData>();

	struct TerrainData {
		public Matrix4x4 matrix;
		public Vector4   color;
	}
	HashMap<int, int         > terrainSizeMap  = new HashMap<int, int         >();
	HashMap<int, TerrainData > terrainDataMap  = new HashMap<int, TerrainData >();



	void OnEnable() {
		creatureBatcher = new GPUBatcher<CreatureData>(creatureMaterial, quadMesh, 0);
		particleBatcher = new GPUBatcher<ParticleData>(particleMaterial, quadMesh, 0);
		terrainBatcher  = new GPUBatcher<TerrainData >(terrainMaterial,  quadMesh, 0);
	}

	void OnDisable() {
		creatureBatcher.Dispose();
		particleBatcher.Dispose();
		terrainBatcher .Dispose();
	}

	void Start() {
		if (creatureAtlasMap) foreach (var pair in creatureAtlasMap.AtlasMap) {
			string[] split = pair.Key.Split('_');
			if (split.Length != 5) continue;

			int[] value = new int[5];
			value[0] = (int)Enum.Parse(typeof(CreatureType ), split[0]);
			value[1] = (int)Enum.Parse(typeof(AnimationType), split[1]);
			value[2] = int.Parse(split[2]);
			value[3] = int.Parse(split[3]);
			value[4] = int.Parse(split[4]);

			int[] key = new int[5];
			key[0] = 0;
			key[1] = key[0] + (((value[0] + 1) & 0xff) << 24);
			key[2] = key[1] + (((value[1] + 1) & 0xff) << 16);
			key[3] = key[2] + (((value[2] + 1) & 0xff) <<  8);
			key[4] = key[3] + (((value[3] + 1) & 0xff) <<  0);
			
			for (int k = 4 - 1; -1 < k; k--) {
				if (!creatureSizeMap.ContainsKey(key[k])) creatureSizeMap.Add(key[k], 0);
				creatureSizeMap[key[k]] += 1;
				if (k == 0 || creatureSizeMap.ContainsKey(key[k - 1])) break;
			}
			if (!creatureSizeMap.ContainsKey(key[4])) creatureSizeMap[key[4]] = value[4];
			if (1 < creatureSizeMap[key[3]]) creatureSizeMap[key[4]] += creatureSizeMap[key[4] - 1 << 0];

			creatureDataMap.Add(key[4], new CreatureData() {
				position = new Vector3(0, 0, 0),
				rotation = new Vector4(0, 0, 0, 1),
				scale    = new Vector3(pair.Value.size.x, pair.Value.size.y, 1) / 16,

				tiling   = new Vector2(pair.Value.tiling.x, pair.Value.tiling.y),
				offset   = new Vector2(pair.Value.offset.x, pair.Value.offset.y),
				color    = new Vector3(1, 1, 1),
				emission = 0,
				alpha    = 1,
			});
		}

		if (particleAtlasMap) foreach (var pair in particleAtlasMap.AtlasMap) {
			string[] split = pair.Key.Split('_');
			if (split.Length != 2) continue;

			int[] value = new int[2];
			value[0] = (int)Enum.Parse(typeof(ParticleType), split[0]);
			value[1] = int.Parse(split[1]);

			int[] key = new int[2];
			key[0] = 0;
			key[1] = key[0] + (((value[0] + 1) & 0xff) << 8);
			key[2] = key[1] + (((value[1] + 1) & 0xff) << 0);

			for (int k = 2 - 1; -1 < k; k--) {
				if (!particleSizeMap.ContainsKey(key[k])) particleSizeMap.Add(key[k], 0);
				particleSizeMap[key[k]] += 1;
				if (k == 0 || particleSizeMap.ContainsKey(key[k - 1])) break;
			}
			if (!particleSizeMap.ContainsKey(key[2])) particleSizeMap[key[2]] = value[2];
			if (1 < particleSizeMap[key[1]]) particleSizeMap[key[2]] += particleSizeMap[key[2] - 1 << 0];

			particleDataMap.Add(key[2], new ParticleData() {
				position = new Vector3(0, 0, 0),
				scale    = new Vector3(pair.Value.size.x, pair.Value.size.y, 1) / 16,

				tiling   = new Vector2(pair.Value.tiling.x, pair.Value.tiling.y),
				offset   = new Vector2(pair.Value.offset.x, pair.Value.offset.y),
				color    = new Vector3(1, 1, 1),
				emission = 0,
				alpha    = 1,
			});
		}

		if (terrainAtlasMap) foreach (var pair in terrainAtlasMap.AtlasMap) {
			
		}
	}

	public void AddCreature(
		Vector3 position,
		Vector4 rotation,
		Vector3 scale,

		CreatureType creatureType,
		AnimationType animationType,
		int direction,
		int duration) {

		creatureBatcher.Add(new CreatureData() {
			position = position,
			rotation = rotation,
			scale    = scale,

			tiling   = new Vector2(1, 1),
			offset   = new Vector2(0, 0),
			color    = new Vector3(1, 1, 1),
			emission = 0,
			alpha    = 1,
		});
	}
	
	void LateUpdate() {
		creatureBatcher.Draw();
		particleBatcher.Draw();
		terrainBatcher .Draw();

		creatureBatcher.Clear();
		particleBatcher.Clear();
		terrainBatcher .Clear();
	}
}
