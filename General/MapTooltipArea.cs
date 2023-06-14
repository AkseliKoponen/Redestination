using RD.DB;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RD
{
	[RequireComponent(typeof(Image))]
	public class MapTooltipArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler
	{
		MapMover _mapMover;
		public Hyperlink _tooltip;
		Image _img;
		public bool _visible;
		public bool _locked = false;
		LerpTransform lerper;
		private void Awake()
		{
			_img = GetComponent<Image>();
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
			_locked = true;
			_mapMover._preventHide = true;
		}
		void ShowTooltip()
		{
			if (_tooltip)
			{
				lerper.StartLerpScale(Vector3.one * 1.1f, 5);
				TooltipSystem.DisplayTooltip(_tooltip, CodeTools.GetRecttransformPivotPoint(GetComponent<RectTransform>(), new Vector2(1f, 0f), true));
				_visible = true;
			}
			else
			{
				Debug.Log("Unimplemented Tooltip @ " + gameObject.name);
			}
		}
		public void HideTooltip()
		{
		
			if (_visible && !_locked)
			{
				lerper.StartLerpScale(Vector3.one, 5);
				_visible = false;
				TooltipSystem.HideAllTooltips();
			}
		}
	}
}
