using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ES3Types;

namespace RD.UI
{
	[RequireComponent(typeof(Image))]
	public class MindSpace : MonoBehaviour
	{
		public CodeTools.GridPoint _point;
		private void Awake()
		{
			_img = GetComponent<Image>();
			if (_locked)
				Lock();
			//GetComponentInChildren<TextMeshProUGUI>().enabled = false;
		}
		Image _img;
		[SerializeField] Color _emptyColor;
		[SerializeField] Color _defaultFillColor;
		[SerializeField] Color _errorColor;
		[SerializeField] Sprite _lockedSprite;
		[SerializeField] Sprite _defaultFrameSprite;
		[SerializeField] Image _frameImg;
		ThoughtUI _thought;
		public bool _locked = false;
		public void Unlock()
		{
			_locked = false;
			_img.sprite = _defaultFrameSprite;
		}
		public void Lock()
		{
			_img.sprite = _lockedSprite;
			_locked = true;
		}
		/// <summary>
		/// Set a <paramref name="thought"/>to this space.
		/// </summary>
		/// <returns>TRUE if not occupied</returns>
		public bool SetThought(ThoughtUI thought)
		{
			bool empty = _thought == null;
			if(empty)
			{
				_thought = thought;
				if (!_thought._occupiedSpaces.Contains(this))
					_thought._occupiedSpaces.Add(this);
				ResetColor();
			}
			else
			{
				_img.color = _errorColor;
			}
			return empty;
		}
		public ThoughtUI GetThought()
		{
			return _thought;
		}
		public void ResetColor()
		{
			_img.color = _thought==null? _emptyColor : _defaultFillColor;
		}
		public void Unfill()
		{
			if (_thought != null)
				_thought = null;
			ResetColor();
		}
	}
}