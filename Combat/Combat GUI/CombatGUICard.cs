using System;
using System.Collections;
using System.Collections.Generic;
using RD.DB;
using RD;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static RD.CodeTools;

namespace RD.Combat
{
	[RequireComponent(typeof(LerpTransform))]
	public class CombatGUICard : MonoBehaviour
	{
		[NonSerialized] public Card _card;
		[SerializeField] TrailRenderer _trailRendererPrefab;
		Transform _trailRenderer;

		[Serializable]public struct Parts {
			public TextMeshProUGUI _textName;
			public TextMeshProUGUI _textDescription;
			public Image _descriptionPanel;
			public CardRangeDisplay _rangeDisplay;
			public GameObject _extension;
			public GameObject _descriptionBackground;
			public Image _cardImage;
			public Image _inkSplash;
			public GameObject _cardBack;
			public GameObject _playableGlow;
			public Image _outline;
			public Image _background;
			public Image _nameField;
		}
		public Parts _parts;
		[NonSerialized]public CombatGUIHand _hand;
		int _originalSiblingIndex = 0;
		float originalY;
		Vector3 originalPosition;
		[Tooltip("Min = Unextended,\nCurrent = extension before Description,\nMax = Maximum extension")]
		[SerializeField] Bint _heightLimit;
		[SerializeField] int _heightExtra = 0;
		[System.Flags]
		public enum CardState
		{
			Highlighted = 1,
			Attached = 2,
			Targeting = 4,
			BeingDrawn = 8,
			BeingPlayed = 16,
			BasicActionCard = 32,
			InTransition = 64,
			DeckView = 128
		} // _cardState |= CardState.Highlighted -> Adds Highlighted................ _cardState &= ~CardState.Highlighted -> Removes Highlighted
		public CardState _cardState { get; private set; }
		//_cardState.HasFlag(CardState.Highlighted)
		bool _playable = false;
		bool _blockInteractions = false;
		LerpTransform _lerp;
		LerpTransform _childLerp;
		float highlightSpeed = 7f;
		[NonSerialized]public bool _inTransition = false;
		[SerializeField] AnimationCurve _animationCurveDiscardX;
		[SerializeField] AnimationCurve _animationCurveDiscardY;
		[SerializeField] AnimationCurve _animationCurveDiscardZ;
		CombatCharacter _cc;
		bool _mouseOver = false;
		RectTransform _childRect;
		RectTransform _rt;
		private void Start()
		{
			_rt = GetComponent<RectTransform>();
			_lerp = GetComponent<LerpTransform>();
			_childLerp = transform.GetChild(0).GetComponent<LerpTransform>();
			if (_cardState.HasFlag(CardState.BasicActionCard))
				return;
			originalY = _childLerp.transform.localPosition.y;
			_hand = transform.parent.GetComponent<CombatGUIHand>();
			_originalSiblingIndex = transform.GetSiblingIndex();
		}

		private void Update()
		{

			if (_trailRenderer)
				_trailRenderer.position = transform.position;
		}

