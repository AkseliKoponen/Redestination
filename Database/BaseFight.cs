using System;
using System.Collections.Generic;
using RD.Combat;
using RD;
using UnityEngine;

namespace RD.DB
{
	public class BaseFight : BaseObject
	{
		public Arena _hostileArena;
		public Arena _friendlyArena;
		public Facing _enemySide = Facing.Right;
		[Serializable]
		public class Arena
		{
			public List<CombatSpace> _combatSpaces;
			[Serializable]
			public class CombatSpace
			{
				public bool _empty;
				//public int _playerSpawnImportance; //1 = Atris, 2 = Willon and less important characters and so forth
				//public CombatCharacter _occupant;
				public BaseCombatCharacter _occupant;
				public Combat.Facing _facing;
				public Aura _aura;
				public Combat.CombatCharacter.Alliance _characterType;
				public CombatSpace()
				{
					_empty = false;
					//_playerSpawnImportance = 0;
					_facing = Combat.Facing.Right;
					_characterType = Combat.CombatCharacter.Alliance.Enemy;
				}
				public void Tick()
				{
				}
				public void Empty()
				{
					_occupant = null;
					//_playerSpawnImportance = 0;
				}
			}

			public Arena(int spaceCount = 4)
			{
				_combatSpaces = new List<CombatSpace>();
				for(int i = 0;i<spaceCount;i++)
					_combatSpaces.Add(new CombatSpace());
				//_combatSpaces[0]._playerSpawnImportance = 1;
				//_combatSpaces[0]._empty = spaceCount>=4;
			}
			public void EmptyAll()
			{
				foreach (CombatSpace cs in _combatSpaces)
					cs.Empty();
			}
			public void FillAll()
			{
				foreach (CombatSpace cs in _combatSpaces)
					cs._empty = false;
			}
			public void Flip()
			{
				_combatSpaces.Reverse();
			}
		}
		public int _victoryConditionID = 0;
		public int _defeatConditionID = 0;
		//Conditionals for more enemies or stuff?
		public void ChangeSides()
		{
			_hostileArena.Flip();
			_friendlyArena.Flip();
		}
		public BaseFight()
		{
			_hostileArena = new Arena();
			_hostileArena.FillAll();
			_friendlyArena = new Arena(2);
			_friendlyArena.EmptyAll();
			_layOutSpace = new LayOutSpace(new List<int> { 1 }, new List<string> { "General" });
		}
		public float _levelEstimate { get; private set; } = 0;
		public void EstimateLevel()
		{
			float f = 0;
			int count = -1;
			foreach(Arena.CombatSpace cs in _hostileArena._combatSpaces)
			{
				if (cs._occupant)
				{
					f += cs._occupant._level;
					count++;
				}
			}
			f *= (1 + (count * 0.1f));
			_levelEstimate = f;
		}
	}
	public static class VictoryCondition
	{
		public static bool CheckVictoryCondition(int id)
		{
			bool conditionsMet = false;
			switch (id)
			{
				case 0:
					//All enemies dead
					foreach(Combat.CombatCharacter cc in CodeTools._combatManager._combatCharacters)
					{
						if (cc == null) continue;
						if(cc._alliance == Combat.CombatCharacter.Alliance.Enemy && !cc._isDead)
						{
							return false;
						}
					}
					return true;
			}
			return conditionsMet;
		}
	}
	public static class DefeatCondition
	{
		public static bool CheckDefeatCondition(int id)
		{
			bool conditionsMet = false;
			switch (id)
			{
				case 0:
					//All playerCharacters dead
					foreach (Combat.CombatCharacter cc in CodeTools._combatManager._combatCharacters)
					{
						if (cc == null) continue;
						if (cc._alliance == Combat.CombatCharacter.Alliance.Player && !cc._isDead)
						{
							return false;
						}
					}
					return true;
			}
			return conditionsMet;
		}
	}
}