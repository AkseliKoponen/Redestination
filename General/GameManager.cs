using System.Collections;
using System.Collections.Generic;
using RD.Combat;
using RD.DB;
using RD.Dialog;
using RD;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Reflection;
using System.IO;

namespace RD
{
	public class GameManager : MonoBehaviour
	{
		public static bool _debug = true;
		public static GameManager _current;
		public GUIParts _guiParts;
		[System.NonSerialized]public StoryVariables _storyVariables;
		public bool _autoLoad = false;
		[System.Serializable]
		public struct GUIParts
		{
			public UI.MenuCanvas _menuCanvas;
			public DeckViewer _deckViewer;
			public GUIInventory _guiInventory;
			public UI.InspectionWindow _inspection;
			public Canvas _worldCanvas;
			public Canvas _UICanvas;
			public Canvas _highlightCanvas;
			public TestTransition testTransition;
			public Transform _partyStorage;
		}
		[System.NonSerialized] public bool _disableGeneralControls = false;
		enum MenuState
		{
			Default,
			Options,
			Map
		}
		private void Awake()
		{
			_current = this;
			SceneHandler.Init();
			CodeTools.InitDatabase();
			Cameras.Initialize(gameObject);
			Object.DontDestroyOnLoad(gameObject);
			_storyVariables = new StoryVariables();
			GameRules.SetMode(GameRules.DrawMode.HandLimit);
			if (CodeTools.CreateDirectoryIfNotExist(GetSaveSettings().FullPath)) Load();

		}
		static void UpdateTime()
		{
			CodeTools.Tm.UpdateDelta(UnityEngine.Time.deltaTime);
		}

		void Update()
		{
			UpdateTime();
		}

		public void ToggleMenu()
		{
			
			_guiParts._menuCanvas.Toggle(!_guiParts._menuCanvas.gameObject.activeSelf);
		}

		public void OnEscape(InputAction.CallbackContext cbt)
		{
			//return;
			if(CodeTools.IsKeyClick(cbt))
				SceneHandler.EnterScene("MainMenu");
			if (CombatManager._current != null)
			{

			}
		}

		public static void Five(InputAction.CallbackContext cbt)
		{
			if (CodeTools.IsKeyClick(cbt))
				CombatGUIHand._current.UpdateHandPositions(0);//Party.GetCharacters()[0].LevelUp();//Save();
		}
		public static void Four(InputAction.CallbackContext cbt)
		{
			if (CodeTools.IsKeyClick(cbt))
				WipeSaves();//Party.GetCharacters()[0].LevelUp();//Save();
		}
		public static void Three(InputAction.CallbackContext cbt)
		{
			if (CodeTools.IsKeyClick(cbt))
				WipeSaves();//Party.GetCharacters()[0].LevelUp();//Save();
		}
		public static void Two(InputAction.CallbackContext cbt)
		{
			if (CodeTools.IsKeyClick(cbt))
				WipeSaves();//Party.GetCharacters()[0].LevelUp();//Save();
		}
		public static void One(InputAction.CallbackContext cbt)
		{
			if (CodeTools.IsKeyClick(cbt))
				WipeSaves();//Party.GetCharacters()[0].LevelUp();//Save();
		}
		public static void Save()
		{
			ES3Settings settings = GetSaveSettings();
			Party.SaveParty();
			_current._storyVariables.Save(settings);
			//ES3.Save(GetSavePath+"StoryVariables", _current._storyVariables,settings);
		}
		public static void WipeSaves()
		{

			ES3Settings settings = GetSaveSettings();
			System.IO.DirectoryInfo di = new DirectoryInfo(settings.FullPath);

			foreach (FileInfo file in di.GetFiles())
			{
				file.Delete();
			}
			foreach (DirectoryInfo dir in di.GetDirectories())
			{
				dir.Delete(true);
			}
			Debug.Log("Saves wiped!");
		}
		public static void Load()
		{
			if (!_current._autoLoad)
				return;
			ES3Settings settings = GetSaveSettings();
			_current._storyVariables.Load(settings);
			Party.LoadParty();
			Party.Store();
		}
		public static ES3Settings GetSaveSettings()
		{
			ES3Settings settings = new ES3Settings();//new ES3Settings(ES3.EncryptionType.AES, "y0ushalln0tpassw0rd");
			settings.path = GetSaveFolder();
			return settings;
		}
		public static string GetSaveFolder()
		{
			return "TopSecret/DoNotOpen/Seriously/";
		}

