using System;
using System.Collections;
using System.Collections.Generic;
using RD.Combat;
using RD;
using UnityEngine;
using static RD.CodeTools;
using RD.UI;

namespace RD.DB
{
	
	public class BaseEffect : BaseObject
	{
		#region Variables
		//if requirements do not match, cancel spell effect. The card can still be played but this particular spell will have no effect.
		//--------------Damage and HP MOD------------
		//public HealthData _modHP = new HealthData(true);
		//public HealthData _setHP = new HealthData(true);
		public bool _dealsDamage = false;
		public int _damageFlat;                                                 //Use this for attack damage
		[Tooltip("Melee = sword damage + STR*3.\nRanged = pistol damage + DEX*3\nSpecial = INT*3")]
		public DamageType _damageType;
		public int _modInit;                            // Change combatCharacter Initiative, forcing them back or forth in turn order
		//--------------------MISCELLANEOUS-------------
		//public MovementType _movementType = MovementType.stay;
		[Tooltip(">0 = Move Towards Enemy, <0 = Move Away From Enemy")]
		public int _moveAmount = 0;                       //Positive values = push, negative values = push
		public ApplyAura _applyAura = new ApplyAura();
		/*
	 * Spell visual effects
	*/
		public enum DamageType {None, Melee, Ranged, Special, Heal, Override}
		public enum SpellPowerType { Damage, ModHealth, Draw, AddEnergy, None }
		public enum MovementType { stay, moveToTarget, moveBeforeTarget, moveBehindTarget, swapPlaces }

		[Tooltip("How likely the AI will use cards with this effect")]
		public float _attraction = 0;
		[Serializable]
		public struct ApplyAura
		{
			public BaseAura _aura;
			public int _duration;
			public int _cases;
			public ApplyAura(bool tru)
			{
				_aura = null;
				_duration = 1;
				_cases = 1;
			}
			public ApplyAura(BaseAura baseaura)
			{
				_aura = baseaura;
				_duration = 1;
				_cases = 1;
			}
			public ApplyAura(ApplyAura aa)
			{
				_aura = aa._aura;
				_duration = aa._duration;
				_cases = aa._cases;
			}
		}
		[Serializable]
		public struct HealthData
		{
			public int flat;
			public float missing;
			public Bfloat fractional;
			public HealthData(bool tru)
			{
				fractional = new Bfloat(0);
				flat = 0;
				missing = 0;
			}
		}

