using System.Collections.Generic;
using RD.DB;
using RD;
using UnityEngine;
using static RD.CodeTools;
using System.Reflection;

namespace RD.Combat
{
	public enum Facing { Left, Right }

	
	public class Deck
	{
		public int _beingDiscarded = 0;
		public CombatCharacter _cc;
		public List<Card> _ideas { get; protected set; }
		public List<BaseCard> _baseIdeas { get; protected set; }
		public bool GetVisible()
		{
			if(_cc._alliance == CombatCharacter.Alliance.Player)
				return true;
			else
			{
				foreach (Card c in _ideas)
					if (c._visible)
						return true;
				return false;
			}
		}
		public Deck(List<BaseCard> basecards, bool shuffle = true)
		{
			_baseIdeas = new List<BaseCard>();
			_baseIdeas.AddRange(basecards);
			_ideas = new List<Card>();
			foreach (BaseCard bas in basecards)
			{
				_ideas.Add((Card)ScriptableObject.CreateInstance(typeof(Card)));
				_ideas[_ideas.Count - 1].Init(bas);
				//_cards.Add(new Card(bas));
			}
			//Debug.Log("DeckCardCount = " + basecards.Count);
			if (shuffle && basecards.Count > 1) Shuffle();
		}
		/*
	 * if insertIndex < 0, add the card to the last position
	 */
		public void AddCards(int cardID, int cardCount = 1, bool shuffleIntoDeck = true, int insertIndex = 0)
		{
			for (int i = 0; i < cardCount; i++)
			{
				Card c = (Card)ScriptableObject.CreateInstance(typeof(Card));
				c.Init((Card)CodeTools.db.Get<BaseCard>(cardID));
				if (shuffleIntoDeck)
					_ideas.Insert(UnityEngine.Random.Range(0, _ideas.Count), c);
				else
				{
					if (insertIndex < 0)
						_ideas.Add(c);
					else
						_ideas.Insert(Mathf.Clamp(insertIndex, 0, _ideas.Count), c);
				}
				Debug.Log(c._name + " added to "+_deckType.ToString());
				PrintDeck();
			}
		}
		public void AddDeck(List<Card> cs, bool shuffleIntoDeck = true, int insertIndex = 0)
		{
			if (shuffleIntoDeck)
			{
				foreach (Card c in cs)
					_ideas.Insert(UnityEngine.Random.Range(0, _ideas.Count), c);
			}
			else
			{
				if (insertIndex < 0)
					_ideas.AddRange(cs);
				else
				{
					foreach (Card c in cs)
						_ideas.Insert(Mathf.Clamp(insertIndex, 0, _ideas.Count), c);
				}

			}
		}
		public void Purge(CombatCharacter owner)
		{
			foreach (Card c in _ideas)
				c.Purge();
		}

		public void Shuffle()
		{
			List<Card> temp = new List<Card>();
			temp.AddRange(_ideas);
			_ideas.Clear();
			while (temp.Count > 0)
			{
				int r = UnityEngine.Random.Range(0, temp.Count);
				_ideas.Add(temp[r]);
				temp.RemoveAt(r);

			}
		}
		public void Clear()
		{
			_ideas.Clear();
		}
		public void PrintDeck()
		{
			Debug.Log("-------------");
			Debug.Log("Printing "+_deckType+" deck");
			for (int i = 0; i < _ideas.Count; i++)
			{
				Debug.Log("Card " + i + ": " + _ideas[i]._name + " with cardID " + _ideas[i]._id);
			}
			Debug.Log("-------------");
		}
		public void CombineCards(List<Card> cards) {
			if (cards.Count > 0)
			{
				Card newc = new Card(cards[0]);
				cards.RemoveAt(0);
				//All of the text will be parsed
				//newc._artSprite=
				foreach (Card c in cards)
				{
					#region Targeting
					if (!newc._target.HasFlag(BaseCard.Target.Self) && c._target.HasFlag(BaseCard.Target.Self))
						c._target += 1;
					if (!newc._target.HasFlag(BaseCard.Target.Friend) && c._target.HasFlag(BaseCard.Target.Friend))
						c._target += 2;
					if (!newc._target.HasFlag(BaseCard.Target.Enemy) && c._target.HasFlag(BaseCard.Target.Enemy))
						c._target += 4;
					if (!newc._target.HasFlag(BaseCard.Target.Object) && c._target.HasFlag(BaseCard.Target.Object))
						c._target += 8;
					if (!newc._target.HasFlag(BaseCard.Target.Ground) && c._target.HasFlag(BaseCard.Target.Ground))
						c._target += 16;
					//if (newc._range.min > c._range.min) newc._range.min = c._range.min;
					//if (newc._range.max < c._range.max) newc._range.max = c._range.max;
					if (!c._randomTarget) newc._randomTarget = false;
					if (newc._multiTarget < c._multiTarget) newc._multiTarget = c._multiTarget;
					#endregion
					#region Effects
					newc._effects._effects.AddRange(c._effects._effects);
					newc._effects._requirements.Clear();
					newc._effects._onHoldEffects.AddRange(c._effects._onHoldEffects);
					#endregion
					#region Features
					if (c._features._playOnDraw==false) newc._features._playOnDraw = false;
					if (c._features._unplayable == false) newc._features._unplayable = false;
					if (c._features._repeating == true) newc._features._repeating = true;
					if (c._features._burn == true) newc._features._burn = true;
					if (c._features._volatile == true) newc._features._volatile = true;
					if (c._features._fumble == true) newc._features._fumble = true;
					//newc._features._charges += c._features._charges;
					#endregion
					newc._description += c.GetDescription();
				}
			
			}
			else Debug.LogError("Trying to combine zero cards");
		}
		public enum DeckType { Draw, Discard, Deplete, Hand}
		public DeckType _deckType;
	}
	public class Card : BaseCard
	{

