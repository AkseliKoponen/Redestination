using UnityEngine;

namespace RD
{
	public class Shaker : MonoBehaviour
	{
		Vector3 _force;
		float Time;
		float _speed;
		int _shakes = 0;
		public AnimationCurve _curve;
		Vector3 startPos;
		Vector3 startScale;
		Vector3 startAngles;
		ShakeType type = ShakeType.Null;
		public enum ShakeType {Null,Pos,Scale,Angles}
	
		// Update is called once per frame
		void Update()
		{
			if (_shakes != 0)
			{
				int t = (int)Time;
				Time += CodeTools.Tm.GetWorldDelta()*_speed;
				float value = _curve.Evaluate(Mathf.PingPong(Time, 1) - 0.5f);
				switch (type)
				{
				
					case ShakeType.Pos:
						transform.position = new Vector3(startPos.x + value * _force.x,
							startPos.y + value * _force.y,
							startPos.z + value * _force.z);
						break;
				}
				if (t < (int)Time)
				{
					_shakes--;
				}
				if (_shakes == 0)
					Kill();
			}	   
		}
		public void Kill()
		{

			Normalize();
			Destroy(this);
		}
		void Normalize()
		{
			if (type == ShakeType.Null)
				return;
			switch (type)
			{
				case ShakeType.Pos:
					transform.position = startPos;
					break;
				case ShakeType.Angles:
					transform.eulerAngles = startAngles;
					break;
				case ShakeType.Scale:
					transform.localScale = startScale;
					break;

			}


		}

		public void Shake(Vector3 force,float time = 5, int shakeCount = 1, ShakeType shakeType = ShakeType.Pos, AnimationCurve animationCurve = null)//use negative value for infinite shaking
		{
			Normalize();
			if (animationCurve == null)
			{
				if (_curve == null)
				{
					_curve = new AnimationCurve();
					_curve = UIAnimationTools.DefaultCurve(_curve,true);
				}
			}
			_force = force;
			_shakes = shakeCount;
			_speed = 1f/time*(_shakes>0?_shakes:1)*2;
			type = shakeType;
			startPos = transform.position;
			startAngles = transform.eulerAngles;
			startScale = transform.localScale;
		}
	}
}
