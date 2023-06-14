using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[CustomEditor(typeof(BaseFXAudio))]
	public class BaseFXAudioEditor : BaseEditor
	{
		public override void OnInspectorGUI()
		{
			BaseFXAudio _base = target as BaseFXAudio;
			DrawClassFields(_base, 0, false, true);
			if (GUILayout.Button("Save"))
			{
				AutoRename(_base);
			}
		}
		protected override void Dirtify()
		{
			BaseFXAudio _base = target as BaseFXAudio;
			EditorUtility.SetDirty(_base);
		}
	}
}
