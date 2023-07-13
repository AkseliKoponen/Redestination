using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RD.UI
{
	public class MapMover : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		//public Vector3 max;
		//public Vector3 min;
		public static MapMover _current;
		public static MapNode _currentTooltip;
		public CodeTools.Bfloat scaleLimit;
		public float moveSpeed = 20f;
		RectTransform _rt;
		RectTransform _canvas;
		void Awake()
		{
			_current = this;
		}
		private void Start()
		{
			//_inputs.UI.ScrollWheel.performed += ScrollScale;
			_rt = GetComponent<RectTransform>();
			_canvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
			TotalZoomOut();
		}
		//Vector3 clickPos;
		//Vector3 clickRtPos;
		// Update is called once per frame
		Vector2 movement;
		void Update()
		{
			_rt.anchoredPosition3D += new Vector3(movement.x * -moveSpeed, movement.y * -moveSpeed, 0);
			KeepPositionWithinBounds();
		}
		Vector3 MousePos()
		{
			return _canvas.GetComponent<Canvas>().ScreenToCanvasPosition(Mouse.current.position.ReadValue());
		}
		public void ScrollScale(InputAction.CallbackContext context)
		{
			Vector2 scroll = context.ReadValue<Vector2>();
			float zoom = scroll.y / 2000f;
			Vector3 scale = _rt.localScale + Vector3.one * zoom;
			scale = CodeTools.ClampVector3(scale, scaleLimit.min, scaleLimit.max);
			Vector2 mousePos = MousePos();
			float height = _rt.localScale.y * _rt.sizeDelta.y;
			float width = _rt.localScale.x * _rt.sizeDelta.x;
			Vector2 pivot = new Vector2();
			pivot.x = (mousePos.x - _rt.anchoredPosition3D.x) / width + 0.5f;
			pivot.y = (mousePos.y - _rt.anchoredPosition3D.y) / height + 0.5f;
			_rt.SetPivot(pivot);
			_rt.localScale = scale;
			_rt.SetPivot(Vector2.one * 0.5f);
			KeepPositionWithinBounds();
			if (_currentTooltip != null)
				_currentTooltip.ShowTooltip();

		}
		public void KeyMovement(InputAction.CallbackContext context)
		{
			if (context.canceled)
				movement = Vector2.zero;
			else
				movement = context.ReadValue<Vector2>();
		}
		void KeepPositionWithinBounds()
		{
			Vector3 pos = _rt.anchoredPosition3D;
			float height = _rt.localScale.y * _rt.sizeDelta.y;
			float width = _rt.localScale.x * _rt.sizeDelta.x;
			float cheight = _canvas.sizeDelta.y;// * canvas.GetComponent<Canvas>().scaleFactor;
			float cwidth = _canvas.sizeDelta.x;// * canvas.GetComponent<Canvas>().scaleFactor;
			Vector2 wiggleRoom = new Vector2();
			wiggleRoom.y = (height - cheight) / 2;
			wiggleRoom.x = (width - cwidth) / 2;
			//Debug.Log("Wiggle = "+wiggleRoom);
			pos.x = Mathf.Clamp(pos.x, -wiggleRoom.x, wiggleRoom.x);
			pos.y = Mathf.Clamp(pos.y, -wiggleRoom.y, wiggleRoom.y);
			pos.z = 0;
			_rt.anchoredPosition3D = pos;
		}

		void TotalZoomOut()
		{
			transform.position = Vector3.zero;
			float xMax = _rt.sizeDelta.x / _canvas.sizeDelta.x;
			float yMax = _rt.sizeDelta.y / _canvas.sizeDelta.y;
			float scale = 1/(xMax > yMax ? yMax : xMax);
			//Debug.Log(scale);
			scaleLimit.min = scale;
			transform.localScale = Vector3.one * scale;
			KeepPositionWithinBounds();
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			//Debug.Log("Begin Drag");
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			//Debug.Log("End Drag");
		}

		public void OnDrag(PointerEventData eventData)
		{
			//Debug.Log("Drag");
			_rt.anchoredPosition += eventData.delta;
			KeepPositionWithinBounds();
		}
	
		public void Click(InputAction.CallbackContext context)
		{
			if (CodeTools.IsMouseClick(context,false))
			{
				//Debug.Log("MapMover Click");
				if(!MapNode._disableLocking)
					StartCoroutine(HideTooltips());
			}
		}
		IEnumerator HideTooltips()
		{
			int frame = 1;
			while (frame > 0)
			{
				frame--;
				yield return null;
			}
			if (!_preventHide)
			{
				TooltipSystem.HideAllTooltips();
				foreach (MapNode mn in FindObjectsOfType<MapNode>())
				{
					mn.HideTooltip();
				}
			}
			else
				_preventHide = false;
		
		}
		public bool _preventHide;
	}
}
