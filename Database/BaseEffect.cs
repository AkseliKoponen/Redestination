using System;
using System.Collections;
using System.Collections.Generic;
using RD.Combat;
using RD;
using UnityEngine;
using static RD.CodeTools;

namespace RD.DB
{
	
	public class BaseEffect : BaseObject
	{
		#region Variables
		//if requirements do not match, cancel spell effect. The card can still be played but this particular spell will have no effect.
		//--------------Damage and HP MOD------------
		//public HealthData _modHP = new HealthData(true);
		//public HealthData _setHP = new HealthData(true);
		public bool _dealsDamage = false;
		public int _damageFlat;                                                 //Use this for attack damage
		[Tooltip("Melee = sword damage + STR*3.\nRanged = pistol damage + DEX*3\nSpecial = INT*3")]
		public DamageType _damageType;
		public int _modInit;                            // Change combatCharacter Initiative, forcing them back or forth in turn order
		                //0 = str, 1 = agi, 2 = int, 3 = tena ----------- multiply attributes to get the total bonus --------- attributeBonuses affect _damagebonus, _modHP and _addEnergy
		//-------------Card Targeting----------------
		public CardTargeting _cardTargeting = new CardTargeting(true);
		//------------CARD MODIFICATION--------------
		public CardModification _cardModification = new CardModification(true);
		//------------ADDING CARDS TO HAND & DECK-------
		public CardRecovery _cardRecovery = new CardRecovery(true);                //Recover your own cards from deplete or discard
		public AddCard _addCard = new AddCard();                         //Shuffle new cards to deck or add them to directly to hand
		public CardManagement _cardManagement = new CardManagement(true);
		//--------------------MISCELLANEOUS-------------
		//public MovementType _movementType = MovementType.stay;
		[Tooltip(">0 = Move Towards Enemy, <0 = Move Away From Enemy")]public int _moveAmount = 0;                       //Positive values = push, negative values = push
		//public bool _pushStopOnCollision = true;          //Stop push movement if the next position is not empty
		public ApplyAura _applyAura = new ApplyAura();
		/*
	 * Spell visual effects
	*/
		public enum DamageType {None, Melee, Ranged, Special, Heal, Override}
		public enum SpellPowerType { Damage, ModHealth, Draw, AddEnergy, None }
		public enum MovementType { stay, moveToTarget, moveBeforeTarget, moveBehindTarget, swapPlaces }

		[Tooltip("How likely the AI will use cards with this effect")]
		public float _attraction = 0;
		[Serializable]
		public struct CardModification
		{
			public BaseCard.Effects _modCardAddEffect;            //Add an extra effect to the card, for example gain 2 hp when you play the card
			public int _modCardPower;                         //cardPower not found in CardData, only in instantiated Cards. Always a default of 0
			public bool _modCardPurge;                        //Purge() removes any temporary modifications from cards
			public CardModification(bool tru = true)
			{
				_modCardAddEffect = new BaseCard.Effects(true);
				_modCardPower = 0;
				_modCardPurge = false;
			}
		}
		[Serializable]
		public struct CardTargeting
		{
			public enum CardTarget { This, Hand, Draw, Discard }
			public CardTarget _target;
			public int _targetCount;
			public bool _random;
			public CardTargeting(CardTarget target, int targetCount = 0, bool random = false)
			{
				_target = target;
				_targetCount = targetCount;
				_random = random;
			}
			public CardTargeting(bool tru)
			{
				_target = CardTarget.This;
				_targetCount = 1;
				_random = false;
			}
		}
		[Serializable]
		public struct CardManagement
		{
			public int _discardCards;
			public bool _discardRandom;
			public int _depleteCards;
			public bool _depleteRandom;
			public int _drawCards;
			public Deck.DeckType _drawDeckType;
			public bool _shuffleDiscardIntoDraw;   //Shuffle discarded cards back into the deck
			public bool _shuffleDrawDeck;           //Shuffle the draw deck
			public CardManagement(bool tru)
			{
				_discardCards = 0;
				_discardRandom = false;
				_depleteCards = 0;
				_depleteRandom = false;
				_drawCards = 0;
				_shuffleDiscardIntoDraw = false;
				_shuffleDrawDeck = false;
				_drawDeckType = Deck.DeckType.Draw;
			}
		}
		[Serializable]
		public struct CardRecovery
		{
			public Deck.DeckType _recoveryDeck;
			public Deck.DeckType _recoveryTarget;
			public int _recoveryCount;
			public bool _random;

