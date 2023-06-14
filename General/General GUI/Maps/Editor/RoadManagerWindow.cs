using UnityEditor;
using UnityEngine;

namespace RD
{
	public class RoadManagerWindow : EditorWindow
	{
		RoadManager _RM;
		RoadSystem.Road _editedRoad;
		bool _newRoadMode;
		bool _editRoadMode;
		[MenuItem("Tools/RoadManager")]
		static void OpenWindow()
		{
			RoadManagerWindow window = EditorWindow.GetWindow(typeof(RoadManagerWindow)) as RoadManagerWindow;

			window.Show();
		}

		private void OnGUI()
		{
			if (_RM == null)
				_RM = new RoadManager();
			GUILayout.BeginVertical();
			if(GUILayout.Button("New Road"))
			{
				_newRoadMode = true;
				_RM.NewRoad();
			}
			if (_newRoadMode)
			{
				if(GUILayout.Button("Save Road"))
				{
					_RM.SaveNewRoad();
				}
			}

			foreach(RoadSystem.Road road in _RM._roadSystem._roads)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(road.name, GUILayout.Width(200));
			
				if (GUILayout.Button("View"))
				{
					_RM._roadSystem.VisualizeRoad(road);
				}
				if (GUILayout.Button("Delete"))
				{
					_RM._roadSystem._roads.Remove(road);
					Repaint();
				}
				GUILayout.EndHorizontal();
			}



			if (GUILayout.Button("Visualize Roads"))
			{
				_RM.VisualizeAllRoads();
			}
			GUILayout.EndVertical();
		}
	}
}
