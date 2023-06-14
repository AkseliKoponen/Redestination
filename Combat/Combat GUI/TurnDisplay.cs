using System.Collections.Generic;
using UnityEngine;

namespace RD.Combat
{
	public class TurnDisplay : MonoBehaviour
	{


		/*
	 * Base speed is 10
	+1 per dex
	+5 from haste and -5 from slow

	When it is your turn, display a preview of your next position on the turn order.

	A more detailed graph could be:
	0....X.....1..X.......2
	Position here is 10/speed so a speed of 5 would be at 2 while a speed of 21 would be at 0.45
	Actually do this! But maybe no graph displayed? This way the faster characters would get more turns eventually.

	Speed = 10(base) + 1(dex bonus) + 5 (haste) = 16
	TurnOrder = 10 / 16 = 0.625


	Everyone has their speed and turn speed separated. When turn ends, everyone's turn speed goes towards 0.
	 */
		RectTransform _rt;
		public static TurnDisplay _current;
		public static float _turnMax = 2f;
		public TurnIcon _turnIconPrefab;
		List<Character> _characters;
		Character _currentCharacter;
		public class Character	//Display Multiple Turn Icons for each character
		{
			bool _highlighted = false;
			bool _targeted = false;
			bool _turn = false;
			List<TurnIcon> _icons;
			public CombatCharacter _cc;
			//public float _delay = 0;
			public Character(CombatCharacter cc)
			{
				_cc = cc;
				_icons = new List<TurnIcon>();
			}

