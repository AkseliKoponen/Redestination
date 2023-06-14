using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RD.Combat
{
	public class Portrait : MonoBehaviour
	{
		public Image _portraitImage;
		public TextMeshProUGUI _damageText;
		public TextMeshProUGUI _nameText;

		public void SetPortrait(HistoryDisplay.TargetResult tr)
		{
			CombatCharacter cc = tr.cc;
			Sprite sprite;
			RectTransform prt = GetComponent<RectTransform>();
			RectTransform rt = _portraitImage.GetComponent<RectTransform>();
			if (cc._dialogCharacter && cc._dialogCharacter._portrait)
			{
				sprite = cc._dialogCharacter._portrait;
				rt.sizeDelta = Vector2.zero;
			}
			else
			{
				sprite = cc._cSprite._defaultSprite;
				float width = prt.sizeDelta.x*0.5f;
				rt.sizeDelta = new Vector2(width, sprite.rect.height / sprite.rect.width * width);
			}
			_portraitImage.sprite = sprite;
			_nameText.text = cc.GetName();
			int damage = tr.damage != null ? tr.damage.GetDamage() : 0;
			if (tr.damage == null)
				Debug.Log(tr.cc.GetName() + " tr damage is null");
			//Debug.Log(cc.GetName() + " portrait damage = " + damage);
			if (damage != 0)
			{
				_damageText.gameObject.SetActive(true);
				_damageText.text = (damage*-1).ToString();
				_damageText.color = (damage > 0)?Color.red:Color.green;
			}
			else
			{
				_damageText.gameObject.SetActive(false);
			}
		}
	}
}
