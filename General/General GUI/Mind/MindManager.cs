using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static RD.CodeTools;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;

namespace RD.UI
{
	public class GridTransform
	{
		public GridTransform(GridTransform gt)
		{
			_points = new List<GridPoint>(gt._points);
			_position = gt._position;
			_rotation = gt._rotation;
		}
		public GridTransform()
		{

			_points = new List<GridPoint>();
			_position = new GridPoint(0, 0);
		}
		public void Rotate()
		{
			_rotation++;
			while (_rotation > 3)
				_rotation -= 4;
		}
		public List<GridPoint> _points;
		public GridPoint _position;
		/// <summary>
		/// 0 = 0 deg, 1 = 90 deg, 2 = 180 deg, 3 = 270 deg
		/// </summary>
		public int _rotation;
	}
	public class MindManager : MonoBehaviour
	{
		[Serializable]
		public class SpaceArray
		{
			public SpaceArray(int length)
			{
				_blocks = new List<MindSpace>();
				for (int i = 0; i < length; i++)
					_blocks.Add(null);
			}
			public List<MindSpace> _blocks = new List<MindSpace>();
		}
		public static MindManager _current;
		public List<SpaceArray> _spaceArray;
		public List<MindSpace> _spaces;
		ThoughtUI _movedThought;
		private void Awake()
		{
			_current = this;
		}
		/// <summary>
		/// Avoid double inputs
		/// </summary>
		bool _receivedInput = false;
		private void Update()
		{
			_receivedInput = false;
		}
		public void SetMoveThought(ThoughtUI thought)
		{
			_receivedInput = true;
			_movedThought = thought;
		}
		/// <summary>
		/// Rotate the thought being currently moved counter-clockwise
		/// </summary>
		public void RotateCCW()
		{
			if (_movedThought != null)
			{
				_movedThought.Rotate(3);
			}
		}
		/// <summary>
		/// Rotate the thought being currently moved clockwise
		/// </summary>
		public void RotateCW()
		{
			if (_movedThought != null)
			{
				_movedThought.Rotate(1);
			}
		}

