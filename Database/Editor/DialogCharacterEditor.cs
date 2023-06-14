using RD;
using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(DialogCharacter))]
	public class DialogCharacterEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			DialogCharacter _base = target as DialogCharacter;
			if (_base._useFactionColor)
				_base._color = _base.GetFactionColor();
			DrawClassFields(_base, 0, false, true);
			Preview(_base);
			if (GUILayout.Button("Save"))
			{
				AutoRename(_base);
			}
		}
		void Preview(DialogCharacter dc)
		{
			GUILayout.BeginHorizontal();
			if (dc._portrait != null)
				GUILayout.Box(CodeTools.textureFromSprite(dc._portrait), GUILayout.Width(100), GUILayout.Height(100));
			GUIStyle textStyle = new GUIStyle();
			textStyle.fontStyle = FontStyle.Bold;
			textStyle.normal.textColor = dc._color;
			Texture2D tex = new Texture2D(2, 2);
			tex.SetTextureColor(new Color(0.2f, 0.2f, 0.2f));
			textStyle.fontSize = 20;
			GUILayout.Label(dc.GetCharacterRaw(), textStyle);
			GUILayout.EndHorizontal();
		}
		protected override void Dirtify()
		{
			DialogCharacter _base = target as DialogCharacter;
			EditorUtility.SetDirty(_base);
		}
	}
}
