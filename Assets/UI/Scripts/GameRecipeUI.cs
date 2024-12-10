using UnityEngine;
using UnityEngine.UI;



public class GameRecipeUI : MonoSingleton<GameRecipeUI> {
    
	[SerializeField] Image recipeBackground;
	[SerializeField] Image recipeImage;



	static Image RecipeBackground {
		get   =>  Instance? Instance.recipeBackground : default;
		set { if (Instance) Instance.recipeBackground = value; }
	}
	static Image RecipeImage {
		get   =>  Instance? Instance.recipeImage : default;
		set { if (Instance) Instance.recipeImage = value; }
	}

	public static bool IsShowing {
		get => RecipeImage? RecipeImage.enabled : default;
		set {
			if (RecipeBackground) RecipeBackground.enabled = value;
			if (RecipeImage     ) RecipeImage     .enabled = value;
		}
	}



	public void SwitchRecipe() => IsShowing = !IsShowing;
}
