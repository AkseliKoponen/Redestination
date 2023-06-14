using RD.DB;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RD.CodeTools;

namespace RD.UI
{
	public class MenuCanvas : MonoBehaviour
	{
		[SerializeField] RectTransform _buttonParent;
		[SerializeField] MenuButton _buttonprefab;
		[SerializeField] RectTransform _backgroundImage;
		[SerializeField] CanvasGroup _childCanvasGroup;
		List<MenuButton> _buttons;
		[SerializeField] MenuState _menuState;
		public enum MenuState {None, MainMenu, Pause, Options, AfterCombat, CombatChoice}
		CanvasGroup _cg;
		private void Awake()
		{
			_buttons = new List<MenuButton>();
			_cg = GetComponent<CanvasGroup>();
			_buttonParent.GetComponent<CanvasGroup>().alpha = 0;
			if(_menuState == MenuState.MainMenu)
				StartCoroutine(FadeMenu(CreateDefaultButtons));
		}
		public void CreateMenu(MenuState ms)
		{
			_menuState = ms;
			gameObject.SetActive(true);
			StartCoroutine(FadeMenu(CreateDefaultButtons));
		}
		void ClearOldButtons()
		{
			if (_buttons.Count > 0)
			{
				foreach(MenuButton bu in _buttons)
				{
					Destroy(bu.gameObject);
				}
				_buttons.Clear();
			}
		}
		string CreateDefaultButtons(string s="")
		{
			ClearOldButtons();
			switch (_menuState)
			{
				default:
					Debug.Log("Unassigned menustate");
					break;
				case MenuState.MainMenu:
					CreateButton("Begin");
					CreateButton("Dialog");
					CreateButton("Combat");
					CreateButton("Map");
					CreateButton("Movement");
					CreateButton("Options");
					break;
				case MenuState.Options:
					CreateButton("Back");
					CreateButton("Brains");
					CreateButton("Eyes");
					CreateButton("Ears");
					CreateButton("Hands");
					break;
				case MenuState.AfterCombat:
					if(GetComponent<TestTransition>() && GetComponent<TestTransition>()._battlesSinceCamp>=3)
						CreateButton("Camp", true);
					else
						CreateButton("Camp", false);
					CreateButton("Next Combat");
					break;
				case MenuState.CombatChoice:
					for(int i = 0; i < 10; i++)
					{
						CreateButton("Fight lvl " + (i + 1));
					}
					break;
			}
			StartCoroutine(MarkForRefresh());
			return s;
			//RefreshLayout();
		}
		IEnumerator FadeMenu(System.Func<string,string> MethodName)
		{
			ToggleInteractions(false);
			Task t = new Task(UIAnimationTools.FadeCanvasGroupAlpha(_buttonParent.GetComponent<CanvasGroup>(), false, 6));
			while (t.Running)
			{
				yield return null;
			}
			MethodName("");
			float frames = 0.15f;
			while (frames > 0)
			{
				frames-= UnityEngine.Time.deltaTime;
				yield return null;
			}
			ToggleInteractions(true);
			t = new Task(UIAnimationTools.FadeCanvasGroupAlpha(_buttonParent.GetComponent<CanvasGroup>(), true, 6));
		}
		IEnumerator MarkForRefresh()
		{
			yield return new WaitForFixedUpdate();
			RefreshLayout();
		}
		void RefreshLayout()
		{
			VerticalLayoutGroup vert = _buttonParent.GetComponentInParent<VerticalLayoutGroup>();
			if (vert)
			{
				//vert.CalculateLayoutInputHorizontal();
				vert.CalculateLayoutInputVertical();
				//horizLayoutGroup.SetLayoutHorizontal();
				vert.SetLayoutVertical();
				RectTransform rt = vert.GetComponent<RectTransform>();
				LayoutRebuilder.MarkLayoutForRebuild(rt);
				LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
				vert.enabled = false;
				vert.enabled = true;
				//LayoutRebuilder.MarkLayoutForRebuild(rt);
			}
			//LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
			Canvas.ForceUpdateCanvases();
		}
		MenuButton CreateButton(string buttonName, bool enabled = true)
		{
			if (_buttonParent)
			{
				MenuButton bu = Instantiate(_buttonprefab, _buttonParent);
				bu.GetComponentInChildren<TextMeshProUGUI>().text = buttonName;
				bu.gameObject.name = buttonName;
				bu.GetComponent<Button>().onClick.AddListener(() => ButtonClick(bu.gameObject.name));
				bu.Toggle(enabled);
				//Debug.Log(bu.gameObject.name + "was assigned a listener");
				_buttons.Add(bu);
				return bu;
			}
			else {
				Debug.LogError("_menuParent unassigned");
				return null;
			}
		}
		public void Toggle(bool enabled)
		{
			ClearTooltips();
			gameObject.SetActive(enabled);
			if(!_cg)Awake();
			StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(_cg, enabled, 1));
			Tm.SetWorldTime(enabled?0:1);
		}

