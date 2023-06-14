using System.Collections;
using System.Collections.Generic;
using RD;
using TMPro;
using UnityEngine;
using static RD.CodeTools;
//using PixelCrushers.DialogueSystem;

namespace RD.Combat
{
	public class CombatCharacterVisuals : MonoBehaviour
	{
		public SpriteRenderer _spriteRendererCharacter;
		public SpriteRenderer _spriteRendererHighlight;
		public Transform _healthBarPosition;
		public CombatSelectableObject _selectableObject { get; private set; }
		CombatCharacter _cc;
		public Animator _animator { get; private set; }
		public Facing _facing { get; protected set; }
		LerpTransform _lerpTransformSprite;
		[SerializeField]Transform _damageTextPosition;
		[SerializeField]DamageText _damageTextPrefab;
		[SerializeField]public Transform _particleFXPosition;
		bool dead = false;
		Vector3 _originalCharacterScale;
		//TEMPORARY
		[SerializeField]ParticleSystem _bloodSplash;
		public StatusBar _statusBar { get; set; }
		public Transform _nameTextPosition;
		public Sprite _defaultSprite;
		float _durationTextBase = 1f;
		/*
	Barker _barker;
	public DialogueDatabase _barks;
	DialogueActor _dialogueActor;
	DialogueSystemTrigger _dialogueSystemTrigger;
	*/
		public string _dialogueActorName;
		public void Initialize(CombatCharacter cc)
		{
			_cc = cc;
			if (!_spriteRendererCharacter)
				_spriteRendererCharacter = transform.Find("Combat Character").Find("Character Sprite").GetComponent<SpriteRenderer>();
			if (!_spriteRendererHighlight)
				_spriteRendererHighlight = transform.Find("Combat Character").transform.GetChild(0).GetComponent<SpriteRenderer>();
			if (GetComponent<CombatSelectableObject>())
			{
				//Debug.Log("SelectableObject IS Set");
				_selectableObject = GetComponent<CombatSelectableObject>();
				_selectableObject.Start();
			}
			_animator = _spriteRendererCharacter.GetComponent<Animator>();
			_lerpTransformSprite = _spriteRendererCharacter.GetComponent<LerpTransform>();
			if (!_particleFXPosition)
				_particleFXPosition = transform.Find("ImpactFX Position");
			/*
		_barker = new Barker();
		_dialogueActor = GetComponent<DialogueActor>();
		_dialogueSystemTrigger = GetComponent<DialogueSystemTrigger>();
		*/
			_originalCharacterScale = _spriteRendererCharacter.transform.localScale;
			_defaultSprite = _spriteRendererCharacter.sprite;
			_facing = cc._facing;

		}
		public void UpdateStats(bool instant = false)
		{
			if (_statusBar)
			{
				_statusBar.UpdateStats(instant);
			}

		}
		public void Flip()
		{
			if (_facing == Facing.Left)
				_facing = Facing.Right;
			else
				_facing = Facing.Left;
			_spriteRendererCharacter.flipX = !_spriteRendererCharacter.flipX;
			foreach(Transform t in transform.GetComponentsInChildren<Transform>())
			{
				t.localPosition = new Vector3(t.localPosition.x * -1, t.localPosition.y, t.localPosition.z);
			}
		}

