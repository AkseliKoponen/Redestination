using System.Collections.Generic;
using UnityEngine;

namespace RD.DB
{
	public class Hyperlink : BaseObject
	{

		[Tooltip("You can search for hyperlinks with either ID number or ID string")]
		public string _idString;
		public List<string> _optionalStrings = new List<string>();
		public Hyperlink()
		{
			_optionalStrings = new List<string>();
			_layOutSpace = new LayOutSpace(new List<int> { 1 }, new List<string> { "All" });
		}


		public bool CheckName(string name, bool caseSensitive = false)
		{
			if (!caseSensitive)
				name = name.ToLower();
			if (name == (caseSensitive ? _name : _name.ToLower()))
			{
				return true;
			}
			if (name == (caseSensitive ? _idString : _idString.ToLower()))
			{
				return true;
			}
			if (_optionalStrings != null)
				foreach (string n in _optionalStrings)
					if (name == (caseSensitive ? n : n.ToLower()))
					{
						return true;
					}
			return false;
		}


	}
}


