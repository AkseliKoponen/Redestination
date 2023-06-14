using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RD.Combat;
using RD.UI;

namespace RD
{
	public class TestTransition : MonoBehaviour
	{
		public MenuCanvas _menuCanvas;
		public MenuButton _campButton;
		public int _battlesSinceCamp { get; private set; } = 0;
		public int _battlesPerCamp = 3;

		public void AfterCombatMenu()
		{
			_battlesSinceCamp++;
			gameObject.SetActive(true);
			_menuCanvas.CreateMenu(MenuCanvas.MenuState.AfterCombat);

		}
		public void Hide()
		{
			StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(GetComponentInChildren<CanvasGroup>(), false, 5, true));
		}
		public void NextCombat()
		{
			//GameManager.NextCombat();
		}
		public void Camp()
		{
			_battlesSinceCamp = 0;
			Debug.Log("Camp!");
			CombatCharacter cc = Party.GetCharacters()[0];
			cc.Camp();
			GameManager._current._guiParts._inspection.DisplayCharacter(cc,false);
			//Display Talent
		}
	}
}
