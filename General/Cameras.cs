using System.Collections;
using UnityEngine;
using static RD.CodeTools;

namespace RD
{
	public static class Cameras
	{
		public static Camera _uiCamera;
		public static Camera _highlightCamera;
		public static Camera _basicCamera;
		//static LayerMask _basicLayerMask;
		static float defaultCameraSize = 5f;
		public static void Initialize(GameObject cameraParent) {
			_uiCamera = cameraParent.transform.Find("UI Camera").GetComponent<Camera>();
			_highlightCamera = cameraParent.transform.Find("Highlight Camera").GetComponent<Camera>();
			_highlightBlackBars = _highlightCamera.transform.Find("Black Bars");
			_basicCamera = cameraParent.transform.Find("Basic Camera").GetComponent<Camera>();
			//_basicLayerMask = _basicCamera.cullingMask;
		}
		static Color basicBackgroundColor = Color.black;
		public static IEnumerator FlashHighlight(Color? color = null, float duration = 0.5f)
		{
			Color c = color ?? Color.white;
			basicBackgroundColor = _basicCamera.backgroundColor;
			_basicCamera.backgroundColor = c;
			//_postProcessingManager.CritFlash(true);
			_combatGUI.ToggleFlashSettings(true);
			//_highlightCamera.clearFlags = CameraClearFlags.SolidColor;
			ToggleHighlightBars(true);
			if (duration > 0)
			{
				yield return new WaitForSeconds(duration);
				UnflashHighlight();
			}
		}
		public static void UnflashHighlight()
		{
			_basicCamera.backgroundColor = basicBackgroundColor;
			_combatGUI.ToggleFlashSettings(false);
			//_basicCamera.cullingMask = _basicLayerMask;
			//_postProcessingManager.CritFlash(false);
			ToggleHighlightBars(false);
			//_highlightCamera.clearFlags = CameraClearFlags.Nothing;
		}

		static Transform _highlightBlackBars;
		static void ToggleHighlightBars(bool active)
		{
			for(int i = 0; i < _highlightBlackBars.childCount; i++)
			{
				_highlightBlackBars.GetChild(i).gameObject.SetActive(active);
			}
		}
		public static float _camSize = 5;
		public static void SetSize(float size)
		{
			_camSize = size;
			_uiCamera.orthographicSize = size;
			_basicCamera.orthographicSize = size;
			_highlightCamera.orthographicSize = size;
			if (Combat.HealthBarManager._current != null)
			{
				Combat.HealthBarManager._current.UpdateScale();
			}
		}
	}
}
