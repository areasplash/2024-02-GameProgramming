using UnityEngine;



public class GameInteractionUI : MonoSingleton<GameInteractionUI> {

	[SerializeField] CustomText m_InteractText0;
	[SerializeField] CustomText m_InteractText1;



	public static CustomText InteractText0 {
		get           =>  Instance ? Instance.m_InteractText0 : default;
		private set { if (Instance)  Instance.m_InteractText0 = value; }
	}
	public static CustomText InteractText1 {
		get           =>  Instance ? Instance.m_InteractText1 : default;
		private set { if (Instance)  Instance.m_InteractText1 = value; }
	}
}
