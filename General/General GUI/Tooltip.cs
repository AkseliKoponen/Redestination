using System.Collections;
using RD.Combat;
using RD.DB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
		int maxChars = 135;
		public void SetMaxLines(int max = 5)
		{
			if (max <= 0)
				max = 99999;
			maxLines = max;
		}
		#region Toggle
		public void Toggle(BaseObject obj)
		{
			gameObject.SetActive(true);
			if (_nameText.text.Equals(obj._name))
				return;
			_nameText.text = obj._name;
			totalText = obj.GetLimitedDescription(maxChars);
			SetDescription();
		}
		public void Toggle(Item obj)
		{
			gameObject.SetActive(true);
			if (_nameText.text.Equals(obj._name))
				return;
			_nameText.text = obj._name;
			totalText = obj.GetLimitedDescription(maxChars);
			SetDescription();
			_itemImage.AddItem(obj);
			_itemType.text = obj.GetTypeName();
		}
		public void Toggle(Weapon obj)
		{
			gameObject.SetActive(true);
			if (_nameText.text.Equals(obj._name))
				return;
			_nameText.text = obj._name;
			totalText = obj.GetLimitedDescription(maxChars);
			SetDescription();
			_itemImage.AddItem(obj);
			_itemType.text = obj.GetTypeName();
		}
		public void Toggle(Equipment obj)
		{
			gameObject.SetActive(true);
			if (_nameText.text.Equals(obj._name))
				return;
			_nameText.text = obj._name;
			totalText = obj.GetLimitedDescription(maxChars);
			SetDescription();
			_itemImage.AddItem(obj);
			_itemType.text = obj.GetTypeName();
		}
		public void Toggle(Consumable obj)
		{
			gameObject.SetActive(true);
			if (_nameText.text.Equals(obj._name))
				return;
			_nameText.text = obj._name;
			totalText = obj.GetLimitedDescription(maxChars);
			SetDescription();
			_itemImage.AddItem(obj);
			_itemType.text = obj.GetTypeName();
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
			_descriptionText.maxVisibleLines = maxLines;
			_descriptionText.text = totalText;
			_descriptionText.ForceMeshUpdate();
			LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
			//_descriptionText.maxVisibleLines = maxLines;
			//SetHeight();
		}
		#region DeleteMe
		void ClampTooltip()
		{
			/*
			int frameWait = 1;
			while (frameWait > 0)
			{
				frameWait--;
				yield return null;
			}*/
			int lineCount = _descriptionText.textInfo.lineCount;
			_descriptionText.text = totalText;
			_descriptionText.ForceMeshUpdate();
			//if (lineCount > maxLines){
			//Debug.Log("LineCount = " + lineCount + "\nMaxlines = " + maxLines);
			int charIndex = _descriptionText.textInfo.lineInfo[maxLines - 1].lastCharacterIndex;
			Debug.Log(charIndex +" vs "+_descriptionText.text.Length);
			//if (charIndex < _descriptionText.text.Length){
				string tempText = _descriptionText.text.Substring(0, charIndex - 1);
				charIndex = tempText.LastIndexOf(' ');
				//Debug.Log("Total Line length = " + totalText.Length + "\n and " + maxLines + "th line lastCharIndex = " + charIndex);
				//find previous space
				tempText = totalText.Substring(0, charIndex) + " ...";
				_descriptionText.text = tempText;
			//}
			//_descriptionText.ForceMeshUpdate();
			LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
			TooltipSystem.Refresh();
			//}
		}
#endregion
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
