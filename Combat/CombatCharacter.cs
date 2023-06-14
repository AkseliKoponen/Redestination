using System;
using System.Collections;
using System.Collections.Generic;
using RD.DB;
using RD;
using UnityEngine;
using static RD.CodeTools;
using System.Reflection;

namespace RD.Combat
{
	public class CombatCharacter : MonoBehaviour
	{

		[NonSerialized] public CombatAI _ai;
		//public Sprite _spritePortrait;
		public DialogCharacter _dialogCharacter;
		public string _description { get; protected set; }
		public Alliance _alliance { get; protected set; }
		public ObjectName _name;
		public Sprite _turnIcon;
		public CombatCharacterVisuals _cSprite;
		[NonSerialized] public CombatSelectableObject _selectObject;
		public BaseCombatCharacter _baseCC { get; protected set; }
		//-------------Numbers
		public int _level { get; protected set; }
		public Attributes _attributes;
		[Tooltip("Contains: crit & grazeMultipliers, armor")]
		public Stats _stats;
		public GlobalStats _globalStats;
		public Trackers _trackers;
		[NonSerialized] public Bint _actions;
		public Decks _deck { get; protected set; }
		[NonSerialized] public Card _specialActionCard;
		[NonSerialized] public List<Aura> _auras = new List<Aura>();
		[NonSerialized] public List<Talent> _talents = new List<Talent>();
		//------------Position and Rotation
		[NonSerialized] public int _position;
		public Facing _facing;
		[NonSerialized] public bool _blockLineOfSight = true;
		//-----------Items
		public Inventory _inventory { get; protected set; }
		public CombatEventSystem _combatEvents;
		//-------status flags
		public StatusFlags _statusFlags = new StatusFlags();
		public struct StatusFlags
		{
			public bool _injured;
		}
		public struct Trackers
		{
			public bool _myTurn;
			public int _damageTaken;
			public List<CombatCharacter> _retaliationTargets;
			public int _cardsPlayed;
			public void ResetCombat()
			{
				_damageTaken = 0;
				ResetTurnStart();
				ResetTurnEnd();
			}
			public void ResetTurnEnd()
			{
				_retaliationTargets = new List<CombatCharacter>();
				_cardsPlayed = 0;
				_myTurn = false;
			}
			public void ResetTurnStart()
			{
				_myTurn = true;
				_cardsPlayed = 0;

			}
		}
		public class Stats
		{
			public float critMultiplier;
			public int passiveDraw;
			public Bint armor;
			public Bint hp;
			public Bint barrier;
			public int speed;
			public int initiative;
			public int maxHandSize;
			public Stats(int maxHP, Attributes attributes)
			{
				initiative = 0;
				passiveDraw = 2;
				critMultiplier = 2f;
				armor = new Bint(0);
				hp = new Bint(maxHP);
				barrier = new Bint(0);
				barrier.SetMax(hp.max, false);
				speed = 10 + attributes._dexterity.current;
				maxHandSize = 5 + attributes._intelligence.current / 3;
			}
			public Stats(Stats stats)
			{
				CopyBaseFields(stats, this, typeof(Stats));
			}
			public Stats()
			{
				initiative = 0;
				passiveDraw = 2;
				critMultiplier = 2f;
				armor = new Bint(0);
				hp = new Bint(0);
				barrier = new Bint(0);
				barrier.SetMax(hp.max, false);
				speed = 10;
				maxHandSize = 5;
			}
			public float GetCritMultiplier()
			{
				return critMultiplier;
			}
			public int HP()
			{
				return hp.current;
			}
			public int GetInitialSpeed()
			{
				return speed + initiative;
			}
		}
		public struct ObjectName
		{

			public string prefix;
			public string name;
			public string suffix;
			public string abbrev;
			public ObjectName(string BaseName, string prefx = "", string suffx = "")
			{
				prefix = prefx;
				name = BaseName;
				suffix = suffx;
				abbrev = "";
				GenerateAbbreviation();
			
			}
			public void GenerateAbbreviation()
			{
				int abbreviationLength = 6;
				string sfx = RemoveVowels(suffix).Replace(" ","");
				string n = name.Replace(" ", "");
				string str = n + sfx;
				if (str.Length > abbreviationLength)
					str = RemoveVowels(n);
				if (str.Length + sfx.Length > abbreviationLength)
				{
					if (sfx.Length > 1)
						sfx = sfx.Substring(sfx.Length - 1);
					str = str.Substring(0, abbreviationLength - sfx.Length) + sfx;
				}
				abbrev = str;
			}
		}
		public class GlobalStats
		{
			public int _talentPointsAvailable = 0;
			public GlobalStats(GlobalStats copy)
			{
				CopyBaseFields(copy, this, typeof(GlobalStats));
			}
			public GlobalStats()
			{
				_talentPointsAvailable = 0;
			}
		}
		public static string Abbreviate(string name, int abbreviationLength = 6, string suffx = "")
		{
			string sfx = RemoveVowels(suffx).Replace(" ", "");
			string n = name.Replace(" ", "");
			string str = n + sfx;
			if (str.Length > abbreviationLength)
				str = RemoveVowels(n);
			if (str.Length + sfx.Length > abbreviationLength)
			{
				if (sfx.Length > 1)
					sfx = sfx.Substring(sfx.Length - 1);
				str = str.Substring(0, abbreviationLength - sfx.Length) + sfx;
			}
			return str;
		}

