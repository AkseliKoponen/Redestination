using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static RD.CodeTools;

namespace RD
{
	public class RoadSystem : MonoBehaviour
	{
		public GameObject _roadPrefab;
		public GameObject _roadMask;
		public List<Road> _roads;
		public List<MapNode> _mapNodes;
		Canvas _canvas;

		[Serializable] public class Road
		{
			public List<Vector3> _points;
			public MapNode _nodeStart;
			public MapNode _nodeEnd;
			public string name;
			[NonSerialized]public GameObject _visualRoad;
			public List<float> _flowLengths;
			public bool _flowing;
			public Road(List<Vector3> points, RoadSystem system)
			{
				_points = new List<Vector3>();
				_points.AddRange(points);
				_nodeStart = system.FindClosestMapNode(points[0]);
				_nodeEnd = system.FindClosestMapNode(points[points.Count - 1]);
				name = _nodeStart.name + " - " + _nodeEnd.name;
				_flowing = false;
				_flowLengths = new List<float>();
			}


		}
		public void InputSpace(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (IsKeyClick(cbt))
				VisualizeAllRoads();
		}
		public void InputFlipRoad(UnityEngine.InputSystem.InputAction.CallbackContext cbt)
		{
			if (IsKeyClick(cbt))
				FlipAllRoads();
		}
		void FlipAllRoads()
		{
			foreach (Road road in _roads)
			{
				FlipRoad(road);
			}
		}
		void VisualizeAllRoads()
		{
			foreach (Road road in _roads)
			{
				VisualizeRoad(road);
				FlowRoadStart(road);
			}
		}
		public void ConvertLineToRoad(LineRenderer lineRenderer)
		{
			//CanvasScaler cs = _canvas.GetComponent<CanvasScaler>();
			//float scalerFactor = cs.scaleFactor;
			//Debug.Log("scaleFactor = " + scalerFactor);
			List<Vector3> points = new List<Vector3>();
			for(int i = 0; i < lineRenderer.positionCount; i++)
			{
				points.Add(_canvas.WorldToCanvasPosition(lineRenderer.GetPosition(i)));
				//points.Add(lineRenderer.GetPosition(i));
			}
			Road road = new Road(points,this);
			_roads.Add(road);
		}

		/*

	public List<Vector3> GetRoadPointsInWorld(Road road)
	{
		List<Vector3> points = new List<Vector3>();
		//RectTransformUtility.ScreenPointToWorldPointInRectangle()
		foreach (Vector3 point in road._points)
		{
			points.Add(point);
			//points.Add(_canvas.);		//Tässä on virheitä

		}
		return points;
	}

	Vector3 CanvasToWorldPosition(Vector2 point)
	{
		Camera cam = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
		Vector3 outPoint = new Vector3();
		CanvasScaler cs = _canvas.GetComponent<CanvasScaler>();
		float scalerFactor = cs.scaleFactor;
		Debug.Log("scaleFactor = " + scalerFactor);
		//RectTransformUtility.ScreenPointToWorldPointInRectangle(this.GetComponent<RectTransform>(), point, cam, out outPoint);
		outPoint = cam.ScreenToWorldPoint(point);
		outPoint.z = 0;
		return outPoint;
	}
	*/

		MapNode FindClosestMapNode(Vector2 point)
		{
			if (_mapNodes == null || _mapNodes.Count < 1)
				return null;
			MapNode mapnode = _mapNodes[0];
			float distance = Vector2.Distance(mapnode.GetComponent<RectTransform>().anchoredPosition, point);
			foreach (MapNode mn in _mapNodes)
			{
				float newdistance = Vector2.Distance(mn.GetComponent<RectTransform>().anchoredPosition, point);
				if (newdistance < distance)
				{
					mapnode = mn;
					distance = newdistance;
				}
			}
			return mapnode;
		}

		public void FindCanvas()
		{
			if (_canvas == null)
			{
				_canvas = GetComponentInParent<Canvas>();
			}
		}

