using System.Reflection;
using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;
using RD.Combat;

namespace RD.DB
{
	[CustomEditor(typeof(BaseFight))]
	public class BaseFightEditor : BaseEditor
	{

		public override void OnInspectorGUI()
		{
			BaseFight _base = target as BaseFight;
			#region Draw Arenas in right order
			EditorGUILayout.BeginHorizontal();
			int margin = 100;
			if (_base._enemySide == Facing.Left)
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(margin));
				EditorGUILayout.BeginFoldoutHeaderGroup(true, "Hostile Arena");
				_base._hostileArena = DrawArena(_base._hostileArena,margin);
				EditorGUILayout.EndFoldoutHeaderGroup();
				EditorGUILayout.EndVertical();
				GUILayout.Space(100);
				EditorGUILayout.BeginVertical(GUILayout.Width(margin));
				EditorGUILayout.BeginFoldoutHeaderGroup(true, "Friendly Arena");
				_base._friendlyArena = DrawArena(_base._friendlyArena, margin);
				EditorGUILayout.EndFoldoutHeaderGroup();
				EditorGUILayout.EndVertical();
			}
			else
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(margin));
				EditorGUILayout.BeginFoldoutHeaderGroup(true, "Friendly Arena");
				_base._friendlyArena = DrawArena(_base._friendlyArena, margin);
				EditorGUILayout.EndFoldoutHeaderGroup();
				EditorGUILayout.EndVertical();
				GUILayout.Space(100);
				EditorGUILayout.BeginVertical(GUILayout.Width(margin));
				EditorGUILayout.BeginFoldoutHeaderGroup(true, "Hostile Arena");
				_base._hostileArena = DrawArena(_base._hostileArena, margin);
				EditorGUILayout.EndFoldoutHeaderGroup();
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();
			#endregion
			DrawClassFields(_base, 0, false, true);
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Difficulty level: " + _base._levelEstimate);
			if (GUILayout.Button("Save (and estimate)"))
			{
				_base.EstimateLevel();
				AutoRename(_base);
			}
			EditorGUILayout.EndVertical();
		}
		protected override void Dirtify()
		{
			BaseFight _base = target as BaseFight;
			EditorUtility.SetDirty(_base);
		}
		static BaseFight.Arena DrawArena(BaseFight.Arena arena, int margin)
		{
			bool dividedFieldMode = EditorGuiTools._simpleMove;
			//BaseFight.Arena arena = (BaseFight.Arena)field.GetValue(_class);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(margin));

			/*if (dividedFieldMode)arena._enemySide = (Combat.Facing)EditorGUILayout.EnumPopup(arena._enemySide, GUILayout.Width(50));
			else */EditorGUILayout.LabelField("", GUILayout.Width(margin));
			EditorGUILayout.LabelField("Name", GUILayout.Width(margin));
			EditorGUILayout.LabelField("Empty", GUILayout.Width(margin));
			if(!dividedFieldMode)EditorGUILayout.LabelField("Facing", GUILayout.Width(margin));
			//EditorGUILayout.LabelField("CharacterSpawn", GUILayout.Width(margin));
			EditorGUILayout.LabelField("Occupant", GUILayout.Width(margin));
			EditorGUILayout.LabelField("", GUILayout.Width(margin));
			EditorGUILayout.LabelField("", GUILayout.Width(margin));
			EditorGUILayout.EndVertical();
			for (int i = 0; i < arena._combatSpaces.Count; i++)
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(margin));
				EditorGUILayout.BeginHorizontal();
				BaseFight.Arena.CombatSpace comSpa = arena._combatSpaces[i];
				if (i > 0)
				{
					if (GUILayout.Button("<-", GUILayout.Width(27)))
					{
						arena._combatSpaces[i] = arena._combatSpaces[i - 1];
						arena._combatSpaces[i - 1] = comSpa;
						break;
						//Move left
					}
				}
				else
					EditorGUILayout.LabelField("", GUILayout.Width(27));
				if (GUILayout.Button("delete", GUILayout.Width(46)))
				{
					arena._combatSpaces.RemoveAt(i);
					i--;
					continue;
				}
				if (i < arena._combatSpaces.Count - 1)
					if (GUILayout.Button("->", GUILayout.Width(27)))
					{
						arena._combatSpaces[i] = arena._combatSpaces[i + 1];
						arena._combatSpaces[i + 1] = comSpa;
						break;
						//Move Right
					}
				EditorGUILayout.EndHorizontal();
				bool full = (comSpa._occupant != null) && !comSpa._empty;

				if (full)
				{
					EditorGUILayout.LabelField(comSpa._occupant!=null?comSpa._occupant._nameAbbreviation:"_BASE!", GUILayout.Width(100));
				}
				else
					EditorGUILayout.LabelField("Empty", GUILayout.Width(margin));
				comSpa._empty = EditorGUILayout.Toggle(comSpa._empty);
				if (!dividedFieldMode)
				{
					if (comSpa._occupant != null)
					{
						EditorGUILayout.BeginHorizontal();
						comSpa._facing = (Combat.Facing)EditorGUILayout.EnumPopup(comSpa._facing, GUILayout.Width(50));
						EditorGUILayout.LabelField(comSpa._facing == Combat.Facing.Left ? "<<" : ">>", GUILayout.Width(30));
						EditorGUILayout.EndHorizontal();
					}
					else
						EditorGUILayout.LabelField("", GUILayout.Width(100));
				}
				else
				{
					//comSpa._facing = Combat.CombatCharacter.GetReverseFacing(arena._enemySide);
				}
				if (comSpa._empty)
					comSpa.Empty();
				bool disabled = comSpa._empty;
				if (disabled) EditorGUI.BeginDisabledGroup(true);
				//comSpa._playerSpawnImportance = EditorGUILayout.IntField(comSpa._playerSpawnImportance, GUILayout.Width(30));
				//comSpa._occupant = (Combat.CombatCharacter)EditorGUILayout.ObjectField(comSpa._occupant, typeof(Combat.CombatCharacter), false, GUILayout.Width(100));
				comSpa._occupant = (BaseCombatCharacter)EditorGUILayout.ObjectField(comSpa._occupant, typeof(BaseCombatCharacter), false, GUILayout.Width(100));
				if (comSpa._occupant && comSpa._occupant._visualObject._turnIcon != null)
				{
					GUILayout.Box(CodeTools.textureFromSprite(comSpa._occupant._visualObject._turnIcon), GUILayout.Width(62), GUILayout.Height(18));
				}
				if (disabled) EditorGUI.EndDisabledGroup();
				EditorGUILayout.EndVertical();
				EditorGUILayout.LabelField("", GUILayout.Width(20));

				arena._combatSpaces[i] = comSpa;
			}
			if (GUILayout.Button("Add", GUILayout.Width(50)))
			{
				arena._combatSpaces.Add(new BaseFight.Arena.CombatSpace());
			}
			
			EditorGUILayout.EndHorizontal();
			return arena;
		}
	}
}
