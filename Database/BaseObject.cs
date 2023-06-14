using System.Collections.Generic;
using RD;
using UnityEngine;

namespace RD.DB
{
	public class BaseObject : ScriptableObject
	{
		public string _name;
		public int _id;		//Set to public if need emergency fix
		public string _description;
		public bool _requireSpecialCode;
		public string _actualDescription;
		public bool _debugLog = false;
	
		protected LayOutSpace _layOutSpace;
		public LayOutSpace GetLayout()
		{
			return _layOutSpace;
		}
	
		public string GetName()
		{
			return _name == "" ? "MISSING" : _name;
		}
		public string GetFileName()
		{
			return _id + " - " + _name;
		}
		public void SetID(int id)
		{
			_id = id;
		}
		public int GetID()
		{
			return _id;
		}

		public void UpdateDescription(BaseCollection baseCollection = null)
		{
			bool load = baseCollection == null;
			if (load)
				baseCollection = Resources.Load<BaseCollection>("Database/Collection");
			_actualDescription = Translator.AddStyleAndHyperlinks(_description, this, baseCollection);
			_descriptionSaved = _description;
			if(load)
				Resources.UnloadAsset(baseCollection);
			//Debug.Log("_actualDescription Updated. Use GetDescription() to get it.");
		}
		public List<BaseObject> _links = new List<BaseObject>();
		public void AddLink(BaseObject obj)
		{
			if (_links == null)
				_links = new List<BaseObject>();
			else
				foreach (BaseObject hl in _links)
					if (hl !=  obj)
						return;
			_links.Add(obj);
		}
		public List<BaseObject> GetLinks()
		{
			List<BaseObject> bobs = new List<BaseObject>();
			foreach (BaseObject hl in _links)
				bobs.Add(hl);
			return bobs;
		}
		public string GetDescription()
		{
			if (_actualDescription == "")
				_actualDescription = _description;
			return _actualDescription;
		}
		[SerializeField] string _descriptionSaved;
		public bool GetDescriptionModified()
		{
			return _descriptionSaved == _description;
		}
	}


	public class LayOutSpace
	{

		public List<int> _spaces = new List<int>();
		public List<bool> layOuts = new List<bool>();
		public List<string> _names = new List<string>();
		public LayOutSpace(List<int> spaces, List<string> names)
		{
			_spaces = spaces;
			_names = names;
			layOuts.Clear();
			for (int i = 0; i < _spaces.Count; i++)
			{
				layOuts.Add(false);
				if (i == _spaces.Count - 1)
				{
					layOuts[i] = true;
				}
			}
		}
		public LayOutSpace()
		{
			_spaces = new List<int> { 1 };
			_names = new List<string> { "" };
			for (int i = 0; i < _spaces.Count; i++)
			{
				layOuts.Add(false);
				if (i == _spaces.Count - 1)
				{
					layOuts[i] = true;
				}
			}
		}
		public void AddLayOutSpace(LayOutSpace lsp, bool after = true)
		{
			if (after)
			{
				_spaces.AddRange(lsp._spaces);
				_names.AddRange(lsp._names);
				layOuts.AddRange(lsp.layOuts);
			}
			else
			{
				_spaces.InsertRange(0, lsp._spaces);
				_names.InsertRange(0,lsp._names);
				layOuts.InsertRange(0,lsp.layOuts);
			}
		}

	}
}