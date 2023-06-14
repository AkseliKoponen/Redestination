using System;
using System.Collections;
using System.Collections.Generic;
using RD.DB;
using RD.Dialog;
using RD;
using UnityEngine;
using UnityEngine.InputSystem;
using static RD.CodeTools;
using static UnityEditor.PlayerSettings;

namespace RD.Combat
{
	public class CombatManager : MonoBehaviour
	{
		public bool debug;
		//public List<CombatCharacter> fighters;
		//public List<Facing> fighterOrientations;
		[NonSerialized]
		public List<CombatCharacter> _combatCharacters;
		[NonSerialized]
		public List<CombatTerrain> _terrains;
		//public GameObject _turnArrow;
	//	public Arena _arena;
		//public Arena _friendlyArena;
		BaseFight _baseFight;
		List<CombatCharacter> _highlightedCharacters = new List<CombatCharacter>();
		CombatCharacter _selectedCharacter;
		[SerializeField] Transform _damageTextParent;
		[SerializeField] RectTransform _bottomBar;
		bool _combatStarted = false;
		public GameObject _terrainPrefab;
		public static CombatManager _current;
		public void Reset()
		{
			Graveyard._current.Clear();
			_combatCharacters.Clear();
			_highlightedCharacters = new List<CombatCharacter>();
			
		}
		private void Awake()
		{
			CodeTools.SetCombatManager(this);
			_current = this;
			CreateArenas();
		}
		public void StartCombat()
		{
			GUIAnimationManager.Reset();
			_continueCombat = true;
			ExpandBottomBar(false);
			TurnDisplay._current.Init(_combatCharacters);
			float time = 1;
			StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(_combatGUI.GetComponent<CanvasGroup>(), true, 1, true));

			#region FinalizeCharacters
			foreach(CombatCharacter cc in ArenaManager._hostile._cChars)
				cc.DrawCard(5, false, 0);
			_combatGUI.AddHealthbars(_combatCharacters);

			#endregion
			_combatStarted = true;
			Invoke("StartTurn", time);
		}
		void StartTurn()
		{
			ArenaManager.NextTurn();
		}

