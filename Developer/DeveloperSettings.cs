using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using static RD.CodeTools;
using RD.Combat;
using RD.DB;
using System;

namespace RD
{
	public class DeveloperSettings : MonoBehaviour
	{
		[SerializeField] TMP_InputField _inputField;
		public static DeveloperSettings _current;
		[SerializeField] Image _background;
		private void Awake()
		{
			_current = this;
		}
		public static bool _consoleEnabled = false;
		public static void GraveKey(InputAction.CallbackContext cbt)
		{
			if (CodeTools.IsKeyClick(cbt))
				_current.StartCoroutine(_current.ToggleConsole());
		}
		Task t;
		IEnumerator ToggleConsole()
		{
			if (t != null && t.Running)
				yield break;
			_consoleEnabled = !_consoleEnabled;
			Time.timeScale = 1;
			t = new Task(UIAnimationTools.FadeCanvasGroupAlpha(transform.GetChild(0).GetComponent<CanvasGroup>(), _consoleEnabled, 4, true));
			while (t.Running)
				yield return null;
			t = null;
			if (_consoleEnabled)
			{
				Time.timeScale = 0;
				_inputField.ActivateInputField();
			}
			else
			{
				_inputField.DeactivateInputField();
			}
		}
		[Serializable]public struct Command
		{
			public string name;
			public string description;
		}
		public List<Command> _commands;
		public void ReceiveCommand(string command)
		{
			if (command == "")
				return;
			bool match = false;
			for(int i = 0; i < _commands.Count; i++)
			{
				if(_commands[i].name.ToLower() == command.ToLower())
				{
					match = true;
					break;
				}
			}
			if (!match)
			{
				Debug.Log("Unrecognized command <color=red>" + command + "</color>");
				return;
			}
			switch (command)
			{
				default:
					Debug.Log("Unrecognized command <color=red>" + command + "</color>");
					break;
				case "injure":
					Injure();
					break;
			}
			void Injure()
			{
				CombatCharacter cc = _combatGUI._displayedCharacter;
				float currentPercentage = cc._stats.hp.GetPercentage();
				if (currentPercentage > 0.5f)
				{
					int dmg = cc._stats.hp.CalculatePercentage(currentPercentage - 0.5f);
					cc.TakeDamage(dmg, null, true, false);
				}
				else
				{
					cc._stats.hp.SetToPercentage(0.5f);
					cc.UpdateStats();
				}
			}
		}
	}
}