		#endregion
		public BaseEffect()
		{
			_layOutSpace = new LayOutSpace(new List<int> { 1, 3, 4 }, new List<string> { "Damage and hp mod", "Misc", "General" });

		}
		public void ActivateOnMind(BaseThought.ThoughtModification tm, Thought thought, GridTransform gt = default)
		{
			if (gt == default)
				gt = thought._gTransform;
			switch (_id)
			{
				default:
					Debug.LogError("UNIMPLEMENTED HARD CODE AT " + _id);
					break;
			}
		}
		public IEnumerator Activate(CombatCharacter target, Combat.CombatCharacter source, float power)
		{
			//Debug.Log("Card Result = " + result.ToString());
			if(_debugLog)
				Debug.Log("<color=teal>"+ GetFileName() + "</color> targeting <color=orange>" + (target?target.GetName():" ground")+"</color>");
			if (!_requireSpecialCode)
			{
				#region Damage 
				if (target != null && _dealsDamage)
				{
					Damage damage = new Damage(this, source, target,null,power);
					source.StartCoroutine(source.PrepareOutgoingDamage(damage));
					source.AddDamageToHistory(damage);
					target.prepareIncomingDamageTask = new Task(target.PrepareIncomingDamage(damage));
					if(_id == 21)
					{
						source._inventory._ranged.Trigger();
					}
				}
				if(target!=null && _damageType == DamageType.Heal)
				{
					target.TakeHeal(source.EstimateDamageQuick(this, power, target));
				}
				#endregion
				#region Movement
				if (_moveAmount != 0)
				{
					_combatManager.PushCharacter(_moveAmount, source, target, false);
				}
				#endregion
				#region Auras
				if(_applyAura._aura!= null)
				{
					ApplyAura aa = new ApplyAura(_applyAura);
					if (_applyAura._aura._stackingType == BaseAura.StackingType.Case)
						aa._cases = Mathf.RoundToInt(aa._cases * power);
					else if(_applyAura._aura._stackingType == BaseAura.StackingType.Duration)
						aa._duration = Mathf.RoundToInt(aa._duration * power);
					target.AddAura(aa, source);
				}
				#endregion
			}
			else
			{//Hardcoded Effects
				switch (_id)
				{
					default:
						Debug.LogError("Unimplemented Hard Code at " + _name);
						break;
					case 12:
						//012 - Barrier. Gain barrier.
						int bar = Mathf.RoundToInt(power * (_damageFlat + (source._attributes.CalculateEffectScaling(this))));
						target._stats.barrier.Add(bar,true);
						target.UpdateStats(true);
						break;
					case 25:
					case 26:
						//0025 - Haste
						//0026 - Slow
						if(TurnDisplay._current.CommitHastePreview() == false)
						{
							TurnDisplay._current.HasteCharacter(target, Mathf.RoundToInt(power * (_id==25?3f:-3f)));
						}
						
						//Do nothing, the haste/slow is already applied in preview.
						break;
					case 34:
					case 39:
					case 42:
						#region Reload
						//034 - Reload and spend a reaction
						//039 - Reload
						//042 - Reload 1 turn
						{
							Weapon pistol = target._inventory._ranged;
							if (pistol == null || pistol.ReadyToFire())
								break;
							if (_id == 39)
								pistol.Reload();
							else if (_id == 34)
							{
								Aura reaction = source.GetAura((BaseAura)db.Get<BaseAura>(0));
								if (reaction != null)
								{
									reaction.RemoveCases(1);
									pistol.Reload();
								}
							}
							else if (_id == 42)
							{
								Aura rld = source.GetAura((BaseAura)db.Get<BaseAura>(19));
								if (rld != null){
									rld.Trigger(null);
								}
							}
						}
						#endregion
						break;
					case 38:
						//0038 - End Turn
						break;
					case 40:
						//040 - Gain 10% HP
						int heal = Mathf.RoundToInt((target._stats.hp.max - target._stats.hp.current) * 0.1f * power);
						target.TakeHeal(heal, source);
						break;
					case 45:
						//045 - Regain an action.
						source.RegainActions();
						break;
				}
			}
			yield break;
		}
		public int GetNumberQuick(float powerMultiplier, CombatCharacter source)
		{
			
			if (_requireSpecialCode)
			{
				switch (_id)
				{
					default:
						break;
					case 12:
						//012 - Barrier. Gain barrier.
						return Mathf.RoundToInt(powerMultiplier * (_damageFlat + (source._attributes.CalculateEffectScaling(this))));
				}
			}
			if (!_dealsDamage)
			{
				float value = 0;
				value = _damageFlat;
				if (value != 0) return Mathf.RoundToInt(value * powerMultiplier);
				value = _moveAmount;
				if (value != 0) return Mathf.RoundToInt(value * powerMultiplier);
				Debug.LogError("Unable to determine quick number for effect " + _name + ". Returning 0!");


			}
			if (source == null)
				return Mathf.RoundToInt(_damageFlat * powerMultiplier);
			return Mathf.RoundToInt(source.EstimateDamageQuick(this,powerMultiplier));
			
			
		}
		public IEnumerator TargetCardsInHand(int count, string verb)
		{
			_combatGUI.PauseTimers(true);
			TargetingSystem ts = _combatGUI.ActivateHandTargeting(count,verb);
			//Hand has to be moved before the darkening screen
			_postProcessingManager.Target(true);
			while (ts.ready==0)
			{
				yield return null;
			}
			_postProcessingManager.Target(false);
			List<Card> cards = new List<Card>();
			foreach (CombatGUICard cgcard in ts.GetSelectedCards())
			{
				cards.Add(cgcard._card);
			}
			//hand has to be moved back
			_combatGUI.PauseTimers(false);
			yield return cards;
		}
	}
}
