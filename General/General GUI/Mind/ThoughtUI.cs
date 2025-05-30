using System;
using System.Collections.Generic;
using UnityEngine;
using RD.UI;
using UnityEngine.UI;
using RD;
using UnityEngine.InputSystem;
using System.Collections;
using RD.DB;
using Coffee.UISoftMask;
#if UNITY_EDITOR
using UnityEditor;
#endif
using static RD.CodeTools;
namespace RD.UI {
	public class ThoughtUI : MonoBehaviour
	{
		public static int _squareSize = 64;
		public enum ThoughtShape
		{
			One,
			Two,
			TriLong,
			TriAngle,
			QuadCube,
			QuadLong,
			QuadAngleA,
			QuadAngleB,
			QuadMid,
			QuadSA,
			QuadSB,
			PentaCubeA,
			PentaCubeB,
			PentaLA,
			PentaLB,
			PentaAngle,
			PentaCross,
			PentaHole,
			PentaMidA,
			PentaMidB,
			PentaLong,
			PentaZig,
			PentaZag,
			PentaW,
			PentaFourA,
			PentaFourB,
			PentaT,
			PentaNA,
			PentaNB,
		}
		/// <summary>
		/// Get the list of GridPoints used by the <paramref name="shape"/>. See Assets/Prefabs/UI/Mind/Thought Scheme.jpg for more info
		/// </summary>
		public static List<GridPoint> GetShapePoints(ThoughtShape shape)
		{
			List<GridPoint> gps = new List<GridPoint>() { new GridPoint(0, 0) };
			switch ((int)shape)
			{
				default:
					Debug.LogError("Unimplemented shape " + shape.ToString());
					break;
				case 0:
					//One
					break;
				case 1:
					//Two
					gps.Add(new GridPoint(0, 1));
					break;
				case 2:
					//TriLong
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(0, -1));
					break;
				case 3:
					//TriAngle
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(1, 0));
					break;
				case 4:
					//QuadCube
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(1, 0));
					gps.Add(new GridPoint(1, 1));
					break;
				case 5:
					//QuadLong
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(0, 2));
					gps.Add(new GridPoint(0, -1));
					break;
				case 6:
					//QuadAngleA
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(0, 2));
					gps.Add(new GridPoint(-1, 0));
					break;
				case 7:
					//QuadAngleB
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(0, 2));
					gps.Add(new GridPoint(1, 0));
					break;
				case 8:
					//QuadMid
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(0, -1));
					gps.Add(new GridPoint(1, 0));
					break;
				case 9:
					//QuadSA
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(-1, 0));
					gps.Add(new GridPoint(1, 1));
					break;
				case 10:
					//QuadSB
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(1, 0));
					gps.Add(new GridPoint(-1, 1));
					break;
				case 11:
					//PentaCubeA
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(1, 0));
					gps.Add(new GridPoint(1, 1));
					gps.Add(new GridPoint(0, -1));
					break;
				case 12:
					//PentaCubeB
					gps.Add(new GridPoint(-1, 0));
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(-1, 1));
					gps.Add(new GridPoint(0, -1));
					break;
				case 13:
					//Penta-L-A
					gps.Add(new GridPoint(1, 1));
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(0, -1));
					gps.Add(new GridPoint(0, -2));
					break;
				case 14:
					//Penta-L-B
					gps.Add(new GridPoint(-1, 1));
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(0, -1));
					gps.Add(new GridPoint(0, -2));
					break;
				case 15:
					//PentaAngle
					gps.Add(new GridPoint(0, -2));
					gps.Add(new GridPoint(0, -1));
					gps.Add(new GridPoint(-1, 0));
					gps.Add(new GridPoint(-2, 0));
					break;
				case 16:
					//PentaCross
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(0, -1));
					gps.Add(new GridPoint(1, 0));
					gps.Add(new GridPoint(-1, 0));
					break;
				case 17:
					//PentaHole
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(0, -1));
					gps.Add(new GridPoint(1, 1));
					gps.Add(new GridPoint(1, -1));
					break;
				case 18:
					//PentaMidA
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(0, -1));
					gps.Add(new GridPoint(1, 0));
					gps.Add(new GridPoint(0,-2));
					break;
				case 19:
					//PentaMidB
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(0, -1));
					gps.Add(new GridPoint(1, 0));
					gps.Add(new GridPoint(0, 2));
					break;
				case 20:
					//PentaLong
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(0, 2));
					gps.Add(new GridPoint(0, -1));
					gps.Add(new GridPoint(0, -2));
					break;
				case 21:
					//PentaZig
					gps.Add(new GridPoint(-1, -1));
					gps.Add(new GridPoint(-1, 0));
					gps.Add(new GridPoint(1, 1));
					gps.Add(new GridPoint(1, 0));
					break;
				case 22:
					//PentaZag
					gps.Add(new GridPoint(-1, 1));
					gps.Add(new GridPoint(-1, 0));
					gps.Add(new GridPoint(1, -1));
					gps.Add(new GridPoint(1, 0));
					break;
				case 23:
					//PentaW
					gps.Add(new GridPoint(1, 0));
					gps.Add(new GridPoint(1, 1));
					gps.Add(new GridPoint(0, -1));
					gps.Add(new GridPoint(-1, -1));
					break;
				case 24:
					//PentaFourA
					gps.Add(new GridPoint(1, 0));
					gps.Add(new GridPoint(-1, 0));
					gps.Add(new GridPoint(-1, 1));
					gps.Add(new GridPoint(0, -1));
					break;
				case 25:
					//PentaFourB
					gps.Add(new GridPoint(1, 0));
					gps.Add(new GridPoint(-1, 0));
					gps.Add(new GridPoint(1, 1));
					gps.Add(new GridPoint(0, -1));
					break;
				case 26:
					//PentaT
					gps.Add(new GridPoint(0, 1));
					gps.Add(new GridPoint(1, 1));
					gps.Add(new GridPoint(-1, 1));
					gps.Add(new GridPoint(0, -1));
					break;
				case 27:
					//PentaNA
					gps.Add(new GridPoint(0, -1));
					gps.Add(new GridPoint(0, -2));
					gps.Add(new GridPoint(1, 0));
					gps.Add(new GridPoint(1, 1));
					break;
				case 28:
					//PentaNB
					gps.Add(new GridPoint(0, -1));
					gps.Add(new GridPoint(0, -2));
					gps.Add(new GridPoint(-1, 0));
					gps.Add(new GridPoint(-1, 1));
					break;
			}
			return gps;
		}
		/// <summary>
		/// Get the local position of (0,0) block for <paramref name="shape"/>
		/// </summary>
		static Vector2 GetZeroPoint(ThoughtShape shape)
		{
			float x = 0;
			float y = 0;
			//padding of 2 pixels
			switch ((int)shape)
			{
				default:
					Debug.LogError("Unimplemented shape " + shape.ToString());
					break;
				case 0:
					//One
					break;
				case 1:
					//Two
					y = -0.5f;
					break;
				case 2:
					//TriLong
					break;
				case 3:
					//TriAngle
					x = -0.5f;
					y = -0.5f;
					break;
				case 4:
					//QuadCube
					x = -0.5f;
					y = -0.5f;
					break;
				case 5:
					//QuadLong
					y = -0.5f;
					break;
				case 6:
					//QuadAngleA
					y = -1;
					x = 0.5f;
					break;
				case 7:
					//QuadAngleB
					y = -1;
					x = -0.5f;
					break;
				case 8:
					//QuadMid
					x = -0.5f;
					break;
				case 9:
				case 10:
					//QuadSA
					//QuadSB
					y= -0.5f;
					break;
				case 11:
					//PentaCubeA
					x = -0.5f;
					break;
				case 12:
					//PentaCubeB
					x = 0.5f;
					break;
				case 13:
					//Penta-L-A
					x -= 0.5f;
					y = 0.5f;
					break;
				case 14:
					//Penta-L-B
					x = 0.5f;
					y = 0.5f;
					break;
				case 15:
					//PentaAngle
					x = 1;
					y = 1;
					break;
				case 16:
					//PentaCross
					break;
				case 17:
					//PentaHole
					x -= 0.5f;
					break;
				case 18:
					//PentaMidA
					x -= 0.5f;
					y += 0.5f;
					break;
				case 19:
					//PentaMidB
					x -= 0.5f;
					y -= 0.5f;
					break;
				case 20:
					//PentaLong
					break;
				case 21:
					//PentaZig
					break;
				case 22:
					//PentaZag
					break;
				case 23:
					//PentaW
					break;
				case 24:
					//PentaFourA
					break;
				case 25:
					//PentaFourB
					break;
				case 26:
					//PentaT
					break;
				case 27:
					//PentaNA
					x -= 0.5f;
					y += 0.5f;
					break;
				case 28:
					//PentaNB
					x += 0.5f;
					y += 0.5f;
					break;
			}
			Vector2 zp = new Vector2(x * (_squareSize / 64) * 64, y * (_squareSize / 64) * 64);
			return zp;
		}
		/// <summary>
		/// Get the sprites used by <paramref name="shape"/>
		/// </summary>
		/// <returns>[0]background<br/>
		/// [1]solid frame<br/>
		/// [2]dash frame<br/>
		/// [3]dot frame</returns>
		public static List<Sprite> GetShapeSprites(ThoughtShape shape)
		{
			List<Sprite> sprites = new List<Sprite>();
			string path = "Art/Mind/ThoughtFrames/" + shape.ToString();
			sprites.Add(Resources.Load<Sprite>(path+"_fill"));
			sprites.Add(Resources.Load<Sprite>(path));//solid
			sprites.Add(Resources.Load<Sprite>(path + "_dash"));
			sprites.Add(Resources.Load<Sprite>(path + "_dotted"));
			return sprites;
		}
		public Image _imgMask;
		public Image _imgBorder;
		public Image _imgFill;
		public GridTransform _gtransform = new GridTransform();
		[NonSerialized]public List<MindSpace> _occupiedSpaces = new List<MindSpace>();
		[NonSerialized]public List<MindSpace> _spaces;
		[NonSerialized]public Color _color = default;
		public RD.Combat.Thought _thought;
		RectTransform _rt;
		Image _image;
		ThoughtShape _shape = ThoughtShape.One;
		public ThoughtShape _testShape;
		public RectTransform _zeroPoint;
		bool _attached = false;
		Vector3 _attachOffset;
		void Awake()
		{
			TestShape(_testShape);
		}
		public void Update()
		{
			if (_attached)
			{
				transform.position = (Vector2)(GetCursorPosition() + _attachOffset);
			}
		}
		bool _receivedInput = false;
		/// <summary>
		/// Disable Input and enable them again after <paramref name="time"/> has pased
		/// </summary>
		IEnumerator DisableInput(float time = 0.15f)
		{
			_receivedInput = true;
			time += Tm.GetUIDelta();
			while (time > 0)
			{
				time -= Tm.GetUIDelta();
				yield return null;
			}
			_receivedInput = false;
		}
		bool _mouseOver = false;
		Color _backgroundColor;
		Color _highlightColor;
		public void Highlight()
		{
			_mouseOver = true;
			_imgFill.GetComponent<Outline>().enabled = true;
			_imgBorder.GetComponent<Outline>().enabled = true;
			if (_thought)_thought.DisplayTooltip(_rt);
		}
		public void Lowlight()
		{
			_mouseOver = false;
			TooltipSystem.HideAllTooltips();
			if (_attached)
				return;
			_imgFill.GetComponent<Outline>().enabled = false;
			_imgBorder.GetComponent<Outline>().enabled = false;

		}
		public void Attach()
		{
			if (_receivedInput)
				return;
			StartCoroutine(DisableInput());
			if (!_attached)
			{
				_attached = true;
				transform.SetAsLastSibling();
				_attachOffset = transform.position-Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
				MindManager._current.ReleaseSpaces(_occupiedSpaces);
				MindManager._current.SetMoveThought(this);
			}
			else Detach();
			//highlight
		}
		public void Detach()
		{
			if (_receivedInput)
				return;
			StartCoroutine(DisableInput());
			//lowlight
			bool successfulDetach = MindManager._current.AttachThought();
			_attached = !successfulDetach;
			if (!successfulDetach)
			{
				_occupiedSpaces = new List<MindSpace>();
			}
			if (!_mouseOver)
				Lowlight();
		}
		public void Rotate(int count = 1)
		{
			float angle = count * 90;
			while (count > 0)
			{
				count--;
				_gtransform.Rotate();
			}
			Vector3 rot = transform.localEulerAngles;
			rot.z += angle;
			while (rot.z > 360) rot.z -= 360;
			transform.localEulerAngles = rot;
		}
		public void Initialize(Combat.Thought thought)
		{
			TestShape(thought._shape);
		}

		public void TestShape(ThoughtShape shape)
		{
			_rt = GetComponent<RectTransform>();
			_shape = shape;
			_image = GetComponent<Image>();
			_image.alphaHitTestMinimumThreshold = 0.5f;
			List<Sprite> sprites = GetShapeSprites(shape);
			SetSprite(_image, sprites[0]);
			SetSprite(_imgMask, sprites[0]);
			SetSprite(_imgBorder, true ? sprites[1] : sprites[2]);
			_zeroPoint.anchoredPosition = GetZeroPoint(_shape);
			_gtransform._points = GetShapePoints(shape);
			_backgroundColor = _image.color;
			Vector3 hsv = CodeTools.GetHSV(_backgroundColor);
			hsv.z += 0.25f;
			_highlightColor = Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
			void SetSprite(Image img, Sprite spr)
			{
				img.sprite = spr;
				img.SetNativeSize();
				RectTransform rt = img.GetComponent<RectTransform>();
				rt.sizeDelta = new Vector2(_rt.sizeDelta.x - 2, _rt.sizeDelta.y - 2);//The images have 1px padding
			}
		}
