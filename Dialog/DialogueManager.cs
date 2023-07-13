using System;
using System.Collections.Generic;
using System.Reflection;
using Ink.Runtime;
using RD.DB;
using RD;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

namespace RD.Dialog
{
	public class DialogueManager : MonoBehaviour
	{
		//use TMPRO Text Selector when choosing reply
		public Story _story;
		public Inkerface.DialogMode _dialogMode;
		DialogPanel _dialogPanelCurrentNPC;
		DialogPanel _dialogPanelCurrentPlayer;
		public DialogPanel _dialogPanelNPC;
		public DialogPanel _dialogPanelPlayer;
		public CanvasGroup _storyBook;
		public Image _storyBookImage;
		public CanvasGroup _background;
		public GameObject _splashTextPrefab;
		public RectTransform _bottomBar;
		public List<Inkerface.Reply> _dialogHistory { get; private set; } = new List<Inkerface.Reply>();
		public float _autoContinueDelay = -1;
		int _replyID;
		bool _waitForChoice = false;
		DialogCharacter _protagonist;
		public void Awake()
		{
			Inkerface._dialogueManager = this;
			_protagonist = (DialogCharacter)CodeTools.db.Get <DialogCharacter>(1);
		}
		private void Start()
		{
		
			TextAsset inkFile = SceneHandler._sceneConstructor._inkFile;
			ToggleDialogPanel(true);        //TEMPORARY
			if (!inkFile)
			{
				Debug.LogError("_inkJsonFile not set");
				return;
			}
			ChangeStory(inkFile);
		}
		#region Input

		public void InputDialogHistory(UnityEngine.InputSystem.InputAction.CallbackContext cbt) {
			/*
		if (AcceptInput())
			if (CodeTools.IsClick(cbt))
				_dialogPanelCurrentNPC.ToggleDialogHistory(_dialogHistory);
				*/
		}
		public void InputSkip(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (AcceptInput())
				if (CodeTools.IsKeyClick(cbt))
				{
					Skip();
				}
		}
		#region NumberInput
		public void InputNumber1(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if(AcceptInput())
				if (CodeTools.IsKeyClick(cbt))
					HandleNumberKeyInputs(1);
		}
		public void InputNumber2(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (AcceptInput())
				if (CodeTools.IsKeyClick(cbt))
					HandleNumberKeyInputs(2);
		}
		public void InputNumber3(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (AcceptInput())
				if (CodeTools.IsKeyClick(cbt))
					HandleNumberKeyInputs(3);
		}
		public void InputNumber4(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (AcceptInput())
				if (CodeTools.IsKeyClick(cbt))
					HandleNumberKeyInputs(4);
		}
		public void InputNumber5(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (AcceptInput())
				if (CodeTools.IsKeyClick(cbt))
					HandleNumberKeyInputs(5);
		}
		public void InputNumber6(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (AcceptInput())
				if (CodeTools.IsKeyClick(cbt))
					HandleNumberKeyInputs(6);
		}
		public void InputNumber7(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (AcceptInput())
				if (CodeTools.IsKeyClick(cbt))
					HandleNumberKeyInputs(7);
		}
		public void InputNumber8(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (AcceptInput())
				if (CodeTools.IsKeyClick(cbt))
					HandleNumberKeyInputs(8);
		}
		public void InputNumber9(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (AcceptInput())
				if (CodeTools.IsKeyClick(cbt))
					HandleNumberKeyInputs(9);
		}
		#endregion
		void HandleNumberKeyInputs(int number) {
			//Debug.Log(number.ToString());
			if (_story.currentChoices.Count >= number || (_waitForContinue && number == 1))
			{

				ChooseOption(Inkerface.choiceTag + (number-1));
			}
		}
		public bool AcceptInput()
		{
			if (!_inputEnabled)
				return false;
			if (CodeTools._combatManager.CombatActive())
				return false;
			else
				return true;
		}
		#endregion
		#region Ink Logic
		public void ChangeStory(TextAsset inkFile, string knot = default)
		{
			if (_story != null)
			{
				Inkerface.UpdateStoryVariables(false);
			}
			_story = Inkerface.StartNewStory(inkFile);
			if (knot != default)
				Inkerface.GoToKnot(knot);
			ReadLine();
		}
		void NextLine()
		{
			if(_dialogPanelCurrentPlayer)
				_dialogPanelCurrentPlayer.Clear();
			ReadLine();
		}
		void ReadLine(string playerReply = default)
		{
			if (Inkerface.GetNextLine())
			{
				if (Inkerface.currentLine._isCommand)
					return;
				Inkerface.currentLine.Dbug();
				AddHistory(Inkerface.currentLine);
				switch (_dialogMode)
				{
					case Inkerface.DialogMode.Darkness:
						Instantiate(_splashTextPrefab, _background.transform).GetComponent<SplashableText>().Init(Inkerface.currentLine);
						break;
					case Inkerface.DialogMode.Dialog:
					case Inkerface.DialogMode.Storybook:
						_dialogPanelCurrentNPC.NewLine(Inkerface.currentLine,false, playerReply);
						break;
					default:
						Debug.Log("Unimplemented DialogMode " + _dialogMode.ToString());
						break;

				}
			}
			else
			{

				NewChoices();
			}
		}
		bool _waitForContinue = false;
		void NewChoices(bool onlyContinue = false)
		{
			//Debug.Log("New Choices!\nAdditionalLine = " + additionalLine);
			_waitForChoice = true;
			_waitForContinue = onlyContinue;
			//Debug.Log("NewChoices");
			DialogPanel dp = _dialogMode == Inkerface.DialogMode.Dialog ? _dialogPanelCurrentPlayer : _dialogPanelCurrentNPC;
			dp.NewChoices(!onlyContinue?Inkerface.GetChoices():Inkerface.GetOnlyContinueChoice(),onlyContinue);
		}

