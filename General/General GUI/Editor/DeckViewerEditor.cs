using UnityEditor;

namespace RD
{
	[CustomEditor(typeof(DeckViewer))]
	public class DeckViewerEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			DeckViewer _base = target as DeckViewer;
			DrawDefaultInspector();
			_base.Refresh(true);
		}
	}
}
