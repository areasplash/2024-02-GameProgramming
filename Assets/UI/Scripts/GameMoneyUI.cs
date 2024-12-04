using UnityEngine;
using UnityEngine.UI;



public class GameMoneyUI : MonoBehaviour {
    
	[SerializeField] RectTransform fill;
	[SerializeField] RectTransform anchor;

	[Space]
	[Header("Sprites")]
	[SerializeField] Sprite character0;
	[SerializeField] Sprite character1;
	[SerializeField] Sprite character2;
	[SerializeField] Sprite character3;
	[SerializeField] Sprite character4;
	[SerializeField] Sprite character5;
	[SerializeField] Sprite character6;
	[SerializeField] Sprite character7;
	[SerializeField] Sprite character8;
	[SerializeField] Sprite character9;
	[SerializeField] Sprite characterComma;



	int money = -1;

	void LateUpdate() {
		if (money == GameManager.Money) return;
		money = GameManager.Money;

		if (fill) {
			float ratio = Mathf.Clamp01(money / GameManager.MoneyThreshold);
			fill.localScale = new Vector3(ratio, 1, 1);
		}

		if (anchor) {
			string text = money.ToString();
			for (int i = 3; i < text.Length; i += 4) text = text.Insert(text.Length - i, ",");

			int length = Mathf.Max(text.Length, anchor.childCount); 
			for (int i = 0; i < length; i++) {
				if (anchor.childCount <= i) {
					GameObject number = new GameObject("Number");
					number.transform.SetParent(anchor.transform);
					number.AddComponent<Image>().enabled = false;
					RectTransform rectTransform = number.transform as RectTransform;
					rectTransform.anchoredPosition = new Vector2(i * 8, 0);
					rectTransform.sizeDelta = new Vector2(32, 32);
					rectTransform.localScale = Vector3.one;
				}
				else {
					anchor.GetChild(i).TryGetComponent(out Image image);
					image.enabled = false;
				}
			}
			for (int i = 0; i < text.Length; i++) {
				Sprite sprite = text[i] switch {
					'0' => character0,
					'1' => character1,
					'2' => character2,
					'3' => character3,
					'4' => character4,
					'5' => character5,
					'6' => character6,
					'7' => character7,
					'8' => character8,
					'9' => character9,
					',' => characterComma,
					_ => null
				};
				if (sprite) {
					anchor.GetChild(i).TryGetComponent(out Image image);
					image.sprite = sprite;
					image.enabled = true;
				}
			}
		}
	}
}
