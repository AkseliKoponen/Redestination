using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RD;
using RD.DB;

namespace RD.Combat
{
	[RequireComponent(typeof(CanvasGroup))]
	public class CardRangeDisplay : MonoBehaviour
	{
		public TextMeshProUGUI _rangeText;
		public TextMeshProUGUI _areaText;

		public void SetRange(Card card)
		{
			if (card.TargetOnlySelf())
			{
				_rangeText.text = "Self";
				_areaText.transform.parent.gameObject.SetActive(false);
				RectTransform rangeBox = _rangeText.transform.parent.GetComponent<RectTransform>();
				rangeBox.pivot = new Vector2(0.5f, 1);
				rangeBox.anchoredPosition = Vector2.zero;
			}
			else
			{
				_rangeText.text = card._meleeRange ? "Melee" : "Ranged";
				switch (card._multiTarget)
				{
					case BaseCard.MultiTargetType.One:
						_areaText.text ="1 target";
						break;
					case BaseCard.MultiTargetType.Two:
						_areaText.text = "2 targets";
						break;
					case BaseCard.MultiTargetType.Three:
						_areaText.text = "3 targets";
						break;
					case BaseCard.MultiTargetType.All:
						_areaText.text = "all";
						break;
				}
			}
		}

		public void FadeOut(float speed) {
			StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(GetComponent<CanvasGroup>(), false, speed));
		}
	}
}
