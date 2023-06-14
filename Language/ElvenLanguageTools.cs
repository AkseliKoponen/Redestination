using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RD
{
	[CreateAssetMenu(fileName = "Elven Dictionary",menuName = "Redestination/Statics/Elven Dictionary")]
	public class ElvenLanguageTools : ScriptableObject
	{
		[Serializable]
		public class Translation
		{
			[SerializeField] public string _originalLanguage;
			[SerializeField]public List<string> _englishLanguage = new List<string> { ""};
		}
		[SerializeField]public List<Translation> _dictionary = new List<Translation>();
		public string FindTranslation(string english, bool dbug = true)
		{
			foreach (Translation t in _dictionary)
				if (t._englishLanguage.Contains(english))
					return t._originalLanguage.ToLower();
			if(dbug)Debug.Log("<color=red>Translation not found for '" + english + "'</color>.");
			return "<untranslated>";
		}
		public bool HasWord(string elven)
		{
			foreach (Translation t in _dictionary)
				if (t._originalLanguage.ToLower() == elven.ToLower())
					return true;
			return false;
		}
		public string TranslateNumber(string numbertext)
		{
			float number;
			if (!float.TryParse(numbertext, out number))
			{
				Debug.Log(numbertext + " is not a valid number");
				return "";
			}
			else
				number = float.Parse(numbertext);
			string str = FindTranslation(number.ToString(),false);
			int i = (int)number;
			int tempi = i;
			if (str == "<untranslated>")
			{
				str = "";
				AddTens(283);
				AddTens(94);
				AddTens(31);
				int fives = (tempi-1) / 5;
				if (fives > 0)
					str += FindTranslation(fives.ToString());
				tempi -= (fives*5);
				int singles = tempi;
				if(singles>0)
					str += FindTranslation(singles.ToString());
			}
			
			Debug.Log("<color=green>"+numbertext + " = '" + str + "' in elvish</color>");
			void AddTens(int value)
			{
				if (tempi < value)
					return;
				string valuetxt = FindTranslation(value.ToString());
				while (tempi >= value)
				{
					str += valuetxt;
					tempi -= value;
				}
			}
			return str;
		}
		public void ImportTranslation(TextAsset csv)
		{
			string fulltext = csv.ToString();
			string txt = fulltext;
			int backup = 50;
			while (txt.Contains("\n") && backup>0)
			{
				ReadLine();
				backup--;
			}
			void ReadLine()
			{
				txt = txt.Substring(txt.IndexOf("\n")+1);
				int comma = txt.IndexOf(",");
				string elven = txt.Substring(0, comma);
				string english = txt.Substring(comma+1, txt.Substring(comma+1).IndexOf(","));
				Debug.Log(elven + " = " + english);
				if(HasWord(elven) == false)
				{
					Translation t = new Translation();
					t._englishLanguage.Clear();
					if (english.Contains("/"))
					{
						t._englishLanguage.Add(english.Substring(0, english.IndexOf("/")).ToLower());
						t._englishLanguage.Add(english.Substring(english.IndexOf("/")+1).ToLower());

					}
					else
						t._englishLanguage.Add(english.ToLower());
					t._originalLanguage = elven.ToLower();
					_dictionary.Add(t);
				}
			}
		}
	}
}