using System;
using System.Collections.Generic;
using UnityEngine;

namespace RD
{
	[CreateAssetMenu(fileName = "Name Generator", menuName = "Redestination/Name Generator", order = 1)]
	public class NameGenerator: ScriptableObject
	{
		public List<GeneratorName> names;
		public List<string> combinedNames;
		public bool tripleNames = false;
		[Serializable]
		public struct GeneratorName
		{
			[SerializeField]public string name;
			[SerializeField]public bool lastOnly;
		}
		public NameGenerator()
		{
			names = new List<GeneratorName>();
			combinedNames = new List<string>();
		}
		public List<string> GenerateNames()
		{
			if (names == null)
				return new List<string>();
			combinedNames = new List<string>();
			combinedNames.Clear();
			for(int i = 0; i < names.Count; i++)
			{
				if (!names[i].lastOnly)
				{
					for (int k = 0; k < names.Count; k++)
					{
						string s = names[i].name + names[k].name.ToLower();
						if (k != i && !combinedNames.Contains(s))
						{
							combinedNames.Add(s);
						}
						if(tripleNames && !names[k].lastOnly)
						{
							for(int j = 0; j < names.Count; j++)
							{
								s = names[i].name + names[k].name.ToLower()+names[j].name.ToLower();
								if (k != i && j!= i && j!=k && !combinedNames.Contains(s))
								{
									combinedNames.Add(s);
								}
							}
						}
					}
				}
			}
			return combinedNames;
		}
	}
}
