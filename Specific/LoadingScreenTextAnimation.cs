using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static RD.CodeTools;
namespace RD.UI {
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class LoadingScreenTextAnimation : MonoBehaviour
	{
		TextMeshProUGUI _textMesh;
		public TextAnimationType _textAnimationType;
		public float _tickTime = 1f;
		string originalText;
		public enum TextAnimationType { Ellipsis, LetterShuffle, None }
		public bool _running = true;
		private void Awake()
		{
			_textMesh = GetComponent<TextMeshProUGUI>();
			originalText = _textMesh.text;
			tempTime = _tickTime;
			if (_textAnimationType == TextAnimationType.LetterShuffle)
				Tick();

		}
		float tempTime;
		void Update()
		{
			if (_running)
			{
				tempTime -= Tm.GetUIDelta() * Random.Range(0.5f, 1.5f);
				if (tempTime <= 0)
				{
					Tick();
					tempTime += _tickTime;
				}
			}
			
		}

		void Tick()
		{
			switch (_textAnimationType)
			{
				case TextAnimationType.Ellipsis:
					string currentText = _textMesh.text;
					int i = currentText.IndexOf('.');
					if ((i > 0 && currentText.Length - i < 3) || i < 0)
					{
						_textMesh.text = currentText + ".";
					}
					else
						_textMesh.text = originalText;
					break;
				case TextAnimationType.LetterShuffle:
					_textMesh.text = DialogTools.ShuffleString(originalText.ToLower());
					//Debug.Log(_textMesh.text);
					break;
				case TextAnimationType.None:
					_running = false;
					_textMesh.text = originalText;
					break;
			}
		}
		public void Reset()
		{
			_running = false;
			_textMesh.text = originalText;
		}
	}
}
