using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RD
{
	public static class Lerp 
	{
		public static Lerper PositionUI(RectTransform rt, Vector3 endPos, float time)
		{
			Lerper l = rt.gameObject.AddComponent<Lerper>();
			l.LerpMoveUI(endPos, time);
			return l;
		}
		public static Lerper Position(GameObject go, Vector3 endPos, float time)
		{
			Lerper l = go.AddComponent<Lerper>();
			l.LerpMove(endPos,time);
			return l;
		}
		public static void StopAll(GameObject go)
		{
			foreach(Lerper l in go.GetComponents<Lerper>())
			{
				l.Stop();
			}
		}
		public static void EndAll(GameObject go)
		{
			foreach (Lerper l in go.GetComponents<Lerper>())
			{
				l.End();
			}
		}
	}
	public class Lerper : MonoBehaviour
	{
		public bool _end = false;
		Task t;
		bool _local = false;
		Vector3 _endPos = default;
		public void End()
		{
			if (_endPos != default)
			{
				if (!_local) transform.position = _endPos;
				else transform.localPosition = _endPos;
			}
			t.Stop();
			Destroy(this);
		}
		public void Pause()
		{
			if (t != null)
				t.Pause();
		}
		public void Stop()
		{
			if (t!=null)
				t.Stop();
			Destroy(this);
		}
		public Task LerpMove( Vector3 endPos, float time)
		{
			_endPos = endPos;
			t = new Task(Mover());
			return t;
			IEnumerator Mover()
			{
				Vector3 startPos = transform.position;
				float t = 0;
				while (t < time && !_end)
				{
					t += CodeTools.Tm.GetUIDelta();
					transform.position = Vector3.Lerp(startPos, endPos, t / time);
					yield return null;
				}
				transform.position = endPos;
				Destroy(this);
			}
		}
		public Task LerpMoveUI(Vector2 endPos, float time)
		{
			RectTransform rt = GetComponent<RectTransform>();
			_local = true;
			_endPos = endPos;
			t = new Task(Mover());
			return t;
			IEnumerator Mover()
			{
				Vector3 startPos = rt.localPosition;
				//Debug.Log("Moving from " + startPos.ToString() + " to " + _endPos.ToString());
				//Debug.Log("world = " + endPos + "\nLocal = " + _endPos + "\nVector = " + rt.InverseTransformVector(endPos));
				float t = 0;
				while (t < time && !_end)
				{
					t += CodeTools.Tm.GetUIDelta();
					rt.localPosition = Vector3.Lerp(startPos, _endPos, t / time);
					yield return null;
				}
				transform.localPosition = _endPos;
				Destroy(this);
			}
		}
	}
}
