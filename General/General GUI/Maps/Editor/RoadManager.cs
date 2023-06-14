using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RD
{
	public class RoadManager
	{
		public RoadSystem _roadSystem;
		public LineRenderer _currentLineRenderer;
		public GameObject _roadPrefab;
		public GameObject _roadMask;

		public RoadManager()
		{
			_roadSystem = (RoadSystem)GameObject.FindObjectOfType(typeof(RoadSystem));
			LoadRoadPrefabs();
			if (_roadSystem._roads == null)
				_roadSystem._roads = new List<RoadSystem.Road>();
			if (_roadSystem._mapNodes == null)
				_roadSystem._mapNodes = new List<MapNode>();
			_roadSystem.FindCanvas();
			DeleteOld();
		}
		void CreateLR()
		{

			GameObject go = new GameObject();
			go.transform.parent = _roadSystem.transform;
			_currentLineRenderer = go.AddComponent<LineRenderer>();
			Vector3 pos2 = _currentLineRenderer.GetPosition(1);
			pos2.x += 2;
			_currentLineRenderer.SetPosition(1, pos2);
			_currentLineRenderer.startWidth = 0.1f;
			_currentLineRenderer.endWidth = 0.1f;
			Selection.activeGameObject = go;
			_currentLineRenderer.material = null;
			_currentLineRenderer.gameObject.tag = "DeleteMeEditor";
		}
		public void NewRoad()
		{
			if (_currentLineRenderer != null)
				GameObject.DestroyImmediate(_currentLineRenderer.gameObject);
			CreateLR();
		}

		/*
	public void EditRoad(RoadSystem.Road road)
	{
		if (_currentLineRenderer != null)
			GameObject.DestroyImmediate(_currentLineRenderer.gameObject);
		CreateLR();
		_currentLineRenderer.positionCount = road._points.Count;
		List<Vector3> points = _roadSystem.GetRoadPointsInWorld(road);
		for(int i = 0; i < road._points.Count; i++)
			_currentLineRenderer.SetPosition(i, points[i]);
	}
	*/
		public void SaveNewRoad()
		{
			_roadSystem.ConvertLineToRoad(_currentLineRenderer);
			if (_currentLineRenderer != null)
				GameObject.DestroyImmediate(_currentLineRenderer.gameObject);
		}

		public void VisualizeAllRoads()
		{
			DeleteOld();
			foreach (RoadSystem.Road road in _roadSystem._roads)
			{
				_roadSystem.VisualizeRoad(road,false);
			}
		}
		void DeleteOld()
		{
			return;
			List<GameObject> deletes = new List<GameObject>();
			deletes.AddRange(GameObject.FindGameObjectsWithTag("DeleteMeEditor"));
			foreach (GameObject delgo in deletes)
				GameObject.DestroyImmediate(delgo);
		}
		void LoadRoadPrefabs()
		{
			_roadPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/UI/Map/Prefabs Map/Road Prefab.prefab", typeof (GameObject));
			_roadMask = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/UI/Map/Prefabs Map/Road Mask Parent.prefab", typeof(GameObject));
		}
	}
}