		/// <summary>
		/// Create CombatCharacter from base or loaded character
		/// </summary>
		public CombatCharacter Initialize(object obj)
		{
			if (!_cSprite)
				_cSprite = GetComponent<CombatCharacterVisuals>();
			//SetLayer(transform, 9);
			bool isBase = true;
			if (obj.GetType().Equals(typeof(BaseCombatCharacter)))
				_baseCC = (BaseCombatCharacter)obj;
			else if (obj.GetType().Equals(typeof(Party.CombatCharacterInfo)))
			{
				Party.CombatCharacterInfo cci = (Party.CombatCharacterInfo)obj;
				_baseCC = cci.bcc;
				isBase = false;
			}
			else
			{
				Debug.LogError("Unsupported CC type ->" + obj.GetType().Name);
			}
			_alliance = _baseCC._characterType;
			_combatEvents = new CombatEventSystem(this);
			_name = new ObjectName(_baseCC._name);
			_trackers = new Trackers();
			_trackers.ResetCombat();
			_actions = new Bint(_alliance == Alliance.Player ? GameRules._actionsPerTurnPC : GameRules._actionsPerTurnNPC);
			_inventory = new Inventory(this);
			if (_baseCC._specialActionCard != null)
			{
				_specialActionCard = new Card(_baseCC._specialActionCard, true);
				_specialActionCard.SetOwner(this);
			}
			if (isBase)
				CreateBase(_baseCC);
			else
			{
				Party.CombatCharacterInfo cci = (Party.CombatCharacterInfo)obj;
				LoadFromData(cci.ccd);
			}
			_cSprite.Initialize(this);
			_cSprite.UpdateStats();
			_ai = new CombatAI(this);
			void CreateBase(BaseCombatCharacter bcc)
			{
				//Debug.Log("Initializing " + GetName() + " - Base");
				_description = bcc._description;
				_attributes = new Attributes(this,bcc._attributes);
				_level = bcc._level;
				_stats = new Stats(bcc._hp,_attributes);
				RecalculateStats();
				_stats.hp.Fill();
				_stats.armor.current = 3;
				_stats.critMultiplier = 2f;
				_deck = new Decks(new Deck(bcc._deck, true), this);
				if(bcc._meleeWeapon!=null)_inventory.Equip(Weapon.Create(bcc._meleeWeapon,this));
				if (bcc._rangedWeapon != null) _inventory.Equip(Weapon.Create(bcc._rangedWeapon,this));
				/*Weapon it = (Weapon)ScriptableObject.CreateInstance(typeof(Weapon));
				Weapon pistol = (Weapon)ScriptableObject.CreateInstance(typeof(Weapon));
				it.Init(bcc._meleeWeapon == null ? (BaseWeapon)db.Get<BaseWeapon>(0) : bcc._meleeWeapon);
				_inventory._weaponMelee = it;*/
				_inventory.AddItem((BaseConsumable)db.Get<BaseConsumable>(0));
				_inventory.AddItem((BaseWeapon)db.Get<BaseWeapon>(0));
				_inventory.AddItem((BaseEquipment)db.Get<BaseEquipment>(0));
				_inventory.AddItem((BaseItem)db.Get<BaseItem>(0));
				_globalStats = new GlobalStats();
				foreach (BaseTalent bt in bcc._talents)
					AddTalent(bt);
			}
			void LoadFromData(CombatCharacterData ccd)
			{
				//Debug.Log("Initializing " + GetName() + " - Data");
				_level = ccd._level;
				_description = ccd._description;
				_attributes = new Attributes(this,ccd._attributes);
				_stats = new Stats(ccd._stats);
				_globalStats = new GlobalStats(ccd._globalStats);
				_deck = new Decks(ccd._deckCards, this);
				foreach(int i in ccd._talentIDs)
				{
					AddTalent((BaseTalent)db.Get<BaseTalent>(i));
				}
				#region Inventory and Weapons
				foreach (int i in ccd._inventoryItems) _inventory.AddItem((BaseItem)db.Get<BaseItem>(i));
				foreach (int i in ccd._inventoryConsumables) _inventory.AddItem((BaseConsumable)db.Get<BaseConsumable>(i));
				foreach (int i in ccd._inventoryEquipment) _inventory.AddItem((BaseEquipment)db.Get<BaseEquipment>(i));
				foreach (int i in ccd._inventoryWeapons) _inventory.AddItem((BaseWeapon)db.Get<BaseWeapon>(i));

				if (ccd._meleeID >= 0)
					_inventory.Equip(Weapon.Create((BaseWeapon)db.Get<BaseWeapon>(ccd._meleeID), this));
				else
				{
					Debug.Log("Unable to load melee weapon for " + GetName());
					_inventory.Equip(Weapon.Create(_baseCC._meleeWeapon, this));
				}

				if (ccd._rangedID >= 0)
					_inventory.Equip(Weapon.Create((BaseWeapon)db.Get<BaseWeapon>(ccd._rangedID), this));
				else
				{
					Debug.Log("Unable to load ranged weapon for " + GetName());
					_inventory.Equip(Weapon.Create(_baseCC._rangedWeapon, this));
				}
				#endregion
			}
			#region TestingEffects
			//AddAura(db.GetEffect(19)._applyAuras[0], this, false);//protection
			#endregion
			UpdateStats();
			if(GameManager._debug)
				foreach(FieldInfo fi in _combatEvents.GetType().GetFields())
				{
					//Debug.Log(fi.Name + " of type "+fi.FieldType)
					if(fi.FieldType == typeof(CombatEventSystem.EventTrigger))
					{
						CombatEventSystem.EventTrigger et = (CombatEventSystem.EventTrigger)fi.GetValue(_combatEvents);
						et._name = fi.Name + " of " +GetName();
					}
				}
			return this;
		}

