using JetBrains.Annotations;
using RD.Combat;
using RD.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static RD.CodeTools;

namespace RD.DB
{
	public class BaseThought : BaseObject
	{
		[Serializable] public enum ThoughtType { Behaviour, Thought, Emotion, None }

		[System.Flags]
		[Serializable]
		public enum ThoughtClass
		{
			Soldier = 1,
			Spy = 2,
			Inventor = 4,
			Willon = 8,
			Mardrak = 16,
			NPC = 32,
			Generic = 64
		}
		/// <summary>
		///  One, Two, Three, All 
		/// </summary>
		[Serializable] public enum MultiTargetType { One, Two, Three, All }

		[System.Flags]
		[Serializable]
		public enum Target
		{
			Self = 1,
			Friend = 2,
			Enemy = 4,
			Object = 8,
			Ground = 16
		}

		public ThoughtType _thoughtType = ThoughtType.Behaviour;
		public ThoughtClass _thoughtClass;
		public string _nameCombinationNoun;
		public string _nameCombinationAdjective;
		//--------Targeting
		public Target _target;
		public bool _meleeRange = false;
		public bool _requireLineOfSight;
		public bool _randomTarget;
		public MultiTargetType _multiTarget;
		//---------------Behavior (Active) Effects------------
		public List<Behaviour> _effects = new List<Behaviour>();
		public List<CharacterRequirement> _requirements = new List<CharacterRequirement>();
		public int _cooldown = 0;
		public bool _free = false;
		//------------Thought/Emotion (Passive Effects)----------------//Passive effects
		public List<BaseTalent> _charBonus = new List<BaseTalent>();
		public List<ThoughtModification> _thoughtModifications = new List<ThoughtModification>();
		
		//-----------ART-------------------------------//
		public Sprite _artFillPattern;
		public bool _halfSize;
		public Color _color;
		public ThoughtUI.ThoughtShape _shape;
		public BaseFX _baseFX;
		public BaseThought()
		{
			_layOutSpace = new LayOutSpace(new List<int> { 5, 9, 2, 5 }, new List<string> { "Behaviour", "Thought", "Art", "General" });
		}

		[System.Flags]
		[Serializable]
		public enum ThoughtPosition
		{
			Adjacent = 15,
			Up = 1,
			Down = 2,
			Left = 4,
			Right = 8,
			Row = 16,
			Column = 32
		}
		/// <summary>
		/// Instructions for how to modify other thoughts.
		/// </summary>
		[Serializable]
		public class ThoughtModification
		{
			[Tooltip("Which thoughts to target.")]
			public ThoughtPosition _position;
			public List<BaseEffect> _effects = new List<BaseEffect>();
			public float _powerMultiplier = 1f;
			[Tooltip("This Thought will not function if there is not a matching thought in target position.\n Set as None to disable requirement.")]
			public ThoughtType _reqType;
			public ThoughtModification()
			{
				_effects = new List<BaseEffect>();
				_position = ThoughtPosition.Adjacent;
				_powerMultiplier = 1f;
				_reqType = ThoughtType.None;
			}
		}
		/// <summary>
		/// Target/Self
		/// </summary>
		[Serializable]
		public enum TargetType { Target, Self }
		/// <summary>
		/// Instructions for an action, including the effect and its power and what it targets and requires.
		/// </summary>
		[Serializable]
		public struct Behaviour
		{
			public BaseEffect _effect;
			public TargetType _target;
			public float _powerMultiplier;
			public CharacterRequirement _requirement;
			public bool _linkToPast;
			public Behaviour(bool tru)
			{
				_effect = null;
				_target = TargetType.Target;
				_powerMultiplier = 1f;
				_requirement = new CharacterRequirement(tru);
				_linkToPast = false;
			}
			public string GetDescription(string conditional = "if", Target target = default, MultiTargetType multi = MultiTargetType.One, bool skipCondition = false)
			{
				string s = "";
				if (_effect == null)
					return s;
				bool secondMention = false;
				if (!skipCondition && _requirement._requirement != null)
				{
					s += _requirement.GetDescription(conditional, multi) + ", ";
					secondMention = true;
				}
				string edesc = _effect._description;
				
				edesc = Translator.TranslateTargetTag(edesc, _target, multi, secondMention, target);
				edesc = GetNumberTags(edesc, _effect, _powerMultiplier);
				s += edesc;
				return s.ToLower();
			}
			public void ToggleLink()
			{
				_linkToPast = !_linkToPast;
			}
		}
		public static string GetNumberTags(string edesc, BaseEffect effect, float powerMultiplier)
		{
			foreach (string s in Translator._numberTags)
			{
				foreach (string tag in new List<string> { "[" + s + "]", "{" + s + "}" })
				{
					if (edesc.Contains(tag))
					{
						edesc = edesc.Replace(tag, "{" + s + effect._id + "," + powerMultiplier + "}");
					}
				}
			}
			foreach (string s in new List<string> { "[s]", "[es]" })
				if (edesc.Contains(s))
					edesc = edesc.Replace(s, powerMultiplier >= 2 ? s.Substring(1, s.Length - 2) : "");
			if (edesc.Contains("[a:"))
			{
				int beginIndex = edesc.IndexOf("[a:");
				int endIndex = edesc.Substring(beginIndex).IndexOf("]");
				string tag = edesc.Substring(beginIndex, endIndex + 1);
				string replacement = "";
				int cases = Mathf.RoundToInt(powerMultiplier * (effect._applyAura._aura._stackingType == BaseAura.StackingType.Duration ? effect._applyAura._duration : effect._applyAura._cases));
				if (cases > 1)
				{
					replacement = cases + " ";
				}
				replacement += "[" + tag.Substring(3, endIndex - 3) + "]";
				edesc = edesc.Replace(tag, replacement);
			}
			return edesc;
		}
		/// <summary>
		/// A requirement targeting either self or the "target"
		/// </summary>
		[Serializable]
		public class CharacterRequirement
		{
			public BaseRequirement _requirement;
			public TargetType _target;
			public CharacterRequirement(BaseRequirement br, TargetType tt)
			{
				_requirement = br;
				_target = tt;
			}
			public CharacterRequirement(bool tru)
			{
				_requirement = null;
				_target = TargetType.Target;
			}
			public string GetDescription(string conditional = "if", MultiTargetType multi = MultiTargetType.One)
			{
				string desc = conditional + " " + _requirement._description;
				desc = Translator.TranslateTargetTag(desc, _target, multi);
				return desc;
			}
			public bool InstantCheck(CombatCharacter self, CombatCharacter target)
			{
				if (_requirement == null)
					return true;
				return _requirement.InstantCheck(_target == TargetType.Target ? target : self);
			}
		}

