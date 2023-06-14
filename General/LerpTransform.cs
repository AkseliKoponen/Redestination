using UnityEngine;

namespace RD
{
	public class LerpTransform : MonoBehaviour
	{
		public LerpStyle _lerpStyle = LerpStyle.Oneshot;
		[Header("Position")]
		public bool lerpPosition = false;
		[Tooltip("1 = default, 2 = double speed")] public float _lerpSpeedPosition = 1;
		public bool _LocalPosition = true;
		public AnimationCurve _animationCurvePosX = AnimationCurve.Linear(0, 1, 0, 1);
		public bool _animationCurvePosXisAllAxis = true;
		public AnimationCurve _animationCurvePosY = AnimationCurve.Linear(0, 1, 0, 1);
		public AnimationCurve _animationCurvePosZ = AnimationCurve.Linear(0, 1, 0, 1);
		public Vector3 _lerpStartPosition;
		public Vector3 _lerpEndPosition;
		float _lerpTimePosition;
		bool _lerpPositionCurvatureEnabled = false;
		Vector3 _lerpPositionCurvature;
		AnimationCurve _curvatureCurve;
		//---------------------
		[Header("Rotation")]
		public bool lerpRotation = false;
		[Tooltip("1 = default, 2 = double speed")] public float _lerpSpeedRotation = 1;
		public bool _LocalRotation = true;
		public AnimationCurve _animationCurveRot = AnimationCurve.Linear(0, 1, 0, 1);
		public Vector3 _lerpStartRotation;
		public Vector3 _lerpEndRotation;
		public bool _lerpRotOnlyZ = true;
		float _lerpTimeRotation;
		//---------------------
		[Header("Scale")]
		public bool lerpScale = false;
		[Tooltip("1 = default, 2 = double speed")] public float _lerpSpeedScale = 1;
		public AnimationCurve _animationCurveScale = AnimationCurve.Linear(0,1,0,1);
		public Vector3 _lerpStartScale;
		public Vector3 _lerpEndScale;
		float _lerpTimeScale;
		//---------------------
		float _lerpRotateAmountTotal;
		float _lerpRotateAmountEnd;
		float _lerpRotateAmountRemaining;
		float _lerpRotateAmountStart;
		Vector3 _lerpRotateAroundPoint;
		bool _rotateAround = false;
		//---------------------
		public enum LerpStyle { Oneshot=1, Boomerang=2, Loop=3 }
		int lerpingStateScale;
		int lerpingStatePosition;
		int lerpingStateRotation;
		bool _paused = false;
		public bool debug = false;
		// Update is called once per frame
		void Update()
		{
			if (!_paused)
			{
				float time = CodeTools.Tm.GetGlobalDelta();
				if (lerpingStateScale > 0)
					LerpScale(time);
				if (lerpingStateRotation > 0)
					LerpRotation(time);
				if (lerpingStatePosition > 0)
					LerpPosition(time);
			}
		}

		public void StartLerpPosition(Vector3 endPosition, bool local = true, float speed = 1f, LerpStyle style = LerpStyle.Oneshot)
		{
			_lerpPositionCurvatureEnabled = false;
			_lerpEndPosition = endPosition;
			if (float.IsInfinity(speed))
			{
				Debug.LogError("Lerp speed infinity -> speed = 1");
				speed = 1f;
			}
			_lerpSpeedPosition = speed;
			_lerpStartPosition = local ? transform.localPosition : transform.position;
			_lerpTimePosition = 0f;
			_lerpStyle = style;
			_LocalPosition = local;
			lerpingStatePosition = (int)style;
			if (debug) Debug.Log("startPos = " + _lerpStartPosition + "\nendpos = " + _lerpEndPosition);
		}
		public void StartLerpPositionCurved(Vector3 endPosition, Vector3 curvature,AnimationCurve curvatureCurve,bool local = true, float speed = 1f, LerpStyle style = LerpStyle.Oneshot)
		{
			//KURWA
			StartLerpPosition(endPosition, local, speed, style);
			_lerpPositionCurvature = curvature;
			_lerpPositionCurvatureEnabled = true;
			_curvatureCurve = curvatureCurve;
		}
		/*
	public void StartLerpRotation(Vector3 endRotation, bool local = true, float speed = 1f, LerpStyle style = LerpStyle.Oneshot)
	{
		_lerpEndRotation = endRotation;
		if (float.IsInfinity(speed))
		{
			Debug.LogError("Lerp speed infinity -> speed = 1");
			speed = 1f;
		}
		_lerpSpeedRotation = speed;
		_lerpStartRotation = local?transform.localEulerAngles:transform.eulerAngles;
		_lerpTimeRotation = 0f;
		_lerpStyle = style;
		_LocalRotation = local;
		lerpingStateRotation = (int)style;
	}
	*/
		public void StartLerpRotate(float degrees, float speed = 1f, LerpStyle style = LerpStyle.Oneshot)
		{
			//degrees = Mathf.DeltaAngle(0, degrees);
			_lerpRotateAmountTotal = degrees;
			_lerpRotateAmountEnd = degrees + transform.localEulerAngles.z;
			_lerpRotateAmountStart = transform.localEulerAngles.z;
			_lerpRotateAmountRemaining = degrees;
			if (float.IsInfinity(speed))
			{
				Debug.LogError("Lerp speed infinity -> speed = 1");
				speed = 1f;
			}
			_lerpSpeedRotation = speed;
			_lerpTimeRotation = 0f;
			_lerpStyle = style;
			_rotateAround = false;
			lerpingStateRotation = (int)style;
		}
		public void StartLerpRotate(Vector3 rotateAroundPoint,float degrees, float speed = 1f, LerpStyle style = LerpStyle.Oneshot)
		{
			//degrees = Mathf.DeltaAngle(0, degrees);
			_lerpRotateAroundPoint = rotateAroundPoint;
			_rotateAround = true;
			_lerpRotateAmountTotal = degrees;
			_lerpRotateAmountEnd = degrees + transform.localEulerAngles.z;
			_lerpRotateAmountStart = transform.localEulerAngles.z;
			_lerpRotateAmountRemaining = degrees;
			if (float.IsInfinity(speed))
			{
				Debug.LogError("Lerp speed infinity -> speed = 1");
				speed = 1f;
			}
			_lerpSpeedRotation = speed;
			_lerpTimeRotation = 0f;
			_lerpStyle = style;
			lerpingStateRotation = (int)style;

		}
		public void StartLerpScale(Vector3 endScale, float speed = 1f, LerpStyle style = LerpStyle.Oneshot, AnimationCurve animationCurve = default)
		{
			if (animationCurve != default)
				_animationCurveScale = animationCurve;
			_lerpEndScale = endScale;
			if (float.IsInfinity(speed))
			{
				Debug.LogError("Lerp speed infinity -> speed = 1");
				speed = 1f;
			}
			_lerpSpeedScale = speed;
			_lerpStartScale = transform.localScale;
			_lerpTimeScale = 0f;
			_lerpStyle = style;
			lerpingStateScale = (int)style;
		}

