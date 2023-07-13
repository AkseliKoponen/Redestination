using System.Collections;
using RD;
using TMPro;
using UnityEngine;
using static RD.Dialog.Inkerface;
using static RD.CodeTools;
using Ink.Parsed;
using System.Collections.Generic;

namespace RD.Dialog
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class SplashableText : MonoBehaviour
	{
		public class CharacterPosition
		{
			public Vector2 _position;
			public string _name;
			public CharacterPosition(Vector2 pos, string name)
			{
				_position = pos;
				_name = name;
			}
			public static float _distanceBetweenPoints = 600;
			public static Vector2 GetIndexPosition(int index)
			{
				switch (index)
				{
					default:
					case 0:
						return Vector2.zero;
					case int n when (n < 9 && n >= 1):
						{
							float dist = _distanceBetweenPoints;
							float mod = index % 2 > 0 ? -1 : 1;
							float angle = (int)((index - 1) / 2) * 45;
							Vector2 point = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle) * dist * mod, Mathf.Sin(Mathf.Deg2Rad * angle) * dist * mod);
							return point;
						}
					case int n when (n < 26 && n >= 9):
						{
							float dist = _distanceBetweenPoints * 2;
							float mod = index % 2 > 0 ? -1 : 1;
							float angle = (int)((index - 1) / 2) * 22.5f * mod;
							Vector2 point = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle) * dist * mod, Mathf.Sin(Mathf.Deg2Rad * angle) * dist * mod);
							return point;
						}
				}
			}
		}

		public static List<CharacterPosition> positions = new List<CharacterPosition>();
		SplashText _st;
		TextMeshProUGUI _tm;
		public Task _writingTask;
		public AudioClip _audioClip;
		Reply _reply;
		public void Init(Reply r)
		{
			_tm = GetComponent<TextMeshProUGUI>();
			if (r.FullLine() == " " || r.FullLine() == "")
			{
				Destroy(gameObject);
				return;
			}
			r.TrimQuotes();
			_reply = r;
			SplashText st = new SplashText();
			st.text = r._text;
			if (r.IsGesture())
				_audioClip = null;
			_tm.color = _dialogueManager.IsBackgroundDark() ? r.dialogCharacter._darknessColor : r.dialogCharacter._whitenessColor;
			_tm.text = st.text;
			_tm.ForceMeshUpdate();
			CharacterPosition pos = null;
			foreach(CharacterPosition p in positions)
			{
				if(p._name == r.dialogCharacter._name)
				{
					pos = p;
					break;
				}
			}
			if (pos != null)
			{
				st.randomLocation = false;
				st.position = pos._position;
				GetComponent<RectTransform>().anchoredPosition = st.position;
			}
			else
			{
				GetComponent<RectTransform>().anchoredPosition = CharacterPosition.GetIndexPosition(positions.Count);
				positions.Add(new CharacterPosition(GetComponent<RectTransform>().anchoredPosition, r.dialogCharacter._name));
			}
			st.SetDuration(_dialogueManager.GetTextDuration(_tm.GetParsedText())*4);
			Init(st);

			void Init(SplashText st)
			{

				_st = st;
				_tm = GetComponent<TextMeshProUGUI>();
				_tm.text = st.text;
				if(r.dialogCharacter._splashFontAsset !=null && r.dialogCharacter._splashFontMaterial != null)
				{
					_tm.font = r.dialogCharacter._splashFontAsset;
					_tm.fontSharedMaterial = r.dialogCharacter._splashFontMaterial;
				}
				if (_st.fadeInTime > 0)
				{
					Color c = _tm.color;
					c.a = 0;
					_tm.color = c;
					transform.localScale = Vector3.one * _st.specs.startScale;
					GetComponent<LerpColor>().StartLerpAlpha(1, 0, 1 / _st.fadeInTime);
					GetComponent<LerpTransform>().StartLerpScale(Vector3.one, 1 / _st.fadeInTime);
				}
				else
				{
					transform.localScale = Vector3.one;
					Color c = _tm.color;
					c.a = 1;
					_tm.color = c;
				}
				if (_st.specs.writingAnimation)
				{
					_writingTask = new Task(DialogTools.WriteText(_st.text, _tm, 1, _audioClip));
				}
				else
				{
					_tm.maxVisibleCharacters = 99999;
					if (r.dialogCharacter._splashAudio != null)
					{
						SoundEffectPlayer._current.Play(r.dialogCharacter._splashAudio);
					}
				}
				if (_st.specs.jitter)
				{
					VertexJitter vj = gameObject.AddComponent<VertexJitter>();
					vj.CurveScale = _tm.fontSize;
					vj.RemoveAfterTime();
					//vj.AutoJitter(1);
				}
				if (_st.specs.random)
				{
					string str = r.GetUntaggedText();
					Task t = new Task(DialogTools.ShuffleStringForTime(0.1f + (0.015f*(str.Length)), 0.1f, _tm, str));
				}
				StartCoroutine(RemoveAfterDuration());
			}
		}
		public void RandomizeLocation(RectTransform bounds)
		{
			_tm.ForceMeshUpdate();
			Debug.Log("Preferred size for '" + _tm.text + "' \n" +
				"is (" + _tm.preferredWidth + ", " + _tm.preferredHeight + ")");
			Vector2 position;
			int r = 20;
			do
			{
				r--;
				position = GetRandomLocation();
			} while (!(/*CompareY(30) ||*/ ComparePosition(position)) && r>0);
			GetComponent<RectTransform>().anchoredPosition = position;

			Vector2 GetRandomLocation()
			{

				float width = 450;//_tm.preferredWidth + 250;
				float height = 450;
				Bfloat posX = new Bfloat();
				posX.max = bounds.anchoredPosition.x + ((bounds.rect.width - width) / 2);
				posX.min = posX.max * -1;
				posX.current = Random.Range(posX.min, posX.max);
				Bfloat posY = new Bfloat();
				posY.max = bounds.anchoredPosition.y + ((bounds.rect.height - height) / 2);
				posY.min = posY.max * -1;
				posY.current = Random.Range(posY.min, posY.max);
				return new Vector2(posX.current, posY.current);
			}
			bool CompareY(float distance)
			{
				foreach(Transform t in transform.parent)
				{
					if (t != transform)
					{
						float y = t.GetComponent<RectTransform>().anchoredPosition.y;
						if (Mathf.Abs(y - position.y) < distance)
						{
							Debug.Log(t.gameObject.name +t.gameObject.GetInstanceID() +" has y of " + y + " AND\n"+gameObject.name+gameObject.GetInstanceID()+" has y of "+ position.y);
							return false;
						}
					}
				}
				return true;
			}
			bool ComparePosition(Vector2 position)
			{
				
				Debug.Log("asd!");
				float minimumDistance = 400;
				foreach (CharacterPosition cp in positions)
				{
					Debug.Log("Distance to "+cp._name+" is " + Vector2.Distance(position, cp._position));
				}
				return true;

				/*foreach(CharacterPosition cp in positions)
				{
					if (Vector2.Distance(position, cp._position) > minimumDistance)
					{
						return true;
					}
				}
				return false;*/
			}
		}
		IEnumerator RemoveAfterDuration()
		{
			float t = _st.duration;
			while (t > 0)
			{
				t -= CodeTools.Tm.GetUIDelta();
				yield return null;
			}
			if (_dialogueManager!=null && _reply!=null)
			{
				_dialogueManager.WriteReady(_st.GetDelayBeforeContinue());
			}
			GetComponent<LerpColor>().StartLerpAlpha(0, 1, 1 / _st.fadeOutTime);
			GetComponent<LerpTransform>().StartLerpScale(Vector3.one*_st.specs.startScale, 1 / _st.fadeOutTime);
			t = _st.fadeOutTime;
			while (t > 0)
			{
				t -= CodeTools.Tm.GetUIDelta();
				yield return null;
			}
			Destroy(gameObject);

		}

		[System.Serializable]
		public class SplashText
		{
			public string text = "";
			public float fadeInTime = 0.25f;
			public float fadeOutTime = 0.5f;
			public float duration { get; private set; } = 1.5f;
			public float waitAfter = 0.5f;
			public bool randomLocation = false;
			public Vector2 position;
			public Specifics specs;
			[System.Serializable]
			public class Specifics
			{
				public float startScale = 1;
				public bool jitter = false;
				public bool writingAnimation = false;
				public bool random = false;
				public float writingSpeed = 1f;
			}
			public void SetDuration(float basetime)
			{
				switch (DialogueManager.Settings.SplashText._writeMode)
				{
					default:
						duration = basetime;
						break;
					case DialogueManager.Settings.SplashText.WriteMode.Write:
						duration = 0.75f * basetime;
						break;
				}
			}
			public SplashText()
			{
				specs = new Specifics();
				switch (DialogueManager.Settings.SplashText._writeMode)
				{
					case DialogueManager.Settings.SplashText.WriteMode.Fade:
						fadeInTime = 0.5f;
						specs.startScale = 1.25f;
						break;
					case DialogueManager.Settings.SplashText.WriteMode.Write:
						specs.writingAnimation = true;
						break;
					case DialogueManager.Settings.SplashText.WriteMode.Instant:
						specs.jitter = true;
						break;
					case DialogueManager.Settings.SplashText.WriteMode.Random:
						fadeInTime = 0.25f;
						fadeOutTime = 0.5f;
						specs.random = true;
						break;
				}
			}
			void SetTime()
			{
				if (DialogueManager.Settings.SplashText._writeMode == DialogueManager.Settings.SplashText.WriteMode.Fade)
					waitAfter *= 2;
			}
			public float GetDelayBeforeContinue()
			{
				if (DialogueManager.Settings.SplashText._enableMultiText) {

					if (DialogueManager.Settings.SplashText._writeMode == DialogueManager.Settings.SplashText.WriteMode.Fade)
						return waitAfter / 2;
					else
						return 0;
				}
				else
					return waitAfter;
			}
		}
	}
}
