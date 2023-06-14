using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(BaseItem))]
	public class BaseItemEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseItem _base = target as BaseItem;
			DrawClassFields(_base,0,false,true);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Find References", GUILayout.Width(300)))
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
			BaseItem _base = target as BaseItem;
			EditorUtility.SetDirty(_base);
		}

	}
}