		void LerpPosition(float dt)
		{
			_lerpTimePosition += dt * _lerpSpeedPosition;
			if (debug) Debug.Log(gameObject.name + " -LerpPos-\n Delta = " + dt + "\nTotal time = " + _lerpTimePosition);
			LerpPosSet();
			if (_lerpTimePosition >= 1)
			{
				switch (_lerpStyle)
				{
					case LerpStyle.Loop:
						_lerpTimePosition -= 1;
						Vector3 tempPos = _lerpEndPosition;
						_lerpEndPosition = _lerpStartPosition;
						_lerpStartPosition = tempPos;
						LerpPosSet();
						break;
					case LerpStyle.Boomerang:
						lerpingStatePosition--;
						if (lerpingStatePosition > 0)
						{
							_lerpTimePosition -= 1;
							Vector3 tempPosx = _lerpEndPosition;
							_lerpEndPosition = _lerpStartPosition;
							_lerpStartPosition = tempPosx;
							LerpPosSet();
						}
						break;
					case LerpStyle.Oneshot:
						lerpingStatePosition = 0;
						_lerpTimePosition -= 1;
						break;
				}
			}
		}
		void LerpPosSet()
		{
			if (_animationCurvePosXisAllAxis)
			{
				Vector3 newPos = Vector3.LerpUnclamped(_lerpStartPosition, _lerpEndPosition, _animationCurvePosX.Evaluate(Mathf.Clamp(_lerpTimePosition, 0, 1)));
				if (_lerpPositionCurvatureEnabled) newPos += _curvatureCurve.Evaluate(_lerpTimePosition) * _lerpPositionCurvature;
				//Vector3 alternateLerp = Vector3.Lerp(_lerpStartPosition, _lerpEndPosition, _lerpTimePosition);
				if (_LocalPosition)
					transform.localPosition = newPos;
				else
					transform.position = newPos;
				//if(debug)Debug.Log("Lerppos = " + newPos+"\nposition = "+transform.position+ "\nalternateLerp = " + alternateLerp);
			}
			else
			{
				Vector3 newPos = new Vector3();
				newPos.x = Mathf.LerpUnclamped(_lerpStartPosition.x, _lerpEndPosition.x, _animationCurvePosX.Evaluate(_lerpTimePosition));
				newPos.y = Mathf.LerpUnclamped(_lerpStartPosition.y, _lerpEndPosition.y, _animationCurvePosY.Evaluate(_lerpTimePosition));
				newPos.z = Mathf.LerpUnclamped(_lerpStartPosition.z, _lerpEndPosition.z, _animationCurvePosZ.Evaluate(_lerpTimePosition));
				if (_lerpPositionCurvatureEnabled)
				{
					newPos += _curvatureCurve.Evaluate(_lerpTimePosition) * _lerpPositionCurvature;
					//Debug.Log(_lerpTimePosition + "  =  "+ _curvatureCurve.Evaluate(_lerpTimePosition));
				}
				if (_LocalPosition)
					transform.localPosition = newPos;
				else
					transform.position = newPos;
			}
		}