		public void DisplayCardInHand(Card c, bool playability = true)
		{
			_card = c;
			BasicInit();
			_hand = transform.parent.GetComponent<CombatGUIHand>();
			_cc = _hand._deck._cc;
			gameObject.name = "GUICard - " + _card._name+" - "+transform.GetSiblingIndex();
			_parts._extension.SetActive(false);
			UpdateTexts();
			UpdatePlayable();
			SetCardClass();
			Flip(c._visible);
		}
		public void InitCardForTargeting(BaseCard c, CombatCharacter source)
		{
			_cc = source;
			_card = new Card(c);
			AssignPreviewSettings();
		}
		public void InitCardForHistory(Card c, CombatCharacter source)
		{
			_cc = source;
			_card = c;
			AssignPreviewSettings();
			ToggleGlow(false);
		}
		void BasicInit()
		{
			Start();
			_card._gUICard = this;
			_parts._textName.text = _card._name;

			_parts._cardImage.sprite = _card._artSprite;
			if (_card._inkSplash > 0)
			{
				_parts._inkSplash.sprite = Resources.Load<Sprite>("Card Art/Ink Splash " + Mathf.Clamp(_card._inkSplash, 1, BaseCard.GetInkSplashCount()).ToString());

			}
			else
			{
				_parts._cardImage.GetComponent<Coffee.UIExtensions.Unmask>().enabled = false;
				_parts._inkSplash.gameObject.SetActive(false);
			}
			_childRect = _childLerp.GetComponent<RectTransform>();
			_parts._rangeDisplay.SetRange(_card);
		}
		void AssignPreviewSettings()
		{
			BasicInit();
			UpdateTexts(_card._descriptionFinal);
			ToggleExtension(true);
			_card._visible = true;
			UpdatePlayable();
			_cardState |= CardState.BasicActionCard;
			_cardState |= CardState.Targeting;
			Flip(true);
			SetCardClass();
		}
		DeckViewer _deckViewer;
		public void DisplayCardForDeckView(Card c,CombatCharacter cc = null, bool selectable = false)
		{
			_card = c;
			BasicInit();
			_cc = cc;
			gameObject.name = "GUICard - " + _card.name;
			_parts._extension.SetActive(true);
			UpdateTexts();
			_blockInteractions = !selectable;
			_cardState |= CardState.DeckView;
			_deckViewer = GameManager._current._guiParts._deckViewer;
			SetCardClass();
			Flip(c._visible);
		}
		public void Flip(bool visible)
		{
			_parts._cardBack.SetActive(!visible);
			if (!visible)
			{
				_parts._extension.SetActive(false);
				Vector2 backSize = Vector2.one * 214;
				_childRect.sizeDelta = backSize;
				//Debug.Log("Not visible and sizeDelta = " + _childRect.sizeDelta);
			}
		}
		public IEnumerator AddToDeck(Card c, Vector3 deckPosition, float flashDuration, float moveDuration, bool moveBetweenDecks = false)
		{
			DisplayCardForDeckView(c);
			if (moveBetweenDecks)
			{
				Flip(true);
				_rt.localScale = Vector3.one * 0.25f;
			}
			_blockInteractions = true;
			_inTransition = true;
			while (flashDuration > 0)
			{
				flashDuration -= Tm.GetUIDelta();
				yield return null;
			}
			if (moveBetweenDecks)
			{
				//float noiseScale = 0.25f;
				//AnimationTools.NoiseCurve(_lerp._animationCurvePosX, noiseScale);
				//AnimationTools.NoiseCurve(_lerp._animationCurvePosY, noiseScale);
				//AnimationTools.NoiseCurve(_lerp._animationCurvePosZ, noiseScale);
				_lerp.StartLerpPositionCurved(deckPosition,new Vector3(0,UnityEngine.Random.Range(-2f,2f),0),_animationCurveDiscardZ, false, 1f / moveDuration);
			}
			else
			{
				_lerp.StartLerpPosition(deckPosition, false, 1f / moveDuration);
				_lerp.StartLerpScale(Vector3.zero, 1f / moveDuration);
			}
			StartCoroutine(AddTrail(moveBetweenDecks?0.3f:1f));
		}
		void SetCardClass(bool descPanelColorChange = false)
		{
			Color panelColor = new Color(0.2f, 0.2f, 0.2f);
			switch (_card._cardClass)
			{
				default:
					break;
				case BaseCard.CardClass.Inventor:
					panelColor = Color.HSVToRGB(214/255, 0.9f, 0.30f);
					break;
				case BaseCard.CardClass.Soldier:
					panelColor = Color.HSVToRGB(0, 0.9f, 0.30f);
					break;
				case BaseCard.CardClass.Spy:
					panelColor = Color.HSVToRGB(128/255, 0.9f, 0.30f);
					break;
			}
			_parts._textName.transform.parent.GetComponent<Image>().color = panelColor;
			if (descPanelColorChange)
			{
				float v;
				float h;
				float s;
				Color.RGBToHSV(panelColor, out h, out s, out v);
				_parts._descriptionPanel.color = Color.HSVToRGB(h, s, 1);
			}
		}
		public void UpdateTexts(string txt)
		{
			_parts._textName.text = _card._name;
			_parts._textDescription.text = txt;
			RefreshDescription();
		}
		public void UpdateTexts(List<int> damages)
		{
			string text = Translator.PrepCardText(_card.GetDescription(), damages);
			_parts._textDescription.text = text;
			_card._descriptionFinal = text;
			RefreshDescription();
		}
		public static bool _displayUnmitigatedDamageForEnemies = true;
		public void UpdateTexts(CombatCharacter target = null)
		{
			_parts._textName.text = _card._name;
			string text = Translator.PrepCardText(_card.GetDescription(), this, target);
			_parts._textDescription.text = text;
			_card._descriptionFinal = text;
			RefreshDescription();
		}
		void RefreshDescription()
		{
			_parts._textName.text = _card._name;
			//_parts._rangeDisplay.SetRange(_card._range.min, _card._range.max, false);
			_parts._textDescription.ForceMeshUpdate();
			if (_parts._extension.activeSelf)
			{
				_childRect.sizeDelta = new Vector2(_childRect.sizeDelta.x, Mathf.Clamp(_heightLimit.current + _parts._textDescription.preferredHeight + _heightExtra,_heightLimit.min,_heightLimit.max));
			}
		}
		public float ToggleExtension(bool active)
		{
			if (_parts._extension.activeSelf == active)
				return _parts._textDescription.preferredHeight + _heightExtra;
			_parts._extension.SetActive(active);
			if (active)
			{
				UpdateTexts();
			}
			else
			{
				_childRect.sizeDelta = new Vector2(_childRect.sizeDelta.x, _heightLimit.min);
			}
			return _parts._textDescription.preferredHeight + _heightExtra;
		}
		public bool CheckPlayability()
		{
			UpdatePlayable();
			return _playable;
		}
		public void Highlight()
		{
			if(!_cardState.HasFlag(CardState.BeingDrawn))_mouseOver = true;

			if (Highlightable())
			{
				_cardState |= CardState.Highlighted;
				_hand.HighlightCard(this);
				float txtheight = ToggleExtension(true);
				float angles = Mathf.DeltaAngle(transform.localEulerAngles.z,_childLerp.transform.localEulerAngles.z);
				_childLerp.StartLerpRotate(angles,highlightSpeed);
				float endy = _hand.GetInspectHeightWorld(txtheight);
				_childLerp.StartLerpPosition(new Vector3(_childLerp.transform.position.x, endy),false, highlightSpeed);
				PreviewHaste();
				transform.SetAsLastSibling();
				Invoke("ActivateTooltips", 1 / highlightSpeed);
			}
			else if(_cardState.HasFlag(CardState.Targeting) && _cardState.HasFlag(CardState.BasicActionCard) && !_cardState.HasFlag(CardState.Highlighted))
			{
				_cardState |= CardState.Highlighted;
				ActivateTooltips();
			}
			else if (_cardState.HasFlag(CardState.Targeting) || _cardState.HasFlag(CardState.DeckView))
			{
				ActivateTooltips();
				if (_cardState.HasFlag(CardState.DeckView))
				{
					transform.localScale = Vector3.one * 1.05f;
					transform.SetAsLastSibling();
				}
			}
			bool Highlightable()
			{
				return !_cardState.HasFlag(CardState.Highlighted) &&
				       !_cardState.HasFlag(CardState.Targeting) &&
				       !_cardState.HasFlag(CardState.DeckView) &&
				       !_cardState.HasFlag(CardState.BeingPlayed) &&
					   !_cardState.HasFlag(CardState.BeingDrawn) &&
					   !_inTransition &&
				       !_blockInteractions &&
				       (!_hand || !_hand._blockInteraction);
			}
			void PreviewHaste()
			{
				if (_card._autoPlay)
				{
					TurnDisplay._current.PreviewHasteCharacter(_cc, _card.GetHasteEffect(_cc));
				}
			}
		}
		void HideTooltips()
		{
			if (_card._links.Count > 0)
			{
				TooltipSystem.HideAllTooltips();
			}
		}
		public void Lowlight()
		{
			if (_cardState.HasFlag(CardState.Attached))
				return;
			if (!_cardState.HasFlag(CardState.BeingDrawn)) _mouseOver = false;
			HideTooltips();
			if (Lowlightable())
			{
				//Debug.Log("Legit Lowlight");
				_cardState &= ~CardState.Highlighted;
				_hand.LowlightCard();
				_childLerp.StartLerpPosition(new Vector3(0, originalY,0), true, highlightSpeed);
				float angles = Mathf.DeltaAngle(_childLerp.transform.localEulerAngles.z,0);
				_childLerp.StartLerpRotate(angles, highlightSpeed);
				transform.SetParent(_hand.transform);
				transform.SetSiblingIndex(_originalSiblingIndex);
				_hand.UpdateHandSiblingIndex();
				TurnDisplay._current.RevertHastePreview();
				//_inTransition = true;
				ToggleExtension(false);
				//Invoke("UpdateSiblingIndex", 1/highlightSpeed);
			}
			else if (_cardState.HasFlag(CardState.DeckView))
			{
				transform.localScale = Vector3.one;
				transform.SetSiblingIndex(_originalSiblingIndex);
			}
			bool Lowlightable()
			{
				return _cardState.HasFlag(CardState.Highlighted) && !_cardState.HasFlag(CardState.Targeting) && !_cardState.HasFlag(CardState.BeingDrawn) && !_inTransition && !_hand._blockInteraction && !_cardState.HasFlag(CardState.BeingPlayed);
			}
			if (_cardState.HasFlag(CardState.Targeting) && _cardState.HasFlag(CardState.BasicActionCard) && _cardState.HasFlag(CardState.Highlighted))
			{
				_cardState &= ~CardState.Highlighted;
				//Debug.Log("Remove GLOW EFFECT OR OTHER HIGHLIGHT");
			}
		}
		public void ForceLowlight()
		{
			if (_cardState.HasFlag(CardState.Highlighted) || _inTransition && !_cardState.HasFlag(CardState.BasicActionCard) && !_cardState.HasFlag(CardState.BeingPlayed))
			{
				if (!_childLerp)
					Start();
				_childLerp.transform.localPosition = Vector3.zero;
				_childRect.sizeDelta = new Vector2(_childRect.sizeDelta.x, 185);
				float angles = Mathf.DeltaAngle(_childLerp.transform.localEulerAngles.z,0);
				_childLerp.transform.Rotate(new Vector3(0, 0, angles));
				_childLerp.StopAll();
				_lerp.StopAll();
				ToggleExtension(false);
				_cardState &= ~CardState.Highlighted;
				_inTransition = false;
			}
		}

