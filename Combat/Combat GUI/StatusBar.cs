using System.Collections;
using System.Collections.Generic;
using RD;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RD.CodeTools;
namespace RD.Combat
{
	public class StatusBar : MonoBehaviour
	{
		public CombatCharacter _cc { get; private set; }
		public float speed = 1f;
		[SerializeField] Image _healthImg;
		[SerializeField] Image _healthLossImg;
		[SerializeField] Image _absorbBar;
		public Transform _aurasParent;
		float _healthTextOriginalY;
		[SerializeField] TextMeshProUGUI _healthText;
		[SerializeField] TextMeshProUGUI _nameText;
		[SerializeField] Image _selectionLines;
		[SerializeField] Sprite _selectionImage;
		[SerializeField] Sprite _myTurnImage;
		[SerializeField] AuraGUI _auraGUIPrefab;
		List<AuraGUI> _auras = new List<AuraGUI>();
		int _healthCurrent;
		int _absorbCurrent;
		HealthBarManager _manager;
		public void Init(CombatCharacter cc, HealthBarManager healthBarManager)
		{
			_manager = healthBarManager;
			_cc = cc;
			_cc._cSprite._statusBar = this;
			_healthTextOriginalY = _healthText.transform.parent.localPosition.y;
			_healthCurrent = cc._stats.hp.current;
			_absorbCurrent = cc._stats.barrier.current;
			gameObject.name = "Health Bar" + _cc.GetName();
			{
				Color slc = new Color();
				switch (cc._alliance)
				{
					case CombatCharacter.Alliance.Enemy:
						slc = Color.red;//ColorUtility.TryParseHtmlString("#FF3A2A", out slc);
						break;
					case CombatCharacter.Alliance.Player:
						ColorUtility.TryParseHtmlString("#66CEFF", out slc);
						break;
					case CombatCharacter.Alliance.Object:
					case CombatCharacter.Alliance.Friendly:
						ColorUtility.TryParseHtmlString("#66FFB4", out slc);
						break;
				}
				_selectionLines.color = slc;
			}
			_selectionLines.gameObject.SetActive(false);
			UpdateStats(true);
			Update();
			ToggleText(false);
		}
		public bool UpdateStats(bool instant = false)
		{
			//Debug.Log("UpdateStats - "+_cc.GetName());
			UpdateName();
			bool changes = false;
			if (_healthCurrent != _cc._stats.hp.current || _absorbCurrent != _cc._stats.barrier.current)
				changes = true;
			if (instant)
			{
				_healthImg.fillAmount = _cc._stats.hp.GetPercentage();
				_healthLossImg.fillAmount = _healthImg.fillAmount;
				_healthCurrent = _cc._stats.hp.current;
				_absorbCurrent = _cc._stats.barrier.current;
				_absorbBar.fillAmount = Mathf.Clamp((float)_absorbCurrent / (float)_healthCurrent, 0f, 1f);
				SetHealthText();
			}
			else if(changes)
			{
				StartCoroutine(LerpValues());
			}
			RefreshAuras();
			return changes;
		}
		void SetHealthText(int hp = default, int barrier = default)
		{
			string hpString = "HP: " + (hp == default ? _cc._stats.hp.GetCompareString(" / ") : hp + " / " + _cc._stats.hp.max);
			if(barrier!=default || _cc._stats.barrier.current > 0)
			{
				hpString += "\n<color=#00AEC6>Barrier: " + (barrier==default?_cc._stats.barrier.current:barrier)+ "</color>";
			}
			_healthText.text = hpString; 
		}
		private void Update()
		{

			ChangeCanvas();
			transform.position = new Vector3(_cc._cSprite._healthBarPosition.position.x,_combatGUI.GetHealthBarY());
			_nameText.transform.parent.position = _cc._cSprite._nameTextPosition.position;

		}
		int oldLayer = -1;
		public void ChangeCanvas()
		{
			//TODO: Not change canvas every tick
			int layer = _cc._cSprite._spriteRendererCharacter.gameObject.layer;
			if (layer != oldLayer)
			{
				if (layer == 10)
				{
					Transform parent = GameManager._current._guiParts._highlightCanvas.transform;
					if (transform.parent != parent)
						gameObject.transform.SetParent(parent);
				}
				else
				{
					Transform parent = _combatGUI._state != CombatGUI.StateOfGUI.Targeting ? _manager.transform : GameManager._current._guiParts._worldCanvas.transform;
					if (transform.parent != parent)
						gameObject.transform.SetParent(parent);
				}
			}
			oldLayer = layer;
		}
		IEnumerator LerpValues()
		{
			float time = 1f;
			float elapsedTime = 0f;
			float timeDiv = 0;
			int hpStart = _healthCurrent;
			int hpEnd = _cc._stats.hp.current;
			_healthImg.fillAmount = _cc._stats.hp.GetPercentage();
			float hpFillStart = _healthLossImg.fillAmount;
			float hpFillEnd = _healthImg.fillAmount;
			float absorbStart = _absorbBar.fillAmount;
			if (absorbStart > 0)
			{
				int hpDiff = Mathf.Abs(_healthCurrent - _cc._stats.hp.current);
				int barrierDiff = Mathf.Abs(_absorbCurrent - _cc._stats.barrier.current);
				timeDiv = (float)barrierDiff / (float)(hpDiff + barrierDiff);
				float absorbEnd = _cc._stats.barrier.GetPercentage();
				while (elapsedTime <= timeDiv)
				{
					elapsedTime += Tm.GetUIDelta() * speed;
					float fillAmount = Mathf.Lerp(absorbStart, absorbEnd, elapsedTime / timeDiv);
					_absorbBar.fillAmount = fillAmount;
					SetHealthText(hpStart,(int)Mathf.Lerp(_absorbCurrent,_cc._stats.barrier.current,elapsedTime/timeDiv));
					yield return null;
				}

				_absorbCurrent = _cc._stats.barrier.current;
				_absorbBar.fillAmount = absorbEnd;
			}
			//Lerp Barrier too!!!
			elapsedTime = 0;
			time -= timeDiv;
			while (elapsedTime <= time)
			{
				elapsedTime += Tm.GetUIDelta() * speed;
				_healthLossImg.fillAmount = Mathf.Lerp(hpFillStart, hpFillEnd, elapsedTime/time);
				SetHealthText(Mathf.RoundToInt(Mathf.Lerp(hpStart, hpEnd, elapsedTime/time)));
				yield return null;
			}
			_healthCurrent = _cc._stats.hp.current;
		}
		void UpdateName()
		{
			_nameText.text = _cc.GetName();
		}
		public void ToggleText(bool show)
		{
			//Debug.Log("ToggleText of "+_cc.GetName() +" = "+ show);
			if (_nameText == null || _healthText == null)
				return;
			if (show) {
				RectTransform boxT = _healthText.transform.parent.GetComponent<RectTransform>();
				Vector2 sizeDelta = boxT.sizeDelta;
				sizeDelta.y = _healthText.preferredHeight+18;
				sizeDelta.x = _healthText.preferredWidth+18;
				//sizeDelta.y = 20 + _healthText.preferredHeight + 5;
				boxT.sizeDelta = sizeDelta;

				sizeDelta = _nameText.transform.parent.GetComponent<RectTransform>().sizeDelta;
				sizeDelta.x = _nameText.preferredWidth + 8;
				_nameText.transform.parent.GetComponent<RectTransform>().sizeDelta = sizeDelta;
				boxT.localPosition = new Vector3(boxT.localPosition.x, _healthTextOriginalY);
			}
			_nameText.transform.parent.gameObject.SetActive(show);
			_healthText.transform.parent.gameObject.SetActive(show);
			if(show==false)Select(show);
			//_barText.gameObject.SetActive(show);
		}
		public void Destroy()
		{
			_manager.RemoveHealthbar(this);
			Destroy(gameObject);
		}
		public void SetTurn(bool enabled)
		{
			if (enabled)
			{
				_selectionLines.sprite = _myTurnImage;
				_selectionLines.gameObject.SetActive(enabled);
			}
			else
			{
				_selectionLines.GetComponent<LerpColor>()._disableObjectAfterLerp = true;
				_selectionLines.sprite = _selectionImage;
			}
			UIAnimationTools.ImageFadeIn(enabled, _selectionLines.GetComponent<LerpColor>(), _selectionLines.GetComponent<LerpTransform>(), 5, 1.25f);
		}
		public void Select(bool enabled)
		{
			if (_cc._trackers._myTurn)
				return;
			QuickToggleSelectionLines(enabled);
		}
		void QuickToggleSelectionLines(bool enabled)
		{
			Color c = _selectionLines.color;
			c.a = enabled ? 1 : 0;
			_selectionLines.color = c;
			_selectionLines.gameObject.SetActive(enabled);
			_selectionLines.transform.localScale = Vector3.one;
		}
		public void RefreshAuras()
		{
			foreach(AuraGUI ag in _auras.ToArray())
			{
				if (_cc._auras.Contains(ag._aura))
				{
					ag.Refresh();
				}
				else
				{
					_auras.Remove(ag);
					ag.Destroy();
				}
			}
			DrawAuras(_cc._auras);
		}
		public Transform GetSelectionLines()
        {
			return _selectionLines.transform;
        }
		public void DrawAuras(List<Aura> auras)
		{
			//ClearAuras();
			List<AuraGUI> templist = new List<AuraGUI>();
			foreach(Aura au in auras)
			{
				//Debug.Log(au._name);
				if (au._visible == false) {
					//Debug.Log(au._name+".visible = false");
					continue;
				}
				bool contains = false;
				foreach (AuraGUI ag in _auras)
				{
					if (ag._aura == au)
						contains = true;
				}
				if (contains==false)
				{
					//Debug.Log("Creating auraGUI for " + au._name);
					AuraGUI aura = Instantiate(_auraGUIPrefab, _aurasParent);
					aura.Init(au);
					templist.Add(aura);
				}
			}
			_auras.AddRange(templist);
		}
		public void ClearAuras()
		{
			foreach(AuraGUI aura in _auras)
			{
				Destroy(aura.gameObject);
			}
			_auras.Clear();
		}


	}
}
