using System;
using System.Collections;
using System.Collections.Generic;
using RD.DB;
using RD;
using TMPro;
using UnityEngine;
using static RD.Combat.CombatManager;
using static RD.CodeTools;

namespace RD.Combat
{
	/// <summary>
	/// Handles targeting correct targets with the played card.
	/// </summary>
	public class TargetingSystem : MonoBehaviour
	{
		public static TargetingSystem _current;
		[NonSerialized] public int ready = 0;           //-1 = cancel, 1 = ready, 0 = not ready (still deciding targets)
		public TextMeshProUGUI _textPrompt;
		public CombatCharacter _cc { get; private set; }
		List<CombatCharacter> _targets;
		List<CombatCharacter> _targetsViable;
		public List<TMP_ColorGradient> _colorGradients;
		public Card _card { get; private set; }
		public Card _playedCard { get; private set; }
		int _highlightLayer;
		int _defaultCharacterLayer;
		CombatGUICard _cardAttack;
		CombatGUICard _cardMove;
		[SerializeField] RectTransform _cardAttackPosition;
		[SerializeField] RectTransform _cardMovePosition;
		CombatCharacter _owner;
		bool dbug = false;
        private void Awake()
        {
			_current = this;
			_highlightLayer = LayerMask.NameToLayer("Highlight");
			_defaultCharacterLayer = LayerMask.NameToLayer("Character");
			Toggle(false);
        }
		void Toggle(bool enabled, bool instant = true)
		{
			gameObject.SetActive(enabled);
			if (instant) UIAnimationTools.SetCanvasGroupActive(GetComponent<CanvasGroup>(), enabled);
			else
				StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(GetComponent<CanvasGroup>(), enabled, 5, true));
		}
        void InitCards(CombatCharacter source, Card card)
		{
			_cardAttack = Instantiate(CombatGUIHand._current._combatGUICardPrefab, _cardAttackPosition.transform.parent);
			RectTransform rt = _cardAttack.GetComponent<RectTransform>();
			rt.pivot = _cardAttackPosition.pivot;
			rt.anchorMax = _cardAttackPosition.anchorMax;
			rt.anchorMin = _cardAttackPosition.anchorMin;
			rt.anchoredPosition = _cardAttackPosition.anchoredPosition;
			_cardAttack.InitCardForTargeting((BaseCard)CodeTools.db.Get<BaseCard>(17), source);
			//_cardAttack._card.CombineEffects(card._effectsMelee, true);

			_cardMove = Instantiate(CombatGUIHand._current._combatGUICardPrefab, _cardMovePosition.transform.parent);
			rt = _cardMove.GetComponent<RectTransform>();
			rt.pivot = _cardMovePosition.pivot;
			rt.anchorMax = _cardMovePosition.anchorMax;
			rt.anchorMin = _cardMovePosition.anchorMin;
			rt.anchoredPosition = _cardMovePosition.anchoredPosition;
			_cardMove.InitCardForTargeting((BaseCard)CodeTools.db.Get<BaseCard>(18), source);
			//_cardMove._card.CombineEffects(card._effectsManeuver, true);
		}
		public void SetCardSelection(CombatGUICard card)
		{
			_playedCard._gUICard.ToggleTargetSelection(_playedCard._gUICard == card);
			_cardAttack.ToggleTargetSelection(_cardAttack == card);
			_cardMove.ToggleTargetSelection(_cardMove == card);
			_card = card._card;
			HighlightTargets(false);
			UpdateCharacterTargeting();
		}
		[Serializable]
		struct Title
		{
			[SerializeField] public string txt;
			[SerializeField] public float width;
		}
		public void Escape()
		{
			if (ready == 0)
			{
				Cancel();
			}
		}
		//Tooltip _tooltip;
		public void InitializeTargeting(Card card)
		{
			//
			_card = card;
			_playedCard = card;
			Toggle(true, false);
			_owner = card.GetOwner();
			ready = 0;
			_cc = card.GetOwner();

			_targets = new List<CombatCharacter>();
			_targetsViable = new List<CombatCharacter>();
			_multiTargetObjects = new List<CombatCharacter>();
			if (_card._specialActionCard)
			{
				TooltipSystem.LockTooltip(_card, _combatGUI._comps._cardPlayZoom.anchoredPosition,new Vector2(0.5f,1f));
				//TooltipSystem.LockTooltip(_card, GetRecttransformPivotPoint(_combatGUI._comps._cardPlayZoom, new Vector2(0.5f,0.5f), true));
			}
			//if targeting characters
			UpdateCharacterTargeting();
			//Else target cards in hand?
		}
		#region CombatCharacterSelection

