using RD;
using TMPro;
using UnityEngine;

namespace RD.Combat
{
	[RequireComponent(typeof(TextMeshPro))]
	[RequireComponent(typeof(Rigidbody2D))]
	public class DamageText : MonoBehaviour
	{
		CombatCharacterVisuals _ccv;
		TextMeshPro _textMesh;
		float _minY;
		Rigidbody2D _rb;
		[SerializeField] float _riseSpeed;
		[SerializeField] Vector2 forceMultiplier;
		string _txtType ="";
		string _style;
		string _gradient;

		/// <summary>
		/// <para>Sets the standard settings for text depending on textType. Automatically destroys itself after duration.</para>
		/// <br>. . "dmg" -> normal bouncy hit</br>
		/// <br>. . "aura"-> aura message</br>
		/// <br>. . "barrier"-> floaty heal</br>
		/// <br>. . "heal"-> floaty heal</br>
		/// <br>. . "dot" -> small floaty damage for dots</br>
		/// <br>. . "static" -> a large static text</br>
		/// </summary>
		public void Init(CombatCharacterVisuals ccv, string txt,float minY, string textType, float duration = 2f, int direction = 0)
		{
			_ccv = ccv;
			_textMesh = GetComponent<TextMeshPro>();
			_rb = GetComponent<Rigidbody2D>();
			_txtType = textType;
			AssignTextType();

			string completeStr = _style +  _gradient + txt + "</s>" + "</gradient>";
			GetComponent<LerpColor>().StartLerpAlpha(0, 1, 1 / duration);
			_textMesh.text = completeStr;
			Invoke("Clear", duration);


			void AssignTextType()
			{
				switch (_txtType)
				{
					default:
						_style = "<style=Damage>";
						_gradient = "<gradient=DamageGrad>";
						Debug.LogError("Unimplemented txtType '" + _txtType + "'");
						GetComponent<LerpTransform>().StartLerpScale(Vector3.zero, 1 / duration);
						break;

					case "dmg":
						_style = "<style=Damage>";
						_gradient = "<gradient=DamageGrad>";
						float distanceFromScreenEdge = _ccv.GetPositionOnScreen().x;
						if ((distanceFromScreenEdge < 0 && direction > 0) || (distanceFromScreenEdge > 0 && direction < 0))
							distanceFromScreenEdge = 1 + Mathf.Abs(distanceFromScreenEdge);
						else
							distanceFromScreenEdge = 1 - Mathf.Abs(distanceFromScreenEdge);
						Bounce(direction, distanceFromScreenEdge);
						GetComponent<LerpTransform>().StartLerpScale(Vector3.zero, 1 / duration);
						break;

					case "heal":
						_style = "<style=Heal>";
						_gradient = "<gradient=HealGrad>";
						GetComponent<LerpTransform>().StartLerpScale(Vector3.zero, 1 / duration);
						break;
					case "barrier":
						_style = "<style=Barrier>";
						_gradient = "<gradient=BarrierGrad>";
						break;
					case "aura":
						_style = "<style=Aura>";
						_gradient = "<gradient=AuraGrad>";
						break;

					case "dot":
						_style = "<style=Dot>";
						_gradient = "<gradient=DamageGrad>";
						GetComponent<LerpTransform>().StartLerpScale(Vector3.zero, 1 / duration);
						break;

					case "static":
						_style = "<style=Result>";
						_gradient = "<gradient=DamageGrad>";
						break;
				}
			}
		}
		void Clear()
		{
			Transform papa = transform.parent;
			transform.SetParent(null);
			if (papa.childCount == 0)
				Destroy(papa.gameObject);
			Destroy(gameObject);
		}

		private void Update()
		{
			MoveText();

			void MoveText()
			{
				switch (_txtType)
				{
					case "dmg":
						Vector3 pos = transform.position;
						if (pos.y < _minY)
						{
							pos.y = _minY;
							transform.position = pos;
							//_rb.simulated = false;
							_rb.velocity = new Vector2(_rb.velocity.x * 0.75f, 0);
							_rb.AddForce(new Vector2(0, forceMultiplier.y));
							forceMultiplier.y *= 0.67f;
						}
						break;
					default:
						transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + CodeTools.Tm.GetWorldDelta() * _riseSpeed, transform.localPosition.z);
						break;
					case "static":
						break;
				}
			}
		}

		public void Flash(Color? color, float flashSpeed = 0.5f, int flashCount = 1)
		{
			Color c = color ?? Color.white;
		}


		public void Bounce(float direction, float distanceFromEdge = 0.5f)
		{
			Vector2 xForce = new Vector2(15, 20)*distanceFromEdge;
			float x = Random.Range(xForce.x, xForce.y) * forceMultiplier.x;
			float y = Random.Range(0.85f, 1.15f) * forceMultiplier.y;
			_rb.simulated = true;
			x = direction > 0 ? x : -x;
			_rb.AddForce(new Vector2(x, y));
			float torque = 0.08f;
			_rb.AddTorque(direction>0?-torque:torque);
		}
	}
}
