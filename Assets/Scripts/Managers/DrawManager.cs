using UnityEngine;
using Unity.Collections;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Draw Manager Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(DrawManager)), CanEditMultipleObjects]
	public class DrawManagerEditor : Editor {

		SerializedProperty m_SphereMesh;
		SerializedProperty m_ShadowMaterial;
		SerializedProperty m_QuadMesh;
		SerializedProperty m_CreatureMaterial;
		SerializedProperty m_ParticleMaterial;
		SerializedProperty m_CreatureAtlasMap;
		SerializedProperty m_ParticleAtlasMap;

		DrawManager I => target as DrawManager;

		void OnEnable() {
			m_SphereMesh       = serializedObject.FindProperty("m_SphereMesh");
			m_ShadowMaterial   = serializedObject.FindProperty("m_ShadowMaterial");
			m_QuadMesh         = serializedObject.FindProperty("m_QuadMesh");
			m_CreatureMaterial = serializedObject.FindProperty("m_CreatureMaterial");
			m_ParticleMaterial = serializedObject.FindProperty("m_ParticleMaterial");
			m_CreatureAtlasMap = serializedObject.FindProperty("m_CreatureAtlasMap");
			m_ParticleAtlasMap = serializedObject.FindProperty("m_ParticleAtlasMap");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change Draw Manager Properties");
			Space();
			LabelField("Shadow", EditorStyles.boldLabel);
			PropertyField(m_SphereMesh);
			PropertyField(m_ShadowMaterial);
			Space();
			LabelField("Material", EditorStyles.boldLabel);
			PropertyField(m_QuadMesh);
			PropertyField(m_CreatureMaterial);
			PropertyField(m_ParticleMaterial);
			PropertyField(m_CreatureAtlasMap);
			PropertyField(m_ParticleAtlasMap);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Draw Manager
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



public class DrawManager : MonoBehaviour {

	// Fields

	[SerializeField] Mesh     m_SphereMesh;
	[SerializeField] Material m_ShadowMaterial;

	[SerializeField] Mesh       m_QuadMesh;
	[SerializeField] Material   m_CreatureMaterial;
	[SerializeField] Material   m_ParticleMaterial;
	[SerializeField] AtlasMapSO m_CreatureAtlasMap;
	[SerializeField] AtlasMapSO m_ParticleAtlasMap;



	// Methods
	
	GPUBatcher<CreatureData> creatureBatcher;
	GPUBatcher<ParticleData> particleBatcher;

	void ConstructGPUBatcher() {
		creatureBatcher = new GPUBatcher<CreatureData>(m_CreatureMaterial, m_QuadMesh, 0);
		particleBatcher = new GPUBatcher<ParticleData>(m_ParticleMaterial, m_QuadMesh, 0);
		creatureBatcher.param.layer = LayerMask.NameToLayer("Entity");
		particleBatcher.param.layer = LayerMask.NameToLayer("Entity");
		particleBatcher.param.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
	}

	void DestructGPUBatcher() {
		creatureBatcher?.Dispose();
		particleBatcher?.Dispose();
	}

	void Draw() {
		Creature[] creatures = FindObjectsByType<Creature>(FindObjectsSortMode.None);
		float[] alpha = new float[creatures.Length];

		Vector3 cameraRotation = CameraManager.I.Rotation;
		foreach (var creature in creatures) {
			int direction = 0;
			//Quaternion rotation = creature.transform.rotation;
			//float y = 0.0f + 2.0f * (rotation.y * rotation.w + rotation.x * rotation.z);
			//float x = 1.0f - 2.0f * (rotation.y * rotation.y + rotation.x * rotation.x);
			//int yaw = (int)Mathf.Repeat(Mathf.Atan2(y, x) * 180f / Mathf.PI - cameraRotation.y + 540f, 256);
			int yaw = (int)(creature.transform.eulerAngles.y - cameraRotation.y / 360f * 256f);
			int i;
			bool xflip = false;
			switch (GetCreatureSize(creature.CreatureType, creature.AnimationType)) {
				case  1: i = (yaw +  0) / 128 %  2; direction = 0 < i ?  2 - i : i; xflip = 0 < i; break;
				case  2: i = (yaw +  0) / 128 %  2; direction = i;                  xflip = false; break;
				case  3: i = (yaw + 32) /  64 %  4; direction = 2 < i ?  4 - i : i; xflip = 2 < i; break;
				case  4: i = (yaw + 32) /  64 %  4; direction = i;                  xflip = false; break;
				case  5:
				case  6: 
				case  7: i = (yaw + 16) /  32 %  8; direction = 4 < i ?  8 - i : i; xflip = 4 < i; break;
				case  8: i = (yaw + 16) /  32 %  8; direction = i;                  xflip = false; break;
				case  9:
				case 10:
				case 11:
				case 12:
				case 13:
				case 14:
				case 15: i = (yaw +  8) /  16 % 16; direction = 8 < i ? 16 - i : i; xflip = 4 < i; break;
				case 16: i = (yaw +  8) /  16 % 16; direction = i;                  xflip = false; break;
			}
			int count = GetCreatureSize(creature.CreatureType, creature.AnimationType, direction);
			int total = GetCreatureSize(creature.CreatureType, creature.AnimationType, direction, count - 1);
			int m = 0;
			int l = 0;
			int r = count - 1;
			int iValue = (int)(creature.Offset * 1000) % (total == 0 ? 1 : total);
			int lValue;
			int rValue;
			while (l <= r) {
				m = (l + r) / 2;
				lValue = GetCreatureSize(creature.CreatureType, creature.AnimationType, direction, m - 1);
				rValue = GetCreatureSize(creature.CreatureType, creature.AnimationType, direction, m + 0);
				if      (iValue < lValue) r = m - 1;
				else if (rValue < iValue) l = m + 1;
				else break;
			}
			//Debug.Log("yaw: " + yaw + " direction: " + direction + " xflip: " + xflip);
			//Debug.Log($"{creature.CreatureType}_{creature.AnimationType}_{direction}_{m}");
			CreatureData drawData = GetCreatureData(creature.CreatureType, creature.AnimationType, direction, m);
			drawData.position = creature.transform.position;
			drawData.rotation = ToVector4(cameraRotation);
			if (xflip) drawData.scale.y *= -1;
			creatureBatcher.Add(drawData);
		}

		creatureBatcher.Draw();
		particleBatcher.Draw();

		creatureBatcher.Clear();
		particleBatcher.Clear();
	}

	Vector4 ToVector4(Vector3 eulerAngle) {
		Quaternion quaternion = Quaternion.Euler(eulerAngle);
		return new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
	}

	Vector4 ToVector4(Quaternion quaternion) {
		return new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
	}



	readonly HashMap<int, int>          creatureSizeMap = new HashMap<int, int>();
	readonly HashMap<int, int>          particleSizeMap = new HashMap<int, int>();
	readonly HashMap<int, CreatureData> creatureDataMap = new HashMap<int, CreatureData>();
	readonly HashMap<int, ParticleData> particleDataMap = new HashMap<int, ParticleData>();

	int GetCreatureSize(
		CreatureType  creatureType  = (CreatureType )(-1),
		AnimationType animationType = (AnimationType)(-1),
		int           direction     = -1,
		int           index         = -1
	) => creatureSizeMap.TryGetValue(
		((((int)creatureType  + 1) & 0xff) << 24) |
		((((int)animationType + 1) & 0xff) << 16) |
		(((direction          + 1) & 0xff) <<  8) |
		(((index              + 1) & 0xff) <<  0),
		out int count) ? count : 0;
	
	CreatureData GetCreatureData(
		CreatureType  creatureType,
		AnimationType animationType,
		int           direction,
		int           index
	) => creatureDataMap.TryGetValue(
		((((int)creatureType  + 1) & 0xff) << 24) |
		((((int)animationType + 1) & 0xff) << 16) |
		(((direction          + 1) & 0xff) <<  8) |
		(((index              + 1) & 0xff) <<  0),
		out CreatureData data) ? data : new CreatureData {
			position = new Vector3(0, 0, 0),
			rotation = new Vector4(0, 0, 0, 1),
			scale    = new Vector3(1, 1, 1),
			tiling   = new Vector2(1, 1),
			offset   = new Vector2(0, 0),
			color    = new Vector3(1, 1, 1),
			emission = 0,
			alpha    = 1
		};

	void LoadMap() {
		float PixelPerUnit = UIManager.I.PixelPerUnit;

		// CreatureType _ AnimationType _ Direction _ Index _ Duration
		creatureSizeMap.Clear();
		creatureDataMap.Clear();
		if (m_CreatureAtlasMap) foreach (var pair in m_CreatureAtlasMap.atlasMap) {
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
		if (m_ParticleAtlasMap) foreach (var pair in m_ParticleAtlasMap.atlasMap) {
			string[] split = pair.Key.Split('_');
			if (split.Length != 3) continue;

			int[] value = new int[3];
			value[0] = (int)Enum.Parse(typeof(ParticleType), split[0]);
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



	// Lifecycle

	void OnEnable() => ConstructGPUBatcher();
	void OnDisable() => DestructGPUBatcher();

	void Start() => LoadMap();
	void LateUpdate() => Draw();
}



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



	// Methods

	public NativeArray<T> GetArray() => narray;

	public void Add(T value) => Insert(length, value);

	public void Insert(int index, T value) {
		if (Capacity < index + 1) Capacity = Mathf.Max(Capacity + 1, Capacity * 2);
		if (0 < length - index) NativeArray<T>.Copy(narray, index, narray, index + 1, length - index);
		narray[index] = value;
		length += 1;
	}

	public void AddRange(NativeList<T> list) => InsertRange(length, list);

	public void InsertRange(int index, NativeList<T> list) {
		int i = list.Length;
		if (Capacity < length + i) Capacity = Mathf.Max(Capacity + i, Capacity * 2);
		if (0 < length - index) NativeArray<T>.Copy(narray, index, narray, index + i, length - index);
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



	// Properties

	public int Length => narrayStructured.Length;

	public int Capacity {
		get => narrayStructured.Capacity;
		set => narrayStructured.Capacity = value;
	}



	// Constructor, Destructor

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



	// Methods

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
