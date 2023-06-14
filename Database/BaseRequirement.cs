using RD.Combat;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static RD.CodeTools;

namespace RD.DB
{

	public class BaseRequirement : BaseObject
	{
		public bool _invertTrue = false;
		[Tooltip("If checking versus multiple targets, require all of them to pass.\n If false require only one of them to pass.")]
		public bool _requireAND = false;
		//----------------Cards--------------------------------
		[Tooltip("Amount of cards in hand")]
		public Bint _handSize;
		[Tooltip("Amount of cards in draw deck")]
		public Bint _drawSize;
		[Tooltip("Amount of cards in discard deck")]
		public Bint _discardSize;
		[Tooltip("Amount of cards in depleted")]
		public Bint _depleteSize;
		//----------------Damage-------------------------------
		[Tooltip("Health < 50%")]
		public bool _injured;
		[Tooltip("Health = 100%")]
		public bool _unharmed;
		[Tooltip("Has taken atleast this much damage this combat")]
		public int _damageTaken;
		//------------------Effects-------------
		public BaseAura _auraRequired = null;       //UNCOMMENT CHECKAURAS

		[Tooltip("How many times you were hit last turn.")]
		public bool _retaliation;    //if you were hit last turn by the target
		[Tooltip("First card played.")]
		public bool _leading; //first card played
		[Tooltip("Card kills the target. Requirement has to be AFTER a damage dealing effect.")]
		public bool _lethal; //card kills target
							 //---------------Positioning---------------------------
		[Tooltip("First character in arena.")]
		public bool _melee = false;
		[Tooltip("Last character in arena.")]
		public bool _last = false;
		[Tooltip("Not melee and not last.")]
		public bool _middle = false;
		/*
		[Tooltip("Surrounded by enemies.")]
		public bool _flanked;
		[Tooltip("No allies adjacent.")]
		public bool _alone;*/
		//----------------Misc--------------------------
		public int _turnCount;
		public BaseRequirement()
		{
			_handSize = new Bint(-1, -1);
			_drawSize = new Bint(-1, -1);
			_discardSize = new Bint(-1, -1);
			_depleteSize = new Bint(-1, -1);
			_layOutSpace = new LayOutSpace(new List<int> { 3, 4, 3, 4, 3 }, new List<string> { "Cards", "Damage", "Effects", "Position", "Misc" });
		}
		/// <summary>
		/// Return true if requirements are fulfilled, false if not
		/// </summary>
		public bool InstantCheck(CombatCharacter target)
		{
			if (_requireSpecialCode)
			{
				switch (_id)
				{
					default:
						break;
					case 20:
					case 21:
						//Is your Turn
						return _invertTrue != target._trackers._myTurn;
					case 24:
						//024 - Missing Actions.
						return target._actions.IsMaxed() == false;

				}

			}
			bool b = InstantCheck(new List<CombatCharacter> { target });
			return b;
		}
		/// <summary>
		/// Return true if requirements are fulfilled, false if not
		/// </summary>
		public bool InstantCheck(List<CombatCharacter> targets)
		{
			bool temp=true;
			foreach (CombatCharacter target in targets)
			{
				temp = CheckTarget(target);

				if ((temp == false && _requireAND) || (temp == true && !_requireAND))
					break;
			}
			Dbug(temp);
			return temp; //(_invertTrue ? !temp : temp); //_invert true has been moved to CheckTarget

			bool CheckTarget(CombatCharacter target)
			{
				bool hasContent = false;
				bool pass = true;
				#region Cards
				Bint bi = _handSize;
				if (bi.max > -1)
				{
					hasContent = true;
					if (target._deck._mind._ideas.Count > bi.max) pass = false;
				}
				if (bi.min > 0)
				{
					hasContent = true;
					if (target._deck._mind._ideas.Count < bi.min)
						pass = false;
				}
				bi = _drawSize;
				if (bi.max > -1)
				{
					hasContent = true;
					if (target._deck._subcon._ideas.Count > bi.max) pass = false;
				}
				if (bi.min > 0)
				{
					hasContent = true;
					if (target._deck._subcon._ideas.Count < bi.min) pass = false;
				}
				bi = _discardSize;
				if (bi.max > -1)
				{
					hasContent = true;
					if (target._deck._forget._ideas.Count > bi.max)
						pass = false;
				}
				if (bi.min > 0)
				{
					hasContent = true;
					if (target._deck._forget._ideas.Count < bi.min)
						pass = false;
				}
				#endregion
				#region Damage
				if (_injured)
				{
					hasContent = true;
					if (!target._statusFlags._injured) pass = false;
				}
				if (_unharmed)
				{
					hasContent = true;
					if (!target._stats.hp.IsMaxed()) pass = false;
				}
				if (_damageTaken > 0)
				{
					hasContent = true;
					if (_damageTaken < target._trackers._damageTaken)
						pass = false;
				}
				#endregion
				#region Effects
				if (_auraRequired != null)
				{
					hasContent = true;
					pass = target.HasAura(_auraRequired);
				}
				if (_leading)
				{
					hasContent = true;
					if (target._trackers._cardsPlayed > 0) pass = false;
				}
				#endregion
				#region Positioning
				if (_melee)
				{
					hasContent = true;
					if (target != CombatManager.ArenaManager.GetContainingArena(target).GetMeleeCharacter()) pass = false;
				}
				if (_last)
				{
					hasContent = true;
					if (target != CombatManager.ArenaManager.GetContainingArena(target).GetLastCharacter()) pass = false;
				}
				if (_middle)
				{
					hasContent = true;
					if (target != CombatManager.ArenaManager.GetContainingArena(target).GetLastCharacter() && target != CombatManager.ArenaManager.GetContainingArena(target).GetMeleeCharacter())
						pass = false;
				}
				#endregion
				if (hasContent && _invertTrue)
					return !pass;
				return pass;
			}
		}
		public bool RetaliationCheck (Damage dmg)
		{
			return RetaliationCheck(dmg._target, dmg._source);
		}
		public bool RetaliationCheck(CombatCharacter target, CombatCharacter source)
		{
			if (target == null || source==null)
				return false;
			bool b = true;
			if (_retaliation)
			{
				if (source._trackers._retaliationTargets.Contains(target))
					b = !_invertTrue;
				else
					b = _invertTrue;
			}

			Dbug(b,target);
			//Debug.Log("RetaliationCheck on " + GetFileName() + " result = "+b);
			return b;
		}
		public bool RepeatCheck(Card playedCard)
		{
			bool b = playedCard._onRepeat;
			Dbug(b);
			return b;
		}
		public bool HardCodeCheck(Damage dmg)
		{
			switch (_id)
			{
				default:
					Debug.LogError("Uncoded HardCode at " + GetFileName());
					return true;

			}
		}
		public int delayCheckState = 0;
		public IEnumerator DelayedCheck(CombatCharacter target, Card card)
		{
			delayCheckState = 0;
			bool checksOut = true;
			bool temp = true;
			if (!InstantCheck(target))
				temp = false;
			if (!temp)
			{
				checksOut = false;
			}
			else
			{
				if (_lethal)
				{
					temp = false;
					//If any of the targets are dying, then true
					Task t = new Task(LethalChecker());
					while (t.Running)
						yield return null;
					checksOut = temp == false ? temp : checksOut;

				}
			}
			delayCheckState = checksOut ? 1 : -1;
			IEnumerator LethalChecker()
			{
				if (target != null && !target._IsDying)
				{
					if (target.prepareIncomingDamageTask != null)
					{
						//Debug.Log("target = "+target.GetName());
						while (target.prepareIncomingDamageTask != null && target.prepareIncomingDamageTask.Running)
						{
							yield return null;
						}
						if (target == null || target._IsDying)
							temp = true;
					}
				}
				else
					temp = true;
			}
			//card.CollectResult(this, checksOut ? 1 : -1);
		}

