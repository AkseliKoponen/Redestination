using UnityEngine;

namespace RD
{
	public class ParticleSystemAutoDestroy : MonoBehaviour
	{
		private ParticleSystem _particleSystem;


		public void Start()
		{
			_particleSystem = GetComponent<ParticleSystem>();
		}

		public void Update()
		{
			if (_particleSystem)
			{
				if (!_particleSystem.IsAlive())
				{
					Destroy(gameObject);
				}
			}
		}
	}
}