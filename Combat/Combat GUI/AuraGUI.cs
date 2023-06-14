using RD.DB;
using RD;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace RD.Combat
{
	[RequireComponent(typeof(Image))]
	public class AuraGUI : MonoBehaviour
	{
		//max 44 visible auras per Character
		public TextMeshProUGUI _text;
		public Image _image;
		public Gradient _positiveColor;
		public Gradient _negativeColor;
		public Gradient _neutralColor;
		public Aura _aura { get; private set; }
		public BaseTalent _talent { get; private set; }
		RectTransform _rt;
		RectTransform _textRt;
		Image _background;
		public GUIType _type { get; private set; } = GUIType.Aura;

		public enum GUIType { Aura,Talent};

		// Start is called before the first frame update
		void Start()
		{
			_rt = GetComponent<RectTransform>();
			_textRt = _text.GetComponent<RectTransform>();
			_background = GetComponent<Image>();
		}

 
		public void Init(Aura aura)
		{
			_aura = aura;
			gameObject.name = _aura._name;
			Start();
			SetIconPolarity(_aura._polarity);
			_image.sprite = _aura._sprite!=null?_aura._sprite : Resources.Load<Sprite>("Icons/Default");
			switch (_aura._stackingType)
			{
				case BaseAura.StackingType.None:
					_text.gameObject.SetActive(false);
					break;
				case BaseAura.StackingType.Case:
					_text.gameObject.SetActive(_aura._cases>1);
					break;
				case BaseAura.StackingType.Duration:
					_text.gameObject.SetActive(_aura._duration > 1);
					break;

			}
			Refresh();
			Animate(true);
		}
		void SetIconPolarity(CodeTools.Polarity polarity)
		{
			switch (polarity)
			{
				case CodeTools.Polarity.Neutral:
					_image.GetComponent<UIGradient>().LinearGradient = _neutralColor;
					break;
				case CodeTools.Polarity.Positive:
					_image.GetComponent<UIGradient>().LinearGradient = _positiveColor;
					break;
				case CodeTools.Polarity.Negative:
					_image.GetComponent<UIGradient>().LinearGradient = _negativeColor;
   					break;
			}

		}
		void Update()
		{
			RefreshTextTransform();
		}
		void RefreshTextTransform()
		{
			if (!_text.gameObject.activeSelf)
				return;
			_textRt.sizeDelta = _rt.sizeDelta * 0.67f;
			_textRt.anchoredPosition = new Vector2(_rt.sizeDelta.x * -0.1f, 0);
		}
		public void Init(BaseTalent talent)
		{
			_talent = talent;
			Start();
			SetIconPolarity(_talent._polarity);
			_image.sprite = _talent._sprite != null ? _talent._sprite : Resources.Load<Sprite>("Icons/Default");
			_text.gameObject.SetActive(false);

		}
		void Animate(bool enabled)
		{
			if (enabled)
				transform.localScale = Vector3.zero;
			else
			{
				GetComponent<LerpTransform>()._animationCurveScale = GetComponent<LerpColor>()._curve;
				transform.localScale = Vector3.one;
			}
			gameObject.SetActive(enabled);
			UIAnimationTools.ImageFadeIn(enabled, GetComponent<LerpColor>(), GetComponent<LerpTransform>(), 7, 0);
		}
		public void Destroy()
		{
			Animate(false);
			Invoke("DestroyImmediate", 1 / 7f);
		}
		void DestroyImmediate()
		{
			Destroy(this);
		}
		public void Refresh()
		{
			//Debug.Log("REFRESH");
			switch (_aura._stackingType)
			{
				case BaseAura.StackingType.Case:
					_text.gameObject.SetActive(_aura._cases > 1);
					_text.text = _aura._cases.ToString();
					break;
				case BaseAura.StackingType.Duration:
					_text.gameObject.SetActive(true);
					_text.text = _aura._duration.ToString();
					break;
				default:
					_text.gameObject.SetActive(false);
					break;
			}
			RefreshTextTransform();
		}
		public void Highlight()
		{
			//Debug.Log("Highlight!");
			//Vector2 position = GameManager._current._guiParts._UICanvas.WorldToCanvasPosition(_rt.position);
			Vector2 pos = CodeTools.GetRecttransformPivotPoint(_rt, new Vector2(1, 1),true);
			//Debug.Log(pos);
			TooltipSystem.DisplayTooltip(_aura, pos);
			return;
		}
		public void Unhighlight()
		{
			TooltipSystem.HideAllTooltips();
			//_tooltip.HideTooltips();
		}
	}
}
