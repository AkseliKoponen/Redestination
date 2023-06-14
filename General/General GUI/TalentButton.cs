using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RD;
using RD.DB;

namespace RD.UI
{
	[RequireComponent(typeof(Button))]
	public class TalentButton : MonoBehaviour
	{
		public BaseTalent _bt { get; private set; }
		Button _button;
		public Sprite _chooseSprite;
		public Image _talentImg;
		Color _blockColor = Color.gray;
		bool _tooltipped = false;
		public bool _chosen { get; private set; } = false;
		public void AssignTalent(BaseTalent bt)
		{
			_button = GetComponent<Button>();
			_bt = bt;
			_talentImg.sprite = bt._sprite!=null?bt._sprite:null;

		}
		public void Disable()
		{
			_talentImg.color = _chosen?Color.white:_blockColor;
			_button.interactable = _chosen?true:false;
		}
		public void Enable()
		{
			_talentImg.color = Color.white;
			_button.interactable = true;
		}
		public void Click()
		{

			GetComponentInParent<InspectionWindow>().ClickTalent(this);
		}
		public void Choose()
		{
			GetComponent<Image>().sprite = _chooseSprite;
			_chosen = true;
		}
		public void Unchoose()
		{
			//Enable();
			_chosen = false;
			GetComponent<Image>().sprite = _button.spriteState.disabledSprite;
		}
		public void MouseOver()
		{
			TooltipSystem.DisplayTooltip(_bt, CodeTools.GetRecttransformPivotPoint(GetComponent<RectTransform>(), new Vector2(0, 0.5f), true));
			_tooltipped = true;
		}
		public void MoseExit()
		{
			if(_tooltipped)
				TooltipSystem.HideAllTooltips();
			_tooltipped = false;
		}
	}
}