		public void WriteReady(float autoContinueDelayOverride = -1)
		{
			if (_story.canContinue)
			{
				if (autoContinueDelayOverride >= 0)
				{
					if (autoContinueDelayOverride > 0)
						Invoke("NextLine", autoContinueDelayOverride);
					else NextLine();
				}
				else if (_autoContinueDelay >= 0)
				{
					if (_autoContinueDelay > 0)
						Invoke("NextLine", _autoContinueDelay);
					else NextLine();
				}
				else
				{
					NewChoices(true);
				}
			}
			else
			{
				NextLine();
			}
		}
		public string ChooseOption(string option)
		{
			if (_dialogPanelCurrentNPC!= null && !_dialogPanelCurrentNPC.ReadyToContinue())
				return "";
			//Debug.Log("Story can" + (!story.canContinue ? " not" : "") + " continue.");
			if (!Inkerface.story.canContinue)//if story can continue (only one option "1. Continue") then continue without adding the text to history
			{
				int uchoicestringlength = 7;
				int choice = int.Parse(option.Substring(uchoicestringlength, option.Length - uchoicestringlength));
				Inkerface.story.ChooseChoiceIndex(choice);
				string str = Inkerface.story.Continue();
				Inkerface.Reply r = new Inkerface.Reply(_protagonist, str);
			
				str = r.GetHistoryFormat();
				_waitForChoice = false;
				//NewChoices(true);
				if (_dialogMode == Inkerface.DialogMode.Storybook)
					_dialogPanelCurrentNPC.ClearLastText();
				AddHistory(r);
				ReadLine(str);
				return str;
			}
			else //if only continuing, do not add history
			{
				//Debug.Log("Continuing");
				if (_dialogPanelCurrentPlayer)
				{
					_dialogPanelCurrentPlayer.Clear();
				}
				else
				{
					_dialogPanelCurrentNPC.ClearLastText();
				}
				NextLine();
				return "";
			}
		}
		void Skip()
		{
			Debug.Log("Skip");
		
			if (_dialogPanelCurrentNPC._writingTask!=null &&_dialogPanelCurrentNPC._writingTask.Running)
			{
				_dialogPanelCurrentNPC._writingTask.Stop();
				_dialogPanelCurrentNPC.WriteReady();
			}
			else if (!_waitForChoice)
			{
				NextLine();
			}
		}
		public void CombatVictory()
		{
			List<string> parameters = CodeTools.SliceString(Inkerface._afterCombat, ' ');
			string scene = parameters[0];
			string knot = parameters.Count > 1 ? parameters[1] : default;
			if (parameters.Count < 3)
			{
				ToggleDialogPanel(true);
			}
			else
				Inkerface.InkFunction.Invoke(parameters[2]);
			if (scene == "this")
			{
				if (knot != default)
				{
					Inkerface.GoToKnot(knot);
				}
				NextLine();
			}
			else
			{
				SceneConstructor sceneConstructor;
				int id;
				if(int.TryParse(scene,out id)) sceneConstructor = (SceneConstructor)CodeTools.db.Get<SceneConstructor>(id);
				else sceneConstructor = (SceneConstructor)CodeTools.db.Get<SceneConstructor>(scene);
				if (sceneConstructor == null)
				{
					Debug.LogError("Invalid SceneConstructor!\n" + scene);
					return;
				}
				TextAsset file = sceneConstructor._inkFile;
				ChangeStory(file, knot);
				SceneHandler.EnterBaseScene(sceneConstructor);
			}

			Invoke("EnableInput", 1);
		
		}
		bool _inputEnabled = true;
		public void EnableInput()
		{
			_inputEnabled = true;
		}
		public void DisableInput()
		{
			_inputEnabled = false;
		}
		public void AddHistory(Inkerface.Reply reply)
		{
			//Debug.Log("adding to history " + reply.GetHistoryFormat());
			_dialogHistory.Add(reply);
		}
		#endregion
		#region Commands
		float _fadeSpeedCanvasGroup = 5;
		public void ToggleStorybook(bool active)
		{
			ToggleCanvasGroup(_storyBook,active);
			if (active)
			{
				ToggleDialogPanel(false);
				_dialogPanelCurrentNPC = _storyBook.GetComponentInChildren<DialogPanel>();
				_dialogPanelCurrentPlayer = null;
				_background.blocksRaycasts = true;
				_background.alpha = 1;
				SetDialogMode(Inkerface.DialogMode.Storybook);
			}
			else
			{
			}
		}
		public void ToggleDialogPanel(bool active)
		{
			CanvasGroup cg = _dialogPanelNPC.transform.parent.GetComponent<CanvasGroup>();
			ToggleCanvasGroup(cg, active);
			if (active)
			{
				_dialogPanelCurrentNPC = _dialogPanelNPC;
				_dialogPanelCurrentPlayer = _dialogPanelPlayer;
				ToggleStorybook(false);
				ToggleDarkness(false);
				SetDialogMode(Inkerface.DialogMode.Dialog);
			}
			else
			{
			}
		}
		
