using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using RD.DB;

namespace RD.UI
{
	[CustomEditor(typeof(TalentTree))]
	public class TalentTreeEditor: Editor
	{
		public override void OnInspectorGUI()
		{
			TalentTree tt = target as TalentTree;
			GUILayout.BeginHorizontal();
			GUILayout.Label("Length:", GUILayout.Width(50));
			tt._length = EditorGUILayout.IntField(tt._length,GUILayout.Width(50));
			if (GUILayout.Button("Save",GUILayout.Width(100)))
			{
				Save();
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			GUILayout.Label("Level");
			GUILayout.Label("2");
			GUILayout.Label("4");
			GUILayout.Label("6");
			GUILayout.Label("8");
			GUILayout.EndVertical();
			foreach(FieldInfo fif in tt.GetType().GetFields())
				if (fif.FieldType == typeof(List<BaseTalent>))
					DrawColumn(fif);
			GUILayout.EndHorizontal();

			void DrawColumn(FieldInfo fif)
			{
				GUILayout.Space(20);
				List<BaseTalent> column = (List<BaseTalent>)fif.GetValue(tt);
				while (column.Count < tt._length)
				{
					column.Add(null);
				}
				GUILayout.BeginVertical();
				EditorGUILayout.LabelField(fif.Name, GUILayout.Width(120));
				for(int i = 0; i < column.Count; i++)
				{
					if (fif.Name.Equals("_column3")) {
						EditorGUILayout.LabelField("General "+(i+1));
					}
					else
					column[i] = (BaseTalent)EditorGUILayout.ObjectField(column[i], typeof(BaseTalent), GUILayout.Width(120));
				}
				GUILayout.EndVertical();
				fif.SetValue(tt,column);
			}
			void Save()
			{
				BaseCollection db = Resources.Load<BaseCollection>("Database/Collection");
				tt._column3 = new List<BaseTalent>();
				foreach(int id in new List<int> { 2, 3, 4, 5 })
				{
					tt._column3.Add((BaseTalent)db.Get<BaseTalent>(id));
				}
				EditorUtility.SetDirty(tt);
				AssetDatabase.SaveAssets();
			}
		}
	}
}
