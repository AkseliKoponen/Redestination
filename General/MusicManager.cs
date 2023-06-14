using System.Collections;
using UnityEngine;

namespace RD
{
	public class MusicManager : MonoBehaviour
	{
		public static MusicManager _current;
		public static AudioSource _audioSource { get; private set; }
		private void Awake()
		{
			_current = this;
			_audioSource = GetComponent<AudioSource>();
		}

		public static void FadeToggle(bool activate, float time)
		{

			_current.StartCoroutine(FadeMusic(activate, time));
		}

		static IEnumerator FadeMusic(bool activate, float speed)
		{
			if (activate)
				_audioSource.Play();
			float startVolume = _audioSource.volume;
			float endVolume = activate ? 1 : 0;
			float time = 0;
			while (time < 1)
			{
				_audioSource.volume = Mathf.Lerp(startVolume, endVolume, time / 1);
				time += CodeTools.Tm.GetGlobalDelta() * speed;
				yield return null;
			}
			if(!activate)
				_audioSource.Pause();
		}

	}
}
