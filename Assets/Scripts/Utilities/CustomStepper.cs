using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using System;

using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using TMPro;

#if UNITY_EDITOR
	using UnityEditor;
	using UnityEditor.UI;
	using static UnityEditor.EditorGUILayout;
#endif



#if UNITY_EDITOR
	[CustomEditor(typeof(CustomStepper)), CanEditMultipleObjects]
	public class CustomStepperEditor : SelectableEditor {

		SerializedProperty m_PrevRect;
		SerializedProperty m_NextRect;
		SerializedProperty m_TextTMP;
		SerializedProperty m_OnStateUpdated;
		SerializedProperty m_OnValueChanged;

		CustomStepper I => target as CustomStepper;

		protected override void OnEnable() {
			base.OnEnable();
			m_PrevRect       = serializedObject.FindProperty("m_PrevRect");
			m_NextRect       = serializedObject.FindProperty("m_NextRect");
			m_TextTMP        = serializedObject.FindProperty("m_TextTMP");
			m_OnStateUpdated = serializedObject.FindProperty("m_OnStateUpdated");
			m_OnValueChanged = serializedObject.FindProperty("m_OnValueChanged");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			Space();
			PropertyField(m_PrevRect);
			PropertyField(m_NextRect);
			PropertyField(m_TextTMP);
			Space();
			I.canMovePrev = Toggle("Can Move Prev", I.canMovePrev);
			I.canMoveNext = Toggle("Can Move Next", I.canMoveNext);
			Space();
			PropertyField(m_OnStateUpdated);
			PropertyField(m_OnValueChanged);
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
	}
#endif



// ====================================================================================================
// Custom Stepper
// ====================================================================================================

public class CustomStepper : Selectable, IPointerClickHandler {

	[Serializable] public class StepperUpdatedEvent : UnityEvent<CustomStepper> {}
	[Serializable] public class StepperChangedEvent : UnityEvent<int> {}



	// Fields

	[SerializeField] RectTransform   m_PrevRect;
	[SerializeField] RectTransform   m_NextRect;
	[SerializeField] TextMeshProUGUI m_TextTMP;

	[SerializeField] StepperUpdatedEvent m_OnStateUpdated;
	[SerializeField] StepperChangedEvent m_OnValueChanged;



	// Properties

	RectTransform rectTransform => transform as RectTransform;
	
	public bool canMovePrev {
		get => m_PrevRect && m_PrevRect.gameObject.activeSelf;
		set {
			if (m_PrevRect) m_PrevRect.gameObject.SetActive(value);
		}
	}

	public bool canMoveNext {
		get => m_NextRect && m_NextRect.gameObject.activeSelf;
		set {
			if (m_NextRect) m_NextRect.gameObject.SetActive(value);
		}
	}

	LocalizeStringEvent localizeStringEvent;

	public string text {
		get => m_TextTMP ? m_TextTMP.text : string.Empty;
		set {
			localizeStringEvent ??= GetComponent<LocalizeStringEvent>();
			localizeStringEvent.StringReference = null;
			if (m_TextTMP) m_TextTMP.text = value;
		}
	}

	public int value {
		get => 0;
		set {
			onValueChanged?.Invoke(value);
			Update();
		}
	}
	
	public StepperUpdatedEvent onStateUpdated {
		get => m_OnStateUpdated;
		set => m_OnStateUpdated = value;
	}

	public StepperChangedEvent onValueChanged {
		get => m_OnValueChanged;
		set => m_OnValueChanged = value;
	}



	// Methods

	public void OnPointerClick(PointerEventData eventData) {
		if (interactable) {
			Vector2 point = rectTransform.InverseTransformPoint(eventData.position);
			value = (0 <= point.x) && (point.x < rectTransform.rect.width / 3) ? -1 : 1;
		}
	}

	public override void OnMove(AxisEventData eventData) {
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
				DoStateTransition(SelectionState.Pressed, false);
				value = - 1;
				return;
			case MoveDirection.Right:
				DoStateTransition(SelectionState.Pressed, false);
				value = + 1;
				return;
		}
		base.OnMove(eventData);
	}

	ScrollRect scrollRect;

	bool TryGetComponentInParent<T>(out T component) where T : Component {
		Transform parent = transform.parent;
		while (parent) {
			if (TryGetComponent(out component)) return true;
			else parent = parent.parent;
		}
		component = null;
		return false;
	}

	public override void OnSelect(BaseEventData eventData) {
		base.OnSelect(eventData);
		if (eventData is AxisEventData) {
			if (scrollRect || TryGetComponentInParent(out scrollRect)) {
				Vector2 anchoredPosition = scrollRect.content.anchoredPosition;
				float pivot = rectTransform.rect.height / 2 - rectTransform.anchoredPosition.y;
				anchoredPosition.y = pivot - scrollRect.viewport.rect.height / 2;
				scrollRect.content.anchoredPosition = anchoredPosition;
			}
		}
	}

	public void SetLocalizeText(string table, string tableEntry) {
		if (localizeStringEvent || TryGetComponent(out localizeStringEvent)) {
			localizeStringEvent.StringReference = new LocalizedString {
				TableReference      = table,
				TableEntryReference = tableEntry
			};
			localizeStringEvent.RefreshString();
		}
	}

	public void Update() {
		onStateUpdated?.Invoke(this);
	}



	// Cycle

	protected override void OnEnable() {
		base.OnEnable();
		Update();
	}
}