		void UpdateCharacterTargeting()
		{
			//Debug.Log("<color=green>UpdateCharacterTargeting</color>");
			_combatGUI._comps._basicCG.blocksRaycasts = false;
			_combatGUI._comps._basicCG.interactable = false;
			bool self = _card._target.HasFlag(BaseCard.Target.Self);
			bool ally = _card._target.HasFlag(BaseCard.Target.Friend);
			bool enemy = _card._target.HasFlag(BaseCard.Target.Enemy);
			bool empty = _card._target.HasFlag(BaseCard.Target.Ground);
			bool obj = _card._target.HasFlag(BaseCard.Target.Object);
			ArenaManager.ClearGhosts();
			CreateTitleFromCard();
			if (self)
				_targetsViable.Add(_cc);
			//List<CombatTerrain> _spaces = ArenaManager.GetSpacesInRange(_cc, _card._range, _card._requireLineOfSight);
			List<CombatTerrain> _spaces = _card._meleeRange ? ArenaManager.GetSpacesInMeleeRange(_cc) : ArenaManager.GetTerrains();
			if (dbug) Debug.Log("spaces in range = " + _spaces.Count);
			for (int i = _spaces.Count - 1; i >= 0; i--)
			{
				CombatTerrain spc = _spaces[i];
				//remove unviable spaces
				if (spc.IsEmpty() && !empty)
				{   //if empty and can't target empty
					if (dbug) Debug.Log("Removed because can't target empty");
					_spaces.Remove(spc);
				}
				else if (!spc.IsEmpty() && !(self || ally || enemy || obj))
				{   //if not empty and can only target empty
					if (dbug) Debug.Log("Removed because can only target empty");
					_spaces.Remove(spc);
				}
				else if (spc.ContainsCharacter())
				{
					CombatCharacter cc = spc.GetCharacter();
					if (cc == _cc && !self)
					{          //if self and can't target self
						if (dbug) Debug.Log("Removed self");
						_spaces.Remove(spc);
					}
					else if ((cc._alliance == CombatCharacter.Alliance.Friendly || cc._alliance == CombatCharacter.Alliance.Player) && !ally)
					{       //if ally and can't target ally
						if (dbug) Debug.Log("Removed ally");
						_spaces.Remove(spc);
					}
					else if (cc._alliance == CombatCharacter.Alliance.Enemy && !enemy)
					{                //if enemy and can't target enemy
						if (dbug) Debug.Log("Removed Enemy");
						_spaces.Remove(spc);
					}
				}
				else if (!obj)
				{
					if (dbug) Debug.Log("Removes debris");  //space is debris and can't target obj
					_spaces.Remove(spc);
				}
			}
			if (dbug) Debug.Log("Viable Targets = " + _spaces.Count);
			foreach (CombatTerrain spc in _spaces)
			{
				//Debug.Log(spc.Print());
				if (spc.ContainsCharacter() && !_targetsViable.Contains(spc.GetCharacter()))
				{
					_targetsViable.Add(spc.GetCharacter());
				}
				else if (!spc.ContainsCharacter())
				{
					spc.CreateGhost(_cc._cSprite._spriteRendererCharacter);
					_targetsViable.Add(spc._ghostInstance.GetComponent<CombatCharacter>());
				}

			}

			if  (_card._autoPlay)
			{
				if (_card._multiTarget == BaseCard.MultiTargetType.All)
				{
					TargetAll();
				}
				else if (_card._randomTarget)
				{
					TargetRandom();
					return;
				}
				else if (_card.TargetOnlySelf())
				{
					TargetSelf();
					return;
				}
			}
			HighlightTargets(true);
			_combatManager.LimitSelection(_targetsViable, false);
			void TargetRandom()
			{
				if (_targetsViable.Count == 0)
				{
					Debug.Log("No Viable targets for " + _card._name);
					return;
				}
				//Get a random target from the viable targets
				int random = UnityEngine.Random.Range(0, _targetsViable.Count);
				TargetObject(_targetsViable[random]);

			}
			void TargetSelf()
			{
				TargetObject(_owner);
			}
		}
		public void TargetAll()
		{
			_targets.AddRange(_targetsViable);
			ready = 1;
			ExitTargeting();
		}
		public bool TargetObject(CombatCharacter co)
		{
			if (_targetsViable.Contains(co))
			{
				_targets.Add(co);
				if (_multiTargetObjects != null) _targets.AddRange(_multiTargetObjects);
				ready = 1;
				ExitTargeting();
				return true;
			}
			else return false;
		}
		bool CheckTargetViability(CombatCharacter source, CombatCharacter target, BaseCard bc)
        {
			BaseCard.Target t = bc._target;
			bool dbug = bc._debugLog;
			bool self = t.HasFlag(BaseCard.Target.Self);
			bool ally = t.HasFlag(BaseCard.Target.Friend);
			bool enemy = t.HasFlag(BaseCard.Target.Enemy);
			//Debug.Log(source.GetName() + " vs "+obj.GetName());
			//remove unviable spaces
			bool hostile = source.IsHostileTowards(target);
			bool viable = true;
			if (target == source)
			{
				viable = self;
			}
			else if (!hostile)
			{
				viable = ally;
			}
			else if (hostile)
			{
				viable = enemy;
			}

			if (dbug) Debug.Log("("+bc.GetFileName() +") where "+source.GetName()+ " is targeting " +target.GetName()+" is "+(viable?"<color=olive>VIABLE":"<color=red>UNVIABLE")+"</color>");
			return viable;
		}
		public bool CheckIfCardPlayableOnTarget(CombatCharacter target, Card c)
		{
			CombatCharacter owner = c.GetOwner();
			if (owner == null)
			{
				Debug.LogError(c._name + " has no owner.");
				return false;
			}
			if (owner._selectObject == null) return false;
			else if (!CheckTargetViability(owner,target, c)) {
				Debug.Log("<color=red>" + c._name + " can't target "+target.GetName()+"</color>");
				return false;
			}
			if (owner == target) return true;
			bool distance = CheckDistanceViable(c);
			return distance;
			bool CheckDistanceViable(Card c)
			{
				List<CombatTerrain> _spaces = c._meleeRange ? ArenaManager.GetSpacesInMeleeRange(_owner) : ArenaManager.GetTerrains();
				//List<CombatTerrain> _spaces = ArenaManager.GetSpacesInRange(owner, c._range, c._requireLineOfSight);
				foreach(CombatTerrain ct in _spaces)
				{
					if (ct.ContainsCharacter() && ct.GetCharacter() == target)
						return true;
				}
				//Debug.Log("Unviable Range");
				return false;
			}
		}
		public List<CombatCharacter> GetValidTargetsForCard(Card c)
		{
			CombatCharacter owner = c.GetOwner();
			if (owner == null)
			{
				Debug.LogError(c._name + " has no owner.");
				return null;
			}

			List<CombatCharacter> ccs = new List<CombatCharacter>();
			if (!c._meleeRange)
				ccs.AddRange(_combatManager._combatCharacters);
			else {
				foreach(CombatTerrain ct in CombatManager.ArenaManager.GetSpacesInMeleeRange(owner))
				{
					if (ct.ContainsCharacter())
						ccs.Add(ct.GetCharacter());
				}
			}
			for(int i = ccs.Count - 1; i >= 0; i--)
			{
				if (!CheckTargetViability(owner, ccs[i], c))
					ccs.RemoveAt(i);
			}
			return ccs;
		}
		void CreateTitleFromCard()
		{
			string title;
			if (_card.TargetOnlySelf())
			{
				title = "Target yourself";
			}
			else if (_card._randomTarget)
			{
				title = "Random target" + (_card._multiTarget > 0 ? "s" : "");
			}
			else
			{
				if (_card._multiTarget > 0)
					title = "Choose the targets";
				else
					title = "Choose a target";
			}
			SetTitle(title);
		}
		void SetTitle(string text)
		{
			if (text == "")
				_textPrompt.transform.parent.gameObject.SetActive(false);
			else
				_textPrompt.transform.parent.gameObject.SetActive(true);
			_textPrompt.text = text;
			_textPrompt.ForceMeshUpdate();
			RectTransform titleParent = _textPrompt.transform.parent.GetComponent<RectTransform>();
			titleParent.sizeDelta = new Vector2(_textPrompt.margin.x + _textPrompt.margin.z + _textPrompt.preferredWidth, titleParent.sizeDelta.y);
		}
		List<CombatCharacter> _multiTargetObjects;
		/// <summary>
		/// Handle Multitargeting for selected object
		/// </summary>
		public List<CombatCharacter> HandleMultiObjectTargeting(CombatCharacter cc, bool rightSide)
		{
			HighlightMultiTargetObjs(false);
			_multiTargetObjects = new List<CombatCharacter>();
			
            switch (_card._multiTarget)
            {
				default:
					break;
				case BaseCard.MultiTargetType.Two:
					AddTerrainToList(ArenaManager.GetAdjacentSpace(cc, rightSide));
					if (_multiTargetObjects == null || _multiTargetObjects.Count == 0)
					{
						AddTerrainToList(ArenaManager.GetAdjacentSpace(cc, !rightSide));
					}
					if(_multiTargetObjects.Count>1)
					_combatGUI._comps._healthBarManager.AddMultiTargetSymbol(cc, _multiTargetObjects[0]);
					//Create & symbol between them
					break;
				case BaseCard.MultiTargetType.Three:
					//Get both left and right characters
					AddTerrainToList(ArenaManager.GetAdjacentSpace(cc, true));
					AddTerrainToList(ArenaManager.GetAdjacentSpace(cc, false));
					_combatGUI._comps._healthBarManager.AddMultiTargetSymbol(cc, _multiTargetObjects[0]);
					_combatGUI._comps._healthBarManager.AddMultiTargetSymbol(cc, _multiTargetObjects[1]);
					//Create & symbol on both left and right side
					break;
				case BaseCard.MultiTargetType.All:
					_multiTargetObjects.AddRange(_targetsViable);
					_multiTargetObjects.Remove(cc);
					//& symbol for adjacents?
					break;
			}
			HighlightMultiTargetObjs(true);
			return _multiTargetObjects;

			void AddTerrainToList(CombatTerrain ct)
            {
				if (ct == null)
					return;
                if (ct.ContainsCharacter())
                {
					CombatCharacter selcc = ct.GetCharacter();
					if (selcc && CheckTargetViability(_cc,ct.GetCharacter(),_card))
						_multiTargetObjects.Add(selcc);
                }
            }
		}
		public void HighlightMultiTargetObjs(bool enabled)
		{
            
			if (_multiTargetObjects != null && _multiTargetObjects.Count > 0)
			{
				foreach (CombatCharacter cso in _multiTargetObjects)
				{
					if ((!enabled && !_targetsViable.Contains(cso)) || enabled)
					{
						HighlightCharacter(cso, enabled);
					}
				}
				if (!enabled)
				{
					_multiTargetObjects.Clear();
					_combatGUI._comps._healthBarManager.ClearMultiTargetSymbols();
				}
			}
		}
		public void HighlightTargets(bool highlight)
		{

			foreach (CombatCharacter co in _targetsViable)
			{
				if (!_targets.Contains(co))
					HighlightCharacter(co, highlight);
			}
			if (!_targetsViable.Contains(_owner) && highlight)
				HighlightCharacter(_owner, highlight, true);

		}
		void HighlightCharacter(CombatCharacter cc, bool highlight = true, bool self = false)
		{
			//Debug.Log((highlight?"":"Un")+"Highlighting " + co.GetName());
			int layer = highlight ? _highlightLayer : _defaultCharacterLayer;
			if (cc == null)
				return;
			cc.gameObject.layer = layer;
			if (!highlight)
				cc._selectObject.NormalizeScale();
			if (cc)
			{
				if (!self) cc._cSprite._spriteRendererHighlight.gameObject.SetActive(highlight);
				cc._cSprite._spriteRendererCharacter.gameObject.layer = layer;
			}
			else
			{
				foreach (Transform t in cc.transform)
					t.gameObject.layer = layer;
			}
		}
		void ExitTargeting()
		{
			//Debug.Log("<color=purple>ExitTargeting()</color>");
			ArenaManager.ClearGhosts();
			_combatGUI._comps._basicCG.blocksRaycasts = true;
			_combatGUI._comps._basicCG.interactable = true;
			HighlightTargets(false);
			_combatGUI._comps._healthBarManager.ClearMultiTargetSymbols();
			HighlightCharacter(_owner, ready == 1 ? true : false, true);
			_combatManager.LimitSelection(_targetsViable, true);
			_combatGUI._state = CombatGUI.StateOfGUI.Free;
			ShowCard();
			ClearCards(ready);
			Toggle(false);
			TooltipSystem.UnlockTooltip();
			void ShowCard()
            {
				if (ready != 1)
					return;
				_tempCard = null;
				if (_card != _playedCard) {
					//Debug.Log("Hiding "+ _playedCard._gUICard.gameObject.name);
					UIAnimationTools.TotalToggleCanvasGroup(_playedCard._gUICard.GetComponent<CanvasGroup>(), false);
					CombatGUICard go = Instantiate(_card._gUICard.gameObject,CombatGUIHand._current.transform).GetComponent<CombatGUICard>();
					go.transform.position = _card._gUICard.transform.position;
					go.transform.parent = _combatGUI._comps._highlight;
					_tempCard = go.gameObject;
				}
                else
                {
					if(_playedCard._gUICard)
					_playedCard._gUICard.transform.parent = _combatGUI._comps._highlight;
                }
            }
		}
		GameObject _tempCard;
		public void Clear()
        {
			if (_tempCard)
			{
				GUIAnimationManager.AddDestroyGameObject(_tempCard, 0);
				_tempCard = null;
				GUIAnimationManager.AddAnimation(ShowPlayedCard(), "ShowPlayedCard");
			}
			ClearCards(0);
		}
		void ClearCards(int rd)
		{
			if (rd != 1)
			{
				DeleteCard(_cardMove);
				DeleteCard(_cardAttack);
				_cardMove = null;
				_cardAttack = null;
			}
			else
			{
				if (_card._gUICard != _cardMove)
				{
					DeleteCard(_cardMove);
					_cardMove = null;
				}
				if (_card._gUICard != _cardAttack)
				{
					DeleteCard(_cardAttack);
					_cardAttack = null;
				}
			}
			void DeleteCard(CombatGUICard cgc)
			{
				if (cgc != null)
				{
					DestroyImmediate(cgc.gameObject);
				}
			}
		}
		IEnumerator ShowPlayedCard()
        {

			UIAnimationTools.TotalToggleCanvasGroup(_playedCard._gUICard.GetComponent<CanvasGroup>(), true);
			yield return null;
		}
		public List<Card> GetCardsToDiscard()
        {
			Clear();
			return new List<Card> { _playedCard };
		}
		public List<CombatCharacter> GetTargets()
		{
			//Debug.Log("TargetCount = " + _targets.Count);
			//foreach (CombatCharacter cc in _targets)Debug.Log(cc.GetName());
			return _targets;
		}
		public bool CheckObjectValid(CombatSelectableObject co)
		{
			if(co._cc)
				return _targetsViable.Contains(co._cc);
			return false;
		}
		public bool CheckObjectValid(CombatCharacter cc)
		{
			return _targetsViable.Contains(cc);
		}