		public void ButtonClick(string btnName)
		{
			string s = btnName.ToLower();
			switch (s)
			{
				default:
					if(s.Contains("fight lvl "))
					{
						ClearOldButtons();
						string number = s.Substring("fight lvl ".Length);
						try
						{
							gameObject.SetActive(false);
							SceneConstructor scene = (SceneConstructor)db.Get<SceneConstructor>("Test Fight Level "+number);
							SceneHandler.EnterBaseScene(scene);
						}
						catch { }
					}
					else
						Debug.Log("Unknown button press");
					break;
				case "map":
					ToggleInteractions(false);
					SceneHandler.EnterScene("map", new List<string> { "world map" });
					//StartCoroutine(LoadScene("Map", false));
					//_state = MenuState.Map;
					break;
				case "combat":
					ToggleInteractions(false);
					SceneHandler.EnterScene("combat");
					//StartCoroutine(LoadScene("CombatScene", false));
					break;
				case "options":
					_menuState = MenuState.Options;
					StartCoroutine(FadeMenu(CreateDefaultButtons));
					break;
				case "back":
					switch (_menuState)
					{
						case MenuState.Options:
							_menuState = MenuState.MainMenu;
							StartCoroutine(FadeMenu(CreateDefaultButtons));
							break;
						default:
							Debug.Log("unassigned menustate");
							break;
					}
					break;
				case "begin":
					StartCoroutine(Begin());
					break;
				case "dialog":
					ToggleInteractions(false);
					SceneHandler.EnterScene("dialog");
					break;
				case "movement":
					ToggleInteractions(false);
					SceneHandler.EnterScene("movement", new List<string> {"dorosby" });
					break;
				case "camp":
					if (GetComponent<TestTransition>())
						GetComponent<TestTransition>().Camp();
					ClearOldButtons();
					break;
				case "next combat":
					CreateMenu(MenuState.CombatChoice);
					break;
			}
		}
		void ToggleInteractions(bool enable)
		{
			if (GetComponent<CanvasGroup>())
			{
				GetComponent<CanvasGroup>().interactable = enable;
				GetComponent<CanvasGroup>().blocksRaycasts = enable;
			}
			else
			{
				_childCanvasGroup.interactable = enable;
				_childCanvasGroup.blocksRaycasts = enable;
			}
		}
		public void ButtonMouseOver(RectTransform button)
		{
			string btnName = button.gameObject.name.ToLower();
			Vector3 tooltipPosition = GetRecttransformPivotPoint(button, new Vector2(1, 0.5f),true);
			switch (btnName)
			{
				default:
					Debug.Log("Unknown button '"+btnName+"'");
					break;
				case "map":
					TooltipSystem.DisplayTooltip((Hyperlink)db.Get<Hyperlink>(1), tooltipPosition);
					break;
			}
		}
		IEnumerator Begin()
		{
			CanvasGroup cg = _childCanvasGroup;
			float fadeTime = 0;
			if (cg)
			{
				fadeTime = 0.25f;
				cg.interactable = false;
				StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(cg, false, 1/fadeTime));
			}
			while (fadeTime > 0)
			{
				fadeTime -= Tm.GetUIDelta();
				yield return null;
			}
			if (_backgroundImage)
			{
				fadeTime = 1f;
				_backgroundImage.GetComponent<LerpTransform>().StartLerpScale(Vector3.one * 10f, 1f / fadeTime);
				//MusicManager.FadeToggle(false,1f/fadeTime);
			}
			while (fadeTime > 0)
			{
				fadeTime -= Tm.GetUIDelta();
				yield return null;
			}
			SceneHandler.EnterScene("new game");
		}
		public void ClearTooltips()
		{
			TooltipSystem.HideAllTooltips();
		}
	
	}
}
