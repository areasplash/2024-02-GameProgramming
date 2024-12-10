using UnityEngine;
using UnityEngine.UI;



public class GameRecipeUI : MonoBehaviour {
    
	[SerializeField] Image recipeBackground;
	[SerializeField] Image recipeImage;

	

	public bool IsShowing {
		get => recipeBackground? recipeBackground.enabled : default;
		set {
			if (recipeBackground) recipeBackground.enabled = value;
			if (recipeImage     ) recipeImage     .enabled = value;
		}
	}



	public void SwitchRecipe() {
		GameManager.SFXSource.PlayOneShot(GameManager.TurnPageSFX);
		IsShowing = !IsShowing;
	}
}
