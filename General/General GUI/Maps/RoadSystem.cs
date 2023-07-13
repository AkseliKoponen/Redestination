using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using static RD.UI.RoadSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
using RD;
namespace RD.UI
{
	public class RoadSystem : MonoBehaviour
	{
		#region MapNodeConnection
		[Serializable]
		public class MapNodeConnection
		{
			public MapNode _nodeTo;
			public MapNode _nodeFrom;
			public List<Vector3> _road = new List<Vector3>();
			public List<Road> _roadPoints = new List<Road>();
			public MapNodeConnection(MapNodeConnection mnc)
			{
				_nodeTo = mnc._nodeTo;
				_nodeFrom = mnc._nodeFrom;
				_road = new List<Vector3>();
				_road.AddRange(mnc._road);
			}
			public MapNodeConnection(MapNode from, MapNode to)
			{
				_road = new List<Vector3>
			{
				from.transform.position,
				to.transform.position
			};
				_nodeTo = to;
				_nodeFrom = from;
			}
			public string GetName()
			{
				return _nodeFrom.gameObject.name + "---------" + _nodeTo.gameObject.name;
			}
			public void Reverse()
			{
				(_nodeFrom, _nodeTo) = (_nodeTo, _nodeFrom);
				_road.Reverse();
				_roadPoints.Reverse();
			}
			public static int CompareByName(MapNodeConnection mnc, MapNodeConnection mnc1)
			{
				return string.Compare(mnc.GetName(), mnc1.GetName(), StringComparison.Ordinal);
			}
		}
		#endregion
		List<MapNode> _mapNodes;
		[NonSerialized] public MapNode _currentNode;
		public Canvas _canvas;
		//public float _canvasScale;
		public Flagpole _playerFlag;
		public float _footStepsSpeed = 50;
		public GameObject _footstepsPrefab;
		public Road _roadPrefab;
		public Transform _footStepsParent;
		public Transform _roadParent;
		public GameObject _XPrefab;
		private void Awake()
		{
			_current = this;
			//_playerPosition = _connections[UnityEngine.Random.Range(0, _connections.Count - 1)]._nodeFrom;
			_currentNode = _connections[0]._nodeFrom;
			_playerFlag.transform.position = _currentNode.transform.position;
			_mapNodes = FindObjectsOfType<MapNode>().ToList();
			
		}
		private void Start()
		{
			HighlightConnections();
		}
		GameObject _XObject;
		public void TravelTo(MapNode pos)
		{
			if (_XObject)Destroy(_XObject);

			LowlightMapNodes();
			//pos.Highlight();
			//pos._locked = true;
			VisualizeRoad(_currentNode, pos);
			_currentNode = pos;
			_XObject = Instantiate(_XPrefab,transform);
			_XObject.transform.position = pos.transform.position;
			_XObject.transform.SetSiblingIndex(_playerFlag.transform.GetSiblingIndex());
			_XObject.GetComponent<Animator>().Play("Draw");
		}

