using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RD
{
	public class LerpColor : MonoBehaviour
	{
		[NonSerialized]public Color _oldColor;
		[NonSerialized] public Color _newColor;
		float lerpTimer;
		[NonSerialized] public float speed;
		public bool _enabled = false;
		[NonSerialized] public bool boomerang;
		public AnimationCurve _curve = new AnimationCurve();
		Image img;
		SpriteRenderer _spriteRenderer;
		TextMeshPro _textMeshPro;
		TextMeshProUGUI _textMeshProUGUI;
		Text _text;
		[SerializeField]CompType type;
		ColorType _colorType;
		public enum CompType { Null,Image,Text,TMPro,TMProUGUI,SpriteRenderer}
		public enum ColorType { A,RGB,RGBA}
		public bool _disableObjectAfterLerp;
		public struct ColorBool
		{
			public Color c;
			public bool enabled;
			public ColorBool(Color color, bool enable = false)
			{
				c = color;
				enabled = enable;
			}
		}
		private void Update()
		{
			if (_enabled)
			{
				lerpTimer = lerpTimer + CodeTools.Tm.GetUIDelta() * speed;
				if (lerpTimer >= 1)
				{
					if (boomerang)
					{
						boomerang = false;
						Color temp = _oldColor;
						_oldColor = _newColor;
						_newColor = temp;
						lerpTimer -= 1;
					}
					else
					{
						_enabled = false;
						if (_disableObjectAfterLerp)
							gameObject.SetActive(false);
					}
				}
				SetColor();
			}
		}
		public void StartLerp(ColorBool newColor, ColorBool oldColor, ColorType colorType = ColorType.RGBA, float lerpSpeed = 1,CompType t = CompType.Null,AnimationCurve curve = null, bool boomerang = false)
		{


			if (t == CompType.Null)
			{
				t = type;
				if (type == CompType.Null)
				{
					Debug.LogError("Unassigned LerpColor Type on " + gameObject.name + "!");
					return;
				}
			}
			else
				type = t;


			{
				Color c = AutoComponent();
				if (!oldColor.enabled)
					_oldColor = c;
			
				switch (type)
				{
					case CompType.Image:
						img = GetComponent<Image>();
						if (!oldColor.enabled)
							_oldColor = img.color; 
						c = img.color;
						break;
					case CompType.SpriteRenderer:
						_spriteRenderer = GetComponent<SpriteRenderer>();
						if (!oldColor.enabled)
							_oldColor = _spriteRenderer.material.GetColor("_Color");
						c = _spriteRenderer.material.GetColor("_Color");
						break;
					case CompType.Text:
						_text = GetComponent<Text>();
						if (!oldColor.enabled)
							_oldColor = _text.color;
						c = _text.color;
						break;
					case CompType.TMPro:
						_textMeshPro = GetComponent<TextMeshPro>();
						if (!oldColor.enabled)
							_oldColor = _textMeshPro.color;
						c = _textMeshPro.color;
						break;
					case CompType.TMProUGUI:
						_textMeshProUGUI = GetComponent<TextMeshProUGUI>();
						if (!oldColor.enabled)
							_oldColor = _textMeshProUGUI.color;
						c = _textMeshProUGUI.color;
						break;
				}
				if (colorType==ColorType.A)
				{
					oldColor.c = new Color(c.r, c.g, c.b, oldColor.c.a);
					newColor.c = new Color(c.r, c.g, c.b, newColor.c.a);
				}
			}
			speed = lerpSpeed;
			_newColor = newColor.c;
			_enabled = true;
			lerpTimer = 0;
			if (oldColor.enabled)
				_oldColor = oldColor.c;
			if (curve == null && _curve.keys.Length < 2)
			{
				_curve = new AnimationCurve();
				UIAnimationTools.DefaultCurve(_curve);
			}
			if (curve != null)
				_curve = curve;
			_colorType = colorType;
			SetColor();
		}
		public void StartLerpAlpha(float newAlpha, float oldAlpha = -1, float lerpspeed = 1)
		{
			Color c = AutoComponent();
			if (oldAlpha < 0)
				_oldColor = c;
			else
				_oldColor = new Color(c.r, c.g, c.b, oldAlpha);
			_newColor = new Color(c.r, c.g, c.b, newAlpha);
			speed = lerpspeed;
			_colorType = ColorType.A;
			if (_curve == null || _curve.keys.Length < 2)
			{
				_curve = new AnimationCurve();
				UIAnimationTools.DefaultCurve(_curve);
			}
			_enabled = true;
			lerpTimer = 0;
			SetColor();
		}

		public Color AutoComponent()
		{
			if (GetComponent<Image>())
			{
				type = CompType.Image;
				img = GetComponent<Image>();
				return img.color;
			}
			else if (GetComponent<SpriteRenderer>())
			{
				type = CompType.SpriteRenderer;
				_spriteRenderer = GetComponent<SpriteRenderer>();
				return _spriteRenderer.material.GetColor("_Color");
			}
			else if (GetComponent<Text>())
			{
				type = CompType.Text;
				_text = GetComponent<Text>();
				return _text.color;
			}
			else if (GetComponent<TextMeshPro>())
			{
				type = CompType.TMPro;
				_textMeshPro = GetComponent<TextMeshPro>();
				return _textMeshPro.color;
			}
			else if (GetComponent<TextMeshProUGUI>())
			{
				type = CompType.TMProUGUI;
				_textMeshProUGUI = GetComponent<TextMeshProUGUI>();
				return _textMeshProUGUI.color;
			}
			else
				return Color.white;
		}
		public Color SetColor()
		{
		
			Color temp = Color.Lerp(_oldColor, _newColor, _curve.Evaluate(lerpTimer));
			Color current = new Color();
			switch (type)
			{
				case CompType.Image:
					current = img.color;
					break;
				case CompType.SpriteRenderer:
					current = _spriteRenderer.material.GetColor("_Color");
					break;
				case CompType.Text:
					current = _text.color;
					break;
				case CompType.TMPro:
					current = _textMeshPro.color;
					break;
				case CompType.TMProUGUI:
					current = _textMeshProUGUI.color;
					break;
			}
			if (_colorType==ColorType.A)
			{
				temp.r = current.r;
				temp.g = current.g;
				temp.b = current.b;
			}
			else if (_colorType == ColorType.RGB)
			{
				temp.a = current.a;
			}
			
			switch (type)
			{
				case CompType.Image:
					img.color = temp;
					break;
				case CompType.SpriteRenderer:
					_spriteRenderer.material.SetColor("_Color", temp);
					break;
				case CompType.Text:
					_text.color = temp;
					break;
				case CompType.TMPro:
					_textMeshPro.color = temp;
					break;
				case CompType.TMProUGUI:
					_textMeshProUGUI.color = temp;
					break;
			}
			return temp;
		}

	}
}
