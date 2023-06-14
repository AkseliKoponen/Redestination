using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace RD
{
	[CustomEditor(typeof(ElvenLanguageTools))]
	public class ElvenLanguageEditor: Editor
	{
		public string numberText="";
		public TextAsset csv = null;
		public override void OnInspectorGUI()
		{
			ElvenLanguageTools dictionary = target as ElvenLanguageTools;
			GUILayout.BeginHorizontal();
			GUILayout.Label("Translate number");
			numberText = GUILayout.TextField(numberText);
			GUILayout.EndHorizontal();
			if(GUILayout.Button("Translate number"))
			{
				dictionary.TranslateNumber(numberText);
			}
			GUILayout.Space(20);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Import .csv");
			csv = (TextAsset)EditorGUILayout.ObjectField(csv, typeof(TextAsset));
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Import") && csv!=null)
			{
				dictionary.ImportTranslation(csv);
				EditorUtility.SetDirty(dictionary);
			}
			GUILayout.Space(20);
			DrawDefaultInspector();
		}

	}
}