#if UNITY_EDITOR
		public void PreviewThought(BaseThought bt)
		{
			TestShape(bt._shape);
			_imgFill.sprite = bt._artFillPattern;
			_imgFill.color = bt._color;
			_imgBorder.color = bt._color;
			if (bt._halfSize)
			{
				RectTransform fillRT = _imgFill.GetComponent<RectTransform>();
				fillRT.sizeDelta = new Vector2(256, 256);
			}
		}
#endif
	}
}
#if UNITY_EDITOR
[CustomEditor(typeof(ThoughtUI))]
public class ThoughtUIEditor : Editor
{
	bool _showDefault = true;
	ThoughtUI.ThoughtShape _shape;
	ThoughtUI _thought;
	public override void OnInspectorGUI()
	{
		_showDefault = EditorGUILayout.BeginFoldoutHeaderGroup(_showDefault, "Defaults"); EditorGUILayout.EndFoldoutHeaderGroup();
		if (_showDefault) DrawDefaultInspector();
		if (!_showDefault)
			CustomEditor();
	}
	void CustomEditor()
	{
		EditorGUILayout.LabelField("----------Custom----------");
		_thought = target as ThoughtUI;
		
		_shape = (ThoughtUI.ThoughtShape)EditorGUILayout.EnumPopup(_shape);
		if (GUILayout.Button("Test"))
		{
			_thought.TestShape(_shape);
		}
	}
}
#endif