		static List<int> _hasteEffectIDs = new List<int> { 25, 26 };
		//id, name and description+
		BaseCard _base;
		public bool _playedThisTurn = false;
		public bool _onRepeat;
		CombatCharacter _owner;
		public CombatCharacter GetOwner()
		{
			return _owner;
		}
		public CombatGUICard _gUICard;
		public int _roll = 0;
		public Damage _damage;
		public bool _visible = false;
		public bool _combined = false;
		public bool _hasHaste = false;
		bool _alreadyInited = false;
		public bool _autoPlay = false;
		public bool _specialActionCard = false;
		public bool _dealsDamage { get; private set; } = false;
		public bool _isHostile { get; private set; } = false;
		public string _descriptionFinal;
		public void Init(BaseCard bc)
		{
			_base = bc;
			CopyBaseVariables();
			CheckIfHasHaste();
			_dealsDamage = DealsDamage();
			_isHostile = _dealsDamage ? true : CheckHostility();
			SetAutoPlay();
			void CopyBaseVariables()
			{
				if (!_alreadyInited)
				{
					_base.LoadArtSprite();
					
				}
				CodeTools.CopyBaseFields(_base, this, typeof(BaseCard));
				_alreadyInited = true;
			}
			void CheckIfHasHaste()
			{
				_hasHaste = false;
				if (_effects._effects == null)
					return;
				foreach (EffectTarget et in _effects._effects)
				{
					if (et._effect == null)
						continue;
					if (_hasteEffectIDs.Contains(et._effect._id))
						_hasHaste = true;
				}
			}
			bool DealsDamage()
			{

				if (_effects._effects == null || _effects._effects.Count == 0)
					return false;
				foreach (EffectTarget be in _effects._effects)
				{
					if (be._effect == null)
						continue;
					if (be._effect._dealsDamage)
						return true;
			}
				return false;
			}
			bool CheckHostility()
			{
				return false;
			}
		}
		void SetAutoPlay()
		{
			if (!GameSettings.CombatSettings._fastTargeting)
			{
				_autoPlay = false;
				return;
			}
			if (_multiTarget == BaseCard.MultiTargetType.All || _randomTarget || TargetOnlySelf())
			{
				_autoPlay = true;
			}
			else
				_autoPlay = false;
		}
		public int GetHasteEffect(CombatCharacter target)
        {
			int hasteAmount = 3;
			if (!_hasHaste)
				return 0;
            else
            {
				foreach (EffectTarget et in _effects._effects)
				{
					int i = GetHasteEffectFromET(et);
					if (i != 0) return i;
				}
			}
			if (_debugLog)
				Debug.Log("Returning 0 when targeting " + target.GetName());
			return 0;
			int GetHasteEffectFromET(EffectTarget et)
            {
				hasteAmount = Mathf.RoundToInt(hasteAmount * et._powerMultiplier);
				if (et._target == TargetType.Self && target == _owner)
				{
					if (et._effect._id == _hasteEffectIDs[0])
						return hasteAmount;
					else if (et._effect._id == _hasteEffectIDs[1])
						return hasteAmount*-1;
				}
				else if (et._target != TargetType.Self && target != _owner)
				{

					if (et._effect._id == _hasteEffectIDs[0])
						return hasteAmount;
					else if (et._effect._id == _hasteEffectIDs[1])
						return hasteAmount*-1;
				}
				return 0;
			}
        }
		public Card(BaseCard bc)
		{
			Init(bc);
		}
		public Card(BaseCard bc, bool specialActionCard)
		{
			_specialActionCard = true;
			Init(bc);
		}
	
		public void Purge()
		{
			Init(_base);
		}

