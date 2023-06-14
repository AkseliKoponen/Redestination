using System;
using System.Collections.Generic;
using RD.Combat;
using UnityEngine;

namespace RD.DB
{
	public class BaseItem : BaseObject
	{

		/*
	 * Inheriting from BaseItem
	 * ->Equipment
	 * ->->Weapon
	 * ->Consumable
	 * 
	*/
		public int _price;
		public Sprite _artSprite;
		protected Combat.CombatCharacter _owner;
		protected Inventory _inventory;
		public ItemFlags _itemFlags;
		//public BaseCard.EffectTarget _onUse;

		//[System.Flags]
		[Serializable]
		public enum ItemFlags
		{
			None = 0,
			Armor = 1,
			MeleeWeapon = 2,
			RangedWeapon = 4,
			Jewelry = 8,
			Healing = 16,
			Supplies = 32,
			Curiosity = 64,
			Magical = 128
		}



		public BaseItem()
		{

			_layOutSpace = new LayOutSpace(new List<int> { 1, 3 }, new List<string> { "Item","General" });
			//_layOutSpace.AddLayOutSpace(new LayOutSpace(),false);
		}



		public string GetItemDescription()
		{
			if (GetType() == typeof(Weapon))
			{
				Weapon wpn = (Weapon)this;
				return wpn.GetWeaponDescription();
			}
			else
			{
				return _price > 0 ? "Value: " + _price + "\n" + GetDescription() : GetDescription();
			}
		}
		public void SetOwner(CombatCharacter owner)
		{
			_owner = owner;
		}
	}
}
