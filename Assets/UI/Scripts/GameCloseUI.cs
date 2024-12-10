using UnityEngine;
using UnityEngine.UI;



public class GameCloseUI : MonoBehaviour {
    
	[SerializeField] Image next;
	[Space]

	[SerializeField] Image table;
	[SerializeField] Image chair;
	[SerializeField] Image pot;
	[SerializeField] Image expansion;
	[SerializeField] Image staff;
	[Space]

	[SerializeField] Image tableMax;
	[SerializeField] Image chairMax;
	[SerializeField] Image potMax;
	[SerializeField] Image expansionMax;
	[SerializeField] Image staffMax;
	[Space]

	[SerializeField] RectTransform anchorTable;
	[SerializeField] RectTransform anchorChair;
	[SerializeField] RectTransform anchorPot;
	[SerializeField] RectTransform anchorExpansion;
	[SerializeField] RectTransform anchorStaff;
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
	[SerializeField] Sprite characterMoney;



	public bool IsTableActive     => table    .color.a == 1.0f;
	public bool IsChairActive     => chair    .color.a == 1.0f;
	public bool IsPotActive       => pot      .color.a == 1.0f;
	public bool IsExpansionActive => expansion.color.a == 1.0f;
	public bool IsStaffActive     => staff    .color.a == 1.0f;
	public bool IsNextActive      => next     .color.a == 1.0f;



	bool isOpen = false;

	int costTable     = -1;
	int costChair     = -1;
	int costPot       = -1;
	int costStaff     = -1;
	int costExpansion = -1;

	int money = -1;

	void Start() {
		SwitchUI(false);
	}

	void SwitchUI(bool value) {
		next     .gameObject.SetActive(value);

		table    .gameObject.SetActive(value);
		chair    .gameObject.SetActive(value);
		pot      .gameObject.SetActive(value);
		expansion.gameObject.SetActive(value);
		staff    .gameObject.SetActive(value);

		tableMax	.gameObject.SetActive(value);
		chairMax	.gameObject.SetActive(value);
		potMax		.gameObject.SetActive(value);
		expansionMax.gameObject.SetActive(value);
		staffMax	.gameObject.SetActive(value);
		
		anchorTable    .gameObject.SetActive(value);
		anchorChair    .gameObject.SetActive(value);
		anchorPot      .gameObject.SetActive(value);
		anchorExpansion.gameObject.SetActive(value);
		anchorStaff    .gameObject.SetActive(value);
	}

	void LateUpdate() {
		if (isOpen != GameManager.IsOpen) {
			isOpen  = GameManager.IsOpen;
			SwitchUI(!isOpen);
		}

		if (costTable != GameManager.TableCost) {
			costTable  = GameManager.TableCost;
			if (anchorTable) {
				string text = costTable.ToString();
				//for (int i = 3; i < text.Length; i += 4) text = text.Insert(text.Length - i, ",");
				text = text.Insert(0, "M");
				DrawText(anchorTable, text);
			}
		}
		if (costChair != GameManager.ChairCost) {
			costChair  = GameManager.ChairCost;
			if (anchorChair) {
				string text = costChair.ToString();
				//for (int i = 3; i < text.Length; i += 4) text = text.Insert(text.Length - i, ",");
				text = text.Insert(0, "M");
				DrawText(anchorChair, text);
			}
		}
		if (costPot != GameManager.PotCost) {
			costPot  = GameManager.PotCost;
			if (anchorPot) {
				string text = costPot.ToString();
				//for (int i = 3; i < text.Length; i += 4) text = text.Insert(text.Length - i, ",");
				text = text.Insert(0, "M");
				DrawText(anchorPot, text);
			}
		}
		if (costExpansion != GameManager.ExpansionCost) {
			costExpansion  = GameManager.ExpansionCost;
			if (anchorExpansion) {
				string text = costExpansion.ToString();
				//for (int i = 3; i < text.Length; i += 4) text = text.Insert(text.Length - i, ",");
				text = text.Insert(0, "M");
				DrawText(anchorExpansion, text);
			}
		}
		if (costStaff != GameManager.StaffCost) {
			costStaff  = GameManager.StaffCost;
			if (anchorStaff) {
				string text = costStaff.ToString();
				//for (int i = 3; i < text.Length; i += 4) text = text.Insert(text.Length - i, ",");
				text = text.Insert(0, "M");
				DrawText(anchorStaff, text);
			}
		}

		if (money != GameManager.Money) {
			money  = GameManager.Money;
			UpdateText(table,     anchorTable,     tableMax,     GameManager.TableAvailable);
			UpdateText(chair,     anchorChair,     chairMax,     GameManager.ChairAvailable);
			UpdateText(pot,       anchorPot,       potMax,       GameManager.PotAvailable);
			UpdateText(expansion, anchorExpansion, expansionMax, GameManager.ExpansionAvailable);
			UpdateText(staff,     anchorStaff,     staffMax,     GameManager.StaffAvailable);
		}
	}

	void DrawText(RectTransform anchor, string text) {
		if (anchor) {
			int length = Mathf.Max(text.Length, anchor.childCount); 
			for (int i = 0; i < length; i++) {
				if (anchor.childCount <= i) {
					GameObject number = new GameObject("Number");
					number.transform.SetParent(anchor.transform);
					Image image = number.AddComponent<Image>();
					image.raycastTarget = false;
					image.enabled = false;
					RectTransform rectTransform = number.transform as RectTransform;
					rectTransform.anchoredPosition = new Vector2(-length * 4f + 4f + i * 8, 0);
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
					'M' => characterMoney,
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

	void UpdateText(Image image, RectTransform anchor, Image maxImage, int available) {
		if (image) image.color = new Color(1.0f, 1.0f, 1.0f, available switch {
			1 => 1.00f,
			_ => 0.25f,
		});
		for (int i = 0; i < anchor.childCount; i++) {
			if (anchor.GetChild(i).TryGetComponent(out Image e))
				e.color = new Color(1.0f, 1.0f, 1.0f, available switch {
					2 => 0.00f,
					1 => 1.00f,
					_ => 0.25f,
				});
		}
		if (maxImage) maxImage.color = new Color(1.0f, 1.0f, 1.0f, available switch {
			2 => 0.25f,
			_ => 0.00f,
		});
	}

	public void OnTableClick    () { if (IsTableActive    ) GameManager.OnTableClick    (); }
	public void OnChairClick    () { if (IsChairActive    ) GameManager.OnChairClick    (); }
	public void OnPotClick      () { if (IsPotActive      ) GameManager.OnPotClick      (); }
	public void OnExpansionClick() { if (IsExpansionActive) GameManager.OnExpansionClick(); }
	public void OnStaffClick    () { if (IsStaffActive    ) GameManager.OnStaffClick    (); }
	public void OnNextClick     () {                        GameManager.OnNextClick     (); }
}
