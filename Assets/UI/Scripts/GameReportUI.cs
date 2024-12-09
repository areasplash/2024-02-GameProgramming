using UnityEngine;



public class GameReportUI : MonoSingleton<GameReportUI> {

	[Header("Title")]
	[SerializeField] CustomText DailyReport;
	[Space]

	[Header("Text")]
	[SerializeField] CustomText CustomerCount;
	[SerializeField] CustomText Money;
	[SerializeField] CustomText Reputation;
	[SerializeField] CustomText DebtDeadline;
	[SerializeField] CustomText MoneyNeeded;
	[Space]

	[Header("Value")]
	[SerializeField] CustomText CustomerCountValue;
	[SerializeField] CustomText MoneyValue;
	[SerializeField] CustomText ReputationValue;
	[SerializeField] CustomText DebtDeadlineValue;
	[SerializeField] CustomText MoneyNeededValue;



	public static void SetCustomerCount(string value) {
		if (Instance && Instance.CustomerCountValue)
			Instance.CustomerCountValue.Text = value;
	}
	public static void SetMoney(string value) {
		if (Instance && Instance.MoneyValue)
			Instance.MoneyValue.Text = value;
	}
	public static void SetReputation(string value) {
		if (Instance && Instance.ReputationValue)
			Instance.ReputationValue.Text = value;
	}
	public static void SetDebtDeadline(string value) {
		if (Instance && Instance.DebtDeadlineValue)
			Instance.DebtDeadlineValue.Text = value;
	}
	public static void SetMoneyNeeded(string value) {
		if (Instance && Instance.MoneyNeededValue)
			Instance.MoneyNeededValue.Text = value;
	}



	bool isOpen = false;

	void Start() {
		SwitchUI(false);
	}

	void SwitchUI(bool value) {
		DailyReport.gameObject.SetActive(value);
		
		CustomerCount.gameObject.SetActive(value);
		Money        .gameObject.SetActive(value);
		Reputation   .gameObject.SetActive(value);
		DebtDeadline .gameObject.SetActive(value);
		MoneyNeeded  .gameObject.SetActive(value);

		CustomerCountValue.gameObject.SetActive(value);
		MoneyValue        .gameObject.SetActive(value);
		ReputationValue   .gameObject.SetActive(value);
		DebtDeadlineValue .gameObject.SetActive(value);
		MoneyNeededValue  .gameObject.SetActive(value);
	}

	void LateUpdate() {
		if (isOpen != GameManager.IsOpen) {
			isOpen  = GameManager.IsOpen;
			SwitchUI(!isOpen);
		}
	}
}