		/// <summary>
		/// Reactivate a combatcharacter, that has already been created
		/// </summary>
		public CombatCharacter Prepare()
		{
			_combatEvents = new CombatEventSystem(this);
			gameObject.SetActive(true);
			SetLayer(transform, 9);
			RemoveAuras();
			_deck.Reset();
			_trackers.ResetCombat();
			if (_talents.Count > 0) RefreshTalents();
			TalentCheat();
			return this;
			void RefreshTalents()
			{

				List<BaseTalent> bts = new List<BaseTalent>();
				for (int i = 0; i < _talents.Count; i++)
				{
					bts.Add(_talents[i]._base);
					RemoveTalent(_talents[i]);
				}
				foreach (BaseTalent bt in bts)
					AddTalent(bt);
			}
			void TalentCheat()
			{
				for(int i = 2; i < 13; i++)
				{
					AddTalent((BaseTalent)db.Get<BaseTalent>(i));
				}
			}
		}
		public void AnnounceDraw(object card)
		{
			if (card.GetType().Equals(typeof(Card)))
			{
				Card c = (Card)card;
				Debug.Log(GetName() + " drew " + c._name);
			}
		}
		public void Thorns(object obj)
		{
			if (obj.GetType().Equals(typeof(CombatCharacter)))
			{
				CombatCharacter cc = (CombatCharacter)obj;

				if (cc)
				{
					cc.TakeDamage(1,  this, true, false);
					cc.UpdateStats();
				}
			}
		}
		public void StartTurn()
		{
			RegainActions();
			_trackers.ResetTurnStart();
			//_combatGUI._comps._GUIHand.UpdateHandPositions(0);
			_cSprite._statusBar.SetTurn(true);
			_combatGUI.RefreshUI(1);
			if (GameRules._drawAtTheStartOfTurn)
			{

				DrawToHandLimit();
				//Debug.Log("Drawing to handLimit");
			}
			_combatEvents.OnTurnStart(this);
			if (_alliance != Alliance.Player)
			{
				Task t = new Task(_ai.Act());
			}
			
		}
		public bool AbleToAct()
		{
			return !_isDead && !_IsDying;
		}
		public void EndTurn()
		{

			_combatEvents.OnTurnEnd(this);
			_combatGUI.RefreshUI();
			_combatManager.CheckConditions();
			if (!_combatManager._continueCombat)
				return;
			if(AbleToAct())
				_cSprite._statusBar.SetTurn(false);
			_trackers.ResetTurnEnd();
			_deck.Fumble();
			GUIAnimationManager.AddAnimation(EndTurnCoroutine(), GetName()+" End Turn()");
		}
		public void EndCombat()
		{
			_combatEvents.OnCombatEnd(this);
			
		}
		IEnumerator EndTurnCoroutine()
		{
			if (GameRules._drawAtTheStartOfTurn == false)
			{
				GUIAnimationManager.GUIAnimation animation = DrawCard(_stats.passiveDraw);
				while (animation != null && GUIAnimationManager._free == false)
				{
					yield return null;
				}
			}

			float handLowerTime = _combatGUI._comps._GUIHand.DisplayHand(false);

			float endDelay = GameSettings.UISettings.GetEndTurnDelay(_alliance) + handLowerTime;
			while (endDelay > 0)
			{
				endDelay -= Tm.GetWorldDelta();
				yield return null;
			}
			CombatManager.ArenaManager.NextTurn();
		}
		public void UpdateStats(bool instant = false)
		{

			_statusFlags._injured = _stats.hp.GetPercentage() <= 0.5f;
			//flanked & alone
			_cSprite.UpdateStats(instant);
			_combatEvents.OnRefreshStats(this);
			//_combatGUI.RefreshUI();
		}
		public void Camp()
		{
			//Gain levels (TODO: XP threshold for levels?)
			foreach(Talent t in _talents)
			{
				if (t._charges)
				{
					t._chargesPerCombat.Fill();
					t._chargesPerDay.Fill();
				}
			}
			_stats.hp.Fill();
			for(int i = _auras.Count - 1; i >= 0; i--)
			{
				_auras[i].Remove(true);
			}
			_attributes.Reset();
			_trackers.ResetCombat();
			LevelUp();
			UpdateStats();
		}
		public IEnumerator PrepareOutgoingDamage(Damage dmg)
		{
			_combatEvents.OnPrepareOutgoingDamage(dmg);
			//dmg.Lock();
			//Wait one frame to get event data that may affect damage. Then finalize estimation if only estimating damage.
			int tick = 1;
			while (tick > 0)
			{
				tick--;
				yield return null;
			}
			if (dmg._estimation)
				dmg.CompleteEstimation();
		}
		[NonSerialized] public bool _IsDying = false;
		public Task prepareIncomingDamageTask;
		public IEnumerator PrepareIncomingDamage(Damage dmg)
		{
			if (!dmg._estimation && !_trackers._retaliationTargets.Contains(dmg._source))
				_trackers._retaliationTargets.Add(dmg._source);
			_combatEvents.OnPrepareIncomingDamage(dmg);
			int tick = 1;
			while (tick > 0)
			{
				tick--;
				yield return null;
			}
			if (dmg._estimation) dmg.CompleteEstimation();
			else
			{
				//Events!
				while (_combatGUI._state == CombatGUI.StateOfGUI.Prepare)
				{
					yield return null;
				}
				dmg.Dbug();
				_IsDying = TakeDamage(dmg.GetDamage(), dmg._source, false, true);
			}
			prepareIncomingDamageTask = null;
		}
		public bool TakeDamage(int damage, CombatCharacter source = null, bool ignoreEvents = false, bool majorDamage = true)
		{

			bool dead = false;
			float animTime = _combatGUI.durationEstimate;
			if (AvoidDamage(source))
			{
				return false;
			}
			int dir = 0;
			if (source != null)
				dir = CombatManager.ArenaManager.GetDirectionFromTo(source, this);
			_cSprite.DisplayDamage(damage, animTime, dir, majorDamage);
			if (_stats.barrier.current > 0)
			{
				int bar = _stats.barrier.current;
				_stats.barrier.Add(-damage);
				damage -= bar;
			}
			if(damage>0)
				_stats.hp.Add(-damage);
			if (_stats.hp.current <= 0)
			{
				_combatManager.StartCoroutine(_combatManager.KillCharacter(this, source));
				dead = true;
			}
			if (!ignoreEvents)
				_combatEvents.OnAttacked(source);
			UpdateStats();
			return dead;
		}
		public bool TakeHeal(int heal, CombatCharacter source = null)
		{
			if (_stats.hp.IsMaxed() || heal <= 0)
				return false;
			_cSprite.ShowText(heal.ToString(), "heal");
			_stats.hp.Add(heal);
			return true;
		}
		public bool AvoidDamage(CombatCharacter source)
		{
			bool avoided = false;
			float animTime = 0.5f;
			#region Reaction
			{
				Aura reaction = GetAura((BaseAura)db.Get<BaseAura>(0));
				if (reaction != null)
				{
					_cSprite.ShowText("Parry", "aura");
					_cSprite.Animate("Parry", animTime);
					avoided = true;
					reaction.Trigger(source);
					_combatEvents.OnReaction(source);
				}
			}
			#endregion
			return avoided;
		}
		public bool _isDead { get; protected set; } = false;
		public bool Die(CombatCharacter killer = null)
		{
			if (_isDead)
				return false;
			_isDead = true;
			float deathtimer = 1f;
			if (killer != null)
			{
			
				_cSprite.BloodSplatter(CombatManager.ArenaManager.GetDirectionFromTo(killer, this));
				killer._combatEvents.OnKill(this);
			}
			_cSprite.Animate("Death", deathtimer);
			//Debug.Log("Killer = " + killer.GetName());
			_cSprite._statusBar.Destroy();
			Debug.Log(GetName() + " has died");
			TurnDisplay._current.RemoveCharacter(this);
			_combatEvents.OnDeath(killer);
			Invoke("Remove", deathtimer);
			return true;
		}
		private void Remove()
		{
			//Destroy(_cSprite._healthBar.gameObject);
			//gameObject.SetActive(false);
			Graveyard.Remove(gameObject);
			//Destroy(gameObject);
		}
		public void AddToName(string prefix = "", string suffix = "")
		{
			_name.prefix = prefix;
			_name.suffix = suffix;
			_name.GenerateAbbreviation();
		}
		public string GetName(bool includeFixes = true)
		{
			if (includeFixes)
				return _name.prefix + _name.name + _name.suffix;
			else
				return _name.name;
		}
		public void RecalculateStats()
		{
			int maxhp = _attributes._tenacity.current * 5 + _level * 5 + _baseCC._hp;
			if (_stats.hp.max != maxhp)
				_stats.hp.ModMax(maxhp-_stats.hp.max);
		}
		#region PlayCard
		List<Damage> _cardPlayDamages;
		public Task PlayCard(Card c, List<CombatCharacter> targets)
		{	Task t = new Task(PlayCardTask(c, targets));
			GUIAnimationManager.AddAnimation(t, GetName() + " PlayCard " + c._name);
			return t;
		}
		IEnumerator PlayCardTask(Card c, List<CombatCharacter> targetCharacters)
		{
			_cardPlayDamages = new List<Damage>();
			bool endTurn = false;
			AddActions(-1);
			#region PrepareToActivateCard
			_deck._mind._ideas.Remove(c);
			Task cardFX = new Task(_combatGUI.ActivateCardFX(c, this, targetCharacters.Count == 1 ? targetCharacters[0] : null), true);
			if (!c._features._repeating)
				_deck._mind._beingDiscarded += 1;
			float animTime = _combatGUI.durationEstimate;
			_combatGUI._state = CombatGUI.StateOfGUI.Prepare;
			
			/*
			 if (targetCharacters[0] != this)
			{
				Facing f = CombatManager.ArenaManager.GetOrientation(CombatManager.ArenaManager.GetPosition(this), CombatManager.ArenaManager.GetPosition(targetCharacters[0]));
				if (f != _facing)
					Flip();
			}
			*/
			animTime = _combatGUI.durationEstimate;
			while (_combatGUI._state == CombatGUI.StateOfGUI.Prepare)
				yield return null;
			#endregion
			#region CardEffects
			{
				Task t;
				foreach (BaseCard.EffectTarget et in c._effects._effects)
				{
					if (!AbleToAct())
						break;
					t = new Task(ActivateEffect(et));
					while (t.Running)
					{
						yield return null;
					}
				}
			}
			IEnumerator ActivateEffect(BaseCard.EffectTarget spell)
			{
				List<Task> tasks = new List<Task>();
				foreach (CombatCharacter cc in GetEffectTargets())
				{
					tasks.Add(new Task(ActivateOnTarget(cc)));
				}
				GUIAnimationManager.AddAnimations(tasks, " effect " + spell._effect._name);
				while (TasksAreRunning(tasks))
					yield return null;
				IEnumerator ActivateOnTarget(CombatCharacter cc) {
					BaseRequirement br = spell._requirement._requirement;
					if (br != null)
					{
						CombatCharacter requirementChar = spell._requirement._target == BaseCard.TargetType.Target ? cc : this;
						if (br.IsInstant())
						{
							if (br._requireSpecialCode == false)
							{
								if (br.InstantCheck(requirementChar) == false)
									yield break;
								if (br._retaliation) {
									if(br.RetaliationCheck(cc, this) == false)
									yield break;
								}
							}
							else if (br.RepeatCheck(c) == false)
								yield break;
						}
						else
						{
							Task t = new Task(br.DelayedCheck(requirementChar, c));
							while (t.Running)
								yield return null;
							if (br.delayCheckState != 1)
								yield break;
						}
					}
					if (spell._effect._id == 38) endTurn = true; //BaseEffect 038 - End Turn
					Task task = new Task(spell._effect.Activate(cc, this, spell._powerMultiplier,c));
					_combatEvents.OnActivateEffect(spell._effect);
					while (task.Running)
						yield return null;
				}
				List<CombatCharacter> GetEffectTargets()
				{
					if (spell._target == BaseCard.TargetType.Target)
						return targetCharacters;
					else
						return new List<CombatCharacter> { this };
				}
			}
			#endregion
			#region After Card is played
			c._playedThisTurn = true;
			_trackers._cardsPlayed++;
			TargetingSystem._current.Clear();
			while (cardFX.Running)	//Wait for the animations to finish before continuing
				yield return null;
			TurnDisplay._current.ClearTargets();
			CombatGUIHand._current.Hide(false, 0.33f);
			if (!c._specialActionCard)
			{
				if (!c._features._repeating)
				{
					_deck._mind._beingDiscarded -= 1;
					DiscardCards(TargetingSystem._current.GetCardsToDiscard(), c == TargetingSystem._current._playedCard);
				}
				else
				{
					c._onRepeat = true;
					if (c._gUICard)
						c._gUICard.ReturnToHand();
				}
			}
			else
				_combatGUI._comps._specialActionButton.Disable();
			UpdateStats();
			_combatEvents.OnIdea(c);
			while(_combatGUI._state!= CombatGUI.StateOfGUI.Free)
				yield return null;
			HistoryDisplay._current.AddEntry(GetCharacterTargetResult(this), c, GetCardResults());
			_cSprite.Animate("Idle");
			foreach (CombatCharacter cc in targetCharacters)
			{
				if (cc._cSprite != null)
					cc._cSprite.Animate("Idle");
			}
			//_cSprite.BarkRandom("Crit");
			#endregion
			HistoryDisplay.TargetResult GetCharacterTargetResult(CombatCharacter cc)
			{
				Damage dmg = new Damage(this, cc);
				foreach (Damage d in _cardPlayDamages)
				{
					if (d._target == cc)
					{
						dmg.AddDamage(d);
					}
				}
				return new HistoryDisplay.TargetResult(cc, dmg);
			}
			HistoryDisplay.CardResult GetCardResults(bool includeSelf = false)
			{
				List<HistoryDisplay.TargetResult> targetResults = new List<HistoryDisplay.TargetResult>();
				foreach (CombatCharacter cso in targetCharacters)
				{
					if (cso == this && includeSelf == false)
						continue;
					targetResults.Add(GetCharacterTargetResult(cso));
					
				}
				return new HistoryDisplay.CardResult(targetResults);
			}
			
			AutomatedTurnEnd();
			void AutomatedTurnEnd()
			{
				if (endTurn)
					EndTurn();
				else if (_alliance == Alliance.Player && GameSettings.UISettings._automaticTurnEnd)
				{
					if (_actions.current <= 0)
						EndTurn();
					else if (_deck.HasPlayableCards() == false)
					{
						EndTurn();
					}
				}
			}
		}
		#endregion
		public void AddDamageToHistory(Damage dmg)
		{
			//Debug.Log("Added " + dmg.GetInfo() + " to history");
			_cardPlayDamages.Add(dmg);
		}
		#region Card Draw and Discard
		public GUIAnimationManager.GUIAnimation DrawToHandLimit()
		{
			int drawCount = _stats.maxHandSize - _deck._mind._ideas.Count;
			if (drawCount > 0)
			{
				GUIAnimationManager.GUIAnimation animation = DrawCard(drawCount);
				return animation;
			}
			else
				return null;
		}
		public GUIAnimationManager.GUIAnimation DrawCard(int drawCount = 1, bool fromBottom = false, float animTime = 0.25f, Deck deck = null)
		{
			//Debug.Log(GetName() + " Drawing " + drawCount + " cards");
			if (deck == null)
				deck = _deck._subcon;
			GUIAnimationManager.GUIAnimation animation = null;
			List<Task> tasks = new List<Task>();
			List<Card> cards = new List<Card>();

			for (int i = 0; i < drawCount; i++)
			{
				Card c = _deck.DrawCard(deck, fromBottom);
				if (c == null)
					break;
				cards.Add(c);
				_combatEvents.OnThink(c);
				c.OnThink(this);
			}
			if(cards.Count>0)//if (_deck.hand._cards.Count - _deck.hand._beingDiscarded < _stats.maxHandSize)
			{
				if (_combatGUI._displayedCharacter = this)
				{
					if (animTime > 0)
					{
						Vector3 startPos = _combatGUI.GetRectTransform(deck._deckType).position;
						tasks.Add(new Task(_combatGUI._comps._GUIHand.AddCards(cards, animTime, startPos)));
					}
				}
				else
				{
					Debug.Log(GetName() + " is not displayed character, drawing not displayed!");
				}

			}
			else
			{
				Debug.Log(GetName() + " Hand full!");
			}
		
			//Debug.Log("<color=green>Made it here</color>");
			animation = GUIAnimationManager.AddAnimations(tasks, GetName() + " Draw "+drawCount);
			return animation;

		}
		public GUIAnimationManager.GUIAnimation AddCard(BaseCard bc, Deck targetDeck, Vector3 startPos, int drawCount = 1, float animTime = 0.25f)
		{
			if (targetDeck == null)
				targetDeck = _deck._mind;
			GUIAnimationManager.GUIAnimation animation = null;
			List<Card> cards = new List<Card>();
			for(int i = 0; i < drawCount; i++)
			{
				Card c = new Card(bc);
				cards.Add(c);
				if (targetDeck == _deck._mind)
					c.OnThink(this);
			}
			if (targetDeck == _deck._mind && _deck._mind._ideas.Count >= _stats.maxHandSize)
				targetDeck = _deck._forget;
			if (_combatGUI._displayedCharacter = this)
			{
				if (animTime > 0)
				{
					switch (targetDeck._deckType)
					{
						default:
							foreach (Card c in cards)
							{
								animation = GUIAnimationManager.AddAnimation(new Task(_combatGUI.DisplayCardMovingToDeck(c, startPos, _combatGUI.GetRectTransform(targetDeck._deckType).position)));
							}
							targetDeck._ideas.AddRange(cards);
							break;
						case Deck.DeckType.Hand:
							targetDeck._ideas.AddRange(cards);
							animation = GUIAnimationManager.AddAnimation(new Task(_combatGUI._comps._GUIHand.AddCards(cards, animTime, startPos)));
							//targetDeck.AddCards(c.GetID());
							break;
					}
				}
			}
			return animation;
		}
		public void DiscardRandomCards(int amount)
		{
			amount = Mathf.Clamp(amount, 0, _deck._mind._ideas.Count);
			List<Card> cards = new List<Card>();
			List<Card> randoCards = new List<Card>();
			randoCards.AddRange(_deck._mind._ideas);
			for(int i = amount;i>0 && randoCards.Count > 0; i--)
			{
				int rnd = UnityEngine.Random.Range(0, randoCards.Count);
				cards.Add(randoCards[rnd]);
				randoCards.RemoveAt(rnd);
			}
			DiscardCards(cards, false);
		}
		public void DiscardCard(Card c,bool played = false)
		{
			Debug.Log("Played = " + played);
			DiscardCards(new List<Card> { c }, played);
		}
		public void DiscardCards(List<Card>cards, bool played = false)
		{
			List<Task> tasks = new List<Task>();
			foreach (Card c in cards)
			{
				c._onRepeat = false;
				c.OnForget();
				_deck._mind._ideas.Remove(c);
				Task t = CreateDiscardTask(c,played);
				if (t != null)
					tasks.Add(t);
			}
			if (tasks.Count > 0)
				GUIAnimationManager.AddAnimations(tasks, GetName() + " Discard "+cards.Count+" cards");
		
			_combatGUI.RefreshUI();

			Task CreateDiscardTask(Card c, bool played = true)
			{
				Task t = null;
				if (c._features._burn && played)
				{
					_deck._regress._ideas.Add(c);
					t = c._gUICard.Remove(true);
				}
				else
				{
					_deck._forget._ideas.Add(c);
					if (c._gUICard) t = c._gUICard.Remove(false);
				}
				return t;

			}
		}
		public void DepleteRandomCards(int amount)
		{
			for (int i = amount; i > 0 && _deck._mind._ideas.Count > 0; i--)
			{
				int rnd = UnityEngine.Random.Range(0, _deck._mind._ideas.Count);
				Card c = _deck._mind._ideas[rnd];
				DepleteCard(c);
			}
		}
		public void DepleteCard(Card c)
		{
			c._onRepeat = false;
			c.OnRepress();
			_deck._mind._ideas.Remove(c);
			_deck._regress._ideas.Add(c);
			c._gUICard.Remove(true);
		}
		#endregion
		public class Decks
		{
			public Deck _mind { get; private set; }
			public Deck _subcon { get; private set; }
			public Deck _forget { get; private set; }
			public Deck _regress { get; private set; }
			public Deck _recollection { get; private set; }
			CombatCharacter _cc;
			public Decks(List<int> cardIds, CombatCharacter cc)
			{
				List<BaseCard> cards = new List<BaseCard>();
				foreach (int id in cardIds)
				{
					cards.Add((BaseCard)db.Get<BaseCard>(id));
				}
				_recollection = new Deck(cards);
				_subcon = new Deck(cards, true);
				_cc = cc;
				//Dbug();
				Reset();
				void Dbug(){

					string ids = "";
					for(int i = 0; i < cardIds.Count; i++)
					{
						if (i > 0)
							ids += ", ";
						BaseCard bc = (BaseCard)db.Get<BaseCard>(cardIds[i]);
						ids = ids + "[" + cardIds[i] +"]"+bc._name;
						if (i % 5 == 0 && i!=0)
							ids = ids + "\n";

					}
					Debug.Log("Creating new deck for "+_cc.GetName()+" with following cards:\n"+ids);
				}
			}
			public Decks(Deck deck, CombatCharacter cc)
			{
				_recollection = deck;
				_subcon = _recollection;
				_cc = cc;
				Reset();
			}
			public void Reset()
			{
				_subcon = new Deck(_subcon._baseIdeas, true);
				_forget = new Deck(new List<BaseCard>());
				_regress = new Deck(new List<BaseCard>());
				_mind = new Deck(new List<BaseCard>());
				_recollection._cc = _cc;
				_subcon._cc = _cc;
				_forget._cc = _cc;
				_regress._cc = _cc;
				_mind._cc = _cc;
				_subcon._deckType = Deck.DeckType.Draw;
				_forget._deckType = Deck.DeckType.Discard;
				_regress._deckType = Deck.DeckType.Deplete;
				_mind._deckType = Deck.DeckType.Hand;
			}
			public Card DrawCard(Deck deck, bool fromBottom = false)
			{
				Card c;
				if (deck == _subcon)
				{
					if (_subcon._ideas.Count == 0)   //if no cards left to draw, shuffle discard into draw
					{
						ShuffleDiscardIntoDraw(true);
					}
				}
				if (deck._ideas.Count == 0)
				{
					//Debug.LogError(_cc.GetName() + " trying to draw from a deck that contains 0 cards!");
					return null;
				}
				if (!fromBottom)
				{
					c = deck._ideas[0];
					deck._ideas.RemoveAt(0);
				}
				else
				{
					c = deck._ideas[deck._ideas.Count - 1];
					deck._ideas.RemoveAt(deck._ideas.Count - 1);
				}
				_mind._ideas.Add(c);
				return c;
			}
			public void ShuffleDiscardIntoDraw(bool visual)
			{

				if (visual)
				{
					GUIAnimationManager.AddAnimation(new Task(_combatGUI.ShuffleDeck(_forget, _subcon, _forget._ideas.Count)), "Shuffle Discard Into Draw");
				}
				_subcon.AddDeck(_forget._ideas);
				_subcon.Shuffle();
				_forget._ideas.Clear();
			}
			public Deck GetDeckByName(string deckName)
			{
				deckName = deckName.ToLower();
				switch (deckName)
				{
					default:
						Debug.LogError("Unknown deckName ~" + deckName + "~ defaulting to Draw");
						return _subcon;
					case "draw":
						return _subcon;
					case "discard":
						return _forget;
					case "deplete":
						return _regress;
					case "hand":
						return _mind;
				}
			}
			public Deck GetDeckByDeckType(Deck.DeckType dt)
			{
				switch (dt)
				{
					case Deck.DeckType.Deplete:
						return _regress;
					case Deck.DeckType.Discard:
						return _forget;
					case Deck.DeckType.Draw:
						return _subcon;
					case Deck.DeckType.Hand:
						return _mind;
					default:
						Debug.Log("Unknown Decktype");
						return null;

				}
			}
			public void Fumble()
			{
				List<Card> discards = new List<Card>();
				foreach (Card c in _mind._ideas)
				{
					if (c._features._fumble)
						discards.Add(c);
				}
				_cc.DiscardCards(discards);
			}
			public bool HasVisibleCards()
			{
				if (_cc._alliance == Alliance.Player || _cc._alliance == Alliance.Friendly)
					return true;
				else
				{
					if (HasVisibles(_mind)) return true;
					if (HasVisibles(_subcon)) return true;
					if (HasVisibles(_forget)) return true;
					if (HasVisibles(_regress)) return true;
				}
				return false;
			}
			public bool HasVisibleCardsInHand()
			{

				if (HasVisibles(_mind)) return true;
				return false;
			}
			public bool HasPlayableCards()
			{
				if (_mind._ideas.Count == 0)
					return false;
				foreach (Card c in _mind._ideas)
				{
					if (c.CheckPlayable())
						return true;
				}
				return false;
			}
			bool HasVisibles(Deck d)
			{
				foreach (Card c in d._ideas)
				{
					if (c._visible)
						return true;
				}
				return false;
			}
		}
		#region Aura Management
		public Aura GetAura(BaseAura baseaura)
		{
			foreach (Aura a in _auras)
				if (a._base == baseaura)
					return a;
			return null;
		}
		public bool HasAura(string name)
		{
			return HasAura((BaseAura)db.Get<BaseAura>(name));
		}
		public bool HasAura(int id)
		{
			return HasAura((BaseAura)db.Get<BaseAura>(id));
		}
		public bool HasAura(BaseAura auratype)
		{
			foreach (Aura a in _auras)
				if (auratype == a._base)
					return true;
			return false;
		}
		public void AddAura(BaseEffect.ApplyAura appliedAura, CombatCharacter source, bool message = true) {
			if(appliedAura._aura._debugLog)
				Debug.Log(GetName() + " gained " + appliedAura._aura._name);
			bool matchFound = false;
			BaseAura baseAura = appliedAura._aura;
			//search for already matching auras
			for (int i = 0; i < _auras.Count && !matchFound; i++) 
			{
				if (_auras[i]._base == baseAura)
				{
					matchFound = true;
					if (baseAura._stackingType != BaseAura.StackingType.None)
						_auras[i].AddStacks(baseAura._stackingType == BaseAura.StackingType.Case ? appliedAura._cases : appliedAura._duration);
				}
			} 
			//if no match, add a new aura
			if (!matchFound)
			{
				//Debug.Log("!matchfound ja iffistä sisään");
				Aura aura = new Aura(appliedAura, this, source);
				_auras.Add(aura);
				//if (aura._stackingType == BaseAura.StackingType.Case) aura.AddStacks(appliedAura._cases);
				//aura.AddStacks(appliedAura._stacks);


				switch (baseAura._tickType)
				{
					case BaseAura.TickType.beforeTurn:
						aura.AddTrigger(_combatEvents._OnTurnStart);
						break;
					case BaseAura.TickType.afterTurn:
						aura.AddTrigger(_combatEvents._OnTurnEnd);
						break;
					case BaseAura.TickType.trigger:
						//Does not lose duration or stacks passively
						break;
				}
				if (!baseAura._requireSpecialCode)
				{
					if (aura._damageModFlat != 0 || aura._damageModFractional != 1)
						aura.AddTrigger(_combatEvents._OnPrepareOutgoingDamage,aura.ModDamageOutgoing);
					if (aura._damageModIncomingFlat != 0 || aura._damageModIncomingFractional != 1)
						aura.AddTrigger(_combatEvents._OnPrepareIncomingDamage, aura.ModDamageIncoming);
					
				}
				else
				{
					switch (baseAura._id)
					{
						case 0:
							//000 - Reaction
							//aura.AddTrigger(_combatEvents._OnAttacked);
							break;
						case 1:
							//001 - Riposte
							aura.AddTrigger(_combatEvents._OnReaction, aura.Trigger);
							break;
						case 19:
							//019 - Reloading
							break;
						default:
							Debug.LogError("Unknown Aura ID [" + baseAura._id + "] on " + GetName());
							break;
					}
				}
			}
			if (message)
			{
				string txt ="<style=Aura>" +appliedAura._aura._name + (appliedAura._cases > 1 ? (" ("+appliedAura._cases)+")" : "")+"</style>";
				_cSprite.ShowText(txt, "aura");
			}
			if(baseAura._visible)
				_cSprite._statusBar.RefreshAuras();
			//PrintAuras();
		}
		public void RemoveAuras()
		{
			for(int i = _auras.Count - 1; i >= 0; i--)
			{
				_auras[i].Remove(true);
			}
		}
		public void PrintAuras()
		{
			Debug.Log("------------"+_name.name + " auras-------------");
			for (int i = 0; i < _auras.Count; i++)
			{
				Debug.Log("Auras[" + i + "] = " + _auras[i]._name + "(x "+_auras[i]._cases+")"+ " : " + _auras[i].GetDescription());
			}
		}
		#endregion
		#region TalentManagement
		public bool HasTalent(BaseTalent bt)
		{
			foreach (Talent t in _talents)
				if (t._base == bt)
					return true;
			return false;
		}
		public void TalentLearn(BaseTalent bt)
		{

			_globalStats._talentPointsAvailable = Mathf.Clamp(_globalStats._talentPointsAvailable - 1, 0, 99);
			AddTalent(bt);
		}
		public void AddTalent(BaseTalent bt)
		{
			if (HasTalent(bt))
			{
				//Debug.Log("Can't add duplicates of talents on "+GetName()+" ("+bt._name+")");
				return;
			}
			//Debug.Log("Adding talent " + bt._name + " to " + GetName()+" with the trigger "+bt._trigger.ToString());
			Talent t = new Talent(bt, this);
			_talents.Add(t);
			AssignTriggers();
			if (t._visibleAura) StartCoroutine(CreateAuraFromTalent());
			//Dbug();
			void AssignTriggers()
			{
				switch (t._trigger)
				{
					case BaseTalent.TalentTrigger.Passive:
					default:
						break;
					case BaseTalent.TalentTrigger.CombatStarts:
						_combatEvents._OnCombatStart._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.CombatEnds:
						_combatEvents._OnCombatEnd._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.YouDefeatAnEnemy:
						_combatEvents._OnKill._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.YouWouldDie:
						_combatEvents._OnDeath._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.YouTakeDamage:
						_combatEvents._OnPrepareIncomingDamage._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.YouDealDamage:
						_combatEvents._OnPrepareOutgoingDamage._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.YouMove:
						_combatEvents._OnMove._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.YouThink:
						_combatEvents._OnThink._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.YouForgetAnIdea:
						_combatEvents._OnForget._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.YouRepressAnIdea:
						_combatEvents._OnRepress._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.ActivateEffect:
						_combatEvents._OnActivateEffect._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.ApplyAura:
						_combatEvents._OnApplyAura._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.GainAura:
						_combatEvents._OnApplyAura._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.YouActualizeAnIdea:
						_combatEvents._OnIdea._delegates += t.Trigger;
						break;
					case BaseTalent.TalentTrigger.YouAreInjured:
						_combatEvents._OnPrepareIncomingDamage._delegates += t.Trigger;
						break;
				}

			}
			IEnumerator CreateAuraFromTalent()
			{
				int updates = 1;
				while (updates > 0)
				{
					updates--;
					yield return null;
				}
				Aura a = new Aura(t);
				_auras.Add(a);
				_cSprite._statusBar.RefreshAuras();
			}
			void Dbug()
			{
				if (_talents.Count == 0)
				{
					Debug.Log(GetName() + " has no talents");
					return;
				}
				Debug.Log("-----\n"+GetName() + " now has the following talents:");
				for(int i = 0;i<_talents.Count;i++)
				{
					Debug.Log(i + ". " + _talents[i]._name);
				}
				Debug.Log("------");
			}
		}
		public void RemoveTalent(Talent t)
		{
			//Debug.Log("Attempting to remove Talent " + t._name);
			foreach(FieldInfo fi in _combatEvents.GetType().GetFields())
			{
				if(fi.GetType()== typeof(CombatEventSystem.EventTrigger))
				{
					CombatEventSystem.EventTrigger et = (CombatEventSystem.EventTrigger)fi.GetValue(_combatEvents);
					try { et._delegates -= t.Trigger; }
					catch { Debug.LogError("No Trigger delegate to delete"); }
				}
			}
			_talents.Remove(t);
		//	Dbug();

			void Dbug()
			{
				if (_talents.Count == 0)
				{
					Debug.Log(GetName() + " has no talents");
					return;
				}
				Debug.Log("-----\n" + GetName() + " now has the following talents:");
				for (int i = 0; i < _talents.Count; i++)
				{
					Debug.Log(i + ". " + _talents[i]._name);
				}
				Debug.Log("------");
			}
		}
		public void RemoveTalent(BaseTalent bt)
		{
			foreach (Talent t in _talents)
				if (t._id == bt._id)
				{
					RemoveTalent(t);
					return;
				}
		}
		#endregion
		public struct Resources
		{
			public Bint _HP;
			public Bint _energy;

