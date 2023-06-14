using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(SceneConstructor))]
	public class SceneConstructorEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			SceneConstructor _base = target as SceneConstructor;
			DrawClassFields(_base, 0, false, true);
			if (GUILayout.Button("Save"))
			{
				AutoRename(_base);
			}
		}
		protected override void Dirtify()
		{
			SceneConstructor _base = target as SceneConstructor;
			EditorUtility.SetDirty(_base);
		}
	}
}