		Task _footStepsAnimation;
		void VisualizeRoad(MapNode from, MapNode to)
		{
			Debug.Log("<color=teal>Visualizing road from " + from.gameObject.name + " to " + to.gameObject.name+"</color>");
			for(int i = _footStepsParent.childCount - 1; i >= 0; i--)
			{
				Destroy(_footStepsParent.GetChild(i).gameObject);
			}
			_playerFlag.transform.position = from.transform.position;
			MapNodeConnection mnc = GetConnection(from, to);
			if (mnc._nodeFrom != from)
				mnc.Reverse();
			List<RectTransform> footsteps = new List<RectTransform>();
			for(int i = 0; i < mnc._roadPoints.Count-1; i++)
			{
				Vector3 start = mnc._roadPoints[i].GetComponent<RectTransform>().localPosition;
				Vector3 end = mnc._roadPoints[i+1].GetComponent<RectTransform>().localPosition;
				GameObject gor = GameObject.Instantiate(_footstepsPrefab);
				gor.transform.SetParent(_footStepsParent.transform);
				gor.name = mnc.GetName()+">"+i;
				RectTransform grt = gor.GetComponent<RectTransform>();
				grt.localPosition = start;
				grt.localScale = Vector3.one;
				float length = Vector2.Distance(start, end);
				grt.pivot = new Vector2(0, 0.5f);
				float imagewidth = _footstepsPrefab.GetComponent<RectTransform>().sizeDelta.x;
				length = Mathf.RoundToInt(length / imagewidth);
				length *= imagewidth;
				grt.sizeDelta = new Vector2(length, grt.sizeDelta.y);
				float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * 180 / Mathf.PI;
				grt.eulerAngles = new Vector3(0, 0, angle);
				grt.anchoredPosition = start;
				grt.localPosition = new Vector3(grt.localPosition.x, grt.localPosition.y, 0);
				footsteps.Add(grt);
			}
			_footStepsAnimation = new Task(AnimateSteps(footsteps));

			IEnumerator AnimateSteps(List<RectTransform> steps, bool growWidth = true)
			{
				List<int> lengths = new List<int>();
				float imagewidth = growWidth ? _footstepsPrefab.GetComponent<RectTransform>().sizeDelta.x : _footstepsPrefab.GetComponent<RectTransform>().sizeDelta.y;
				float timePerStep = imagewidth/_footStepsSpeed;
				//Add the original widths to _widths and set them to 0
				foreach (RectTransform rt in steps)
				{
					if (growWidth)
					{
						lengths.Add(Mathf.RoundToInt(rt.sizeDelta.x));
						rt.sizeDelta = new Vector2(0, rt.sizeDelta.y);
					}
					else
					{
						lengths.Add(Mathf.RoundToInt(rt.sizeDelta.y));
						rt.sizeDelta = new Vector2(rt.sizeDelta.x, 0);
					}
				}
				Lerper lper = null;
				lengths[lengths.Count-1] -= Mathf.RoundToInt(imagewidth);//Remove the last step
				for (int i = 0; i < steps.Count; i++)
				{
					RectTransform step = steps[i];
					//Keep _playerFlag parent and sibling index
					step.GetComponent<Image>().enabled = true;
					float length = (growWidth ? step.sizeDelta.x : step.sizeDelta.y);
					if (i == 0)
						length += imagewidth;
					SetLength(length,step);
					float timer = Time.time;
					while (length < lengths[i])
					{
						length = Mathf.Clamp(length + CodeTools.Tm.GetUIDelta() * _footStepsSpeed, 0, lengths[i]);
						bool b = SetLength(Mathf.RoundToInt(length / imagewidth) * imagewidth, step);
						if (!b)
						{
							if (lper != null) lper.End();
							lper = Lerp.PositionUI(_playerFlag.GetComponent<RectTransform>(), CodeTools.GetPositionInLocalSpace(_playerFlag.transform, step.GetChild(0).transform), timePerStep / 3f);
						}
						yield return null;
					}
					//while (lper != null)yield return null;
				}
				if (lper != null) lper.Stop();
				Lerp.PositionUI(_playerFlag.GetComponent<RectTransform>(),CodeTools.GetPositionInLocalSpace(_playerFlag.transform,_currentNode.transform), timePerStep / 2f);
				float t = timePerStep;
				while (t > 0)
				{
					t -= CodeTools.Tm.GetUIDelta();
					yield return null;
				}
				LowlightMapNodes();
				HighlightConnections();
				if (_XObject)Destroy(_XObject);

				bool SetLength(float length, RectTransform step)
				{
					Vector2 v = step.sizeDelta;
					Vector2 temp = v;
					if (growWidth) v.x =length;
					else v.y = length;
					step.sizeDelta = v;
					return v == temp;
				}
			}
		}
		void LowlightMapNodes()
		{
			foreach (MapNode mn in _mapNodes)
				mn.Lowlight();
		}
		public void HighlightConnections()
		{
			List<MapNode> nodes = GetNodesTo(_currentNode);
			foreach(MapNode mn in nodes)
			{
				mn.Highlight();
			}
		}
		public List<MapNodeConnection> GetConnectionsTo(MapNode from)
		{
			List<MapNodeConnection> conns = new List<MapNodeConnection>();
			foreach (MapNodeConnection mnc in _connections)
			{
				if (mnc._nodeFrom == from || mnc._nodeTo == from)
					conns.Add(mnc);
			}
			return conns;
		}
		public List<MapNode> GetNodesTo(MapNode from)
		{
			List<MapNode> nodes = new List<MapNode>();
			foreach(MapNodeConnection mnc in _connections)
			{
				if (mnc._nodeFrom == from)
					nodes.Add(mnc._nodeTo);
				else if (mnc._nodeTo == from)
				{
					nodes.Add(mnc._nodeFrom);
				}
			}
			return nodes;
		}
		public MapNodeConnection GetConnection(MapNode from, MapNode to)
		{
			foreach (MapNodeConnection mnc in _connections)
			{
				if ((mnc._nodeFrom == from && mnc._nodeTo == to) || (mnc._nodeTo == from && mnc._nodeFrom == to))
					return mnc;
			}
			return null;
		}
		public bool HasConnection(MapNode from, MapNode to)
		{
			foreach(MapNodeConnection mnc in _connections)
			{
				if ((mnc._nodeFrom == from && mnc._nodeTo == to) || (mnc._nodeTo == from && mnc._nodeFrom == to))
					return true;
			}
			return false;
		}
#if UNITY_EDITOR
		public bool _drawRoads = true;
		[NonSerialized]public int connectionHandle = -1;
		void OnDrawGizmosSelected()
		{
			if (_drawRoads)
			{
				if (connectionHandle < 0)
				{
					foreach (MapNodeConnection mnc in _connections)
					{
						DrawRoad(mnc);
					}
				}
				else
				{
					DrawRoad(_connections[connectionHandle]);

				}
			}
			else if (connectionHandle >= 0)
			{
				DrawRoad(_connections[connectionHandle]);
			}
			
		}
		void DrawRoad(MapNodeConnection mnc)
		{
			if (mnc != null && mnc._road != null)
			{
				Gizmos.color = Color.red;
				for (int i = 0; i < mnc._road.Count - 1; i++)
				{
					Gizmos.DrawLine(mnc._road[i], mnc._road[i+1]);
				}
			}
		}
		public void DrawMapNode(MapNode mn)
		{
			foreach(MapNodeConnection mnc in GetConnectionsTo(mn))
			{
				DrawRoad(mnc);
			}
		}
#endif
		public static RoadSystem _current;
		public List<MapNodeConnection> _connections;
	}
	#region Editor
#if UNITY_EDITOR
	[CustomEditor(typeof(RoadSystem))]
	public class RoadSystemEditor : Editor
	{
		int connectionHandle = -1;
		int roadHandle = -1;
		RoadSystem rs;
		bool editOpen;
		MapNode _nodeFrom;
		MapNode _nodeTo;
		List<bool> connectionFolds = new List<bool>();
		bool _defaultInspector = false;
		public override void OnInspectorGUI()
		{
			rs = target as RoadSystem;

			if (GUILayout.Button((_defaultInspector ? "Hide " : "Show ") + "Default Inspector"))
				_defaultInspector = !_defaultInspector;
			if (_defaultInspector)
			{
				DrawDefaultInspector();
				GUILayout.Label("");
			}
			if (Application.isPlaying)
				return;
			//if (rs._canvas != null)rs._canvasScale = rs._canvas.transform.localScale.x;
			rs.connectionHandle = connectionHandle;
			/* //Sorting
			if(GUILayout.Button("Sort Connections"))
			{
				foreach(MapNodeConnection mnc in rs._connections)
					if(string.Compare(mnc._nodeFrom.gameObject.name, mnc._nodeTo.gameObject.name, StringComparison.Ordinal) > 0)
						mnc.Reverse();
				rs._connections.Sort(MapNodeConnection.CompareByName);
			}
			*/
			while (rs._connections.Count > connectionFolds.Count)
				connectionFolds.Add(false);
			if (rs._connections.Count > 0)
			{
				if (GUILayout.Button(editOpen==false?"Edit Connections":"Stop editing connections"))
					editOpen = !editOpen;
			}
			else
				editOpen = false;
			if (editOpen)
			{
				GUILayout.BeginVertical();
				if (rs._connections.Count > 0)
				{
					for (int y = 0; y < rs._connections.Count; y++)
					{
						RoadSystem.MapNodeConnection mnc = rs._connections[y];
						connectionFolds[y] = EditorGUILayout.BeginFoldoutHeaderGroup(connectionFolds[y], mnc.GetName());
						if (connectionFolds[y])
						{
							if (mnc._road == null || mnc._road.Count < 2)
							{
								RoadSystem.MapNodeConnection temp = new RoadSystem.MapNodeConnection(mnc._nodeFrom, mnc._nodeTo);
								mnc._road = new List<Vector3>();
								mnc._road.AddRange(temp._road);
							}


							for (int i = 0; i < mnc._road.Count; i++)
							{

								GUILayout.BeginHorizontal();
								if (i == 0)
									EditorGUILayout.LabelField("Start: " + mnc._nodeFrom.gameObject.name);
								else if (i == mnc._road.Count - 1)
									EditorGUILayout.LabelField("End: " + mnc._nodeTo.gameObject.name);
								else
								{
									EditorGUILayout.LabelField("Point " + i + " " + mnc._road[i].ToString());
									if (connectionHandle == y && roadHandle == i)
									{
										if (GUILayout.Button("Stop Edit"))
										{
											connectionHandle = -1;
											roadHandle = -1;
											UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
										}

									}
									else if (GUILayout.Button("Edit"))
									{
										connectionHandle = y;
										roadHandle = i;
										UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
									}
								}
								if (i != 0 && i != mnc._road.Count - 1)
								{
									if (GUILayout.Button("-"))
									{
										mnc._road.RemoveAt(i);
										GUILayout.EndHorizontal();
										break;
									}
								}
								if (i != mnc._road.Count - 1 && GUILayout.Button("+"))
								{
									Vector3 temp = Vector3.Lerp(mnc._road[i], mnc._road[i + 1], 0.5f);
									mnc._road.Insert(i + 1, temp);
									roadHandle = i + 1;
									connectionHandle = y;
									GUILayout.EndHorizontal();
									break;
								}

								GUILayout.EndHorizontal();
							}
						}
						EditorGUILayout.EndFoldoutHeaderGroup();
					}
				}
				GUILayout.EndVertical();
			}
			if(!editOpen)
			{
				connectionHandle = -1;
				roadHandle = -1;
				GUILayout.Label("Add a connection");
				GUILayout.BeginHorizontal();
				GUILayout.Label("From"); _nodeFrom = (MapNode)EditorGUILayout.ObjectField(_nodeFrom, typeof(MapNode),true);
				GUILayout.Label("To"); _nodeTo = (MapNode)EditorGUILayout.ObjectField(_nodeTo, typeof(MapNode), true);
				GUILayout.EndHorizontal();
				if ((_nodeFrom != null && _nodeTo != null))
				{
					if (rs.HasConnection(_nodeFrom, _nodeTo))
					{
						GUILayout.Label("Connection Already Exists");
					}
					else if(GUILayout.Button("Add new Connection"))
					{
						rs._connections.Add(new RoadSystem.MapNodeConnection(_nodeFrom, _nodeTo));
						//_nodeFrom = null;
						_nodeTo = null;
						UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
					}
				}
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Overlap distance");
				overlapDistance= EditorGUILayout.FloatField(overlapDistance);
				GUILayout.EndHorizontal();
				if (GUILayout.Button("Create All New RoadPoints"))
				{
					List<Road> roads = new List<Road>();
					{
						List<Road> olds = FindObjectsOfType<Road>().ToList();
						int tempi = olds.Count;
						while (olds.Count > 0)
						{
							DestroyImmediate(olds[0].gameObject);
							olds.RemoveAt(0);
						}
						Debug.Log("Destroyed " + tempi + " old roadpoints");
					}
					foreach(MapNodeConnection mnc in rs._connections)
					{
						mnc._roadPoints = new List<Road>();
						if (mnc._road == null || mnc._road.Count == 0)
							continue;
						Road previousRoad = null;
						foreach(Vector3 pos in mnc._road)
						{
							Road r = Overlap(pos);
							if (r == null) {
								string name = pos == mnc._road[0] ? "Road from " + mnc._nodeFrom.gameObject.name
									: pos == mnc._road[mnc._road.Count - 1] ? "Road from " + mnc._nodeTo.gameObject.name
									:"Road between " + mnc._nodeFrom.gameObject.name +" and " + mnc._nodeTo.gameObject.name;
								r = CreateRoad(name, pos);
							}
							else if(r.gameObject.name.StartsWith("Road between"))
							{
								if (r.gameObject.name.Contains(mnc._nodeFrom.gameObject.name) == false)
									r.gameObject.name = r.gameObject.name + " and " + mnc._nodeFrom.gameObject.name;
								if (r.gameObject.name.Contains(mnc._nodeTo.gameObject.name) == false)
									r.gameObject.name = r.gameObject.name + " and " + mnc._nodeTo.gameObject.name;
							}
							if (previousRoad!=null)
							{
								previousRoad._connections.Add(r);
								r._connections.Add(previousRoad);
							}
							previousRoad = r;
							mnc._roadPoints.Add(r);
						}
					}
					void CreateRoadForConnection(MapNodeConnection mnc)
					{
						mnc._roadPoints = new List<Road>();
						if (mnc._road == null || mnc._road.Count == 0)
							return;
						Road previousRoad = null;
						foreach (Vector3 pos in mnc._road)
						{
							Road r = Overlap(pos);
							if (r == null)
							{
								string name = pos == mnc._road[0] ? "Road from " + mnc._nodeFrom.gameObject.name
									: pos == mnc._road[mnc._road.Count - 1] ? "Road from " + mnc._nodeTo.gameObject.name
									: "Road between " + mnc._nodeFrom.gameObject.name + " and " + mnc._nodeTo.gameObject.name;
								r = CreateRoad(name, pos);
							}
							else if (r.gameObject.name.StartsWith("Road between"))
							{
								if (r.gameObject.name.Contains(mnc._nodeFrom.gameObject.name) == false)
									r.gameObject.name = r.gameObject.name + " and " + mnc._nodeFrom.gameObject.name;
								if (r.gameObject.name.Contains(mnc._nodeTo.gameObject.name) == false)
									r.gameObject.name = r.gameObject.name + " and " + mnc._nodeTo.gameObject.name;
							}
							if (previousRoad != null)
							{
								previousRoad._connections.Add(r);
								r._connections.Add(previousRoad);
							}
							previousRoad = r;
							mnc._roadPoints.Add(r);
						}
					}
					Debug.Log("<color=green>Created a total of "+roads.Count+" points for "+rs._connections.Count+" connections</color>");
					Road Overlap(Vector3 pos)
					{
						foreach(Road r in roads)
						{
							if(Vector3.Distance(r.transform.position, pos) < overlapDistance)
							{
								return r;
							}
						}
						return null;
					}
					Road CreateRoad(string name,Vector3 pos)
					{
						GameObject go = Instantiate(rs._roadPrefab.gameObject);
						go.transform.SetParent(rs._roadParent);
						go.transform.position = pos;
						go.name = name;
						Road r = go.GetComponent<Road>();
						roads.Add(r);
						return r;
					}
				}
			}
		}
		float overlapDistance = 10;
		public void OnSceneGUI()
		{
			if (rs != null && roadHandle > -1 && connectionHandle > -1)
			{
				Handles.color = Color.red;
				Vector3 pos = rs._connections[connectionHandle]._road[roadHandle];
				Handles.DrawSolidDisc(pos, Vector3.forward, 0.1f);
				rs._connections[connectionHandle]._road[roadHandle] = Handles.PositionHandle(pos, rs.transform.rotation);
				GUIStyle style = new GUIStyle();
				style.fontSize = 20;
				style.normal.textColor = Color.red;
				Handles.Label(pos, "Road point " + roadHandle + " from " + rs._connections[connectionHandle]._nodeFrom.gameObject.name +
					" to " + rs._connections[connectionHandle]._nodeTo.gameObject.name, style);
			}
		}
	}
#endif
	#endregion
}