		void LerpRotation(float dt)
		{
			if (debug) Debug.Log(gameObject.name + " -LerpRot- " + dt);

			_lerpTimeRotation += dt * _lerpSpeedRotation;
			float turnamount = Mathf.Lerp(_lerpRotateAmountStart,_lerpRotateAmountEnd,_lerpTimeRotation);
			turnamount = Mathf.DeltaAngle(transform.localEulerAngles.z, turnamount);
			if (!_rotateAround)
			{
				transform.Rotate(new Vector3(0, 0, turnamount));
			}
			else
			{
				transform.RotateAround(_lerpRotateAroundPoint, new Vector3(0, 0, 1), turnamount);
			}
			_lerpRotateAmountRemaining += _lerpRotateAmountTotal > 0 ? turnamount : turnamount * -1;
			_lerpRotateAmountRemaining = Mathf.Clamp(_lerpRotateAmountRemaining, (_lerpRotateAmountTotal > 0 ? 0 : _lerpRotateAmountTotal), (_lerpRotateAmountTotal > 0 ? _lerpRotateAmountTotal: 0));
			if (_lerpTimeRotation >= 1)
			{
				_lerpRotateAmountRemaining = 0;
				transform.localEulerAngles =new Vector3(0,0, _lerpRotateAmountEnd);
				switch (_lerpStyle)
				{
					case LerpStyle.Loop:
						_lerpTimeRotation -= 1;
						_lerpRotateAmountTotal *= -1;
						_lerpRotateAmountEnd = _lerpRotateAmountTotal + transform.localEulerAngles.z;
						_lerpRotateAmountStart = transform.localEulerAngles.z;
						break;
					case LerpStyle.Boomerang:
						lerpingStateRotation--;
						if (lerpingStateRotation > 0)
						{
							_lerpTimeRotation -= 1;
							_lerpRotateAmountTotal *= -1;
							_lerpRotateAmountEnd = _lerpRotateAmountTotal + transform.localEulerAngles.z;
							_lerpRotateAmountStart = transform.localEulerAngles.z;
						}
						break;
					case LerpStyle.Oneshot:
						lerpingStateRotation = 0;
						_lerpTimeRotation -= 1;
						break;
				}
			}
		}


		void LerpScale(float dt)
		{
			if (debug)
				Debug.Log(gameObject.name + " -LerpScale- " + dt);
			_lerpTimeScale += dt * _lerpSpeedScale;
			//transform.localScale = Vector3.Lerp(_lerpStartScale, _lerpEndScale, _lerpTimeScale);
			transform.localScale = Vector3.Lerp(_lerpStartScale, _lerpEndScale, _animationCurveScale!=null?_animationCurveScale.Evaluate( Mathf.Clamp(_lerpTimeScale,0,1)):Mathf.Clamp(_lerpTimeScale,0,1));
			if (_lerpTimeScale >= 1)
			{
				switch (_lerpStyle)
				{
					case LerpStyle.Loop:
						_lerpTimeScale -= 1;
						Vector3 tempScale = _lerpEndScale;
						_lerpEndScale = _lerpStartScale;
						_lerpStartScale = tempScale;
						transform.localScale = Vector3.Lerp(_lerpStartScale, _lerpEndScale, _animationCurveScale.Evaluate(Mathf.Clamp(_lerpTimeScale, 0, 1)));
						break;
					case LerpStyle.Boomerang:
						lerpingStateScale--;
						if (lerpingStateScale > 0)
						{
							_lerpTimeScale -= 1;
							Vector3 tempScalex = _lerpEndScale;
							_lerpEndScale = _lerpStartScale;
							_lerpStartScale = tempScalex;
							transform.localScale = Vector3.Lerp(_lerpStartScale, _lerpEndScale, _animationCurveScale.Evaluate(Mathf.Clamp(_lerpTimeScale, 0, 1)));
						}
						else
						{
							_lerpTimeScale -= 1;
						}
						break;
					case LerpStyle.Oneshot:
						lerpingStateScale = 0;
						_lerpTimeScale -= 1;
						break;
				}
			}
		}

		public void EndLerp()
		{
			_lerpTimeRotation = 1;
			_lerpTimeScale = 1;
			_lerpTimePosition = 1;
			Update();
		}
		public bool LerpDone()
		{
			return (lerpingStatePosition <= 0 && lerpingStateRotation <= 0 && lerpingStateScale <= 0);

		}
		public void StopAll()
		{
			lerpingStatePosition = 0;
			lerpingStateScale = 0;
			lerpingStateRotation = 0;
		}
		public void SetPause(bool paused)
		{
			_paused = paused;
		}

		public void RandomizeTimeScale(float min = 0.1f, float max = 0.9f)
		{
			_lerpTimeScale = Random.Range(min, max);
		}
		public void RandomizeTimeRotation(float min = 0.1f, float max = 0.9f)
		{
			_lerpTimeRotation = Random.Range(min, max);
		}
		public void RandomizeTimePosition(float min = 0.1f, float max = 0.9f)
		{
			_lerpTimePosition = Random.Range(min, max);
		}
	}
}
