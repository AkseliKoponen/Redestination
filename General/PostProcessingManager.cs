using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RD
{
	public class PostProcessingManager : MonoBehaviour
	{
		[Serializable]
		public struct PPEffects {
			public Volume _timeOfDay;
			public Volume _backgroundFade;
			public Volume _backgroundBlack;
			public Volume _vignette;
			public Volume _critFlash;
		}
		[Serializable]
		public struct TimeOfDay
		{
			public VolumeProfile dawn;
			public VolumeProfile noon;
			public VolumeProfile dusk;
			public VolumeProfile night;
		}
		public PPEffects _postProcessingEffects;
		public TimeOfDay _timeOfDay;
		// Start is called before the first frame update
		private void Start()
		{
			Normalize();
			CodeTools.SetPostProcessingManager(this);
		}
		public void Normalize()
		{
			_postProcessingEffects._timeOfDay.enabled = true;
			_postProcessingEffects._backgroundFade.enabled = false;
			_postProcessingEffects._vignette.enabled = false;
			_postProcessingEffects._backgroundBlack.enabled = false;
			_postProcessingEffects._critFlash.enabled = false;
		}
		public void FadeBackground(bool darken)
		{
			_postProcessingEffects._backgroundFade.enabled = true;
			_postProcessingEffects._vignette.enabled = darken;
			_postProcessingEffects._backgroundBlack.enabled = darken;
		}
		public void CritFlash(bool enabled)
		{
			_postProcessingEffects._timeOfDay.enabled = !enabled;
			_postProcessingEffects._backgroundFade.enabled = !enabled;
			_postProcessingEffects._vignette.enabled = !enabled;
			_postProcessingEffects._backgroundBlack.enabled = !enabled;
			_postProcessingEffects._critFlash.enabled = enabled;
		}
		public void Target(bool enabled)
		{
			_postProcessingEffects._backgroundFade.enabled = enabled;
			_postProcessingEffects._vignette.enabled = enabled;
			//_postProcessingEffects._backgroundBlack.enabled = enabled;
		}
	}
}