		#region Testing
		public static void AfterCombatMenu()
		{
			TestTransition tt = _current._guiParts.testTransition;
			tt.AfterCombatMenu();

		}
		public static void NextCombat()
		{
			_current._guiParts.testTransition.Hide();
			Debug.Log("NextCombat!");
			SceneHandler.EnterScene("combat");
		}
		#endregion
	}

	public static class SceneHandler
	{
		public static void Init()
		{
			_args = new Arguments();
		}
		public static Arguments _args;
		public static SceneConstructor _sceneConstructor;
		public class Arguments
		{
			public List<string> sceneArgs;
			public Arguments()
			{
				sceneArgs = new List<string>();
			}
		}

		public static void EnterScene(string scene, List<string> args = default)
		{
			if (args == default)
				args = new List<string>();
			_args.sceneArgs = args;
			switch (scene.ToLower())
			{
				default:
					Debug.LogError("Unknown Scene ('" + scene + "')");
					break;
				case "dialog":
					EnterBaseScene((SceneConstructor)CodeTools.db.Get <SceneConstructor>(3));
					break;
				case "base":
					if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("BaseScene"))
						GameManager._current.StartCoroutine(LoadScene("BaseScene", false));
					else
						GameManager._current.StartCoroutine(LoadingScreen(scene, args));//CodeTools._combatManager.CreateArenas();
					break;
				case "new game":
					GameManager._current.StartCoroutine(LoadScene("New Game", false));
					break;
				case "map":
					GameManager._current.StartCoroutine(LoadScene("Map", false));
					break;
				case "mainmenu":
					GameManager._current.StartCoroutine(LoadScene("MainMenu", false));
					break;
				case "movement":
					GameManager._current.StartCoroutine(LoadScene("Movement", false));
					break;
				case "combat":
					EnterBaseScene((SceneConstructor)CodeTools.db.Get<SceneConstructor>("Combat Test"));
					break;
				case "load":

					break;
			}
		}
		public static void EnterBaseScene(SceneConstructor sceneConstructor)
		{
			_sceneConstructor = sceneConstructor;
			EnterScene("Base", new List<string> { _sceneConstructor._inkFile.name});
		}
		static IEnumerator LoadScene(string sceneName, bool additive = false)
		{
			Party.Store();
			AsyncOperation ops;
			ops = SceneManager.LoadSceneAsync(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
			while (!ops.isDone)
			{
				yield return null;
			}
			if (!additive)
				SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

		}
		static IEnumerator LoadingScreen(string scene, List<string> args = default, bool save = true)
		{
			Task t = new Task(LoadScene("BaseScene", false));
			while (t.Running)
			{
				yield return null;
			}
			if (save)
				GameManager.Save();
			EnterScene(scene, args);
		}
	}