		public void Animate(string animationName,float speed = 1) {
			switch (animationName)
			{
				case "Death":
					dead = true;
					AnimationCurve ac = new AnimationCurve();
					ac = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.9f, 0.7f), new Keyframe(1, 1));
					ac.preWrapMode = WrapMode.Clamp;
					ac.postWrapMode = WrapMode.Clamp;
					_spriteRendererCharacter.GetComponent<LerpColor>().StartLerp(new LerpColor.ColorBool(new Color(0, 0, 0, 0), true), new LerpColor.ColorBool(new Color()), LerpColor.ColorType.RGB, speed);
					_spriteRendererCharacter.gameObject.AddComponent<LerpColor>().StartLerp(new LerpColor.ColorBool(new Color(0, 0, 0, 0), true), new LerpColor.ColorBool(new Color()), LerpColor.ColorType.A, speed, LerpColor.CompType.SpriteRenderer, ac);
					break;
				default:
					//Debug.LogError("Unknown animationName " + animationName);
					break;
			}
			if (_animator)
			{
				_animator.Play(animationName);
				//_animator.Play(animationName, 0, speed);
			}
			if (animationName == "Idle" || animationName == "IdleCombat")
				StopShaking();
		}
		public void StopAnimation()
		{
			//Debug.Log("Todo: stop animation");
		}
		public void BloodSplatter(int direction = 0)
		{
			return;
			ParticleSystem ps = Instantiate(_bloodSplash.gameObject, _particleFXPosition).GetComponent<ParticleSystem>();
			ps.transform.eulerAngles = (new Vector3(0, 0, -70 * direction));
		}

		public void StartLerpScale(float lerpScale, float speed = 1, bool enableSelection = false)
		{
			if (dead)
				return;
			Vector3 baseScale = _originalCharacterScale;
			if (_selectableObject._highlighted)
				baseScale *= _selectableObject._highlightLerpScale;
			_lerpTransformSprite.StartLerpScale(lerpScale*(lerpScale==1?baseScale:_originalCharacterScale), speed);
			_selectableObject.lerping = true;
			if (enableSelection)
				Invoke("EndLerping", 1 / speed);
		}
		public void NormalizeScale()
		{

			_selectableObject.NormalizeScale();
		}
		void EndLerping()
		{
			_selectableObject.lerping = false;
		}
		float _damageTextCooldown = 0f;
		public void DisplayDamage(float damage, float duration, int direction, bool animate = true)
		{
			if (damage > 0)
			{
				Shake(1, 0.5f);
				if (animate) Animate("Take Hit", duration);
			}
			StartCoroutine(CreateText(damage.ToString(),"dmg",direction));

		}
		/// <summary>
		/// <para>Display text like damage or aura on character. textTypes are as follows</para>
		/// <br>. . "dmg" -> normal bouncy hit</br>
		/// <br>. . "aura"-> aura message</br>
		/// <br>. . "heal"-> floaty heal</br>
		/// <br>. . "barrier"-> floaty heal</br>
		/// <br>. . "dot" -> small floaty damage for dots</br>
		/// <br>. . "static" -> a large static text</br>
		/// </summary>
		public void ShowText(string text, string textType="dmg", int direction = 0)
		{
			StartCoroutine(CreateText(text, textType, direction));
		}
		IEnumerator CreateText(string text, string textType, int direction = 0)
		{
			while (_damageTextCooldown > 0)
			{
				_damageTextCooldown -= Tm.GetWorldDelta();
				yield return null;
			}
			_damageTextCooldown = 0.2f;
			float duration = 2;
			if (duration <= 0)
			{
				duration = _combatGUI.durationEstimate;
				if (duration <= 0)
					yield break;
			}
			duration += 0.35f;
			Transform parent = _combatManager.AddDamageTextParent(_damageTextPosition.gameObject);
			DamageText dt = GameObject.Instantiate(_damageTextPrefab, parent);
			if (parent.childCount > 1)
				RePositionDamageText(dt.transform, direction < 0);
			dt.Init(this,text, parent.transform.position.y,textType, duration, direction);
		}
		void RePositionDamageText(Transform child, bool growLeft)
		{
			float diff = child.GetSiblingIndex() * 1f;
			Vector3 pos = child.transform.localPosition;
			pos.x += growLeft?-diff:diff;
			pos.y -= diff / 2;
			child.localPosition = pos;
		}

		public Vector2 GetPositionOnScreen(bool percentage = true)
		{
			Vector2 pos = Camera.main.WorldToScreenPoint(transform.position);
			Vector2 screensize = new Vector2(Screen.width, Screen.height);
			pos -= screensize / 2;
			pos /= screensize / 2;
			return pos;
		}

		public void Shake(int count = 1, float time = 0.3f)
		{
			_spriteRendererCharacter.gameObject.AddComponent<Shaker>().Shake(new Vector3(0.2f, 0, 0), time, count);

		}
		public void StopShaking()
		{
			if(_spriteRendererCharacter.gameObject.GetComponent<Shaker>())
				_spriteRendererCharacter.gameObject.GetComponent<Shaker>().Kill();
		}

		public IEnumerator FlashColor(Color color,float duration)
		{
			Material material = _spriteRendererCharacter.material;
			Color originalColor = material.GetColor("_Color");
			Shader txtShader = Shader.Find("GUI/Text Shader");
			Shader defaultShader = material.shader;
			material.shader = txtShader;
			material.SetColor("_Color", color);
			yield return new WaitForSeconds(duration);
			material.shader = defaultShader;
			material.SetColor("_Color", originalColor);
		}
		public void MoveToPosition(Vector3 position, float time)
		{
			if (time <= 0)
			{
				_cc.transform.position = position;
			}
			else
			{
				//LerpTransform papaLerp = _cc.GetComponent<LerpTransform>();//? _cc.GetComponent<LerpTransform>():_cc.gameObject.AddComponent<LerpTransform>();
				//Debug.Log(papaLerp.gameObject.name);
				_cc.GetComponent<LerpTransform>().StartLerpPosition(position,false,1/time);
				//_lerpTransformSprite.StartLerpPosition(transform.position, false, 1 / time);
			}
		}
		/*
	public void BarkRandom(string category)
	{
		if (false)
		{
			float rand = Random.Range(0, 100);
			if (rand < 70)
				return;
		}
		string title = _dialogueActorName + "/" + category;
		Conversation con = _barks.GetConversation(title);
		//Debug.Log("title = " + con.Title);
		_dialogueSystemTrigger.barkText = GetRandomStringFromConversation(con);
		_dialogueSystemTrigger.Fire(transform);
		
	}

	class Barker
	{
		
	}*/
	}
}
