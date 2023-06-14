using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using static RD.CodeTools;

/*
 * 
 * TODO:
 * Translate description calls to game data like [DMG0]
 * -Requires Combat Manager
 * Characters
 * Summons
 * -Requires Character
 * 
 * 
 */


/*
public static class Database
{
	public static List<Blueprint.CardBlueprint> cardDatabase = new List<Blueprint.CardBlueprint>();
	public static List<Blueprint.SpellBlueprint> spellDatabase = new List<Blueprint.SpellBlueprint>();
	public static List<Blueprint.AuraBlueprint> auraDatabase = new List<Blueprint.AuraBlueprint>();
	public static List<Blueprint.SummonBlueprint> summonDatabase = new List<Blueprint.SummonBlueprint>();
	public static List<Blueprint.RequirementsBlueprint> requirementsDatabase = new List<Blueprint.RequirementsBlueprint>();
	public static List<Blueprint.TalentBlueprint> talentDatabase = new List<Blueprint.TalentBlueprint>();
	public static HyperlinkDatabase hyperlinkDatabase = new HyperlinkDatabase();
	public 
	static string combatDatabaseName= "Combat Database";
	public static bool dbug = false;
	//TEST STUFF
	public static int DatabasesRead = 0;



	public static void ReadAllData()
	{

		ReadData(typeof(Hyperlink));
		ReadData(typeof(Blueprint.CardBlueprint));
		ReadData(typeof(Blueprint.SpellBlueprint));
		ReadData(typeof(Blueprint.AuraBlueprint));
		*//*
		ReadData(typeof(Blueprint.RequirementBlueprint));
		ReadData(typeof(Blueprint.ItemBlueprint));
		ReadData(typeof(Blueprint.SummonBlueprint));
		*//*
	}

	static void ReadData(Type type) {
		StreamReader file = new StreamReader(Application.dataPath + "/Resources/" + combatDatabaseName + " - " + 
			(type==typeof(Blueprint.CardBlueprint)?"Cards":
			((type==typeof(Blueprint.SpellBlueprint))?"Spells":
			((type == typeof(Blueprint.AuraBlueprint))?"Auras":
			((type == typeof(Blueprint.AuraBlueprint)) ? "Summons":
			(type == typeof(Hyperlink)) ? "Hyperlinks" : 
			"NULL"))))
			+ ".csv");
		string line;
		List<string> cards = new List<string>();
		Dictionary<string, List<string>> infoStrings = new Dictionary<string, List<string>>();
		line = file.ReadLine();
		while ((line = file.ReadLine()) != null)
		{
			//if (dbug) Debug.Log("Line = " + line);
			string[] values = line.Split(new char[] { ',' });
			List<string> valueList = new List<string>();
			foreach (string str in values) valueList.Add(str.Replace('$',',')); //In DataBase, fields with multiple values have the values separated by '$'
			//if (dbug) Debug.Log("values.length = " + values.Length);
			//Add the right data to the right database
			{
				if (type == typeof(Blueprint.CardBlueprint))
				{
					Blueprint.CardBlueprint newCard = new Blueprint.CardBlueprint(valueList);
					cardDatabase.Add(newCard);
					if(dbug)Debug.Log("Added Card ["+newCard._id+"] " + newCard._name);
				}
				else if (type == typeof(Hyperlink))
				{
					hyperlinkDatabase.Add(new Hyperlink(valueList));
				}
				else if (type == typeof(Blueprint.SpellBlueprint))
				{
					Blueprint.SpellBlueprint newCard = new Blueprint.SpellBlueprint(valueList);
					spellDatabase.Add(newCard);
					if(dbug)Debug.Log("Added Spell [" + newCard._id + "] " + newCard._name);
				}
				else if (type == typeof(Blueprint.AuraBlueprint))
				{
					Blueprint.AuraBlueprint newAura = new Blueprint.AuraBlueprint(valueList);
					auraDatabase.Add(newAura);
					if(dbug)Debug.Log("Added Aura [" + newAura._id + "] " + newAura._name);
				}
				else if (type == typeof(Blueprint.SummonBlueprint))
				{
					Blueprint.SummonBlueprint newCard = new Blueprint.SummonBlueprint(valueList);
					summonDatabase.Add(newCard);
				}
				else if (type == typeof(Blueprint.RequirementsBlueprint))
				{

					Blueprint.RequirementsBlueprint newCard = new Blueprint.RequirementsBlueprint(valueList);
					requirementsDatabase.Add(newCard);
				}
				else
				{
					Debug.LogError("Unknown type when reading data!");
					return;
				}
			} 
		}

		DatabasesRead++;
	}

	*//*
	 * Translates values separated by "," into a list of values
	 *//*
	public static List<string> TranslateMultifield (string str)
	{
		List<string> valueList = new List<string>();
		string[] values = str.Split(new char[] { ',' });
		foreach (string value in values)
			valueList.Add(value);
		return valueList;
	}

}


public class Blueprint
{
	public int _id { get; protected set; }
	public string _name { get; protected set; }
	public string _description { get; protected set; } = "";

	public class RequirementsBlueprint : Blueprint
	{
		public bool _enabled { get; protected set; } = false;
		public bool _invertTrue { get; protected set; } = false;
		public int _auraRequired { get; protected set; }
		//----------------Cards--------------------------------
		public int _handSize { get; protected set; }
		public int _drawSize { get; protected set; }
		public int _discardSize { get; protected set; }
		//----------------Damage-------------------------------
		public bool _injured { get; protected set; }
		public bool _unharmed { get; protected set; }
		public int _damageTaken { get; protected set; }
		//---------------Positioning---------------------------
		public bool _flanked { get; protected set; }
		public bool _alone { get; protected set; }
		//----------------Misc--------------------------
		public int _turnCount { get; protected set; }

		public RequirementsBlueprint(List<string> values)
		{
			int i = 0;
			_enabled = true;
			_invertTrue = ParseBool(values[i],false);i++;
			if (values[i]!="")_auraRequired = ParseInt(values[i]); i++;
			//-------------------------------------------------------
			_handSize = ParseInt(values[i]);i++;
			_drawSize = ParseInt(values[i]); i++;
			_discardSize = ParseInt(values[i]); i++;
			//-------------------------------------------------------
			_injured = ParseBool(values[i], false); i++;
			_unharmed= ParseBool(values[i], false); i++;
			_damageTaken = ParseInt(values[i]); i++;
			//-------------------------------------------------------
			_flanked = ParseBool(values[i], false); i++;
			_alone = ParseBool(values[i], false); i++;
			//-------------------------------------------------------
			_turnCount = ParseInt(values[i],0); i++;
		}
		public bool SetActive(bool active = true)
		{
			_enabled = active;
			return _enabled;
		}
		
		 public bool CheckRequirement(CombatCharacter cha)
		{
			if (!_enabled) return true;
			bool checksOut = false;
			return (_invertTrue?!checksOut:checksOut);
		}

	}
	public struct BlueprintTarget
	{
		public int _ID;
		public Targets _spellTargets;
		public BlueprintTarget(int BlueprintID, Targets blueprintTargets)
		{
			_ID = BlueprintID;
			_spellTargets = blueprintTargets;
		}
		public enum Targets { TARGET, SELF, BOTH }
	}

	public class ItemBlueprint : Blueprint
	{
		public int _value { get; protected set; }
		public int _level { get; protected set; }
		public bool _requireCode { get; protected set; }
		public ItemBlueprint(List<string> values)
		{
			int i = 1;
			_id = int.Parse(values[i]); i++;
			_name = values[i]; i++;
			_description = values[i]; i++;
			_value = ParseInt(values[i], 0);i++;
			_level = ParseInt(values[i], 0); i++;
		}
		public struct CardCount
		{
			public int cardID { get; private set; }
			public int count { get; private set; }
			public CardCount(int id, int amount = 1)
			{
				cardID = id;
				count = amount;
			}
		}
	}

	public class EquipmentBlueprint: ItemBlueprint
	{
		public int _damage { get; protected set; }
		public int armor { get; protected set; }
		public List<int> _talents { get; protected set; } = new List<int>();
		public CombatCharacter.Attributes _attributes { get; protected set; }
		public List<CardCount> _cards { get; protected set; } = new List<CardCount>();		//Weapons add weapon cards to deck, trinkets or such could add items with a single charge or something?

		public EquipmentBlueprint(List<string> values): base(values)
		{
			int i = 1;
			//.....
			if (values[i].Contains(","))
			{
				List<string> cardValues = Database.TranslateMultifield(values[i]);
				while (cardValues.Count > 1)
				{
					_cards.Add(new CardCount(ParseInt(cardValues[0]), ParseInt(cardValues[1])));
					cardValues.RemoveRange(0, 1);
				}
			}i++; //Unpack CardCount
		}
	}
	public class CardBlueprintVariables : Blueprint
	{
		public CardType _cardType { get; protected set; }
		public bool useMeleeWeaponDamage { get; protected set; } = false;
		public bool useRangedWeaponDamage { get; protected set; } = false;
		public List <BlueprintTarget> _requirements { get; protected set; }             //If requirements are not filled, the card can not be played
		public List <BlueprintTarget> _requirementsCrit { get; protected set; }             //If requirements are not filled, the card can not be played
																				 //------------------------------------------//
		public Target _target { get; protected set; } = new Target();
		public Bint _range { get; protected set; }
		public bool _randomTarget { get; protected set; } = false;
		public int _multiTarget { get; protected set; } = 0;			//0 = only one target, 1 = two adjacent targets(2 total), 2 = target and adjacent targets(3 total), 3 = All possible targets
		//------------------------------------------//
		public List<BlueprintTarget> _applySpells { get; protected set; } = new List<BlueprintTarget>();
		public List<SpellBlueprint.AppliedAura> _appliedAuras { get; protected set; } = new List<SpellBlueprint.AppliedAura>();
		public int _hitMod { get; protected set; }                                //Flat increase (or decrease) to hit chance
		public int _critMod { get; protected set; }                               //Flat increase (or decrease) to crit chance --- NOTE: crit <= hit ALWAYS
		public bool _playOnDraw { get; protected set; } = false;
		public bool _unplayable { get; protected set; } = false;
		public bool _repeating { get; protected set; } = false;
		public bool _deplete { get; protected set; } = false;
		public int _charges = 0;
		public bool _fumble { get; protected set; } = false;
		public bool _requireSpecialCode { get; protected set; } = false;
		//-----------ART-------------------------------//
		public Sprite _artSprite { get; protected set; } //="Default Card Art.png" if empty, go to default
		public string _cardBG { get; protected set; } //="Default Card Background.png" if empty, go to default
		public bool _focusedAnimation { get; protected set; }
		public string _animation { get; protected set; }
		public string _impactEffect { get; protected set; }
		//-----------------NOT READ FROM DATABASE -------------------------//
		public string _animationPrep { get; protected set; }
		public string _descriptionText { get; protected set; } = "";
		public List<Hyperlink> _hyperlinks { get; protected set; } = new List<Hyperlink>();
		*//*
		 * CardInstances made from CardData
		 * CardInstances have variables to allow them to be modified within combat
		 * List _applySpellsMod
		 *//*

		public enum CardType { Skill, Melee, Ranged, Weapon, Item, Spell }
		public struct Target
		{
			public bool _self;
			public bool _ally;
			public bool _enemy;
			public bool _empty;
			public bool _object;
			public Target(bool self = false, bool ally = false, bool enemy = false, bool empty = false, bool obj = false)
			{
				_self = self;
				_ally = ally;
				_enemy = enemy;
				_empty = empty;
				_object = obj;
			}
			
			public bool OnlySelf()
			{
				return (_self && !_ally && !_enemy && !_empty && !_object);
			}
		}
		public bool IsAttack()
		{
			return _cardType == CardType.Melee || _cardType == CardType.Ranged || _cardType == CardType.Weapon;
		}
		public string GetSliderText()
		{
			string sliderText;
			if (_cardType == CardType.Melee || _cardType == CardType.Ranged || _cardType == CardType.Weapon)
				sliderText = "hit";
			else
				sliderText = "success";
			return sliderText;
		}
	}
	public class CardBlueprint : CardBlueprintVariables
	{
		public CardBlueprint(List<string> values)
		{
			int i = 1;
			_id = int.Parse(values[i]); i++;
			_name = values[i]; i++;
			_cardType = (CardType)Enum.Parse(typeof(CardType), values[i]); i++;
			_description = values[i]; i++;
			if (values[i].Contains(","))
			{
				List<string> requirementValues = Database.TranslateMultifield(values[i]);
				while (requirementValues.Count >= 2)
				{
					_requirements.Add(new BlueprintTarget(ParseInt(requirementValues[0]), (BlueprintTarget.Targets)Enum.Parse(typeof(BlueprintTarget.Targets), requirementValues[1])));
					requirementValues.RemoveRange(0, 2);
				}
				if(requirementValues.Count > 0 && requirementValues.Count < 2)
				{
					Debug.LogError("Error translating requirementValues " + values[i] + " of cardID " + values[0]);
				}
			} i++; //Unpack requirements
			if (values[i].Contains(","))
			{
				List<string> requirementValues = Database.TranslateMultifield(values[i]);
				while (requirementValues.Count >= 2)
				{
					_requirementsCrit.Add(new BlueprintTarget(ParseInt(requirementValues[0]), (BlueprintTarget.Targets)Enum.Parse(typeof(BlueprintTarget.Targets), requirementValues[1])));
					requirementValues.RemoveRange(0, 2);
				}
				if (requirementValues.Count > 0 && requirementValues.Count < 2)
				{
					Debug.LogError("Error translating requirementValues " + values[i] + " of cardID " + values[0]);
				}
			} i++; //Unpack Critical Requirements
				 //------------------------------------------//
			_target = new Target(ParseBool(values[i]), ParseBool(values[i + 1]), ParseBool(values[i + 2]), ParseBool(values[i + 3]), ParseBool(values[i + 4])); i += 5;
			_range = new Bint(ParseInt(values[i + 1]), ParseInt(values[i]));i += 2;
			_randomTarget = _target.OnlySelf()?true:ParseBool(values[i]); i++;
			_multiTarget = ParseInt(values[i]); i++;
			//------------------------------------------//
			if (values[i].Contains(","))
			{
				List<string> spellValues = Database.TranslateMultifield(values[i]);
				while (spellValues.Count >= 2)
				{
					_applySpells.Add(new BlueprintTarget(ParseInt(spellValues[0]), (BlueprintTarget.Targets)Enum.Parse(typeof(BlueprintTarget.Targets), spellValues[1].ToUpper())));
					spellValues.RemoveRange(0, 2);
				}
				if (spellValues.Count > 0 && spellValues.Count < 2)
				{
					Debug.LogError("Error translating SpellValues " + values[i] + " of cardID " + values[0]);
				}
			} i++; //Unpack spells
			if (values[i].Contains(","))
			{
				List<string> auravalues = Database.TranslateMultifield(values[i]);
				while (auravalues.Count > 2)
				{
					_appliedAuras.Add(new SpellBlueprint.AppliedAura(int.Parse(auravalues[0]), int.Parse(auravalues[1]), int.Parse(auravalues[2])));
					auravalues.RemoveRange(0, 3);
				}
				if (auravalues.Count > 0 && auravalues.Count < 3)
				{
					Debug.LogError("Error translating AuraValues " + values[i] + " of spellID " + values[0]);
				}
			}	i++; //Unpack ApplyAuras
			_playOnDraw = ParseBool(values[i]); i++;
			_unplayable = ParseBool(values[i]); i++;
			_repeating = ParseBool(values[i]); i++;
			_deplete = ParseBool(values[i]); i++;
			_charges = ParseInt(values[i]); i++;
			_fumble = ParseBool(values[i]); i++;
			_requireSpecialCode = ParseBool(values[i]); i++;
			//------------------------------------------//
			string path = "Cards/" + (values[i] == "" ? "Block" : values[i]);
			if(Translator.dbug)
				Debug.Log(" Card path = " + path);
			_artSprite = UnityEngine.Resources.Load<Sprite>(path); i++;
			_cardBG = values[i]; i++;

			//--------------------Animations and Impacts---------------//
			_focusedAnimation = ParseBool(values[i], false); i++;
			if (_cardType == CardType.Melee || (_cardType == CardType.Weapon && _range.max <= 1))
				useMeleeWeaponDamage = true;
			else if (_cardType == CardType.Ranged || (_cardType == CardType.Weapon && _range.max > 1))
				useRangedWeaponDamage = true;
			if (values[i] == "")
			{
				if (useMeleeWeaponDamage)
					_animation = "Attack";
				else if (useRangedWeaponDamage)
					_animation = "AttackRanged";
				else
					_animation = "Cast";
			}
			else
				_animation = values[i]; i++;
			if (values[i] == "")
				_impactEffect = "Default";
			else
				_impactEffect = values[i]; i++;
			_animationPrep = _animation + "Prep";

			//-----------------NOT READ FROM DATABASE -------------------------//
			_hyperlinks = Translator.GetHyperlinks(_description);
			_descriptionText = Translator.TranslateString(_description);
		}
	}
	public class SpellBlueprint : Blueprint
	{
		
		public BlueprintTarget _requirements { get; protected set; }             //if requirements do not match, cancel spell effect. The card can still be played but this particular spell will have no effect.
		public HealthData _modHP { get; protected set; }
		public HealthData _setHP { get; protected set; }
		public int _damageBonus { get; protected set; }                           //Use this for attack damage																	  //public Resources _modPowerBasedOnResources { get; protected set; }		//For Example add damage based on missing health
		public bool _useWeaponDamage { get; protected set; }                      //If dealing damage, add the basic attack damage?
		public int _modInit { get; protected set; }                             // Change combatCharacter Initiative, forcing them back or forth in turn order
		public float[] _attributeBonuses { get; protected set; }                  //0 = str, 1 = agi, 2 = int, 3 = tena ----------- multiply attributes to get the total bonus --------- attributeBonuses affect _damagebonus, _modHP and _addEnergy
																				  //------------CARD MODIFICATION--------------
		public bool _modCards { get; protected set; } = false;                    //If True, then process the following variables
		public CardTargeting _cardTargeting { get; protected set; }
		public int _modCardCost { get; protected set; }                           //Decrease or increase card cost
		public BlueprintTarget _modCardAddSpellID { get; protected set; }               //Add an extra effect to the card, for example gain 2 hp when you play the card
		public int _modCardPower { get; protected set; }                          //cardPower not found in CardData, only in instantiated Cards. Always a default of 0
		public bool _modCardPurge { get; protected set; }                         //Purge() removes any temporary modifications from cards
																				  //------------ADDING CARDS TO HAND & DECK-------
		public CardRecovery _cardRecovery { get; protected set; }                 //Recover your own cards from deplete or discard
		public AddCard _addCard { get; protected set; }                           //Shuffle new cards to deck or add them to directly to hand
		public int _discardCards { get; protected set; } = 0;
		public bool _discardRandom { get; protected set; } = true;
		public int _depleteCards { get; protected set; } = 0;
		public bool _depleteRandom { get; protected set; } = true;
		public int _drawCards { get; protected set; }
		//--------------------MOVEMENT AND PUSH---------
		public MovementType _movementType { get; protected set; } = MovementType.stay;
		public int _pushAmount { get; protected set; } = 0;                       //Positive values = push, negative values = push
		public bool _pushStopOnCollision { get; protected set; } = true;          //Stop push movement if the next position is not empty
																				  //--------------------MISCELLANIOUS-------------
		public bool _shuffleDiscardIntoDraw { get; protected set; } = false;      //Shuffle discarded cards back into the deck
		public bool _shuffleDrawDeck { get; protected set; } = false;             //Shuffle the draw deck
		public List<AppliedAura> _applyAuras { get; protected set; } = new List<AppliedAura>();
		public ApplySummon _applySummon { get; protected set; }
		public bool _requireSpecialCode { get; protected set; }
	 *//*
	  * Spell visual effects
	 *//*
		public bool _dealsDamage { get; private set; } = true;
		public enum SpellPowerType { Damage, ModHealth, Draw, AddEnergy, None }
		public enum MovementType { stay, moveTotarget, moveBeforeTarget, moveBehindTarget, swapPlaces }
		public struct CardTargeting
		{
			public enum CardTarget { This, Hand, Draw, Discard }
			public CardTarget _target;
			public int _targetCount;
			public bool _random;
			public CardTargeting(CardTarget target, int targetCount = 1, bool random = false)
			{
				_target = target;
				_targetCount = targetCount;
				_random = random;
			}
		}
		public struct CardRecovery
		{
			public RecoveryDeck _recoveryDeck;
			public RecoveryTarget _recoveryTarget;
			public int _recoveryCount;
			public bool _random;

			public CardRecovery(RecoveryDeck recoveryDeck, RecoveryTarget recoveryTarget, int recoveryCount = 1, bool random = false)
			{
				_recoveryDeck = recoveryDeck;
				_recoveryTarget = recoveryTarget;
				_recoveryCount = recoveryCount;
				_random = random;
			}
			public enum RecoveryDeck { Discard, Deplete }
			public enum RecoveryTarget { Hand, Draw, Discard }
		}
		public struct AddCard
		{
			public int _cardID;
			public CardDestination _cardDestination;
			public int _cardCount;
			public bool _random;
			public AddCard(int cardID, CardDestination cardDestination, int cardCount = 1, bool random = false)
			{
				_cardID = cardID;
				_cardDestination = cardDestination;
				_cardCount = cardCount;
				_random = random;
			}
			public enum CardDestination { Hand, Draw, Discard, Deplete }
		}
		public struct AppliedAura
		{
			public int _auraID;
			public int _auraDuration;
			public int _auraStacks;

			public AppliedAura(int auraID, int auraDuration, int auraPower)
			{
				_auraID = auraID;
				_auraDuration = auraDuration;
				_auraStacks = auraPower;
			}
		}
		public struct ApplySummon
		{
			public int _summonID;
			public int _summonDuration;
			public int _summonPower;

			public ApplySummon(int auraID, int auraDuration, int auraPower)
			{
				_summonID = auraID;
				_summonDuration = auraDuration;
				_summonPower = auraPower;
			}
		}
		public void ActivateSpell(CombatCharacter target, CombatCharacter source, Card c)
		{
			Debug.Log("Activating Spell " + _name + " on " + target.GetName());
			float multiplier = c._result == Card.Result.Fail ? 0 : c._result == Card.Result.Crit ? source._stats.GetCritMultiplier() :1;
			if (target.GetType() == typeof(CombatCharacter))
			{
				CombatCharacter cc = (CombatCharacter)target;
				foreach(AppliedAura aa in _applyAuras)
				{
					cc.AddAura(aa,source);
				}
				if (_dealsDamage)
				{
					Debug.Log("Source = " + source.GetName());
					int flatDmg = source.GetDamageEstimate(this, c._result, c.useMeleeWeaponDamage, c.useRangedWeaponDamage);
					Damage dmg = new Damage(flatDmg, this,c._result, Damage.DamageType.Physical, source, cc);
					cc.StartCoroutine(cc.PrepareDamage(dmg));
					//cc.TakeDamage(source.GetDamageEstimate(this,c._result,c.useMeleeWeaponDamage,c.useRangedWeaponDamage),c._result,false,source);
				}
				if (_drawCards > 0)
				{
					int dc = MultiplyInt(_drawCards, multiplier);
					Debug.Log("Draw"+ dc+" Cards");
					cc.DrawCard(dc);
				}
			}
		}

		public SpellBlueprint(List<string> values)
		{
			int i = 1;
			_id = int.Parse(values[i]); i++;
			_name = values[i]; i++;
			_description = values[i]; i++;
			if (values[i].Contains(","))
			{
				List<string> requirementValues = Database.TranslateMultifield(values[i]);
				_requirements = new BlueprintTarget(ParseInt(requirementValues[0]), (BlueprintTarget.Targets)Enum.Parse(typeof(BlueprintTarget.Targets), requirementValues[1]));
			} i++; //Unpack requirements
				   //------------BASIC EFFECTS--------------
			_modHP = new HealthData(values.GetRange(i, 4)); i += 4;
			_setHP = new HealthData(values.GetRange(i, 4)); i += 4;
			_damageBonus = ParseInt(values[i]); i++;
			_useWeaponDamage = ParseBool(values[i]); i++;
			_modInit = ParseInt(values[i]); i++;
			_attributeBonuses = new float[4] { ParseFloat(values[i]), ParseFloat(values[i + 1]), ParseFloat(values[i + 2]), ParseFloat(values[i + 3]) }; i += 4;
			//------------CARD MODIFICATION--------------
			_modCards = ParseBool(values[i]); i++;
			if (_modCards)
			{
				_cardTargeting = new CardTargeting((CardTargeting.CardTarget)Enum.Parse(typeof(CardTargeting.CardTarget), values[i]), ParseInt(values[i + 1]), ParseBool(values[i + 2])); i+=3;			//Unpack Targeting
				_modCardCost = ParseInt(values[i]); i++;
				if (values[i].Contains(","))
				{
					List<string> spellValues = Database.TranslateMultifield(values[i]);
					_modCardAddSpellID = new BlueprintTarget(int.Parse(spellValues[0]), (BlueprintTarget.Targets)Enum.Parse(typeof(BlueprintTarget.Targets), spellValues[1].ToUpper()));
				}i++;			//Unpack ApplySpell
				_modCardPower = ParseInt(values[i]); i++;
				_modCardPurge = ParseBool(values[i]); i++;
			}
			else i += 7;
			//------------ADDING CARDS TO HAND & DECK-------
			if (values[i].Contains(","))
			{
				List<string> tempValues = Database.TranslateMultifield(values[i]);
				_cardRecovery = new CardRecovery((CardRecovery.RecoveryDeck)Enum.Parse(typeof(CardRecovery.RecoveryDeck), tempValues[0]),
				(CardRecovery.RecoveryTarget)Enum.Parse(typeof(CardRecovery.RecoveryTarget), tempValues[1]),
				_drawCards = ParseInt(tempValues[2]), ParseBool(tempValues[3]));
			}i++;        //Unpack CardRecovery
			if (values[i].Contains(","))
			{
				List<string> tempValues = Database.TranslateMultifield(values[i]);
				_addCard = new AddCard(ParseInt(tempValues[0]),
				(AddCard.CardDestination)Enum.Parse(typeof(AddCard.CardDestination), tempValues[1]),
				_drawCards = ParseInt(tempValues[2]), ParseBool(tempValues[3]));
			}i++;        //Unpack AddCard
			_discardCards = ParseInt(values[i]); i++;
			_discardRandom = ParseBool(values[i]); i++;
			_depleteCards = ParseInt(values[i]); i++;
			_depleteRandom = ParseBool(values[i]); i++;
			_drawCards = ParseInt(values[i]); i++;
			//--------------------MOVEMENT AND PUSH---------
			if(values[i]!="")_movementType = (MovementType)Enum.Parse(typeof(MovementType), values[i]); i++;
			_pushAmount = ParseInt(values[i]); i++;
			_pushStopOnCollision = ParseBool(values[i]); i++;
			//--------------------MISCELLANIOUS-------------
			_shuffleDiscardIntoDraw = ParseBool(values[i]); i++;
			_shuffleDrawDeck = ParseBool(values[i]); i++;
			if (values[i].Contains(","))
			{
				List<string> auravalues = Database.TranslateMultifield(values[i]);
				while (auravalues.Count > 2)
				{
					_applyAuras.Add(new AppliedAura(int.Parse(auravalues[0]), int.Parse(auravalues[1]), int.Parse(auravalues[2])));
					auravalues.RemoveRange(0, 3);
				}
				if (auravalues.Count > 0 && auravalues.Count < 3)
				{
					Debug.LogError("Error translating AuraValues " + values[i] + " of spellID " + values[0]);
				}
			} i++; //Unpack ApplyAuras
			if (values[i].Contains(","))
			{
				List<string> summonValues = Database.TranslateMultifield(values[i]);
				_applySummon = new ApplySummon(int.Parse(summonValues[0]), int.Parse(summonValues[1]), int.Parse(summonValues[2]));
			} i++; //Unpack Summon
			_requireSpecialCode = ParseBool(values[i]); i++;

			//-----------------------
			if (_damageBonus > 0 || _useWeaponDamage)
				_dealsDamage = true;
			else
				_dealsDamage = false;
		}

		public struct HealthData
		{
			public int flat;
			public Bfloat fractional;
			public float missing;
			public HealthData(List<string> values)
			{
				flat = ParseInt(values[0]);
				fractional = new Bfloat(ParseFloat(values[1]));
				fractional.current = ParseFloat(values[2]);
				missing = ParseFloat(values[3]);

			}
		}
	}
	public class EnhancementBlueprint:Blueprint
	{//--------------------Time and triggers-------------
		public TriggerType _triggerType { get; protected set; } = TriggerType.constant;   //Whether the Talent never 'ticks' and is instead a constant buff, or 'ticks' at the start or end of turn
		public BlueprintTarget _requirements { get; protected set; }                    //If Requirements are not met, suppress Talent effects
		//---------------------Numbers------------
		public int _modHealth { get; protected set; }                                     //If constant, modify max hp. If triggering, deal damage/heal every turn
		public CombatCharacter.Attributes _attributeMod { get; protected set; }                                //0 = str, 1 = agi, 2 = int, 3 = tena
		public int _armorMod { get; protected set; }
		public int _damageModFlat { get; protected set; }                                 //flat increase or decrease to damage
		public float _damageModFractional { get; protected set; } = 1;                        //1 = full damage, 0 = no damage, 2 = double damage, 0.5 = half damage
		public int _damageModIncomingFlat { get; protected set; }                                 //flat increase or decrease to damage
		public float _damageModIncomingFractional { get; protected set; } = 1;                        //1 = full damage, 0 = no damage, 2 = double damage, 0.5 = half damage
		public bool  _damageModAffectsHeal { get; protected set; }
		//---------------------Effects------------

		//----------Card Modification--------------
		public bool _modCards { get; protected set; } = false;                    //If True, then process the following variables
		public SpellBlueprint.CardTargeting _cardTargeting { get; protected set; }
		public BlueprintTarget _modCardAddSpellID { get; protected set; }				//Add an extra effect to the card, for example gain 2 hp when you play the card
		public int _modCardPower { get; protected set; }                          //cardPower not found in CardData, only in instantiated Cards. Always a default of 0
		public bool _modCardPurge { get; protected set; }                         //Purge() removes any temporary modifications from cards
		//------------ADDING CARDS TO HAND & DECK-------
		public SpellBlueprint.CardRecovery _cardRecovery { get; protected set; }                 //Recover your own cards from deplete or discard
		public SpellBlueprint.AddCard _addCard { get; protected set; }                           //Shuffle new cards to deck or add them to directly to hand
		public int _discardCards { get; protected set; } = 0;
		public bool _discardRandom { get; protected set; } = true;
		public int _depleteCards { get; protected set; } = 0;
		public bool _depleteRandom { get; protected set; } = true;
		public int _drawCards { get; protected set; }
		public bool _shuffleDiscardIntoDraw { get; protected set; } = false;      //Shuffle discarded cards back into the deck
		public bool _shuffleDrawDeck { get; protected set; } = false;             //Shuffle the draw deck

		public Sprite _sprite{ get; protected set; }
		public bool _negative { get; protected set; } = false;
		public bool _requireSpecialCode { get; protected set; }
		*//*
		 *List of Auras requiring special code:
		 * Parry
		 * 
		 *//*

		public EnhancementBlueprint(List<string> values)
		{
			int i = 1;
			_id = int.Parse(values[i]); i++;
			_name = values[i]; i++;
			_description = values[i]; i++;
			//--------------------Time and triggers-------------
			_triggerType = (TriggerType)Enum.Parse(typeof(TriggerType), values[i]);i++;
			if (values[i].Contains(","))
			{
				List<string> requirementValues = Database.TranslateMultifield(values[i]);
				_requirements = new BlueprintTarget(ParseInt(requirementValues[0]), (BlueprintTarget.Targets)Enum.Parse(typeof(BlueprintTarget.Targets), requirementValues[1]));
			}
			i++; //Unpack requirements
			_modHealth = ParseInt(values[i], 0); i++;
			//---------------------Stats and status------------
			_attributeMod = new CombatCharacter.Attributes(ParseInt(values[i], 0),
				ParseInt(values[i + 1], 0), ParseInt(values[i + 2], 0),
				ParseInt(values[i + 3], 0)); i += 4;
			_armorMod = ParseInt(values[i], 0); i++;
			_damageModFlat = ParseInt(values[i], 0); i++;
			_damageModFractional = ParseInt(values[i], 0); i++;
			_damageModIncomingFlat = ParseInt(values[i], 0); i++;
			_damageModIncomingFractional = ParseInt(values[i], 0); i++;
			_damageModAffectsHeal = ParseBool(values[i]); i++;
			//------------CARD MODIFICATION--------------
			_modCards = ParseBool(values[i]); i++;
			if (_modCards)
			{
				_cardTargeting = new SpellBlueprint.CardTargeting((SpellBlueprint.CardTargeting.CardTarget)Enum.Parse(typeof(SpellBlueprint.CardTargeting.CardTarget), values[i]), ParseInt(values[i + 1]), ParseBool(values[i + 2])); i += 3;         //Unpack Targeting
				if (values[i].Contains(","))
				{
					List<string> spellValues = Database.TranslateMultifield(values[i]);
					_modCardAddSpellID = new BlueprintTarget(int.Parse(spellValues[0]), (BlueprintTarget.Targets)Enum.Parse(typeof(BlueprintTarget.Targets), spellValues[1].ToUpper()));
				}
				i++;            //Unpack ApplySpell
				_modCardPower = ParseInt(values[i]); i++;
				_modCardPurge = ParseBool(values[i]); i++;
			}
			else i += 6;
			//------------ADDING CARDS TO HAND & DECK-------
			if (values[i].Contains(","))
			{
				List<string> tempValues = Database.TranslateMultifield(values[i]);
				_cardRecovery = new SpellBlueprint.CardRecovery((SpellBlueprint.CardRecovery.RecoveryDeck)Enum.Parse(typeof(SpellBlueprint.CardRecovery.RecoveryDeck), tempValues[0]),
				(SpellBlueprint.CardRecovery.RecoveryTarget)Enum.Parse(typeof(SpellBlueprint.CardRecovery.RecoveryTarget), tempValues[1]),
				_drawCards = ParseInt(tempValues[2]), ParseBool(tempValues[3]));
			}
			i++;        //Unpack CardRecovery
			if (values[i].Contains(","))
			{
				List<string> tempValues = Database.TranslateMultifield(values[i]);
				_addCard = new SpellBlueprint.AddCard(ParseInt(tempValues[0]),
				(SpellBlueprint.AddCard.CardDestination)Enum.Parse(typeof(SpellBlueprint.AddCard.CardDestination), tempValues[1]),
				_drawCards = ParseInt(tempValues[2]), ParseBool(tempValues[3]));
			}
			i++;        //Unpack AddCard
			_discardCards = ParseInt(values[i]); i++;
			_discardRandom = ParseBool(values[i]); i++;
			_depleteCards = ParseInt(values[i]); i++;
			_depleteRandom = ParseBool(values[i]); i++;
			_drawCards = ParseInt(values[i]); i++;
			_shuffleDiscardIntoDraw = ParseBool(values[i], false); i++;
			_shuffleDrawDeck = ParseBool(values[i],false); i++;

			_negative = ParseBool(values[i], false); i++;
			
			string path = "Icons/" + (values[i] == "" ? "Default" : values[i]);
			//Debug.Log("Aura path = " + path);
			_sprite = UnityEngine.Resources.Load<Sprite>(path); i++;
			_requireSpecialCode = ParseBool(values[i], false); i++;
			//Debug.Log("i = " + i);
		}
		public EnhancementBlueprint() { }
		public enum TriggerType { constant, afterTurn, beforeTurn }

	}
	public class TalentBlueprint: EnhancementBlueprint {
		int _maxRank = 1;
		public TalentBlueprint(List<string> values) : base(values)
		{
			int i = 0;
		}
		public TalentBlueprint()
		{

		}
	 }
	public class AuraBlueprint : EnhancementBlueprint
	{
		//--------------------Time and triggers-------------
		public int _duration { get; protected set; } = 0;                                 //Every time the aura ticks, reduce duration by 1. If constant, reduce duration before turn
		public StackingType _stackingType { get; protected set; } = StackingType.Duration;
		public int _baseStacks { get; protected set; } = 1;								//How many stacks of the buff are included
		public bool _durationIsModHealth { get; protected set; } = false;                 //If true, modHealth will be reduced by 1 every turn instead of duration.
		*//*
		* Auras todo:
		*	Haste
		*		Prevents you from being hastened again. Lasts until the end of the hastened turn.
		*	Slow
		*		Prevents you from being slowed again. Lasts until the end of your slowed turn.
		* 
		*List of Auras requiring special code:
		* Parry
		* Haste/Slow
		*//*

		public enum StackingType { Stack, Duration, None }
		public AuraBlueprint(List<string> values) : base(values)
		{	//Constructors run in order from base class first to inherited class last.
			//Note that initialisers (both static and instance variables) run in the opposite direction. 
			//what should int i be? 0 or 20?
			int i = 36;
			//Debug.Log(values[i] + "," + values[i+1]);
			_duration = ParseInt(values[i], 0); i++;
			_stackingType = (StackingType)Enum.Parse(typeof(StackingType), values[i]);i++;
			_baseStacks = ParseInt(values[i], 0); i++;
			_durationIsModHealth = ParseBool(values[i],false); i++;

		}
		public AuraBlueprint() { }
	}
	public class SummonBlueprint : Blueprint
	{

		public SummonBlueprint(List<string> values)
		{
			int i = 0;
			_id = int.Parse(values[i]); i++;
			_name = values[i]; i++;

		}
	}
}

*/