			public void Clear()
			{
				if (_icons != null)
				{
					foreach (TurnIcon td in _icons)
					{
						Destroy(td.gameObject);
					}
				}
				_icons = new List<TurnIcon>();
			}
			public void Wait(float time)
			{
				foreach(TurnIcon ti in _icons)
				{
					ti.Wait(time);
				}
			}
			public float GetNextIcon()
			{
				return _icons[0].GetTurnOrder();
			}
			public void StartTurn()
			{
				_turn = true;
				_icons[0].StartTurn();
				Highlight(true,true);
			}
			public void Init()
			{
				FillIcons();
			}
			public void FillIcons()
			{
				float init = -0.1f*_cc._stats.initiative;
				float spd = _turnMax-(0.1f*_cc._stats.speed);
				float max = _turnMax+0.1f;
				//Debug.Log("spd of " + _cc.GetName() + " = " + spd);
				if (_icons.Count == 0)
				{
					for (int i = 0;((i+1) * spd) < max; i++)
					{
						CreateIcon(init + i*spd);
					}
				}
				else
				{
					float lastIcon = _icons[_icons.Count - 1].GetTurnOrder();
					//0
					//0.7
					//----CreateIcon(2)
					//2.1
					for(int i = 0; (lastIcon + ((i+1) * spd)) < max; i++)
					{
						float delay = lastIcon + (i * spd);
						//Debug.Log(_cc.GetName() + " new Icon @ " + (delay+spd));
						CreateIcon(delay);
					}
				}
				if (false)Dbug();

				void Dbug()
				{
					Debug.Log("-----------------------------");
					Debug.Log(_cc.GetName());
					foreach (TurnIcon ti in _icons)
					{
					
						Debug.Log(System.Math.Round(ti.GetTurnOrder(), 1));
					}
					Debug.Log("Next Icon would be at " + (_icons[_icons.Count - 1].GetTurnOrder() + spd));
					Debug.Log("-----------------------------");
				}
				//UpdatePositions(true);
				//Create Turn Displays until 2
				void CreateIcon(float delay)
				{
					//Debug.Log(_cc.GetName()+".CreateIcon(" + delay + ")");
					TurnIcon ti = Instantiate(_current._turnIconPrefab, _current.transform.GetChild(0));
					RectTransform tirt = ti.GetComponent<RectTransform>();
					tirt.localPosition = new Vector3(tirt.localPosition.x,_current._rt.localPosition.y-_current._rt.sizeDelta.y-tirt.sizeDelta.y, 0);
					_icons.Add(ti);
					ti.Init(this, delay);
					ti.UpdatePosition(false);
                    if (_icons.Count > 1)
                    {
						TurnIcon prevTI = _icons[0];
						if (_highlighted)
						{
							ti._highlightFrame.enabled = true;
							ti.CopyHighlight(prevTI);
						}
                        if (_targeted)
                        {
							ti.EnableCrosshair(prevTI.GetCrossHair().color);
                        }
					}
				}
			}
			public void Target(bool hostile = true)
			{
				Color c = hostile ? Color.red * 0.5f : Color.green * 0.5f;
				foreach(TurnIcon ti in _icons)
				{
					ti.EnableCrosshair(c);
				}
			}
			public void Untarget()
			{
				foreach (TurnIcon ti in _icons)
				{
					ti.DisableCrosshair();
				}
			}
			public void Haste(int hasteAmount)
			{
				Debug.Log("<color=cyan>--------------------------------</color>");
				Debug.Log("<color=cyan>Applying "+hasteAmount+" Haste on " + _cc.GetName()+"</color>");
				_current.Log(6);
				
				foreach (TurnIcon ti in _turn?_icons.GetRange(1,_icons.Count-1):_icons)
				{
					//if (!(_turn && ti == _icons[0]))
						ti.Haste(hasteAmount);
					//else
						//Debug.Log("<color=orange>Kaka</color>");
				}
				
				foreach (TurnIcon ti in _icons)
				{
					if (!(_turn && ti == _icons[0]))
					{
						ti.UpdatePosition();
					}
				}
				if (hasteAmount>0)
					FillIcons();
				_current.Refit();
				Debug.Log("<color=cyan>----AFTER----</color>");
				_current.Log(6);
				Debug.Log("<color=cyan>--------------------------------</color>");
			}
			public List<TurnIcon> GetIcons()
			{
				return _icons;
			}
			public void EndTurn()
			{
				_turn = false;
				if (_icons.Count > 0)
				{
					Destroy(_icons[0].gameObject);
					_icons.RemoveAt(0);
				}
				Highlight(false);
			}
			public void ToggleTarget(bool enable, Color col = default)
			{
				if (enable)
				{
					if (col == default)
						col = Color.red;
					foreach (TurnIcon ti in _icons)
					{
						ti.EnableCrosshair(col);
					}
				}
				else
				{
					foreach (TurnIcon ti in _icons)
					{
						ti.DisableCrosshair();
					}
				}
				_targeted = enable;
			
			}
			public void Highlight(bool enable, bool turn = false)
			{
				if (enable == false && turn == false && _turn == true)
				{
					turn = true;
					enable = true;
				}
				_highlighted = enable;
				#region Highlight Color
				Color col1;
				Color col2;
				if (!turn)
				{
					col1 = new Color(0.749f, 0.585f, 0);
					col2 = new Color(0.749f, 0.025f, 0);
				}
				else
				{
					col1 = new Color(0, 0.749f, 0.688f);
					col2 = new Color(0, 0.502f, 0.749f);
				}
				#endregion
				foreach (TurnIcon ti in _icons)
				{
					ti._highlightFrame.enabled = enable;
					if (enable)
					{

						ti._highlightFrame.GetComponent<UIGradient>().LinearColor1 = col1;
						ti._highlightFrame.GetComponent<UIGradient>().LinearColor2 = col2;
					}

				}
                if (!enable)
                {
					ToggleTarget(false);
                }
			}
		}
		private void Awake()
		{
			_current = this;
			_rt = GetComponent<RectTransform>();
		}
		public void Init(List<CombatCharacter> _cChars)
		{
			ClearCharacters();
			List<int> initiatives = new List<int>();
			foreach(CombatCharacter cc in _cChars)
			{
				while (initiatives.Contains(cc._stats.GetInitialSpeed()))
				{
					cc._stats.initiative -= 1;
				}
				initiatives.Add(cc._stats.GetInitialSpeed());
				Character c = new Character(cc);
				c.Init();
				_characters.Add(c);
			}
			//UpdatePositions();
		}
		public CombatCharacter NextTurn()
		{
			if (_characters == null || _characters.Count < 1)
				return null;
			EndCurrentTurn();
			//Log();
			List<TurnIcon> tis = GetTurnIconsInOrder();
			while (tis.Count>0 && tis[0]._tdCharacter == _currentCharacter)
			{
				tis.RemoveAt(0);
			}
			_currentCharacter = tis[0]._tdCharacter;
			Wait(tis[0].GetTurnOrder());
			_currentCharacter.StartTurn();
			return tis[0]._cc;
		}
		public void Wait(float time)
		{
			foreach (Character c in _characters)
			{
				c.Wait(time);
			}
			UpdatePositions();
		}
		public void UpdatePositions()
		{
			foreach(Character c in _characters)
			{
				foreach (TurnIcon ti in c.GetIcons())
				{
					ti.UpdatePosition();
				}
			}
			foreach(Character c in _characters)
			{
				c.FillIcons();
			}
		}
		public void ClearCharacters()
		{
			if (_characters != null)
			{
				foreach(Character c in _characters)
				{
					c.Clear();
				}
			}
			_characters = new List<Character>();
		}
		public TurnIcon TimeOccupied(TurnIcon source)
		{
			float time = source.GetTurnOrder();
			float minDifference = 0.01f;
			foreach(Character c in _characters)
			{
				if (c == source._tdCharacter)
					continue;
				List<TurnIcon> tis = c.GetIcons();
				if (tis != null && tis.Count>0)
				{
					foreach(TurnIcon ti in tis)
					{
						if (ti == source)
						{
							Debug.LogError("Something weird");
							continue;
						}
						if (Mathf.Abs(ti.GetTurnOrder()-time)<minDifference)
						{
							Debug.Log(time + " of "+ source._cc.GetName() + " overlaps with\n" + ti.GetTurnOrder()+" of "+c._cc.GetName());
							return ti;
						}
					}
				}
			}
			//Debug.Log(time + " of " + source._cc.GetName() + " does NOT overlap with anything!");
			return null;
		}
		public void EndCurrentTurn()
		{
			if(_currentCharacter!=null)
				_currentCharacter.EndTurn();
		}
		Character GetCharacter(CombatCharacter cc)
		{
			foreach (Character c in _characters)
			{
				if (c._cc == cc)
				{
					return c;
				}
			}
			return null;
		}
		public static float GetSpeedOfCombatCharacter(CombatCharacter cc)
		{
			Character ch = null;
			foreach (Character c in _current._characters)
			{
				if (c._cc == cc)
				{
					ch = c;
					break;
				}
			}
			if (ch == null)
			{
				Debug.LogError(cc.GetName() + " not found in TurnDisplay order");
				return 0;
			}
			return ch.GetNextIcon();
		}
		public void RemoveCharacter(CombatCharacter cc)
		{
			Character c = GetCharacter(cc);
			if (c!=null)
			{
				c.Clear();
				_characters.Remove(c);
			}
		}
		public void TargetCharacter(CombatCharacter cc, bool enable)
		{
			Character c = GetCharacter(cc);
			if (c != null)
			{
				c.ToggleTarget(enable);
				c.Highlight(enable);
			}
		}
		public void ClearTargets()
		{
			foreach(Character c in _characters)
			{
				c.Untarget();
			}
		}
		public void HighlightCharacter(CombatCharacter cc, bool enable) {
			Character c = GetCharacter(cc);
			if (c != null)
			{
				//Debug.Log((!enable ? "Unh" : "H") + "ighlighting");
				c.Highlight(enable);
			}
		}
		List<HastePreview> _hastePrevs = new List<HastePreview>();
		class HastePreview
        {
			Character c;
			int haste;
			public HastePreview(Character character, int hasteAmount)
            {
				c = character;
				haste = hasteAmount;
				c.Haste(hasteAmount);
			}