		public void InputSpace(InputAction.CallbackContext cbt)
		{/*
		if (_combatStarted)
		{
			if (IsKeyClick(cbt))
				_arena._currentCharacter.EndTurn();
			//Victory();
		}*/
		}
		public void InputD(InputAction.CallbackContext cbt)
		{
			if (_combatStarted)
			{
				if (IsKeyClick(cbt))
					ArenaManager._currentCharacter.DrawCard();
			}
		}
		void CreateArenas()
		{
			if (SceneHandler._sceneConstructor._baseFight == null || SceneHandler._sceneConstructor._baseFight._hostileArena == null)
			{
				Debug.LogError("Unassigned fight or arena");
				return;
			}
			//Create terrains equal to combatSpaces count
			_combatCharacters = new List<CombatCharacter>();
			_terrains = new List<CombatTerrain>();
			_baseFight = SceneHandler._sceneConstructor._baseFight;
			if (_baseFight._enemySide == Facing.Left)
			{
				ArenaManager._hostile = new Arena(ReadArena(_baseFight._hostileArena));
				CreateFriendlies();
			}
			else
			{
				CreateFriendlies();
				ArenaManager._hostile = new Arena(ReadArena(_baseFight._hostileArena));
			}
			ArenaManager.CountTerrains();
			AddCombatCharacterSuffixes(_combatCharacters);
			for (int t = 0; t < _terrains.Count; t++)
			{
				CombatTerrain ct = _terrains[t];
				Vector3 pos = ct.transform.position;
				pos.x = t * 2 - (_terrains.Count - 1);
				pos.y += ct.transform.parent.position.y;
				ct.transform.position = pos;
				if (ct.ContainsCharacter())
				{
					ct.GetCharacter().transform.position = pos;
				}
			}
			ArenaManager.SetOrientation(_baseFight._enemySide);
			foreach(CombatCharacter cc in _combatCharacters)
			{
				SetLayer(cc.transform, 9);
			}
			List<CombatTerrain> ReadArena(BaseFight.Arena arena)
			{
				List<CombatTerrain> cts = new List<CombatTerrain>();
				for (int i = 0; i < arena._combatSpaces.Count; i++)
				{
					CombatTerrain ct = CreateCombatTerrain();
					cts.Add(ct);
					//Create combatCharacters to the terrains that should have them
					if (!arena._combatSpaces[i]._empty && arena._combatSpaces[i]._occupant)
					{
						CombatCharacter cc = arena._combatSpaces[i]._occupant.Instantiate();
						_combatCharacters.Add(cc);
						//Debug.Log(cc.GetName());
						ct.SetObject(cc._cSprite._selectableObject);
						cc.transform.SetParent(GameObject.Find("Characters").transform);
					}
				}
				return cts;
			}
			CombatTerrain CreateCombatTerrain()
			{

				CombatTerrain ct = Instantiate(_terrainPrefab).GetComponent<CombatTerrain>();
				_terrains.Add(ct);
				ct.transform.SetParent(GameObject.Find("Terrains").transform);
				return ct;
			}
			void CreateFriendlies()
			{
				List<CombatCharacter> party = Party.GetCharacters();
				List<CombatTerrain> combatSpaces = ReadArena(_baseFight._friendlyArena);
				List<CombatTerrain> emptySpaces = new List<CombatTerrain>();
				foreach (CombatTerrain ct in combatSpaces)
					if (ct.IsEmpty())
						emptySpaces.Add(ct);
				while (emptySpaces.Count < party.Count)
				{
					CombatTerrain extra = CreateCombatTerrain();
					emptySpaces.Add(extra);
					combatSpaces.Add(extra);
				}
				for (int i = 0; i < party.Count; i++)
				{
					int spaceIndex = _baseFight._enemySide == Facing.Left ? i : emptySpaces.Count - 1 - i;
					emptySpaces[spaceIndex].SetObject(party[i]._selectObject);
					_combatCharacters.Add(party[i]);
					party[i].transform.SetParent(GameObject.Find("Characters").transform);
				}
				ArenaManager._friendly = new Arena(combatSpaces);

				foreach (CombatCharacter cc in party)
					cc.Prepare();
			}
			//Positions Characters and terrains
		}
		void AddCombatCharacterSuffixes(List<CombatCharacter> ccs)
		{
			List<string> names = new List<string>();
			foreach (CombatCharacter cc in ccs)
				names.Add(cc._name.name);
			for (int i = 0; i < names.Count; i++)
			{
				string name = names[i];
				int counter = 1;
				int totalCounter = 0;
				for (int k = 0; k < names.Count; k++)
				{
					if (names[k] == name)
					{
						totalCounter++;
						if (k < i)
							counter++;
					}

				}
				if (totalCounter > 1)
					ccs[i].AddToName("", " " + counter);
			}
		}
		public void UpdateTargetSide(CombatCharacter co, bool rightSide)
		{

			List<CombatCharacter> multiObjects = _combatGUI._comps._targetingSystem.HandleMultiObjectTargeting(co, rightSide);
			if (multiObjects != null && multiObjects.Count > 0)
			{
				foreach (CombatCharacter cc in multiObjects)
				{
					if (cc)
						cc._cSprite._statusBar.Select(true);
					HighlightObject(cc);
					TurnDisplay._current.PreviewHasteCharacter(cc, TargetingSystem._current._card.GetHasteEffect(cc));
				}
			}
		}
		public bool OnHighlight(CombatCharacter co,bool rightSide)
		{
			//Debug.Log("CM Highlight " + co._cc.GetName());
			if (!_combatStarted || GameManager._current._guiParts._inspection.gameObject.activeSelf)
				return false;
			if (_combatGUI._state == CombatGUI.StateOfGUI.Free || (_combatGUI._state == CombatGUI.StateOfGUI.Targeting && _combatGUI._comps._targetingSystem.CheckObjectValid(co)))
			{
				if (_highlightedCharacters.Count > 0)
					UnHighlight();

				HighlightObject(co);
				if (_combatGUI._state == CombatGUI.StateOfGUI.Targeting && _combatGUI._comps._targetingSystem.CheckObjectValid(co))
				{
					TurnDisplay._current.PreviewHasteCharacter(TargetingSystem._current._cc, TargetingSystem._current._card.GetHasteEffect(TargetingSystem._current._cc));
					TurnDisplay._current.PreviewHasteCharacter(co, TargetingSystem._current._card.GetHasteEffect(co));
					if (TargetingSystem._current._card._gUICard != null && TargetingSystem._current.ready == 0)
					{
						TargetingSystem._current._card._gUICard.UpdateTexts(co);
						
					}
					co._cSprite._statusBar.Select(true);
				}
				return true;
			}
			else
			{
				//Debug.Log(_combatGUI._state);
				return false;
			}

		}
		void HighlightObject(CombatCharacter co)
		{
			_highlightedCharacters.Add(co);
			if (_combatGUI._state == CombatGUI.StateOfGUI.Targeting)
			{
				TurnDisplay._current.TargetCharacter(co, true);
			}
			else TurnDisplay._current.HighlightCharacter(co, true);
			co._cSprite._statusBar.ToggleText(true);

		}
		public void UnHighlight()
		{
			//Debug.Log("Unhighlight!");
			if (_highlightedCharacters.Count==0 || !_combatStarted)
				return;
			if (_combatGUI._state == CombatGUI.StateOfGUI.Targeting)
			{
				_combatGUI._comps._targetingSystem.HighlightMultiTargetObjs(false);
				TurnDisplay._current.RevertHastePreview();
				if (TargetingSystem._current._card._gUICard != null && TargetingSystem._current.ready == 0)
				{
					TargetingSystem._current._card._gUICard.UpdateTexts();
					
				}
			}
			foreach (CombatCharacter co in _highlightedCharacters)
			{
				UnHighlightCharacter(co);
			}
			_highlightedCharacters.Clear();
			void UnHighlightCharacter(CombatCharacter co)
			{
				if (co == null)
					return;
				//co._cSprite._statusBar.ToggleText(false);
				if (_combatGUI._state == CombatGUI.StateOfGUI.Targeting)
					TurnDisplay._current.TargetCharacter(co, false);
				else
					TurnDisplay._current.HighlightCharacter(co, false);
				co._selectObject.UnHighlight();
			}
		}

