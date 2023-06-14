using System.Collections.Generic;
using UnityEngine;

namespace RD.Combat
{
	public class HealthBarManager : MonoBehaviour
	{
		public static HealthBarManager _current;
		List<StatusBar> _statusBars = new List<StatusBar>();
		[SerializeField] GameObject _healthbarPrefab;
		List<GameObject> _multiTargetSymbols = new List<GameObject>();
		[SerializeField] GameObject _multiTargetPrefab;
		private void Awake()
		{
			_current = this;
		}
		public void AddHealthbar(CombatCharacter cc)
		{
			StatusBar sb = Instantiate(_healthbarPrefab, transform).GetComponent<StatusBar>();
			sb.Init(cc, this);
			sb.gameObject.SetActive(true);
			_statusBars.Add(sb);
			sb.transform.localScale = GetScale();
		}
		public void UpdateHealthbars(bool instant = true)
		{
			foreach(StatusBar hb in _statusBars)
			{
				hb.UpdateStats(instant);
			}
		}
		public void ToggleHealthbarText(CombatCharacter cc, bool enabled)
		{
			foreach(StatusBar hb in _statusBars)
			{
				if(hb._cc == cc)
				{
					Debug.Log("Match!");
					hb.ToggleText(enabled);
					return;
				}
			}
		}
		public void RemoveHealthbar(StatusBar hb)
		{
			_statusBars.Remove(hb);
		}
		public void AddMultiTargetSymbol(CombatCharacter ccA, CombatCharacter ccB)
        {
			Transform sbA = GetStatusBar(ccA).GetSelectionLines();
			Transform sbB = GetStatusBar(ccB).GetSelectionLines();

			if(sbA!=null && sbB != null)
            {
				GameObject mts = GameObject.Instantiate(_multiTargetPrefab, sbA);
				mts.GetComponent<RectTransform>().position = new Vector2((sbA.position.x + sbB.position.x) / 2, sbA.position.y);
				mts.GetComponent<RectTransform>().localPosition = new Vector2(mts.GetComponent<RectTransform>().localPosition.x, 0);
				_multiTargetSymbols.Add(mts);
            }
			StatusBar GetStatusBar(CombatCharacter cc)
            {
				foreach (StatusBar sb in _statusBars)
					if (sb._cc == cc)
						return sb;
				return null;
            }
        }
		public void ClearMultiTargetSymbols()
        {
			foreach (GameObject go in _multiTargetSymbols)
				Destroy(go);
			_multiTargetSymbols.Clear();
        }
		public void UpdateScale()
		{
			Vector3 scale = GetScale();
			foreach(StatusBar sb in _statusBars)
			{
				sb.transform.localScale = scale;
			}
		}
		Vector3 GetScale()
		{
			return Vector3.one * (5 / Cameras._camSize);
		}
	}
}