		public void ToggleDarkness(bool active)
		{
			ToggleCanvasGroup(_background, active);
			if (active)
			{
				SplashableText.positions = new List<SplashableText.CharacterPosition>();
				_background.GetComponent<Image>().color = Color.black;
				ToggleStorybook(false);
				ToggleDialogPanel(false);
				_dialogPanelCurrentNPC = null;
				_dialogPanelCurrentPlayer = null;
				SetDialogMode(Inkerface.DialogMode.Darkness);
			}
			else
			{
			}
			//Disable dialog windows
			//Enable writing on screen
		}
		public void ToggleCanvasGroup(CanvasGroup cg, bool active)
		{
			cg.interactable = active;
			cg.blocksRaycasts = active;
			if (active)
				cg.alpha = 1;
			else
				cg.alpha = 0;
		}
		void SetDialogMode(Inkerface.DialogMode dm)
		{
			_dialogMode = dm;
			if (_dialogPanelCurrentNPC)
			{
				_dialogPanelCurrentNPC.Clear();
				_dialogPanelCurrentNPC._dialogMode = dm;
			}
			if (_dialogPanelCurrentPlayer)
			{
				_dialogPanelCurrentPlayer.Clear();
				_dialogPanelCurrentPlayer._dialogMode = dm;
			}
		}
		public void SetStorybookImage(Sprite sprite)
        {
			float t = _storyBookAnimationTime * 1.25f;
			if (_dialogMode == Inkerface.DialogMode.Storybook)
				StartCoroutine(UIAnimationTools.ImageTransitionAlpha(_storyBookImage, t, sprite, _storyBookAnimationTime));
			else
				_storyBookImage.sprite = sprite;
		}
		#endregion
		#region Tools
		public float GetTextDuration(string parsedText)
		{
			return DialogTools.GetWriteTimeEstimate(parsedText);
		}
		public float _storyBookAnimationTime = 0.15f;
		public bool IsBackgroundDark()
		{
			Color c = _background.GetComponent<Image>().color;
			//Debug.Log("grayscale = "+c.grayscale);
			return c.grayscale < 0.5f;
		}
		public int AssignReplyID()
		{
			_replyID++;
			return _replyID-1;
		}
		#endregion
		public static class Settings
		{
			public static class SplashText
			{
				public enum WriteMode {Write, Fade, Instant, Random}
				public static WriteMode _writeMode = WriteMode.Write;
				public static bool _enableMultiText = false;
				public static bool _sound = true;
				public static void Reset()
				{
					_writeMode = WriteMode.Write;
					 _enableMultiText = false;
				}
			}
			public static void ResetSettings()
			{
				SplashText.Reset();
			}

		}
	}

