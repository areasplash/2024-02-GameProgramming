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
// Custom Button
// ====================================================================================================

public class CustomButton : Selectable, IPointerClickHandler {

	[System.Serializable] public class ButtonFresh : UnityEvent {}
	[System.Serializable] public class ButtonEvent : UnityEvent {}



	// Fields

	[SerializeField] TextMeshProUGUI textTMP;
	[SerializeField] TextMeshProUGUI labelTMP;

	public ButtonFresh onFreshInvoked = new ButtonFresh();
	public ButtonEvent onClick        = new ButtonEvent();



	// Properties

	TextMeshProUGUI TextTMP  { get => textTMP;  set => textTMP  = value; }
	TextMeshProUGUI LabelTMP { get => labelTMP; set => labelTMP = value; }



	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(CustomButton)), CanEditMultipleObjects]
		public class CustomButtonEditor : SelectableEditor {
			CustomButton I => target as CustomButton;

			T ObjectField<T>(string label, T obj) where T : Object {
				return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), true);
			}

			public override void OnInspectorGUI() {
				base.OnInspectorGUI();
				
				Space();
				LabelField("Button", EditorStyles.boldLabel);
				I.TextTMP  = ObjectField("Text",  I.TextTMP );
				I.LabelTMP = ObjectField("Label", I.LabelTMP);

				Space();
				PropertyField(serializedObject.FindProperty("onFreshInvoked"));
				PropertyField(serializedObject.FindProperty("onClick"       ));

				if (GUI.changed) EditorUtility.SetDirty(target);
				serializedObject.ApplyModifiedProperties();
			}
		}
	#endif



	// Methods

	RectTransform Rect => transform as RectTransform;

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable) onClick?.Invoke();
	}

	public override void OnMove(AxisEventData eventData) {
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
			case MoveDirection.Right:
				DoStateTransition(SelectionState.Pressed, false);
				onClick?.Invoke();
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
		onFreshInvoked?.Invoke();
	}

	public void Refresh() {
	}
}
