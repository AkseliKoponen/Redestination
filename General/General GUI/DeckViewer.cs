using System.Collections;
using System.Collections.Generic;
using RD.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RD.CodeTools;
namespace RD
{
	[RequireComponent(typeof(RectTransform))]
	public class DeckViewer : MonoBehaviour
	{
		[SerializeField] Vector2 _padding;
		[SerializeField] Vector2 _spacing;
		[SerializeField] bool _automaticCardSize;
		[SerializeField] Vector2 _cardSize;
		[SerializeField] int _columnlimit;
		[SerializeField] TextMeshProUGUI _title;
		[SerializeField] CombatGUICard _cardPrefab;
		[SerializeField] Canvas _canvas;
		[SerializeField] Button _closeButton;
		public float _scrollSensitivity = 10f;
		RectTransform _parentRT;
		RectTransform _rt;
		List<CombatGUICard> _cards;
		// Update is called once per frame
		void Update()
		{
			Refresh(false);
		}
		public void InputScroll(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (rowCount > 3)
			{
				Vector2 scrollValue = cbt.ReadValue<Vector2>();
				//Debug.Log(scrollValue);
				float scroll = scrollValue.y / 360f;
				if (Mathf.Abs(scroll) > 0)
				{
					if (!_rt) _rt = GetComponent<RectTransform>();
					float maxy = miny + ((rowCount - 3) * (_cardSize.y + _spacing.y) * 0.5f);
					_rt.anchoredPosition = new Vector2(_rt.anchoredPosition.x, Mathf.Clamp(_rt.anchoredPosition.y + scroll * _scrollSensitivity, miny, maxy));
				}
			}
		}
		int rowCount = 0;
		public void Refresh(bool editor = false)
		{
			if (editor && Application.isPlaying)
				return;
			if (!_parentRT)
				Init();
			if (editor)
			{
				Init(editor);
				_cards = new List<CombatGUICard>();
				foreach(CombatGUICard card in GetComponentsInChildren<CombatGUICard>())
				{
					_cards.Add(card);
				}
			}
			UpdateParentSize();
			float width = _parentRT.rect.width - (_padding.x*2);
			float height = _parentRT.rect.height - (_padding.y*2);
			float distance = _cardSize.x + _spacing.x;
			int columnLimit = Mathf.Clamp((int)(width / distance),0,_columnlimit>0?_columnlimit:99);
			float startPosX = (columnLimit - 1) / -2f * distance;// transform.localPosition.x - width / 2 + _cardSize.x / 2;
			float startPosY = transform.localPosition.y + height / 2 - _cardSize.y / 2;
			int row = -1;
			int column = 0;
			foreach (CombatGUICard card in _cards)
			{
				if (column == 0)
					row++;
				_cardSize.y = Mathf.Clamp(card.GetTotalHeight(), _cardSize.y, card.GetTotalHeight());
				card.transform.localPosition = GetPos();
				column++;
				if (column>=columnLimit)
				{
					column = 0;
				}
			}
			rowCount = row + 1;
			Vector2 GetPos()
			{
				return new Vector2(startPosX + (column * distance), startPosY - (row * (_cardSize.y + _spacing.y)));
			}

		}
		float miny;
		private void Init(bool editor = false)
		{
			_parentRT = transform.parent.GetComponent<RectTransform>();
			_rt = GetComponent<RectTransform>();
			if(!editor && _cards!=null && _cards.Count > 0)
				ClearCards();
			_cards = new List<CombatGUICard>();
			miny = _rt.anchoredPosition.y;
			UpdateParentSize();
		}
		void UpdateParentSize()
		{
			Vector2 CanvasSize = _parentRT.parent.GetComponent<RectTransform>().sizeDelta;
			_parentRT.sizeDelta = new Vector2(CanvasSize.x > 0 ? CanvasSize.x * 0.67f : _parentRT.sizeDelta.x, CanvasSize.y > 0 ? CanvasSize.y : _parentRT.sizeDelta.y);
		}

		public void ViewDeck(Deck deck,string title = "Deck", int targetCount = 0)
		{
			Init();
			_canvas.gameObject.SetActive(true);
			bool selection = targetCount > 0;
			foreach (Card c in deck._ideas)
			{
				CombatGUICard cgc = Instantiate(_cardPrefab, transform);
				cgc.DisplayCardForDeckView(c,deck._cc,selection);
				_cards.Add(cgc);
			}
			_title.text = title;
			_title.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(_title.preferredWidth + 30, _title.transform.parent.GetComponent<RectTransform>().sizeDelta.y);
			Refresh();
			_closeButton.gameObject.SetActive(!selection);
		}
		public void Hide(bool destroyChildren = true)
		{
			if (destroyChildren) ClearCards();
			_rt.anchoredPosition = new Vector2(_rt.anchoredPosition.x, miny);
			_canvas.gameObject.SetActive(false);
		}

		void ClearCards()
		{

			foreach (CombatGUICard card in _cards)
			{
				Destroy(card.gameObject);
			}
			_cards.Clear();
		}
		TargetingSystem.CardTargetingTool ctt;
		public TargetingSystem.CardTargetingTool InitializeDeckTargeting(int count,Deck d)
		{
			_combatGUI._comps._targetingSystem.UnhighlightTargets();
			ViewDeck(d, "Select " + count + " card"+ Plural(count), count);
			ctt = new TargetingSystem.CardTargetingTool(_cards);
			Task t = new Task(ctt.CardTargeting(count));
			StartCoroutine(DeckTargeting(t));
			return ctt;
		}
		IEnumerator DeckTargeting(Task t)
		{
			while (t.Running)
				yield return null;
			_canvas.gameObject.SetActive(false);
		}
		public bool SelectCard(CombatGUICard card)
		{
			return ctt.ToggleCardSelect(card);
		}
	}
}
