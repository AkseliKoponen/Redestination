using RD.Combat;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static RD.CodeTools;

namespace RD.DB
{
	public class BaseCard : BaseObject
	{
		[Serializable] public enum CardType { Skill, Attack, Item }
		[Serializable] public enum MultiTargetType { One, Two, Three, All }

		[System.Flags] [Serializable] public enum Target { Self = 1,
			Friend = 2,
			Enemy = 4,
			Object = 8,
			Ground = 16 }

		[System.Flags]
		[Serializable]
		public enum CardClass
		{
			Soldier = 1,
			Spy = 2,
			Inventor = 4,
			Willon = 8,
			Mardrak = 16,
			NPC = 32,
			Generic = 64
		}

		//--------Card Texts-------
		public CardType _cardType = CardType.Skill;
		public CardClass _cardClass;
		public string _nameCombinationNoun;
		public string _nameCombinationAdjective;
		//--------Targeting
		public Target _target;
		//[Tooltip("Can't hit targets closer than min range and targets further than max range")]
		public bool _meleeRange = false;
		public bool _requireLineOfSight;
		public bool _randomTarget;
		public MultiTargetType _multiTarget;
		//---------------Effects
		public Effects _effects = new Effects(true);
		//------------Features----------------
		public Features _features = new Features(true);
		//-----------ART-------------------------------//
		public Sprite _artSprite; //="Default Card Art.png" if empty, go to default
		[Tooltip("0 = no splash\n1-4 different splashes")]
		public int _inkSplash = 4;
		[SerializeField]string _artSpriteString = "";
		//public string _cardBG; //="Default Card Background.png" if empty, go to default
		public BaseFX _baseFX;
		public string _flavourText = "";
		//public string _animationPrep;
		public BaseCard()
		{
			_layOutSpace = new LayOutSpace(new List<int> { 5, 5, 1, 1, 4 }, new List<string> { "Target", "Effects",  "Features", "Art", "General" });
		}

		[Serializable] public struct Effects
		{
			public List<EffectTarget> _effects;
			[Tooltip("If Requirements are not met, can't play the card")]
			public List<RequirementTarget> _requirements;

			[Tooltip("At the end of your turn trigger these effects if you are holding the idea. \nNote: target is always self.")]
			public List<EffectTarget> _onHoldEffects;
			[Tooltip("Thinking the idea will trigger these effects.\nNote: target is always self.")]
			public List<EffectTarget> _onThinkEffects;
			[Tooltip("Forgetting the idea will trigger these effects.\nNote: target is always self.")]
			public List<EffectTarget> _onForgetEffects;
			[Tooltip("Repressing the idea will trigger these effects.\nNote: target is always self.")]
			public List<EffectTarget> _onRepressEffects;
			public Effects(bool def)
			{
				_effects = new List<EffectTarget>();
				_requirements = new List<RequirementTarget>();
				_onHoldEffects = new List<EffectTarget>();

				_onThinkEffects = new List<EffectTarget>();
				_onForgetEffects = new List<EffectTarget>();
				_onRepressEffects = new List<EffectTarget>();
			}
			public bool IsDefault()
			{
				return _effects.Count == 0 && _onHoldEffects.Count == 0 && _onThinkEffects.Count == 0 && _onForgetEffects.Count == 0 && _onRepressEffects.Count == 0;
			}

		}
		[Serializable]
		public enum TargetType { Target, Self }
		[Serializable]
		public struct EffectTarget
		{
			public BaseEffect _effect;
			public TargetType _target;
			public float _powerMultiplier;
			public RequirementTarget _requirement;
			public bool _linkToPast;
			public EffectTarget(bool tru)
			{
				_effect = null;
				_target = TargetType.Target;
				_powerMultiplier = 1f;
				_requirement = new RequirementTarget(tru);
				_linkToPast = false;
			}
			public string GetDescription(string conditional = "if", BaseCard.Target target = default, MultiTargetType multi = MultiTargetType.One, bool skipCondition = false)
			{
				string s = "";
				if (_effect == null)
					return s;
				bool secondMention = false;
				if (!skipCondition && _requirement._requirement != null)
				{
					s += _requirement.GetDescription(conditional,multi)+", ";
					secondMention = true;
				}
				string edesc = _effect._description;
				edesc = Translator.TranslateTargetTag(edesc, _target, multi,secondMention,target);
				edesc = GetNumberTags(edesc, _effect, _powerMultiplier);
				/*
				foreach(string tag in new List<string>{ "[dmg]","{dmg}"})
					if (edesc.Contains(tag))
						edesc = edesc.Replace(tag,"{dmg" +_effect._id+","+_powerMultiplier+"}");
				foreach (string tag in new List<string> { "[barrier]", "{barrier}" })
					if (edesc.Contains(tag))
						edesc = edesc.Replace(tag, "{barrier" + _effect._id + "," + _powerMultiplier + "}");
				*/
				s += edesc;
				return s.ToLower();
			}
			public void ToggleLink()
			{
				_linkToPast = !_linkToPast;
			}
		}
		public static string GetNumberTags(string edesc,BaseEffect effect, float powerMultiplier)
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
			foreach(string s in new List<string> { "[s]","[es]"})
				if (edesc.Contains(s))
					edesc = edesc.Replace(s, powerMultiplier >= 2 ? s.Substring(1,s.Length-2) : "");
			if (edesc.Contains("[a:"))
			{
				int beginIndex = edesc.IndexOf("[a:");
				int endIndex = edesc.Substring(beginIndex).IndexOf("]");
				string tag = edesc.Substring(beginIndex, endIndex+1);
				string replacement="";
				int cases = Mathf.RoundToInt(powerMultiplier * (effect._applyAura._aura._stackingType == BaseAura.StackingType.Duration ? effect._applyAura._duration : effect._applyAura._cases));
				if (cases>1)
				{
					replacement = cases+ " ";
				}
				replacement += "["+tag.Substring(3,endIndex-3)+"]";
				edesc = edesc.Replace(tag, replacement);
			}
			return edesc;
		}
		[Serializable]
		public class RequirementTarget
		{
			public BaseRequirement _requirement;
			public TargetType _target;
			public RequirementTarget(BaseRequirement br, TargetType tt)
			{
				_requirement = br;
				_target = tt;
			}
			public RequirementTarget(bool tru)
			{
				_requirement = null;
				_target = TargetType.Target;
			}
			public string GetDescription(string conditional = "if", MultiTargetType multi = MultiTargetType.One)
			{
				string desc = conditional+" "+_requirement._description;
				desc = Translator.TranslateTargetTag(desc, _target, multi);
				return desc;
			}
			public bool InstantCheck(CombatCharacter self, CombatCharacter target)
			{
				if (_requirement == null)
					return true;
				return _requirement.InstantCheck(_target == TargetType.Target?target:self);
			}
		}

