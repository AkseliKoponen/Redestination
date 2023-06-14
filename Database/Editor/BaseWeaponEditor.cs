using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(BaseWeapon))]
	public class BaseWeaponEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseWeapon _base = target as BaseWeapon;
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
			BaseWeapon _base = target as BaseWeapon;
			EditorUtility.SetDirty(_base);
		}

	}
}
