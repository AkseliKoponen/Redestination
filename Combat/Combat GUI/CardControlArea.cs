using UnityEngine;
using UnityEngine.EventSystems;

namespace RD.Combat
{
	public class CardControlArea : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		CombatGUICard card;
		// Start is called before the first frame update
		void Awake()
		{
			card = GetComponentInParent<CombatGUICard>();
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			card.OnBeginDrag(eventData);
			//Debug.Log("Begin Drag");
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			card.OnEndDrag(eventData);
			//Debug.Log("End Drag");
		}

		public void OnDrag(PointerEventData eventData)
		{
			card.OnDrag(eventData);
		}

	}
}
