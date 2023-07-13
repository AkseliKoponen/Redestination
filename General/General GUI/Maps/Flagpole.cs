using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RD.UI
{
	public class Flagpole : MonoBehaviour
	{
		RectTransform _rt;
		[SerializeField] Image _flag;
		Image _flagPole;
		Transform _originalParent;
		int _originalSindex;
		private void Awake()
		{
			_originalSindex = transform.GetSiblingIndex();
			_originalParent = transform.parent;
			_rt = GetComponent<RectTransform>();
			_flagPole = GetComponent<Image>();
		}
		public void SetFlagColor(Color c)
		{
			_flag.color = c;
		}
		public void SetPoleColor(Color c)
		{
			_flagPole.color = c;
		}
		public void StepAnchor()
		{
			_rt.anchorMin = _rt.anchorMax = new Vector2(1, 0.5f);
		}
		public void ResetAnchor()
		{

			_rt.anchorMax = _rt.anchorMin = Vector2.one * 0.5f;
		}
		public void Reparent()
		{
			transform.SetParent(_originalParent);
			transform.SetSiblingIndex(_originalSindex);
		}
	}
}