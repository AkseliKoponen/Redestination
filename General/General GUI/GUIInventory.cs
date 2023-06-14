using System.Collections.Generic;
using RD.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace RD
{
	public class GUIInventory : MonoBehaviour
	{
		[SerializeField] GameObject _inventoryParent;
		[SerializeField] Button _closeButton;
		[SerializeField]Canvas _canvas;
		[SerializeField] GUIInventorySlot _inventorySlotPrefab;
		Inventory _inventory;
		List<GUIInventorySlot> _inventorySlots;

		void Init()
		{
			_inventorySlots = new List<GUIInventorySlot>();
			_inventorySlots.AddRange(GetComponentsInChildren<GUIInventorySlot>());
		}
		public void DisplayInventory(Inventory inventory)
		{
			if (_inventorySlots == null)
			{
				Init();
			}
			foreach(GUIInventorySlot invs in _inventorySlots)
			{
				invs.Clear();
			}
			_canvas.gameObject.SetActive(true);
			_inventory = inventory;
			foreach (GUIInventorySlot child in _inventorySlots)
				child.gameObject.SetActive(true);
			for (int i = _inventorySlots.Count-1; i >= _inventory._maxCapacity-1; i--)
				_inventorySlots[i].gameObject.SetActive(false);
			{
				int i = 0;
				for (int k = 0; k < _inventory._weapons.Count && k + i < transform.childCount; k++)
					_inventorySlots[k+i].AddItem(_inventory._weapons[k]);
				i += _inventory._weapons.Count;
				for (int k = 0; k < _inventory._equipment.Count && k + i < transform.childCount; k++)
					_inventorySlots[k+i].AddItem(_inventory._equipment[k]);
				i += _inventory._equipment.Count;
				for (int k = 0; k < _inventory._consumables.Count && k + i < transform.childCount; k++)
					_inventorySlots[k+i].AddItem(_inventory._consumables[k]);
				i += _inventory._consumables.Count;
				for (int k = 0; k < _inventory._items.Count && k + i < transform.childCount; k++)
					_inventorySlots[k+i].AddItem(_inventory._items[k]);
			}
		}

		public void Close()
		{
			_canvas.gameObject.SetActive(false);
		}
	}
}
