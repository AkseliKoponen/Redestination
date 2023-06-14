using UnityEngine;
using UnityEngine.UI;

namespace RD
{
	[RequireComponent(typeof(Image))]
	public class ResizePanel : MonoBehaviour
	{
		#region Disabled
		/*
	bool isDialogPanel = false;
	DialogPanel _dialogPanel;
	Image _image;
	RectTransform _rt;
	Vector2 _originalDimensions;
	public bool enableLeft = false;
	public bool enableRight = false;
	public bool enableUp = false;
	public bool enableDown = false;
	
	private void Start()
	{
		Initialize();
		enableUp = true;
	}
	
	public void Initialize()
	{
		if (GetComponent<DialogPanel>() != null)
		{
			_dialogPanel = GetComponent<DialogPanel>();
			isDialogPanel = true;
		}
		_image = GetComponent<Image>();
		_rt = GetComponent<RectTransform>();
		_originalDimensions = _rt.sizeDelta;
	}

	private void Update()
	{
		return;
		if (enableUp) {

			Vector2 mouseDelta = Mouse.current.delta.ReadValue();
			float deltaY = mouseDelta.y;

			if (_draggable && Mathf.Abs(deltaY)>0)
			{
				_rt.sizeDelta = new Vector2(_rt.sizeDelta.x,Mathf.Clamp(_rt.sizeDelta.y+deltaY,_originalDimensions.y,1000));
				if (isDialogPanel)
					_dialogPanel.Refresh();

			}
			//detect if mouse is dragging


		}
		if (enableLeft) { }

	}

	public void Reset(bool resetDimension = false)
	{
		enabled = false;
		if(resetDimension)_rt.sizeDelta = _originalDimensions;
	}

	public void EnableCursorDrag()
	{
		_draggable = true;
		_cursorOnArea = true;
		GlobalSettings.SetDragCursor();

	}
	bool _draggable = false;
	bool _cursorOnArea = false;
	*/
		#endregion
		#region DisabledBecauseOfNewInput
		/*
	public void DisableCursorDrag()
	{
		_cursorOnArea = false;
		if (Input.GetAxis("MouseLeft") > 0)
			StartCoroutine(WaitToDisableDrag());
		else
		{
			GlobalSettings.ResetCursor();
			_draggable = false;
		}
	}

	IEnumerator WaitToDisableDrag()
	{
		while (Input.GetAxis("MouseLeft") > 0)
			yield return null;
		if (!_cursorOnArea)
		{
			GlobalSettings.ResetCursor();
			_draggable = false;
		}
	}*/
		#endregion
	}
}
