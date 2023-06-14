using System;
using System.Collections.Generic;
using RD.Combat;
using RD.DB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RD.CodeTools;
namespace RD.UI
{
	public class InspectionWindow:MonoBehaviour
	{
		public Combat.CombatCharacter _cc { get; private set; }
		public AuraGUI _auraGUIPrefab;
		public SerializedComponents _components;
		List<AuraGUI> _auras;
		[Serializable]public struct SerializedComponents
		{
			public TextMeshProUGUI _textName;
			public TextMeshProUGUI _textDescription;
			public Image _imgSprite;
			public TextMeshProUGUI _textLevel;
			public TextMeshProUGUI _textAttributeNames;
			public TextMeshProUGUI _textAttributeValues;
			public TextMeshProUGUI _textAttributePointsAvailable;
			public TextMeshProUGUI _textWeaponNames;
			public TextMeshProUGUI _textWeaponDamage;
			public Transform _effectLayout;
			public Button _inventoryButton;
			public CanvasGroup _mind;
			public CanvasGroup _misc;
			public TabManager _effectsTalentsTab;
			public Button _deckButton;
			public Button _handButton;
			public Button _discardButton;
		}
		CanvasGroup _cg;
		public Color _enabledColor = Color.white;
		public Color _disabledColor = Color.gray;
		bool _talentPointAvailable = false;
		public void DisplayCharacter(Combat.CombatCharacter cc, bool defaultView = true)
		{
			_cg = GetComponent<CanvasGroup>();
			gameObject.SetActive(true);
			StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(_cg, true, 5, true));

			if (_auras != null)
			{
				if (_auras.Count > 0)
				{
					for(int i = _auras.Count - 1; i >= 0; i--)
					{
						_auras[i].Destroy();
						_auras.RemoveAt(i);
					}

				}
			}
			_auras = new List<AuraGUI>();
			_cc = cc;

			if (defaultView)
				ViewDefault();
			else
				ViewLevelUp();
			UpdateGUI(true);
			
		}
		public void UpdateGUI(bool replaceAuras = false)
		{

			CombatCharacter c = _cc;
			DisplayArtAndTexts();
			DisplayAttributes(_cc._attributes);
			DisplayTalents();
			DisplayAuras();
			//Weapon damages
			//Deck Sizes
			void DisplayArtAndTexts()
			{

				_components._textName.text = c.GetName(false);
				_components._textDescription.text = c._description;
				_components._imgSprite.sprite = c._cSprite._spriteRendererCharacter.sprite; //Get Default Sprite
				_components._textLevel.text = "Level " + c._level;
				_components._textWeaponNames.text = (_cc._inventory._melee != null ? "Melee Damage\n" : "")
													+ (_cc._inventory._ranged != null ? "Ranged Damage" : "");
				//effectID 1 = strike, 22 = shoot
				_components._textWeaponDamage.text = (_cc._inventory._melee != null ? _cc.EstimateDamageQuick((BaseEffect)db.Get<BaseEffect>(1)) + "\n" : "")
													 + (_cc._inventory._ranged != null ? _cc.EstimateDamageQuick((BaseEffect)db.Get<BaseEffect>(22)).ToString() : "");
				_components._inventoryButton.gameObject.SetActive(c._alliance == Combat.CombatCharacter.Alliance.Player);
				InitDeckButton(_components._deckButton, c._deck._subcon);
				InitDeckButton(_components._discardButton, c._deck._forget);
				InitDeckButton(_components._handButton, c._deck._mind);
				_components._misc.transform.GetChild(1).gameObject.SetActive(!(_cc._attributes._availablePoints <= 0 && _cc._globalStats._talentPointsAvailable <= 0));
			}
			void DisplayAttributes(CombatCharacter.Attributes attr)
			{
				_components._textAttributeValues.text = _cc._stats.hp.GetCompareString() + "\n"
					+ attr.StrengthToString() + "\n" +
					attr.DexterityToString() + "\n" +
					attr.IntelligenceToString() + "\n" +
					attr.TenacityToString();
				_components._textAttributePointsAvailable.gameObject.SetActive(_cc._attributes._availablePoints > 0);
				_components._textAttributePointsAvailable.text = "points available: " + attr._availablePoints;
			}
			void DisplayTalents()
			{

				if (_cc._baseCC._talentTree != null)
				{
					TalentTree tt = _cc._baseCC._talentTree;
					TalentButton[] tbs = _components._effectsTalentsTab._tabs[1]._contents.gameObject.GetComponentsInChildren<TalentButton>();
					//Debug.Log("CC.Level = " + cc._level+"\nmathlevel = "+ (_cc._level / 2));
					for (int i = 0; i < tbs.Length && i < tt.GetLength(); i++)
					{
						BaseTalent bt = tt.GetTalentFromIndex(i);
						tbs[i].AssignTalent(bt);
						//if CC already has the talent, then lock(true)?
						foreach (Talent t in _cc._talents)
						{
							if (t._base == bt)
								tbs[i].Choose();
							else
								tbs[i].Unchoose();
						}
						if ((_cc._globalStats._talentPointsAvailable > 0 && ((_cc._level / 2) > (i / 3))))
							tbs[i].Enable();
						else
							tbs[i].Disable();
					}

				}
				else
				{
					_components._effectsTalentsTab._tabs[1]._button.enabled = false;
				}
			}
			void DisplayAuras()
			{

				if (replaceAuras)
					ClearAuras();
				int j = 0;
				foreach (Aura a in _cc._auras)
				{
					if (j < 42)
					{
						bool match = false;
						foreach (AuraGUI ag in _auras)
						{
							if (ag._type == AuraGUI.GUIType.Aura && a == ag._aura)
							{
								match = true;
								break;
							}
						}
						if (!match)
						{
							AuraGUI temp = Instantiate(_auraGUIPrefab.gameObject, _components._effectLayout).GetComponent<AuraGUI>();
							_auras.Add(temp);
							temp.Init(a);
						}
					}
					else
						break;
					j++;
				}
			}

		}

