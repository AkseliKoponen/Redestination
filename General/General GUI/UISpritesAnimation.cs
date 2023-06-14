using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RD
{
	[RequireComponent(typeof(Image))]
	public class UISpritesAnimation : MonoBehaviour
	{
		public float _animationSpeed = 1f;
		public AnimationClip _animationClip;
		public bool _looping = true;

		float TimePerFrame;
		bool _running;
		public List<Sprite> _sprites = new List<Sprite>();
		private Image _image;
		private int _index = 0;
		private float Timer = 0;

		void Awake()
		{
			_image = GetComponent<Image>();
			Restart();
		}
		private void Restart()
		{
			TimePerFrame = _animationClip.length / _sprites.Count;
			Timer = 0;
			_index = 0;
			_running = true;
		}
		private void Update()
		{
			if (_running)
			{
				if ((Timer += Time.deltaTime*_animationSpeed) >= TimePerFrame)
				{
					Timer -= TimePerFrame;
					_image.sprite = _sprites[_index];
					if (!_looping && _index + 1 == _sprites.Count)
						enabled = false;
					_index = (_index + 1) % _sprites.Count;
				}
			}
		}
		public void Stop()
		{
			_running = false;
		}
		public void Resume(bool cont = true) {
			if (cont)
				_running = true;
			else
				Restart();
		}


	
	}
}