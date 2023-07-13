using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RD.UI
{
	public class Road : MonoBehaviour
	{
		public List<Road> _connections = new List<Road>();

#if UNITY_EDITOR
		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			foreach (Road r in _connections)
			{
				Gizmos.DrawLine(transform.position, r.transform.position);
			}
		}
#endif
	}
}
