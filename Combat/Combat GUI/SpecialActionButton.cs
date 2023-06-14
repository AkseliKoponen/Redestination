using RD.DB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static RD.CodeTools;
namespace RD.Combat
{
	[RequireComponent(typeof(CanvasGroup))]
	public class SpecialActionButton : MonoBehaviour
	{
		public Image _artField;
		public Image _glow;
		public Image _darkener;
		Card _actionCard;
		CombatCharacter _cc;
		CanvasGroup _cg;
		RectTransform _rt;
		Button _button;
		private void Awake()
		{
			_cg = GetComponent<CanvasGroup>();
			_rt = GetComponent<RectTransform>();
			_button = GetComponent<Button>();
		}
		public bool AssignCharacter(CombatCharacter cc)
		{
			if (cc._specialActionCard == null) {
				Hide();
				return false;
			}
			_cc = cc;
			_actionCard = cc._specialActionCard;
			Show();
			return true;
		}
		public void Hide()
		{
			Disable();
			StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(_cg, false, 4, true));
			_actionCard = null;
		}
		public void Disable()
		{
			_button.interactable = false;
			_darkener.gameObject.SetActive(true);
			_glow.gameObject.SetActive(false);
		}
		public void Show()
		{
			Enable();
			_artField.sprite = _actionCard._artSprite;
			StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(_cg, true, 4, true));
		}
		public void Enable()
		{
			_button.interactable = true;
			_darkener.gameObject.SetActive(false);
			_glow.gameObject.SetActive(true);
		}
		public void PointerOver()
		{
			if(_button.interactable)
				TooltipSystem.DisplayTooltip(_actionCard, GetRecttransformPivotPoint(_rt, new Vector2(1, 1f), true));
			//show tooltip
		}
		public void PointerExit()
		{
			//hide tooltip
			if (_button.interactable)
				TooltipSystem.HideAllTooltips();
		}

		public void Click()
		{
			StartCoroutine(Activate());
		}
		IEnumerator Activate()
		{
			_combatGUI.ActivateTargeting(_actionCard);
			TargetingSystem ts = TargetingSystem._current;
			while (ts.ready == 0)
			{
				yield return null;
			}
			if (ts.ready > 0)
			{
				_cc.PlayCard(_actionCard, ts.GetTargets());
			}
		}
	}
}