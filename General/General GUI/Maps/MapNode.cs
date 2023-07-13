using RD.DB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RD.UI
{
	

	[RequireComponent(typeof(Image))]
	public class MapNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler
	{
		public Sprite _highlightSprite;
		Sprite _defaultSprite;
		MapMover _mapMover;
		public Hyperlink _tooltip;
		Image _img;
		public static bool _disableLocking = true;
		public bool _visible;
		public bool _locked = true;
		LerpTransform lerper;
		private void Awake()
		{
			_img = GetComponent<Image>();
			_defaultSprite = _img.sprite;
			lerper = GetComponent<LerpTransform>();
			_mapMover = GetComponentInParent<MapMover>();
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			ShowTooltip();
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			HideTooltip();
		}
		public void OnPointerClick(PointerEventData pointerEventData)
		{
			if (!_locked)
			{
				TravelTo();
			}
		}
		public void TravelTo()
		{
			RoadSystem._current.TravelTo(this);
		}
		public void Highlight()
		{
			if (_highlightSprite)
			{
				_img.sprite = _highlightSprite;
			}
			_locked = false;
		}
		public void Lowlight()
		{
			_locked = true;
			_img.sprite = _defaultSprite;
		}
		public void ShowTooltip()
		{
			if (_tooltip)
			{
				if(!_visible)lerper.StartLerpScale(Vector3.one * 1.1f, 5);
				Vector2 pos = CodeTools.GetRecttransformPivotPoint(GetComponent<RectTransform>(), new Vector2(1f, 0f), true);
				if (!_visible) TooltipSystem.DisplayTooltip(_tooltip, pos);
				else TooltipSystem.DisplayTooltips(new List<BaseObject>() { _tooltip }, pos, null, true);
				MapMover._currentTooltip = this;
				_visible = true;
			}
			else
			{
				Debug.Log("Unimplemented Tooltip @ " + gameObject.name);
			}
		}
		public void HideTooltip()
		{
		
			if (_visible)
			{
				if (MapMover._currentTooltip == this)
					MapMover._currentTooltip = null;
				lerper.StartLerpScale(Vector3.one, 5);
				_visible = false;
				TooltipSystem.HideAllTooltips();
			}
		}

#if UNITY_EDITOR
		void OnDrawGizmosSelected()
		{
			RoadSystem rs = FindObjectOfType<RoadSystem>();
			if (rs != null)
			{
				rs.DrawMapNode(this);
			}
		}
#endif

		}
}
