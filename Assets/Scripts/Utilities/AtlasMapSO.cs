using UnityEngine;
using UnityEngine.ProBuilder;

using System;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Atlas Map SO Editor
// ====================================================================================================

#if UNITY_EDITOR
	[CustomEditor(typeof(AtlasMapSO)), CanEditMultipleObjects]
	public class AtlasMapSOEditor : Editor {

		SerializedProperty m_TextureDirectory;
		SerializedProperty m_PrefabDirectory;
		SerializedProperty m_MaximumAtlasSize;
		SerializedProperty m_Padding;
		SerializedProperty m_Atlas;
		SerializedProperty m_AtlasMap;

		AtlasMapSO I => target as AtlasMapSO;

		void OnEnable() {
			m_TextureDirectory  = serializedObject.FindProperty("m_TextureDirectory");
			m_PrefabDirectory   = serializedObject.FindProperty("m_PrefabDirectory");
			m_MaximumAtlasSize  = serializedObject.FindProperty("m_MaximumAtlasSize");
			m_Padding           = serializedObject.FindProperty("m_Padding");
			m_Atlas             = serializedObject.FindProperty("m_Atlas");
			m_AtlasMap          = serializedObject.FindProperty("m_AtlasMap");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			Undo.RecordObject(target, "Change Atlas Map SO Properties");
			Space();
			PropertyField(m_TextureDirectory);
			PropertyField(m_PrefabDirectory);
			Space();
			PropertyField(m_MaximumAtlasSize);
			PropertyField(m_Padding);
			PropertyField(m_Atlas);
			if (m_Atlas.objectReferenceValue) {
				BeginHorizontal();
				PrefixLabel("Generate Atlas");
				if (GUILayout.Button("Generate")) I.GenerateAtlas();
				EndHorizontal();
			}
			BeginHorizontal();
			PrefixLabel("Atlas Map Size");
			LabelField($"{((AtlasMapSO.AtlasMap)m_AtlasMap.boxedValue).Count}");
			EndHorizontal();
			PropertyField(m_AtlasMap);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Atlas Map SO
// ====================================================================================================

[Serializable] public struct TextureData {
	public Vector2 size;
	public Vector2 tiling;
	public Vector2 offset;
}



[CreateAssetMenu(fileName = "AtlasMap", menuName = "Scriptable Objects/AtlasMap")]
public class AtlasMapSO : ScriptableObject {

	[Serializable] public class AtlasMap : HashMap<string, TextureData> {}



	// Fields

	[SerializeField] string    m_TextureDirectory = "Assets/Textures";
	[SerializeField] string    m_PrefabDirectory  = "Assets/Prefabs";
	[SerializeField] int       m_MaximumAtlasSize = 8192;
	[SerializeField] int       m_Padding          = 0;
	[SerializeField] Texture2D m_Atlas            = null;
	[SerializeField] AtlasMap  m_AtlasMap         = new AtlasMap();



	// Properties

	public Texture2D atlas    => m_Atlas;
	public AtlasMap  atlasMap => m_AtlasMap;



	// Methods

	#if UNITY_EDITOR
		T[] LoadAsset<T>(string path) where T : UnityEngine.Object {
			string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { path });
			T[] assets = new T[guids.Length];
			for (int i = 0; i < guids.Length; i++) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			}
			return assets;
		}

		public void GenerateAtlas() {
			Texture2D[] textures = LoadAsset<Texture2D>(m_TextureDirectory);
			GameObject[] prefabs = LoadAsset<GameObject>(m_PrefabDirectory);

			Texture2D atlas = new Texture2D(m_MaximumAtlasSize, m_MaximumAtlasSize);
			Rect[] rects = atlas.PackTextures(textures, m_Padding, m_MaximumAtlasSize);
			byte[] bytes = atlas.EncodeToPNG();
			File.WriteAllBytes(AssetDatabase.GetAssetPath(this.atlas), bytes);
			AssetDatabase.Refresh();

			AtlasMap prevMap = atlasMap;
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
			m_AtlasMap = nextMap;
		}
	#endif
}



// ====================================================================================================
// Hash Map
// ====================================================================================================

[Serializable] public class HashMap<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver {

	// Fields

	[SerializeField] List<K> m_Keys   = new List<K>();
	[SerializeField] List<V> m_Values = new List<V>();



	// Methods

	public void OnBeforeSerialize() {
		m_Keys  .Clear();
		m_Values.Clear();
		foreach (var pair in this) {
			m_Keys  .Add(pair.Key  );
			m_Values.Add(pair.Value);
		}
	}

	public void OnAfterDeserialize() {
		Clear();
		for (int i = 0; i < m_Keys.Count; i++) Add(m_Keys[i], m_Values[i]);
	}
}
