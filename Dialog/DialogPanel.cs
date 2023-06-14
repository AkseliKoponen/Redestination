using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using RD.DB;
using RD;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RD.Dialog.Inkerface;
using static RD.CodeTools;

namespace RD.Dialog
{
	public class DialogPanel : MonoBehaviour
	{
		[SerializeField] Image _portrait;
		[SerializeField] TextMeshProUGUI _text;
		RectTransform _textRT;
		RectTransform _rt;
		Bfloat _heightLimit;
		public DialogCharacter _dialogCharacter;
		List<Choice> _choices;
		//bool _dialogHistoryEnabled;
		[SerializeField] Slider _slider;
		[SerializeField] RectTransform _dragBar;
		[SerializeField] TextMeshProUGUI _argumentHealth;
		ResizePanel _resizePanel;
		Vector2 _originalPosition;
		public WritingType _writingType = WritingType.Instant;
		[Range(20, 500)]
		public int _writingSpeed = 50; //characters per second			//Slow = 20, Medium = 30, Quick = 50
		public float _writingSpacePause = 0.03f;
		public bool _writingShuffle = false;
		public Task _writingTask;
		public enum WritingType { Instant, Word, Letter}
		public DialogMode _dialogMode;
		Sprite _defaultSprite;

		string _lastText;
		private void Awake()
		{
			_rt = GetComponent<RectTransform>();
			_heightLimit = new Bfloat(165, 450);
			if(_portrait)_defaultSprite = _portrait.sprite;
			if (_slider && _dialogMode == DialogMode.Dialog)
			{
				_slider.onValueChanged.AddListener((float val) => UpdateScroll(val));
				/*_slider.gameObject.SetActive(false);
			
			_resizePanel = GetComponent<ResizePanel>();
			if (_resizePanel)
				_resizePanel.enabled = false;
			if (_dragBar)
				_dragBar.gameObject.SetActive(false);
			*/
			}
			_textRT = _text.GetComponent<RectTransform>();
			Clear();
			_originalPosition = _textRT.anchoredPosition;
			if(_dialogMode== DialogMode.Dialog)
			ChangeDialogCharacter(_dialogCharacter);
		}


