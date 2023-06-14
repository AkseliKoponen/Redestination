using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(BaseFX))]
	public class BaseFXEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseFX _base = target as BaseFX;
			DrawClassFields(_base, 0, false, true);
			if (GUILayout.Button("Save"))
			{
				AutoRename(_base);
			}
		}
		protected override void Dirtify()
		{
			BaseFX _base = target as BaseFX;
			EditorUtility.SetDirty(_base);
		}
	}
}