		public void ToggleStatusTexts(bool enabled)
		{
			if (!enabled)
			{
				foreach (CombatCharacter cc in _combatCharacters)
				{
					cc._cSprite._statusBar.ToggleText(false);
				}
			}
			else
			{
				foreach(CombatCharacter cc in _highlightedCharacters)
				{
					cc._cSprite._statusBar.ToggleText(true);
				}
			}
		}
		public bool SelectCharacter(CombatCharacter selectedCharacter)
		{
			selectedSomething = true;
			if (_combatGUI._state == CombatGUI.StateOfGUI.Free)
			{
				selectedCharacter._selectObject.SelectionIndicator(true);
				if (_selectedCharacter)
					_selectedCharacter._selectObject.DeSelect();
				_selectedCharacter = selectedCharacter;
				//Move Selection arrow
				return true;
			}
			else if (_combatGUI._state == CombatGUI.StateOfGUI.Targeting)
			{
				
				_combatGUI._comps._targetingSystem.TargetObject(selectedCharacter);
				return false;
			}
			else return false;

		}
		public void LimitSelection(List<CombatCharacter> targets, bool enable = false)
		{
			if (!enable)
			{
				foreach (CombatCharacter cc in _combatCharacters)
				{
					if (!targets.Contains(cc))
						cc._selectObject.enableInteraction = false;
					else
						cc._selectObject.enableInteraction = true;
				}
			}
			else
				foreach (CombatCharacter obj in _combatCharacters)
				{
					obj._selectObject.enableInteraction = true;
				}
		}
	