	[System.Serializable]
	public class StoryVariables
	{
		static string _fileName = "StoryVariables.es3";
		public StoryVariables()
		{
		}
		public string _occupation = "inventor"; //"inventor", "soldier", "spy"
		public int _connection = 100;
		#region Inventor Specific Variables
		public bool _wirdScolded = false;
		#endregion
		#region Soldier Specific Variables
		#endregion
		#region Spy Specific Variables
		#endregion
		static List<string> ignoredFields = new List<string>{
			"_ignoredFields"
			};
		public void Load(ES3Settings settings)
		{
			
			if(ES3.FileExists(settings.FullPath + _fileName) == false)
			{
				Debug.Log("New Save");
				return;
			}
			string loadPath = GameManager.GetSaveFolder() + _fileName;
			foreach (FieldInfo fi in GetType().GetFields())
			{
				string fname = fi.Name;
				if (ignoredFields.Contains(fname))
					continue;
				//Debug.Log(fname);
				switch (fname)
				{
					default:
						fi.SetValue(this, ES3.Load(fname, loadPath, settings));
						break;
				}
			}

		}
		public void Save(ES3Settings settings)
		{
			foreach (FieldInfo fi in GetType().GetFields())
			{
				string fname = fi.Name;
				if (ignoredFields.Contains(fname) || fi.GetValue(this) == null)
					continue;
				//Debug.Log(fname);

				switch (fname)
				{
					default:
						ES3.Save(fname, fi.GetValue(this), GameManager.GetSaveFolder() + _fileName, settings);
						break;
				}
			}
		}
		public class PsychosocialCrisis
		{
			public string _name;
			public string _description;
			public State _state = State.Inactive;
			public enum State {Inactive, Active, Outcome1, Outcome2}
			string outcome1;
			string outcome2;
		
		}
	}
	public static class GameRules
	{
		public static int _handLimitHard { get; private set; } = 10;	//The maximum number of cards possible in hand under any circumstance
		public static int _handLimit { get; private set; } = 5;         //See DrawMode.HandLimit
		public static CodeTools.Bint _deckLimit { get; private set; } = new CodeTools.Bint(10, 10);	//The maximum and minimum amount of cards the player can have in their deck
		public static int _duplicateMax { get; private set; } = 1;		//How many copies of the same card you can have in your deck
		public static int _actionsPerTurnNPC { get; private set; } = 1;
		public static int _actionsPerTurnPC { get; private set; } = 2;
		public static int _drawPerTurnNPC { get; private set; } = 1;	//Only applies in FlatDraw mode
		public static int _drawPerTurnPC { get; private set; } = 1;     //Only applies in FlatDraw mode
		public static bool _drawAtTheStartOfTurn { get; private set; } = true; //If false, draw at the end of turn
		public enum DrawMode { FlatDraw, HandLimit}
		/*
	 Flat Draw: Draw flat amount of cards each turn.
	 ----
	 HandLimit: Draw cards upto hand limit.
	 You may hold more cards than HandLimit, but you will not draw more until you get rid of them.
	 -----
	*/
		public static DrawMode _drawMode { get; private set; } = DrawMode.HandLimit;
		public static void SetMode(DrawMode dm)
		{
			_deckLimit.SetMin(10);
			_deckLimit.SetMax(10);
			_drawMode = dm;
			switch (_drawMode)
			{
				case DrawMode.FlatDraw:
					_handLimit = _handLimitHard;
					_actionsPerTurnPC = 3;
					_drawAtTheStartOfTurn = false;
					break;
				case DrawMode.HandLimit:
					_handLimit = 5;
					_actionsPerTurnPC = 3;
					_drawAtTheStartOfTurn = true;
					break;
			}
		}

	}
	public static class GameSettings
	{
		public static class UISettings
		{
			static float _endTurnDelayPC = 0.5f;
			static float _endTurnDelayNPC = 0.5f;
			public static bool _automaticTurnEnd = false;
			public static float GetEndTurnDelay(Combat.CombatCharacter.Alliance alliance)
			{
				switch (alliance)
				{
					default:
						return _endTurnDelayNPC;
					case Combat.CombatCharacter.Alliance.Player:
						return _endTurnDelayPC;
				}
			}
			public static bool _enableCardTargetingAreas = false;
		}
		public static class CombatSettings
		{
			public static bool _fastTargeting = true;
		}
	}

	

}
namespace RD.Combat
{
	public static class Party
	{
		static string _fileName = "Party.es3";
		public class CombatCharacterInfo
		{
			public CombatCharacter cc;
			public CombatCharacterData ccd;
			public BaseCombatCharacter bcc;
			public CombatCharacterInfo(BaseCombatCharacter data)
			{
				bcc = data;
				cc = null;
				ccd = null;
				Instantiate();
				ccd = new CombatCharacterData(cc);
			}
			public CombatCharacterInfo(CombatCharacterData data)
			{
				ccd = data;
				bcc = ccd._bcc;
				cc = null;
			}
			public CombatCharacter Instantiate()
			{
				cc = GameObject.Instantiate(bcc._visualObject);
				if (ccd == null) cc.Initialize(bcc);
				else cc.Initialize(this);
				return cc;
			}
		}
		static List<CombatCharacterInfo> _ccis;
		public static List<CombatCharacter> GetCharacters()
		{
			if (_ccis != null)
				_ccis = CodeTools.TrimNulls(_ccis);
			if (_ccis == null || _ccis.Count == 0)
			{
				_ccis = new List<CombatCharacterInfo>();
			}
			List<CombatCharacter> ccs = new List<CombatCharacter>();
			if (_ccis.Count == 0)
			{
				_ccis.Add(new CombatCharacterInfo((BaseCombatCharacter)CodeTools.db.Get<BaseCombatCharacter>(0)));
			}
			foreach(CombatCharacterInfo cci in _ccis)
			{
				cci.ccd._cc = cci.cc;
			}
			for(int i = 0; i < _ccis.Count; i++)
			{
				if (_ccis[i].cc)
					ccs.Add(_ccis[i].cc);
				else
				{
					ccs.Add(_ccis[i].Instantiate());
				}
			}
			foreach (CombatCharacter cc in ccs)
			{
				//cc.Prepare();
			}
			//Dbug();
			return ccs;
			void Dbug()
			{

				Debug.Log("Party contains the following characters:");
				for(int i = 0; i < ccs.Count; i++)
				{
					Debug.Log("[" + i + "] - " + ccs[i].GetName() +" instantiated");
				}
			}
		}
		public static void LoadParty()
		{
			_ccis = new List<CombatCharacterInfo>();
			ES3Settings settings = GameManager.GetSaveSettings();
			if (!ES3.FileExists(settings.FullPath + _fileName))
				return;
			List<string> saveNames = (List<string>)ES3.Load("SaveNames", GameManager.GetSaveFolder() + "Party.es3", settings);
			if (saveNames == null || saveNames.Count == 0)
				return;
			foreach(string sn in saveNames)
			{
				_ccis.Add(new CombatCharacterInfo(new CombatCharacterData(sn,settings)));
			}
			GetCharacters();
		}
		public static void SaveParty()
		{
			if (_ccis != null)
			{
				ES3Settings settings = GameManager.GetSaveSettings();
				List<string> saveNames = new List<string>();
				foreach (CombatCharacterInfo cci in _ccis)
				{
					//cci.ccd._cc = cci.cc;
					saveNames.Add(cci.ccd.Save(settings));
				}

				ES3.Save("SaveNames", saveNames, GameManager.GetSaveFolder() + "Party.es3", settings);
				//ES3.Save("SaveNames", saveNames, settings);
			}
		}
		static void LogParty()
		{
			Debug.Log("---------");
			Debug.Log("Party contains:");
			foreach (CombatCharacterInfo cci in _ccis)
			{
				if (cci.cc != null)
					Debug.Log(cci.cc.GetName());
				else
					Debug.Log("Unavailable CC");
			}
			Debug.Log("---------");
		}
		public static void Store()
		{
			foreach (CombatCharacterInfo inf in _ccis)
			{
				
				CombatCharacter cc = inf.cc;
				cc.transform.SetParent(GameManager._current._guiParts._partyStorage);
				CodeTools.SetLayer(inf.cc.transform, 31);
				cc.gameObject.SetActive(false);
			}
		}
	}