		public bool CheckPlayable()
		{
			if (_owner == null)
			{
				Debug.LogError(_name + " owner is null, card is not playable!");
				return false;
			}
			return CheckSelfRequirements();
		}
		bool CheckSelfRequirements()
		{
			if (_features._unplayable || (_owner._actions.current <= 0 && !_features._free))
				return false;
			else if (_effects._requirements == null || _effects._requirements.Count == 0)
				return true;
			else
			{
				foreach (RequirementTarget req in _effects._requirements)
				{
					if (req._target == TargetType.Self && req._requirement.InstantCheck(_owner) == false)
						return false;
				}
			}
			return true;
		}
		public bool CheckRequirements(CombatCharacter target, CombatCharacter source = null)
		{
			if (_features._unplayable && _owner._actions.current <= 0)
				return false;
			else if (_effects._requirements == null || _effects._requirements.Count == 0)
				return true;
			else
			{
				bool temp = true;
				foreach (RequirementTarget req in _effects._requirements)
				{
					if (req._requirement.InstantCheck(req._target == TargetType.Target ? target : source != null ? source : target) == false)
						temp = false;
				}
				return temp;
			}
		}
		public bool RequirementsAreInstant()
		{
			foreach (RequirementTarget br in _effects._requirements)
				if (!br._requirement.IsInstant())
					return false;
			foreach (EffectTarget et in _effects._effects)
				if (!et._requirement._requirement.IsInstant())
					return false;
			return true;
		}
		struct DelayedResult
		{
			public int _result;
			public BaseRequirement _baseRequirement;
			public DelayedResult(BaseRequirement br)
			{
				_baseRequirement = br;
				_result = 0;
			}
			public DelayedResult(BaseRequirement br, int result)
			{
				_baseRequirement = br;
				_result = result;
			}
			public void SetResult(int i)
			{
				_result = i;
			}
		}
		public void SetOwner(CombatCharacter owner)
		{
			_owner = owner;
			switch (owner._alliance)
			{
				case CombatCharacter.Alliance.Enemy:
					//_visible = false;
					break;
				default:
					//_visible = false;
					break;
				case CombatCharacter.Alliance.Friendly:
				case CombatCharacter.Alliance.Player:
					_visible = true;
					break;
			}
			_visible = true; //Just for testing!
		}
		public void OnThink(CombatCharacter owner)
		{
			SetOwner(owner);
			foreach (EffectTarget et in _effects._onThinkEffects)
			{
				owner.StartCoroutine(et._effect.Activate(owner, owner, et._powerMultiplier));
				//_owner.AddAura(new BaseEffect.ApplyAura(ba), _owner);
			}
		}
		public void OnForget()
		{
			foreach (EffectTarget et in _effects._onForgetEffects)
			{
				_owner.StartCoroutine(et._effect.Activate(_owner, _owner, et._powerMultiplier));
			}
		}
		public void OnRepress()
		{
			foreach (EffectTarget et in _effects._onRepressEffects)
			{
				_owner.StartCoroutine(et._effect.Activate(_owner, _owner, et._powerMultiplier));
			}
		}
		public void InventionCombination(Card c)
		{
			if (!c._cardClass.HasFlag(CardClass.Inventor) || !_cardClass.HasFlag(CardClass.Inventor))
				Debug.LogError("Trying to Combine non-inventor cards");
			else
			{
				int r = UnityEngine.Random.Range(0, 2);
				if (r == 0)
					_name = _nameCombinationAdjective +" "+ c._nameCombinationNoun;
				else
					_name = c._nameCombinationAdjective +" "+ _nameCombinationNoun;
			}
			_combined = true;
			#region Features
			if (!c._features._burn)
				_features._burn = false;
			if (!c._features._fumble)
				_features._fumble = false;
			if (!c._features._volatile)
				_features._volatile = false;
			if (c._features._repeating)
				_features._repeating = true;
			#endregion
			CombineEffects(c._effects);
			//Description?!?!?!
			/*
			if (_range.min > c._range.min)
				_range.SetMin(c._range.min);
			if (_range.max < c._range.max)
				_range.SetMax(c._range.max);
			*/
			if (c._multiTarget > _multiTarget)
				_multiTarget = c._multiTarget;
			if (c._cardType == CardType.Attack)
				_cardType = CardType.Attack;
			_target |= c._target; //Works?

			if (_gUICard)
				_gUICard.UpdateTexts();
		}
		public void CombineEffects(Effects effects, bool automateDescription = false)
		{
			foreach (EffectTarget be in effects._effects)
			{
				_description += "\n" + be._effect._description;
				_effects._effects.Add(be);
			}
			if (effects._effects.Count > 0)
			{
				UpdateDescription(CodeTools.db);
				if(_gUICard)
				{
					_gUICard.UpdateTexts(_owner);
				}
			}
		}
		public bool GetSpecialState()
		{
			if (_onRepeat)
				return true;
			else
				return false;
		}
		List<int> _damages = new List<int>();
		public void SetDamageSlots(int count)
		{
			_damages = new List<int>();
			for (int i = 0; i < count; i++)
				_damages.Add(-999);
		}
		public void DamageEstimation(int damage, int index)
		{
			_damages[index] = damage;
			if (_gUICard)
			{
				foreach (int dmg in _damages)
					if (dmg == -999)
						return;
				_gUICard.UpdateTexts(_damages);
			}
		}
		public bool DisplayMitigatedDamage()
		{
			if (!_owner)
				return true;
			else
				return _owner._alliance != CombatCharacter.Alliance.Enemy;
		}
		public float GetGeneralAttraction()
		{
			
			float attraction = 0; 
			if (_features._fumble) attraction += 0.33f;
			if (_features._burn) attraction -= 0.33f;
			if (_owner._alliance == CombatCharacter.Alliance.Enemy && _visible)
				attraction += 0.15f;

			return attraction;
		}
		public float GetAttractionOnTarget(CombatCharacter target)
		{
			float attraction = 0;
			foreach(EffectTarget et in _effects._effects)
			{
				if (et._requirement.InstantCheck(_owner, target))
				{
					if (et._effect._applyAura._aura != null)
					{
						//if target already has the aura, don't add to attractiveness
						BaseAura ba = et._effect._applyAura._aura;
						if (target.HasAura(ba))
							continue;
					}
					attraction += et._effect._attraction * et._powerMultiplier;
				}
			}
			if (_dealsDamage) {  
				if( target.IsHostileTowards(_owner) && target.HasAura(11))
					attraction *= 1.5f;
				if (_owner.HasAura(12))
					attraction *= 1.5f;
			}
			return attraction;
		}
	}
	public class Aura : BaseAura
	{
		public BaseAura _base;
		public int _cases = 0;
		public CombatCharacter _source;
		public CombatCharacter _host;
		public List<CombatEventSystem.DataFormat> _eventTriggers = new List<CombatEventSystem.DataFormat>();
		public Aura(BaseEffect.ApplyAura aa, CombatCharacter host, CombatCharacter source)
		{
			_host = host;
			_source = source;
			_base = aa._aura;
			CodeTools.CopyBaseFields(_base, this, typeof(BaseAura));
			_duration = aa._duration;
			_cases = aa._cases;
			SetDescription();
		}
		public Aura(Talent t)
		{
			_host = t._cc;
			_source = t._cc;
			_description = t._description;
			_name = t._name;
			_sprite = t._auraIcon;
			SetDescription();
			_cases = 0;
			_duration = 0;
			_visible = true;
			_polarity = t._polarity;
			_stackingType = StackingType.None;
		}
		void SetDescription()
		{
			string desc = _description;

			if (desc.Contains("[source]"))
				desc = desc.Replace("[source]", _source.GetName());
			if (desc.Contains("[source's]"))
				desc = desc.Replace("[source's]", _source.GetName()+"'s");
			if (desc.Contains("[host]"))
				desc = desc.Replace("[host]",_host.GetName());
			if (desc.Contains("[host's]"))
				desc = desc.Replace("[host's]", _host.GetName() + "'s");
			//numbers?
			if (!desc.EndsWith(".") && !desc.EndsWith("!") && !desc.EndsWith("?"))
				desc = desc + ".";
			_actualDescription = Capitalize(desc);
			_actualDescription = Translator.PrepGenericText(_actualDescription);


			_actualDescription = SetNumbersInDescription(_actualDescription);
			string SetNumbersInDescription(string str)
			{
				while (str.Contains("{") && str.Substring(str.IndexOf("{")).Contains("}"))
				{
					int startIndex = str.IndexOf("{");
					string stuff = str.Substring(startIndex);
					int endIndex = stuff.IndexOf("}") + 1;
					stuff = stuff.Substring(0, endIndex);
					int begin = 0;
					
					for (int i = 0; i < str.Length; i++)
					{
						if (char.IsNumber(stuff.ToCharArray()[i]))
						{
							begin = i;
							break;
						}
					}
					int id = -1;
					int number = 0;
					if (int.TryParse(stuff.Substring(6, stuff.Substring(begin).IndexOf(",")), out id))
					{
						float power = 0;
						string powerstring = stuff.Substring(stuff.IndexOf(",") + 1);
						powerstring = powerstring.Substring(0, powerstring.Length - 1);
						if (float.TryParse(powerstring, out power))
						{
							BaseEffect be = (BaseEffect)CodeTools.db.Get<BaseEffect>(id);
							number = be.GetNumberQuick(power,_source);
						}
						else Debug.LogError("Invalid format when parsing. \n"+stuff);
					}
					else Debug.LogError("Invalid format when parsing. \n" + stuff);
					str = str.Substring(0, startIndex) + Translator._cardKeyword + number + Translator._close + str.Substring(startIndex + endIndex);

				}
				return str;
			}
		}
		public void AddStacks(int i)
		{
			i = Mathf.Abs(i);
			if (_stackingType == StackingType.Case)
			{
				ModOwnerStats(false);
				_cases += i;
				ModOwnerStats(true);
			}
			else if (_stackingType == StackingType.Duration)
				_duration += i;

		}

