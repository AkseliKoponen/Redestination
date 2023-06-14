using System;
using System.Collections;
using System.Collections.Generic;
using RD.DB;
using RD;
using UnityEngine;
using UnityEngine.UI;
using static RD.CodeTools;

namespace RD.Combat
{
	public class CombatGUI : MonoBehaviour
	{
        #region Variables
        public StateOfGUI _state = StateOfGUI.Free;
		[NonSerialized] public CombatCharacter _displayedCharacter;
		public Camera _UICamera;
		[Serializable]
		public struct Components
		{
			public GameObject _basicUI;
			public CombatGUIHand _GUIHand;
			public CombatGUIDeck _GUIDraw;
			public CombatGUIDeck _GUIDiscard;
			public CombatGUIDeck _GUIDeplete;
			public RectTransform _cardPlayArea;
			public RectTransform _cardStrikeArea;
			public RectTransform _cardMoveArea;
			public TargetingSystem _targetingSystem;
			public Image _cardPlayBackground;
			public HealthBarManager _healthBarManager;
			public Transform _highlight;
			public MessageBox _messageBox;
			public Image _overlayTotal;
			public ActionDisplay _actionDisplay;
			public CanvasGroup _basicCG;
			public SpecialActionButton _specialActionButton;
			public Button _endTurnButton;
			public RectTransform _cardPlayZoom;
			public CanvasGroup _highlightBackground;
		}
		public Components _comps;
		public enum StateOfGUI { Free,Targeting,Prepare,Result, Post}
		public float durationEstimate { get; private set; } = 1f;

