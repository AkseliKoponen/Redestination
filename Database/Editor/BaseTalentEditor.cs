using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(BaseTalent))]
	public class BaseTalentEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseTalent _base = target as BaseTalent;
			DrawClassFields(_base,0,false,true);
			GUILayout.BeginHorizontal();
			if (_base.GetDescriptionModified())
			{
				GUILayout.Space(100);
				GUILayout.Label("Description Saved");
			}
			GUILayout.EndHorizontal();
			

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Generate Description"))
				_base.GenerateDescription();
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
			BaseTalent _base = target as BaseTalent;
			EditorUtility.SetDirty(_base);
		}

	}
}
