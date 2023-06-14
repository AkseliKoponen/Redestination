using System;
using System.Collections;
using System.Collections.Generic;
using RD;
using UnityEngine;
using static RD.CodeTools;

namespace RD.Combat
{
	public class CombatGUIHand : MonoBehaviour
	{
		public static CombatGUIHand _current;
		public Deck _deck;
		public float maxCardSpace = 1400;   //edited in editor
		public float maxDistance = 300;     //edited in editor
		[SerializeField] public Transform _cardRotationPoint;
		[SerializeField] public RectTransform _cardInspectPosition;
		[NonSerialized]public float _cardYDefault = 0;
		public CombatGUICard _combatGUICardPrefab;
		public List<CombatGUICard> _cards;//{ get; private set; }
		CombatGUICard _highlightedCard;
		public AnimationCurve _handCurvature;
		public bool playable { private set; get; }
		public AnimationCurve _cardDiscardScale;
		[NonSerialized] public bool _blockInteraction = false;
		float _handDisplayY;
		float _handLoweredY = -800;
		CanvasGroup _canvasGroup;
        private void Awake()
		{
			_current = this;
			_cards = new List<CombatGUICard>();
			_handDisplayY = transform.localPosition.y;
			_canvasGroup = GetComponent<CanvasGroup>();
		}
		public float CreateHand(Deck d,bool cardsPlayable)
		{
			//Debug.Log("Create Hand of "+d._cc.GetName()+"!");
			float t = DisplayHand(true);
			if (_cards.Count > 0)
				ClearDeck();
			_deck = d;
			playable = cardsPlayable;
			for (int i = 0; i < d._ideas.Count; i++)
			{
				_cards.Add(CreateCard(_deck._ideas[i]));
			}
			UpdateHandPositions(0);
			return t;
		}

