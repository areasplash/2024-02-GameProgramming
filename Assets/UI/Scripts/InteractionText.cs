using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;



public class InteractionText : MonoBehaviour {

    // ================================================================================================
	// Fields
	// ================================================================================================
    [SerializeField] Camera mainCamera;
    [SerializeField] Canvas gameCanvas;
    [SerializeField] Player player;
    [SerializeField] TextMeshProUGUI textMeshPro1;
	[SerializeField] TextMeshProUGUI textMeshPro2;


    // ================================================================================================
	// Lifecycle
	// ================================================================================================
	
    Entity interactable;
    Vector2 targetPoint;
    LocalizeStringEvent worldText1;
    LocalizeStringEvent worldText2;

    void Start() {
        worldText1 = textMeshPro1.GetComponent<LocalizeStringEvent>();
        worldText2 = textMeshPro2.GetComponent<LocalizeStringEvent>();
    }
	void LateUpdate() {
        Entity interactablePrev = interactable;
		Utility.GetMatched(player.transform.position, player.SenseRange, (Entity entity) => {
			return entity != player && entity.Interactable(player) != InteractionType.None;
		}, ref interactable);

		if (interactablePrev != interactable) {
			textMeshPro1.gameObject.SetActive(interactable);
			textMeshPro2.gameObject.SetActive(interactable);
			    
		}

        if (interactable) {
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(interactable.transform.position);
                RectTransform canvasRect = gameCanvas.GetComponent<RectTransform>();

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPoint,
                    gameCanvas.worldCamera,
                    out targetPoint
                );
				string text1 = interactable.GetType().Name;
				string text2 = interactable.Interactable(player).ToString();
				worldText1.StringReference.SetReference("UI Table", text1);
				worldText2.StringReference.SetReference("UI Table", text2);
                textMeshPro1.rectTransform.anchoredPosition = targetPoint + Vector2.up * 20;
			    textMeshPro2.rectTransform.anchoredPosition = Vector2.Lerp(targetPoint, interactable.transform.position, 0.5f);
        }    
    }
}