		#region Input
		public void InputCircle(InputAction.CallbackContext context)
		{
			if (CodeTools.IsKeyClick(context))
			{
				RotateCCW();
			}
		}
		public void InputSquare(InputAction.CallbackContext context)
		{
			if (CodeTools.IsKeyClick(context))
			{
				RotateCW();
			}
		}
		public void InputTriangle(InputAction.CallbackContext context)
		{
			if (CodeTools.IsKeyClick(context))
			{
			}
		}
		public void InputCross(InputAction.CallbackContext context)
		{
			if (CodeTools.IsKeyClick(context))
			{
				if (_movedThought && !_receivedInput) _movedThought.Detach();
			}
		}
		public void InputMouseClick(InputAction.CallbackContext context)
		{
			if (CodeTools.IsMouseClick(context))
			{
				if (_movedThought && !_receivedInput) _movedThought.Detach();
			}
		}
		#endregion
		public void ReleaseSpaces(List<MindSpace> occupiedSpaces)
		{
			foreach(MindSpace ms in occupiedSpaces)
			{
				ms.Unfill();
			}
		}
		public bool AttachThought()
		{
			if (_movedThought == null)
				return false;
			_receivedInput = true;
			ThoughtUI thought = _movedThought;
			_movedThought = null;
			thought._zeroPoint.transform.SetParent(transform);
			foreach(MindSpace ms in _spaces)
				{
					ms.ResetColor();
				}
			int id = 0;
			float distance = Vector3.Distance(_spaces[0].transform.localPosition, thought._zeroPoint.localPosition);
			for (int i=1; i < _spaces.Count; i++)
			{
				float dist = Vector3.Distance(_spaces[i].transform.localPosition, thought._zeroPoint.localPosition);
				if (dist < distance)
				{
					id = i;
					distance = dist;
				}
			}
			thought._zeroPoint.transform.SetParent(thought.transform);
			GridPoint prevPoint = thought._gtransform._position;
			thought._gtransform._position = _spaces[id]._point;
			if (CastThought(thought))
			{
				Vector2 offset = thought._zeroPoint.anchoredPosition;
				for (int r = 0; r < thought._gtransform._rotation; r++)
				{
					offset = RotateBy90(offset);
				}
				thought.transform.position = _spaces[id].transform.position;
				thought.GetComponent<RectTransform>().anchoredPosition -= offset;
				return true;
			}
			else
			{
				Debug.LogError("Multiple overlapping squares causes bugs!!");
				thought._gtransform._position = prevPoint;
				_movedThought = thought;
				return false;
			}
		}
		public bool CastThought(ThoughtUI thought)
		{
			GridTransform gt = thought._gtransform;
			List<GridPoint> gps = new List<GridPoint>();
			foreach(GridPoint gp in gt._points)
			{
				GridPoint g = new GridPoint(gp);
				for(int i = 0; i < gt._rotation; i++)
				{
					g.Rotate();
				}
				g.Add(gt._position);
				gps.Add(g);
			}
			bool valid = true;
			ThoughtUI overlappingThought = null;
			List<MindSpace> mss = new List<MindSpace>();
			foreach(GridPoint gp in gps)
			{
				if(valid && (gp.x<7 && gp.x>=0 && gp.y < 7 && gp.y>=0))
				{
					MindSpace ms = GetSpace(gp);
					mss.Add(ms);
					if (ms.SetThought(thought) == false) {
						//if overlapping with multiple thoughts, prevent placement
						if (overlappingThought == null)
							overlappingThought = ms.GetThought();
						else if(overlappingThought!=ms.GetThought())
							valid = false;
					}
				}
				else
				{
					valid = false;
					Debug.Log("<color=red>"+gp.ToString()+"</color> is out of bounds!");
				}
			}
			if (valid && overlappingThought != null)
				Swap();
			return valid;
			void Swap()
			{
				overlappingThought.transform.position = GetCursorPosition();
				overlappingThought.Attach();
				foreach (MindSpace ms in mss)
				{
					ms.SetThought(thought);
				}
			}
		}
		public MindSpace GetSpace(GridPoint gp)
		{
			return _spaceArray[gp.x]._blocks[gp.y];
		}

	}
#if UNITY_EDITOR
	[CustomEditor(typeof(MindManager))]
	public class MindManagerEditor : Editor
	{
		MindManager _mm;
		public bool _showIndex = true;
		public bool _showDefault = true;
		GridPoint _position;
		int _rotation;
		ThoughtUI.ThoughtShape shape = ThoughtUI.ThoughtShape.One;
		public override void OnInspectorGUI()
		{
			_showDefault = EditorGUILayout.BeginFoldoutHeaderGroup(_showDefault, "Defaults"); EditorGUILayout.EndFoldoutHeaderGroup();
			if (_showDefault) DrawDefaultInspector();
			else CustomEditor();
		}
		void CustomEditor()
		{

			_mm = target as MindManager;
			#region Testing
			/*
			if(GUILayout.Button("Autoassign GP"))
			{
				foreach (MindSpace ms in _mm._spaces)
				{
					int x, y;
					RectTransform rt= ms.GetComponent<RectTransform>();
					x = Mathf.RoundToInt((rt.anchoredPosition.x-32) / 64);
					y = Mathf.RoundToInt((rt.anchoredPosition.y + 366) / 64);
					ms._point = new GridPoint(x,y);
					EditorUtility.SetDirty(ms.transform.GetChild(0).gameObject);
				}
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Clockwise >v"))
			{
				foreach (Mindblock mb in _mm._mindblocks)
				{
					mb._point.Rotate(true);
					EditorUtility.SetDirty(mb.transform.GetChild(0).gameObject);
				}
			}
			if (GUILayout.Button("Counter-clockwise >^"))
			{
				foreach(Mindblock mb in _mm._mindblocks)
				{
					mb._point.Rotate(false);
					EditorUtility.SetDirty(mb.transform.GetChild(0).gameObject);
				}
			}
			GUILayout.EndHorizontal();
			if(GUILayout.Button("Test GP"))
			{
				_mm.TestGridPoints();
			}
			*/
			/*
			if(GUILayout.Button("Add Children",GUILayout.Width(100))){
				_mm._mindblocks = new List<Mindblock>();
				foreach (Mindblock mb in _mm.GetComponentsInChildren<Mindblock>())
					_mm._mindblocks.Add(mb);
			}
			*/
			/*
			if (_mm._mindblocks.Count>4 && GUILayout.Button("Generate Groups"))
			{
				GridPoint _amount;
				{ int temp = Mathf.RoundToInt(Mathf.Sqrt(_mm._mindblocks.Count));
					Debug.Log(temp);
				_amount = new GridPoint(temp, temp);
				}
				_mm._mbs = new List<MindManager.BlockArray>();
				_mm._mbs.Capacity = _amount.x;
				for(int x = 0; x < _amount.x; x++)
				{
					_mm._mbs.Add(new MindManager.BlockArray(_amount.y));
				}
				foreach(Mindblock mb in _mm._mindblocks)
				{
					_mm._mbs[mb._point.x]._blocks[mb._point.y] = mb;
				}
			}*/
			GUILayout.EndHorizontal();
			#endregion
			EditorGUILayout.LabelField("----------");
		}
		GridPoint GetGP(RectTransform rt, GridPoint offset, int size)
		{
			int x, y;
			x = Mathf.RoundToInt((rt.anchoredPosition.x + offset.x) / size);
			y = Mathf.RoundToInt((rt.anchoredPosition.y + offset.y) / size);
			GridPoint gp = new GridPoint(x, y);
			return gp;
		}
	}
#endif
}
