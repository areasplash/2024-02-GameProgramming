using UnityEngine;

using System.Collections.Generic;
using UnityEngine.Localization.Components;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Player : Entity {

	// ================================================================================================
	// Fields
	// ================================================================================================
	
	[SerializeField] LocalizeStringEvent m_WorldText1;
	[SerializeField] LocalizeStringEvent m_WorldText2;



	LocalizeStringEvent WorldText1 {
		get => m_WorldText1;
		set => m_WorldText1 = value;
	}
	LocalizeStringEvent WorldText2 {
		get => m_WorldText2;
		set => m_WorldText2 = value;
	}

	string WorldText1String {
		get => m_WorldText1.StringReference.ToString();
		set => m_WorldText1.StringReference.SetReference("UI Table", value);
	}
	string WorldText2String {
		get => m_WorldText2.StringReference.ToString();
		set => m_WorldText2.StringReference.SetReference("UI Table", value);
	}

	



	#if UNITY_EDITOR
		[CustomEditor(typeof(Player))] class CreatureEditor : ExtendedEditor {
			Player I => target as Player;
			public override void OnInspectorGUI() {
				Begin("Player");

				LabelField("Entity", EditorStyles.boldLabel);
				I.EntityType = EnumField ("Entity Type", I.EntityType);
				I.MotionType = EnumField ("Motion Type", I.MotionType);
				I.Offset     = FloatField("Offset",      I.Offset);
				I.Color      = ColorField("Color",       I.Color);
				I.Intensity  = FloatField("Intensity",   I.Intensity);
				Space();
				I.HitboxType    = EnumField("Hitbox Type",    I.HitboxType);
				I.AttributeType = FlagField("Attribute Type", I.AttributeType);
				I.SenseRange    = Slider   ("Sense Range",    I.SenseRange, 0, 32);
				Space();

				LabelField("Rigidbody", EditorStyles.boldLabel);
				I.Velocity       = Vector3Field("Velocity",        I.Velocity);
				I.ForcedVelocity = Vector3Field("Forced Velocity", I.ForcedVelocity);
				I.GroundVelocity = Vector3Field("Ground Velocity", I.GroundVelocity);
				I.GravitVelocity = Vector3Field("Gravit Velocity", I.GravitVelocity);
				Space();

				LabelField("Player", EditorStyles.boldLabel);
				I.WorldText1 = ObjectField("World Text 1", I.WorldText1);
				I.WorldText2 = ObjectField("World Text 2", I.WorldText2);
				End();
			}
		}
	#endif



	// ================================================================================================
	// Methods
	// ================================================================================================



	// ================================================================================================
	// Lifecycle
	// ================================================================================================

	Queue<Vector3> queue = new Queue<Vector3>();

	Vector2 pointPosition;
	Vector3 rotation;

	Entity interactable;

	void Update() {

		// Input

		Vector3 input = Vector3.zero;
		if (UIManager.ActiveCanvas == CanvasType.Game) {
			if (CameraManager.Target != gameObject) CameraManager.Target = gameObject;
			
			input += CameraManager.Instance.transform.right   * InputManager.MoveDirection.x;
			input += CameraManager.Instance.transform.forward * InputManager.MoveDirection.y;
			input.y = 0;
			input.Normalize();
			
			if (InputManager.GetKeyDown(KeyAction.LeftClick)) {
				if (CameraManager.TryRaycast(InputManager.PointPosition, out Vector3 hit)) {
					FindPath(hit, ref queue);
				}
			}
			if (InputManager.GetKeyDown(KeyAction.RightClick)) {
				pointPosition = InputManager.PointPosition;
				rotation = CameraManager.EulerRotation;
			}
			if (InputManager.GetKey(KeyAction.RightClick)) {
				float mouseSensitivity = UIManager.MouseSensitivity;
				float delta = InputManager.PointPosition.x - pointPosition.x;
				CameraManager.EulerRotation = rotation + new Vector3(0, delta * mouseSensitivity, 0);
			}
		}

		// Movement

		if (input == Vector3.zero && queue.Count == 0) {
			if (MotionType != MotionType.Idle) Offset = 0;
			MotionType = MotionType.Idle;
			Velocity = Vector3.zero;
		}
		else {
			if (MotionType != MotionType.Move) Offset = 0;
			MotionType = MotionType.Move;
			if (input != Vector3.zero) {
				Velocity = input * 5;
				if (0 < queue.Count) queue.Clear();
			}
			else {
				Vector3 delta = queue.Peek() - transform.position;
				Velocity = new Vector3(delta.x, 0, delta.z).normalized * 5;
				if (new Vector3(delta.x, 0, delta.z).sqrMagnitude < 0.02f) queue.Dequeue();
			}
			if (Velocity != Vector3.zero) transform.rotation = Quaternion.LookRotation(Velocity);
		}

		// Interaction

		Entity interactablePrev = interactable;
		Utility.GetMatched(transform.position, SenseRange, (Entity entity) => {
			return entity != this && entity.Interactable(this) != InteractionType.None;
		}, ref interactable);

		if (interactablePrev != interactable) {
			WorldText1.gameObject.SetActive(interactable);
			WorldText2.gameObject.SetActive(interactable);
			if (interactable) {
				WorldText1String = interactable.GetType().Name;
				WorldText2String = interactable.Interactable(this).ToString();
			}
		}
		if (interactable) {
			if (InputManager.GetKeyDown(KeyAction.Interact)) interactable.Interact(this);
			WorldText1.transform.position = interactable.transform.position + Vector3.up * 2;
			WorldText2.transform.position = Vector3.Lerp(transform.position, interactable.transform.position, 0.5f);
			WorldText1.transform.rotation = CameraManager.Rotation;
			WorldText2.transform.rotation = CameraManager.Rotation;
		}
	}
}