		public void ModOwnerStats(bool addition)
		{
			int multiplier = addition ? 1 : -1;
			multiplier *= _cases;
			_host._stats.armor.Add(_armorMod * multiplier, false);
		}
		public void RemoveDuration(int i = 1)
		{
			_duration-= i;
			if (_duration < 0)
				Remove();
		}
		public void RemoveCases(int i = 1)
		{
			_cases -= i;
			if (_cases < 0)
				Remove();
		}
		public void AddTrigger(CombatEventSystem.EventTrigger evtr, CombatEventSystem.EventTrigger.Delegate deleg = default)
		{
			if (deleg == default)
				deleg = Tick;
			evtr._delegates += deleg;
			_eventTriggers.Add(new CombatEventSystem.DataFormat(evtr, deleg));
		}
		public void Trigger(object obj)
		{
			if (_debugLog)
				Debug.Log(GetFileName() + " triggered");
			if (_requireSpecialCode)
			{
				switch (_id)
				{
					case 0:
						//Debug.Log("Reaction Trigger!");
						_cases--;
						break;
					case 1:
						if (obj.GetType() == typeof(CombatCharacter))
						{
							CombatCharacter cc = (CombatCharacter)obj;
							if (cc == _host)
								return;
							_host.StartCoroutine(_tickEffects[0].effect.Activate((CombatCharacter)obj, _host, _tickEffects[0].powerMultiplier));
							Debug.Log("TODO: Riposte text or animation");
						}
						break;
					default:
						Debug.LogError("Unknown Aura ID [" + _id + "] on " + _host.GetName());
						break;
					case 17:
						//017 - Dazed
						_cases--;
						_host._actions.current -= 1;
						break;
					case 19:
						//019 - Reloading
						_duration--;
						if (_duration < 0)
							_host._inventory._ranged.Reload();
						break;
				}
			}
			else
			{
				if(_tickType == TickType.trigger)
					switch (_stackingType)
					{
						case StackingType.Case:
							_cases--;
							break;
						case StackingType.Duration:
							_duration--;
							break;
						case StackingType.None:
							break;
					}
			}
			if (_cases <= 0 || _duration <= 0)
			{
				Remove();
			}
			_host._cSprite._statusBar.RefreshAuras();
		}
		public void Tick(object obj)
		{
			if (_triggerOnTick)
				Trigger(obj);
			if (_requireSpecialCode)
			{
				switch (_id)
				{

					default:
						//Debug.LogError("Unknown Aura ID [" + _id + "] on " + _host.GetName());
						DefaultTick();
						break;
				}
			}
			else
				DefaultTick();
			if (_cases <= 0 || _duration <= 0)
			{
				Remove();
			}
			_host._cSprite._statusBar.RefreshAuras();
			void DefaultTick()
			{
				if (_tickType != TickType.trigger)
					_duration--;
			}
		}
		public void Remove(bool ignoreExpiration = false)
		{
			if (!ignoreExpiration)
			{
				if (_requireSpecialCode)
				{
					switch (_id)
					{
						case 1:
						case 0:
							break;
						default:
							//Debug.LogError("Unknown Aura ID [" + _id + "] on " + _host.GetName());
							break;
					}
				}
			}
			foreach(CombatEventSystem.DataFormat data in _eventTriggers)
			{
				data.Clear();
			}
			_host._auras.Remove(this);
		}
		public void DebugListEventTriggers()
		{
			if (_debugLog)
				foreach (CombatEventSystem.DataFormat evtr in _eventTriggers)
					Debug.Log("Trigger:-" + evtr._eventTrigger._name + "- for " + evtr._delegate.Method.Name + " of " + _name);
		}