		public float DisplayHand(bool enabled)
        {
			if (!_canvasGroup.interactable)
			{
				Hide(true, 0);
			}
			float moveSpeed = 2;
			Vector3 from;
			Vector3 to;
			if (enabled)
			{
				from = new Vector3(0, _handLoweredY, 0);
				to = new Vector3(0, _handDisplayY, 0);
			}
            else
			{
				from = new Vector3(0, _handDisplayY, 0);
				to = new Vector3(0, _handLoweredY, 0);

			}
			transform.localPosition = from;
			GetComponent<LerpTransform>().StartLerpPosition(to, true, moveSpeed);
			if(_cards != null && _cards.Count > 1)
            {
				foreach(CombatGUICard cgc in _cards)
                {
					MoveCardTogether();
                }
            }
			return 1/moveSpeed;

			void MoveCardTogether()
            {

            }
		}
		public void Hide(bool hide, float time = 0.33f)
		{
			if (hide != _canvasGroup.interactable)
				return;
			//Debug.Log("<color=teal>"+(hide ? "Hiding " : "Displaying ") + "hand</color>");
			if (time > 0)
				StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(_canvasGroup, !hide, 1 / time, true));
			else
				UIAnimationTools.SetCanvasGroupActive(_canvasGroup, !hide);
		}
		public CombatGUICard CreateCard(Card card)
		{
			CombatGUICard CGC = Instantiate(_combatGUICardPrefab.gameObject, transform).GetComponent<CombatGUICard>();
			CGC.DisplayCardInHand(card);
			CGC.transform.localPosition = new Vector3(CGC.transform.localPosition.x, _cardYDefault);
			return CGC;
		}
		public IEnumerator AddCards(List<Card> cards, float animTime, Vector3 startPos)
		{
			List<CombatGUICard> CGCs = new List<CombatGUICard>();
			foreach(Card c in cards)
			{
				CombatGUICard CGC = Instantiate(_combatGUICardPrefab.gameObject, transform).GetComponent<CombatGUICard>();
				StartCoroutine(CGC.Draw(animTime));
				CGC.DisplayCardInHand(c);
				CGC.StartCoroutine(CGC.AddTrail());
				CGCs.Add(CGC);
			}
		
			ForcelowlightCards();
			SetHandTransition(true);
			UpdateHandPositions(animTime,_cards.Count+CGCs.Count);
			int index = 0+_cards.Count;
			_cards.AddRange(CGCs);
			foreach (CombatGUICard CGC in CGCs)
			{
				CGC.transform.position = startPos;
				CGC.transform.GetChild(0).localScale = Vector3.zero;
				CGC.transform.GetChild(0).GetComponent<LerpTransform>().StartLerpScale(Vector3.one, 1 / animTime);
				float rotz = GetCardRotations(index, false, _cards.Count);
				//Vector3 endPos = RotateAroundWithoutMoving(CGC.transform, _cardRotationPoint.position, rotz, CGC.transform.TransformPoint(new Vector3(0, _cardYDefault)));
				CGC.transform.localPosition = new Vector3(0, _cardYDefault);
				CGC.transform.RotateAround(_cardRotationPoint.position, new Vector3(0, 0, 1), rotz);
				Vector3 endPos = CGC.transform.localPosition;
				CGC.transform.position = startPos;
				CGC.GetComponent<LerpTransform>().StartLerpPosition(endPos, true, 1 / animTime);
				index++;
				if (index < CGCs.Count)
				{
					float delay = 0.1f;
					while (delay > 0)
					{
						delay -= Tm.GetUIDelta();
						yield return null;
					}
				}
			}
			UpdateHandSiblingIndex();

			int buffertick = 2;
			float tempTime = animTime;
			while (buffertick > 0)
			{
				tempTime -= Tm.GetUIDelta();
				if (tempTime < 0)
					buffertick--;
				yield return null;
			}
			SetHandTransition(false);

		}
		public void ReturnCardToHand(CombatGUICard cg, float animTime = 0.2f)
		{
			if (_highlightedCard == cg)
				_highlightedCard = null;
			cg.transform.SetParent(transform);
			_cards.Remove(cg);
			ForcelowlightCards();
			_cards.Insert(0, null);
			UpdateHandPositions(0.25f);
			_cards[0] = cg;
			//_cards.Insert(0, cg);
			//_cards.Add(cg);
			Vector3 startPos = cg.transform.position;
			float rotz = GetCardRotations(0, false,_cards.Count);
			cg.transform.localEulerAngles = Vector3.zero;
			cg.transform.localPosition = new Vector3(0, _cardYDefault);
			cg.transform.RotateAround(_cardRotationPoint.position, new Vector3(0, 0, 1), rotz);
			Vector3 endPos = cg.transform.localPosition;
			cg.transform.position = startPos;
			cg.GetComponent<LerpTransform>().StartLerpPosition(endPos, true, 1 / animTime);
			//Debug.Break();
			UpdateHandSiblingIndex();
		}
		void ClearDeck()
		{
			for (int i = _cards.Count - 1; i >= 0; i--)
				RemoveCard(_cards[i]);
		}
		public void RemoveCard(CombatGUICard c)
		{
			if (_cards.Contains(c))
				_cards.Remove(c);
			//Debug.Log("REMOVING " + c.gameObject.name);
			Graveyard.Remove(c.gameObject);
			//Destroy(c.gameObject);
		}
		public void ForcelowlightCards()
		{
			for (int i = 0; i < _cards.Count; i++)
			{
				_cards[i].ForceLowlight();
			}
		}
		public void SetHandTransition(bool state)
		{
			foreach(CombatGUICard c in _cards)
			{
				c._inTransition = state;
			}
		}
		public void UpdateHandPositions(float animTime = 0.25f,int cardCount = default)
		{
			if (cardCount == default)
				cardCount = _cards.Count;
			//Debug.Log("<color=orange>UpdateHandPositions!</color>\ncardCount = "+cardCount);
			//	ForcelowlightCards();
			for (int i = 0; i < _cards.Count; i++)
			{
				if (_cards[i] == null)
				{
					Debug.Log("cards[" + i + "] is null!");
					continue;
				}
				CombatGUICard cg = _cards[i];
				float deltaRot = GetCardRotations(i,true,cardCount);
				Vector3 rotationPos = GetPositionForAngle(cg, GetCardRotations(i, false, cardCount));
				if (deltaRot!=0)
				{
					if (animTime > 0)
						cg.GetComponent<LerpTransform>().StartLerpRotate(deltaRot,1/animTime);
					else
						cg.transform.RotateAround(_cardRotationPoint.position,new Vector3(0,0,1), deltaRot);
				}
				if (!cg.transform.localPosition.Equals(rotationPos) && animTime > 0)
				{
					cg.GetComponent<LerpTransform>().StartLerpPosition(rotationPos, true, 1 / animTime);
				}
				else
					cg.transform.localPosition = rotationPos;
				UpdateHandSiblingIndex();
			}
			BlockInteractions(animTime);
			Vector3 GetPositionForAngle(CombatGUICard cg, float angle)
			{
				Vector3 oldPos = cg.transform.localPosition;
				Vector3 oldRot = cg.transform.localEulerAngles;
				cg.transform.localEulerAngles = Vector3.zero;
				cg.transform.localPosition = new Vector3(0, _cardYDefault);
				cg.transform.RotateAround(_cardRotationPoint.position, new Vector3(0, 0, 1), angle);
				Vector3 rotationPos = cg.transform.localPosition;
				cg.transform.localPosition = oldPos;
				cg.transform.localEulerAngles = oldRot;
				return rotationPos;
			}
		}
		public void UpdateHandSiblingIndex()
		{
			for(int i = _cards.Count-1; i >=0; i--)
			{
				if (_cards[i] == null)
					continue;
				_cards[i].CheckPlayability();
				if (_cards[i] == _highlightedCard)
				{
					_cards[i].transform.SetAsLastSibling();
				}
				else if (!_cards[i]._cardState.HasFlag(CombatGUICard.CardState.Highlighted))
				{
					_cards[i].transform.SetSiblingIndex(_cards.Count - 1 - i);
				}

			}
		}
		public void HighlightCard(CombatGUICard card)
		{
			_highlightedCard = card;
			//keep track of cards!
		}
		public void LowlightCard()
		{
			_highlightedCard = null;
		}
		public enum PlayPosition { None=0, Card=1, Strike=2, Move=3}
		public PlayPosition CheckBounds(Vector3 pos)
		{
			Canvas canvas = GetComponentInParent<Canvas>();
			pos = canvas.ScreenToCanvasPosition(pos);
			if (CheckBoundsOfArea(pos, _combatGUI._comps._cardPlayArea))
				return PlayPosition.Card;
			if (CheckBoundsOfArea(pos, _combatGUI._comps._cardStrikeArea))
				return PlayPosition.Strike;
			if (CheckBoundsOfArea(pos, _combatGUI._comps._cardMoveArea))
				return PlayPosition.Move;
			return PlayPosition.None;
		}
		bool CheckBoundsOfArea(Vector2 pos, RectTransform area)
		{
			Rect rect = new Rect();
			rect.width = area.rect.width;
			rect.height = area.rect.height;
			Vector2 position = area.localPosition;
			position.x -= (area.pivot.x - 0.5f) * area.rect.width;
			position.y -= (area.pivot.y - 0.5f) * area.rect.height;
			rect.position = position;
			Vector2 min = new Vector2(rect.x - (rect.width / 2), rect.y - (rect.height / 2));
			Vector2 max = new Vector2(rect.x + (rect.width / 2), rect.y + (rect.height / 2));
			//Debug.Log("Min " + min + "---- Max " + max);
			return (pos.x > min.x && pos.x < max.x && pos.y > min.y && pos.y < max.y);

		}
		public void DiscardCard(CombatGUICard c)
		{
			_cards.Remove(c);
			UpdateHandPositions();
		}
		public IEnumerator BurnCard(CombatGUICard cgcard)
		{
			Task t = new Task(cgcard.BurnCard());
			_cards.Remove(cgcard);
			UpdateHandPositions();
			while (t.Running)
				yield return null;
			RemoveCard(cgcard);
		}
		/*float GetCardX(int cardIndex)
		{
			float distance = Mathf.Clamp(maxCardSpace / _deck._cards.Count, maxCardSpace / _deck._cards.Count, maxDistance);
			float startX = (_deck._cards.Count - 1) / -2f * distance;
			float xDist = startX + distance * cardIndex;
			return xDist;
		}*/
		public float GetInspectHeightWorld(float txtHeight = 0)
        {
			Vector3 tempos = _cardInspectPosition.localPosition;
			_cardInspectPosition.localPosition = new Vector3(_cardInspectPosition.localPosition.x, _cardInspectPosition.localPosition.y + txtHeight);
			float worldHeight = _cardInspectPosition.position.y;
			_cardInspectPosition.localPosition = tempos;
			return worldHeight;
		}
		float GetCardRotations(int cardIndex, bool delta = true,int handsize = default)
		{
			if(handsize == default)
				handsize = _cards.Count;
			Vector3 cardRot = Vector3.zero;
			if (handsize > 1)
			{
				float maxAngle = 5;
				float totalArea = 35;
				float distance = Mathf.Clamp(totalArea / handsize, totalArea / handsize, maxAngle);
				float startZ = (handsize - 1) / -2f * distance;
				float zDist = startZ + distance * cardIndex;
				cardRot.z = zDist;
			}
			if (delta)
			{
				Transform cardT = _cards[cardIndex].transform;
				cardRot.z = Mathf.DeltaAngle(cardT.localEulerAngles.z, cardRot.z);//cardRot.z -= cardT.localEulerAngles.z;
			}
			//Debug.Log("<color=green>Rotation for " + (cardIndex +1)+" of "+handsize+ " is " + cardRot.z+"</color>");
			return cardRot.z;
		}
		[NonSerialized] public bool _cardTargetingMode = false;
		Transform _originalParent;
		public void HighlightHand()
		{
			Debug.Log("HighlightHand");
			SetHandGlow(true);
			_originalParent = transform.parent.parent;
			_cardTargetingMode = true;
			transform.parent.SetParent(_combatGUI._comps._highlight);
		}
		public IEnumerator LowlightHand()
		{
			BlockInteractions(_combatGUI.durationEstimate);
			while (_blockInteractions.Running)
				yield return null;
			transform.parent.SetParent(_originalParent);
		}
		float blockTime = 0f;
		Task _blockInteractions;
		void BlockInteractions(float time)
		{
			if (time <= 0)
				return;
			if (_blockInteractions == null || !_blockInteractions.Running)
			{
				_blockInteractions = new Task(BlockCoroutine(time));
			}
			else
				SetBlockTime(time);
		}
		void SetBlockTime(float time)
		{
			blockTime = Mathf.Clamp(time, blockTime, time);
		}
		IEnumerator BlockCoroutine(float time)
		{
			SetBlockTime(time);
			//Debug.Log("Time = " + blockTime);
			_cardTargetingMode = false;
			_blockInteraction = true;
			while (blockTime > 0)
			{
				blockTime -= Tm.GetUIDelta();
				yield return null;
			}
			_blockInteraction = false;
		}
		public void SetHandGlow(bool active, CombatGUICard exception = null)
		{
			foreach(CombatGUICard card in _cards)
			{
				if(!exception || (card != exception))
				{
					card.ToggleGlow(active);
				}
			}
		}
	}
}