			public Resources(int hp, int energy)
			{
				_HP = new Bint(hp);
				_energy = new Bint(energy);
			}
		}
		public struct Attributes
		{
			public static int _maxAttrib = 10;
			public Bint _strength;
			public Bint _dexterity;
			public Bint _intelligence;
			public Bint _tenacity;
			public int _availablePoints;
			CombatCharacter _cc;
			public Attributes(CombatCharacter cc, int i = 0)
			{
				_cc = cc;
				_strength = new Bint(i);
				_dexterity = new Bint(i);
				_intelligence = new Bint(i);
				_tenacity = new Bint(i);
				_availablePoints = 0;
			}
			public Attributes(CombatCharacter cc, BaseCombatCharacter.Attributes attr)
			{
				_cc = cc;
				_strength = new Bint(attr.strength);
				_dexterity = new Bint(attr.dexterity);
				_intelligence = new Bint(attr.intelligence);
				_tenacity = new Bint(attr.tenacity);
				_availablePoints = 0;
			}
			public Attributes(CombatCharacter cc, Attributes attr)
			{
				_cc = cc;
				_strength = new Bint(attr._strength);
				_dexterity = new Bint(attr._dexterity);
				_intelligence = new Bint(attr._intelligence);
				_tenacity = new Bint(attr._tenacity);
				_availablePoints = 0;
			}
			public void Reset()
			{
				_strength.current = _strength.max;
				_dexterity.current = _dexterity.max;
				_intelligence.current = _intelligence.max;
				_tenacity.current = _tenacity.max;
			}
			public bool AddToAttribute(string attrib)
			{
				if (_availablePoints > 0)
				{
					//Debug.Log("adding +1 to " + attrib);
					switch (attrib.ToLower())
					{
						default:
							Debug.LogError(attrib + " does not match any attributestring");
							return false;
						case "str":
							if(_strength.max<_maxAttrib)
							_strength.ModMax(1);
							break;
						case "dex":
							if (_dexterity.max < _maxAttrib)
								_dexterity.ModMax(1);
							break;
						case "int":
							if (_intelligence.max < _maxAttrib)
								_intelligence.ModMax(1);
							break;
						case "ten":
							if (_tenacity.max < _maxAttrib)
								_tenacity.ModMax(1);
							else
								return false;
							break;

					}
					_cc.RecalculateStats();
					_cc.UpdateStats(true);
					_cc._stats.hp.Fill();
					_availablePoints-=1;
					return true;
				}
				else
					return false;
			}
			public string StrengthToString()
			{
				string s = _strength.ToString();
				if (_strength.current < _strength.max)
					s = "<color=#FF6B00>" + s + "</color>";
				else if (_strength.current > _strength.max)
					s = "<color=#00FF6F>" + s + "</color>";
				return s;

			}
			public string DexterityToString()
			{
				Bint temp = _dexterity;
				string s = temp.ToString();
				if (temp.current < temp.max)
					s = "<color=#FF6B00>" + s + "</color>";
				else if (temp.current > temp.max)
					s = "<color=#00FF6F>" + s + "</color>";
				return s;

			}
			public string IntelligenceToString()
			{
				Bint temp = _intelligence;
				string s = temp.ToString();
				if (temp.current < temp.max)
					s = "<color=#FF6B00>" + s + "</color>";
				else if (temp.current > temp.max)
					s = "<color=#00FF6F>" + s + "</color>";
				return s;

			}
			public string TenacityToString()
			{
				Bint temp = _tenacity;
				string s = temp.ToString();
				if (temp.current < temp.max)
					s = "<color=#FF6B00>" + s + "</color>";
				else if (temp.current > temp.max)
					s = "<color=#00FF6F>" + s + "</color>";
				return s;

			}
			public void Dbug()
			{
				Debug.Log("STR: " + _strength.current + " - " + "DEX: " + _dexterity.current + " - " + "INT: " + _intelligence.current + " - " + "TEN: " + _tenacity.current);
			}
			public float CalculateEffectScaling(BaseEffect effect)
			{
				float dmg = 0;
				switch (effect._damageType)
				{
					default:
						break;
					case BaseEffect.DamageType.Melee:
						dmg += 2 * _strength.current;
						break;
					case BaseEffect.DamageType.Ranged:
						dmg += 2 * _dexterity.current;
						break;
					case BaseEffect.DamageType.Heal:
						dmg += 2 * _intelligence.current;
						break;
				}
				return dmg;
			}
		}
		public enum Alliance { Enemy = 1, Friendly = 2, Player = 3, Object = 4 }
		public bool IsHostileTowards(CombatCharacter cc)
		{
			switch (_alliance)
			{
				case Alliance.Enemy:
					if (cc._alliance == Alliance.Enemy)
						return false;
					break;

				case Alliance.Friendly:
				case Alliance.Player:
					if (cc._alliance == Alliance.Player || cc._alliance == Alliance.Friendly)
						return false;
					break;

				default:
					return false;

			}
			return true;
		}
		public int EstimateDamageQuick(BaseEffect spell, float spellDamageMultiplier = 1f, CombatCharacter target=null)
		{
			int dmg = 0;
			if (spell._damageType==BaseEffect.DamageType.Melee)
			{
				if (_inventory._melee != null)
				{
					dmg += _inventory._melee._weaponDamage;
				}
				else
					dmg += 0;
			}
			else if (spell._damageType == BaseEffect.DamageType.Ranged)
			{
				if (_inventory._ranged != null)
				{
					dmg += _inventory._ranged._weaponDamage;
				}
				else
					dmg += 0;
			}
			dmg += spell._damageFlat;
			dmg += (int)_attributes.CalculateEffectScaling(spell);
			dmg = Mathf.RoundToInt(dmg * spellDamageMultiplier);
			if (target != null)
			{
				dmg = target.CalculateDamageReduction(dmg,spell._damageType==BaseEffect.DamageType.Heal);
			}
			//check if qualify for critical damage
			return dmg;
		}
		public int BeginEstimateDamageReal(Damage dmg)
		{
			StartCoroutine(PrepareOutgoingDamage(dmg));
			return dmg.GetDamage();
			//Start Real estimation and return quick estimate
		}
		public int CalculateDamageReduction(int incomingDamage, bool heal = false)
		{
			if (!heal)
			{
				incomingDamage = Mathf.Clamp(incomingDamage - _stats.armor.current, 0, 999);
				return incomingDamage;
			}
			else
			{
				return HasAura((BaseAura)db.Get<BaseAura>(20))?0:incomingDamage;
			}
		}
		public int GetAuraBPDuration(BaseAura abp)
		{   //TODO TAKE NOTICE OF DURATION INCREASING TALENTS
			return abp._duration;
		}
		public void Flip()
		{
			if (_facing == Facing.Left)
				_facing = Facing.Right;
			else
				_facing = Facing.Left;
			_cSprite.Flip();
		}
		public bool CheckAuraDuplicate(Aura aura) {
			foreach(Aura a in _auras)
			{
				if (a._id == aura._id && a != aura)
					return true;
			}
			return false;
		}
		public void RegainActions(int count = 0)
		{
			//add checks for debuffs that reduce AP regen
			if (count <= 0)
				_actions.Fill();
			else
				_actions.Add(count);
			if (_combatGUI._displayedCharacter == this)
				_combatGUI._comps._actionDisplay._actionOrb.UpdateActions(null);
		}
		public void AddActions(int add)
		{
			_actions.Add(add);
			if (_combatGUI._displayedCharacter == this)
				_combatGUI._comps._actionDisplay._actionOrb.UpdateActions(null);
		}
		public void GetSpecialAction()
		{

		}
		public Sprite GetPortrait()
		{
			if (_dialogCharacter && _dialogCharacter._portrait)
				return _dialogCharacter._portrait;
			else
				return _cSprite._spriteRendererCharacter.sprite;
		}
		public void LevelUp(int i = 1)
		{
			_level ++;
			if (_level % 2 == 0)
				_globalStats._talentPointsAvailable++;
			_attributes._availablePoints += 2;
			i--;
			if (i > 0)
				LevelUp(i);
			else
				RecalculateStats();
		}
	}


}