		void Cancel()
		{
			ready = -1;
			HighlightMultiTargetObjs(false);
			ExitTargeting();
		}
		public void UnhighlightTargets()
		{
			foreach (CombatCharacter cc in _targets)
			{
				HighlightCharacter(cc, false);
				//Debug.Log(cc._name);
			}
			foreach (CombatCharacter cc in _targetsViable)
			{
				HighlightCharacter(cc, false);
			}
			HighlightCharacter(_owner, false, true);
		}
		public void UpdateCardDamageOnTarget(CombatCharacter target = null)
		{
			if (_card._damage != null)
			{
				if (target != null)
				{
					target.prepareIncomingDamageTask = new Task(target.PrepareIncomingDamage(_card._damage));
				}
				else
				{
					_card._damage.RemoveTarget();
				}
			}
		}
		#endregion

		#region CardSelection
		CardTargetingTool crt;

		public IEnumerator InitializeHandTargeting(int count, string verb)
		{
			ready = 0;
			Toggle(true);
			_cardAttack.transform.parent.gameObject.SetActive(false);
			CombatGUIHand hand = _combatGUI._comps._GUIHand;
			string txt = verb + " " + count + " cards";
			SetTitle(txt);
			hand.HighlightHand();
			crt = new CardTargetingTool(hand);
			Task t = new Task(crt.CardTargeting(count));
			while (t.Running)
				yield return null;
			hand.StartCoroutine(hand.LowlightHand());
			ready = 1;
			Toggle(false);

		}
		public List<CombatGUICard> GetSelectedCards()
		{
			return crt.targetedCards;
		}
		public bool SelectCard(CombatGUICard cgcard)
		{
			if (crt.targetedCards.Contains(cgcard))
			{
				crt.targetedCards.Remove(cgcard);
				return false;
			}
			else
			{
				crt.targetedCards.Add(cgcard);
				return true;
			}
		}