        #region Unused
        /*
		 
		void ResolveState()
		{
			_inTransition = false;
			if (_cardState.HasFlag(CardState.Attached))
				return;
		
			if (_mouseOver && !_cardState.HasFlag(CardState.Highlighted))
				Highlight();
			else if(!_mouseOver && _cardState.HasFlag(CardState.Highlighted))
				Lowlight();
		}

		void UpdateSiblingIndex()
		{
			ResolveState();
			if (!_cardState.HasFlag(CardState.Highlighted))
			{
				transform.SetSiblingIndex(_originalSiblingIndex);
				_hand.UpdateHandSiblingIndex();
			}
			else
				transform.SetAsLastSibling();
		}*/
        #endregion
        public void Select()
		{
			if (_blockInteractions)
				return;
			if (!_cardState.HasFlag(CardState.DeckView))
			{
				if (_hand == null || !_hand._cardTargetingMode)
				{
					if (!_cardState.HasFlag(CardState.Targeting))
					{
						if (!_hand.playable || _cardState.HasFlag(CardState.Targeting) || _blockInteractions)
							return;
						if (_playable && _cardState.HasFlag(CardState.Highlighted)) //if you are eligible to play the card and it is Highlighted, allow it to be dragged
						{
							_hand._blockInteraction = true;
							_cardState |= CardState.Attached;
							HideTooltips();
							originalPosition = transform.position;
							_combatGUI.ToggleCardPlayAreas(true);
						}
					}
					else
					{
						_combatGUI._comps._targetingSystem.SetCardSelection(this);
					}
				}
				else if (!_cardState.HasFlag(CardState.BeingPlayed))
				{
					bool selected = _combatGUI._comps._targetingSystem.SelectCard(this);
					SetGlowColor(!selected);
				}
			}
			else
			{
				bool selected = _deckViewer.SelectCard(this);
				SetGlowColor(!selected);
			}
		}
		public void DetachFromMouse()
		{
			if (!_cardState.HasFlag(CardState.BasicActionCard))
			{
				if (!_cardState.HasFlag(CardState.Attached) || !_hand.playable || _cardState.HasFlag(CardState.Targeting))
					return;
				//If card is in viable area, attempt to play the card, otherwise return the card to hand
				_combatGUI.ToggleCardPlayAreas(false);
				CombatGUIHand.PlayPosition playPosition = _hand.CheckBounds(Mouse.current.position.ReadValue());
				if (playPosition>0)//Check the card area
				{
					_hand._blockInteraction = false;
					StartCoroutine(Target());
				}
				else
				{
					_hand._blockInteraction = false;
					//transform.position = originalPosition;
					_cardState &= ~CardState.Attached;
					Lowlight();
					_lerp.StartLerpPosition(originalPosition, false, 3f);
					StartCoroutine(BlockInteractions(1f/3f));
				}
			}
		}
		public Task Remove(bool deplete = false)
		{
			Task t;
			if (!deplete)
			{
				StartCoroutine(AddTrail());
				_lerp._animationCurvePosX = _animationCurveDiscardX;
				_lerp._animationCurvePosY = _animationCurveDiscardY;
				//Debug.Log("<color=purple>Discarding " + gameObject.name+"</color>");
				_lerp._animationCurvePosXisAllAxis = false;
				float spd = 2;
				_lerp.StartLerpScale(Vector3.zero, spd);
				_lerp.StartLerpPosition(_combatGUI._comps._GUIDiscard.transform.position, false, spd);
				_lerp.StartLerpRotate(-90, spd);
				_hand.DiscardCard(this);
				t = new Task(WaitForLerpToEnd());
			}
			else
			{
				ToggleGlow(false);
				//Flip(false);
				//ToggleExtension(false);
				t = new Task(_hand.BurnCard(this));
			}
			return t;
			IEnumerator WaitForLerpToEnd()
			{
				while (!_lerp.LerpDone())
					yield return null;
				_hand.RemoveCard(this);
			}
		}
		public BurnSettings _burnSettings;
		[Serializable]public class BurnSettings {
			public bool enabled = true;
			public ParticleSystem _ps;
			public Material burnMaterial;
			public float burnDelay;
			public float burnTime;
		}

