using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RD.Combat
{
	public class Graveyard : MonoBehaviour
	{
		public static Graveyard _current;
		public AutoDelete _autoDelete;
		[System.Serializable]
		public struct AutoDelete
		{
			public bool _combatCharacter;
			public bool _combatGUICard;
		}
		List<GameObject> _corpses;
		private void Awake()
		{
			_current = this;
			_corpses = new List<GameObject>();
		}
		public static void Remove(GameObject go)
		{
			_current._corpses.Add(go);
			_current.Delete(go);
			go.transform.SetParent(_current.transform);
			go.gameObject.SetActive(false);
		}
		public void Clear()
		{
			for(int i = _corpses.Count - 1; i >= 0; i--)
			{
				Destroy(_corpses[i]);
				_corpses.RemoveAt(i);
			}
		}
		public static GameObject GetByName(string name)
		{
			foreach (GameObject go in _current._corpses)
				if (go.name.Equals(name))
					return go;
			Debug.Log(name + " not found in graveyard.");
			return null;
		}
		void Delete(GameObject go)
		{
			if (
				(_autoDelete._combatCharacter && go.GetComponent<CombatCharacter>())
				|| (_autoDelete._combatGUICard && go.GetComponent<CombatGUICard>())
				)
				{
					_current.StartCoroutine(_current.DestroyAfterTime(go, 2));
				}
		}
		IEnumerator DestroyAfterTime(GameObject go,float time = 2)
		{
			while (time > 0)
			{
				time -= CodeTools.Tm.GetWorldDelta();
				yield return null;
			}
			Destroy(go);
			_corpses.Remove(go);
		}
	}
}
