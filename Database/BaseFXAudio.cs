using System.Collections.Generic;
using UnityEngine;

namespace RD.DB
{
	public class BaseFXAudio : BaseObject
	{
		[System.Serializable]
		public struct Clip
		{
			public AudioClip clip;
			public float volume;
			public float pitch;
			public Clip(bool tru = true)
			{
				volume = 1;
				clip = null;
				pitch = 1;
			}
			public Clip(AudioClip ac)
			{
				volume = 1;
				clip = ac;
				pitch = 1;
			}
		}
		[Tooltip("If multiple clips, plays one at random")]
		public List<Clip> _clips;
		public int _playCount;
		public float _playDelay;
		public BaseFXAudio()
		{
			_clips = new List<Clip>();
			_playCount = 1;
			_playDelay = 0;
			_layOutSpace = new LayOutSpace(new List<int> { 1,1 }, new List<string> { "Clips","General" });
		}

		public void AddClips(Object[] selectedObjects)
		{
			foreach(Object obj in selectedObjects)
			{
				_clips.Add(new Clip((AudioClip)obj));
			}
		}
	}
}