	public class CombatCharacterData
	{
		static List<string> ignoredFields = new List<string>{
		"_ignoredFields",
		"_cc",
		"_visualObject",
		"_bcc"
		};
		public CombatCharacter _cc;
		public BaseCombatCharacter _bcc;
		public int _bccID;
		public string _name;
		public string _description;
		public int _level;
		public int _meleeID = -1;
		public int _rangedID = -1;
		public int _armorID = 0;
		public int _ringID = 0;
		public int _amuletID = 0;
		public int _talismanID = 0;
		public List<int> _inventoryItems = new List<int>();
		public List<int> _inventoryWeapons = new List<int>();
		public List<int> _inventoryEquipment = new List<int>();
		public List<int> _inventoryConsumables = new List<int>();
		public List<int> _deckCards = new List<int>();
		public List<int> _talentIDs = new List<int>();
		public CombatCharacter.Attributes _attributes = new CombatCharacter.Attributes();
		public CombatCharacter.Stats _stats;
		public CombatCharacter.GlobalStats _globalStats;
		public string Save(ES3Settings settings)
		{
			Debug.Log("Saving " + _name);
			RefreshData();
			string saveName = GameManager.GetSaveFolder()+"Party - " + _name + ".es3";
			foreach (FieldInfo fi in GetType().GetFields())
			{
				string fname = fi.Name;
				if (ignoredFields.Contains(fname) || fi.GetValue(this) == null)
					continue;
				//Debug.Log(fname);
				switch (fname)
				{
					default:
						ES3.Save(fname, fi.GetValue(this), saveName, settings);
						break;
				}
			}
			return saveName;
		}
		public void RefreshData()
		{
			LoadInventory();
			_deckCards = new List<int>();
			foreach (Card c in _cc._deck._recollection._ideas) _deckCards.Add(c._id);
			_talentIDs = new List<int>();
			foreach (BaseTalent bt in _cc._talents) _talentIDs.Add(bt._id);
			_name = _cc.GetName(false);
			_description = _cc._description;
			_level = _cc._level;
			_attributes = _cc._attributes;
			_stats = _cc._stats;
			_globalStats = _cc._globalStats;
			_bcc = _cc._baseCC;
			_bccID = _bcc._id;
			void LoadInventory()
			{
				Inventory inv = _cc._inventory;
				foreach (BaseItem it in inv._items) _inventoryItems.Add( it._id);
				foreach (BaseEquipment it in inv._equipment) _inventoryEquipment.Add(it._id);
				foreach (BaseConsumable it in inv._consumables) _inventoryConsumables.Add(it._id);
				foreach (BaseWeapon it in inv._weapons) _inventoryWeapons.Add(it._id);

				_meleeID = inv._melee != null ? inv._melee._id : 0;
				_rangedID = inv._ranged != null ? inv._ranged._id : 0;
				_ringID = inv._ring != null ? inv._ring._id : 0;
				_armorID = inv._armor != null ? inv._armor._id : 0;
				_amuletID = inv._amulet != null ? inv._amulet._id : 0;
				_talismanID = inv._talisman != null ? inv._talisman._id : 0;
			}
		}
		public CombatCharacterData(CombatCharacter cc)
		{
			if (cc == null)
				return;
			_cc = cc;
			RefreshData();
			//Other Equipment?

		}
		public CombatCharacterData(string loadPath,ES3Settings settings)
		{
			foreach (FieldInfo fi in GetType().GetFields())
			{
				string fname = fi.Name;
				if (ignoredFields.Contains(fname))
					continue;
				//Debug.Log(fname);
				switch (fname)
				{
					default:
						fi.SetValue(this, ES3.Load(fname, loadPath, settings));
						break;
				}
			}
			_bcc = (BaseCombatCharacter)CodeTools.db.Get<BaseCombatCharacter>(_bccID);
		}
	}
}