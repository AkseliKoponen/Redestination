using System.Collections;
using System.Collections.Generic;
using RD.Combat;
using RD.DB;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RD
{
	public class TooltipSystem : MonoBehaviour
	{
		public static TooltipSystem current;
		public int width;
		public int height;
		[SerializeField] Tooltip _tooltipItem;
		[SerializeField] Tooltip _lockedTooltip;
		List<Tooltip> _tooltips;
		RectTransform _rectTransform;
		List<RectTransform> _tooltipRows;
		public void Awake()
		{
			current = this;
			_rectTransform = GetComponent<RectTransform>();
			_tooltips = new List<Tooltip>();
			_tooltips.AddRange(GetComponentsInChildren<Tooltip>());
			_tooltipRows = new List<RectTransform>();
			foreach(Tooltip tt in _tooltips)
			{
				RectTransform rt = tt.transform.parent.GetComponent<RectTransform>();
				if (!_tooltipRows.Contains(rt))
					_tooltipRows.Add(rt);
			}
			HideTooltips();
		}

		#region HideTooltips
		public static void TotalReset()
		{
			foreach (Tooltip tt in current._tooltips)
				tt.Unlock();
			current._lockedTooltip.Unlock();
		}
		public static void HideAllTooltips()
		{
			current.StartCoroutine(HideManager(new Task(UIAnimationTools.FadeCanvasGroupAlpha(current.GetComponent<CanvasGroup>(), false, current.fadeSpeed))));
		}
		static bool hideAfterFade = false;
		static IEnumerator HideManager(Task t)
		{
			hideAfterFade = true;
			while (t.Running)
			{
				if (!hideAfterFade)
				{
					t.Stop();
					yield break;
				}
				yield return null;
			}
			if (hideAfterFade)
			{
				HideTooltips();
			}
		}
		static void HideTooltips(List<Tooltip> tts = default)
		{
			current.GetComponent<CanvasGroup>().alpha = 0;
			if (tts != default)
			{
				foreach (Tooltip tt in tts)
				{
					tt.Hide();
				}
				current._tooltipItem.gameObject.SetActive(false);
			}
			else
			{
				foreach (Tooltip tt in current._tooltips)
				{
					tt.Hide();
				}
			}
		}
		public static void UnlockTooltip()
		{
			current._lockedTooltip.Unlock();
		}
		#endregion
		#region ItemTooltips
		public static void DisplayTooltip(Item item, Vector2 position = default)
		{
			current._tooltipItem.Toggle(item);
			ShowItemTooltip(position);
		}
		public static void DisplayTooltip(Weapon item, Vector2 position = default)
		{
			current._tooltipItem.Toggle(item);
			ShowItemTooltip(position);
		}
		public static void DisplayTooltip(Equipment item, Vector2 position = default)
		{
			current._tooltipItem.Toggle(item);
			ShowItemTooltip(position);
		}
		public static void DisplayTooltip(Consumable item, Vector2 position = default)
		{
			current._tooltipItem.Toggle(item);
			ShowItemTooltip(position);
		}
		static void ShowItemTooltip(Vector2 position = default)
		{
			HideTooltips();
			if (position == default)
				position = Mouse.current.position.ReadValue();
			Tooltip tt = current._tooltipItem;
			tt.gameObject.SetActive(true);
			Repivot(tt);
			Vector3 pos = Camera.main.ScreenToWorldPoint(position);
			pos.z = 0;
			tt.transform.position = pos;
			hideAfterFade = false;
			current.StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(current.GetComponent<CanvasGroup>(), true, current.fadeSpeed));
			//current.StartCoroutine(current.FadeCanvasGroupAlpha(true));
		}
		#endregion
		#region Tooltip Display
		Vector2 _anchorPosition;
		public static void DisplayTooltip(DialogCharacter dc, Vector2 position = default, Vector2? anchor = null)
		{
			DisplayTooltip(dc.GenerateHyperlink(), position,anchor);

		}
		public static void DisplayTooltip(BaseObject link, Vector2 position = default, Vector2? anchor = null)
		{
			DisplayTooltips(new List<BaseObject> { link}, position,anchor);
		}
		public static Tooltip LockTooltip(BaseObject link, Vector2 anchoredPosition, Vector2? anchor = null)
		{
			List<BaseObject> h = new List<BaseObject>();
			h.Add(link);
			HideTooltips();
			Tooltip tt = current._lockedTooltip;
			//Debug.Log(tt.gameObject.name);
			tt.SetMaxLines(-1);
			tt.Toggle(link);
			
			tt.GetComponent<RectTransform>().SetPivot(anchor == null ?new Vector2(0.5f, 0.5f) : (Vector2)anchor);
			//Debug.Log(anchoredPosition);
			tt.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			//tt.GetComponent<RectTransform>().localPosition = position;
			//current.UpdatePosition();
			//current.StartCoroutine(current.RefreshAfterFrame());
			return tt;
		}
		public static void DisplayTooltips(List<BaseObject> links, Vector2 position = default, Vector2? anchor = null, bool instant = false)
		{
			if (position == default)
				position = Mouse.current.position.ReadValue();
			current._anchorPosition = Camera.main.ScreenToWorldPoint(position);
			Vector2 parentPivot = links.Count == 1 ? new Vector2(0.5f, 0f):new Vector2(0.5f,1f);
			foreach(RectTransform rt in current._tooltipRows)
			{
				rt.pivot = parentPivot;
			}
			HideTooltips();
			hideAfterFade = false;
			//Reposition on screen!
			if (links.Count <= 2)
			{
				VerticalTooltips();
			}
			else
			{
				float spaceX = Screen.width - position.x;
				float spaceY = Screen.height - position.y;
				float ratio = spaceX / spaceY; // <0.5 means prioritize Columns, >2 means prioritize rows
				if (ratio < 0.5f || spaceX<500f)
				{
					VerticalTooltips();
				}
				else if(ratio >0.5f && ratio < 2)
				{
					if (links.Count < 10)
						SquareTooltips();
					else
						VerticalTooltips();
				}
				else
				{
					HorizontalTooltips();
				}
				//Debug.Log(position);
				Vector2 pivot = new Vector2(0, 0.5f);// anchor==default?new Vector2(0, 0.5f):anchor;
				//Debug.Log("Pivot = " + pivot);
				Vector2 sdEstim = GetSizeDeltaEstimation();
				if (position.x + sdEstim.x > Screen.width)
				{
					//Debug.Log("Too wide, changing pivot");
					pivot.x = position.x + sdEstim.x / Screen.width;
				}
				if (position.y - sdEstim.y / 2f < 0 || position.y + sdEstim.y / 2f > Screen.height)
				{
					//Debug.Log("Too tall/low, changing pivot");
					pivot.y = position.y - sdEstim.y / Screen.height;
				}
				current._rectTransform.pivot = pivot;
			}
			//current.UpdatePosition();
			current.StartCoroutine(current.RefreshAfterFrame(instant));
			void VerticalTooltips()
			{
				float pivotY = anchor==null?position.y / Screen.height:((Vector2)anchor).y;
				current._rectTransform.pivot = new Vector2(position.x / current.transform.parent.GetComponent<RectTransform>().rect.width > 0.80f?1:0, pivotY);

				for (int i = 0; i < links.Count; i++)
				{
					current._tooltips[i].Toggle(links[i]);
				}

			}
			void SquareTooltips()
			{
				//Debug.Log("SquareTT");
				List<Tooltip> _tooltips = new List<Tooltip>();
				int side = 3;
				int count = links.Count;
				if (count <= 4)
					side = 2;
				List<Tooltip> tooltips = new List<Tooltip>();
				for (int column = 0; column < side && count > 0; column++)
				{
					for(int row = 0; row < side && count > 0; row++)
					{
						count--;
						tooltips.Add(current._tooltips[(column*4) + row]);
						//Debug.Log("column = " + column + "\nrow = " + row);
					}
				}
				for (int i = 0; i < tooltips.Count; i++)
				{
					tooltips[i].Toggle(links[i]);
				}

			}
			void HorizontalTooltips()
			{
				//Debug.Log("HorizontalTooltips!");
				List<Tooltip> tooltips = new List<Tooltip>();
				int count = links.Count;
				for (int row = 0; count > 0 && row > current.height; row++)
				{
					for (int column = 0; column <= current.width && count > 0; column++)
					{
						tooltips.Add(current._tooltips[column * current.height + row]);
						count--;
					}
				}
				for (int i = 0; i < tooltips.Count; i++)
				{
					tooltips[i].Toggle(links[i]);
				}
			}
			Vector2 GetSizeDeltaEstimation()
			{
				Vector2 sizeDelta = new Vector2();
				int xtooltips = 0;
				int ytooltips = 0;
				foreach(Transform t in current.transform)
				{
					bool rowActive = false;
					int ytotal = 0;
					foreach(Transform ct in t)
					{
						if (ct.gameObject.activeSelf)
						{
							rowActive = true;
							ytotal++;
						}
					}
					if (rowActive)
						xtooltips++;
					if (ytotal > ytooltips)
						ytooltips = ytotal;
				}
				sizeDelta.x = xtooltips * current.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
				sizeDelta.y = ytooltips * 100;
				return sizeDelta;
			}
		}
		public static void Repivot(Tooltip tt)
		{

			Vector3 origpos = tt.transform.position;
			Vector2 screenpos = Camera.main.ScreenToWorldPoint(origpos);
			if (tt.GetWidth() > Screen.width-screenpos.x|| tt.GetHeight()/2 > Screen.height-screenpos.y)
			{

				tt.GetComponent<RectTransform>().SetPivot(new Vector2(tt.transform.position.x / Screen.width, tt.transform.position.y / Screen.height));
			}
			else
			{
				tt.GetComponent<RectTransform>().SetPivot(new Vector2(0,0.5f));
			}
			tt.transform.position = origpos;
		}
		float fadeSpeed = 10f;
		IEnumerator RefreshAfterFrame(bool instant)
		{
			//Debug.Break();
			int frame = 1;
			while (frame > 0)
			{
				frame--;
				yield return null;
			}
			current.UpdatePosition();
			if (!instant) current.StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(current.GetComponent<CanvasGroup>(), true, current.fadeSpeed));
			else current.GetComponent<CanvasGroup>().alpha = 1;
			//Debug.Break();
		}
		public static void Refresh()
		{
			current.UpdatePosition();
		}
		void UpdatePosition()
		{
			current.transform.position = current._anchorPosition;
			current.transform.position = CodeTools.GetRecttransformPivotPoint(current._rectTransform, current._rectTransform.pivot,false);
			LayoutRebuilder.ForceRebuildLayoutImmediate(current._rectTransform);
		}
		#endregion
		#region TooltipLock
		public static void LockTooltip()
		{
			Tooltip tt = current._tooltips[0];
			if (tt.gameObject.activeSelf)
			{
				Debug.Log("Would Display More about " + tt.GetName());
			}
		}
		#endregion
	}
}
