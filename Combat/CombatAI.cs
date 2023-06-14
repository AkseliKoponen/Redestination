using System;
using System.Collections;
using System.Collections.Generic;
using RD.DB;
using RD;
using UnityEngine;
using static RD.CodeTools;

namespace RD.Combat {
	public class CombatAI
	{
		public CombatCharacter _cc { get; private set; }
		static float _thinkTime = 0.5f;
		static bool _debug = true;
		Aggression _aggression = Aggression.neutral;
		enum Aggression { defensive, neutral, aggressive };
		/* Defensive = Prioritize survival over hurting the enemy
	 * Neutral = No priorities
	 * Aggressive = Prioritize hurting the enemy over protecting yourself
	 */

		Profile profile = Profile.none;
		enum Profile { none, beast, simple, strategic, berserker, coward };
		/* None = Pass every turn
	 * Beast = Always aggro. Predictable and prone to abuse. Attacks their target, even when they are protected etc.
	 * Simple = Beastlike, but will not yield if it belives it can still beat you. Can change target if not taunted and a more viable target exists
	 * Strategic = Try to make the best play possible every turn. Picks best target in each situation.
	 * Berserker = Always aggro. Wiser than beast.
	 * Coward = always defensive
	 */

		/*
	  * If wounded & not alone, defend yourself. 
	  * Else aggressive.
	  */
		public CombatAI(CombatCharacter combatCharacter, string awareness = "none")
		{
			_cc = combatCharacter;
			Enum.TryParse(awareness, true, out profile);
		}