	public static class Inkerface
	{
		#region Inkerface Base
		public static bool _recordMode = true;
        public static DialogueManager _dialogueManager;
		public static Story story;
		public static Reply currentLine;
		public static VariablesState _variablesState;
		public static Symbols _symbols;
		public struct Symbols
		{
			public string dialogCharacter { get; private set; }
			public string dialogCharacterLink { get; private set; }
			//public string gesture { get; private set; }
			public string glue { get; private set; }
			public string tagCommand { get; private set; }
			public List <string> quotationMarks { get; private set; }
			public Symbols(bool b)
			{
				dialogCharacter = "§";
				dialogCharacterLink = "|";
				glue = "¤";
				tagCommand = "^";
				quotationMarks = new List<string> { "\"", "“", "”", "”" };
			}
		}
		public enum DialogMode { None, Dialog, Storybook, Darkness }
		public static string choiceTag { get; private set; } = "~choice";
		public static float textSpeed; //lower = faster, 0 = the whole text appears instantly.
		public static Story StartNewStory(TextAsset inkJsonFile)
		{
			DialogueManager.Settings.ResetSettings();
			_symbols = new Symbols(true);
			story = new Story(inkJsonFile.text);
			_variablesState = story.variablesState;
			currentLine = new Reply(story.currentText);
			UpdateStoryVariables(true);
		
			return story;
		}
		public static void UpdateStoryVariables(bool assignFromGameToStory = false)
		{
			bool dbug = false;
			StoryVariables sv = GameManager._current._storyVariables;
			foreach (FieldInfo field in sv.GetType().GetFields())
			{

				if (field.Name == "hideFlags" || field.IsPrivate) continue;
				if (_variablesState.GlobalVariableExistsWithName(field.Name))
				{
					if (assignFromGameToStory)
					{
						if (dbug) Debug.Log("<color=teal>Assigning " + field.Name + " as " + field.GetValue(sv).ToString() + "</color>");
						_variablesState[field.Name] = field.GetValue(sv);
					}
					else
					{
						if (dbug) Debug.Log("<color=teal>Assigning " + field.Name + " as " + _variablesState[field.Name].ToString()+"</color>");
						field.SetValue(sv, _variablesState[field.Name]);
					}
				}
				else
				{
					//Debug.Log("StoryVariable not implemented in Story (" + field.Name + ")");
				}
			}
			_nextScene = _variablesState["_nextScene"].ToString();
			_afterCombat = _variablesState["_afterCombat"].ToString();
		}
		public static void GoToKnot(string knot)
		{
			//Return true if knot found
			story.ChoosePathString(knot);

		}
		public static string _nextScene; //Read after reaching END (InkFileName knot dialogMode) use "this" as inkFileName to stay in same story
		public static string _afterCombat; //Read after winning combat (InkFileName knot dialogMode) use "this" as inkFileName to stay in same story
		public static bool GetNextLine()
		{
			if (!story.canContinue)
				return false;
			string txt = "";
			do
			{
				txt += story.Continue();
			} while (txt.EndsWith("¤"));
			//Debug.Log(txt);
			currentLine = new Reply(txt);
			//currentLine = new Reply(story.Continue());
		
			return true;
		}
		public static List<Choice> GetOnlyContinueChoice()
		{
			List<Choice> choices = new List<Choice>();
			choices.Add(new Choice());
			choices[0].text = "Continue.";
			return choices;
		}
		public static List<Choice> GetChoices()
		{
			List<Choice> finishedChoices = new List<Choice>();
			foreach(Choice c in story.currentChoices)
			{
				//c.text = FormatChoice(c.text);
				finishedChoices.Add(c);
			}
			//Debug.Log(finishedChoices.Count);
			return finishedChoices;
		}
		public static void RecordLine(string s)
		{
			if(_dialogueManager._dialogMode == DialogMode.Darkness && (s.StartsWith("§") && s.Contains(":")))
			{
				//if Darkness: remove the speaker name as it is not visible for the player and shouldn't be recorded for history
				s = s.Substring(s.IndexOf(":")+1);
			}
			if (s.Length > 0)
			{
				s = s.ToLower();
				RemoveFormatting();
				
				List<char> chars = new List<char>();
				chars.AddRange(s.ToCharArray());
				s = "";
				List<string> words = new List<string>();
				string word = "";
				foreach(char c in chars)
				{
					if (Translator.consonants.Contains(c) || Translator.vowels.Contains(c))
						word = word + c;
					else if(c == ' ')
						AddWord();
				}
				AddWord();

				string path = Application.dataPath + "/Resources/Ink/Dictionary.txt";
				if (File.Exists(path))
				{
					s = File.ReadAllText(path) + " " + s;
				}
				else
					s = "";
				using (StreamWriter sw = File.CreateText(path))
				{
					sw.Write(s);
					foreach (string w in words)
					{
						if (!s.Contains(w))
						{
							Debug.Log("Wrote " + w + " at " + path);
							sw.Write(w+" ");
						}
					}
					//sw.Close();
				}
				void AddWord()
				{
					if (word.Length>1 && !words.Contains(word) && !word.Contains("link"))
						words.Add(word);
					word = "";
				}
				void RemoveFormatting()
				{
					while (s.Contains("<") && s.Contains(">"))
					{
						int begin = s.IndexOf("<");
						if (begin >= 0)
						{
							int end = s.Substring(begin).IndexOf(">") + begin;
							if (end >= 0)
							{
								s = s.Substring(0, begin) + (end < s.Length ? s.Substring(end + 1) : "");
							}
						}
					}
				}
			}
		}
        #endregion
        public class Reply
		{
			public DialogCharacter dialogCharacter { get; private set; }
			public bool _isCommand = false;
			public string _text;
			string prefix;
			string prefixRaw;
			string rawText;
			string originalText;
			string unTaggedText;
			public int _replyID = -1;
			public void SetDialogCharacter(DialogCharacter dc)
			{
				dialogCharacter = dc;
				Construct(originalText, false);
			}
			public Reply(string line)
			{
				if (_dialogueManager) _replyID = _dialogueManager.AssignReplyID();
				Construct(line);
			}
			void Construct(string line, bool findDialogCharacter = true)
			{
				if (line == "" || line == " ")
					return;
				line = CodeTools.RemoveLineEndings(line);
				originalText = line;
				ReadTags();
				if (!_isCommand)
				{
					rawText = line;
					line = AssignDialogCharacter();
					line = TranslateSymbols(line);
					rawText = CodeTools.RemoveChars(rawText, '@');
					_text = line;
				}
				string AssignDialogCharacter()
				{
					string actorName;
					string endStr = " ";
					List<string> separators = new List<string> { " ", ",", ":", "." };
					if (line.Contains(_symbols.dialogCharacter))
					{
						int dcharIndex = line.IndexOf(_symbols.dialogCharacter);
						string cutline = line.Substring(dcharIndex);
						endStr = EndCharacter(cutline, separators);
						int nameEndIndex = cutline.IndexOf(endStr) - 1;
						actorName = line.Substring(dcharIndex + 1, nameEndIndex);
						if (findDialogCharacter) {
							dialogCharacter = (DialogCharacter)CodeTools.db.Get < DialogCharacter>(actorName);
							if (dialogCharacter == null)
							{
								int skipIndex = cutline.IndexOf(" ") + 1;
								cutline = cutline.Substring(skipIndex);
								endStr = EndCharacter(cutline, separators);
								nameEndIndex = cutline.IndexOf(endStr) - 1 + skipIndex;
								actorName = line.Substring(dcharIndex + 1, nameEndIndex);
								dialogCharacter = (DialogCharacter)CodeTools.db.Get < DialogCharacter>(actorName);
								if (dialogCharacter == null) {
									actorName = "Missing";
									dialogCharacter = (DialogCharacter)CodeTools.db.Get < DialogCharacter>(actorName);
								}
							}
							line = CutName(line, _symbols.dialogCharacter, nameEndIndex);
							if (dcharIndex == 0)
							{

								prefix = dcharIndex==0?GetDialogCharacterString():"";
								prefixRaw = prefix;// dcharIndex == 0 ? dialogCharacter.GetCharacterRaw("", endStr + (endStr == ":" ? " " : "")) : "";
							}
							else
							{
								line = CutName(line, _symbols.dialogCharacter, nameEndIndex);
								line = line.Insert(dcharIndex, GetDialogCharacterString());
							}
						}
						rawText = rawText.Substring(0, dcharIndex) + actorName + rawText.Substring(dcharIndex + nameEndIndex + 1);
					}
					else
					{
						actorName = "Narrator";
						if (findDialogCharacter) dialogCharacter = (DialogCharacter)CodeTools.db.Get<DialogCharacter>(actorName);
					}
					unTaggedText = CodeTools.RemoveChars(line, '"');
					return line;

					string GetDialogCharacterString()
					{
						if (endStr == ":")
						{

							return dialogCharacter.GetCharacter("", endStr +" ", actorName,true);
						}
						else
							return dialogCharacter.GetCharacter("","",actorName,true) + endStr;
					}
					string CutName(string text, string symbol, int length)
					{
						string s = text;
						int start = s.IndexOf(symbol);
						//Debug.Log(start);
						if (start < 0)
							return s;
						string startPiece = s.Substring(0, start);
						string endPiece = s.Substring(start + length + 2);
						s = startPiece + endPiece;
						//Debug.Log("Returning \n" + s);
						return s;
					}
				}
				string EndCharacter(string ln, List<string> cutters)
				{
					int index=9999;
					string c = " ";
					foreach(string cut in cutters)
					{
						if (ln.Contains(cut))
						{
							if (ln.IndexOf(cut) < index)
							{
								c = cut;
								index = ln.IndexOf(cut);
							}
						}
					}
					//Debug.Log("EndCharacter = [" + c+"]");
					return c;
				}
				void ReadTags()
				{
					//Debug.Log("ReadTags()");
					string startSymbol = _symbols.tagCommand;
					string temp = line;
					
					while (temp.Contains(startSymbol) && temp.Substring(temp.IndexOf(startSymbol) + 1).Contains(startSymbol))
					{
						//Debug.Log("Made it here!");
						int stIndex = temp.IndexOf(startSymbol) + 1;
						int endIndex = stIndex + temp.Substring(stIndex).IndexOf(startSymbol);
						int length = endIndex - stIndex;
						string tag = endIndex < temp.Length ? temp.Substring(stIndex, length) : temp.Substring(stIndex);
						if (tag.StartsWith("£"))
						{
							_isCommand = true;
							InkFunction.Invoke(tag);
						}
						else
						{

							
							//add the whole word to the link
							string temptag = tag;
							if (temptag.StartsWith(_symbols.dialogCharacter)) {
								temptag = temptag.Substring(1);
								Debug.Log("temptag = '" + temptag+"'");
								DialogCharacter dc = (DialogCharacter)CodeTools.db.Get<DialogCharacter>(temptag);
								if (dc != null)
								{
									tag = "<link="+_symbols.dialogCharacterLink+dc._id+">"+dc.GetColor(_dialogueManager._dialogMode)+ temptag + "</color></link>";
									Debug.Log("tag = " + tag);
								}
								length = temptag.Length + 2;
							}
							else
							{
								if (temp.Substring(endIndex).Contains(" "))
								{
									//Debug.Log("contains space");
									int lastLetterIndex = temp.Substring(endIndex).IndexOf(" ") + endIndex;
									length = lastLetterIndex - endIndex - 1;
									if (length > 0)
									{
										//Debug.Log("Added "+length + " characters to tag to complete the word.");
										temptag = tag + temp.Substring(endIndex + 1, length);
									}
								}
								tag = "<link=" + tag.ToLower() + "><style=Tooltip>" + temptag + "</style></link>";
								length = temptag.Length + 1;
							}
						}

						temp = temp.Substring(0, stIndex - 1) + (_isCommand ? "" : tag) + (stIndex+length < temp.Length ? temp.Substring(stIndex + length) : "");
						//Debug.Log("Tag = " + tag + "\nTemp = " + temp);
					}
					if (!_isCommand && Inkerface._recordMode)
						RecordLine(line);
					unTaggedText = line;
					line = temp;
				}
			}
			public string GetUntaggedText()
			{
				Debug.Log("Untagged text = '" + unTaggedText+"'"+"\nrawText = '"+rawText+"'");
				return unTaggedText;
			}
			public Reply(DialogCharacter dc, string txt)
			{
				dialogCharacter = dc;
				Construct(txt, false);
				//_text = TranslateSymbols(txt);
				originalText = txt;
			}
			public string TranslateSymbols(string l)
			{
				int quoteCounter = ContainsQuotationMarks(l);
				ReplaceQuotations();
				bool styleOn = false;
				int insertCount = 0;
				while (quoteCounter<l.Length-1 && quoteCounter>=0 && insertCount<10)
				{
					insertCount++;
					if (styleOn)
					{
						l = l.Insert(quoteCounter + 1, "</style>");
						quoteCounter += 10;
					}
					else
					{
						l = l.Insert(quoteCounter, "<style=quote>");
						quoteCounter += 15;
					}
					styleOn = !styleOn;
					if (ContainsQuotationMarks(l.Substring(quoteCounter)+1) == -1)
						break;
					string substring = l.Substring(quoteCounter + (!styleOn ? 1 : 0));
					quoteCounter += ContainsQuotationMarks(substring);
				}
				l = l.Replace(_symbols.glue, "");
				return l;

				void ReplaceQuotations()
				{
					int i = 0;
					int crashSaver = 20;
					while (l.Contains("\"") && crashSaver>0)
					{
						int q = l.IndexOf("\"");
						l = l.Substring(0, q) + (i % 2 == 0 ? "“" : "”") + (q + 1 < l.Length ? l.Substring(q + 1) : "");
						crashSaver--;
					}
				}
			}
			public bool IsGesture()
			{
				if (_text != null && (_text.Contains("gesture\">") || _text.Contains("GestureReply\">")))
				{
					return true;
				}
				else
					return false;
			}
			public string GetHistoryFormat()
			{
				if (!dialogCharacter)
				{
					Debug.Log("no dialog char");
					return FullLine();
				}
				if (dialogCharacter._name == "Atris")
				{
					//Debug.Log("dialogchar = Atris");
					int quoteIndex = ContainsQuotationMarks(rawText);
					if (quoteIndex==0)
					{
						//Debug.Log("Quote:" + "'"+rawText+"'");
						return dialogCharacter.GetCharacter("", ": ",default,true) + _text + "\n";
					}
					else
					{
						//Debug.Log("Gesture:" + "'" + rawText + "'");
						string temp =  dialogCharacter.GetColor() + "You " + "</color>" + _text.Substring(0, 1).ToLower() + _text.Substring(1)+"\n";
						Debug.Log(temp);
						return temp;
					}
				}
				return FullLine();
			}

