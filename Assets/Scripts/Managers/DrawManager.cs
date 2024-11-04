using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Native List
// ====================================================================================================

public class NativeList<T> : System.IDisposable where T : struct {

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

public class GPUBatcher<T> : System.IDisposable where T : unmanaged {

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
// Draw Data
// ====================================================================================================

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

struct ParticleData {
	public Vector3 position;
	public Vector3 scale;

	public Vector2 tiling;
	public Vector2 offset;
	public Vector3 color;
	public float   emission;
	public float   alpha;
}



// ====================================================================================================
// Draw Manager
// ====================================================================================================

public class DrawManager : MonoBehaviour {

	// Fields

	[SerializeField] Mesh sphereMesh;
	[SerializeField] Mesh quadMesh;

	[SerializeField] Material creatureMaterial;
	[SerializeField] Material particleMaterial;

	[SerializeField] AtlasMapSO creatureAtlasMap;
	[SerializeField] AtlasMapSO particleAtlasMap;

	HashMap<int, int         > creatureSizeMap = new HashMap<int, int         >();
	HashMap<int, int         > particleSizeMap = new HashMap<int, int         >();
	HashMap<int, CreatureData> creatureDataMap = new HashMap<int, CreatureData>();
	HashMap<int, ParticleData> particleDataMap = new HashMap<int, ParticleData>();
	
	GPUBatcher<CreatureData> creatureBatcher;
	GPUBatcher<ParticleData> particleBatcher;



	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(DrawManager)), CanEditMultipleObjects]
	public class DrawManagerEditor : Editor {
		DrawManager I => target as DrawManager;

