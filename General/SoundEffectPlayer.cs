using System.Collections;
using RD.DB;
using UnityEngine;
using UnityEngine.Audio;
using static RD.CodeTools;
namespace RD
{
	public class SoundEffectPlayer : MonoBehaviour
	{
		public AudioMixerGroup _audioMixerGroup;
		public static SoundEffectPlayer _current;
		private void Awake()
		{
			_current = this;
		}
		public void Play(BaseFXAudio bfa)
		{
			if (bfa._clips.Count <= 0) return;
			if (bfa._playCount > 1)
				StartCoroutine(PlaySounds(bfa));
			else
			{
				PlayRandomClip(bfa);
			}
		}
		IEnumerator PlaySounds(BaseFXAudio bfa)
		{
			float delay = bfa._playDelay;
			int count = 0;
			while (count < bfa._playCount)
			{
				PlayRandomClip(bfa);
				while (delay > 0)
				{
					delay -= Tm.GetGlobalDelta();
					yield return null;
				}
				count++;
				delay = bfa._playDelay;
				yield return null;
			}
		}
		void PlayRandomClip(BaseFXAudio bfa)
		{
			AudioSource aus = gameObject.AddComponent<AudioSource>() as AudioSource;
			aus.outputAudioMixerGroup = _audioMixerGroup;
			BaseFXAudio.Clip clip = bfa._clips[Random.Range(0, bfa._clips.Count)];
			aus.pitch = clip.pitch;
			aus.clip = clip.clip;
			aus.volume = clip.volume;
			aus.Play();
			StartCoroutine(DeleteAfterDelay(aus, clip.clip.length*2f));
			//Debug.Log("Playing "+clip.clip.name+" with volume " + clip.volume+"\n and pitch of "+clip.pitch);
			//Debug.Log("Currently " + GetComponents<AudioSource>().Length + " audiosources");
		}
		IEnumerator DeleteAfterDelay(Component c, float delay)
		{
			while (delay > 0)
			{
				delay -= Tm.GetGlobalDelta();
				yield return null;
			}
			Destroy(c);
		}
	}
}
