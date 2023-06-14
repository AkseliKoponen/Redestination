using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;


namespace RD.DB
{
	[CustomEditor(typeof(Hyperlink))]
	public class HyperlinkEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			Hyperlink _base = target as Hyperlink;
			DrawClassFields(_base, 0, false, true);
			GUILayout.BeginHorizontal();
			if (_base.GetDescriptionModified())
			{
				GUILayout.Space(100);
				GUILayout.Label("Description Saved");
			}
			else if (GUILayout.Button("Autolink Description"))
			{
				_base.UpdateDescription();
				AutoRename(_base);
			}
			if (GUILayout.Button("Find References"))
			{
				FindReferences(_base);
			}
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Save"))
			{
				AutoRename(_base);
			}
		}
		protected override void Dirtify()
		{
			Hyperlink _base = target as Hyperlink;
			EditorUtility.SetDirty(_base);
		}
	}
}