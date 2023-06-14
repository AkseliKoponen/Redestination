using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(BaseConsumable))]
	public class BaseConsumableEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseConsumable _base = target as BaseConsumable;
			DrawClassFields(_base,0,false,true);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Find References"))
			{
				FindReferences(_base);
			}
			if (_base.GetDescriptionModified())
			{
				GUILayout.Space(100);
				GUILayout.Label("Description Saved");
			}
			else if (GUILayout.Button("Autolink Description"))
			{
				_base.UpdateDescription();
				Dirtify();
				AutoRename(_base);
			}
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Save"))
			{
				AutoRename(_base);
			}
		}
		protected override void Dirtify()
		{
			BaseConsumable _base = target as BaseConsumable;
			EditorUtility.SetDirty(_base);
		}

	}
}
