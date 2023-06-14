using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static RD.CodeTools;

namespace RD.Combat
{
    public class CombatGUIActionOrb : MonoBehaviour
    {
        [SerializeField] List<Image> _frames;
		[SerializeField] Image _filling;
		[SerializeField] bool _useNumberOnly;
		[SerializeField] TextMeshProUGUI _currentNumber;
		[SerializeField] TextMeshProUGUI _maxNumber;
		[SerializeField] Color _orbColor;
		[SerializeField] Color _textColor;

		CombatCharacter _cc;
        private void Awake()
        {
            GetComponent<CanvasGroup>().alpha = 0;
			UpdateColor();//new Color(0, 0.8509804f, 0.8285179f));
        }
		private void Update()
		{
			UpdateColor();
		}
		public void UpdateActions(object card)
		{
			if (_cc!=null)
			{
				int index = Mathf.Clamp(_cc._actions.max - 1, 0, _frames.Count - 1);
				//Debug.Log("_actions Max = "+_cc._actions.max + "\nindex = " + index);
				if (index < 4 && !_useNumberOnly)
				{
					SetMaxActions(index);
					_filling.fillAmount = _cc._actions.GetPercentage();
				}
				else
				{
					foreach (Image frm in _frames)
						if(frm!=null)frm.gameObject.SetActive(false);
					_currentNumber.transform.parent.gameObject.SetActive(true);
					_currentNumber.text = _cc._actions.current.ToString();
					_maxNumber.text = _cc._actions.max.ToString();
				}
			}

		}
		void SetMaxActions(int index)
		{
			_currentNumber.transform.parent.gameObject.SetActive(false);
			Image activeImg = _frames[index];
			_filling.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, index == 3 ? 120 : 0);
			activeImg.gameObject.SetActive(true);
			foreach (Image frm in _frames)
				if (frm != activeImg) frm.gameObject.SetActive(false);
			_filling.fillAmount = 1;
		}
		void ClearPreviousCharacter()
		{
			if (_cc!=null)
			{
				_cc._combatEvents._OnIdea._delegates -= UpdateActions;
				_cc = null;
				SetMaxActions(0);
			}
		}
		public void SetCharacter(CombatCharacter cc)
		{
			ClearPreviousCharacter();
			_cc = cc;
			_cc._combatEvents._OnIdea._delegates += UpdateActions;
			GetComponent<CanvasGroup>().alpha = 1;
			UpdateActions(null);
		}

        void UpdateColor()
        {
			_filling.color = _orbColor;
			foreach (TextMeshProUGUI txt in _maxNumber.transform.parent.gameObject.GetComponentsInChildren<TextMeshProUGUI>())
				txt.color = _textColor;
        }

    }
}