		CombatGUICard _playedCard;
		#endregion
		bool debug = false;
		private void Awake()
		{
			SetCombatGUI(this);
			_UICamera = Cameras._uiCamera;
			GetComponent<Canvas>().worldCamera = _UICamera;
		}
		public void AddHealthbars(List<CombatCharacter> cChars)
		{
			foreach(CombatCharacter cc in cChars)
			{
				_comps._healthBarManager.AddHealthbar(cc);
			}
		}
		public void FadeUI(bool fadeIn)
		{
			StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(_comps._highlightBackground, fadeIn, 4, true));
		}
		public float GetHealthBarY()
        {
			return 0;
        }
		public void DisplayCharacter(CombatCharacter cc, bool turnOwner = true)
		{
			bool showCards = cc._alliance == CombatCharacter.Alliance.Player || cc._deck.HasVisibleCards();
			//if (showCards)
			//{
			GUIAnimationManager.AddWait(_comps._GUIHand.CreateHand(cc._deck._mind, turnOwner),"Lifting hand");
			_comps._GUIDraw.DisplayDeck(cc._deck._subcon, CombatGUIDeck.DeckType.Draw);
			_comps._GUIDiscard.DisplayDeck(cc._deck._forget, CombatGUIDeck.DeckType.Discard);
			_comps._GUIDeplete.DisplayDeck(cc._deck._regress, CombatGUIDeck.DeckType.Deplete);
			//}
			CanvasGroup cg = _comps._GUIHand.GetComponentInParent<CanvasGroup>();
			if ((showCards && cg.alpha!=1) || (!showCards && cg.alpha==1))
				UIAnimationTools.FadeCanvasGroupAlpha(cg, showCards, 10, true);
			_displayedCharacter = cc;
			_comps._specialActionButton.AssignCharacter(_displayedCharacter);
			_comps._actionDisplay.DisplayCharacter(cc);
			//Debug.Break();
		}
		public void RefreshUI(int level = 0)
		{
			if(debug)Debug.Log("--------Refresh UI-------------");
			_comps._actionDisplay.UpdateCharacter();
			_comps._GUIHand.UpdateHandPositions();
			if (level > 0)
			{
				_comps._healthBarManager.UpdateHealthbars();
			}

		}
		public void ToggleCardPlayAreas(bool active)
		{
			if (GameSettings.UISettings._enableCardTargetingAreas)
			{
				StartCoroutine(UIAnimationTools.FadeCanvasGroupAlpha(_comps._cardPlayArea.GetComponentInParent<CanvasGroup>(), active, 5f, true));
				ToggleOnOff(_comps._cardMoveArea.GetComponentInChildren<CutoutMask>());
				ToggleOnOff(_comps._cardPlayArea.GetComponentInChildren<CutoutMask>());
				ToggleOnOff(_comps._cardStrikeArea.GetComponentInChildren<CutoutMask>());
				//Debug.Log(cg.name);
				void ToggleOnOff(Behaviour comp)
				{
					comp.enabled = false;
					comp.enabled = true;
				}
			}
		}
		public TargetingSystem ActivateTargeting(Card card)
		{
			_comps._targetingSystem.InitializeTargeting(card);
			_playedCard = card._gUICard;
			_state = StateOfGUI.Targeting;
			return _comps._targetingSystem;
		}
		public TargetingSystem ActivateHandTargeting(int count, string verb)
		{
			TargetingSystem ts = _comps._targetingSystem;
			StartCoroutine(ts.InitializeHandTargeting(count,verb));
			return ts;

		}
		public IEnumerator ActivateCardFX(Card card,CombatCharacter source, CombatCharacter target = null)
		{
			BaseFX fx = card._baseFX!=null?card._baseFX:new BaseFX();
			_postProcessingManager.FadeBackground(fx._darkenBackground);
			//Debug.Log("fx.name = " + fx._name);
			float prepareDuration = (fx==null?0.5f:fx._prepareAnimation.Time);
			float focusDuration = (fx == null ? 1.25f : fx._focusAnimation.Time);
			float postDuration = (fx == null ? 0f : fx._postAnimation.Time);
			durationEstimate = prepareDuration + focusDuration+postDuration;
			bool toggleStatusTexts = false;
			if(toggleStatusTexts)_combatManager.ToggleStatusTexts(false);
			if (durationEstimate>0)
			{

				HighlightManager.NewAttack(_comps._targetingSystem.GetTargets());
				HighlightManager.Zoom(8);
				//_comps._highlight.GetChild(0).gameObject.SetActive(true);
				_state = StateOfGUI.Prepare;
				if (prepareDuration > 0)
				{
					Task t = new Task(RunAnimation(fx._prepareAnimation, source, target));
					while (t.Running)
						yield return null;
					durationEstimate -= prepareDuration;
				}
				_state = StateOfGUI.Result;
				focusDuration = Mathf.Clamp(focusDuration, 0.5f, 999);
				if (focusDuration > 0)
				{
					//Debug.Log("Focusing on " + card._baseFX._name);
					Task t = new Task(RunAnimation(fx._focusAnimation, source, target));
					while (t.Running)
						yield return null;
				}
				durationEstimate -= focusDuration;
				HighlightManager.Zoom(8, true);
				_state = StateOfGUI.Post;
				if (postDuration > 0)
				{
					Task t = new Task(RunAnimation(fx._postAnimation, source, target));
					while (t.Running)
						yield return null;
					durationEstimate -= postDuration;
				}
			}
			durationEstimate = 0f;
			_postProcessingManager.Normalize();
			//_comps._highlight.GetChild(0).gameObject.SetActive(false);
			_state = StateOfGUI.Free;
			_comps._basicUI.SetActive(true);
			_comps._targetingSystem.UnhighlightTargets();
			if (toggleStatusTexts) _combatManager.ToggleStatusTexts(true);
			RefreshUI();

			FadeUI(false);
			IEnumerator RunAnimation(BaseFX.BaseFXAnimation anim, CombatCharacter self, CombatCharacter target)
			{
				float waitTime = anim.Time;
				if (anim._animationSelf != BaseFX.AnimationName.None)
					self._cSprite.Animate(anim._animationSelf.ToString());
				else
					self._cSprite.StopAnimation();
				if (target != null && target != self)
				{
					if (anim._animationTarget != BaseFX.AnimationName.None)
						target._cSprite.Animate(anim._animationSelf.ToString());
					else
						target._cSprite.StopAnimation();
				}
				foreach(BaseFX.ParticleEffect pe in anim._particleEffects)
				{
					if (pe._target == BaseCard.TargetType.Target && target == null)
						continue;
					GameObject particle = GameObject.Instantiate(pe._particle);
					particle.transform.SetParent(pe._target == BaseCard.TargetType.Self ? self._cSprite._particleFXPosition : target._cSprite._particleFXPosition);
					particle.transform.localPosition = Vector3.zero;
				}
				if (anim._useWeaponAudio) {
					//if using weapon sound, check if melee or ranged weapon and then if pre,focus or post
					Weapon wpn = null;
					{
						List<int> meleeStrikes = new List<int> { 2 };
						List<int> rangedStrikes = new List<int> { 5 };
						if (meleeStrikes.Contains(card._baseFX._id))
						{
							wpn = source._inventory._melee;
						}
						else if (rangedStrikes.Contains(card._baseFX._id))
						{
							wpn = source._inventory._ranged;
						}
					}
					if (wpn!=null)
					{
						BaseFXAudio bfa = null;
						switch (_state)
						{
							case StateOfGUI.Prepare:
								bfa = wpn._fxDraw;
								break;
							case StateOfGUI.Result:
								bfa = wpn._fxDraw;
								break;
							case StateOfGUI.Post:
								bfa = wpn._fxDraw;
								break;
							default:
								break;
						}
						if (bfa != null)
							SoundEffectPlayer._current.Play(bfa);
					}
				}
				else
				{
					if (anim._audio != null) SoundEffectPlayer._current.Play(anim._audio);
				}
				while (waitTime > 0)
				{
					if (!_paused) waitTime -= Tm.GetUIDelta();
					yield return null;
				}
			}
		}
		bool _paused = false;
		public void PauseTimers(bool paused)
		{
			_paused = paused;
		}
		public void ToggleFlashSettings(bool flashEnabled)
		{
			Tm.SetWorldTime(flashEnabled ? 0.75f : 1f);
			//Time.timeScale = ();
			_comps._basicUI.SetActive(!flashEnabled);
			GameManager._current._guiParts._highlightCanvas.gameObject.SetActive(!flashEnabled);
			GameManager._current._guiParts._worldCanvas.gameObject.SetActive(!flashEnabled);
			_comps._healthBarManager.gameObject.SetActive(!flashEnabled);
			_comps._overlayTotal.gameObject.SetActive(flashEnabled);

		}
		public void InspectCharacter(CombatCharacter cc)
		{
			if (_state == StateOfGUI.Free)
				GameManager._current._guiParts._inspection.DisplayCharacter(cc);
			else if (_state == StateOfGUI.Targeting)
			{
				GameManager._current._guiParts._inspection.DisplayCharacter(cc);
				_comps._targetingSystem.UnhighlightTargets();
			}

		}
		public void UpdateCardDamage(CombatCharacter target = null)
		{
			_comps._targetingSystem.UpdateCardDamageOnTarget(target);
			/*
		if (target == null)
		{
			Debug.Log("Normalizing card damage");
		}
		else
		{
			Debug.Log("Updating card damage for " + target.GetName());
		}
		*/
		}
		public void ViewDeck(Deck deck, string title = "Deck",bool enabled = true)
		{
			if (enabled)
				GameManager._current._guiParts._deckViewer.ViewDeck(deck, title);
			else
				GameManager._current._guiParts._deckViewer.Hide();
		}
		public RectTransform GetRectTransform(Deck.DeckType deck)
		{
			switch (deck)
			{
				default:
				case Deck.DeckType.Draw:
					return _comps._GUIDraw.GetComponent<RectTransform>();
				case Deck.DeckType.Discard:
					return _comps._GUIDiscard.GetComponent<RectTransform>();
				case Deck.DeckType.Hand:
					return _comps._GUIHand.GetComponent<RectTransform>();
				case Deck.DeckType.Deplete:
					return _comps._GUIDeplete.GetComponent<RectTransform>();
			}
		}
		public IEnumerator DisplayCardMovingToDeck(Card c, Vector3 createPosition, Vector3 deckPosition)
		{
			float DisplayTime = 0.35f;
			float MoveToDeckTime = 0.2f;
			CombatGUICard cgc = Instantiate(_comps._GUIHand._combatGUICardPrefab, _comps._GUIHand.transform.parent).GetComponent<CombatGUICard>();
			cgc.transform.position = createPosition;
			cgc.StartCoroutine(cgc.AddToDeck(c, deckPosition, DisplayTime,MoveToDeckTime));
			float t = DisplayTime + MoveToDeckTime;
			while (t > 0)
			{
				t -= Tm.GetUIDelta();
				yield return null;
			}
			Destroy(cgc.gameObject);
		}
		public IEnumerator ShuffleDeck(Deck startDeck, Deck endDeck, int count)
		{
			Vector3 startPos = GetRectTransform(startDeck._deckType).position;
			Vector3 endPos = GetRectTransform(endDeck._deckType).position;
			//Debug.Log("ShuffleDeckHappening");
			//Debug.Log(startPos + startDeck._deckType.ToString());
			//Debug.Log(endPos + endDeck._deckType.ToString());
			float moveTime = 0.5f;
			List<CombatGUICard> cgs = new List<CombatGUICard>();
			for(int i = 0;i<count;i++)
			{

				CombatGUICard cgc = Instantiate(_comps._GUIHand._combatGUICardPrefab, _comps._GUIHand.transform.parent).GetComponent<CombatGUICard>();
				cgc.transform.position = startPos;
				cgc.StartCoroutine(cgc.AddToDeck(new Card((BaseCard)db.Get<BaseCard>(1)), endPos, 0.001f, moveTime,true));
				cgs.Add(cgc);
			}
			moveTime += 0.2f;
			while (moveTime > 0)
			{
				//Debug.Break();
				moveTime -= Tm.GetUIDelta();
				yield return null;
			}
			/*
		if (startDeck != endDeck) {
			endDeck.AddDeck(startDeck._cards);
			endDeck.Shuffle();
			startDeck.Clear();
		}*/
			foreach(CombatGUICard cgc in cgs)
				Destroy(cgc.gameObject);
		}
		public void ViewDeck(string deckName)
		{
			Deck d;
			switch (deckName.ToLower())
			{
				default:
					return;
				case "draw":
					d = _displayedCharacter._deck._subcon;
					if(d._ideas.Count>0)
						ViewDeck(d, "Draw Deck", true);
					break;
				case "discard":
					d = _displayedCharacter._deck._forget;
					if(d._ideas.Count>0)
						ViewDeck(d, "Discard Pile", true);
					break;
			}
		}
		public void ButtonPressEndTurn()
		{
			_displayedCharacter.EndTurn();
		}
	}

	public static class GUIAnimationManager
	{
		static bool dbug = false;
		public static bool _free { get; private set; } = true;
		static List<GUIAnimation> _gUIAnimations = new List<GUIAnimation>();
		public static void Reset()
		{
			foreach(GUIAnimation anim in _gUIAnimations)
			{
				anim.Destroy();
			}
			_gUIAnimations = new List<GUIAnimation>();
			_free = true;
		}
		public class GUIAnimation
		{
			public List<Task> _tasks = new List<Task>();
			public string _name = "Unnamed";
			public GUIAnimation(Task t, string name = default)
			{
				_tasks = new List<Task> { t };
				t.Finished += FinishTask;
				if (name != default) _name = name;
			}
			public GUIAnimation(List<Task>tasks,string name = default)
			{
				_tasks = tasks;
				foreach (Task t in _tasks)
					t.Finished += FinishTask;
				if (name != default) _name = name;
			}
			public void Pause()
			{
				if (dbug) Debug.Log("<color=red>Task " + _name + " paused</color>");
				foreach (Task t in _tasks) t.Pause();
			}
			public void Unpause()
			{
				if (dbug) Debug.Log("<color=green>Task " + _name + " unpaused</color>");
				foreach (Task t in _tasks) t.Unpause();
			}
			public void FinishTask(bool manual)
			{

				//if (dbug) Debug.Log("<color=teal>Task " + _name + " taskFinish</color>");
				for (int i = _tasks.Count - 1; i >= 0; i--)
					if (!_tasks[i].Running)
						_tasks.RemoveAt(i);
				if (_tasks.Count == 0)
					Update(manual);
			}
			public bool IsFinished()
			{
				return _tasks.Count == 0;
			}
			public void Destroy()
			{
				foreach(Task t in _tasks)
				{
					t.Stop();
				}
				_tasks = new List<Task>();
			}
		}
		#region AddAnimation
		public static GUIAnimation AddAnimations(List<Task>t,string taskName = default)
		{
			if (t.Count == 0)
				return null;
			GUIAnimation ga = new GUIAnimation(t, taskName);
			AddToList(ga);
			return ga;
		}
		public static GUIAnimation AddAnimation(IEnumerator enumerator, string taskName)
		{
			return AddAnimation(new Task(enumerator), taskName);
		}
		public static GUIAnimation AddAnimation(Task t, string taskName = default)
		{
			_free = false;
			GUIAnimation ga = new GUIAnimation(t, taskName);
			AddToList(ga);
			return ga;
		}
		public static GUIAnimation AddWait(float timeInSeconds, string taskName = default)
        {
			if (taskName == default)
				taskName = "Wait " + timeInSeconds.ToString();
			return AddAnimation(WaitTask(timeInSeconds), taskName);
        }
		public static GUIAnimation AddDestroyGameObject(GameObject go, float timeInSeconds, string taskName = default)
        {
			if (go == null)
				return null;
			if (taskName == default)
				taskName = "Destroy " +go.name +" after "+timeInSeconds.ToString();
			return AddAnimation(DestroyTask(go,timeInSeconds), taskName);
		}
		static IEnumerator WaitTask(float time)
        {
            while (time > 0)
            {
				time -= Tm.GetUIDelta();
				yield return null;
            }
        }
		static IEnumerator DestroyTask(GameObject go, float timeInSeconds)
        {
            while (timeInSeconds > 0)
            {
				timeInSeconds -= Tm.GetUIDelta();
				yield return null;
            }
			if (go)
				GameObject.DestroyImmediate(go);
        }
		static void AddToList(GUIAnimation ga)
		{
			_free = false;
			if (dbug)
			{
				Debug.Log("<color=Yellow>Added new GUIAnimation (" + ga._name + ") containing "+ga._tasks.Count+" tasks.</color>");
				ListAnimations();
			}
			bool insert = true;
			if (!insert)
			{
				if (_gUIAnimations.Count == 0) ga.Unpause();
				else ga.Pause();
				_gUIAnimations.Add(ga);
			}
			else
			{

				if (_gUIAnimations.Count > 0)
					_gUIAnimations[0].Pause();
				_gUIAnimations.Insert(0, ga);
				ga.Unpause();
			}
		}
		#endregion
		public static void Update(bool manual)
		{
			if(dbug)Debug.Log("<color=orange>Animation finished (" + _gUIAnimations[0]._name + ")</color>");
			_gUIAnimations.RemoveAt(0);
			if (_gUIAnimations.Count > 0)
			{
				if (dbug) Debug.Log("<color=orange>AnimationManager started new animation("+_gUIAnimations[0]._name+").\n Animation total count = "+_gUIAnimations.Count+"</color>");
				_gUIAnimations[0].Unpause();
			}
			else
			{
				if (dbug) Debug.Log("AnimationManager Free");
				_free = true;
			}
		}
		public static void ListAnimations()
		{
			Debug.Log("<color=Yellow>Currently " + _gUIAnimations.Count + " guiAnimations</color>");
			int index = 0;
			foreach (GUIAnimation ga in _gUIAnimations)
			{
				Debug.Log("<color=Yellow>[" + index + "]" + ga._name+"</color>");
				index++;
			}
		}
	}
}