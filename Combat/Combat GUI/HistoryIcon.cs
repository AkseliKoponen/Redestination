using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static RD.CodeTools;

namespace RD.Combat
{
	public class HistoryIcon : MonoBehaviour
	{
		public Image _cardIcon;
		public Material _friendlyColor;
		public Material _hostileColor;
		public Material _highlightColor;
		[SerializeField] CanvasGroup _extension;
		[SerializeField] RectTransform _cardPosition;
		[SerializeField] RectTransform _targetParent;
		[SerializeField] Portrait _sourcePortrait;
		[SerializeField] CombatGUICard _cardPrefab;
		bool extensionInitialized = false;
		bool _isEnemy = false;
		HistoryDisplay.TargetResult _source;
		HistoryDisplay.CardResult _result;
		Card _card;
		public void Init(HistoryDisplay.TargetResult source, HistoryDisplay.CardResult target, Card card)
		{
			_source = source;
			_result = target;
			_card = card;
			_isEnemy = _source.cc._alliance == CombatCharacter.Alliance.Enemy;
			_cardIcon.sprite = card._artSprite;
			SetMaterial();
		}

		public void Highlight()
		{
			HistoryDisplay._current.HideAll(this);
			_cardIcon.material = _highlightColor;
			if (!extensionInitialized)
				InitializeExtension();
			ToggleExtension(true);
		}
		Task _fadeTask;
		void InitializeExtension()
		{
			//GET DAMAGE NUMBERS
			extensionInitialized = true;
			_extension.alpha = 0;
			CreateCard();
			_sourcePortrait.SetPortrait(_source);
			if(_result._characterTargets!= null && _result._characterTargets.Count > 0)
			{
				foreach(HistoryDisplay.TargetResult tr in _result._characterTargets)
				{
					Portrait port = Instantiate(_sourcePortrait.gameObject, _targetParent).GetComponent<Portrait>();
					port.SetPortrait(tr);
				}
			}
			void CreateCard()
			{
				CombatGUICard cg = Instantiate(_cardPrefab.gameObject, _cardPosition).GetComponent<CombatGUICard>();
				cg.InitCardForHistory(_card, _source.cc);
				RectTransform rt = cg.GetComponent<RectTransform>();
				SetPivot(new Vector2(0.5f, 1f));
				rt.anchoredPosition = new Vector2(0, 0);
				//SetPivot(new Vector2(0.5f, 0.5f));
				void SetPivot(Vector2 piv)
				{
					rt.SetPivot(piv);
					rt.anchorMin = piv;
					rt.anchorMax = piv;
				}
			}
		}
		float _fadeSpeed = 8f;
		void ToggleExtension(bool enable)
		{
			if (enable)
			{
				_fadeTask = new Task(UIAnimationTools.FadeCanvasGroupAlpha(_extension, enable, _fadeSpeed, false));
				_extension.transform.SetParent(HistoryDisplay._current.transform);
				_extension.transform.SetAsLastSibling();
			}
			else
			{
				if (_fadeTask!=null)
				{
					_fadeTask.Stop();
					_fadeTask = null;
				}
				_extension.alpha = 0;
				_extension.transform.SetParent(transform);
				_extension.transform.SetAsLastSibling();

			}
			
		}
		public void OnDestroy()
		{
			Destroy(_extension.gameObject);
		}
		public void Lowlight()
		{
			SetMaterial();
			ToggleExtension(false);
		}
		void SetMaterial()
		{

			_cardIcon.material = _isEnemy ? _hostileColor : _friendlyColor;
		}
		public void MouseOver()
		{
			Highlight();
		}
		public void MouseExit()
		{
			Lowlight();
		}

	}
}