		public void VisualizeRoad(Road road, bool reverse = false,bool clearOld = true)
		{
			if (clearOld)
				DeleteOld();
			GameObject roadParent = new GameObject();
			roadParent.transform.parent = transform;
			roadParent.name = road.name;
			roadParent.gameObject.tag = "DeleteMeEditor";
			RectTransform rt = roadParent.AddComponent<RectTransform>();
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.anchoredPosition = Vector2.zero;
			rt.sizeDelta = Vector2.zero;
			rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0);
			rt.localScale = Vector3.one;
			Color color = new Color(0.1f, 0.1f, 0.1f);
			//Color color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
			for (int i = 1; i < road._points.Count; i++)
			{
				Vector2 startPoint = road._points[i - 1];
				Vector2 endPoint = road._points[i];

				GameObject gor = GameObject.Instantiate(_roadPrefab);
				gor.transform.parent = roadParent.transform;
				gor.name = road.name + ", " + (i - 1) + " to " + i + " (Road)";
				gor.gameObject.tag = "DeleteMeEditor";
				Image img = gor.GetComponent<Image>();
				img.color = color;
				RectTransform grt = gor.GetComponent<RectTransform>();
				grt.localPosition = new Vector3(grt.localPosition.x, grt.localPosition.y, 0);
				grt.localScale = Vector3.one;
				float length = Vector2.Distance(startPoint, endPoint);
				grt.pivot = new Vector2(0, 0.5f);
				float angle = Mathf.Rad2Deg * Mathf.Asin((startPoint.y - endPoint.y) / length) * -1;
				float imagewidth = _roadPrefab.GetComponent<RectTransform>().sizeDelta.x;
				length = Mathf.RoundToInt(length / imagewidth);
				length *= imagewidth;

				grt.sizeDelta = new Vector2(length, 10);
				grt.anchoredPosition = startPoint;
				road._flowLengths.Add(length);
				GameObject oldRoad = gor;
				grt.eulerAngles = new Vector3(0, 0, angle);
				gor = GameObject.Instantiate(_roadMask);
				gor.gameObject.tag = "DeleteMeEditor";
				gor.transform.parent = roadParent.transform;
				gor.name = road.name + ", " + (i - 1) + " to " + i;
				RectTransform oldgrt = grt;
				grt = gor.GetComponent<RectTransform>();
				grt.localPosition = new Vector3(grt.localPosition.x, grt.localPosition.y, 0);
				grt.localScale = Vector3.one;
				grt.sizeDelta = new Vector2(length, 10);
				grt.pivot = new Vector2(0, 0.5f);
				grt.anchoredPosition = startPoint;
				grt.eulerAngles = new Vector3(0, 0, angle);
				oldRoad.transform.SetParent(gor.transform);
			}
			road._visualRoad = roadParent;
			Debug.Log(road._visualRoad.name);
		}

		public void FlowRoadStart(Road road,bool reverseFill = false)
		{
			if (!road._visualRoad)
			{
				Debug.LogError("Trying to Flow null road");
				return;
			}
			foreach (Image img in road._visualRoad.GetComponentsInChildren<Image>())
				img.fillAmount = reverseFill?1:0;
			road._flowing = true;
			StartCoroutine(FlowRoad(road,reverseFill));

		}
		IEnumerator FlowRoad(Road road, bool reverseFill = false, float footPrintDuration = 0)
		{
			float time = 0;
			float lerpend = 1;
			float lerpSpeed = 200;
			float imgwidth = _roadPrefab.GetComponent<RectTransform>().sizeDelta.x;
			for (int i = 0; i < road._flowLengths.Count && i<road._visualRoad.transform.childCount; i++)
			{
				float stepsize = 1 / (road._flowLengths[i] / imgwidth);
				while (time < road._flowLengths[i])
				{
					if (!road._flowing)
					{
						break;
					}
					time += CodeTools.Tm.GetUIDelta() * lerpSpeed;
					float fillamount = Mathf.Lerp(0, 1, time / road._flowLengths[i]);
					if(fillamount-road._visualRoad.transform.GetChild(i).GetComponent<Image>().fillAmount >= stepsize)
						road._visualRoad.transform.GetChild(i).GetComponent<Image>().fillAmount = fillamount;
					yield return null;
				}
				road._visualRoad.transform.GetChild(i).GetComponent<Image>().fillAmount = lerpend;
				time -= road._flowLengths[i];
			}

		}

		void FlipRoad(Road road) {
			if (!road._visualRoad)
				return;
			for(int i = 0; i<road._visualRoad.transform.childCount;i++)
			{
				Image img = road._visualRoad.transform.GetChild(i).GetComponent<Image>();
				if (img.fillOrigin == 0)
					img.fillOrigin = 1;
				else
					img.fillOrigin = 0;
				Debug.Log(img.gameObject.name);
				RectTransform childRect = img.transform.GetChild(0).GetComponent<RectTransform>();

				CodeTools.SetPivot(childRect, new Vector2(0.5f, 0.5f));
				childRect.localEulerAngles = new Vector3(0, 0, childRect.localEulerAngles.z-180);
				childRect.anchoredPosition = new Vector2(0, 0);
			}
		}
		void DeleteOld()
		{
			List<GameObject> deletes = new List<GameObject>();
			deletes.AddRange(GameObject.FindGameObjectsWithTag("DeleteMeEditor"));
			foreach (GameObject delgo in deletes)
				GameObject.DestroyImmediate(delgo);
			foreach (Road road in _roads)
				road._flowLengths = new List<float>();
		}
	}
}
