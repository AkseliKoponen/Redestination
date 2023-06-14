using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RD.UI
{
	[RequireComponent(typeof(Button))]
	public class MenuButton : MonoBehaviour
	{
		public Image _blocker;
		public void Toggle(bool enabled)
		{
			GetComponent<Button>().interactable = enabled;
			_blocker.gameObject.SetActive(!enabled);

		}
	}
}
