using RD.Combat;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;
using static RD.DB.EditorGuiTools;
using System.Linq;
using System.Collections.Generic;

namespace RD.DB
{
	[CustomEditor(typeof(BaseCard))]
	public class BaseCardEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseCard _base = target as BaseCard;
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
				_base.SetArtString();
				AutoRename(_base);
				ActiveEditorTracker.sharedTracker.isLocked = false;
				UnityEditor.SceneManagement.StageUtility.GoToMainStage();
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Generate Description"))
				_base.GenerateDescription();
			if(GUILayout.Button("Find References"))
			{
				FindReferences(_base);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Space(300);
			if (GUILayout.Button("Preview Art"))
				Preview();
			GUILayout.EndHorizontal();
		}
		protected override void Dirtify()
		{
			BaseCard _base = target as BaseCard;
			EditorUtility.SetDirty(_base);
		}
		public void Preview()
		{
			//GameObject go = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Combat/Card.prefab");
			ActiveEditorTracker.sharedTracker.isLocked = true;
			Type t = typeof(UnityEditor.Experimental.SceneManagement.PrefabStageUtility);
			MethodInfo mi = t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).Single(m => m.Name == "OpenPrefab" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
			UnityEditor.Experimental.SceneManagement.PrefabStage stage = (UnityEditor.Experimental.SceneManagement.PrefabStage)mi.Invoke(null, new object[] { "Assets/Prefabs/Combat/Card.prefab" });
			
			CombatGUICard cgui = stage.scene.GetRootGameObjects()[0].GetComponentInChildren<CombatGUICard>();
			BaseCard _base = target as BaseCard;
			cgui._parts._cardImage.sprite = _base._artSprite;
			SceneVisibilityManager.instance.ToggleVisibility(cgui.transform.GetChild(0).Find("CardBack").gameObject, true);
			if (_base._inkSplash > 0)
			{
				cgui._parts._inkSplash.sprite = Resources.Load<Sprite>("Card Art/Ink Splash " + Mathf.Clamp(_base._inkSplash, 1, BaseCard.GetInkSplashCount()).ToString());

			}
			else
			{
				cgui._parts._cardImage.GetComponent<Coffee.UIExtensions.Unmask>().enabled = false;
				cgui._parts._inkSplash.gameObject.SetActive(false);
			}
			cgui._parts._textName.text = _base.GetName();
		}


		public void SaveCheck(BaseCard _base)
		{
			foreach (BaseCard.EffectTarget et in _base._effects._effects)
			{
				BaseCollection bc = null;
				if (et._effect == null)
				{
					Debug.LogError("Missing effect on card!");
					continue;
				}
				if (et._effect._id == 21 && (_base._effects._requirements == null || _base._effects._requirements.Count == 0))
				{
					if (bc == null)
					{
						bc = Resources.Load<BaseCollection>("Database/Collection");
					}
					_base._effects._requirements.Add(new BaseCard.RequirementTarget((BaseRequirement)bc.Get<BaseRequirement>(19),BaseCard.TargetType.Self));
				}


			}
		}

	}
}
