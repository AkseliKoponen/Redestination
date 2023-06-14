using System.Collections;
using System.Collections.Generic;
using RD;
using RD.DB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RD
{
	public class NewGameHandler : MonoBehaviour
	{
		public float _delay = 1;

	
		public Canvas _occupationCanvas;
		public CanvasGroup _occupationGroup;
		public TextMeshProUGUI _occupationTitle;
		public List<GameObject> _occupations;
		public bool _debug = false;
		// Start is called before the first frame update
		void Start()
		{
			StartCoroutine(OccupationSelection());
		}
		/*
	public GameObject _textPrefab;
	public List<SplashableText.SplashText> _texts;
	public bool _randomOrder = false;
	public bool _hideCursor = false;
	IEnumerator TextSplasher(float delayStart)
	{
		if (!_debug)
		{
			MusicManager.FadeToggle(false, 0.25f);
			bool cursorvisibility = UnityEngine.Cursor.visible;
			if (_hideCursor)
			{
				UnityEngine.Cursor.visible = false;
			}
			while (delayStart > 0)
			{
				delayStart -= Time.GetUITime();
				yield return null;
			}
			int i = 0;
			float time = 0;
			while (i < _texts.Count)
			{
				Instantiate(_textPrefab, transform).GetComponent<SplashableText>().Init(_texts[i]);
				time += _texts[i].duration + _texts[i].waitAfter;
				while (time > 0)
				{
					time -= Time.GetUITime();
					yield return null;
				}
				i++;
			}
			MusicManager.FadeToggle(true, 0.5f);
			time = 2f;
			while (time > 0)
			{
				time -= Time.GetUITime();
				yield return null;
			}
			if (_hideCursor)
			{
				UnityEngine.Cursor.visible = cursorvisibility;
			}
		}

		SceneHandler.EnterScene("storybook");
	}
	*/
		string _occupation = "";
		IEnumerator OccupationSelection()
		{
			_occupationCanvas.gameObject.SetActive(true);
			//Debug.Break();
			StartCoroutine(DialogTools.WriteText(_occupationTitle.text, _occupationTitle));
			_occupationGroup.alpha = 0;
			float wait = 1;
			while (wait > 0)
			{
				wait -= CodeTools.Tm.GetGlobalDelta();
				yield return null;
			}
			StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(_occupationGroup, true, 5));

			foreach (GameObject o in _occupations)
			{
				//Debug.Log(o.gameObject.name);
				TextMeshProUGUI tm = o.transform.Find("Text Box").Find("Description").GetComponent<TextMeshProUGUI>();
				string txt = tm.text;
				tm.text = "";
				tm.ForceMeshUpdate();
				StartCoroutine(DialogTools.WriteText(txt, tm,(float)txt.Length/30f));
			}
			while (_occupation == "")
			{
				yield return null;
			}
		
			foreach (GameObject o in _occupations)
			{
				CanvasGroup cg = o.GetComponent<CanvasGroup>();
				cg.interactable = false;
				cg.blocksRaycasts = false;
				if (!o.name.Equals(_occupation.ToString()))
				{
					StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(cg, false, 5));

				}
				o.GetComponentInChildren<Button>().gameObject.SetActive(false);
			}
		
			wait = 1;
			while (wait > 0)
			{
				wait -= CodeTools.Tm.GetGlobalDelta();
				yield return null;
			}
			{
				CanvasGroup cg = _occupationCanvas.GetComponent<CanvasGroup>();
				cg.interactable = false;
				cg.blocksRaycasts = false;
				StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(cg, false, 5));
			}
			wait = 1;
			while (wait > 0)
			{
				wait -= CodeTools.Tm.GetGlobalDelta();
				yield return null;
			}
			GameManager._current._storyVariables._occupation = _occupation;
			SceneHandler.EnterBaseScene((SceneConstructor)CodeTools.db.Get<SceneConstructor>(1));
		}

		public void SetOccupation(string oc)
		{
			_occupation = oc.ToLower();
		}

	}
}