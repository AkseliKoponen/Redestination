using System.Collections;
using RD.Combat;
using RD.DB;
using TMPro;
using UnityEngine;

namespace RD
{
	public class Tooltip : MonoBehaviour
	{
		[SerializeField]TextMeshProUGUI _nameText;
		[SerializeField]TextMeshProUGUI _descriptionText;
		[SerializeField] TextMeshProUGUI _itemType;
		[SerializeField] GUIInventorySlot _itemImage;
		[SerializeField] CanvasGroup _cg;
		public bool locked = false;
		string totalText;
		int maxLines = 5;
		public void SetMaxLines(int max = 5)
		{
			if (max <= 0)
				max = 99999;
			maxLines = max;
		}
		#region Toggle
		public void Toggle(BaseObject link)
		{
			gameObject.SetActive(true);
			_nameText.text = link._name;
			totalText = link.GetDescription();
			SetDescription();
		}
		public void Toggle(Item item)
		{
			gameObject.SetActive(true);
			_nameText.text = item._name;
			totalText = item.GetItemDescription();
			SetDescription();
			_itemImage.AddItem(item);
			_itemType.text = item.GetTypeName();
		}
		public void Toggle(Weapon item)
		{
			gameObject.SetActive(true);
			_nameText.text = item._name;
			totalText = item.GetItemDescription();
			SetDescription();
			_itemImage.AddItem(item);
			_itemType.text = item.GetTypeName();
		}
		public void Toggle(Equipment item)
		{
			gameObject.SetActive(true);
			_nameText.text = item._name;
			totalText = item.GetItemDescription();
			SetDescription();
			_itemImage.AddItem(item);
			_itemType.text = item.GetTypeName();
		}
		public void Toggle(Consumable item)
		{
			gameObject.SetActive(true);
			_nameText.text = item._name;
			totalText = item.GetItemDescription();
			SetDescription();
			_itemImage.AddItem(item);
			_itemType.text = item.GetTypeName();
		}
		#endregion
		public string GetName()
		{
			return _nameText.text;
		}
		public string GetDescription()
		{
			return totalText;
		}
		void SetDescription()
		{
			totalText = Translator.PrepGenericText(totalText);
			_descriptionText.text = totalText;
			_descriptionText.maxVisibleLines = maxLines;
			if (maxLines > 0)
				StartCoroutine(ClampTooltip());
			//_descriptionText.maxVisibleLines = maxLines;
			//SetHeight();
		}
		IEnumerator ClampTooltip()
		{
			int frameWait = 1;
			while (frameWait > 0)
			{
				frameWait--;
				yield return null;
			}
			int lineCount = _descriptionText.textInfo.lineCount;
			if (lineCount > maxLines)
			{
				//Debug.Log("LineCount = " + lineCount + "\nMaxlines = " + maxLines);
				int charIndex = _descriptionText.textInfo.lineInfo[maxLines - 1].lastCharacterIndex;
				string tempText = totalText.Substring(0, charIndex - 1);
				charIndex = tempText.LastIndexOf(' ');
				//Debug.Log("Total Line length = " + totalText.Length + "\n and " + maxLines + "th line lastCharIndex = " + charIndex);
				//find previous space
				tempText = totalText.Substring(0, charIndex) + " ...";
				_descriptionText.text = tempText;
			}
		}
		public float GetDescriptionHeight()
		{
			return _descriptionText.preferredHeight;
		}
		public float GetHeight()
		{
			return GetComponent<RectTransform>().sizeDelta.y;
		}
		public float GetWidth()
		{
			return GetComponent<RectTransform>().sizeDelta.x;
		}
		public bool Hide()
		{
			if (!locked)
				gameObject.SetActive(false);
			return !locked;
		}
		public void Lock()
		{
			locked = true;
			_cg.ignoreParentGroups = true;
		}
		public void Unlock()
		{
			locked = false;
			_cg.ignoreParentGroups = false;
			Hide();
		}
	}
}
