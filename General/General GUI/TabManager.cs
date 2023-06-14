using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RD;
namespace RD.UI
{
	public class TabManager : MonoBehaviour
	{
		[Serializable]
		public class Tab
		{
			public bool _open = false;
			public Button _button;
			public CanvasGroup _contents;
			public void Activate(TabManager tm)
			{
				_open = true;
				_button.interactable = false;
				if (!tm)
					tm = _button.GetComponentInParent<TabManager>();
				if (tm._recolorText)
				{
					foreach (Text t in _button.GetComponentsInChildren<Text>())
						t.color = tm._defaultColorText;
					foreach (TextMeshProUGUI t in _button.GetComponentsInChildren<TextMeshProUGUI>())
						t.color = tm._defaultColorText;
				}
				if (tm._recolorButton)
				{
					if (tm._recolorButton && _button.GetComponent<Image>())
					{
						_button.GetComponent<Image>().color = tm._defaultColorButton;
					}
				}

			}
			public void Disable(TabManager tm)
			{
				_open = false;
				_button.interactable = true;
				if (tm._recolorButton && _button.GetComponent<Image>())
				{
					_button.GetComponent<Image>().color = tm._disabledColor;

				}
				if (tm._recolorText)
				{
					foreach (Text t in _button.GetComponentsInChildren<Text>())
						t.color = tm._disabledColor;
					foreach (TextMeshProUGUI t in _button.GetComponentsInChildren<TextMeshProUGUI>())
						t.color = tm._disabledColor;
				}
			}
		}
		public List<Tab> _tabs;
		public Color _disabledColor = Color.gray;
		public Color _defaultColorButton = Color.white;
		public Color _defaultColorText = Color.white;
		public bool _recolorText;
		public bool _recolorButton;
		public bool _expandOpenTab = true;
		public bool _instantTransition = true;
		float _animationSpeed = 5;
		public bool _horizontal = true;
		public float _padding = 6;
		public float _spacing = 5;
		float _openSize = 0;
		float _closedSize = 0;
		RectTransform _rt;
		private void OnEnable()
		{
			_rt = GetComponent<RectTransform>();
			CalculateSizes();
		}
		public void ClickTab(Button b)
		{
			Tab clickedTab = null;
			foreach (Tab t in _tabs)
			{
				if (t._button == b)
				{
					clickedTab = t;
					break;
				}
			}
			if (clickedTab == null)
				return;
			if (clickedTab._open == false)
			{
				foreach (Tab t in _tabs)
					if (t != clickedTab)
						DisableTab(t);

				ActivateTab(clickedTab);
			}



			void DisableTab(Tab t)
			{
				if (t._open)
				{
					t.Disable(this);
					if (!_instantTransition)
						StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(t._contents, false, _animationSpeed, true));
					else
					{
						t._contents.gameObject.SetActive(false);
						UIAnimationTools.SetCanvasGroupActive(t._contents, false);
					}
					if (_expandOpenTab)
					{

						RectTransform rt = t._button.GetComponent<RectTransform>();
						Vector2 size = rt.sizeDelta;
						if (_horizontal)
							size.x = _closedSize;
						else
							size.y = _closedSize;
						rt.sizeDelta = size;
					}
				}
			}

			void ActivateTab(Tab t)
			{
				if (!t._open)
				{
					t.Activate(this);
					if (!_instantTransition)
						StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(t._contents, true, _animationSpeed, true));
					else
					{
						t._contents.gameObject.SetActive(true);
						UIAnimationTools.SetCanvasGroupActive(t._contents, true);
					}
				}
				if (_expandOpenTab)
				{
					RectTransform rt = t._button.GetComponent<RectTransform>();
					Vector2 size = rt.sizeDelta;
					if (_horizontal)
						size.x = _openSize;
					else
						size.y = _openSize;
					rt.sizeDelta = size;
				}
			}
		}
		public void ClickTab(int index)
		{
			if (index < _tabs.Count)
				ClickTab(_tabs[index]._button);
		}
		void CalculateSizes()
		{
			float totalSpace = (_horizontal ? _rt.rect.width : _rt.rect.height) - _padding*2;
			float evenSize = (totalSpace - (_tabs.Count - 1 * _spacing)) / _tabs.Count;
			if (_expandOpenTab)
			{
				_openSize = evenSize * 1.33f;
				totalSpace -= _openSize;
				_closedSize = (totalSpace - (_tabs.Count - 2 * _spacing)) / (_tabs.Count-1);
			}
			else
				_closedSize = _openSize = evenSize;
		}
	}
}
