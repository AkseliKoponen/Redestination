#if UNITY_EDITOR
using RD.Combat;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;
using static RD.DB.EditorGuiTools;
using System.Linq;
using System.Collections.Generic;
using RD.UI;
using UnityEditor.SceneManagement;

namespace RD.DB
{
	[CustomEditor(typeof(BaseThought))]
	public class BaseThoughtEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseThought _base = target as BaseThought;
			DrawClassFields(_base,0,false,true);
			GUILayout.BeginHorizontal();
			if (_base.GetDescriptionModified())
			{
				GUILayout.Space(100);
				GUILayout.Label("Description Saved");
			}
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Save"))
			{
				SaveCheck(_base);
				AutoRename(_base);
				ActiveEditorTracker.sharedTracker.isLocked = false;
				StageUtility.GoToMainStage();
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Generate Description"))
				_base.GenerateDescription();
			if(GUILayout.Button("Find References"))
			{
				FindReferences(_base);
			}
			GUILayout.EndHorizontal();
		}
		protected override void Dirtify()
		{
			BaseThought _base = target as BaseThought;
			EditorUtility.SetDirty(_base);
		}
		public static void Preview(BaseThought target)
		{
			ActiveEditorTracker.sharedTracker.isLocked = true;
			
			Type t = typeof(UnityEditor.Experimental.SceneManagement.PrefabStageUtility);
			//MethodInfo mi = t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).Single(m => m.Name == "OpenPrefab" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
			//UnityEditor.Experimental.SceneManagement.PrefabStage stage = (UnityEditor.Experimental.SceneManagement.PrefabStage)mi.Invoke(null, new object[] { "Assets/Prefabs/UI/Mind/Thought.prefab" });
			EditorSceneManager.OpenScene("Assets/Scenes/Editor/ThoughtPreview.unity");
			ThoughtUI tui = EditorSceneManager.GetActiveScene().GetRootGameObjects()[0].GetComponentInChildren<ThoughtUI>();
			tui.PreviewThought(target as BaseThought);
		}


		public void SaveCheck(BaseThought _base)
		{
			foreach (BaseThought.Behaviour et in _base._effects)
			{
				BaseCollection bc = null;
				if (et._effect == null)
				{
					Debug.LogError("Missing effect on Thought!");
					continue;
				}
				if (et._effect._id == 21 && (_base._requirements == null || _base._requirements.Count == 0))
				{
					if (bc == null)
					{
						bc = Resources.Load<BaseCollection>("Database/Collection");
					}
					_base._requirements.Add(new BaseThought.CharacterRequirement((BaseRequirement)bc.Get<BaseRequirement>(19),BaseThought.TargetType.Self));
				}


			}
		}

	}
}
#endif