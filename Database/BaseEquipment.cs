using System;
using System.Collections.Generic;
using RD.Combat;
using UnityEngine;

namespace RD.DB
{
	public class BaseEquipment : BaseItem
	{
		public EquipmentType _equipmentSlot = EquipmentType.None;
		[Tooltip("Passive Effects are always on when equipped.")]
		public List<BaseTalent> _talents = new List<BaseTalent>();
		public List<BaseCard> _addCards = new List<BaseCard>();          //Shuffle additional cards
		[Serializable]
		public enum EquipmentType
		{
			None = 0,
			Armor = 1,
			Ring = 2,
			Amulet = 4,
			Talisman = 8,
			Ranged = 16,
			Melee = 32
		}
		public BaseEquipment()
		{
			_layOutSpace = new LayOutSpace(new List<int> { 1, 3, 3 }, new List<string> { "Equipment", "Item","General" });
		}
		bool _equipped;
		public void Equip(CombatCharacter equipper)
		{
			_owner = equipper;
			_inventory = _owner._inventory;
			{
				BaseEquipment prevEq = _owner._inventory.GetEquipmentFromSlot(_equipmentSlot);
				if (prevEq != null)
					_owner._inventory.Unequip(prevEq);
			}
			foreach (BaseTalent bt in _talents)
			{
				//add extra talents
			}
			foreach (BaseCard bc in _addCards)
			{
				//add extra cards
			}
			_equipped = true;
		}
		
	}
}