		public void ModDamageOutgoing(object damage)
		{
			if (damage.GetType() != typeof(Damage))
				return;
			Damage dmg = (Damage)damage;
			dmg._damageFlat += _damageModFlat;
			dmg._damageMultipliers.Add(_damageModFractional);
			if (_tickType == TickType.trigger && dmg._estimation == false)
				Trigger(damage);
		}

		public void ModDamageIncoming(object damage)
		{
			if (damage.GetType() != typeof(Damage))
				return;
			Damage dmg = (Damage)damage;
			dmg._damageFlat += _damageModIncomingFlat;
			dmg._damageMultipliers.Add(_damageModIncomingFractional);
			if (_tickType == TickType.trigger && dmg._estimation == false)
				Trigger(damage);

		}
	}
	public class Talent : BaseTalent
	{
		public CombatCharacter _cc { get; private set; }
		public BaseTalent _base { get; private set; }
		public Bint _chargesPerCombat;
		public Bint _chargesPerDay;
		public Talent(BaseTalent bt, CombatCharacter cc)
		{
			_base = bt;
			_cc = cc;
			CopyBaseFields(bt, this, typeof(BaseTalent));
			if (_charges)
			{
				_chargesPerCombat = new Bint(_chargesPerCombat_base);
			}
		}
		public void Trigger(object obj)
		{
			if (_debugLog) Debug.Log(_cc.GetName()+" attempting to trigger talent " + _name);
			CombatCharacter target = null;
			if (obj.GetType() == typeof(Damage))
			{
				Damage dmg = (Damage)obj;
				if (dmg._source == _cc)
					target = dmg._target;
				else if (dmg._target == _cc)
					target = dmg._source;
			}
			else if (obj.GetType() == typeof(CombatCharacter))
			{
				target = (CombatCharacter)obj;
			}
			if (PassRequirements())
			{
				if (_debugLog) Debug.Log(_cc.GetName() + " met requirements to trigger talent " + _name);
				if (_requireSpecialCode)
				{
					switch (_id)
					{
						case 10:
							if (obj.GetType() == typeof(Damage))
							{
								Damage dmg = (Damage)obj;
								dmg._damageMultipliers.Add(0.5f);
							}
							break;
						case 7://Revenge Fantasy
						case 8://Insult and Injury
						case 13://Defense Mechanism
							if (obj.GetType() == typeof(Damage))
							{
								Damage dmg = (Damage)obj;
								dmg._damageMultipliers.Add(1.5f);
							}
							break;
					}
				}
				if (obj.GetType() == typeof(Damage))
				{
					Damage dmg = (Damage)obj;
					if (dmg._estimation)
						return;
				}
				foreach (BaseCard.EffectTarget et in _effects)
				{
					if (!PassEffectRequirements(et))
						continue;
					if (et._target == BaseCard.TargetType.Target && obj.GetType() == typeof(CombatCharacter))
					{
						et._effect.Activate((CombatCharacter)obj, _cc, et._powerMultiplier);
					}
					else if (et._target == BaseCard.TargetType.Self)
					{
						_cc.StartCoroutine(et._effect.Activate(_cc, _cc, et._powerMultiplier));
					}
				}
			}
			bool PassEffectRequirements(BaseCard.EffectTarget et)
			{
				if (et._requirement._requirement == null)
					return true;
				if (et._requirement._target == BaseCard.TargetType.Target && target == null) {
					Debug.LogError("Unable to determine target on requirement of "+_name+" of "+_cc);
					return false;
				}
				return et._requirement._requirement.InstantCheck(et._requirement._target == BaseCard.TargetType.Target ? target : _cc);
			}
			bool PassRequirements()
			{
				bool pass = true;
				switch (_trigger)
				{
					case TalentTrigger.YouAreInjured:
						pass = true;
						Debug.Log("<color=purple>YouAreInjured not implemented!</color>");
						break;
					case TalentTrigger.YouDealDamage:
					case TalentTrigger.YouTakeDamage:
					case TalentTrigger.YouWouldDie:
						//obj should be damage
						if (obj.GetType() == typeof(Damage))
						{
							Damage dmg = (Damage)obj;
							if (_requireSpecialCode)
							{
								switch (_id)
								{
									default:
										break;
									case 7:
										//Revenge Fantasy
										if (dmg._source == _cc)
										{
											pass = _requirements[0].RetaliationCheck(dmg);
										}
										else
											pass = false;
										break;
									case 8:
										//Insult and Injury
										pass = false;
										if (!dmg._target || !dmg._source)
											break;
										if (dmg._source == _cc)
										{
											BaseAura provoke = (BaseAura)db.Get<BaseAura>(13);
											if (dmg._target.HasAura(provoke))
												pass = dmg._target.GetAura(provoke)._source == _cc;
										}
										break;
									case 13:
										//Defense Mechanism
										foreach (BaseRequirement req in _requirements)
										{
											if (!req.InstantCheck(target))
												pass = false;
										}
										break;
								}
							}
							else
							{
								foreach (BaseRequirement req in _requirements)
								{
									if (req._requireSpecialCode && !req.HardCodeCheck(dmg))
										pass = false;
									else if (!req.InstantCheck(target))
										pass = false;
								}
							}
						}
						else
							Debug.LogError("Wrong object type on " + GetFileName());
						break;
					case TalentTrigger.ActivateEffect:
						//obj should be an effect
						if (_triggerEffect == null)
						{
							Debug.LogError("Unassigned TriggerEffect on "+GetFileName()+"!");
						}
						if (obj.GetType() == typeof(BaseEffect))
						{
							BaseEffect be = (BaseEffect)obj;
							if (be != _triggerEffect)
								pass = false;
						}
						else
							pass = false;
						break;
					case TalentTrigger.YouDefeatAnEnemy:
						//obj should be the killed target
						if (obj.GetType() == typeof(CombatCharacter)) {
							CombatCharacter target = (CombatCharacter)obj;
							if (target != _cc)  //rule out suicide?
							{
								pass = true;
							}
							else
								pass = false;
						}
						else
							Debug.LogError("Wrong object type on " + GetFileName());
						break;
					case TalentTrigger.CombatStarts:
					case TalentTrigger.CombatEnds:
					case TalentTrigger.YourTurnEnds:
					case TalentTrigger.YourTurnStarts:
						//obj should be self
						break;

					case TalentTrigger.YouThink:
					case TalentTrigger.YouRepressAnIdea:
					case TalentTrigger.YouForgetAnIdea:
					case TalentTrigger.YouActualizeAnIdea:
						//obj should be a card
						break;
				}
				return pass;
			}
		}
		public void AddCustomTrigger(CombatEventSystem ces)
		{
			switch (_id)
			{
				default:
					Debug.Log("No hardcode for " + _name + " - " + _id);
					break;
				case 5: //retaliate
					ces._OnPrepareOutgoingDamage._delegates += Trigger;
					break;
			}
		}
	}
	public class Damage
	{
		public int id;
		public CombatCharacter _target;
		public CombatCharacter _source;
		public int _damageFlat = 0;
		public int _damageMitigationFlat = 0;
		public float _spellPowerMultiplier = 1f;
		public List<float> _damageMultipliers = new List<float>();
		public DamageType _damageType;
		public enum DamageType { None, Physical, Magical }
		public BaseEffect _baseEffect;
		Card _card;
		public bool _estimation = false;
		public int _descriptionIndex = 0;
		//bool locked = false;
		public Damage(BaseEffect baseEffect,  CombatCharacter source, CombatCharacter target,Card card = null, float damageMultiplier = 1)
		{
			_spellPowerMultiplier = damageMultiplier;
			Init(baseEffect, source, target, card);
			id = Random.Range(0, 10000);
			//Dbug();
		}
		public Damage(CombatCharacter source, CombatCharacter target = null)
		{
			_damageFlat = 0;
			_source = source;
			if (target == null)
				_target = _source;
			else
				_target = target;
		}
		void Init(BaseEffect baseEffect,  CombatCharacter source = null, CombatCharacter target = null, Card card = null)
		{
			_damageFlat = 0;
			_damageType = DamageType.Physical;
			_source = source;// ? source : null;
			_target = target;// ? target : null;
			_baseEffect = baseEffect;
			_damageMultipliers = new List<float>();
			_damageMitigationFlat = 0;
			if (_source != null)
				_damageFlat += _source.EstimateDamageQuick(_baseEffect, _spellPowerMultiplier);
			if (card != null)
			{
				_card = card;
				_estimation = true;
				if(_source)_source.BeginEstimateDamageReal(this);
			}
		}
		public void CompleteEstimation()
		{
			if (_card != null)
			{
				//Debug.Log("Damage = " + GetDamage());
				_card.DamageEstimation(GetDamage(_card.DisplayMitigatedDamage()),_descriptionIndex);
				//if (_source!= null && _source._cardPlayDamages!=null) _source._cardPlayDamages.Add(this);
			}
			else
			{
				//Debug.Log("wtf no card?");
			}
		}
		public int GetDamage(bool mitigate = true)
		{
			//Damageflat * sum of positive and negative damageMultipliers - damageMitigationFlat
			int temp = _damageFlat;
			if (_damageMultipliers.Count > 0)
			{
				float damageFloat = temp;
				foreach (float f in _damageMultipliers)
				{
					if (!mitigate && f < 1)
						continue;
					damageFloat *= f;
				}
				temp = Mathf.RoundToInt(damageFloat);
			}
			if(mitigate)temp -= _damageMitigationFlat;
			return Mathf.Clamp(temp,-9999,9999);
		}
		public void RemoveTarget()
		{
			//Debug.Log("RemoveTarget()");
			Init(_baseEffect, _source, null, _card);
		}
		public void AddDamage(Damage dmg)
		{
			//Debug.Log("Added \n" + dmg.GetInfo() + "\n to \n" + GetInfo());
			_damageFlat += dmg.GetDamage();
		}

