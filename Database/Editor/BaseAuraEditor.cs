using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(BaseAura))]
	public class BaseAuraEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseAura _base = target as BaseAura;
			DrawClassFields(_base, 0, false, true);
			if (GUILayout.Button("Save"))
			{
				AutoRename(_base);
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Generate Description"))
			{
				_base.GenerateDescription();
				Repaint();
			}
			if (GUILayout.Button("Find References"))
			{
				FindReferences(_base);
			}
			GUILayout.EndHorizontal();
		}
		protected override void Dirtify()
		{
			BaseAura _base = target as BaseAura;
			EditorUtility.SetDirty(_base);
		}
	}
}
