using System.Collections;
using RD;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RD.Combat
{
	[RequireComponent(typeof(Image))]
	public class TurnIcon : MonoBehaviour
	{
		RectTransform _rt;
		Image _image;
		[SerializeField] Image _crossHair;
		public Image _highlightFrame;
		TextMeshProUGUI _text;
		float _waitedTime = 0;
		[System.NonSerialized] public CombatCharacter _cc;
		public TurnDisplay.Character _tdCharacter;
		int _defaultSpeed;
		int _hasteModifiers;
		public int _fitModifiers = 0;
		float _speedMultiplier = 0.1f;
		[System.NonSerialized] public bool _currentTurn = false;
		public void Init(TurnDisplay.Character cha, float delay = 0)
		{
			_image = GetComponent<Image>();
			_text = GetComponentInChildren<TextMeshProUGUI>();
			_rt = GetComponent<RectTransform>();
			_tdCharacter = cha;
			_cc = cha._cc;
			gameObject.name = "Turn Icon " + _cc.GetName() + transform.GetSiblingIndex();
			_waitedTime = 0 - delay;
			if (_cc._turnIcon)
			{
				_image.sprite = _cc._turnIcon;
				_text.text = "";
			}
			else
			{

				_text.alignment = TextAlignmentOptions.Center;
				_image.color = Color.black;//_image.enabled = false;
				_text.text = _cc._name.abbrev;
			}
			if (_cc._alliance == CombatCharacter.Alliance.Player)
				_defaultSpeed *= 2;
			//Set Icon
			_defaultSpeed = _cc._stats.speed;
			_hasteModifiers = 0;
			SelfFit();
		}
		public void SelfFit()
		{
			Fit(TurnDisplay._current.TimeOccupied(this));
		}
		public void Fit(TurnIcon source)
		{
			if (source == null)
				return;
			TurnIcon delayedIcon;
			//Choose which icon gets delayed, this or the source
			if (_currentTurn)
				delayedIcon = source;
			else if (source._currentTurn)
				delayedIcon = this;
			else if (_fitModifiers > source._fitModifiers)
				delayedIcon = this;
			else
				delayedIcon = source;

			delayedIcon._fitModifiers -= 1;
			delayedIcon.UpdatePosition();
			//Debug.Log(source.gameObject.name + " is delaying " + delayedIcon.gameObject.name + ".\nFit modifiers = " + delayedIcon._fitModifiers);
			TurnIcon ti = TurnDisplay._current.TimeOccupied(delayedIcon);
			if (ti != null)
			{
				ti.Fit(delayedIcon);
			}
		}
		public void Haste(int hasteAmount)
		{
			_hasteModifiers = Mathf.Clamp(hasteAmount+_hasteModifiers,-9999,4);
		}

		public void Wait(float time)
		{
			_waitedTime += time;

		}
		public float StartTurn(bool automaticOverflow = true)
		{
			//reset modifiers and turn time

			/*In rare cases, where the waited time is more than the character should have waited,
		 * (one gets hasted right before their turn starts)
		 * the extra speed is carried over as "overflow" to next turn
		 */
			_currentTurn = true;
			float overflow = GetTurnOrder();
			//_waitedTime = 999999;
			//_hasteModifiers = 0;
			if (overflow > 0 && automaticOverflow)
			{
				return overflow;
			}
			else
				return 0;
		}
		public int GetIntSpeed()
		{
			return _defaultSpeed + _hasteModifiers;
		}
		float GetSpeed()
		{
			float f = TurnDisplay._turnMax - (_speedMultiplier * (_defaultSpeed + _hasteModifiers + _fitModifiers));
			return f;
		}
		public float GetTurnOrder()
		{
			float f = GetSpeed() - _waitedTime;
			return f;
		}
		float _newY;
		Task _lerpTask;
		public void UpdatePosition(bool immediate = false)
		{
			if (_currentTurn)
				return;
			if (_lerpTask != null && _lerpTask.Running)
			{
				//Debug.Log(gameObject.name+"LerpTask.Stop()");
				_lerpTask.Stop();
				//_rt.anchoredPosition = new Vector2(0, _newY);
			}
			float yMax = _rt.parent.GetComponent<RectTransform>().rect.height;
			_newY = Mathf.Lerp(0, yMax, GetTurnOrder() / TurnDisplay._turnMax) * -1;
			//Debug.Log((immediate ? "" : "not ")+ "immediate");
			if (immediate || Mathf.Abs(_newY - _rt.anchoredPosition.y) < 0.1)
			{
				_rt.anchoredPosition = new Vector2(_rt.anchoredPosition.x, _newY);   //if tiny difference, move instantly
			}
			else
			{
				//Debug.Log("Starting lerpitude");
				//if noticeable difference, lerp into place
				_lerpTask = new Task(LerpTo(new Vector2(_rt.anchoredPosition.x, _newY)));
			}
		}
		public IEnumerator LerpTo(Vector2 endPos)
		{
			
			//Debug.Log("Lerp Start");
			float speed = 5;
			float time = 0 - CodeTools.Tm.GetUIDelta();
			Vector2 startPos = _rt.anchoredPosition;
			while (time < 1)
			{
				if (_rt == null)
				{
					Debug.LogError("trying to move Icon of " + _cc.GetName() + " after it has been destroyed");
					yield break;
				}
				//Debug.Log("lerping");
				time += CodeTools.Tm.GetUIDelta() * speed;
				_rt.anchoredPosition = Vector2.Lerp(startPos, endPos, time);
				yield return null;
			}
		}
		public void UpdateNumber(int number)
		{
			if (_text.text.Length > 1 && number > 0 && number < 10)
			{
				_text.alignment = TextAlignmentOptions.Center;
				_text.text = _cc.GetName().Substring(0, _text.text.Length - 2) + " " + number.ToString();
			}
			else
			{
				_text.alignment = TextAlignmentOptions.Right;
				_text.text = number.ToString();
			}
		}
		public void DisableCrosshair()
		{
			_crossHair.enabled = false;
		}
		public void EnableCrosshair(Color c)
		{
			_crossHair.enabled = true;
			_crossHair.color = c;
		}
		public Image GetCrossHair()
		{
			return _crossHair;
		}
		public void CopyHighlight(TurnIcon ti)
		{
			_highlightFrame.GetComponent<UIGradient>().LinearColor1 = ti._highlightFrame.GetComponent<UIGradient>().LinearColor1;
			_highlightFrame.GetComponent<UIGradient>().LinearColor2 = ti._highlightFrame.GetComponent<UIGradient>().LinearColor2;
		}

		public void MouseOver()
		{
			if (_cc._selectObject)
				CodeTools._combatManager.OnHighlight(_cc, true);
			//_character.Highlight(true,false);
		}
		public void MouseExit()
		{
			{
				if (_cc._selectObject)
					CodeTools._combatManager.UnHighlight();
				//_character.Highlight(false);
			}
		}
	}
}