		bool selectedSomething = false;
		public IEnumerator DeSelect()
		{
		
			yield return null;
			if (!selectedSomething)
			{
				if (_selectedCharacter != null)
				{
					_selectedCharacter._selectObject.DeSelect();
					_selectedCharacter = null;
				}
			}
			selectedSomething = false;
		}
		public bool KillCharacterAt(int position)
		{
			//Positition = 8
			//Count = 9
			if (ArenaManager._hostile._terrains.Count > position)
			{
				if (ArenaManager._hostile._terrains[position].ContainsCharacter())
					StartCoroutine(KillCharacter(ArenaManager._hostile._terrains[position].GetCharacter()));
				else
				{
					Debug.LogError("Position " + position + " does not contain character to be killed.");
					return false;
				}
				return true;
			}
			else {
				Debug.LogError("Position " + position + " out of bounds!");
				return false;
			}
		}
		public IEnumerator KillCharacter(CombatCharacter victim, CombatCharacter source = null)
		{
			if (victim.Die(source) == false)
				yield break;
			float delay = 0.05f; 
			while (delay > 0)
			{
				delay -= Tm.GetWorldDelta();
				yield return null;
			}
			while (_combatCharacters.Contains(victim))
				_combatCharacters.Remove(victim);
			while (_highlightedCharacters.Contains(victim))
				_highlightedCharacters.Remove(victim);
			ArenaManager.RemoveObject(victim);
			CheckConditions();
		}
		public void PushCharacter(int pushAmount, CombatCharacter source, CombatCharacter target, bool stopOnCollision)
		{
			//int direction = ArenaManager.GetStandardizedDirection(target);
			MoveCharacter(target, pushAmount, false);
		}
		
