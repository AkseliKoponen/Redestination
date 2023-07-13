using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RD
{
	[RequireComponent(typeof(Canvas))]
	public class CanvasAdapter : MonoBehaviour
	{
		public static CanvasAdapter _current;
		public enum CanvasType { UI, World, Highlight}
		[Tooltip("Copy settings from which canvas")]
		public CanvasType _canvasType;
		[NonSerialized]public Canvas _canvas;
		private void Awake()
		{
			_current = this;
			_canvas = GetComponent<Canvas>();
		}
		private void Start() {
			if(GameManager._current != null)
				Adapt(_canvasType);
			else
			{
				StartCoroutine(UpdateClock());
			}
		}
		public void Adapt(CanvasType canvasType)
		{
			//Mirrors the values set in the Game Manager global canvases
			int order = _canvas.sortingOrder;
			string name = _canvas.gameObject.name;
			Canvas copycan;
			switch (canvasType)
			{
				case CanvasType.Highlight:
					copycan = GameManager._current._guiParts._highlightCanvas;
					break;
				default:
				case CanvasType.UI:
					copycan = GameManager._current._guiParts._UICanvas;
					break;
				case CanvasType.World:
					copycan = GameManager._current._guiParts._worldCanvas;
					break;

			}
			//Debug.Log("Copying");
			var cr = _canvas.renderMode;
			CodeTools.CopyEverything(copycan, _canvas, typeof(Canvas));
			CodeTools.CopyEverything(copycan.GetComponent<CanvasScaler>(), GetComponent<CanvasScaler>(),typeof(CanvasScaler));
			CodeTools.CopyEverything(copycan.GetComponent<GraphicRaycaster>(), GetComponent<GraphicRaycaster>(), typeof(GraphicRaycaster));
			_canvas.sortingOrder = order;
			_canvas.gameObject.name = name;
		}

		IEnumerator UpdateClock()
		{
			while (true)
			{
				CodeTools.Tm.UpdateDelta(Time.deltaTime);
				yield return null;
			}
		}
	}
}
