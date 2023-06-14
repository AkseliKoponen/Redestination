using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static RD.CodeTools;

namespace RD.DB
{
	
	public class BaseTalent : BaseObject
	{
		public enum TalentTrigger
		{
			CombatStarts = 1,
			CombatEnds = 2,
			YourTurnStarts = 4,
			YourTurnEnds = 8,
			YouThink = 16,
			YouForgetAnIdea = 32,
			YouRepressAnIdea = 64,
			ActivateEffect = 128,
			GainAura = 256,
			ApplyAura = 512,
			YouActualizeAnIdea = 1024,
			YouAreInjured = 2048,
			YouTakeDamage = 4096,
			YouDealDamage = 8192,
			YouDefeatAnEnemy = 16384,
			YouWouldDie = 32768,
			YouMove = 65536,
			Passive = 131072,
			HardCode = 262144
		}
		public TalentTrigger _trigger;
		public BaseEffect _triggerEffect;
		public BaseAura _triggerAura;
		public List<BaseCard.EffectTarget> _effects = new List<BaseCard.EffectTarget>();
		[Tooltip("If requirements are not met, the talent will not trigger")]public List<BaseRequirement> _requirements = new List<BaseRequirement>();
		[Tooltip("If charges run out, the talent will not trigger")]
		public bool _charges = false;
		public int _chargesPerCombat_base = 1;
		public int _chargesPerDay_base = 1;
		#region Art
		public Sprite _sprite;
		public string _flavourText;
		#endregion
		#region Visibility
		public bool _visibleAura = false;
		public Sprite _auraIcon;
		#endregion
		public CodeTools.Polarity _polarity = Polarity.Positive;
		public BaseTalent()
		{
			_layOutSpace = new LayOutSpace(new List<int> {1, 8, 2, 2 }, new List<string> {"Trigger", "Art","Visibility", "General" });
		}

		public static string TriggerToPhrase(TalentTrigger tt)
		{
			string phrase = "";
			bool first = true;
			TalentTrigger exceptionValues = new TalentTrigger();
			exceptionValues = TalentTrigger.ActivateEffect | TalentTrigger.ApplyAura | TalentTrigger.GainAura | TalentTrigger.HardCode | TalentTrigger.Passive;
			foreach (Enum value in Enum.GetValues(tt.GetType()))
			{
				
				if (tt.HasFlag(value) && !exceptionValues.HasFlag(value))
				{
					First();
					phrase += "when " + Translator.DecodeName(value.ToString());
				}
			}
			return phrase;
			void First()
			{
				if (!first)
				{
					phrase += " and ";
				}
				else
				{
					phrase += " ";
					first = false;
				}
			}
		}
		public void GenerateDescription()
		{
			string temps = "";
			_links = new List<BaseObject>();
			for (int i = 0; i < _effects.Count; i++)
			{
				string conditional = "if";
				BaseCard.EffectTarget et = _effects[i];
				if (i > 0 && et._requirement._requirement != null)
				{
					if (et._requirement._requirement.IsOppositeOf(_effects[i - 1]._requirement._requirement))  //If the requirement is opposite of the previous effect's requirement
					{
						temps += ". ";
						temps += "Else " + Capitalize(et.GetDescription(conditional,default, BaseCard.MultiTargetType.One, true), true);
						continue;
					}
				}
				else if (i > 0 && et._effect == _effects[i - 1]._effect && (_effects[i - 1]._requirement._requirement == null || _effects[i - 1]._requirement._requirement == et._requirement._requirement))
				{
					temps += " twice";
					continue;
				}
				bool ltp = _effects[i]._linkToPast;
				string temp = et.GetDescription(conditional,default, BaseCard.MultiTargetType.One);
				if (temp != "")
				{
					if (i > 0) temps += ltp ? " and " : ". ";
					temps += ltp ? temp.ToLower() : Capitalize(temp);
				}
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
			temps += TriggerToPhrase(_trigger);
			if (temps.Length > 0 && !(temps.EndsWith(".") || temps.EndsWith("\n")))
				temps += ".";
			foreach (BaseObject temp in Translator.GetPossibleLinksInString(temps))
				if (_links.Contains(temp) == false) _links.Add(temp);
			_description = temps;
			_actualDescription = _description;
		}
	}
}
