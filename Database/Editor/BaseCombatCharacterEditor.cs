using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(BaseCombatCharacter))]
	public class BaseCombatCharacterEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseCombatCharacter _base = target as BaseCombatCharacter;
			DrawClassFields(_base, 0, false, true);
			if (GUILayout.Button("Save"))
			{
				AutoRename(_base);
			}
		}
		protected override void Dirtify()
		{
			BaseCombatCharacter _base = target as BaseCombatCharacter;
			EditorUtility.SetDirty(_base);
		}
	}
}
