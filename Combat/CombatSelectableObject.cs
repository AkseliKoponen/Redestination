using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static RD.CodeTools;

namespace RD.Combat
{
	public class CombatSelectableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		[NonSerialized] public CombatCharacter _cc;
		public Transform _spriteT { get; private set; }
		public GameObject _selectionCircle;
		public bool _selected { get; private set; }
		public bool _highlighted { get; private set; }
		public float _highlightLerpScale = 1.5f;
		public bool enableInteraction = true;
		[NonSerialized] public bool lerping = false;
		public bool _ghost = false;
		Vector3 _scaleDefault;
		public void Awaken()
		{
		}
		public void Start()
		{
			_spriteT = GetComponentInChildren<SpriteRenderer>().transform;
			_scaleDefault = _spriteT.localScale;
			if (transform.parent.GetComponent<CombatCharacter>())
			{
				_cc = transform.parent.GetComponent<CombatCharacter>();
				_cc._selectObject = this;
			}
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			_pointerOver = true;
			ToggleText();
			if (!_highlighted && Allow()){
				Highlight(); 
			}
		}
		void ToggleText()
		{
			if(_cc._cSprite._statusBar != null)
				_cc._cSprite._statusBar.ToggleText(_pointerOver);
		}
		public void Highlight()
		{
			//Debug.Log("Attempting to highlight " + (_cc ? _cc.GetName() : "something"));
			bool cursorIsOnTheRightSide = GetCursorXInRelationToSelf();
			if (_combatManager)
				if (!_combatManager.OnHighlight(_cc,cursorIsOnTheRightSide))
					return;
			_highlighted = true;
			StartCoroutine(UpdateSide());
			_spriteT.localScale = _scaleDefault * _highlightLerpScale;
			//Debug.Log("Highlighting " + (_cc?_cc.GetName():"something"));
			if (!_ghost && _combatGUI._state == CombatGUI.StateOfGUI.Targeting)
			{
				_combatGUI.UpdateCardDamage(_cc);
			}
			IEnumerator UpdateSide()
			{
				while (_highlighted && _combatGUI._state == CombatGUI.StateOfGUI.Targeting)
				{

					//_combatManager.OnHighlight(_cc, GetCursorXInRelationToSelf());
					_combatManager.UpdateTargetSide(_cc, GetCursorXInRelationToSelf());
					yield return null;
				}
			}
		}
		/// <summary>
		/// Return true if cursor is on the right of the center, false if left
		/// </summary>
		public bool GetCursorXInRelationToSelf()
		{
			Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			//Debug.Log(cursorPosition.x > transform.position.x ? "Right" : "Left");
			if (cursorPosition.x > transform.position.x)
				return true;
			else
				return false;
		}
		bool _pointerOver = false;
		public void OnPointerExit(PointerEventData eventData)
		{
			_pointerOver = false;
			ToggleText();
			if (_highlighted && Allow()) _combatManager.UnHighlight();
		}
		public void UnHighlight()
		{
			ToggleText();
			if (lerping)
			{
				if (_unhighlightAfterLerpEndsTask == null || _unhighlightAfterLerpEndsTask.Running == false)
					_unhighlightAfterLerpEndsTask = new Task(UnhighlightAfterLerpEnds());
				return;
			}
			if (_pointerOver)
				return;
			_highlighted = false;
			//if (_combatManager)_combatManager.UnHighlight();
			_spriteT.localScale = _scaleDefault;
			if (!_ghost && _combatGUI._state == CombatGUI.StateOfGUI.Targeting)
			{
				_combatGUI.UpdateCardDamage();
			}
			//Lerp Smaller
		}
		Task _unhighlightAfterLerpEndsTask;
		IEnumerator UnhighlightAfterLerpEnds()
		{
			while (lerping)
				yield return null;
			UnHighlight();
		}


		public void OnPointerClick(PointerEventData eventData)
		{
			//Debug.Log("select");
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				if (!_selected && Allow()) Select();
			}
			else if(eventData.button == PointerEventData.InputButton.Right)
			{
				_combatGUI.InspectCharacter(_cc);
				_combatManager.UnHighlight();
			}
		}
		public void Select()
		{

			if (_combatManager)
				_selected = _combatManager.SelectCharacter(_cc);
			else
				Debug.Log("Combat Manager does not exist??");
		}

	
		public void SelectionIndicator(bool enable = true)
		{
			_selectionCircle.SetActive(enable);
		}
		public void DeSelect()
		{
			_selected = false;
			_selectionCircle.SetActive(false);
		}

		bool Allow()
		{
			return enableInteraction;

			Debug.Log("lerping = " + lerping + "\nEnableInteraction = " + enableInteraction);
			if (!lerping && enableInteraction)
				return true;
			else
				return false;
		}


		public void NormalizeScale()
		{
			_spriteT.localScale = _scaleDefault;//GetNormalScale();
		}
		public Vector3 GetNormalScale()
		{
			return _highlighted ? _scaleDefault * _highlightLerpScale : _scaleDefault;
		}
	}
}
