using UnityEngine;

namespace RD
{
	public class LerpMover : MonoBehaviour
	{
		public Vector3 oldPos;
		public Vector3 newPos;
		float lerpTimer;
		public float speed;
		public bool _enabled = false;
		public bool local;
		public bool boomerang;
		float zMod = 0;
		private void Update()
		{
			if (_enabled) {
				lerpTimer =lerpTimer + CodeTools.Tm.GetUIDelta() * speed;
				if (lerpTimer >= 1)
				{
					if (boomerang)
					{
						boomerang = false;
						Vector3 temp = oldPos;
						oldPos = newPos;
						newPos = temp;
						lerpTimer -= 1;
					}
					else
					{
						transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - zMod);
						_enabled = false;
					}
				}
				if (local)
					transform.localPosition = Vector3.Lerp(oldPos, newPos, lerpTimer);
				else
					transform.position = Vector3.Lerp(oldPos, newPos, lerpTimer);
			
			}
		}
		public void Move(Vector3 newPosition, float lerpSpeed = 1, bool useLocalPosition = true,float modifyZaxis=0, bool boomerang = false)
		{
			speed = lerpSpeed;
			newPos = newPosition;
			_enabled = true;
			local = useLocalPosition;
			lerpTimer = 0;
			if (local)
				oldPos = transform.localPosition;
			else
				oldPos = transform.position;
			zMod = modifyZaxis;
			transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + zMod);
		}

	}
}