		public class CardTargetingTool
		{
			List<CombatGUICard> _cards;
			public List<CombatGUICard> targetedCards = new List<CombatGUICard>();
			public IEnumerator CardTargeting(int targetCount)
			{
				targetedCards = new List<CombatGUICard>();
				if (targetCount >= _cards.Count)
				{
					foreach (CombatGUICard c in _cards)
						targetedCards.Add(c);
				}
				else
				{
					while (targetedCards.Count < targetCount)
						yield return null;
				}

			}
			public bool ToggleCardSelect(CombatGUICard card)
			{
				if (targetedCards.Contains(card))
				{
					targetedCards.Remove(card);
					return false;
				}
				else
				{
					targetedCards.Add(card);
					return true;
				}
			}
			public CardTargetingTool(CombatGUIHand hand)
			{
				_cards = new List<CombatGUICard>();
				foreach (CombatGUICard card in hand._cards)
					_cards.Add(card);
			}
			public CardTargetingTool(List<CombatGUICard> cards)
			{
				_cards = new List<CombatGUICard>();
				foreach (CombatGUICard card in cards)
					_cards.Add(card);
			}
		}
		#endregion

		#region AISettings
		public void DisplayAITargeting(Card c,List<CombatCharacter> targets)
		{
			if (c.GetOwner() == null)
				return;
			//HideHand
			_cc = c.GetOwner();
			_owner = c.GetOwner();
			_targets = targets;
			_targetsViable = targets;
			_playedCard = c;
			c._gUICard.UpdateTexts(targets[0]);
			//Toggle(true);
			foreach (CombatCharacter co in targets)
				HighlightCharacter(co, true);
			HighlightCharacter(_cc, true, true);
			//HighlightTargets(true);
		}
		#endregion
	}
}
