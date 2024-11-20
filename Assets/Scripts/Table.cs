using System.Collections.Generic;
using UnityEngine;



// ====================================================================================================
// Structure
// ====================================================================================================

public class Structure : MonoBehaviour {

	// Constants

	const string PrefabPath = "Prefabs/Structure";



	// Fields

	[SerializeField] Vector3 m_Force = Vector3.zero;



	// Properties

	public Vector3 Force {
		get => m_Force;
		set => m_Force = value;
	}

	public int   LayerMask    { get; private set; }
	public float LayerOpacity { get; set; }



	// Methods

	static List<Structure> structureList = new List<Structure>();
	static List<Structure> structurePool = new List<Structure>();

	static Structure structure;
	static Structure structurePrefab;

	public static Structure Spawn(Vector3 position) {
		if (structurePool.Count == 0) {
			if (!structurePrefab) structurePrefab = Resources.Load<Structure>(PrefabPath);
			structure = Instantiate(structurePrefab);
		}
		else {
			int i = structurePool.Count - 1;
			structure = structurePool[i];
			structure.gameObject.SetActive(true);
			structurePool.RemoveAt(i);
		}
		structureList.Add(structure);
		structure.transform.position = position;
		return structure;
	}

	public static void Despawn(Structure structure) {
		structureList.Remove(structure);
		structurePool.Add   (structure);
		structure.gameObject.SetActive(false);
	}

	void OnRemove() {
		structureList.Remove(this);
		structurePool.Add   (this);
		gameObject.SetActive(false);
	}



	// Lifecycle

	List<Collider> layers       = new List<Collider>();
	bool           layerChanged = false;

	void BeginDetectLayer() {
		layerChanged = false;
	}

	void OnDetectLayerEnter(Collider collider) {
		if (collider.isTrigger) {
			layers.Add(collider);
			layerChanged = true;
		}
	}

	void OnDetectLayerExit(Collider collider) {
		if (collider.isTrigger) {
			layers.Remove(collider);
			layerChanged = true;
		}
	}

	void EndDetectLayer() {
		if (layerChanged) {
			LayerMask = 0;
			for(int i = 0; i < layers.Count; i++) LayerMask |= 1 << layers[i].gameObject.layer;
		}
	}



	List<Collider> grounds         = new List<Collider>();
	bool           groundChanged   = false;
	Rigidbody      groundRigidbody = null;
	Quaternion     groundRotation  = Quaternion.identity;
	bool           isGrounded      = false;

	void BeginDetectGround() {
		groundChanged = false;
	}

	void OnDetectGroundEnter(Collider collider) {
		if (!collider.isTrigger) {
			grounds.Add(collider);
			groundChanged = true;
		}
	}

	void OnDetectGroundExit(Collider collider) {
		if (!collider.isTrigger) {
			grounds.Remove(collider);
			groundChanged = true;
		}
	}

	void EndDetectGround() {
		if (groundChanged) {
			if (0 < grounds.Count) {
				int i = grounds.Count - 1;
				grounds[i].TryGetComponent(out groundRigidbody);
				groundRotation  = grounds[i].transform.rotation;
				isGrounded      = true;
			}
			else {
				groundRigidbody = null;
				groundRotation  = Quaternion.identity;
				isGrounded      = false;
			}
		}
	}

	

	Rigidbody rb;

	void StartPhysics() => TryGetComponent(out rb);

	void UpdatePhysics() {
		Vector3 velocity = Vector3.zero;
		Force = isGrounded ? Vector3.zero : Force + Physics.gravity * Time.deltaTime;
		rb.linearVelocity = groundRotation * velocity + Force;
		if (velocity != Vector3.zero) rb.rotation = Quaternion.LookRotation(velocity);
	}



	void Start() => StartPhysics();

	void OnEnable() {
		layerChanged  = true;
		groundChanged = true;
	}

	void FixedUpdate() {
		EndDetectLayer   ();
		BeginDetectLayer ();
		EndDetectGround  ();
		BeginDetectGround();

		UpdatePhysics();
	}

	void OnTriggerEnter(Collider collider) {
		OnDetectLayerEnter (collider);
		OnDetectGroundEnter(collider);
	}

	void OnTriggerExit(Collider collider) {
		OnDetectLayerExit (collider);
		OnDetectGroundExit(collider);
	}

	void OnDestory() => OnRemove();
}
