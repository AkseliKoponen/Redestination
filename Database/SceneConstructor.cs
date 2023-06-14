using System.Collections.Generic;
using UnityEngine;

namespace RD.DB
{
	[CreateAssetMenu(fileName = "New SceneConstructor", menuName = "Redestination/SceneConstructor", order = 99)]
	public class SceneConstructor : BaseObject
	{
		public GameObject _background;
		public BaseFight _baseFight;
		public TextAsset _inkFile;
		public AudioClip _music;
		public SceneConstructor()
		{
			_layOutSpace = new LayOutSpace(new List<int> { 1 }, new List<string> { "SceneConstructor" });
		}
	}
}