		T ObjectField<T>(string label, T obj) where T : Object {
			return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), true);
		}

		public override void OnInspectorGUI() {
			Space();
			LabelField("Meshes", EditorStyles.boldLabel);
			I.sphereMesh = ObjectField("Sphere Mesh", I.sphereMesh);
			I.quadMesh   = ObjectField("Quad Mesh"  , I.quadMesh  );

			Space();
			LabelField("Materials", EditorStyles.boldLabel);
			I.creatureMaterial = ObjectField("Creature Material",  I.creatureMaterial);
			I.particleMaterial = ObjectField("Particle Material",  I.particleMaterial);

			Space();
			LabelField("Atlas Maps", EditorStyles.boldLabel);
			I.creatureAtlasMap = ObjectField("Creature Atlas Map", I.creatureAtlasMap);
			I.particleAtlasMap = ObjectField("Particle Atlas Map", I.particleAtlasMap);

			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
	#endif



	// Cycle

	void OnEnable() {
		creatureBatcher = new GPUBatcher<CreatureData>(creatureMaterial, quadMesh, 0);
		particleBatcher = new GPUBatcher<ParticleData>(particleMaterial, quadMesh, 0);
	}

	void OnDisable() {
		creatureBatcher?.Dispose();
		particleBatcher?.Dispose();
	}

	void Start() {
		float PixelPerUnit = UIManager.Instance.pixelPerUnit;

		// CreatureType _ AnimationType _ Direction _ Index _ Duration
		creatureSizeMap.Clear();
		creatureDataMap.Clear();
		if (creatureAtlasMap) foreach (var pair in creatureAtlasMap.atlasMap) {
			string[] split = pair.Key.Split('_');
			if (split.Length != 5) continue;

			int[] value = new int[5];
			value[0] = (int)System.Enum.Parse(typeof(CreatureType ), split[0]);
			value[1] = (int)System.Enum.Parse(typeof(AnimationType), split[1]);
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
				creatureSizeMap[key[k]]++;
				if (k == 0 || creatureSizeMap.ContainsKey(key[k - 1])) break;
			}
			if (!creatureSizeMap.ContainsKey(key[4])) creatureSizeMap[key[4]] = value[4];
			if (1 < creatureSizeMap[key[3]]) creatureSizeMap[key[4]] += creatureSizeMap[key[4] - 1];

			creatureDataMap.Add(key[4], new CreatureData() {
				position = new Vector3(0, 0, 0),
				rotation = new Vector4(0, 0, 0, 1),
				scale    = new Vector3(pair.Value.size.x, pair.Value.size.y, 1) / PixelPerUnit,

				tiling   = new Vector2(pair.Value.tiling.x, pair.Value.tiling.y),
				offset   = new Vector2(pair.Value.offset.x, pair.Value.offset.y),
				color    = new Vector3(1, 1, 1),
				emission = 0,
				alpha    = 1,
			});
		}

		// ParticleType _ Index _ Duration
		particleSizeMap.Clear();
		particleDataMap.Clear();
		if (particleAtlasMap) foreach (var pair in particleAtlasMap.atlasMap) {
			string[] split = pair.Key.Split('_');
			if (split.Length != 3) continue;

			int[] value = new int[3];
			value[0] = (int)System.Enum.Parse(typeof(ParticleType), split[0]);
			value[1] = int.Parse(split[1]);

			int[] key = new int[3];
			key[0] = 0;
			key[1] = key[0] + (((value[0] + 1) & 0xff) << 24);
			key[2] = key[1] + (((value[1] + 1) & 0xff) << 16);

			for (int k = 2 - 1; -1 < k; k--) {
				if (!particleSizeMap.ContainsKey(key[k])) particleSizeMap.Add(key[k], 0);
				particleSizeMap[key[k]]++;
				if (k == 0 || particleSizeMap.ContainsKey(key[k - 1])) break;
			}
			if (!particleSizeMap.ContainsKey(key[2])) particleSizeMap[key[2]] = value[2];
			if (1 < particleSizeMap[key[1]]) particleSizeMap[key[2]] += particleSizeMap[key[2] - 1];

			particleDataMap.Add(key[2], new ParticleData() {
				position = new Vector3(0, 0, 0),
				scale    = new Vector3(pair.Value.size.x, pair.Value.size.y, 1) / PixelPerUnit,

				tiling   = new Vector2(pair.Value.tiling.x, pair.Value.tiling.y),
				offset   = new Vector2(pair.Value.offset.x, pair.Value.offset.y),
				color    = new Vector3(1, 1, 1),
				emission = 0,
				alpha    = 1,
			});
		}
	}
	
	void LateUpdate() {
		Creature[] creatures = FindObjectsByType<Creature>(FindObjectsSortMode.None);
		foreach (var creature in creatures) {
			int[] key = new int[5];
			key[0] = 0;
			//key[1] = key[0] + (((creature.type      + 1) & 0xff) << 24);
			//key[2] = key[1] + (((creature.animation + 1) & 0xff) << 16);
			//key[3] = key[2] + (((creature.direction + 1) & 0xff) <<  8);
			//key[4] = key[3] + (((creature.index     + 1) & 0xff) <<  0);
			//CreatureData data = creatureDataMap[key[4]];

			//data.position = creature.transform.position;
			//data.rotation = creature.transform.rotation;
			//data.scale    = creature.transform.localScale;
			//data.color    = creature.color;
			//data.emission = creature.emission;
			//data.alpha    = creature.alpha;
			//creatureBatcher.Add(data);
		}

		creatureBatcher.Draw();
		particleBatcher.Draw();

		creatureBatcher.Clear();
		particleBatcher.Clear();
	}

	/*
	[BurstCompile]
	partial struct GetDrawDataJob : IJobParallelFor {
		[WriteOnly] public NativeArray<DrawData>           buffer;
		[ ReadOnly] public NativeHashMap<int, int>         sizeMap;
		[ ReadOnly] public NativeHashMap<int, DrawData>    dataMap;
		[ ReadOnly] public NativeArray<Entity>             entities;
		[ ReadOnly] public ComponentLookup<LocalTransform> transforms;
		[ ReadOnly] public ComponentLookup<Creature>       creatures;

		int GetCreatureSize(
		CreatureType  creatureType  = (CreatureType )(-1),
		AnimationType animationType = (AnimationType)(-1),
		int           direction     = -1,
		int           index         = -1)
		=> sizeMap.TryGetValue(
			((((int)creatureType  + 1) & 0xff) << 24) |
			((((int)animationType + 1) & 0xff) << 16) |
			(((direction          + 1) & 0xff) <<  8) |
			(((index              + 1) & 0xff) <<  0),
			out int count) ? count : 0;

		DrawData GetCreatureData(
			CreatureType  creatureType,
			AnimationType animationType,
			int           direction,
			int           index)
			=> dataMap.TryGetValue(
				((((int)creatureType  + 1) & 0xff) << 24) |
				((((int)animationType + 1) & 0xff) << 16) |
				(((direction          + 1) & 0xff) <<  8) |
				(((index              + 1) & 0xff) <<  0),
				out DrawData drawData) ? drawData : DrawData.Default;

		float GetRelativeYaw(quaternion rotation) {
			float y = 0.0f + 2.0f * (rotation.value.y * rotation.value.w + rotation.value.x * rotation.value.z);
			float x = 1.0f - 2.0f * (rotation.value.y * rotation.value.y + rotation.value.x * rotation.value.x);
			return math.atan2(y, x) * 180f / math.PI; // - CameraManager.CameraDirection
		}

		public void Execute(int index) {
			var transform = transforms[entities[index]];
			var creature  = creatures [entities[index]];

			int yaw = 0;
			//int  yaw = (int)Mathf.Repeat(GetRelativeYaw(transform.Rotation) + 540f, 256);
			int  i;
			int  direction = 0;
			bool xflip = false;
			switch (GetCreatureSize(creature.creatureType, creature.animationType)) {
				case  1: i = (yaw +  0) /  2 %  2; direction = 0 < i ?  2 - i : i; xflip = 0 < i; break;
				case  2: i = (yaw +  0) /  2 %  2; direction = i;                  xflip = false; break;
				case  3: i = (yaw + 32) /  4 %  4; direction = 2 < i ?  4 - i : i; xflip = 2 < i; break;
				case  4: i = (yaw + 32) /  4 %  4; direction = i;                  xflip = false; break;
				case  5:
				case  6: 
				case  7: i = (yaw + 16) /  8 %  8; direction = 4 < i ?  8 - i : i; xflip = 4 < i; break;
				case  8: i = (yaw + 16) /  8 %  8; direction = i;                  xflip = false; break;
				case  9:
				case 10:
				case 11:
				case 12:
				case 13:
				case 14:
				case 15: i = (yaw +  8) / 16 % 16; direction = 8 < i ? 16 - i : i; xflip = 8 < i; break;
				case 16: i = (yaw +  8) / 16 % 16; direction = i;                  xflip = false; break;
			}
			int count = GetCreatureSize(creature.creatureType, creature.animationType, direction);
			int total = GetCreatureSize(creature.creatureType, creature.animationType, direction, count - 1);
			int m = 0;
			int l = 0;
			int r = count - 1;
			int iValue = (int)(creature.offset * 1000) % (total == 0 ? 1 : total);
			int lValue;
			int rValue;
			while (l <= r) {
				m = (l + r) / 2;
				lValue = GetCreatureSize(creature.creatureType, creature.animationType, direction, m - 1);
				rValue = GetCreatureSize(creature.creatureType, creature.animationType, direction, m + 0);
				if      (iValue < lValue) r = m - 1;
				else if (rValue < iValue) l = m + 1;
				else break;
			}
			DrawData drawData = GetCreatureData(creature.creatureType, creature.animationType, direction, m);
			drawData.position = transform.Position;
			drawData.rotation = transform.Rotation.value;
			if (xflip) drawData.scale.x *= -1;
			buffer[index] = drawData;
		}
	}
	*/
}
