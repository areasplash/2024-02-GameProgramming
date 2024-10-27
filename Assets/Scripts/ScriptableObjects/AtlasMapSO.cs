using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using UnityEngine.ProBuilder;
	using System.IO;
#endif



// ====================================================================================================
// Hash Map, Texture Data, Atlas Map
// ====================================================================================================

[Serializable]
public class HashMap<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {

	[SerializeField] List<TKey  > keys   = new List<TKey  >();
	[SerializeField] List<TValue> values = new List<TValue>();

	public void OnBeforeSerialize() {
		keys  .Clear();
		values.Clear();
		foreach (var pair in this) {
			keys  .Add(pair.Key  );
			values.Add(pair.Value);
		}
	}

	public void OnAfterDeserialize() {
		Clear();
		for (int i = 0; i < keys.Count; i++) Add(keys[i], values[i]);
	}
}



[Serializable]
public struct TextureData {
	public Vector2 size;
	public Vector2 tiling;
	public Vector2 offset;
}



[Serializable]
public class AtlasMap : HashMap<string, TextureData> {}



// ====================================================================================================
// Atlas Map SO
// ====================================================================================================

[CreateAssetMenu(fileName = "AtlasMap", menuName = "Scriptable Objects/AtlasMap")]
public class AtlasMapSO : ScriptableObject {

	[SerializeField] string    texturesDirectory = "Assets/Textures";
	[SerializeField] string    prefabsDirectory  = "Assets/Prefabs";
	[SerializeField] Texture2D atlas;
	[SerializeField] AtlasMap  atlasMap = new AtlasMap();

	[SerializeField] int       maximumAtlasSize = 8192;
	[SerializeField] int       padding          = 0;

	public string    TexturesDirectory => texturesDirectory;
	public string    PrefabsDirectory  => prefabsDirectory;
	public Texture2D Atlas             => atlas;
	public AtlasMap  AtlasMap          => atlasMap;

	public int       MaximumAtlasSize  => maximumAtlasSize;
	public int       Padding           => padding;
}



// ====================================================================================================
// Atlas Map SO Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(AtlasMapSO))]
	public class AtlasMapSOEditor : Editor {
		AtlasMapSO AtlasMapSO;

		SerializedProperty TexturesDirectory;
		SerializedProperty PrefabsDirectory;
		SerializedProperty Atlas;
		SerializedProperty AtlasMap;

		SerializedProperty MaximumAtlasSize;
		SerializedProperty Padding;



		void OnEnable() {
			AtlasMapSO = target as AtlasMapSO;

			TexturesDirectory = serializedObject.FindProperty("texturesDirectory");
			PrefabsDirectory  = serializedObject.FindProperty("prefabsDirectory" );
			Atlas             = serializedObject.FindProperty("atlas"            );
			AtlasMap          = serializedObject.FindProperty("atlasMap"         );

			MaximumAtlasSize  = serializedObject.FindProperty("maximumAtlasSize" );
			Padding           = serializedObject.FindProperty("padding"          );
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Atlas Map", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(TexturesDirectory);
			EditorGUILayout.PropertyField(PrefabsDirectory );
			EditorGUILayout.PropertyField(Atlas            );
			EditorGUILayout.PropertyField(AtlasMap         );

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Atlas Settings", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(MaximumAtlasSize );
			EditorGUILayout.PropertyField(Padding          );
			if (Atlas.objectReferenceValue && GUILayout.Button("Generate Atlas")) GenerateAtlas();

			serializedObject.ApplyModifiedProperties();
		}



		T[] LoadAsset<T>(string path) where T : UnityEngine.Object {
			string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { path });
			T[] assets = new T[guids.Length];
			for (int i = 0; i < guids.Length; i++) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			}
			return assets;
		}

		void GenerateAtlas() {
			Texture2D[] textures = LoadAsset<Texture2D>(TexturesDirectory.stringValue);
			GameObject[] prefabs = LoadAsset<GameObject>(PrefabsDirectory.stringValue);
			int MaximumAtlasSize = this.MaximumAtlasSize.intValue;
			int Padding          = this.Padding.intValue;

			Texture2D atlas = new Texture2D(MaximumAtlasSize, MaximumAtlasSize);
			Rect[] rects = atlas.PackTextures(textures, Padding, MaximumAtlasSize);
			byte[] bytes = atlas.EncodeToPNG();
			File.WriteAllBytes(AssetDatabase.GetAssetPath(Atlas.objectReferenceValue), bytes);
			AssetDatabase.Refresh();

			AtlasMap prevMap = AtlasMapSO.AtlasMap;
			AtlasMap nextMap = new AtlasMap();
			for (int i = 0; i < textures.Length; i++) nextMap[textures[i].name] = new TextureData {
				size   = new Vector2(textures[i].width, textures[i].height),
				tiling = new Vector2(rects   [i].width, rects   [i].height),
				offset = new Vector2(rects   [i].x,     rects   [i].y     ),
			};
			
			for (int i = 0; i < prefabs.Length; i++) {
				bool match = true;
				match &= prefabs[i].TryGetComponent(out ProBuilderMesh probuilderMesh);
				match &= prevMap.TryGetValue(prefabs[i].name, out TextureData prev);
				match &= nextMap.TryGetValue(prefabs[i].name, out TextureData next);
				if (match) {
					List<Vector4> uv = new List<Vector4>();
					probuilderMesh.GetUVs(0, uv);
					foreach (Face face in probuilderMesh.faces) face.manualUV = true;
					for (int j = 0; j < uv.Count; j++) uv[j] = new Vector4(
						(uv[j].x - prev.offset.x) / prev.tiling.x * next.tiling.x + next.offset.x,
						(uv[j].y - prev.offset.y) / prev.tiling.y * next.tiling.y + next.offset.y,
						uv[j].z,
						uv[j].w
					);
					probuilderMesh.SetUVs (0, uv);
					probuilderMesh.ToMesh ();
					probuilderMesh.Refresh();
					string path = AssetDatabase.GetAssetPath(prefabs[i]);
					PrefabUtility.SaveAsPrefabAsset(prefabs[i], path);
				}
			}
			AtlasMapSO.AtlasMap.Clear();
			foreach (var pair in nextMap) AtlasMapSO.AtlasMap.Add(pair.Key, pair.Value);
		}
	}
#endif
