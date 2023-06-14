using System.Collections.Generic;
using RD;
using UnityEngine;
using static RD.CodeTools;

namespace RD.Combat
{
	public static class HighlightManager
	{
		public static List<CombatCharacterVisuals> _highlighted { get; private set; }
		public static void NewAttack(List<CombatCharacter> highlightedObjects)
		{
			_highlighted = new List<CombatCharacterVisuals>();
			foreach(CombatCharacter co in highlightedObjects)
			{
				if (co._cSprite != null)
					_highlighted.Add(co._cSprite);
			}
		}

		public static void Zoom(float speed = 0.5f, bool unzoom = false)
		{
			for(int i = _highlighted.Count - 1; i >= 0; i--)
			{
				if (_highlighted[i] == null)
					_highlighted.RemoveAt(i);
				else
				{
					_highlighted[i].StartLerpScale((unzoom ? 1 : 1.5f), speed, unzoom);
				}
			}
			
		}

		public static void FlashColor(Color? color, float duration = 0f, bool flashCamera = true)
		{
			if (_highlighted.Count <= 0)
				return;
			Color c = color ?? Color.black;
			foreach (CombatCharacterVisuals ccs in _highlighted) {
				_combatGUI.StartCoroutine(ccs.FlashColor(c, duration));
			}
			_combatGUI.StartCoroutine(Cameras.FlashHighlight(CodeTools.NegativeColor(c), duration));
		}
	}
}