		public IEnumerator BurnCard()
		{
			//Debug.Log("<color=red>Burn Started</color>");
			float time = _burnSettings.burnTime;

			float t = time;
			float ot = time;
			StartCoroutine(UIAnimationTools.FadeText(_parts._textName, false, 2f / t));
			StartCoroutine(UIAnimationTools.FadeText(_parts._textDescription, false, 2f / t));
			_parts._rangeDisplay.FadeOut(2/t);
			Material splash = _parts._inkSplash.material = Instantiate<Material>(_burnSettings.burnMaterial);
			splash.SetFloat("BurnSpeed_", 3 / _burnSettings.burnTime);
			splash.SetFloat("BurnDelay_", UnityEngine.Time.time);
			StartCoroutine(HideImage(_burnSettings.burnTime/3, _parts._inkSplash));
			Material im = _parts._cardImage.material = Instantiate<Material>(_burnSettings.burnMaterial);
			im.SetFloat("BurnSpeed_", 1 / _burnSettings.burnTime);
			im.SetFloat("BurnDelay_", UnityEngine.Time.time);
			_parts._cardImage.material = im;
			_parts._nameField.material = im;
			_parts._background.material = im;
			_parts._outline.material = im;
			_parts._descriptionBackground.GetComponent<Image>().material = im;
			_parts._cardBack.GetComponent<Image>().material = im;
			ActivatePS();
			while (t > 0)
			{
				t -= UnityEngine.Time.deltaTime;
				yield return null;
			}
			ot -= t;
			while (ot > 0)
			{
				ot -= UnityEngine.Time.deltaTime;
				yield return null;
			}

			//Debug.Log("<color=red>Burn Complete</color>");
			//	Debug.Break();
			
			IEnumerator HideImage(float time, Image img)
			{
				while (time > 0)
				{
					time -= Tm.GetUIDelta();
					yield return null;
				}
				img.gameObject.SetActive(false);
			}
			void ActivatePS()
			{

				var main = _burnSettings._ps.main;
				ParticleSystem.MinMaxCurve curve = main.startLifetime;
				curve.constantMin = t;
				curve.constantMax = ot * 1.25f;
				main.startLifetime = curve;
				_burnSettings._ps.gameObject.SetActive(true);
				var shape = _burnSettings._ps.shape;
				Vector3 shapeScale = shape.scale;
				shapeScale.y = _burnSettings._ps.GetComponent<RectTransform>().rect.height / 100f;
				//Debug.Log("shapeScale.y = " + shapeScale.y);
				shape.scale = shapeScale;
				shape.position = new Vector3(0, shapeScale.y * -0.5f, 0);
			}
		}
		public IEnumerator AddTrail(float trailScale = 1f)
		{
			int frameBuffer = 1;
			while (frameBuffer > 0)
			{
				frameBuffer--;
				yield return null;
			}
			_trailRenderer = Instantiate(_trailRendererPrefab.gameObject).transform;
			_trailRenderer.transform.position = transform.position;
			_trailRenderer.GetComponent<TrailRenderer>().widthMultiplier = trailScale;
		}
		public IEnumerator Target()
		{
			_lerp.StopAll();
			_childLerp.StopAll();
			_cardState &= ~CardState.Attached;
			CombatCharacter cc = _hand._deck._cc;
			List<CombatCharacter> targets = new List<CombatCharacter>();
			_cardState |= CardState.Targeting;
			_hand.SetHandGlow(false, this);
			#region Repositioning Card and the children
			Vector3 targetPos = _combatGUI._comps._cardPlayZoom.localPosition;
			float childY = _childRect.anchoredPosition.y;
			_childRect.anchoredPosition = Vector2.zero;
			transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + childY);
			//UpdateTooltipManager();
			_rt.SetPivot(new Vector2(0.5f, 1));
			transform.SetParent(_combatGUI._comps._highlight);
			transform.SetAsLastSibling();
			_lerp.StartLerpPosition(targetPos, true, 3);
			#endregion
			TargetingSystem ts = _combatGUI.ActivateTargeting(_card);
			_postProcessingManager.Target(true);
			while (ts.ready == 0)
				yield return null;
			_postProcessingManager.Target(false);
			_rt.SetPivot(new Vector2(0.5f, 0.5f));
			_cardState &= ~CardState.Targeting;
			if (ts.ready == -1)
			{
				SetGlowColor(true);
				_hand.SetHandGlow(true);
				float speed = 5f;
				GetComponent<LerpTransform>().StartLerpPosition(originalPosition, false, speed);
				StartCoroutine(BlockInteractions(1f / speed));
				transform.localScale = Vector3.one;
				Lowlight();
			}
			else if (ts.ready == 1)
			{
				targets.AddRange(ts.GetTargets());
				ts.gameObject.SetActive(false);
				ShowPlayedCard(true);
				Play(targets);
			}

		}
		public void ReturnToHand()
		{
			transform.localScale = Vector3.one;
			ToggleExtension(false);
			_childLerp.transform.localEulerAngles = Vector3.zero;
			_cardState &= ~CardState.Highlighted;
			_cardState &= ~CardState.BeingPlayed;
			_cardState &= ~CardState.Targeting;
			_cardState &= ~CardState.Attached;
			_hand.ReturnCardToHand(this);
		}
		public void Play(List<CombatCharacter> targets)
		{
			CombatCharacter cc = _hand._deck._cc;
			//Card c = _combatGUI._comps._targetingSystem._card;
			//CombatGUICard cgc = c._gUICard;
			if (_hand._cards.Contains(this))
				_hand._cards.Remove(this);
			cc.PlayCard(_card, targets);
			//_combatGUI._state = CombatGUI.StateOfGUI.Rolling;
			//transform.SetParent(_hand.transform);
			//transform.SetSiblingIndex(_originalSiblingIndex);
		}
		public float DisplayAICard()
		{
			_rt.Rotate(new Vector3(0,0,_rt.localEulerAngles.z * -1));
			_childRect.Rotate(new Vector3(0, 0, _childRect.localEulerAngles.z * -1));
			if (_hand._cards.Contains(this)) _hand._cards.Remove(this);
			return ShowPlayedCard(true);
		}
		public float ShowPlayedCard(bool enable)
		{
			_combatGUI.FadeUI(true);
			if (enable)
			{
				_card._visible = true;
				Flip(true);
				ToggleExtension(true);
				transform.SetParent(_combatGUI._comps._cardPlayZoom);
				//_lerp.transform.localScale = Vector3.zero;
				//_lerp.StartLerpScale(Vector3.one * 1.1f, 4);
				_rt.SetPivot(new Vector2(0.5f, 1));
				_rt.anchoredPosition = new Vector2(0, 0);
				_rt.SetPivot(new Vector2(0.5f, 0.5f));
				//transform.localPosition = Vector3.zero;
				transform.SetAsLastSibling();
				//if(_card._baseFX)
			}
			_cardState |= CardState.BeingPlayed;
			return 2f;
		}
		public IEnumerator Draw(float time)
		{
			_cardState |= CardState.BeingDrawn;
			_inTransition = true;
			while (time > 0)
			{
				time -= Tm.GetUIDelta();
				yield return null;
			}
			_cardState &= ~CardState.BeingDrawn;
			_inTransition = false;
			originalY = _childLerp.transform.localPosition.y;
		}
		IEnumerator BlockInteractions(float blockTime)
		{
			_blockInteractions = true;
			blockTime += Tm.GetUIDelta();
			while (blockTime > 0)
			{
				blockTime -= Tm.GetUIDelta();
				yield return null;
			}
			_blockInteractions = false;
		}
		public CombatCharacter GetOwner()
		{
			if (_hand)
				return _hand._deck._cc;
			else
				return _cc;
		}
		void ActivateTooltips()
		{
			if (Tooltippable() && (CardIsInHandAndValid() || _cardState.HasFlag(CardState.Targeting) || _cardState.HasFlag(CardState.DeckView)))
			{
				if (Tooltippable())
				{
					Vector2 pos = GetRecttransformPivotPoint(_childRect, new Vector2(1, 1),true);
					TooltipSystem.DisplayTooltips(_card.GetLinks(),pos);
				}
			}
			bool Tooltippable()
			{
				return _card.GetLinks().Count > 0;
			}
			bool CardIsInHandAndValid()
			{
			
				return !_blockInteractions &&_cardState.HasFlag(CardState.Highlighted) && _mouseOver && _hand != null && !_hand._blockInteraction;
			}
		}
		public void MouseOver()
		{
			_mouseOver = true;
		}
		public void UpdatePlayable() {
			bool playable = _card.CheckPlayable();
			_playable = playable && _card._visible;
			ToggleGlow(_playable);
		}
		public void ToggleGlow(bool actv)
		{
			_parts._playableGlow.SetActive(actv);
		}
		void SetGlowColor(bool playableColor, Color customColor = default(Color))
		{
			Color BasicColor = new Color(0, 0.95f, 1);
			Color ActivatedColor = new Color(1, 0.5f, 0);
			Color SpecialColor = new Color(0, 1, 0); //For example repeating etc
			Color BasicActionCardColor = new Color(0.5f, 0.5f, 0.5f);
			Color color = GetColor();
			_parts._playableGlow.GetComponent<Image>().color = color;
			//Debug.Log(gameObject.name);
			ParticleSystem.MainModule main = _parts._playableGlow.GetComponentInChildren<ParticleSystem>().main;
			main.startColor = color;

			Color GetColor()
			{
				if (customColor != default(Color))
					return customColor;
				if (_cardState.HasFlag(CardState.BasicActionCard))
					return BasicActionCardColor;
				if (playableColor)
				{
					if (_card.GetSpecialState())
						return SpecialColor;
					else
						return BasicColor;
				}
				else
					 return ActivatedColor;

			}
		}
		public void ToggleTargetSelection (bool active)
		{
			SetGlowColor(!active);
			transform.localScale = Vector3.one * (active ? 1 : 0.75f);
		}
		public void FinishLerps()
		{
			_lerp.EndLerp();
			_childLerp.EndLerp();
		}
		public float GetTotalHeight()
		{
			return _childRect.sizeDelta.y;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			//Debug.Log("Begin Drag");
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			//Debug.Log("End Drag");
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (_cardState.HasFlag(CardState.Attached) && !_cardState.HasFlag(CardState.Targeting))
			{
				_rt.anchoredPosition += eventData.delta;
			}
		}
	}
}