		public void Dbug()
		{
			Debug.Log(GetInfo());
			string s = _damageFlat.ToString();
			foreach (float f in _damageMultipliers)
				s += " * " + f.ToString("0.00");
			if (_damageMitigationFlat != 0)
				s += " - " + _damageMitigationFlat;
			s += " = " + GetDamage();
			Debug.Log("<color=red>Damage: " + s + "</color>");
		}

		public string GetInfo()
		{
			return "<color=red>id=" + id + ". " + GetDamage() + " damage from " + (_source != null ? _source.GetName() : "null") + " to " + (_target != null ? _target.GetName() : "null") + "</color>";
		}
	}
	public class Inventory
	{
		public CombatCharacter _cc { get; private set; }
		public int _maxCapacity = 20;
		public List<Item> _items = new List<Item>();
		public List<Weapon> _weapons = new List<Weapon>();
		public List<Consumable> _consumables = new List<Consumable>();
		public List<Equipment> _equipment = new List<Equipment>();
		public Weapon _melee;
		public Weapon _ranged;
		public Equipment _armor;
		public Equipment _ring;
		public Equipment _amulet;
		public Equipment _talisman;
		public Inventory(CombatCharacter cc)
		{
			_cc = cc;
		}
		public bool AddItem(object item)
		{
			if (IsRoomForMore())
			{
				System.Type t = item.GetType();
				if (t == typeof(BaseItem))
					_items.Add(Item.Create((BaseItem)item, _cc));
				else if (t == typeof(BaseConsumable))
					_consumables.Add(Consumable.Create((BaseConsumable)item, _cc));
				else if (t==typeof(BaseWeapon))
					_weapons.Add(Weapon.Create((BaseWeapon)item, _cc));
				else if (t == typeof(BaseEquipment))
					_equipment.Add(Equipment.Create((BaseEquipment)item, _cc));
				return true;
			}
			return false;
		}
		/*
		public bool AddItem(BaseItem i)
		{
			if (IsRoomForMore())
			{
				Item it = (Item)ScriptableObject.CreateInstance(typeof(Item));
				it.Init(i);
				_items.Add(it);
				//_items.Add(new Item(i));
				return true;
			}
			return false;
		}
		public bool AddItem(BaseConsumable i)
		{
			if (IsRoomForMore())
			{
				Consumable it = (Consumable)ScriptableObject.CreateInstance(typeof(Consumable));
				it.Init(i);
				_consumables.Add(it);
				return true;
			}
			return false;
		}
		public bool AddItem(BaseWeapon i)
		{
			if (IsRoomForMore())
			{
				_weapons.Add(Weapon.Create(i));
				return true;
			}
			return false;
		}
		public bool AddItem(BaseEquipment i)
		{
			if (IsRoomForMore())
			{
				Equipment it = (Equipment)ScriptableObject.CreateInstance(typeof(Equipment));
				it.Init(i);
				_equipment.Add(it);
				return true;
			}
			return false;
		}*/
		bool IsRoomForMore()
		{
			return GetCurrentCapacity() < _maxCapacity;
		}
		public int GetCurrentCapacity()
		{
			return _equipment.Count + _weapons.Count + _consumables.Count + _items.Count;
		}
		public void RemoveItem(BaseItem i)
		{

		}
		public FieldInfo GetEquipmentFieldFromType(BaseEquipment.EquipmentType equipmentType)
		{
			if (equipmentType == BaseEquipment.EquipmentType.None)
				return null;
			else
				return GetType().GetField("_" + equipmentType.ToString().ToLower());
		}
		public BaseEquipment GetEquipmentFromSlot(BaseEquipment.EquipmentType equipmentType)
		{
			BaseEquipment prevEq = null;
			switch (equipmentType)
			{
				case BaseEquipment.EquipmentType.None:
					break;
				case BaseEquipment.EquipmentType.Armor:
					prevEq = _armor;
					break;
				case BaseEquipment.EquipmentType.Ring:
					prevEq = _ring;
					break;
				case BaseEquipment.EquipmentType.Amulet:
					prevEq = _amulet;
					break;
				case BaseEquipment.EquipmentType.Talisman:
					prevEq = _talisman;
					break;
				case BaseEquipment.EquipmentType.Ranged:
					prevEq = _ranged;
					break;
				case BaseEquipment.EquipmentType.Melee:
					prevEq = _melee;
					break;

			}
			return prevEq;
		}
		public void Equip(object obj)
		{
			BaseEquipment beq;
			try
			{
				beq = (BaseEquipment)obj;
			}
			catch
			{
				Debug.LogError(_cc.GetName() + " equipping invalid type");
				return;
			}
			FieldInfo _slot = GetEquipmentFieldFromType(beq._equipmentSlot);
			if(_slot.GetValue(this)!=null)
			{
				BaseEquipment prevEq = (BaseEquipment)_slot.GetValue(this);
				if (prevEq != null)
					Unequip(prevEq);
			}
			//Debug.Log("Slot type: "+_slot.FieldType.Name + "\n obj type = "+obj.GetType().Name);
			if (obj.GetType() == typeof(Equipment))
			{
				Equipment eq = (Equipment)obj;
				_slot.SetValue(this,eq);
			}
			else if(obj.GetType() == typeof(Weapon))
			{
				Weapon wep = (Weapon)obj;
				_slot.SetValue(this,wep);
				
			}
			else
			{
				Debug.LogError("Obj wrong type " + obj.GetType().Name);
			}
			foreach (BaseTalent bt in beq._talents)
			{
				//add extra talents
			}
			foreach (BaseCard bc in beq._addCards)
			{
				//add extra cards
			}
		}
		public bool Unequip(object obj)
		{
			BaseEquipment beq;
			try
			{
				beq = (BaseEquipment)obj;
			}
			catch
			{
				Debug.LogError(_cc.GetName() + " unequipping invalid type");
				return false;
			}
			try
			{
				GetEquipmentFieldFromType(beq._equipmentSlot).SetValue(this, null);
				foreach (BaseTalent bt in beq._talents)
				{
					
					
				}
				foreach (BaseCard bc in beq._addCards)
				{
					//remove extra cards
				}
				return true;
			}
			catch
			{
				Debug.LogError("Fatal error when unequipping " + beq._name);
				return false;
			}
		}
		/*
		public void Equip(object obj)
		{
			if(obj.GetType() == typeof(Equipment))
			{
				Equipment eq = (Equipment)obj;
				eq.SetOwner(_cc);
			}
			else if(obj.GetType() == typeof(Weapon))
			{
				Weapon wpn = (Weapon)obj;
				wpn.SetOwner(_cc);
				if (wpn._equipmentSlot == BaseEquipment.EquipmentType.Melee)
				{
					if (_melee != null)
					{
						//Unequip first

					}
					_melee = wpn;
				}
				else if (wpn._equipmentSlot == BaseEquipment.EquipmentType.Ranged)
				{
					if (_ranged!= null)
					{
						//Unequip first

					}
					_ranged = wpn;
				}
			}
		}
		*/
	}
	public class Item : BaseItem
	{
		BaseItem _base;
		public void Init(BaseItem baseItem)
		{
			_base = baseItem;
			CopyBaseVariables();
		}

