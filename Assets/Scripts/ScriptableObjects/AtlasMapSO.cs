using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
	using UnityEngine.ProBuilder;
	using System.IO;
#endif



// ====================================================================================================
// Hash Map, Texture Data, Atlas Map
// ====================================================================================================

[System.Serializable]
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



[System.Serializable]
public struct TextureData {
	public Vector2 size;
	public Vector2 tiling;
	public Vector2 offset;
}



[System.Serializable]
public class AtlasMap : HashMap<string, TextureData> {}



// ====================================================================================================
// Atlas Map SO
// ====================================================================================================

[CreateAssetMenu(fileName = "AtlasMap", menuName = "Scriptable Objects/AtlasMap")]
public class AtlasMapSO : ScriptableObject {

	// Fields (Properties)

	[field: SerializeField] public string    TexturesDirectory { get; private set; } = "Assets/";
	[field: SerializeField] public string    PrefabsDirectory  { get; private set; } = "Assets/";
	[field: SerializeField] public Texture2D Atlas             { get; private set; } = null;
	[field: SerializeField] public AtlasMap  AtlasMap          { get; private set; } = new AtlasMap();

	[field: SerializeField] public int       MaximumAtlasSize  { get; private set; } = 8192;
	[field: SerializeField] public int       Padding           { get; private set; } = 0;



	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(AtlasMapSO)), CanEditMultipleObjects]
		public class AtlasMapSOEditor : Editor {
			AtlasMapSO i => target as AtlasMapSO;

			T ObjectField<T>(string label, T obj) where T : Object {
				return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), true);
			}

			public override void OnInspectorGUI() {
				Space();
				LabelField("Atlas Map", EditorStyles.boldLabel);
				i.TexturesDirectory = TextField  ("Textures Directory", i.TexturesDirectory);
				i.PrefabsDirectory  = TextField  ("Prefabs Directory",  i.PrefabsDirectory );
				i.Atlas             = ObjectField("Atlas",              i.Atlas            );
				                      IntField   ("Atlas Map Count",    i.AtlasMap.Count   );

				Space();
				LabelField("Atlas Settings", EditorStyles.boldLabel);
				i.MaximumAtlasSize  = IntField("Maximum Atlas Size",    i.MaximumAtlasSize );
				i.Padding           = IntField("Padding",               i.Padding          );
				if (i.Atlas && GUILayout.Button("Generate Atlas")) GenerateAtlas();

				if (GUI.changed) EditorUtility.SetDirty(target);
			}



			T[] LoadAsset<T>(string path) where T : Object {
				string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { path });
				T[] assets = new T[guids.Length];
				for (int i = 0; i < guids.Length; i++) {
					string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
					assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				}
				return assets;
			}

			void GenerateAtlas() {
				Texture2D[] textures = LoadAsset<Texture2D>(i.TexturesDirectory);
				GameObject[] prefabs = LoadAsset<GameObject>(i.PrefabsDirectory);

				Texture2D atlas = new Texture2D(i.MaximumAtlasSize, i.MaximumAtlasSize);
				Rect[] rects = atlas.PackTextures(textures, i.Padding, i.MaximumAtlasSize);
				byte[] bytes = atlas.EncodeToPNG();
				File.WriteAllBytes(AssetDatabase.GetAssetPath(i.Atlas), bytes);
				AssetDatabase.Refresh();

				AtlasMap prevMap = i.AtlasMap;
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
				i.AtlasMap.Clear();
				foreach (var pair in nextMap) i.AtlasMap.Add(pair.Key, pair.Value);
			}
		}
	#endif
}
