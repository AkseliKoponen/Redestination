using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(BaseEquipment))]
	public class BaseEquipmentEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseEquipment _base = target as BaseEquipment;
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
			BaseEquipment _base = target as BaseEquipment;
			EditorUtility.SetDirty(_base);
		}

	}
}
