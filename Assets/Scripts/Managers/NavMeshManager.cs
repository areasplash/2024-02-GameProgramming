using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;



public class NavMeshManager : MonoSingleton<NavMeshManager> {

	[SerializeField] List<NavMeshSurface> navMeshSurfaces = new List<NavMeshSurface>();



	public enum Hitbox {
		None = 0,
		S   = -1923039037,
		M   =  -902729914,
		L   =   287145453,
		XL  =   658490984,
		XXL =    65107623,
	}
	static readonly Dictionary<Hitbox, Vector2> HITBOX = new Dictionary<Hitbox, Vector2> {
		{ Hitbox.None, new Vector2(0.00f, 0.00f) },
		{ Hitbox.S,    new Vector2(0.00f, 0.00f) },
		{ Hitbox.M,    new Vector2(0.00f, 0.00f) },
		{ Hitbox.L,    new Vector2(0.00f, 0.00f) },
		{ Hitbox.XL,   new Vector2(0.00f, 0.00f) },
		{ Hitbox.XXL,  new Vector2(0.00f, 0.00f) },
	};

	

	public void Bake() {
		foreach (var navMeshSurface in navMeshSurfaces) navMeshSurface.BuildNavMesh();
	}

	public void Clear() {
		foreach (var navMeshSurface in navMeshSurfaces) navMeshSurface.RemoveData();
	}
}
