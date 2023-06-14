using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RD.CodeTools;

namespace RD.Combat
{
	public class ActionDisplay : MonoBehaviour
	{
		CombatCharacter _cc;
		public TextMeshProUGUI _name;
		public CombatGUIActionOrb _actionOrb;
		public void DisplayCharacter(CombatCharacter cc)
		{
			_cc = cc;
			_actionOrb.SetCharacter(_cc);
			UpdateCharacter();
		}

		public void UpdateCharacter()
		{
			_name.text = _cc.GetName();
		}
	}
}