		public void ChangeDialogCharacter(DialogCharacter dialogcharacter)
		{
			_dialogCharacter = dialogcharacter;
			if (_dialogCharacter._portrait) _portrait.sprite = _dialogCharacter._portrait;
			else _portrait.sprite = _defaultSprite;
			if (_dialogCharacter._color!=null)
			{
				Color c = _dialogCharacter._color;
				c = Color.HSVToRGB(GetHSV(c).x, GetHSV(c).y, 1);
				_portrait.transform.parent.GetComponent<Image>().color = c;
			}
			else
			{
				_portrait.transform.parent.GetComponent<Image>().color = Color.white;
			}

		}
		public void NewLine(Reply reply, bool forceInstant = false,string playerReply = default)
		{
			//Debug.Log("New Line: " + reply.FullLine()+"\n\nPlayerReply = "+playerReply);
			if(reply.dialogCharacter == null) {

			}
			else if (_dialogMode == DialogMode.Dialog && (_dialogCharacter == null || reply.dialogCharacter._id != _dialogCharacter._id))
				ChangeDialogCharacter(reply.dialogCharacter);
			if (_dialogMode == DialogMode.Dialog)
			{

				SetText(playerReply+reply.FullLine());
			}
			else if (_dialogMode == DialogMode.Storybook)
			{
				SetText("\n" + playerReply + reply.FullLine());
			}
			_lastText = reply.FullLine();
			if (!forceInstant && _writingType != WritingType.Instant)
			{
				if (_writingTask != null)
				{
					_writingTask.Stop();
				}
			
				_writingTask = new Task(WriteText(reply));
			}
			else
			{
				WriteReady();
			}
		}
		void SetText(string text, bool forceUpdate = true, bool overrideMode = false)
		{
            if (pageTask != null)
            {
				//Debug.Log("Adding")
				_textBuffer += text;
				return;
			}
			if(!overrideMode)
				_text.text = _dialogMode==DialogMode.Dialog?text:_text.text+text;
			else
			{
				_text.text = text;
			}
			_text.ForceMeshUpdate();
			if (_dialogMode == DialogMode.Storybook && _text.verticalAlignment != VerticalAlignmentOptions.Bottom)
			{
				if (_text.isTextOverflowing)
					_text.verticalAlignment = VerticalAlignmentOptions.Bottom;
				//Debug.Log((_text.isTextOverflowing ? "OVERFLOWING\n" : "NOT OVERFLOWING\n") + _text.text);
			}
			if (forceUpdate)StartCoroutine(UpdateText());

		}
		IEnumerator UpdateText()
		{
			int temp = 0;
			while (temp<2)
			{
				temp++;
				yield return null;
			}
			_text.ForceMeshUpdate(true, true);
		}
		IEnumerator WriteText(Reply reply)
		{
			string orig = reply.GetRawText();
			float wordDelta = 1f;
			float currentTime = 0;
			string prefraw = reply.GetPrefixRaw();
			string parsedText = _text.GetParsedText();
			int length;
			bool additive = _dialogMode == DialogMode.Storybook;
			if (additive)
			{
				length = prefraw == null ? 0 : prefraw.Length;
				length += _text.GetParsedText().Length-1;
				//parsedText = _text.GetParsedText() + prefraw;
				string tempst = _text.text==""?reply.FullLine():_text.text + "\n" + reply.FullLine();
				SetText(tempst,true,true);
			}
			else
			{
				SetText(reply.FullLine());
				length = prefraw == null ? 0 : prefraw.Length;
				//parsedText = prefraw==null?"":prefraw;
			}
			_text.maxVisibleCharacters = length;
			bool textUpdated = false;
			char lastLetter = length >= 1 ? parsedText.ToCharArray()[length-1] : 'a';
			while (_text.maxVisibleCharacters<_text.GetParsedText().Length)
			{
				if(textUpdated)

					currentTime += CodeTools.Tm.GetUIDelta() * _writingSpeed;
				while (currentTime > wordDelta && length<parsedText.Length)
				{
					currentTime -=wordDelta;
					length++;
					if(textUpdated)lastLetter = length >= 1 ? parsedText.ToCharArray()[length - 1] : 'a';
				}
				if (_writingType == WritingType.Letter)
				{
					_text.maxVisibleCharacters = length;
					if (_writingSpacePause > 0 && lastLetter==' ' && textUpdated)
					{
						float currentPause = currentTime-CodeTools.Tm.GetUIDelta();
						while (currentPause < _writingSpacePause)
						{
							currentPause += CodeTools.Tm.GetUIDelta();
							yield return null;
						}
						length++;
						lastLetter = parsedText.ToCharArray()[length - 1];
					}
				}
				else
				{
					if (orig.Substring(0, length).Contains(" "))
					{
						string finaltext = reply.GetPrefix() + reply.TranslateSymbols(orig.Substring(0, orig.Substring(0, length).LastIndexOf(' ')));
						if (_writingShuffle) finaltext = ShuffleWord(finaltext);
						SetText(finaltext);
					}
					else
					{
						SetText(reply.GetPrefix());
					}
				}
				textUpdated = true;
				yield return null;
			}
			WriteReady();
		}
		public void WriteReady()
		{
			_dialogueManager.WriteReady();
			_text.maxVisibleCharacters = 99999;
		}
		float GetWriteTimeEstimates(Reply reply)
		{
			string orig = reply.GetRawText();
			string fullLine = reply.GetPrefixRaw() + orig;
			//Debug.Log("fullLine = " + fullLine);
			int spaceCount = 0;
			foreach (char c in fullLine.ToCharArray())
				if (c == ' ')spaceCount++;
			//Debug.Log("spaceCount = " + spaceCount);
			float wordDelta = 1 / _writingSpeed;
			float totalTime = (float)(orig.Length - spaceCount) * wordDelta + (spaceCount * _writingSpacePause);
			return totalTime;
		}
		string ShuffleWord(string line)
		{
			if (_writingShuffle)
			{
				//Get last word
				int lastspace = line.LastIndexOf(' ');
				if (lastspace <= 1)
					return line;
				string randomWord = line.Substring(lastspace);
				randomWord = randomWord.Substring(0, randomWord.IndexOf('<'));
				foreach (char c in randomWord)
				{
					string chars = "abcdefghijklmnopqrstuvwxyz";
					//string chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&";
					randomWord = randomWord.Replace(c, chars[Random.Range(0, chars.Length)]);
				}
				line = line.Substring(0, lastspace) + randomWord;
			}
			return line;

		}
		int preChoiceLength;
		public void NewChoices(List<Choice> choices,bool onlyContinue=false)
		{

			if (_dialogCharacter.name == "Atris")
				Clear();
			preChoiceLength = _text.text.Length;
			_choices = choices;
			List<string> lines = new List<string>();
			string wholeText = "";
			
			lines.Add("<style=\"Choice\">");

			for (int i = 0; i < _choices.Count; i++)
			{
				string line = (i + 1) + ". " + "<link=\""+choiceTag + (i) + ">"+ _choices[i].text + "</link>\n";
				lines.Add(line);
			}
			if (_dialogMode == DialogMode.Storybook)
				wholeText += "\n";
			foreach(string s in lines)
			{
				wholeText += s;
			}
			wholeText += "</style>";
			SetText(wholeText);
			_lastText = wholeText;
			_text.maxVisibleCharacters = _text.GetParsedText().Length;
		}
		public Reply SetChoice(int choiceID)
		{
			string linkTxt = Inkerface.story.currentChoices[choiceID].text;
			Reply r = new Reply(_dialogCharacter, linkTxt);
			SetText(_dialogMode==DialogMode.Dialog?r.GetHistoryFormat():_text.text.Substring(0,preChoiceLength)+"\n"+r.GetHistoryFormat()+"\n",true,true);
			return r;
		}
		public void Clear()
		{
			SetText("", true, true);
		}
		public void ClearLastText()
		{
			Debug.Log("LastText = " + _lastText);
			if (_lastText != null && _lastText.Length > 0)
			{
				_text.text = _text.text.Substring(0, _text.text.Length - _lastText.Length);
				_lastText = "";
			}
		}
		public void Refresh()
		{
			//Debug.Log("Refresh");
			ToggleScrolling();
		}
		#region DialogHistory
		void ToggleScrolling() {
			if (!_slider)
				return;
			_text.ForceMeshUpdate();
			bool active = _text.isTextOverflowing;
			if (active)
			{
				UpdateScroll(_slider.value);
				UpdateSlider();
			}
			else
			{
		
				_textRT.anchoredPosition = _originalPosition;
			}
		}
		/// <summary>
		/// Adjusts the relative position of the body of the text relative to the viewport.
		/// </summary>
		/// <param name="relativePosition"></param>
		void UpdateScroll(float relativePosition)
		{
			relativePosition = 1 - relativePosition;
			TMP_TextInfo textInfo = _text.textInfo;

			// Check to make sure we have valid data and lines to query.
			if (textInfo == null || textInfo.lineInfo == null || textInfo.lineCount == 0 || textInfo.lineCount > textInfo.lineInfo.Length) return;

			_textRT.anchoredPosition = new Vector2(_textRT.anchoredPosition.x, (_text.preferredHeight - _textRT.rect.height) * relativePosition);
	
		}
		void UpdateSlider()
		{
			// Update Scrollbar
			if (_slider)
			{
				_slider.value = 1-(_textRT.anchoredPosition.y / (_text.preferredHeight - _textRT.rect.height));
			}
		}
		bool _historyEnabled = false;
		public void ToggleHistory(bool enabled)
		{
			if (enabled == _historyEnabled)
				return;
			List<Reply> history = _dialogueManager._dialogHistory;
			if (!enabled && _historyEnabled && _rt.sizeDelta.y<=_heightLimit.min){
				Generics();
				//hide history
				Reply r = history[history.Count - 1];
				_textRT.anchoredPosition = new Vector2(_textRT.anchoredPosition.x, 0);
				NewLine(r,true);
				//Debug.Log(r.FullLine());
				_text.ForceMeshUpdate();
			}
			else if(enabled && !_historyEnabled)
			{
				//display history
				_slider.value = 0f;
				//Somehow keep the Text in place
				Clear();
				foreach (Reply r in history)
				{
					_text.text += r.GetHistoryFormat() + "\n";
				}
				_text.ForceMeshUpdate();
				if (_text.isTextOverflowing)
				{
					Generics();
					ToggleScrolling();
				}
				else
					_historyEnabled = true;
			}
			else
			{
				Debug.Log("enabled = " + enabled + "\n_historyEnabled = " + _historyEnabled);
			}
			void Generics()
			{

				_historyEnabled = enabled;
				Debug.Log("ToggleHistory(" + enabled + ")");
				_slider.StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(_slider.GetComponent<CanvasGroup>(), enabled, 5,true));
			}
		}
		public void DragHistoryButton(float deltaY)
		{

			Vector2 sd = _rt.sizeDelta;
			float newY = _heightLimit.Clamp(sd.y + deltaY);
			if (!(newY>_rt.sizeDelta.y && !_text.isTextOverflowing))
				_rt.sizeDelta = new Vector2(sd.x, newY);
			Refresh();
		}
		public void ScrollHistory(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (!_historyEnabled)
				return;
			float sensitivity = 0.2f / _text.preferredHeight;
			float scroll = cbt.ReadValue<Vector2>().y*sensitivity;
			if (_historyEnabled && scroll != 0)
			{
				Debug.Log("scrollin "+scroll*sensitivity);
				_slider.value = Mathf.Clamp(_slider.value + scroll, 0, 1);
				UpdateScroll(_slider.value);

			}
		}
		#endregion

