using RD.Dialog;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RD.DB
{
	public class DialogCharacter : BaseObject
	{
		public Faction _faction;
		public bool _useFactionColor = false;
		public Color _color;
		public bool _useDefaultSplashColors = true;
		public Color _darknessColor;
		public Color _whitenessColor;
		public TMP_FontAsset _splashFontAsset;
		public Material _splashFontMaterial;
		public BaseFXAudio _splashAudio;
		public string _prefix;
		public string _suffix;
		public List<string> _names = new List<string>();
		public Sprite _portrait;
		public DialogCharacter()
		{
			_layOutSpace = new LayOutSpace(new List<int> { 1, 3, 6  }, new List<string> {"Dialog", "Splash", "General"});
		}
		public string GetCharacter(string prefix = "", string suffix = "", string customName = default, bool link=false)
		{
			if (customName == default)
				customName = _name;
			string col = "<color="+"#"+ColorUtility.ToHtmlStringRGB(_color)+">";
			string lnk = link ? "<link=§" + _id + ">":"";
			string s = col +lnk+prefix+ _prefix + (_name!="Missing"?customName:"") + _suffix +suffix+(link?"</link>":"")+ "</color>";
			return s;
		}
		public string GetColor(Inkerface.DialogMode dialogMode = Inkerface.DialogMode.Dialog)
		{
			string colorStr;
			switch (dialogMode)
			{
				default:
					colorStr = ColorUtility.ToHtmlStringRGB(_color);
					break;
				case Inkerface.DialogMode.Darkness:
					colorStr = ColorUtility.ToHtmlStringRGB(_darknessColor);
					break;
			}
			return "<color=" + "#" + colorStr + ">";
		}
		public string GetCharacterRaw(string prefix = "", string suffix = "")
		{
			string s = prefix  + (_name != "Missing" ? _name : "") + suffix;
			return s;

		}
		public bool CheckName(string name, bool caseSensitive = false)
		{
			if (!caseSensitive)
				name = name.ToLower();
			if (name == (caseSensitive?_name:_name.ToLower()))
				return true;
			foreach (string n in _names)
				if (name == (caseSensitive ? n : n.ToLower()))
					return true;
			return false;
		}
		public enum Faction {None,Federation, Human_Bad, Human_Good, Elf_Bad, Elf_Good, Dwarf_Bad, Dwarf_Good, Beast, Mechanical}

		public Color GetFactionColor()
		{
			int i = (int)_faction;
			Color c = Color.white;
			switch (i)
			{
				case 0:default:
					return c;
				case 1:	//Federation = Queen Blue
					ColorUtility.TryParseHtmlString("#526782", out c);
					return c;
			}
		}

		public Hyperlink GenerateHyperlink()
		{
			Hyperlink hl = (Hyperlink)ScriptableObject.CreateInstance(typeof(Hyperlink));
			hl._name = _name;
			hl._description = _description;
			hl._actualDescription = _actualDescription;
			return hl;
		}

	}
}
