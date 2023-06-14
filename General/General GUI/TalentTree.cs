using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RD.DB;

namespace RD.UI
{
	[CreateAssetMenu(fileName = "New TalentTree", menuName = "Redestination/Misc/TalentTree", order = 1)]
	public class TalentTree : ScriptableObject
	{
		public int _length = 4;
		public List<BaseTalent> _column1;
		public List<BaseTalent> _column2;
		public List<BaseTalent> _column3;

		public TalentTree()
		{
			_column1 = new List<BaseTalent>();
			_column2 = new List<BaseTalent>();
			_column3 = new List<BaseTalent>();
		}
		public BaseTalent GetTalentFromIndex(int index,bool horizontalIndex=true)
		{
			List<BaseTalent> templist = new List<BaseTalent>();
			if (!horizontalIndex)
			{
				templist.AddRange(_column1);
				templist.AddRange(_column2);
				templist.AddRange(_column3);
			}
			else
			{
				
				int x = 0;
				int y = 0;
				while (y < _length)
				{
					while (x < 3)
					{
						if (x == 0) templist.Add(_column1[y]);
						if (x == 1) templist.Add(_column2[y]);
						if (x == 2) templist.Add(_column3[y]);
						x++;
					}
					x = 0;
					y++;
				}
			}
			return templist[index];
		}
		public int GetLength()
		{
			return _column1.Count + _column2.Count + _column3.Count;
		}
	}
}
