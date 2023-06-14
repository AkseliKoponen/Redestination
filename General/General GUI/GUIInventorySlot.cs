using RD.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace RD
{
	public class GUIInventorySlot : MonoBehaviour
	{

		Item _item;
		Weapon _wpn;
		Equipment _equip;
		Consumable _cons;
		[SerializeField] Image _itemIcon;

		public void AddItem(Item item)
		{
			Clear();
			_item = item;
			_itemIcon.sprite = item._artSprite;
			IconToggle();
		}
		public void AddItem(Weapon item)
		{
			Clear();
			_wpn = item;
			_itemIcon.sprite = item._artSprite;
			IconToggle();
		}
		public void AddItem(Equipment item)
		{
			Clear();
			_equip = item;
			_itemIcon.sprite = item._artSprite;
			IconToggle();
		}
		public void AddItem(Consumable item)
		{
			Clear();
			_cons = item;
			_itemIcon.sprite = item._artSprite;
			IconToggle();
		}

		void IconToggle()
		{
			_itemIcon.gameObject.SetActive(true);
			_itemIcon.color = Color.white;
		}
		public void Clear()
		{
			_item = null;
			_wpn = null;
			_equip = null;
			_cons = null;
			_itemIcon.gameObject.SetActive(false);
		}

		public void Mouseover()
		{
			Vector2 pos = CodeTools.GetRecttransformPivotPoint(GetComponent<RectTransform>(), new Vector2(1, 0.5f),false);
			pos = Camera.main.WorldToScreenPoint(pos);
			if(_item)
				TooltipSystem.DisplayTooltip(_item,pos);
			else if(_equip)
				TooltipSystem.DisplayTooltip(_equip, pos);
			else if (_wpn)
				TooltipSystem.DisplayTooltip(_wpn, pos);
			else if (_cons)
				TooltipSystem.DisplayTooltip(_cons, pos);
		}
		public void MouseExit()
		{
			TooltipSystem.HideAllTooltips();
		}
	}
}