			public CardRecovery(Deck.DeckType recoveryDeck, Deck.DeckType recoveryTarget, int recoveryCount = 1, bool random = false)
			{
				_recoveryDeck = recoveryDeck;
				_recoveryTarget = recoveryTarget;
				_recoveryCount = recoveryCount;
				_random = random;
			}
			public CardRecovery(bool tru)
			{
				_recoveryDeck = Deck.DeckType.Discard;
				_recoveryTarget = Deck.DeckType.Hand;
				_random = false;
				_recoveryCount = 0;
			}
		}
		[Serializable]
		public struct AddCard
		{
			public BaseCard _card;
			public Deck.DeckType _cardDestination;
			public int _cardCount;
			[Tooltip("Random = shuffled into deck,\nnot random = top of deck")]
			public bool _random;
			public AddCard(bool tru)
			{
				_card = null;
				_cardDestination = Deck.DeckType.Hand;
				_cardCount = 0;
				_random = false;
			}
		}
		[Serializable]
		public struct ApplyAura
		{
			public BaseAura _aura;
			public int _duration;
			public int _cases;
			public ApplyAura(bool tru)
			{
				_aura = null;
				_duration = 1;
				_cases = 1;
			}
			public ApplyAura(BaseAura baseaura)
			{
				_aura = baseaura;
				_duration = 1;
				_cases = 1;
			}
			public ApplyAura(ApplyAura aa)
			{
				_aura = aa._aura;
				_duration = aa._duration;
				_cases = aa._cases;
			}
		}
		[Serializable]
		public struct HealthData
		{
			public int flat;
			public float missing;
			public Bfloat fractional;
			public HealthData(bool tru)
			{
				fractional = new Bfloat(0);
				flat = 0;
				missing = 0;
			}
		}

		#endregion
		public BaseEffect()
		{
			_cardModification = new CardModification(true);
			_cardRecovery = new CardRecovery();
			_addCard = new AddCard();
			_cardManagement = new CardManagement();
			_layOutSpace = new LayOutSpace(new List<int> { 1, 4, 1, 1, 1, 1, 1, 2 }, new List<string> { "Damage and hp mod", "CardTargeting", "Card Modification", "Card Recovery", "Add Card", "Card Management", "Misc", "General" });

		}