			public void Revert()
            {
                if (c != null)
                {
					c.Haste(-haste);
					c = null;
                }
            }
        }
		public void PreviewHasteCharacter(CombatCharacter cc, int hasteAmount)
		{
			//Debug.Log("Preview " + hasteAmount + " haste for " + cc.GetName());
			if (hasteAmount == 0)
				return;
			//HastePreview.Revert();

			Character c = GetCharacter(cc);
			if (c != null)
            {
				_hastePrevs.Add(new HastePreview(c, hasteAmount));
            }

		}
		public bool CommitHastePreview()
        {
			bool hasHastePrevs = _hastePrevs.Count > 0;
			_hastePrevs.Clear();
			return hasHastePrevs;
        }
		public void RevertHastePreview()
        {
			foreach(HastePreview hp in _hastePrevs)
            {
				hp.Revert();
            }
			_hastePrevs.Clear();
			Refit();
        }
		public void HasteCharacter(CombatCharacter cc, int hasteAmount = 5)
        {
			Character c = GetCharacter(cc);
            if (c!=null)
			{
				c.Haste(hasteAmount);
            }
        }
		public void Refit()
		{
			foreach(Character c in _characters)
			{
				foreach (TurnIcon ti in c.GetIcons())
					if(!ti._currentTurn)ti._fitModifiers = 0;
			}
			foreach (Character c in _characters)
			{
				foreach (TurnIcon ti in c.GetIcons())
					if(!ti._currentTurn)ti.SelfFit();
			}
			UpdatePositions();
		}
		List<TurnIcon> GetTurnIconsInOrder()
		{
			List<TurnIcon> tis = new List<TurnIcon>();
			foreach (Character c in _characters)
				tis.AddRange(c.GetIcons());
			tis.Sort(CompareByAttraction);
			return tis;
			int CompareByAttraction(TurnIcon ti2, TurnIcon ti1)
			{
				return ti2.GetTurnOrder().CompareTo(ti1.GetTurnOrder());
			}
		}
		public void Log(int maxLogCount = 20)
		{
			List<TurnIcon> tis = GetTurnIconsInOrder();
			Debug.Log("------\nTurn order is as follows:");
			for(int i = 0; i < tis.Count && i < maxLogCount; i++)
			{
				Debug.Log((i + 1) + ". " + tis[i]._cc.GetName() + " --- " + tis[i].GetTurnOrder().ToString("0.0"));
			}
			Debug.Log("------");
		}
	}
}