		void InitDeckButton(Button b, Deck d)
		{
			bool vis = d.GetVisible();
			b.interactable = vis;
			b.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = d._ideas.Count.ToString();
			TextMeshProUGUI tmp = b.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
			tmp.color = vis ? _enabledColor : _disabledColor;
			tmp.fontStyle = vis ? FontStyles.Normal : FontStyles.Strikethrough;
		}

		public void CloseWindow()
		{
			ClearAuras();
			StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(_cg, false, 5, true));
			gameObject.SetActive(false);
			if (_combatGUI._state == CombatGUI.StateOfGUI.Targeting)
				_combatGUI._comps._targetingSystem.HighlightTargets(true);
		}
		public void ClickTalent(TalentButton tb)
		{
			//Add or Remove Talent
			if (tb._chosen)
			{
				tb.Unchoose();
				_cc.RemoveTalent(tb._bt);
			}
			else
			{
				tb.Choose();
				_cc.TalentLearn(tb._bt);
			}
			UpdateGUI();
		}
		void ClearAuras()
		{

			for (int k = _auras.Count - 1; k >= 0; k--)
			{
				AuraGUI a = _auras[k];
				_auras.RemoveAt(k);
				Destroy(a.gameObject);
			}
		}
		public void ViewLevelUp()
		{
			UIAnimationTools.SetCanvasGroupActive(_components._misc, true);
			UIAnimationTools.SetCanvasGroupActive(_components._mind, false);
			_components._effectsTalentsTab.ClickTab(1);
			UpdateGUI(true);

		}
		public void GoSleep()
		{
			CloseWindow();
			GameManager.Save();
			GameManager.NextCombat();
		}
		public void ViewDefault()
		{
			UIAnimationTools.SetCanvasGroupActive(_components._misc, false);
			UIAnimationTools.SetCanvasGroupActive(_components._mind, true);
			_components._effectsTalentsTab.ClickTab(0);

		}
		public void ButtonPress(string buttonString)
		{
			string str = buttonString.ToLower();
			switch (str)
			{
				default:
					Debug.Log("Unknown button string " + str);
					break;
				case "hand":
					_combatGUI.ViewDeck(_cc._deck._mind, "Hand", true);
					break;
				case "deck":
					_combatGUI.ViewDeck(_cc._deck._subcon, "Draw Deck", true);
					break;
				case "discard":
					_combatGUI.ViewDeck(_cc._deck._forget, "Discard Pile", true);
					break;
				case "+str":
				case "+dex":
				case "+int":
				case "+ten":
					_cc._attributes.AddToAttribute(buttonString.Substring(1));
					UpdateGUI();
					break;
				case "inventory":
					GameManager._current._guiParts._guiInventory.DisplayInventory(_cc._inventory);
					break;
				case "exit":
					CloseWindow();
					break;
			}
		}
	}
}