		[Serializable] public struct Features
		{
			public bool _playOnDraw;
			public bool _unplayable;
			[Tooltip("The idea will not cost an action to perform.")]
			public bool _free;
			[Tooltip("Once played, the card may be played again until the end of turn.")]
			public bool _repeating;
			[Tooltip("When PLAYED, the card will instead be burned.")]
			public bool _burn;
			[Tooltip("When Fumbled or otherwise discarded,\n the card will instead be placed in the depleted pile.")]
			public bool _volatile;
			[Tooltip("The Card will be discarded at the end of your turn.")]
			public bool _fumble;
			//[Tooltip("Once the charges of a card have run out\n it will be removed from your deck until you rest at a camp.")]
			//public int _charges;
			public Features(bool lol)
			{
				_playOnDraw = false;
				_unplayable = false;
				_repeating = false;
				_burn  = false;
				_volatile = false;
				_fumble  = false;
				_free = false;
				//_charges= 0;
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
		public void LoadArtSprite()
		{
			if (_artSprite != null)
				return;
			if (_artSpriteString != "")
			{
				Sprite sp = Resources.Load<Sprite>("Card Art/" + _artSpriteString);
				_artSprite = sp!=null?sp: Resources.Load<Sprite>("Card Art/Missing");
				return;
			}
			else {
			_artSprite = Resources.Load<Sprite>("Card Art/Missing");
			}
		}
		public void SetArtString()
		{
			if (_artSprite == null)
				_artSprite = Resources.Load<Sprite>("Card Art/Missing");
			if(_artSprite)
				_artSpriteString = _artSprite.name;
		}
		public void GenerateDescription()
		{
			string temps = "";
			_links = new List<BaseObject>();
			if (_features._unplayable) temps += "Unplayable.\n";
			if (_features._free)temps+="[Free]. " ;
			bool openLine = false;
			for (int i = 0; i < _effects._effects.Count; i++)
			{
				string conditional = "if";
				EffectTarget et = _effects._effects[i];
				if(i>0 && et._requirement._requirement != null)
				{
					if (et._requirement._requirement.IsOppositeOf(_effects._effects[i - 1]._requirement._requirement))	//If the requirement is opposite of the previous effect's requirement
					{
						temps += ". ";
						temps += "Else "+Capitalize(et.GetDescription(conditional,_target, _multiTarget,true),true);
						continue;
					}
				}
				else if (i > 0 && et._effect == _effects._effects[i - 1]._effect && (_effects._effects[i - 1]._requirement._requirement == null || _effects._effects[i - 1]._requirement._requirement == et._requirement._requirement))
				{
					temps += " twice";
					continue;
				}
				bool ltp = _effects._effects[i]._linkToPast;
				string temp = et.GetDescription(conditional,_target, _multiTarget);
				if (temp != "")
				{
					if (i > 0) temps += ltp ? " and " : ". ";
					temps += ltp ? temp.ToLower() : Capitalize(temp);
				}
				openLine = true;
				if(et._effect._links!= null && et._effect._links.Count > 0)
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
			if(temps.Length>0 && !(temps.EndsWith(".") || temps.EndsWith("\n")))
				temps += ".";
			if (_effects._onHoldEffects.Count > 0)
			{
				if (openLine) temps += "\n";
				string s = "[Hold]: ";
				for (int i = 0; i < _effects._onHoldEffects.Count; i++)
				{
					if (i > 0)
						s += ",";
					string pv = _effects._onHoldEffects[i].GetDescription();
					s += i == 0 ? Capitalize(pv) : pv;
				}
				s = Capitalize(s) + ".";
				temps += s;
			}
			if (_features._burn || _features._fumble || _features._repeating)
			{
				temps += "\n";
				if (_features._burn) temps += "[Repress]. ";
				if (_features._fumble) temps += "[Fickle]. ";
				if (_features._repeating) temps += "[Repeating]. ";
			}
			foreach (RequirementTarget rt in _effects._requirements)
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

