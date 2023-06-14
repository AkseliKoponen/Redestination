using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(BaseEffect))]
	public class BaseEffectEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseEffect _base = target as BaseEffect;
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
			BaseEffect _base = target as BaseEffect;
			EditorUtility.SetDirty(_base);
		}
	}
}
