using System;
using System.Collections.Generic;
using UnityEngine;

namespace RD.Dialog
{
	[CreateAssetMenu(fileName = "New Conversation Actor List",menuName = "Redestination/Statics/Conversation Actor List",order = 0)]
	public class ConvActorScriptable : ScriptableObject
	{
		public List<ConvActor> _conversationActors;

		public ConvActor GetActorByID(string id)
		{
			foreach(ConvActor ca in _conversationActors)
			{
				if (ca.id.ToLower() == id.ToLower())
					return ca;
			}
			Debug.LogError("ConvActor " + id + " not found in list!");
			return null;
		}
	}

	[Serializable]
	public class ConvActor
	{

		public string id;
		public string name;
		public string styleID;
		public Sprite portrait;

		public ConvActor(ConvActor ca)
		{
			id = ca.id;
			name = ca.name;
			styleID = ca.styleID;
			if (styleID != "" && name != "")
				name = "<style=\"" + styleID + "\">" + name + "</style>";
			if (ca.portrait != null) portrait = ca.portrait;
		}
		public ConvActor (string _id)
		{
			id = _id;
			name = id;
		}
	}
}