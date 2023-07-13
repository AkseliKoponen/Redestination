using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RD.Combat;
using RD.DB;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RD
{
	public static class CodeTools
	{
		public static BaseCollection db { get; private set; }
		public static bool dbLoad { get; private set; } = false;
		public static void InitDatabase()
		{
			if (dbLoad)
				return;
			db = Resources.Load<BaseCollection>("Database/Collection");//.asset");
			
			dbLoad = true;

		}
		public static CombatManager _combatManager { get; private set; }
		public static PostProcessingManager _postProcessingManager { get; private set; }
		public static CombatGUI _combatGUI { get; private set; }
		public static Color NegativeColor(Color c, bool negateAlpha = false) {
			Color neg = new Color(1 - c.r, 1 - c.g, 1 - c.b, (negateAlpha ? 1 - c.a : c.a));
			return neg;
		}
		public static Facing GetReverseFacing(Facing f)
		{
			if (f == Facing.Left)
				return Facing.Right;
			return Facing.Left;
		}
		[Serializable]
		public enum Corner { TopLeft,TopRight,BottomLeft,BottomRight}
		[Serializable]
		public struct Bint //BaseInt (max and min values)
		{
			public int current;
			public int max;
			public int min;

			public Bint(int value, int minimum = 0, bool currentIsZero = false)
			{
				max = value;
				min = Mathf.Clamp(minimum,minimum,max);
				current = (currentIsZero ? 0 : max);
			}
			public Bint(int value, int minimum, bool currentIsZero, bool maxIsZero)
			{
				max = maxIsZero ? 0 : value;
				min = minimum;
				current = (currentIsZero ? 0 : value);
			}
			public Bint(Bint bint)
			{
				current = bint.current;
				max = bint.max;
				min = bint.min;
			}
			public Bint (bool tru)
			{
				current = 0;
				max = 0;
				min = 0;
			}
			public override string ToString()
			{
				return current.ToString();
			}
			public void Fill()
			{
				current = max;
			}
			public void SetMax(int i, bool clampCurrent = true)
			{
				max = i;
				current = clampCurrent ? Mathf.Clamp(current, min, max):current;
			}
			public void SetMin(int i = 0, bool clampCurrent = true)
			{
				min = i;
				current = clampCurrent ? Mathf.Clamp(current, min, max):current;
			}
			public void ModMax(int i,bool modCurrentAsPercentage = true, bool clampCurrent = true)
			{
				float percentage = max!=0?(current / max):1;
				max = max + i;
				current = clampCurrent ? Mathf.Clamp(modCurrentAsPercentage?Mathf.RoundToInt(max*percentage):current, min, max):current;
			}
			public void ModMin(int i, bool clampCurrent = true)
			{
				min = min + i;
				current = clampCurrent ? Mathf.Clamp(current, min, max):current;
			}
			
			public float GetPercentage()
			{
				return (float)current / (float)(max - min);
			}
			public int CalculatePercentage(float percentage)
			{
				return Mathf.RoundToInt(percentage * (max - min));
			}

			/// <summary>
			/// Adds i to current value
			/// --- hardClamp means nothing will be added if the result would be out of bounds.
			/// --- returns false if the resulting value was out of bounds
			/// </summary>
			public bool Add(int i, bool clampCurrent = true, bool hardClamp = false)
			{
				if(current+i<min || current + i > max)
				{
					if (hardClamp)
						return false;
					else
						current = clampCurrent ? Mathf.Clamp(current + i, min, max) : current + i;
						return false;
				}
				current = clampCurrent?Mathf.Clamp(current + i, min, max) : current + i;
				return true;
			}
			public void AddToCurrentAndLimits(int i)
			{
				current += i;
				if (i > 0)
					max += i;
				else
					min += i;
			}
			public string GetCompareString(string divider = "/", bool outOfMax = true)
			{
				return current.ToString() + divider + (outOfMax ? max.ToString() : min.ToString());
			}
			public bool IsMaxed()
			{
				return current >= max;
			}
			public bool Compatible(Bint bi)
			{
				if (bi.min > max || min > bi.max)
					return false;
				return true;
			}
			public void SetToPercentage(float f)
			{
				current = min + Mathf.RoundToInt((max-min) * f);
			}
		}
		[Serializable]
		public struct Bfloat //BaseFloat (max and min values)
		{
			public float current;
			public float max;
			public float min;
			public Bfloat(float value, float minimum = 0, bool currentIsZero = false)
			{
				max = value;
				min = minimum;
				current = (currentIsZero ? 0 : max);
			}
			public Bfloat(Bfloat bf)
			{
				current = bf.current;
				max = bf.max;
				min = bf.min;
			}
			public Bfloat (float minValue, float maxValue)
			{
				min = minValue;
				current = min;
				max = maxValue;
			}
			public void Fill()
			{
				current = max;
			}
			public void SetMax(float f, bool clampCurrent = true)
			{
				max = f;
				current = clampCurrent ? Mathf.Clamp(current, min, max) : current;
			}
			public void SetMin(float f = 0, bool clampCurrent = true)
			{
				min = f;
				current = clampCurrent ? Mathf.Clamp(current, min, max):current;
			}
			public void Add(float f,bool clampCurrent = true)
			{
				current = clampCurrent ? Mathf.Clamp(current + f, min, max):current;
			}
			public bool IsMaxed()
			{
				return current >= max;
			}

			public float Clamp(float value)
			{
				return Mathf.Clamp(value, min, max);
			}
			public bool Compatible(Bfloat bi)
			{
				if (bi.min > max || min > bi.max)
					return false;
				return true;
			}
			public void SetToPercentage(float f)
			{
				current = min + ((max - min) * f);
			}
			public float GetPercentage()
			{
				return current / (max - min);
			}
			public float CalculatePercentage(float percentage)
			{
				return percentage * (max - min);
			}
		}
		[Serializable]
		public enum Polarity { Neutral, Negative, Positive}
		public static bool ParseBool(string str, bool def = false)
		{
			if (str == "" || str == "FALSE")
				return false;
			else if (str == "TRUE")
				return true;
			else
			{
				Debug.LogError("Error parsing " + str + ". Returning FALSE");
				return false;
			}
		}
		public static int ParseInt(string str, int def = 0)
		{
			if (str == "") { return def; }
			else return int.Parse(str);
		}
		public static float ParseFloat(string str, int def = 0)
		{
			if (str == "") { return def; }
			else return float.Parse(str);
		}
		public static void SetCombatManager(CombatManager cm)
		{
			//Debug.Log("CombatManager Set");
			_combatManager = cm;
		}
		public static void SetCombatGUI(CombatGUI gui)
		{
			_combatGUI = gui;
		}
		public static void SetPostProcessingManager(PostProcessingManager ppm)
		{
			_postProcessingManager = ppm;
		}
		public static int MultiplyInt(int i, float multiplier) {
			float fi = i * multiplier;
			i = Mathf.RoundToInt(fi);
			return i;
		}
		public static Vector3 GetHSV(Color c)
		{
			Vector3 v3 = new Vector3();
			Color.RGBToHSV(c, out v3.x, out v3.y, out v3.z);
			return v3;
		}
		public static Texture2D textureFromSprite(Sprite sprite, Vector2 maxDimensions = new Vector2())
		{	//Untested whether maxdimension works!
			int height = (int)sprite.textureRect.width;
			int width = (int)sprite.textureRect.height;
			if (maxDimensions!=new Vector2())
			{
				float aspectRatio = width / height;
				if(maxDimensions.y!=0 && maxDimensions.y < height)
				{
					Mathf.Clamp(height, 0, maxDimensions.y);
					width = (int)(aspectRatio * height);
				}
				if (maxDimensions.x != 0 && maxDimensions.x < width)
				{
					Mathf.Clamp(width, 0, maxDimensions.x);
					height = (int)(width/aspectRatio);
				}
			}
			if (sprite.rect.width != sprite.texture.width)
			{
				Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
				Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
					(int)sprite.textureRect.y,
					width,
					height);
				newText.SetPixels(newColors);
				newText.Apply();
				return newText;
			}
			else
				return sprite.texture;
		}
		public static System.Object CopyEverything(System.Object from, System.Object to, Type typeB, bool dbug = false)
		{
			CopyBaseFields(from, to, typeB, dbug);
			CopyBaseProperties(from, to, typeB, dbug);
			return to;
		}
		public static System.Object CopyBaseFields(System.Object from, System.Object to, Type typeB, bool dbug = false)
		{
			if (dbug)
			{
				Debug.Log("-----------------------------------------------");
				Debug.Log("-----------------------------------------------");
			}
			int i = 0;
			foreach (FieldInfo fromField in from.GetType().GetFields())
			{
				//if (dbug) Debug.Log("From - " + fromField.Name);
				if (!fromField.IsPublic) //|| (field.GetIndexParameters().Length > 0))
				{
					if(dbug)Debug.Log("Error reading " + fromField.Name);
					continue;
				}
				if (typeB.GetField(fromField.Name) == null)
					continue;
				FieldInfo toField = typeB.GetField(fromField.Name);
				if ((toField != null) && (toField.IsPublic))
				{
					if (toField.FieldType.Equals(fromField.FieldType) == false)
					{
						if (dbug)
							Debug.Log("Mismatch in types " + fromField.Name + "(" + fromField.FieldType.Name + ") vs " + toField.Name + "(" + toField.FieldType.Name + ")");
						continue;
					}
					toField.SetValue(to, fromField.GetValue(from));
					if (dbug)
						i++;
				}
				else if (toField == null)
				{
					if(dbug)Debug.Log(toField.Name + " = null");
				}
				else if (!toField.IsPublic)
				{
					if(dbug)Debug.Log("Can't write " + toField.Name);
				}
			}
			if (dbug)
			{
				Debug.Log("TOTAL TRANSFERRED VARIABLES = " + i);
				Debug.Log("-----------------------------------------------");
				Debug.Log("-----------------------------------------------");
			}
			return to;
		}
		public static System.Object CopyBaseProperties(System.Object from, System.Object to, Type typeB, bool dbug = false)
		{
			if (dbug)
			{
				Debug.Log("-----------------------------------------------");
				Debug.Log("-----------------------------------------------");
			}
			int i = 0;
			foreach (PropertyInfo property in from.GetType().GetProperties())
			{
				if (!property.CanRead) //|| (Property.GetIndexParameters().Length > 0))
				{
					if(dbug)Debug.Log("Error reading " + property.Name);
					continue;
				}

				PropertyInfo other = typeB.GetProperty(property.Name);
				if ((other != null) && (other.CanWrite))
				{
					other.SetValue(to, property.GetValue(from));
					if (dbug)
					{
						Debug.Log(property.Name + " - " + property.GetValue(to));
						i++;
					}
				}
				else if (!other.CanWrite && dbug)
				{
					Debug.Log("Can't write " + other.Name);
				}
				else if (other == null && dbug)
				{
					Debug.Log(other.Name + " = null");
				}
			}
			if (dbug)
			{
				Debug.Log("TOTAL TRANSFERRED VARIABLES = " + i);
				Debug.Log("-----------------------------------------------");
				Debug.Log("-----------------------------------------------");
			}
			return to;
		}
		public static string RemoveLineEndings(this string value)
		{
			if (String.IsNullOrEmpty(value))
			{
				return value;
			}
			string lineSeparator = ((char)0x2028).ToString();
			string paragraphSeparator = ((char)0x2029).ToString();

			return value.Replace("\r\n", string.Empty)
						.Replace("\n", string.Empty)
						.Replace("\r", string.Empty)
						.Replace(lineSeparator, string.Empty)
						.Replace(paragraphSeparator, string.Empty);
		}
		public static Vector3 ClampVector3(Vector3 vtr, float min, float max)
		{
			vtr.x = Mathf.Clamp(vtr.x, min, max);
			vtr.y = Mathf.Clamp(vtr.y, min, max);
			vtr.z = Mathf.Clamp(vtr.z, min, max);
			return vtr;
		}
		public static string RemoveChars(string str, char c)
		{
			while (str.Contains(c))
			{
				str = str.Remove(str.IndexOf(c), 1);
			}
			return str;
		}
		public static void SetPivot(this RectTransform rectTransform, Vector2 pivot)
		{
			Vector3 deltaPosition = rectTransform.pivot - pivot;    // get change in pivot
			deltaPosition.Scale(rectTransform.rect.size);           // apply sizing
			deltaPosition.Scale(rectTransform.localScale);          // apply scaling
			deltaPosition = rectTransform.rotation * deltaPosition; // apply rotation

			rectTransform.pivot = pivot;                            // change the pivot
			rectTransform.localPosition -= deltaPosition;           // reverse the position change
		}
		public static string Plural(int count)
		{
			if (count > 1)
				return "s";
			else
				return "";
		}
		public static string CutString(string s, int length)
		{
			int previousSpaceIndex = s.Substring(0, length - 1).LastIndexOf(" ");
			return s.Substring(0, previousSpaceIndex)+" ...";
		}
		public static List<string> SliceString (string s, char slicer)
		{
			List<string> parameters = new List<string>();
			string temp = s;//.Substring(s.IndexOf(slicer) + 1);
			while (temp.Contains(slicer))
			{
				int spaceind = temp.IndexOf(slicer);
				//Debug.Log("spaceInd of " + temp + " = " + spaceind);
				if (spaceind > 0)
				{
					parameters.Add(temp.Substring(0, spaceind));
					if (temp.Length > spaceind + 1)
						temp = temp.Substring(spaceind + 1);
				}
				else
					temp = temp.Substring(1);
			}
			if (temp.Length > 0)
				parameters.Add(temp);
			int i = 0;
			/*Debug.Log("s = " + s);
		foreach (string str in parameters)
		{
			Debug.Log("Parameter" + i + " = " + str);
			i++;
		}*/
			return parameters;
		}
		public static float Average(List<float> values)
		{
			float sum = 0;
			foreach (float f in values)
				sum += f;
			float average = sum / values.Count;
			return average;
		}
		public static Vector2 GetRecttransformPivotPoint(RectTransform rt, Vector2 pivot, bool asScreen = true)
		{
			//(0,0) = bottom left, (1,1) = top right
			Vector2 pos = Vector2.zero;
			Vector3[] v = new Vector3[4];
			rt.GetWorldCorners(v);
			pos.x = Mathf.Lerp(v[1].x, v[2].x, pivot.x);
			pos.y = Mathf.Lerp(v[0].y, v[1].y, pivot.y);
			bool debug = false;
			if (debug) { 
				foreach (Vector3 ve in v) Debug.Log(ve);
				Debug.Log(pos);
			}
			if(asScreen)
				pos = Camera.main.WorldToScreenPoint(pos);
			return pos;
		}
		public static bool IsKeyClick(InputAction.CallbackContext callBackContext, bool OnDown = true)
		{
			//Use IsKeyClick for keyboard and IsMouseClick for mouse
			if (OnDown && callBackContext.started)
				return true;
			else if (!OnDown && callBackContext.canceled)
				return true;
			return false;
		}
		public static bool IsMouseClick(InputAction.CallbackContext callBackContext, bool OnDown = true)
		{
			//Use IsKeyClick for keyboard and IsMouseClick for mouse
			bool started = callBackContext.ReadValueAsButton();
			if (OnDown && started)
				return true;
			else if (!OnDown && !started)
				return true;
			return false;
		}
		public static string RemoveVowels(string str)
		{
			if (str.Length < 1)
				return str;
			bool upper = char.IsUpper(str[0]);
			string vowels = "aeiouyäöåáàéèûüíì";
			vowels += vowels.ToUpper();
			for(int i = str.Length - 1; i >= 0; i--)
			{
				if (vowels.Contains(str[i]))
					str = str.Remove(i, 1);
			}
			str = (upper ? str.Substring(0, 1).ToUpper() : str.Substring(0, 1).ToLower()) + str.Substring(1);
			return str;
		}
		public static string RemoveConsonants(string str)
		{
			if (str.Length < 1)
				return str;
			bool upper = char.IsUpper(str[0]);
			string consonants = "qwrtpsdfghjklzxcvbnm";
			consonants += consonants.ToUpper();
			for (int i = str.Length - 1; i >= 0; i--)
			{
				if (consonants.Contains(str[i]))
					str = str.Remove(i, 1);
			}
			str = (upper ? str.Substring(0, 1).ToUpper() : str.Substring(0, 1).ToLower()) + str.Substring(1);
			return str;
		}
		public static string Capitalize(string str,bool reverse = false)
		{
			//Debug.Log("Got " + str);
			int firstLetter = 0;
			char[] cstr = str.ToLower().ToCharArray();
			for (int i = 0; i < cstr.Length; i++)
			{
				if (Translator.vowels.Contains(cstr[i]) || Translator.consonants.Contains(cstr[i]))
					break;
				firstLetter++;
			}
			str = (firstLetter > 0 ? str.Substring(0, firstLetter) : "") + (!reverse ? str.Substring(firstLetter, 1).ToUpper() : str.Substring(firstLetter, 1).ToLower()) + (str.Length > firstLetter + 1 ? str.Substring(firstLetter + 1) : "");
			//Debug.Log("Returning " + str);
			return str;
		}
		public static bool TasksAreRunning(List<Task> tasks)
		{
			foreach (Task t in tasks)
				if (t.Running)
					return true;
			return false;
		}
		public static List<T> TrimNulls<T>(List<T> list)
		{
			if (list == null)
				return null;
			for(int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i] == null)
					list.RemoveAt(i);
			}
			return list;
		}
		public static class Tm
		{
			static float worldTime = 1;
			static float UITime = 1;
			static float globalTime = 1;
			static float delta;
			public static void UpdateDelta(float t)
			{
				delta = t;
			}
			public static float GetWorldDelta()
			{
				return worldTime * delta;
			}
			public static float GetUIDelta()
			{
				return UITime * delta;
			}
			public static float GetGlobalDelta()
			{
				return globalTime * delta;
			}
			public static void SetWorldTime(float t)
			{
				worldTime = t;
			}
			public static void SetUITime(float t)
			{
				UITime = t;
			}
			public static void SetGlobalTime(float t)
			{
				globalTime = t;
			}
		}
		/// <summary>
		/// Rotate transform and return it to the original position. Returns the new (unused) position.
		/// </summary>
		public static Vector3 RotateAroundWithoutMoving(Transform t, Vector3 point, float angles, Vector3 axis =default)
		{
			Vector3 oldPos = t.position;
			if (axis == default)
				axis = new Vector3(0, 0, 1);
			t.RotateAround(point, axis, angles);
			Vector3 newPos = t.position;
			t.position = oldPos;
			return newPos;
		}
		public static bool CreateDirectoryIfNotExist(string path)
		{
			if (System.IO.Directory.Exists(path) == false)
			{
				System.IO.Directory.CreateDirectory(path);
				return false;
			}
			return true;
		}
		public static void ListMove<T>(this List<T> list, int oldIndex, int newIndex)
		{
			var item = list[oldIndex];

			list.RemoveAt(oldIndex);

			if (newIndex > oldIndex) newIndex--;

			list.Insert(newIndex, item);
		}

		public static void SetLayer(Transform parent, int layer, bool includeChildren = true)
		{
			parent.gameObject.layer = layer;
			if (!includeChildren) return;
			Transform[] children = parent.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (var child in children)
			{
				//            Debug.Log(child.name);
				child.gameObject.layer = layer;
			}
		}
		/// <summary>
		/// Returns the position of B in the local space of A
		/// </summary>
		public static Vector3 GetPositionInLocalSpace(Transform A, Transform B)
		{
			Transform bParent = B.parent;
			int bindex = B.GetSiblingIndex();
			B.SetParent(A.parent);
			Vector3 pos = B.localPosition;
			B.SetParent(bParent);
			B.SetSiblingIndex(bindex);
			return pos;
		}
		public static Color GetRandomColor(bool whiteBackground = false)
		{
			float h = UnityEngine.Random.Range(0f, 1f);
			float v = whiteBackground ? UnityEngine.Random.Range(0f, 0.75f) : UnityEngine.Random.Range(0.25f, 1);
			float s = UnityEngine.Random.Range(0.15f, 0.9f);
			Color c = Color.HSVToRGB(h, s, v);
			return c;
		}
		public static Vector3 Vector3Divide(Vector3 a, Vector3 b)
		{
			Vector3 temp = new Vector3(); ;
			temp.x = (b.x != 0 ? a.x / b.x : a.x);
			temp.y = (b.y != 0 ? a.y / b.y : a.y);
			temp.z = (b.z != 0? a.z / b.z : a.z);
			return temp;
		}
		public static Vector3 Vector3Multiply(Vector3 a, Vector3 b)
		{
			Vector3 temp = new Vector3();
			temp.x = a.x * b.x;
			temp.y = a.y * b.y;
			temp.z = a.z*b.z;
			return temp;
		}
	}
	public static class UIAnimationTools
	{
		public static void ImageFadeIn(bool enabled, LerpColor lerpcolor, LerpTransform lerpTransform, float spd = 5, float startScale = 1.25f)
		{
			//Use 0 on startScale to FadeIn from nothing and >1 to fade in "from surroundings"
			if (enabled)
			{
				lerpcolor.StartLerpAlpha(1, 0, spd);
				lerpcolor._disableObjectAfterLerp = false;
				lerpTransform.transform.localScale = Vector3.one * startScale;
				lerpTransform.StartLerpScale(Vector3.one, spd);
			}
			else
			{
				lerpcolor.StartLerpAlpha(0, 1, spd);
				lerpTransform.transform.localScale = Vector3.one;
				lerpTransform.StartLerpScale(Vector3.one * startScale, spd);
			}
		}
		public static void SetCanvasGroupActive(CanvasGroup cg, bool active)
		{
			cg.alpha = active ? 1 : 0;
			cg.interactable = active;
			cg.blocksRaycasts = active;
		}
		public static IEnumerator FadeCanvasGroupAlpha(CanvasGroup cg,bool fadeIn, float speed = 1f, bool toggleInteractions = false,float scale = 1f)
		{
			if ((cg.alpha == 1 && fadeIn) || (cg.alpha == 0 && !fadeIn))
			{
				ToggleInteractions();
				yield break;
			}
			float startScale = cg.transform.localScale.x;
			float startAlpha = 0;
			float endAlpha = 1;
			if (toggleInteractions)
			{
				cg.interactable = !fadeIn;
				cg.blocksRaycasts = !fadeIn;
			}
			if (!fadeIn)
			{
				endAlpha = 0;
				startAlpha = 1;
			}
			float t = 0;
			while (t <= 1)
			{
				if (speed > 0) t += CodeTools.Tm.GetUIDelta() * speed;
				else t = 1;
				cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
				LerpScale();
				yield return null;
			}
			ToggleInteractions();
			void ToggleInteractions()
			{

				if (toggleInteractions)
				{
					cg.interactable = fadeIn;
					cg.blocksRaycasts = fadeIn;
				}
			}
			void LerpScale()
			{
				if (scale == 1f)
					return;
				float f = Mathf.Lerp(startScale, scale, t);
				cg.transform.localScale = Vector3.one * startScale * f;
			}
		}
		public static IEnumerator FadeText(TextMeshProUGUI text, bool fadeIn, float speed = 1f)
		{
			float startAlpha = fadeIn?0:1;
			float endAlpha = fadeIn?1:0;
			float t = 0;
			while (t <= 1)
			{
				t += CodeTools.Tm.GetUIDelta() * speed;
				Color c = text.color;
				c.a = Mathf.Lerp(startAlpha, endAlpha, t);
				text.color = c;
				yield return null;
			}
		}
		public static void TotalToggleCanvasGroup(CanvasGroup cg,bool enabled)
        {
			cg.alpha = enabled ? 1 : 0;
			cg.interactable = enabled;
			cg.blocksRaycasts = enabled;
		//	Debug.Break();
        }
		public static AnimationCurve DefaultCurve(AnimationCurve curve = null, bool linear = false)
		{
			if (curve == null)
				curve = new AnimationCurve();
			if (linear)
			{
				curve = AnimationCurve.Linear(0, 0, 1, 1);
			}
			else
			{
				for (int i = curve.keys.Length; i > 0; i--)
				{
					curve.RemoveKey(i);
				}
				curve.AddKey(new Keyframe(0, 0));
				curve.AddKey(new Keyframe(1, 1));
			}
			curve.postWrapMode = WrapMode.PingPong;
			curve.preWrapMode = WrapMode.PingPong;
			return curve;
		}
		public static void NoiseCurve(AnimationCurve curve, float randomScale)
		{
			Keyframe[] keys = curve.keys;
			for(int i = 0; i < keys.Length; i++)
			{
				Keyframe kf = keys[i];
				kf.weightedMode = WeightedMode.Both;
				kf.inWeight +=  UnityEngine.Random.Range(-randomScale, +randomScale);
				kf.outWeight += UnityEngine.Random.Range(-randomScale, +randomScale);
				kf.inTangent += UnityEngine.Random.Range(-randomScale, +randomScale);
				kf.outTangent += UnityEngine.Random.Range(-randomScale, +randomScale);
				keys[i] = kf;
			}
			curve.keys = keys;
		}
		public static IEnumerator LerpSizeDelta(RectTransform rt,Vector2 newSize, float time=0.5f)
        {
			if (rt.sizeDelta != newSize) {

				Vector2 originalSize = rt.sizeDelta;
				float spd = 1/time;
				float t = 0;
                while (t < 1)
                {
					t += CodeTools.Tm.GetUIDelta()*spd;
					rt.sizeDelta = Vector2.Lerp(originalSize, newSize, t);
					yield return null;
				}
			}
			rt.sizeDelta = newSize;
		}

		/// <summary>
		/// If image fill == 1, lerp it to 0. Otherwise lerp it to 1
		/// </summary>
		public static IEnumerator ImageTransitionFill(Image img,float time, float lerpBackDelay = -1, Sprite replaceSprite = default, bool lerpBackOpposite = true)
        {
			float t = time;
			bool unfill = (img.fillAmount == 1);
            while (t > 0)
            {
				t -= CodeTools.Tm.GetUIDelta();
				img.fillAmount = GetFillAmount();
				yield return null;
            }
			img.fillAmount = unfill?0:1;
            if (lerpBackDelay >= 0)
            {
                while (lerpBackDelay > 0)
                {
					lerpBackDelay-= CodeTools.Tm.GetUIDelta();
					yield return null;
				}
				if (replaceSprite!=default)img.sprite = replaceSprite;
				FlipOrigin();
				t = time;
				unfill = !unfill;
				while (t > 0)
				{
					t -= CodeTools.Tm.GetUIDelta();
					img.fillAmount = GetFillAmount();
					yield return null;
				}
				img.fillAmount = unfill ? 0 : 1;
				FlipOrigin();
			}

			void FlipOrigin()
            {
				if (!lerpBackOpposite)
					return;
				if (img.fillOrigin == 1)
					img.fillOrigin = 0;
				else
					img.fillOrigin = 1;
			}
			float GetFillAmount()
            {
				float f = Mathf.Clamp(unfill ? (t / time) : (1 - t / time),0,1);
				return f;
			}
        }
		public static IEnumerator ImageTransitionAlpha(Image img, float time, Sprite replaceSprite=default, float lerpBackDelay = -1)
        {
			Color origCol = img.color;
			bool fadeIn = origCol.a == 0;
			float lerpT = time;
            while (lerpT > 0)
            {
				lerpT -= CodeTools.Tm.GetUIDelta();
				SetAlpha(fadeIn ? 1 - lerpT / time: lerpT / time);
				yield return null;
            }
			if(replaceSprite!=default) img.sprite = replaceSprite;
            if (lerpBackDelay >= 0)
            {
				fadeIn = !fadeIn;
				lerpT = time;
				while (lerpT > 0)
				{
					lerpT -= CodeTools.Tm.GetUIDelta();
					SetAlpha(fadeIn ? 1 - lerpT / time : lerpT / time);
					yield return null;
				}
			}
			void SetAlpha(float a)
            {
				a = Mathf.Clamp(a, 0, 1);
				img.color = new Color(img.color.r, img.color.g, img.color.b, a);
            }
		}
	}
	public static class DialogTools
	{
		public static IEnumerator ShuffleStringForTime(float duration, float shuffleInterval, TextMeshProUGUI comp, string str, char spaceChar = ' ')
		{
			while (duration > 0 && comp != null)
			{
				float temp = shuffleInterval;
				while (temp > 0)
				{
					temp -= CodeTools.Tm.GetUIDelta();
					yield return null;
				}
				duration -= shuffleInterval - (temp * -1);
				comp.text = ShuffleString(str, spaceChar);
				yield return null;
			}
			comp.text = str;
		}
		public static string ShuffleString(string str,char spaceChar = ' ')
		{
			int spaceCount = CountCharsInString(str,spaceChar);
			List<string> words = new List<string>();
			string tempStr = str;
			for(int i = 0; i <= spaceCount; i++)
			{
				int pos = tempStr.IndexOf(spaceChar);
				if (pos > 0)
					words.Add(tempStr.Substring(0, pos));
				else
				{
					words.Add(tempStr);
					break;
				}
				if (pos < tempStr.Length)
					tempStr = tempStr.Substring(pos + 1);
				else
					break;
			}
			tempStr = "";
			Dbug();
			
			tempStr = tempStr.Substring(0, tempStr.Length - 1);
			return tempStr;

			void Dbug()
			{
				Debug.Log("--------");
				foreach (string s in words)
				{
					tempStr += ShuffleWord(s) + spaceChar;
					Debug.Log("'"+s+"'" + "->" + tempStr);
					//Debug.Log(s);
				}
			}
		}
		public static string ShuffleWord(string str, int count = -1)
		{
			if (str.Length < 2)
				return str;
			if (count == -1)
				count = str.Length;
			List<char> clist = new List<char>();
			List<char> charArray = new List<char>();
			charArray.AddRange(str.ToCharArray());
			while(count > 0)
			{
				count--;
				int i = UnityEngine.Random.Range(0, charArray.Count);

				clist.Add(charArray[i]);
				charArray.RemoveAt(i);
			}
			while (clist.Count > 0)
			{
				int it = UnityEngine.Random.Range(0, clist.Count);
				int i = UnityEngine.Random.Range(0, charArray.Count);
				charArray.Insert(i, clist[it]);
				clist.RemoveAt(it);
			}
			return new string(charArray.ToArray());
		}
		public static int CountCharsInString(string s,char letter)
		{
			int i = 0;
			foreach(char c in s.ToCharArray())
			{
				if (c == letter)
					i++;
			}
			return i;
		}
		public static IEnumerator WriteText(string line, TextMeshProUGUI _tm, float speed = 1f, AudioClip audioclip = default)
		{
			float spacePause = 0.05f;
			float _writingSpeed = 70;
			string orig = line;
			//Debug.Log("TotalTime = " + GetWriteTimeEstimates(reply));
			float wordDelta = (float)(1f / (float)_writingSpeed);
			float currentTime = 0;
			int length = 0;
			int prefLength = length;
			_tm.text = line;
			_tm.maxVisibleCharacters = length;
			bool textUpdated = false;
			char lastLetter = length >= 1 ?line.ToCharArray()[length - 1] : 'a';
			int lettersPerVocalization = 3;
			int lettersSinceLastVocalization = lettersPerVocalization;
			Vocalize();
			while (length < line.Length)
			{
		
				currentTime += CodeTools.Tm.GetUIDelta()*speed;
				while (currentTime > wordDelta)
				{
					Vocalize();
					currentTime -= wordDelta;
					length++;
					length = Mathf.Clamp(length, 0, line.Length);
					if (textUpdated) lastLetter = length >= 1 ? line.ToCharArray()[length - 1] : 'a';
				}
				_tm.maxVisibleCharacters = length;
				if (spacePause > 0 && lastLetter == ' ' && textUpdated)
				{
					float currentPause = -CodeTools.Tm.GetUIDelta();
					while (currentPause < spacePause)
					{
						currentPause += CodeTools.Tm.GetUIDelta();
						yield return null;
					}
					length++;
					lastLetter = line.ToCharArray()[length - 1];
				}
			
				textUpdated = true;
				yield return null;
			}
			_tm.maxVisibleCharacters = line.Length;

			void Vocalize()
			{
				lettersSinceLastVocalization++;
				if (lettersPerVocalization <= lettersSinceLastVocalization)
				{
					if (audioclip != default && audioclip != null)
					{
						AudioSource audioSource = _tm.gameObject.AddComponent<AudioSource>();
						audioSource.clip = audioclip;
						audioSource.pitch = UnityEngine.Random.Range(0.5f, 1.5f);
						audioSource.PlayOneShot(audioclip);
					}
					lettersSinceLastVocalization = 0;
				}
			}
		}

		public static float GetWriteTimeEstimate(string str, float writingSpeed = 70, float spacePause = 0.05f, float reactionTime = 0.5f)
		{
			int spaceCount = 0;
			foreach (char c in str.ToCharArray())
				if (c == ' ') spaceCount++;
			float wordDelta = 1 / writingSpeed;
			float totalTime = spaceCount * spacePause + ((str.Length - spaceCount) * wordDelta) + reactionTime;
			return totalTime;
		}
	}
	public static class CanvasPositioningExtensions
	{
		public static Vector3 WorldToCanvasPosition(this Canvas canvas, Vector3 worldPosition, Camera camera = null)
		{
			if (camera == null)
			{
				camera = Camera.main;
			}
			var viewportPosition = camera.WorldToViewportPoint(worldPosition);
			return canvas.ViewportToCanvasPosition(viewportPosition);
		}

		public static Vector3 ScreenToCanvasPosition(this Canvas canvas, Vector3 screenPosition)
		{
			var viewportPosition = new Vector3(screenPosition.x / Screen.width,
				screenPosition.y / Screen.height,
				0);
			return canvas.ViewportToCanvasPosition(viewportPosition);
		}

		public static Vector3 ViewportToCanvasPosition(this Canvas canvas, Vector3 viewportPosition)
		{
			var centerBasedViewPortPosition = viewportPosition - new Vector3(0.5f, 0.5f, 0);
			var canvasRect = canvas.GetComponent<RectTransform>();
			var scale = canvasRect.sizeDelta;
			return Vector3.Scale(centerBasedViewPortPosition, scale);
		}
	}
	public static class Translator
	{
		public static bool dbug = false;
		public static List<char> vowels = new List<char> { 'a', 'e', 'i', 'o', 'u', 'y', 'ä', 'å', 'ö' };
		public static List<char> consonants = new List<char> { 'q', 'w', 'r', 't', 'p', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x', 'c', 'v', 'b', 'n', 'm' };
		public static string _cardKeyword { get; private set; } = "<style=KeywordCard>";
		public static string _close { get;private set; } = "</style>";
		public static string TranslateString(string str)
		{
			if (dbug) Debug.Log("Reading " + str);
			string backup = str;
			while (str.Contains("{"))
			{
				int firstIndex = str.IndexOf("{");
				int separator = str.Substring(firstIndex).IndexOf(':') + firstIndex;
				int lastIndex = str.IndexOf("}");
				string toReplace = str.Substring(firstIndex, lastIndex + 1 - firstIndex);
				string replacement = "<style=LinkLight>" + str.Substring(separator + 1, lastIndex - 1 - separator) + "</style>";
				str = str.Replace(toReplace, replacement);
			}
			if (dbug) Debug.Log("Returning " + str);
			return str;
		}
		public static string DecodeName(string str, bool startwithcapital = false)
		{
			str = CodeTools.Capitalize(str,!startwithcapital);
			for(int i = 1; i < str.Length; i++)
			{
				char c = str.ToCharArray()[i];
				if (char.IsUpper(c))
				{
					str = str.Substring(0, i) + " " + char.ToLower(c) + str.Substring(i + 1);
					i++;
				}
			}
			return str;
		}
		public static string TranslateCases(string str, Aura a)
		{
			int stacks = a._cases;
			bool plural = a._cases > 1;
			if (str.Contains("[stack]"))
			{
				if (plural) {
					int startIndexAW = str.IndexOf("[stack]") + 8;
					int endIndexAW = str.Substring(startIndexAW).IndexOf(" ");
					string afterWord = str.Substring(startIndexAW, endIndexAW);
					Debug.Log("AfterWord = " + afterWord);
					str.Replace(afterWord, FormPlural(afterWord));
				}
				str = str.Replace("[stack]", stacks.ToString());
			}
			return str;
		}

		public static List<Hyperlink> GetHyperlinks(string str)
		{
			List<Hyperlink> _hyperlinks = new List<Hyperlink>();
			string backup = str;
			while (str.Contains("{"))
			{
				int firstIndex = str.IndexOf("{");
				string startString = str.Substring(0, firstIndex);
				string endString = str.Substring(firstIndex + 1, str.Length - firstIndex - 1);
				string hyperlinkID = str.Substring(firstIndex + 1,
					backup.Substring(firstIndex).IndexOf(':') - 1);
				str = startString + "H" + endString;
				_hyperlinks.Add((Hyperlink)CodeTools.db.Get<Hyperlink>(hyperlinkID));
			}
			return _hyperlinks;

		}
		public static string PrepGenericText(string str)
		{
			if (str == "")
			{
				return str;
			}
			str = DefaultReplace(str);
			return str;
		}
		public static string PrepCardText(string str, List<int>damages)
		{
			if (str == "")
			{
				return str;
			}
			//Debug.Log("Made it to Prep with damage "+damage);
			str = DefaultReplace(str);
			str =SetNumbersInDescription(str,null,null, damages);
			return str;
		}
		static string DefaultReplace(string s)
		{
			s = s.Replace("\\n", "\n");
			s = s.Replace("[", _cardKeyword);
			s = s.Replace("]", _close);
			return s;
		}
		public static string PrepCardText(string str, CombatGUICard c, CombatCharacter target = null)
		{
			if (str == "")
			{
				Debug.Log(c._card._name + " missing description");
				return str;
			}
			str = DefaultReplace(str);
			str = SetNumbersInDescription(str, c, target);
			return str;
		}
		public static List<string> _numberTags = new List<string> { "dmg", "number","value","aura" };
		static string SetNumbersInDescription(string str, CombatGUICard c=null, CombatCharacter target = null, List<int> finalNumbers = null)
		{
			int descriptionIndex = 0;
			foreach(string tag in _numberTags)
			{
				ReplaceTag(tag);
			}
			return str;
			void ReplaceTag(string tag)
			{
				string brackettag = "{" + tag;
				int count = StringOccurencesCount(str, brackettag);
				if (count <= 0)
					return;
				if (finalNumbers == null && c != null)
					c._card.SetDamageSlots(count);
				for (int i = 0; i < count; i++)
				{
					bool useInt = true;	//if false, use the (int)number instead of numberString when compiling the string
					string numberString = "";
					int number = 0;
					if (finalNumbers != null && finalNumbers.Count > i)
						number = finalNumbers[i];
					int firstIndex = str.IndexOf(brackettag);
					for (int ind = 0; ind < i; ind++)
					{
						firstIndex = str.IndexOf(brackettag, firstIndex);
					}
					int lastIndex = str.Substring(firstIndex).IndexOf("}");
					string completeTag = str.Substring(firstIndex + 1, lastIndex);
					string values = completeTag.Substring(tag.Length, completeTag.Length - tag.Length-1);
					int comma = values.IndexOf(",");
					int effectID = int.Parse(values.Substring(0, comma));
					comma++;
					float powerMultiplier = float.Parse(values.Substring(comma));
					if (tag == "dmg")
					{
						if (c != null) number = CalculateDamage(effectID, powerMultiplier, c, target);
					}
					else
					{
						BaseEffect be = (BaseEffect)CodeTools.db.Get<BaseEffect>(effectID);
						if (be != null)
						{
							if (tag == "aura")
								{
								useInt = false;
								numberString = GetAuraString(be, powerMultiplier);
								}
							else number = be.GetNumberQuick(powerMultiplier, c!=null?c.GetOwner():null);
						}
						else
							Debug.LogError("Invalid effectID " + effectID);
					}
					str = str.Substring(0, firstIndex) + (useInt?(_cardKeyword + number + _close):numberString) + str.Substring(firstIndex + lastIndex+1);
					descriptionIndex++;
				}
			}
			int CalculateDamage(int effectID, float powerMultiplier,CombatGUICard c, CombatCharacter target = null)
			{
				BaseEffect be = (BaseEffect)CodeTools.db.Get<BaseEffect>(effectID);
				int dmg = 0;
				if (be != null)
				{
					Damage damg = new Damage(be, c.GetOwner(), target, c._card, powerMultiplier);
					damg._descriptionIndex = descriptionIndex;
					dmg = damg.GetDamage();
				}
				return dmg;
			}
			string GetAuraString(BaseEffect be, float powerMultiplier)
			{
				BaseEffect.ApplyAura aa = be._applyAura;
				if (aa._aura != null)
				{
					int cases = (int)(powerMultiplier * aa._cases);
					string auraName = aa._aura._name.ToLower();
					string temp = auraName;
					if (cases > 1)
					{
						if (new string[] { "reaction" }.Contains(auraName))
							temp = _cardKeyword + cases + " " + temp+(temp.EndsWith("s") ? "es" : "s")+_close;
						else
							temp = _cardKeyword + cases + " "+ temp+_close;
					}
					else
						temp = _cardKeyword + temp + _close;
					if (aa._duration > 1)
						temp += " for " + _cardKeyword + aa._duration + _close + " turns";
					//Debug.Log("temp = "+temp);
					return temp;
				}
				else return "MISSING AURA";
			}
		}

		public static string AddStyleAndHyperlinks(string str, BaseObject bobj, BaseCollection baseCollection)
		{
			//List<string> keywordsTooltip = new List<string> {"deplete","fumble","leading","retaliating","wound","maneuver","melee","hold","repeating", "refresh","haste","slow"};
			List<string> keywordsHighlight = new List<string> { "superb" };
			for (int p = 0; p < str.Length; p++)
				if (str.ToCharArray()[p] == '[')
				{
					int firstIndex = p;
					int lastIndex = str.Substring(p).IndexOf("]") + p;
					string kw = str.Substring(firstIndex + 1, lastIndex - 1 - firstIndex);
					string kwl = kw.ToLower();
					string style = "<style=KeywordCard>";
					string prefix = "";
					string suffix = "";
					//Katso onko keyword Tooltip listassa, lisää hyperlink
					//Pistetään tää koko höskä kortti-editoriin, ei tarvii runnaa joka ikinen kerta läpi.
					Hyperlink hl = (Hyperlink)baseCollection.Get<Hyperlink>(kwl);
					bool match = false;
					if (hl != null)
					{
						bobj.AddLink(hl);
						style = "<style=KeywordTooltip>";
						match = true;
					}
					else
					{
						for (int i = 0; i < keywordsHighlight.Count; i++)
						{
							if (kwl == keywordsHighlight[i])
							{
								match = true;
								style = "<style=KeywordHighlight>";
								i = keywordsHighlight.Count;
							}
						}
					}
					if (match)
					{
						Debug.Log("replacing " + kw);
						string replacement = style + prefix + kw + suffix + "</style>";
						string toReplace = str.Substring(firstIndex, lastIndex - firstIndex + 1);
						str = str.Replace(toReplace, replacement);
					}
				}
			return str;
		}
		static string FormPlural(string str)
		{
			Debug.Log("Forming plural from "+str);
			string strLower = str.ToLower();
			char lastChar = strLower.ToCharArray()[strLower.Length-1];
			if (lastChar == 'y')
				str = str.Substring(0, str.Length - 1) + "ie";
			else if(new List<char> (){ 'a', 'i', 'o', 'u', 'ä', 'ö', 'å' }.Contains(lastChar))
			{
				str = str + "e";
			}
			str = str + "s";
			Debug.Log(str + " formed");
			return str;
		}
		static string FormPossessive(string str)
		{
			string strLower = str.ToLower();
			char lastChar = strLower.ToCharArray()[strLower.Length - 1];
			if (lastChar == 's')
				return str + "'";
			else
				return str + "'s";
		}

		/// <summary>
		/// Replace target tags with proper words
		/// [target]
		/// [targeto] -> objective case
		/// [target's]
		/// [target is]
		/// [target has]
		/// </summary>
		public static string TranslateTargetTag(string desc, BaseCard.TargetType tt, BaseCard.MultiTargetType multitargetType, bool secondMention = false, BaseCard.Target targetFlags = default)
		{
			bool self = tt == BaseCard.TargetType.Self;
			bool plural = multitargetType == BaseCard.MultiTargetType.One;
			if (GetTargetString(targetFlags) == "target" || self)
			{
				if (desc.Contains("[target]"))
					desc = desc.Replace("[target]", self ? "you" : secondMention ? "they" : Plural());
				if (desc.Contains("[targeto]"))
					desc = desc.Replace("[targeto]", self ? "yourself" : secondMention ? "them" : Plural());
				if (desc.Contains("[target's]"))
					desc = desc.Replace("[target's]", self ? "your" : secondMention ? "their" : !plural ? "the target's" : "the targets'");
				if (desc.Contains("[target is]"))
					desc = desc.Replace("[target is]", self ? "you are" : secondMention ? "they are" : !plural ? "the target is" : "the targets are");
				if (desc.Contains("[target has]"))
					desc = desc.Replace("[target has]", self ? "you have" : secondMention ? "they have" : !plural ? "the target has" : "the targets have");
				if (desc.Contains("[apply]"))
					desc = desc.Replace("[apply]", self ? "gain" : "apply");
			}
			else
			{
				string foe = GetTargetString(targetFlags);
				if (desc.Contains("[target]"))
					desc = desc.Replace("[target]", secondMention ? "they" : Plural(foe));
				if (desc.Contains("[targeto]"))
					desc = desc.Replace("[targeto]",  secondMention ? "them" : Plural(foe));

				if (plural)
					foe = FormPlural(foe);
				foe = (secondMention?"the ":"an ") + foe;
				if (desc.Contains("[target's]"))
					desc = desc.Replace("[target's]",  secondMention ? "their" : FormPossessive(foe));
				if (desc.Contains("[target is]"))
					desc = desc.Replace("[target is]",  secondMention ? "they are" : foe + (plural?" are":"is"));
				if (desc.Contains("[target has]"))
					desc = desc.Replace("[target has]",  secondMention ? "they have" : foe + (plural ? " have" : "has"));
				if (desc.Contains("[apply]"))
					desc = desc.Replace("[apply]", "apply");
			}
			return desc;

			string Plural(string tarString = "target")
			{
				switch (multitargetType)
				{
					default:
					case BaseCard.MultiTargetType.One:
						return Article(tarString)+tarString;
					case BaseCard.MultiTargetType.Two:
						return "two "+FormPlural(tarString);
					case BaseCard.MultiTargetType.Three:
						return "three " + FormPlural(tarString);
					case BaseCard.MultiTargetType.All:
						return "all " + FormPlural(tarString);

				}
			}
			string Article(string str)
			{
				if (secondMention)
					return "the ";
				else
				{
					string strLower = str.ToLower();
					char lastChar = strLower.ToCharArray()[strLower.Length - 1];
					if (vowels.Contains(lastChar))
						return "an ";
					else
						return "a ";
				}
			}
			string GetTargetString(BaseCard.Target target)
			{
				if ((int)target == 1)
					return "yourself";
				else if (target.HasFlag(BaseCard.Target.Enemy) && !target.HasFlag(BaseCard.Target.Friend))
					return "enemy";
				else if (!target.HasFlag(BaseCard.Target.Enemy) && target.HasFlag(BaseCard.Target.Friend))
					return "ally";
				else
					return "target";
			}
		}

		public static int StringOccurencesCount(this string wholestring, string substr)
		{
			int count = 0, index = wholestring.IndexOf(substr);
			while (index != -1)
			{
				count++;
				index = wholestring.IndexOf(substr, index + substr.Length);
			}
			return count;
		}
		public static List<BaseObject> GetPossibleLinksInString(string str)
		{
			int count = StringOccurencesCount(str, "[");
			int countB = StringOccurencesCount(str, "]");
			List<BaseObject> links = new List<BaseObject>();
			if (count != countB)
			{
				Debug.LogError("Uneven count of brackets!");
				return links;
			}
			BaseCollection bc = (BaseCollection)Resources.Load("Database/Collection");
			for (int i = 0; i < count; i++)
			{
				int firstIndex = str.IndexOf("[");
				for (int ind = 0; ind < i; ind++)
				{
					firstIndex = str.IndexOf("[", firstIndex+1);
				}
				int lastIndex = str.Substring(firstIndex).IndexOf("]");
				string tag = str.Substring(firstIndex+1, lastIndex-1);
				if (new List<string> { "strike", "shoot","host", "host's" }.Contains(tag.ToLower()))
					continue;
				Hyperlink hl = (Hyperlink)bc.Get<Hyperlink>(tag);
				if (hl != null && !links.Contains(hl))
					links.Add(hl);
				else if (hl == null)
				{
					BaseAura aura = (BaseAura)bc.Get<BaseAura>(tag);
					if (aura != null && !links.Contains(aura))
						links.Add(aura);
					else if (aura == null)
					{
						Debug.LogError("Could not find aura or hyperlink with the name -" + tag + "-");
					}
				}
			}
			return links;
		}
	}
	public static class DebugTools
	{
		public static void DebugCube(Vector3 position, float size = 0.15f, Color color = default,float time = 0)
		{
			if (color == default)
				color = Color.red;
			List<Vector3> points = new List<Vector3>();
			Vector3 temp = position;
			temp.x += size;
			temp.y += size;
			temp.z += size;
			float f = size * 2f;
			Add(); //+++
			temp.x -= f;
			Add(); //-++
			temp.y -= f;
			Add(); //--+
			temp.z -= f;
			Add(); //---
			temp.y += f;
			Add();//-+-
			temp.x += f;
			temp.y -= f;
			temp.z += f;
			Add();//+-+
			temp.z -= f;
			Add();//+--
			temp.y += f;
			Add();//++-
			for(int i = 0; i < points.Count; i++)
			{
				for (int y = 0; y < points.Count; y++)
				{
					if (i != y)
					{
						if(time<=0)
							Debug.DrawLine(points[i], points[y], color);
						else
							Debug.DrawLine(points[i], points[y], color,time);
					}
				}
			}
			void Add()
			{
				points.Add(temp);
			}
		}
	}
	public class CoroutineWithData
	{
		public Coroutine coroutine { get; private set; }
		public object result;
		private IEnumerator target;
		public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
		{
			this.target = target;
			this.coroutine = owner.StartCoroutine(Run());
		}

		private IEnumerator Run()
		{
			while (target.MoveNext())
			{
				result = target.Current;
				yield return result;
			}
		}
	}
}