			int ContainsQuotationMarks(string text)
			{
				foreach (string s in _symbols.quotationMarks)
				{
					int index = text.IndexOf(s);
					if (index >= 0)
						return index;
				}
				return -1;
			}
			public string FullLine()
			{
				return prefix + _text;
			}
			public string FullLineRaw()
			{
				return prefixRaw + rawText;
			}
			public string GetRawText()
			{
				return rawText;
			}
			public string GetPrefix()
			{
				return prefix;
			}
			public string GetPrefixRaw()
			{
				return prefixRaw;
			}
			public void Dbug()
			{
				return;
				Debug.Log("ID" + _replyID + "\n" + GetPrefix() + "+" + _text + "\n" + (IsGesture() ? "" : "NOT ") + "Gesture");
			}
			public void TrimQuotes()
			{
				foreach(string s in _symbols.quotationMarks)
				{
					_text = _text.Replace(s, "");
				}
			}
		}
		public static class InkFunction
		{
			/// <summary>
			/// Invoke a method according to <paramref name="tag"/>.
			/// </summary>
			/// <param name="tag">Which method to invoke. Not CaseSensitive. Separate method name and parameters with space</param>
			/// <returns></returns>
			public static bool Invoke(string tag)
			{
				bool skipToNext = true;
				string method = tag;
				object[] parameters = null;
				if (tag.Contains(" "))
				{
					List<string> parameterstrings;
					method = tag.Substring(0, tag.IndexOf(" "));
					parameterstrings = CodeTools.SliceString(tag.Substring(tag.IndexOf(" ") + 1), ' ');
					parameters = new object[parameterstrings.Count];
					int i = 0;
					foreach(string ps in parameterstrings)
					{
						parameters[i] = ps;
						i++;
					}
				}
				method = method.Substring(1);
				Type t = typeof(InkFunction);
				bool methodFound = false;
				foreach (MethodInfo mi in t.GetMethods())
				{
					//Debug.Log(mi.Name);
					if (mi.Name.ToLower().Equals(method.ToLower()))
					{
						skipToNext = (bool)t.GetMethod(mi.Name).Invoke(null, parameters);
						methodFound = true;
						break;
					}
				}

				if (skipToNext)
					_dialogueManager.WriteReady(0);
				//Return false if no method found with the name
				if(!methodFound)
					Debug.LogError("Unimplemented method " + method);
				return methodFound;
			}
			#region Darkness
			/// <summary>
			/// Enable darkness mode and turn screen black
			/// </summary>
			/// <returns></returns>
			public static bool Darkness()
			{
				_dialogueManager.ToggleDarkness(true);
				return true;
			}
			/// <summary>
			/// Enable darkness mode and turn screen white
			/// </summary>
			/// <returns></returns>
			public static bool White()
			{
				_dialogueManager.ToggleDarkness(true);
				_dialogueManager._background.GetComponent<Image>().color = Color.white;
				return true;
			}
			/// <summary>
			/// Enable SplashText fade in
			/// </summary>
			/// <returns></returns>
			public static bool StFadein()
			{
				DialogueManager.Settings.SplashText._writeMode = DialogueManager.Settings.SplashText.WriteMode.Fade;
				return true;
			}
			/// <summary>
			/// Enable SplashText write in
			/// </summary>
			/// <returns></returns>
			public static bool StWritein()
			{
				DialogueManager.Settings.SplashText._writeMode = DialogueManager.Settings.SplashText.WriteMode.Write;
				return true;
			}
			/// <summary>
			/// Enable SplashText InstantMode
			/// </summary>
			/// <returns></returns>
			public static bool StInstant()
			{
				DialogueManager.Settings.SplashText._writeMode = DialogueManager.Settings.SplashText.WriteMode.Instant;
				return true;
			}
			/// <summary>
			/// Enable SplashText RandomMode
			/// </summary>
			/// <returns></returns>
			public static bool StRandom()
			{
				DialogueManager.Settings.SplashText._writeMode = DialogueManager.Settings.SplashText.WriteMode.Random;
				return true;
			}
			/// <summary>
			/// Enable multiple SplashText to be displayed at the same time
			/// </summary>
			/// <param name="enabledString">boolean toggle</param>
			/// <returns></returns>
			public static bool StMultiText(string enabledString)
			{
				bool enabled;
				if (!bool.TryParse(enabledString, out enabled))
				{
					Debug.LogError("Can not parse '" + enabledString + "'as bool");
					return true;
				}
				DialogueManager.Settings.SplashText._enableMultiText = enabled;
				return true;
			}
			/// <summary>
			/// Set default values for SplashText (WriteIn true, multitext False)
			/// </summary>
			/// <returns></returns>
			public static bool StReset()
			{
				DialogueManager.Settings.SplashText.Reset();
				return true;
			}
			#endregion
			#region Storybook
			public static bool Storybook()
			{
				_dialogueManager.ToggleStorybook(true);
				return true;
			}
			public static bool Sbimg(string imgName)
			{
				Sprite sprite = Resources.Load<Sprite>("Storybook Art/" + imgName);
				if (sprite)
					_dialogueManager.SetStorybookImage(sprite);
				else
					Debug.LogError("Storybook Art with the name '" + imgName + "' not found!");
				return true;
			}
			public static bool Page()
			{
				_dialogueManager._storyBook.GetComponentInChildren<DialogPanel>().NextPage();
				return true;
			}
			#endregion
			#region Combat
			/// <summary>
			/// Activate CombatMode
			/// </summary>
			/// <returns>False</returns>
			public static bool Fight()
			{
				return Combat();
			}
			/// <summary>
			/// Activate CombatMode
			/// </summary>
			/// <returns>False</returns>
			public static bool Combat()
			{
				_dialogueManager.ToggleDialogPanel(false);
				_dialogueManager.ToggleDarkness(false);
				_dialogueManager.ToggleStorybook(false);
				CodeTools._combatManager.StartCombat();
				UpdateStoryVariables(false);
				_dialogueManager.DisableInput();
				return false;
			}
			/// <summary>
			/// Kill a character at <paramref name="targetPosition"/>
			/// </summary>
			/// <param name="targetPosition">Parsed to int, where to kill target</param>
			/// <returns>true</returns>
			public static bool Kill(string targetPosition)
			{
				int killChar = 0;
				if (Int32.TryParse(targetPosition, out killChar))
				{
					CodeTools._combatManager.KillCharacterAt(killChar);
				}
				else
				{
					Debug.LogError("Invalid Parameter on kill-command (" + targetPosition + ")");
				}
				return true;
			}
			#endregion
			#region Argument
			public static bool ArgMode()
			{

				//Enable visuals for Argument

				return true;
			}
			#endregion
			#region General
			/// <summary>
			/// Plays a sound effect
			/// </summary>
			/// <param name="clip">BaseFXAudio index or name</param>
			/// <returns></returns>
			public static bool PlaySound(string clip)
			{
				int i = -1;
				BaseFXAudio bfa = null;
				if (int.TryParse(clip, out i))
				{
					bfa = (BaseFXAudio)CodeTools.db.Get<BaseFXAudio>(i);
				}
				else
				{
					bfa = (BaseFXAudio)CodeTools.db.Get<BaseFXAudio>(clip);
				}
				if (bfa != null)
				{
					SoundEffectPlayer._current.Play(bfa);
				}
				else
				{
					Debug.LogError("Could not play audioclip '" + clip + "'");
				}
				return true;
			}
			/// <summary>
			/// Set DialogMode to Dialog
			/// </summary>
			/// <returns></returns>
			public static bool Dialogue()
			{
				_dialogueManager.ToggleDialogPanel(true);
				return true;
			}
			/// <summary>
			/// Set DialogMode to Dialog
			/// </summary>
			/// <returns></returns>
			public static bool Dialog()
			{
				return Dialogue();
			}
			/// <summary>
			/// Wait <paramref name="time"/> seconds before continue
			/// </summary>
			/// <param name="time">parse float, time in seconds to wait</param>
			/// <returns></returns>
			public static bool Wait(string time)
			{
				float t = 1;
				if(!float.TryParse(time,out t))
				{
					Debug.Log("Unable to parse " + time+" as float");
				}
				_dialogueManager.WriteReady(t);
				return false;
			}
			/// <summary>
			/// Go to a specific <paramref name="knot"/> at the next scene
			/// </summary>
			/// <param name="knot"></param>
			/// <returns></returns>
			public static bool KnotScene(string knot = default)
			{
				UpdateStoryVariables();
				TextAsset ta;
				ta = (TextAsset)Resources.Load("Ink/" + _nextScene);// + ".json");
				if (ta == null)
				{
					Debug.Log("Unable to find scene '" + _nextScene + "'");
					return false;
				}
				if (knot == null) knot = default;
				_dialogueManager.ChangeStory(ta, knot);
				return false;
			}
			/// <summary>
			/// Go to the start of the next scene
			/// </summary>
			/// <returns></returns>
			public static bool NextScene()
			{
				return KnotScene(default);
			}
			#endregion
		}
		public class Argument
		{
			public int _enemyWill;
			public int _playerWill;
			public Combat.CombatCharacter _pc;
			public Argument(int enemyWill = 20)
			{
				_pc = Combat.Party.GetCharacters()[0];
				_playerWill = _pc._attributes._tenacity.current * 3 + 20;
				_enemyWill = enemyWill;
			}
		}
	}
}