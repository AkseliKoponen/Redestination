using System.Collections.Generic;
using UnityEngine;


/*
* Auras todo:
*	Haste
*		Prevents you from being hastened again. Lasts until the end of the hastened turn.
*	Slow
*		Prevents you from being slowed again. Lasts until the end of your slowed turn.
* 
*List of Auras requiring special code:
* Parry
* Haste/Slow
*/

namespace RD.DB
{
	public class BaseAura : BaseObject
	{
		public Sprite _sprite;
		//--------------------Time and triggers-------------
		public int _duration = 0;                                 //Every time the aura ticks, reduce duration by 1. If constant, reduce duration before turn
		public StackingType _stackingType = StackingType.Duration;
		public int _baseCases  = 1;                           //How many cases of the buff are included
		[Tooltip("When do the cases or duration tick down?\nTrigger ticks when triggered")]
		public TickType _tickType  = TickType.trigger;   //Whether the Talent never 'ticks' and is instead a constant buff, or 'ticks' at the start or end of turn
		public bool _triggerOnTick = true;
		public List<BaseRequirement> _requirements = new List<BaseRequirement>();                  //If Requirements are not met, suppress Talent effects

		[Tooltip("If true, modHealth will be updated to duration")]
		public bool _durationIsModHealth = false;                 //If true, modHealth will be reduced by 1 every turn instead of duration.	
		//---------------------Numbers------------
		public int _modHealth;                                   //If constant, modify max hp. If triggering, deal damage/heal every turn
		public int _armorMod;
		[Tooltip("Flat increase or decrease to damage")]
		public int _damageModFlat;                              //flat increase or decrease to damage
		[Tooltip("Multiply damage with this.\n0 = no damage, 2 = double damage")]
		public float _damageModFractional  = 1;                        //1 = full damage, 0 = no damage, 2 = double damage, 0.5 = half damage
		[Tooltip("Flat increase or decrease to INCOMING damage")]
		public int _damageModIncomingFlat;                             //flat increase or decrease to damage
		[Tooltip("Multiply INCOMING damage with this.\n0 = no damage, 2 = double damage")]
		public float _damageModIncomingFractional  = 1;                        //1 = full damage, 0 = no damage, 2 = double damage, 0.5 = half damage
		public bool _damageModAffectsHeal;
		//---------------------Effects------------
		public List<PowerEffect> _tickEffects = new List<PowerEffect>();
		//----------Card Modification--------------
		public BaseEffect.CardModification _cardModification = new BaseEffect.CardModification(true);
		//-----------Card Management-------------------------
		public BaseEffect.CardManagement _cardManagement = new BaseEffect.CardManagement(true);
		//----------------Misc
		public CodeTools.Polarity _polarity = CodeTools.Polarity.Neutral;
		//public bool _negative;
		public bool _visible;

		public enum TickType { trigger, afterTurn, beforeTurn }
		public enum StackingType { Case, Duration, None }

		[System.Serializable]
		public class PowerEffect
		{
			public BaseEffect effect = null;
			public float powerMultiplier = 1;
		}
		public BaseAura()
		{
			_layOutSpace = new LayOutSpace(new List<int> { 4, 5, 7, 1, 1, 1 }, new List<string> { "Time and Triggering", "Numbers","Effects", "Card Modification", "Card Management", "General" });
		}
		public string GetFlowingDescription()
		{
			return _description.Substring(0, 1).ToLower() + _description.Substring(1,_description.Length-1);
		}
		public void GenerateDescription()
		{
			string s = _description;
			_links = new List<BaseObject>();
			if(_tickEffects.Count>0)
				switch (_tickType)
				{
					case TickType.afterTurn:
						s += "After your turn ";
						break;
					case TickType.beforeTurn:
						s += "At the beginning of your turn ";
						break;
					case TickType.trigger:
						break;
				}
			foreach (PowerEffect pe in _tickEffects)
			{
				if (pe != null)
				{
					string edesc = pe.effect._description;
					edesc = Translator.TranslateTargetTag(edesc, BaseCard.TargetType.Self, BaseCard.MultiTargetType.One,true);
					edesc = BaseCard.GetNumberTags(edesc, pe.effect, pe.powerMultiplier);
					s += edesc.ToLower();
				}
			}
			if(_stackingType == StackingType.Case)
			{
				if (_tickType == TickType.afterTurn)
					s += "\nRemoved after [host's] turn.";
				else if (_tickType == TickType.beforeTurn)
					s += "\nRemoved at the beginning of [host's] turn.";
			}
			_links.AddRange(Translator.GetPossibleLinksInString(s));
			_description = s;
			_actualDescription = _description;
		}
	}
}