		public bool TargetOnlySelf()
		{

			return (int)_target == 1;
		}
		public static int GetInkSplashCount()
		{
			return 4;
		}
		public void GenerateDescription()
		{
			string temps = "";
			_links = new List<BaseObject>();
			if (_free) temps += "[Free]. ";
			bool openLine = false;
			for (int i = 0; i < _effects.Count; i++)
			{
				string conditional = "if";
				Behaviour et = _effects[i];
				if (i > 0 && et._requirement._requirement != null)
				{
					if (et._requirement._requirement.IsOppositeOf(_effects[i - 1]._requirement._requirement))  //If the requirement is opposite of the previous effect's requirement
					{
						temps += ". ";
						temps += "Else " + Capitalize(et.GetDescription(conditional, _target, _multiTarget, true), true);
						continue;
					}
				}
				else if (i > 0 && et._effect == _effects[i - 1]._effect && (_effects[i - 1]._requirement._requirement == null || _effects[i - 1]._requirement._requirement == et._requirement._requirement))
				{
					temps += " twice";
					continue;
				}
				bool ltp = _effects[i]._linkToPast;
				string temp = et.GetDescription(conditional, _target, _multiTarget);
				if (temp != "")
				{
					if (i > 0) temps += ltp ? " and " : ". ";
					temps += ltp ? temp.ToLower() : Capitalize(temp);
				}
				openLine = true;
				if (et._effect._links != null && et._effect._links.Count > 0)
				{
					foreach (BaseObject bob in et._effect._links)
						if (_links.Contains(bob) == false)
							_links.Add(bob);

				}
				if (et._effect._applyAura._aura != null && _links.Contains(et._effect._applyAura._aura) == false)
				{
					_links.Add(et._effect._applyAura._aura);
				}
			}
			if (temps.Length > 0 && !(temps.EndsWith(".") || temps.EndsWith("\n")))
				temps += ".";
			foreach (CharacterRequirement rt in _requirements)
			{
				switch (rt._requirement._id)
				{
					default:
						break;
					case 5:
					case 17:
						temps += "\n" + Capitalize(rt._requirement._description);
						break;
				}
			}
			foreach (BaseObject temp in Translator.GetPossibleLinksInString(temps))
				if (_links.Contains(temp) == false) _links.Add(temp);
			_description = temps;
			_actualDescription = _description;
		}

	}
}