		public IEnumerator Act()
		{
			while (GUIAnimationManager._free == false)
				yield return null;
			float thinktime = _thinkTime;
			while (_cc._deck.HasPlayableCards() && _cc.AbleToAct()) {
				while (thinktime > 0)
				{
					thinktime -= Tm.GetWorldDelta();
					yield return null;
				}
				
				SetAggression();
				Decision decision = Analyze(GetWisdom());
				if (decision != null)
				{
					Card c = decision.card._card;
					CombatCharacter target = decision.target._cc; //MULTITARGET?
					List<CombatCharacter> targets = new List<CombatCharacter> { target };
					if (decision.card._card._multiTarget != BaseCard.MultiTargetType.One)
					{
						foreach(CombatCharacter cc in TargetingSystem._current.GetValidTargetsForCard(decision.card._card))
						{
							if (!targets.Contains(cc))
								targets.Add(cc);
						}
					}
					TargetingSystem._current.DisplayAITargeting(c, targets);
					if (c._gUICard)
					{
						float waitTime = c._gUICard.DisplayAICard();
						while (waitTime > 0)
						{
							waitTime -= Tm.GetUIDelta();
							yield return null;
						}
					}
					Task t  = _cc.PlayCard(c, targets);
					while (t.Running)
						yield return null;
				}
				while (GUIAnimationManager._free == false)
					yield return null;
				Debug.Log(_cc.GetName() + " has " + _cc._actions.current + " actions and " + _cc._deck._mind._ideas.Count + " ideas.");
			}
			while (GUIAnimationManager._free == false)
				yield return null;
			Pass();

			void SetAggression()
			{
				switch (profile)
				{
					default:
						_aggression = Aggression.neutral;
						break;
					case Profile.berserker:
						//Berserker is always aggressive
						_aggression = Aggression.aggressive;
						return;
					case Profile.coward:
						//Coward is always defensive
						_aggression = Aggression.defensive;
						return;
				}
				float hp = _cc._stats.hp.GetPercentage();
				if (hp > 0.33f && hp < 0.66f)
				{
					switch (profile)
					{
						default:
							_aggression = Aggression.defensive;
							break;
						case Profile.beast:
							//Beasts turn aggressive when cornered
							_aggression = Aggression.aggressive;
							break;

					}
				}
				else if (hp <= 0.33f)
				{
					switch (profile)
					{
						default:
							//When at death's door, humanoids will attempt to atleast take the player with them
							_aggression = Aggression.aggressive;
							break;
						case Profile.beast:
							//Beasts have no death wish, they will attempt to survive over killing the player
							_aggression = Aggression.defensive;
							break;

					}
				}
			}
			float GetWisdom()
			{
				return 1;
				switch (profile)
				{
					case Profile.none:
						return 0;
					default:
					case Profile.simple:
					case Profile.beast:
						return 0.5f;
					case Profile.strategic:
						return 1f;
					case Profile.coward:
					case Profile.berserker:
						return 0.75f;

				}
			}
			Decision Analyze(float wisdom)
			{
				float aggression = 0.5f;
				switch (_aggression)
				{
					case Aggression.aggressive:
						aggression *= 2;
						break;
					case Aggression.neutral:
						break;
					case Aggression.defensive:
						aggression *= 0.5f;
						break;
				}
				List<CardAttraction> cards = new List<CardAttraction>();
				{
					List<Card> playablecards = _cc._deck._mind._ideas;
					for (int i = playablecards.Count - 1; i >= 0; i--)
					{
						if (!playablecards[i].CheckPlayable())
						{
							playablecards.RemoveAt(i);
						}
					}
					foreach (Card c in playablecards)
					{
						CardAttraction ca = new CardAttraction(c, aggression);
						if (ca._valid)
							cards.Add(ca);
					}
					cards.Sort(CardAttraction.CompareByAttraction);
				}
				Mathf.Clamp(aggression, 0, 1);
				if (cards.Count < 1)
				{
					//if (_debug)
						Debug.Log("<color=yellow>" + _cc.GetName() + "has no cards to play. No decision made." + "</color>");
					return null;
				}

				List<Decision> decisions = new List<Decision>();
				int analyzeDepth = 3;
				for (int i = 0; i < analyzeDepth && i < cards.Count; i++)
				{
					decisions.AddRange(cards[i].GetDecisions(analyzeDepth));
				}
				if (decisions.Count == 0)
					return null;
				decisions.Sort(Decision.CompareByAttraction);
				int decisionIndex = wisdom < 0.75 ? (wisdom < 0.4f ? 2 : 1) : 0;
				decisionIndex = Mathf.Clamp(decisionIndex, 0, decisions.Count-1);
				//if nothing to do, then move closer?
				Dbug();
				return decisions[decisionIndex];

				void Dbug(string color = "yellow")
				{
					if (!_debug)
						return;
					Debug.Log("<color=" + color + ">-------------------------------</color>");
					Debug.Log("<color=" + color + ">" + _cc.GetName() + " is Analyzing....</color>");
					//foreach (TargetAttraction c in targets) c.Log(color);
					//foreach (CardAttraction c in cards) c.Log(color);
					for (int i = 0; i < decisions.Count; i++)
					{
						Debug.Log(_cc.GetName() + "'s <color=" + color + "> " + (i + 1) + ". decision is to play </color>" + decisions[i].card._card._name + "<color=" + color +
							"> on </color>" + (decisions[i].target._cc != _cc ? decisions[i].target._cc.GetName() : "themself") +
							"<color=" + color + "> with attraction value of </color>" + decisions[i].attraction);
					}
					Debug.Log("<color=" + color + "> Picking decision " + (decisionIndex + 1) + " when wisdom is " + wisdom + "</color>");

					Debug.Log("<color=" + color + ">-------------------------------</color>");
				}
			}
		}
		class CardAttraction
		{
			public Card _card { get; private set; }
			public List<Target> _targetAttractions { get; private set; }