		public bool IsInstant()
		{
			bool isInstant = true;
			if (_lethal) isInstant = false;
			return isInstant;
		}

		/// <summary>
		/// Returns true if the requirements can not be true at the same time. Else returns false.
		/// </summary>
		public bool IsOppositeOf(BaseRequirement br)
		{
			if (br == null)
				return false;
			bool inversion = br._invertTrue != _invertTrue;
			foreach (FieldInfo fi in GetType().GetFields())
			{
				switch (fi.Name)
				{
					case "_invertTrue":
					case "_name":
					case "_id":
					case "_requireSpecialCode":
					case "_description":
					case "_actualDescription":
					case "_hyperlinks":
					case "_delayCheckState":
						break;
					case "_auraRequired":
						if (_auraRequired == br._auraRequired && !inversion)
							return false;
						break;
					default:
						if (Compare(fi.GetValue(br), fi.GetValue(this)))
							return false;
						break;
				}
			}
			if (Compare(br._unharmed, _injured) || Compare(br._injured, _unharmed))
			{
				return false;
			}
			return true;
			bool Compare(object b1, object b2, bool invert = default)
			{
				if (invert == default)
					invert = inversion;
				if (b1.GetType() == typeof(Bint))
				{
					Bint ba = (Bint)b1;
					Bint bb = (Bint)b2;
					return ba.Compatible(bb) != invert;
				}
				else if (b1.GetType() == typeof(Bfloat))
				{
					Bfloat ba = (Bfloat)b1;
					Bfloat bb = (Bfloat)b2;
					return ba.Compatible(bb) != invert;
				}
				else
				{
					return b1 == b2 == invert;
				}
			}
		}
		public void Dbug(bool result, CombatCharacter target = null)
		{
			if(_debugLog)
				Debug.Log(GetFileName() + " is returning " + result +(target!=null?" on "+target:""));
		}
	}
}