		public IEnumerator Activate(CombatCharacter target, Combat.CombatCharacter source, float power, Card c = null)
		{
			//Debug.Log("Card Result = " + result.ToString());
			if(_debugLog)
				Debug.Log("<color=teal>"+ GetFileName() + "</color> targeting <color=orange>" + (target?target.GetName():" ground")+"</color>");
			if (!_requireSpecialCode)
			{
				#region Damage 
				if (target != null && _dealsDamage)
				{
					Damage damage = new Damage(this, source, target,null,power);
					source.StartCoroutine(source.PrepareOutgoingDamage(damage));
					source.AddDamageToHistory(damage);
					target.prepareIncomingDamageTask = new Task(target.PrepareIncomingDamage(damage));
					if(_id == 21)
					{
						source._inventory._ranged.Trigger();
					}
				}
				if(target!=null && _damageType == DamageType.Heal)
				{
					target.TakeHeal(source.EstimateDamageQuick(this, power, target));
				}
				#endregion
				#region CardModification Done
				if (_cardModification._modCardAddEffect.IsDefault()==false || _cardModification._modCardPower!=0 || _cardModification._modCardPurge)
				{
					//Start Card targeting
					//Note: Title comes from effect description
					CoroutineWithData crd = new CoroutineWithData(_combatGUI, TargetCardsInHand(1, GetDescription()));
					while (crd.result == null) yield return null;
					List<Card> cards = (List<Card>)crd.result;
					if (_cardModification._modCardPurge)
					{
						cards[0].Purge();
					}
					if (_cardModification._modCardAddEffect.IsDefault() == false)
					{
						cards[0].CombineEffects(_cardModification._modCardAddEffect, true);
					}
				}
				#endregion
				#region CardRecovery done, missing animations
				if (target != null && _cardRecovery._recoveryCount > 0)
				{
					CardRecovery cr = _cardRecovery;
					Deck recoveryDeck = target._deck.GetDeckByName(cr._recoveryDeck.ToString());
					Deck targetDeck = target._deck.GetDeckByName(cr._recoveryTarget.ToString());
					List<Card> cards = new List<Card>();
					#region GetCards
					if (cr._random)
					{
						for(int i = 0;i<cr._recoveryCount && recoveryDeck._ideas.Count>0; i++)
						{
							int r = UnityEngine.Random.Range(0, recoveryDeck._ideas.Count);
							Card tempcard = recoveryDeck._ideas[r];
							cards.Add(tempcard);
							recoveryDeck._ideas.Remove(tempcard);
						}
					}
					else
					{
						if (cr._recoveryCount < recoveryDeck._ideas.Count)
						{
							_combatGUI.PauseTimers(true);
							DeckViewer dv = GameManager._current._guiParts._deckViewer;
							TargetingSystem.CardTargetingTool ctt = dv.InitializeDeckTargeting(cr._recoveryCount,recoveryDeck);
							while (ctt.targetedCards.Count < cr._recoveryCount)
							{
								yield return null;
							}
							_combatGUI.PauseTimers(false);
							foreach (CombatGUICard tempcard in ctt.targetedCards)
							{
								cards.Add(tempcard._card);
								recoveryDeck._ideas.Remove(tempcard._card);
							}
						}
						else
						{
							foreach(Card tempcard in recoveryDeck._ideas)
							{
								cards.Add(tempcard);
							}
							recoveryDeck._ideas.Clear();
						}
					}
					#endregion
					foreach (Card tempcard in cards)
					{
						targetDeck._ideas.Add(tempcard);
						if (targetDeck._deckType == Deck.DeckType.Hand && _combatGUI._displayedCharacter == target)
						{
							Debug.LogError("Missing Draw animation");
						}
						else
						{
							Debug.LogError("Missing Card Moving animation!");
						}
					}
				}
				#endregion
				#region AddCard Done
				if(target!=null && c!=null && _addCard._card!=null && _addCard._cardCount > 0)
				{
					Debug.Log("AddCard!");
					Deck d = target._deck.GetDeckByDeckType(_addCard._cardDestination);
					target.AddCard(_addCard._card, d, c._gUICard.transform.position,_addCard._cardCount);
					//d.AddCards(_addCard._card._id, _addCard._cardCount, _addCard._random);
					if (_addCard._cardDestination == Deck.DeckType.Hand)
					{
						//Debug.Log("Added a card to hand!");
						//Handyyn, päivitä UI tai mitä tehään??
					}
				}
				#endregion
				#region CardManagement Done
				if(target!=null) {
					CardManagement cm = _cardManagement;
					Combat.CombatCharacter cc = target;
					if (cm._discardCards > 0)
					{
						if (_debugLog)
							Debug.Log("CC has " + cc._deck._mind._ideas.Count + " cards in hand.");
						if (cm._discardRandom)
						{
							cc.DiscardRandomCards((int)(cm._discardCards * power));
						}
						else
						{
							CoroutineWithData crd = new CoroutineWithData(_combatGUI, TargetCardsInHand((int)(power * cm._discardCards), "Discard"));
							while (crd.result == null) yield return null;
							List<Card> cards = (List<Card>)crd.result;
							cc.DiscardCards(cards, false);
						}
					}
					if(cm._depleteCards > 0)
					{
						if (cm._depleteRandom)
							cc.DepleteRandomCards(cm._depleteCards);
						else
						{
							CoroutineWithData crd = new CoroutineWithData(_combatGUI, TargetCardsInHand((int)(power * cm._discardCards),"Deplete"));
							while (crd.result == null) yield return null;
							List<Card> cards = (List<Card>)crd.result;
							foreach (Card depleteCard in cards)
								cc.DepleteCard(depleteCard);
						}
					}
					if (cm._drawCards>0)
					{
						if (_debugLog)
							Debug.Log("Drawing " + cm._drawCards+" while Draw has "+cc._deck._subcon._ideas.Count+" and discard has "+cc._deck._forget._ideas.Count);
						cc.DrawCard((int)(power * cm._drawCards));
					}
					if (cm._shuffleDiscardIntoDraw)
					{
						cc._deck._subcon.AddDeck(cc._deck._forget._ideas);
						cc._deck._forget.Clear();
						cc._deck._subcon.Shuffle();
					}
					else if (cm._shuffleDrawDeck)
						cc._deck._subcon.Shuffle();

				}
				#endregion
				#region Movement
				if (_moveAmount != 0)
				{
					_combatManager.PushCharacter(_moveAmount, source, target, false);
				}
				#endregion
				#region Auras
				if(_applyAura._aura!= null)
				{
					ApplyAura aa = new ApplyAura(_applyAura);
					if (_applyAura._aura._stackingType == BaseAura.StackingType.Case)
						aa._cases = Mathf.RoundToInt(aa._cases * power);
					else if(_applyAura._aura._stackingType == BaseAura.StackingType.Duration)
						aa._duration = Mathf.RoundToInt(aa._duration * power);
					target.AddAura(aa, source);

				}
				#endregion
			}
			else
			{//Hardcoded Effects
				switch (_id)
				{
					default:
						Debug.LogError("Unimplemented Hard Code at " + _name);
						break;
					case 12:
						//012 - Barrier. Gain barrier.
						int bar = Mathf.RoundToInt(power * (_damageFlat + (source._attributes.CalculateEffectScaling(this))));
						target._stats.barrier.Add(bar,true);
						target.UpdateStats(true);
						break;
					case 25:
					case 26:
						//0025 - Haste
						//0026 - Slow
						if(TurnDisplay._current.CommitHastePreview() == false)
						{
							TurnDisplay._current.HasteCharacter(target, Mathf.RoundToInt(power * (_id==25?3f:-3f)));
						}
						
						//Do nothing, the haste/slow is already applied in preview.
						break;
					case 34:
					case 39:
					case 42:
						#region Reload
						//034 - Reload and spend a reaction
						//039 - Reload
						//042 - Reload 1 turn
						{
							Weapon pistol = target._inventory._ranged;
							if (pistol == null || pistol.ReadyToFire())
								break;
							if (_id == 39)
								pistol.Reload();
							else if (_id == 34)
							{
								Aura reaction = source.GetAura((BaseAura)db.Get<BaseAura>(0));
								if (reaction != null)
								{
									reaction.RemoveCases(1);
									pistol.Reload();
								}
							}
							else if (_id == 42)
							{
								Aura rld = source.GetAura((BaseAura)db.Get<BaseAura>(19));
								if (rld != null){
									rld.Trigger(null);
								}
							}
						}
						#endregion
						break;
					case 38:
						//0038 - End Turn
						break;
					case 40:
						//040 - Gain 10% HP
						int heal = Mathf.RoundToInt((target._stats.hp.max - target._stats.hp.current) * 0.1f * power);
						target.TakeHeal(heal, source);
						break;
					case 45:
						//045 - Regain an action.
						source.RegainActions();
						break;
				}
			}
		}
		public int GetNumberQuick(float powerMultiplier, CombatCharacter source)
		{
			
			if (_requireSpecialCode)
			{
				switch (_id)
				{
					default:
						break;
					case 12:
						//012 - Barrier. Gain barrier.
						return Mathf.RoundToInt(powerMultiplier * (_damageFlat + (source._attributes.CalculateEffectScaling(this))));
				}
			}
			if (!_dealsDamage)
			{
				float value = 0;
				value = _damageFlat;
				if(value!=0) return Mathf.RoundToInt(value * powerMultiplier);
				value = _cardManagement._drawCards;
				if (value != 0) return Mathf.RoundToInt(value * powerMultiplier);
				value = _cardManagement._depleteCards;
				if (value != 0) return Mathf.RoundToInt(value * powerMultiplier);
				value = _cardManagement._discardCards;
				if (value != 0) return Mathf.RoundToInt(value * powerMultiplier);
				value = _addCard._cardCount;
				if (value != 0) return Mathf.RoundToInt(value * powerMultiplier);
				value = _moveAmount;
				if (value != 0) return Mathf.RoundToInt(value * powerMultiplier);
				Debug.LogError("Unable to determine quick number for effect " + _name + ". Returning 0!");


			}
			if (source == null)
				return Mathf.RoundToInt(_damageFlat * powerMultiplier);
			return Mathf.RoundToInt(source.EstimateDamageQuick(this,powerMultiplier));
			
			
		}
		public IEnumerator TargetCardsInHand(int count, string verb)
		{
			_combatGUI.PauseTimers(true);
			TargetingSystem ts = _combatGUI.ActivateHandTargeting(count,verb);
			//Hand has to be moved before the darkening screen
			_postProcessingManager.Target(true);
			while (ts.ready==0)
			{
				yield return null;
			}
			_postProcessingManager.Target(false);
			List<Card> cards = new List<Card>();
			foreach (CombatGUICard cgcard in ts.GetSelectedCards())
			{
				cards.Add(cgcard._card);
			}
			//hand has to be moved back
			_combatGUI.PauseTimers(false);
			yield return cards;
		}
	}
}
