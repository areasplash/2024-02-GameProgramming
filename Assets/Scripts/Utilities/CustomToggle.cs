using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
	using UnityEditor;
	using UnityEditor.UI;
	using static UnityEditor.EditorGUILayout;
#endif



// ====================================================================================================
// Custom Toggle
// ====================================================================================================

public class CustomToggle : Selectable, IPointerClickHandler {

	[System.Serializable] public class ToggleFresh : UnityEvent<CustomToggle> {}
	[System.Serializable] public class ToggleEvent : UnityEvent<bool> {}



	// Fields

	[SerializeField] RectTransform   falseRect;
	[SerializeField] RectTransform   trueTect;
	[SerializeField] TextMeshProUGUI falseTMP;
	[SerializeField] TextMeshProUGUI trueTMP;
	[SerializeField] TextMeshProUGUI labelTMP;

	[SerializeField] bool value = false;

	public ToggleFresh onFreshInvoked = new ToggleFresh();
	public ToggleEvent onValueChanged = new ToggleEvent();



	// Properties

	RectTransform   FalseRect { get => falseRect; set => falseRect = value; }
	RectTransform   TrueTect  { get => trueTect;  set => trueTect  = value; }
	TextMeshProUGUI FalseTMP  { get => falseTMP;  set => falseTMP  = value; }
	TextMeshProUGUI TrueTMP   { get => trueTMP;   set => trueTMP   = value; }
	TextMeshProUGUI LabelTMP  { get => labelTMP;  set => labelTMP  = value; }

	public bool Value {
		get => value;
		set {
			if (this.value == value) return;
			this.value = value;
			onValueChanged?.Invoke(Value);
			onFreshInvoked?.Invoke(this );
		}
	}



	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(CustomToggle)), CanEditMultipleObjects]
		public class CustomToggleEditor : SelectableEditor {
			CustomToggle I => target as CustomToggle;

			T ObjectField<T>(string label, T obj) where T : Object {
				return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), true);
			}

			public override void OnInspectorGUI() {
				base.OnInspectorGUI();
				
				Space();
				I.FalseRect   = ObjectField("False Rect", I.FalseRect);
				I.TrueTect    = ObjectField("True Rect",  I.TrueTect );
				I.FalseTMP    = ObjectField("False TMP",  I.FalseTMP );
				I.TrueTMP     = ObjectField("True TMP",   I.TrueTMP  );
				I.LabelTMP    = ObjectField("Label",      I.LabelTMP );

				Space();
				I.Value       = Toggle     ("Value",       I.Value   );

				Space();
				PropertyField(serializedObject.FindProperty("onFreshInvoked"));
				PropertyField(serializedObject.FindProperty("onValueChanged"));
				
				if (GUI.changed) EditorUtility.SetDirty(target);
				serializedObject.ApplyModifiedProperties();
			}
		}
	#endif



	// Methods

	RectTransform Rect => transform as RectTransform;

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable) Value = !Value;
	}

	public override void OnMove(AxisEventData eventData) {
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
			case MoveDirection.Right:
				DoStateTransition(SelectionState.Pressed, false);
				Value = !Value;
				return;
		}
		base.OnMove(eventData);
	}

	public override void OnSelect(BaseEventData eventData) {
		base.OnSelect(eventData);
		if (eventData is AxisEventData) {
			ScrollRect scrollRect = GetComponentInParent<ScrollRect>();
			if (scrollRect) {
				Vector2 anchoredPosition = scrollRect.content.anchoredPosition;
				anchoredPosition.y = -scrollRect.viewport.rect.height / 2;
				anchoredPosition.y -= Rect.anchoredPosition.y - Rect.rect.height / 2;
				scrollRect.content.anchoredPosition = anchoredPosition;
			}
		}
	}



	protected override void Start() {
		base.Start();
		onFreshInvoked?.Invoke(this);
	}
	
	public void Fresh() {
		falseRect?.gameObject.SetActive(!value);
		falseTMP ?.gameObject.SetActive(!value);
		trueTect ?.gameObject.SetActive( value);
		trueTMP  ?.gameObject.SetActive( value);
	}
}
