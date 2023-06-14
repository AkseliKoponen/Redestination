using RD.Combat;
using System;
using System.Collections.Generic;
using UnityEngine;
using static RD.CodeTools;

namespace RD.DB
{
	public class BaseCombatCharacter : BaseObject
	{
		public Combat.CombatCharacter.Alliance _characterType = Combat.CombatCharacter.Alliance.Enemy;
		public int _level = 1;
		public int _hp;
		public int _initiative;
		public Attributes _attributes = new Attributes();
		public List<BaseTalent> _talents = new List<BaseTalent>();
		public RD.UI.TalentTree _talentTree;
		public List<BaseCard> _deck = new List<BaseCard>();
		public BaseCard _specialActionCard;
		[Tooltip("Uses Item ID")]

		//public int _meleeWeapon = -1;
		//public int _rangedWeapon = -1;
		public BaseWeapon _meleeWeapon;
		public BaseWeapon _rangedWeapon;
		//public Sprite _turnIcon;
		public CombatCharacter _visualObject;
		//public DialogCharacter _dialogCharacter;
		[Tooltip("Max Length 15")] public string _nameAbbreviation="";

		[Serializable]
		public class Attributes
		{
			public int strength = 0;
			public int dexterity = 0;
			public int intelligence = 0;
			public int tenacity = 0;
			public Attributes()
			{
				strength = 0;
				dexterity = 0;
				intelligence = 0;
				tenacity = 0;
			}
			public int GetInitiative(int baseInitiative = 0)
			{
				return baseInitiative + (dexterity*1);
			}
		}
		public int CalculateMaxHP()
		{
			return _hp + (_level * 3) + (5 * _attributes.tenacity);
		}
		public BaseCombatCharacter()
		{
			_layOutSpace = new LayOutSpace(new List<int> { 1 }, new List<string> {"General" });
			_deck.Capacity = 6;
		}


		public CombatCharacter Instantiate()
		{
			CombatCharacter cc = Instantiate(_visualObject);
			cc.Initialize(this);
			return cc;
		}
	}
}
