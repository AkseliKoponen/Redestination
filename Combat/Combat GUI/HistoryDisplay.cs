using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static RD.CodeTools;

namespace RD.Combat
{
	public class HistoryDisplay : MonoBehaviour
	{
		[SerializeField] HistoryIcon _historyIconPrefab;
		[SerializeField] Transform _iconArea;
		int _maxEntryCount = 6;
		//[Range(200,600)]public int _maxLength;
		List<HistoryIcon> _entries = new List<HistoryIcon>();
		public static HistoryDisplay _current;
		private void Awake()
		{
			_current = this;
		}
		public static CardResult TempConvertObjIntoCR(List<CombatSelectableObject> cso)
		{
			List<TargetResult> trs = new List<TargetResult>();
			foreach(CombatSelectableObject cs in cso)
			{
				if(cs._cc)
					trs.Add(new TargetResult(cs._cc));
			}
			return new CardResult(trs);
		}
		public void AddEntry(TargetResult source, Card card, CardResult target)
		{
			if (_entries.Count >= _maxEntryCount)
			{
				Destroy(_entries[0].gameObject);
				_entries.RemoveAt(0);
			}
			HistoryIcon hi = Instantiate(_historyIconPrefab.gameObject, _iconArea).GetComponent<HistoryIcon>();
			hi.Init(source, target, card);
			//Instantiate History icon at right place
			_entries.Add(hi);
			PositionEntries();
		}
		void PositionEntries()
		{

		}
		
		public void HideAll(HistoryIcon source)
		{
			foreach (HistoryIcon hi in _entries)
				if (hi != source)
					hi.Lowlight();
		}
		public struct TargetResult
		{
			public CombatCharacter cc;
			public Damage damage;
			public TargetResult(CombatCharacter combatCharacter, Damage dmg = null)
			{
				cc = combatCharacter;
				damage = dmg;
			}
		}
		public class CardResult
		{
			//public List<Damage> _damages;
			//As the character or card gets deleted, perhaps save some other data?
			//Or instead of deletion, just hide them?
			//public List<CombatSelectableObject> _objectTargets;
			public List<TargetResult> _characterTargets;
			public List<Card> _cardTargets;
			public CardResult(List<Card> cardTargets)
			{
				_cardTargets = cardTargets;
			}
			public CardResult(List<TargetResult> characterTargets)
			{
				_characterTargets = characterTargets;
				//_characterTargets.AddRange(characterTargets);
			}

		}
	}
}
