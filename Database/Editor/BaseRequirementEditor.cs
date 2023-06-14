using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(BaseRequirement))]
	public class BaseRequirementEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseRequirement _base = target as BaseRequirement;
			DrawClassFields(_base, 0, false, true);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Save"))
			{
				AutoRename(_base);
			}
			if (GUILayout.Button("Find References"))
			{
				FindReferences(_base);
			}
			GUILayout.EndHorizontal();
		}
		protected override void Dirtify()
		{
			BaseRequirement _base = target as BaseRequirement;
			EditorUtility.SetDirty(_base);
		}
	}
}
