using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



public class Player : Entity {

	// ================================================================================================
	// Fields
	// ================================================================================================
	
	[SerializeField] List<EntityType> m_Holdings = new List<EntityType>();



	public List<EntityType> Holdings {
		get => m_Holdings;
		set => m_Holdings = value;
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
				I.Speed          = Slider      ("Speed",           I.Speed, 0, 20);
				I.Velocity       = Vector3Field("Velocity",        I.Velocity);
				I.ForcedVelocity = Vector3Field("Forced Velocity", I.ForcedVelocity);
				I.GroundVelocity = Vector3Field("Ground Velocity", I.GroundVelocity);
				I.GravitVelocity = Vector3Field("Gravit Velocity", I.GravitVelocity);
				Space();

				LabelField("Player", EditorStyles.boldLabel);
				PropertyField("m_Holdings");
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
				Velocity = input * Speed;
				if (0 < queue.Count) queue.Clear();
			}
			else {
				Vector3 delta = queue.Peek() - transform.position;
				Velocity = new Vector3(delta.x, 0, delta.z).normalized * Speed;
				if (new Vector3(delta.x, 0, delta.z).sqrMagnitude < 0.02f) queue.Dequeue();
			}
			if (Velocity != Vector3.zero) transform.rotation = Quaternion.LookRotation(Velocity);
		}

		// Interaction

		Entity interactablePrev = interactable;

		Utility.GetMatched(transform.position, SenseRange, (Entity entity) => {
			return entity != this && entity.Interactable(this) != InteractionType.None;
		}, ref interactable);

		GameInteractionUI.InteractText0.gameObject.SetActive(interactable);
		GameInteractionUI.InteractText1.gameObject.SetActive(interactable);
		if (interactable) {
			string text0 = interactable.GetType().Name;
			string text1 = interactable.Interactable(this).ToString();
			GameInteractionUI.InteractText0.SetLocalizeText("UI Table", text0);
			GameInteractionUI.InteractText1.SetLocalizeText("UI Table", text1);
			Vector3 pos0 = interactable.transform.position + Vector3.up * 1.5f;
			Vector3 pos1 = Vector3.Lerp(transform.position, interactable.transform.position, 0.5f);
			pos1.y -= 1f;
			GameInteractionUI.InteractText0.transform.position = CameraManager.WorldToScreen(pos0);
			GameInteractionUI.InteractText1.transform.position = CameraManager.WorldToScreen(pos1);

			if (InputManager.GetKeyDown(KeyAction.Interact)) interactable.Interact(this);
		}

		if (0 < Holdings.Count) {
			Vector3 position = transform.position + transform.forward * 0.5f;
			for (int i = 0; i < Holdings.Count; i++) {
				DrawManager.DrawEntity(position + Vector3.up * i * 0.5f, Holdings[i]);
			}
		}
	}
}