		void CopyBaseVariables()
		{
			if (_base._artSprite == null)
			{
				_base._artSprite = Resources.Load<Sprite>("Card Art/Missing");
			}
			CodeTools.CopyBaseFields(_base, this, typeof(BaseItem));

		}
		public string GetTypeName()
		{
			return "Valuable";
		}
		public static Item Create(BaseItem item, CombatCharacter owner)
		{
			Item it = (Item)ScriptableObject.CreateInstance(typeof(Item));
			it.Init(item);
			it._owner = owner;
			return it;
		}
	}
	public class Equipment : BaseEquipment
	{

		BaseEquipment _base;
		public void Init(BaseEquipment baseEquipment)
		{
			_base = baseEquipment;
			CopyBaseVariables();
		}

		void CopyBaseVariables()
		{
			if (_base._artSprite == null)
			{
				_base._artSprite = Resources.Load<Sprite>("Card Art/Missing");
			}
			CodeTools.CopyBaseFields(_base, this, typeof(BaseEquipment));

		}
		public string GetTypeName()
		{
			return _equipmentSlot.ToString();
		}
		public static Equipment Create(BaseEquipment be, CombatCharacter owner)
		{
			Equipment eq = (Equipment)ScriptableObject.CreateInstance(typeof(Equipment));
			eq.Init(be);
			eq._owner = owner;
			return eq;
		}
	}
	public class Consumable : BaseConsumable
	{
		BaseConsumable _base;
		public void Init(BaseConsumable baseConsumable)
		{
			_base = baseConsumable;
			CopyBaseVariables();
		}