		public void MoveCharacter(CombatCharacter cc,int moveAmount = 0, bool stopOnCollision = false)
		{

			Arena a = ArenaManager.GetContainingArena(cc);
			int currentPosition = ArenaManager.GetPosition(cc);
			//Make sure CC doesn't go over the limit
			if (moveAmount < 0) {
				while (currentPosition + moveAmount < 0)
					moveAmount++;
			}
			else while (currentPosition + moveAmount >= a._terrains.Count)
				moveAmount--;
			if (moveAmount != 0)
			{
				
				CombatTerrain currentTerrain = a._terrains[currentPosition];
				CombatTerrain targetTerrain = a._terrains[currentPosition + moveAmount];
				if (!targetTerrain.IsEmpty() && stopOnCollision)
					return;
				currentTerrain.Empty();
				Vector3 finalDestination = cc.transform.position;
				int opposite = moveAmount > 0 ? -1 : 1;
				for(int i = 1; i <= Mathf.Abs(moveAmount); i++)
				{
					CombatTerrain t = a._terrains[currentPosition + i * opposite*-1];
					finalDestination.x = t.transform.position.x;
					if (!t.IsEmpty())
					{
						MoveCharacter(t.GetCharacter(), opposite, false);
					}

				}
				targetTerrain.SetObject(cc._cSprite._selectableObject);
				cc._cSprite.MoveToPosition(finalDestination, _combatGUI.durationEstimate);
			}
			else
				Debug.Log("<color=red>Couldn't move " + cc.GetName()+"</color>");
		}
		public Transform AddDamageTextParent(GameObject source)
		{
			string name = source.transform.parent.gameObject.name + source.GetInstanceID();
			Transform parent = _damageTextParent.Find(name);
			if (parent == null)
			{
				GameObject obj = new GameObject(name);
				obj.transform.SetParent(_damageTextParent);
				obj.name = name;
				obj.transform.position = source.transform.position;
				obj.AddComponent<RectTransform>();
				parent = obj.transform;
			}
			return parent;
		}
		public bool CombatActive()
		{
			return _combatStarted;
		}
		public bool _continueCombat { get; private set; } = true;
		public void CheckConditions()
		{

			if (DefeatCondition.CheckDefeatCondition(_baseFight._defeatConditionID))
			{
				Defeat();
			}
			else if (VictoryCondition.CheckVictoryCondition(_baseFight._victoryConditionID))
			{
				Victory();
			}

		}
		public void Defeat()
		{
			if (!_continueCombat)
				return;
			Debug.Log("___________________________");
			Debug.Log("|    YOU LOSE. DEFEAT.    |");
			Debug.Log("---------------------------");
			_continueCombat = false;
		}
		public void Victory(bool debug = true)
		{
			if (!_continueCombat)
				return;
			_continueCombat = false;
			foreach(CombatCharacter cc in _combatCharacters)
			{
				if (cc != null)
					cc.EndCombat();
			}
			//Victory graphics
			if (debug)
			{
				GameManager.AfterCombatMenu();
			}
			else
			{
				ExitCombat(debug);
				Inkerface._dialogueManager.CombatVictory();
				ExpandBottomBar(true);
			}
		}
		public void ExitCombat(bool debug)
		{
			//Save the party
			_continueCombat = false;
			CanvasGroup canvasGroup = _combatGUI.GetComponent<CanvasGroup>();
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
			StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(canvasGroup, false, 1, false));
			_combatStarted = false;
			if (debug)
			{

			}
			else
			{

				GameManager.Save();
			}
		}
		void ExpandBottomBar(bool expand)
		{
			float expandsize = 400;
			float contractsize = 350;
			StartCoroutine(UIAnimationTools.LerpSizeDelta(_bottomBar, new Vector2(_bottomBar.sizeDelta.x, (expand ? expandsize : contractsize)), 0.25f));
		}
		public static class ArenaManager
		{
			public static Arena _friendly;
			public static Arena _hostile;
			public static CombatCharacter _currentCharacter;
			public static Facing _orientation { get; private set; }
			static List<CombatTerrain> _terrains;
			public static void CountTerrains()
			{
				_terrains = new List<CombatTerrain>();
				_terrains.AddRange(GetTerrains());
				float cameraSize = Mathf.Clamp((float)_terrains.Count / 2f, 4f, 7f);
				Cameras.SetSize(cameraSize);
			}
			public static void NextTurn()
			{

				if (!CombatManager._current._continueCombat)
					return;
				_currentCharacter = TurnDisplay._current.NextTurn();
				_combatGUI.DisplayCharacter(_currentCharacter);
				_currentCharacter.StartTurn();
				_combatGUI._comps._messageBox.DisplayTurn(_currentCharacter);
				//Set cards visible
				//draw cards
			}
			public static bool IsPlayerTurn()
			{
				return _currentCharacter._alliance == CombatCharacter.Alliance.Player;
			}
			public static Arena GetContainingArena(CombatCharacter cc)
			{
				if (_friendly.HasCharacter(cc))
					return _friendly;
				else if (_hostile.HasCharacter(cc))
					return _hostile;
				Debug.Log("Character not found in either");
				return _hostile;
			}

			public static void SetOrientation(Facing enemySide)
			{
				_orientation = enemySide;
				_hostile._facing = GetReverseFacing(_orientation);
				foreach(CombatCharacter cc in _hostile._cChars)
				{
					if (cc._facing != _hostile._facing)
						cc.Flip();
				}
				_friendly._facing = _orientation;
				foreach (CombatCharacter cc in _friendly._cChars)
				{
					if (cc._facing != _friendly._facing)
						cc.Flip();
				}
			}
			public static List<CombatTerrain> GetSpacesInRange(CombatCharacter cc, Bint range, bool requireLineOfSight = false)
			{
				List<CombatTerrain> spc = new List<CombatTerrain>();
				int position = GetSpaceIndexOfChar(cc);
				if (position == -1)
				{
					Debug.LogError("CombatCharacter " + cc.GetName() + " not found in spaces.");
					return spc;
				}
				//pos = 3, range = 2 -> get all from 1 to 5
				#region Add Spaces within range of source position
				List<CombatTerrain> cts = GetTerrains();
				for (int i = position - range.max; i <= range.max + position && i < cts.Count; i++)
				{
					if (i < 0 || i >= cts.Count)
						continue;

					bool valid = true;
					if (requireLineOfSight && Mathf.Abs(i - position) > 1)
					{

						//if LoS required and space between source and i, check if the between spaces are empty
						// X0-0-A = 0-5 = -5
						//int diff = Mathf.Abs(i - position);
						int dir = i < position ? 1 : -1;
						for (int k = i + dir; k != position; k += dir)
						{
							if (k < 0 || k >= cts.Count)
								continue;

							//if any terrain between source and i blocks LoS, don't add the terrain to list
							if (cts[k].BlockLoS())
								valid = false;
						}
					}
					if (valid)
						spc.Add(cts[i]);
				}
				#endregion
				return spc;
			}
			public static List<CombatTerrain> GetSpacesInMeleeRange(CombatCharacter cc)
			{
				List<CombatTerrain> spaces = new List<CombatTerrain>();
				spaces.Add(GetTerrains()[GetPosition(cc)]);
				if (_friendly.HasCharacter(cc))
					spaces.Add(_hostile.GetMeleeTerrain());
				else
					spaces.Add(_friendly.GetMeleeTerrain());
				return spaces;
			}
			/// <summary>
			/// Return Index of the Space that contains the character. Return -1 if no space found
			/// </summary>
			static int GetSpaceIndexOfChar(CombatCharacter cc)
			{
				List<CombatTerrain> cts = GetTerrains();
				for (int i = 0; i < cts.Count; i++)
				{
					if (cts[i].ContainsCharacter() && cc == cts[i].GetCharacter())
					{
						return i;
					}
				}
				return -1;
			}
			public static CombatTerrain GetAdjacentSpace(CombatCharacter cc, bool right)
			{

				if (_hostile.HasCharacter(cc))
				{
					return _hostile.GetAdjacentSpace(cc,right);
				}
				else if (_friendly.HasCharacter(cc))
				{
					return _hostile.GetAdjacentSpace(cc, right);
				}
				return null;
			}
			public static Facing GetOrientation(CombatCharacter source, CombatCharacter target)
			{
				return GetOrientation(GetPosition(source), GetPosition(target));
			}
			public static Facing GetOrientation(CombatTerrain source, CombatTerrain target)
			{
				int sourcePos = -1;
				int targetPos = -1;
				List<CombatTerrain> cts = GetTerrains();
				for (int i = 0; i < cts.Count; i++)
				{
					if (!cts[i] == source)
						sourcePos = i;
					else if (cts[i] == target)
						targetPos = i;
				}
				return GetOrientation(sourcePos, targetPos);
			}
			public static Facing GetOrientation(int sourcePos, int targetPos)
			{

				if (sourcePos >= 0 && targetPos >= 0)
				{
					if (sourcePos > targetPos)
						return Facing.Left;
					else if (sourcePos < targetPos)
						return Facing.Right;
				}
				if (sourcePos >= 0)
					return Facing.Right;
				//Debug.LogError("Error getting Orientation. Returning Right.\n(sourcePos = "+sourcePos+", targetPos = "+targetPos+")");
				return Facing.Left;
			}
			public static int GetStandardizedDirection(CombatCharacter cc)
			{
				if (cc._alliance == CombatCharacter.Alliance.Enemy)
				{
					return _orientation == Facing.Left ? 1 : -1;
				}
				else
				{
					return _orientation == Facing.Left ? -1 : 1;
				}
			}
			public static int GetDirectionFromTo(CombatCharacter source, CombatCharacter target)
			{
				//LogTerrains();
				Facing f = GetOrientation(source, target);
				int dir = (f == Facing.Right ? 1 : -1);
				return dir;
			}
			/* Returns the character's position in Arena, from left to right. If error returns -1 */
			public static int GetPosition(CombatCharacter cc)
			{
				if (_friendly.HasCharacter(cc))
					return _friendly.GetSpaceIndexOfChar(cc);
				else if (_hostile.HasCharacter(cc))
					return _hostile.GetSpaceIndexOfChar(cc);
				Debug.LogError(cc.GetName()+" not found in Terrains");
				return -1;
			}
			public static int GetPosition(CombatSelectableObject combatSelectableObject)
			{
				if (combatSelectableObject.GetComponentInParent<CombatCharacter>())
					return GetPosition(combatSelectableObject.GetComponentInParent<CombatCharacter>());
				else if (combatSelectableObject.GetComponentInParent<CombatTerrain>())
					return GetPosition(combatSelectableObject.GetComponentInParent<CombatTerrain>());
				else
				{
					Debug.Log("Unable to find parent for " + combatSelectableObject.gameObject.name);
					return -1;
				}
			}
			public static int GetPosition(CombatTerrain combatTerrain)
			{
				List<CombatTerrain> cts = GetTerrains();
				for (int i = 0; i < cts.Count; i++)
				{
					if (cts[i] == combatTerrain)
					{
						return i;
					}
				}
				Debug.LogError("Terrain not found");
				return -1;
			}
			public static void RemoveObject(CombatCharacter cc)
			{
				foreach (CombatTerrain ct in GetTerrains())
				{
					if (ct.ContainsCharacter())
					{
						if (ct.GetCharacter() == cc)
							ct.Empty();
					}
				}

			}
			public static void ClearGhosts()
			{
				_friendly.ClearGhosts();
				_hostile.ClearGhosts();
			}
			public static List<CombatTerrain> GetTerrains()
			{
				List<CombatTerrain> cts = new List<CombatTerrain>();
				if (_orientation == Facing.Left)
				{
					cts.AddRange(_hostile._terrains);
					cts.AddRange(_friendly._terrains);
				}
				else
				{
					cts.AddRange(_friendly._terrains);
					cts.AddRange(_hostile._terrains);
				}
				return cts;
			}
			public static void LogTerrains()
			{
				int i = 0;
				foreach (CombatTerrain ct in _terrains)
				{
					Debug.Log("Terrain[" + i + "] - " + (ct.IsEmpty() ? "EMPTY -" : ct.GetCharacter().GetName() + " -"));
					i++;
				}
			}
		}
		public class Arena  //The area for each combat encounter
		{
			public List<CombatCharacter> _cChars { get; protected set; }  //List of Characters in the combat in turn order
			public List<CombatTerrain> _terrains;
			public Facing _facing;
			public Arena(List<CombatTerrain> terrains)
			{
				_cChars = new List<CombatCharacter>();
				_terrains = new List<CombatTerrain>();
				for (int t = 0; t < terrains.Count; t++)
				{
					CombatTerrain ct = terrains[t];
					_terrains.Add(ct);
					if (ct.ContainsCharacter())
						_cChars.Add(ct.GetCharacter());
				}
			}
			public bool HasCharacter(CombatCharacter cc)
			{
				foreach (CombatTerrain ct in _terrains)
					if (ct.ContainsCharacter() && ct.GetCharacter() == cc)
						return true;
				return false;
			}
			public void ClearGhosts()
			{
				foreach (CombatTerrain ct in _terrains)
					ct.ClearGhost();
			}
			public CombatTerrain GetMeleeTerrain()
			{
				foreach(CombatTerrain ct in _terrains)
				{
					if (ct.ContainsCharacter())
						return ct;
				}
				return null;
			}
			public CombatCharacter GetMeleeCharacter()
			{
				CombatTerrain ct = GetMeleeTerrain();
				if (ct)
					return ct.GetCharacter();
				return null;
			}
			public CombatTerrain GetLastOccupiedTerrain()
			{
				for(int i = _terrains.Count - 1; i >= 0; i--)
				{
					if (_terrains[i].ContainsCharacter())
						return _terrains[i];
				}
				return null;
			}
			public CombatCharacter GetLastCharacter()
			{
				CombatTerrain ct = GetLastOccupiedTerrain();
				if (ct)
					return ct.GetCharacter();
				return null;
			}

			public int GetSpaceIndexOfChar(CombatCharacter cc)
			{
				for (int i = 0; i < _terrains.Count; i++)
				{
					if (_terrains[i].ContainsCharacter() && cc == _terrains[i].GetCharacter())
					{
						return i;
					}
				}
				return -1;
			}
			public CombatTerrain GetAdjacentSpace(CombatCharacter cc, bool right)
			{

				int index = GetSpaceIndexOfChar(cc);

				if ((index == 0 && !right) || (index == _terrains.Count - 1 && right)) return null; //Return null if out of bounds
				return _terrains[index + (right ? 1 : -1)];
			}
		}
	}
}