			public float _attraction { get; private set; }  //Attraction to play the card in general
			public float _attractionCritical { get; private set; }  //Attraction to gain the critical effects of the card
			public float _maximalAttraction { get; private set; }   //Attraction of the card on the most attractive target
			public bool _valid { get; private set; } = false;
			public CardAttraction(Card card, float aggression)
			{
				_card = card;
				if (!_card.CheckPlayable())
					return;
				CalculateAttraction(aggression);
				SetTargetAttractions();
				if (_targetAttractions.Count > 0)
				{
					_valid = true;
					_maximalAttraction = _targetAttractions[0]._attraction + _attraction;
				}
				else
				{
					Debug.Log(_card._name + "NOT VALID");
					_valid = false;
					_maximalAttraction = 0;
				}
			}
			void SetTargetAttractions()
			{
				_targetAttractions = new List<Target>();
				{
					List<CombatCharacter> targetcharacters = new List<CombatCharacter>();
					targetcharacters.AddRange(TargetingSystem._current.GetValidTargetsForCard(_card));
					if (_card._debugLog) foreach (CombatCharacter cc in targetcharacters) Debug.Log(cc.GetName() + " is viable target for "+_card._name);
					//if(targetcharacters.Contains(_cc)) targetcharacters.Remove(_cc);
					foreach (CombatCharacter cc in targetcharacters)
					{
						//Debug.Log(_card._name + " -> "+cc.GetName());
						_targetAttractions.Add(new Target(cc, _card.GetOwner(), this));
					}
					switch (_card._multiTarget)
					{
						default:
							break;
						case BaseCard.MultiTargetType.Two:
						case BaseCard.MultiTargetType.Three:
						case BaseCard.MultiTargetType.All:
							//if multitarget, merge the attractiveness of every target
							//npc should only have skills that target the whole party or one
							Target t = new Target(_targetAttractions);
							_targetAttractions = new List<Target> { t };
							break;
					}
					
					_targetAttractions.Sort(Target.CompareByAttraction);
				}
			}
			public List<Decision> GetDecisions(int count = 1, bool idiotic = false)
			{
				List<Decision> decisions = new List<Decision>();
				for (int i = 0; i < count && i < _targetAttractions.Count; i++)
				{
					if (!idiotic && _targetAttractions[i]._attraction < 0)
						continue;
					decisions.Add(new Decision(this, _targetAttractions[i]));
				}
				return decisions;
			}
			public void CalculateAttraction(float aggression)
			{
				_attraction = 0;
				if (aggression >= 0.5f && _card._isHostile)
					_attraction += 1f;
				_attraction += _card.GetGeneralAttraction();
			}
			public static int CompareByAttraction(CardAttraction ca2, CardAttraction ca1)
			{
				return ca1._maximalAttraction.CompareTo(ca2._maximalAttraction);
			}
			public void Log(string color = "yellow")
			{
				Debug.Log("<color=" + color + ">" + _card._name + " has attraction of " + _attraction + ".</color>");
			}

			public class Target
			{
				CombatCharacter _self;
				public CombatCharacter _cc { get; private set; }
				public bool _isAlly { get; private set; } = false;
				public Target(List<Target> multitarget)
				{
					_cc = multitarget[0]._cc;
					_self = multitarget[0]._self;
					foreach (Target t in multitarget)
						_attraction += t._attraction;
				}
				public Target(CombatCharacter target, CombatCharacter self, CardAttraction ca)
				{
					_cc = target;
					_self = self;
					_isAlly = !_self.IsHostileTowards(target);
					bool hostileCard = ca._card._isHostile;
					CalculateAttraction(hostileCard);
					_attraction += ca._card.GetAttractionOnTarget(_cc);
				}

				public float _attraction { get; private set; }
				public void CalculateAttraction(bool hostileCard)
				{
					/*
					 * Attraction in general is increased by
					 * low hp percentage v
					 * turn being close v
					 */
					_attraction += 1 - _cc._stats.hp.GetPercentage();
					_attraction += (TurnDisplay._turnMax - TurnDisplay.GetSpeedOfCombatCharacter(_cc)) / 3;
					if (_isAlly)
					{
						/*
						 * Attraction against friendlies is increased by
						*/
					}
					else
					{

						/*
						 * Attraction against enemies is increased by
						 * provoke
						 * Proximity?
						*/
						Aura provoke = _self.GetAura((BaseAura)db.Get<BaseAura>(13));
						if (provoke != null && provoke._source == _cc)
						{
							_attraction += 100;
						}
					}
					if ((_isAlly && hostileCard) || (!_isAlly && !hostileCard))
					{
						//if attacking ally or healing enemy, reverse the attraction values
						if (_attraction > 0)
						{
							_attraction *= -1;
						}
						_attraction -= 3;
					}
				}
				public static int CompareByAttraction(Target ta2, Target ta1)
				{
					return ta1._attraction.CompareTo(ta2._attraction);
				}
				public void Log(string color = "yellow")
				{
					Debug.Log("<color=" + color + ">" + _cc.GetName() + " has attraction of " + _attraction + ".</color>");
				}
			}
		}
		class Decision
		{
			public CardAttraction card;
			public CardAttraction.Target target;
			public float attraction;
			public Decision(CardAttraction cardAttraction, CardAttraction.Target targetAttraction)
			{
				card = cardAttraction;
				target = targetAttraction;
				attraction = cardAttraction._attraction + targetAttraction._attraction;
			}
			public static int CompareByAttraction(Decision d2, Decision d1)
			{
				return d1.attraction.CompareTo(d2.attraction);
			}
		}
		public void Pass()
		{
			if(_debug)Debug.Log("<color=green>" + _cc.GetName() + " passed.</color>");
			_cc.EndTurn();
		}
	}
}
