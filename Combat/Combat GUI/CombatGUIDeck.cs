using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RD.Combat
{
	public class CombatGUIDeck : MonoBehaviour
	{
		Deck _deck;
		TextMeshProUGUI _deckText;
		Image _image;
		int _cardCount = -1;
		DeckType _deckType;
		public enum DeckType { Draw,Discard,Deplete}
		private void Start()
		{
			_deckText = GetComponentInChildren<TextMeshProUGUI>();
			_image = GetComponentInChildren<Image>();
		}
		public void DisplayDeck(Deck d,DeckType dt = DeckType.Draw)
		{
			_deck = d;
			_deckType = dt;
			UpdateText();
		}

		private void Update()
		{
			if(_deck!=null)
				UpdateText();
		
		}
		public void UpdateText()
		{
			if (_cardCount != _deck._ideas.Count)
			{
				_deckText.text = _deck._ideas.Count.ToString();
				//Tiny visual effect here
				if (_deckType == DeckType.Deplete) 
				{
					if (_deck._ideas.Count == 0) {
						_image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0);
						_deckText.color = new Color(_deckText.color.r, _deckText.color.g, _deckText.color.b, 0); }
					else
					{
						_image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 1);
						_deckText.color = new Color(_deckText.color.r, _deckText.color.g, _deckText.color.b, 1);
					}
				}
			
			}
			_cardCount = _deck._ideas.Count;
		}
	}
}
