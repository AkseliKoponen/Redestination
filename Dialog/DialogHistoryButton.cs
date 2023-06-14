using UnityEngine;
using UnityEngine.EventSystems;

namespace RD.Dialog
{
	public class DialogHistoryButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public DialogPanel _dialogPanel;
		public void Awake()
		{
			if (!_dialogPanel)
				_dialogPanel = GetComponentInParent<DialogPanel>();
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_dialogPanel.ToggleHistory(false);
			//Debug.Log("End Drag");
		}
		public void OnBeginDrag(PointerEventData eventData)
		{
			_dialogPanel.ToggleHistory(true);
		}
		public void OnDrag(PointerEventData eventData)
		{
			_dialogPanel.DragHistoryButton(eventData.delta.y);
		}
	}
}
