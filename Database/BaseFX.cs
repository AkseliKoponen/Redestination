using System;
using System.Collections.Generic;
using UnityEngine;

namespace RD.DB
{
	public class BaseFX:BaseObject
	{
		public BaseFXAnimation _prepareAnimation;
		public BaseFXAnimation _focusAnimation;
		public BaseFXAnimation _postAnimation;
		public bool _darkenBackground = true;
		public bool _zoom = true;
		public bool _showSelf = true;
		public bool _showTarget = true;

		public BaseFX()
		{
			_prepareAnimation = new BaseFXAnimation("none");
			_focusAnimation = new BaseFXAnimation("focus");
			_postAnimation = new BaseFXAnimation("none");
			_layOutSpace = new LayOutSpace(new List<int> { 1,1,1,1 }, new List<string> { "Prepare", "Focus", "Post","General" });
		}
		[Serializable]
		public class BaseFXAnimation
		{
			public float Time; //= 0.5f;
			public AnimationName _animationSelf;
			public AnimationName _animationTarget;
			public List<ParticleEffect> _particleEffects;
			public bool _useWeaponAudio;
			public BaseFXAudio _audio;
			public BaseFXAnimation(string baseType = "none")
			{
				switch (baseType)
				{
					default:
						Time = 0;
						break;
					case "strike":
						Time = 1.25f;
						//_animationSelf = AnimationName.PrepareStrike;
						//_animationTarget = AnimationName.TakeHit;
						break;
					case "focus":
						Time = 1f;
						break;
					case "post":
						Time = 0f;
						//_animationSelf = AnimationName.Stop;
						//	_animationTarget = AnimationName.Stop;
						break;
				}
				_useWeaponAudio = false;
				_particleEffects = new List<ParticleEffect>();
			}

		}

		[Serializable]
		public struct ParticleEffect
		{
			public BaseCard.TargetType _target;
			public GameObject _particle;
			public ParticleEffect(bool tru)
			{
				_target = BaseCard.TargetType.Self;
				_particle = null;
			}
		}
		[Serializable]
		public enum AnimationName
		{
			None,
			Idle,
			PrepareStrike,
			Strike,
			Strike2,
			PrepareCast,
			Cast,
			Cast2,
			Move,
			Move2,
			Move3,
			Item,
			TakeHit,
			Sit,
			Dead
		}
	}
}
