using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RD
{
	[CustomEditor(typeof(NameGenerator))]
	public class NameGeneratorEditor : UnityEditor.Editor
	{
		NameGenerator _base;
		public override void OnInspectorGUI()
		{
			_base = target as NameGenerator;
			DrawDefaultInspector();
			//DrawClassFields(_base);
			if(GUILayout.Button("Generate Names"))
			{
				List<string> names = _base.GenerateNames();
				CreateTextFile(names);
			}
		}
		public void CreateTextFile(List<string> names)
		{
			if (names.Count > 0)
			{

				string path = Application.dataPath +"/"+ _base.name + ".txt";
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				using (StreamWriter sw = File.CreateText(path))
				{
					foreach (String _name in names)
					{
						sw.WriteLine(_name);
					}
				}
				Debug.Log("Created txt file at " + path);
			}
			EditorUtility.SetDirty(_base);
			AssetDatabase.SaveAssets();
		}
	}
}
