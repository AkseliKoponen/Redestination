using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static RD.CodeTools;
namespace RD.UI
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class LoaderText : MonoBehaviour
	{
		TextMeshProUGUI _textMesh;
		public UIGradient _gradient;
		public Image _fillImage;
		public bool _running = false;
		public float minimumLoadTime = 2f;
		public Corner _corner;
		float _maxBrightness = 0.5f;
		private void Awake()
		{
			_textMesh = GetComponent<TextMeshProUGUI>();
		}
		float loadTime = 0;
		void Update()
		{
			if (_running)
			{
				loadTime = Mathf.Clamp(loadTime + Tm.GetUIDelta(), 0, minimumLoadTime);
				float f = Mathf.Lerp(0, 1, loadTime / minimumLoadTime);
				if (_fillImage) _fillImage.fillAmount = f;
				if (_gradient)
				{
					Color c = GetGradientColor(_gradient.GetComponent<Image>().color,f);//Color.white;
					if(false) switch (_corner)
					{
						case Corner.BottomLeft:
							c = GetGradientColor(_gradient.CornerColorLowerLeft, f);
							_gradient.CornerColorLowerLeft = c;
							break;
						case Corner.BottomRight:
							c = GetGradientColor(_gradient.CornerColorLowerRight, f);
							_gradient.CornerColorLowerRight = c;
							break;
						case Corner.TopLeft:
							c = GetGradientColor(_gradient.CornerColorUpperLeft, f);
							_gradient.CornerColorUpperLeft = c;
							break;
						case Corner.TopRight:
							c = GetGradientColor(_gradient.CornerColorUpperRight, f);
							_gradient.CornerColorUpperRight = c;
							break;
					}
					GetComponentInParent<LoaderParent>().SetColors(c);
				}
				int i = Mathf.RoundToInt(f * 100);
				if (loadTime >= minimumLoadTime)
				{
					i = 100;
					LoadReady();
					
				}
				_textMesh.text = i + "%";
				
				
			}
			void LoadReady()
			{
				_running = false;
				_fillImage.fillAmount = 1;
				foreach (LoaderParent lp in FindObjectsOfType(typeof(LoaderParent)))
				{
					lp.LoadReady();
				}
			}
			Color GetGradientColor(Color c,float f)
			{

				Vector3 hsv = GetHSV(c);
				hsv.z = Mathf.Lerp(0, _maxBrightness, f);
				return Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
			}
		}
	}
}