		public void NextPage()
        {
			Debug.Log("Next Page!");
			StartCoroutine(FlipPage());
			_textBuffer = "";
        }
		Task pageTask;
		string _textBuffer;
		IEnumerator FlipPage()
        {
			Image img = _text.GetComponentInParent<Image>();
			float fillSpeed = _dialogueManager._storyBookAnimationTime;
			pageTask = new Task(UIAnimationTools.ImageTransitionFill(img,fillSpeed));
			while (pageTask.Running)
			{
				yield return null;
			}
			_text.text = _textBuffer;
			_text.verticalAlignment = VerticalAlignmentOptions.Top;
			FlipOrigin();
			float t = fillSpeed;
            while (t > 0)
            {
				t -= Tm.GetUIDelta();
				yield return null;
            }
			pageTask = new Task(UIAnimationTools.ImageTransitionFill(img, fillSpeed));
			//img.fillAmount = 1;
			while (pageTask.Running)
			{
				yield return null;
			}
			pageTask = null;
			FlipOrigin();

			void FlipOrigin()
			{
				if (img.fillOrigin == 1)
					img.fillOrigin = 0;
				else
					img.fillOrigin = 1;
			}
		}
		public bool ReadyToContinue()
		{
			if (pageTask != null)
				return false;
			return true;
		}
	}
}
