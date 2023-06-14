using System.Reflection;
namespace RD.Combat
{
	public class CombatEventSystem
	{
		#region CombatActions
		public EventTrigger _OnPrepareOutgoingDamage = new EventTrigger();
		public void OnPrepareOutgoingDamage(object damage = null)
		{
			_OnPrepareOutgoingDamage.Trigger(damage);
		}

		public EventTrigger _OnPrepareIncomingDamage = new EventTrigger();
		public void OnPrepareIncomingDamage(object damage = null)
		{
			_OnPrepareIncomingDamage.Trigger(damage);
		}

		public EventTrigger _OnAttacked = new EventTrigger();
		public void OnAttacked(object attacker = null)
		{
			_OnAttacked.Trigger(attacker);
		}

		public EventTrigger _OnDeath = new EventTrigger();
		public void OnDeath(object attacker = null)
		{
			_OnDeath.Trigger(attacker);
		}

		public EventTrigger _OnAttack = new EventTrigger();
		public void OnAttack(object target = null)
		{
			_OnAttack.Trigger(target);
		}

		public EventTrigger _OnKill = new EventTrigger();
		public void OnKill(object target)
		{
			_OnKill.Trigger(target);
		}
		public EventTrigger _OnReaction = new EventTrigger();
		public void OnReaction(object target)
		{
			_OnReaction.Trigger(target);
		}

		public EventTrigger _OnMove = new EventTrigger();
		public void OnMove(object target)
		{
			_OnMove.Trigger(target);
		}

		public EventTrigger _OnApplyAura= new EventTrigger();
		public void OnApplyAura(object aura)
		{
			_OnMove.Trigger(aura);
		}

		public EventTrigger _OnActivateEffect = new EventTrigger();
		public void OnActivateEffect(object effect)
		{
			_OnActivateEffect.Trigger(effect);
		}


		public EventTrigger _OnRefreshStats = new EventTrigger();
		public void OnRefreshStats(object cc)
		{
			_OnRefreshStats.Trigger(cc);
		}
		#endregion
		//-------------------------------------------------------
		#region Chronology
		public EventTrigger _OnTurnStart = new EventTrigger();
		public void OnTurnStart(object turn)
		{
			_OnTurnStart.Trigger(turn);
		}

		public EventTrigger _OnTurnEnd = new EventTrigger();
		public void OnTurnEnd(object turn)
		{
			_OnTurnEnd.Trigger(turn);
		}

		public EventTrigger _OnCombatStart = new EventTrigger();
		public void OnCombatStart(object encounter)
		{
			_OnCombatStart.Trigger(encounter);
		}

		public EventTrigger _OnCombatEnd = new EventTrigger();
		public void OnCombatEnd(object encounter)
		{
			_OnCombatEnd.Trigger(encounter);
		}
		#endregion
		//-------------------------------------------------------
		#region Card
		public EventTrigger _OnThink = new EventTrigger();
		public void OnThink(object card)
		{
			_OnThink.Trigger(card);
		}

		public EventTrigger _OnForget = new EventTrigger();
		public void OnForget(object card)
		{
			_OnForget.Trigger(card);
		}

		public EventTrigger _OnRepress = new EventTrigger();
		public void OnRepress(object card)
		{
			_OnRepress.Trigger(card);
		}

		public EventTrigger _OnIdea = new EventTrigger();
		public void OnIdea(object card)
		{
			_OnIdea.Trigger(card);
		}
		#endregion
		//-------------------------------------------------------
		public bool dbug = false;
		public CombatEventSystem(CombatCharacter cc)
		{
			if (dbug)
			{
				foreach (FieldInfo fif in this.GetType().GetFields())
				{
					if (fif.FieldType == typeof(EventTrigger))
					{
						EventTrigger et = (EventTrigger)fif.GetValue(this);
						et._name = fif.Name.Substring(1) + " of " + cc.GetName();
						et._debugEnabled = true;
					}
				}
			}
		}
		public class EventTrigger
		{
			public delegate void Delegate(object obj = null);
			public event Delegate _delegates;
			public string _name = "(Unnamed EventTrigger)";
			public bool _debugEnabled = false;
			public void Trigger(object obj = null)
			{
				_delegates?.Invoke(obj);
				if(_debugEnabled)Dbug();
			}
			public void Dbug()
			{
				string s = "<color=orange>" + _name + " has the following delegates:</color>\n";
				int i = 0;
				if(_delegates?.GetInvocationList().Length>0)
					foreach (Delegate dg in _delegates?.GetInvocationList())
					{
						string target = dg.Target.GetType().Name;
						try
						{
							DB.BaseObject bob = (DB.BaseObject)dg.Target;
							target += "("+bob._name+")";
						} catch { }
						s += "["+i+"]"+target+"."+dg.GetMethodInfo().Name + "\n";
						i++;
					}
				UnityEngine.Debug.Log(s);
			}
		}
		public class DataFormat
		{
			public EventTrigger _eventTrigger { get; private set; }
			public EventTrigger.Delegate _delegate { get; private set; }
			public DataFormat(EventTrigger et, EventTrigger.Delegate deleg)
			{
				_eventTrigger = et;
				_delegate = deleg;
			}
			public void Clear()
			{
				_eventTrigger._delegates -= _delegate;
			}
		}
	}
}

