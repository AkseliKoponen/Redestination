using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static RD.UIAnimationTools;
namespace RD.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class LoaderParent : MonoBehaviour
	{
		CanvasGroup _cg;
		public int order = 0;
		public float fadeSpeed = 4;
		public Image _connectionImage;
		LoaderText lt;
		bool ran = false;
		bool last = false;
		private void Awake()
		{
			_cg = GetComponent<CanvasGroup>();
			lt = GetComponentInChildren<LoaderText>();
			if (order == 3)
				last = true;
			if (order <= 0)
				StartCoroutine(TurnOn());
		}
		public void LoadReady()
		{
			//Debug.Log(gameObject.name + "LoadReady()");
			order--;
			if (order <= 0 && !ran)
			{
				StartCoroutine(TurnOn());
			}
			if (order == -1)
			{
				_connectionImage.color = GetComponent<Image>().color;
				StartCoroutine(ImageTransitionFill(_connectionImage, 0.5f));
				if (last)
				{
					Invoke("Launch", 1);
				}
			}
		}
		void Launch()
		{
			float scale = 5;
			StartCoroutine(FadeCanvasGroupAlpha(transform.parent.GetComponent<CanvasGroup>(), false, 1, false, scale));
		}
		IEnumerator TurnOn()
		{
			ran = true;
			Task t = new Task(UIAnimationTools.FadeCanvasGroupAlpha(_cg, true, fadeSpeed, false));
			while (t.Running)
			{
				yield return null;
			}
			lt._running = true;
		}
		public void SetColors(Color c)
		{
			GetComponent<Image>().color = c;
			foreach (Image img in GetComponentsInChildren<Image>())
				img.color = c;
			foreach (TextMeshProUGUI tmp in GetComponentsInChildren<TextMeshProUGUI>())
				tmp.color = c;
		}
	}
}