		void CopyBaseVariables()
		{
			if (_base._artSprite == null)
			{
				_base._artSprite = Resources.Load<Sprite>("Card Art/Missing");
			}
			CodeTools.CopyBaseFields(_base, this, typeof(BaseConsumable));
		}
		public string GetTypeName()
		{
			return "Consumable";
		}
		public static Consumable Create(BaseConsumable bc, CombatCharacter owner)
		{
			Consumable con = (Consumable)ScriptableObject.CreateInstance(typeof(Consumable));
			con.Init(bc);
			con._owner = owner;
			return con;
		}
	}
	public class Weapon : BaseWeapon
	{
		BaseWeapon _base;
		public void Init(BaseWeapon baseItem)
		{
			_base = baseItem;
			CopyBaseVariables();
		}
		void CopyBaseVariables()
		{
			//Debug.Log("_base = " + _base._name);
			if (_base._artSprite == null)
			{
				_base._artSprite = Resources.Load<Sprite>("Card Art/Missing");
			}
			CodeTools.CopyBaseFields(_base, this, typeof(BaseWeapon));

		}
		public string GetWeaponDescription()
		{
			string s = "Price: " + _price + "\nDamage: " + _weaponDamage +"\n"+ GetDescription();
			return s;
		}
		public string GetTypeName()
		{
			return _equipmentSlot == EquipmentType.Ranged ? "Pistol" : "Sword";//_equipmentSlot.ToString() + "\nweapon";
		}
		public int _cooldownRemaining = 0;
		public bool ReadyToFire()
		{
			return _cooldownRemaining <= 0;
		}
		public void Reload()
		{
			_cooldownRemaining = 0;
			Aura reaction = _owner.GetAura((BaseAura)db.Get<BaseAura>(19));
			if (reaction != null)
			{
				reaction.Remove();
			}
		}
		public void Trigger()
		{
			if(_cooldownRemaining<_base._cooldown)
				_cooldownRemaining = _base._cooldown;
			if (_owner == null)
			{
				Debug.LogError("MISSING OWNER");
				return;
			}
			BaseAura reloadAura = (BaseAura)db.Get<BaseAura>(19);
			Aura a = _owner.GetAura(reloadAura);
			if (a!=null)
			{
				a._cases = _cooldownRemaining;
			}
			else
			{
				BaseEffect.ApplyAura aa = new BaseEffect.ApplyAura(reloadAura);
				aa._duration = _cooldownRemaining;
				aa._cases = 1;
				_owner.AddAura(aa, _owner, false);
			}
		}
		public static Weapon Create(BaseWeapon bw, CombatCharacter owner)
		{
			Weapon wpn = (Weapon)ScriptableObject.CreateInstance(typeof(Weapon));
			wpn.Init(bw);
			wpn._owner = owner;
			return wpn;
		}
		/*public Weapon(int damage)
	{
		_id = 0;
		_damage = damage;
		_name = "TestSword";
	}*/
	}
}