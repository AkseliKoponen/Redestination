using System.Collections;
using System.Collections.Generic;
using RD.Combat;
using RD;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RD
{
	public class MessageBox : MonoBehaviour
	{
		public Transform _leftEdge;
		public AnimationCurve _animationCurveSideToSide;
		public GameObject _prefabMessage;
		public List<TMP_ColorGradient> _colorGradients;

		public void DisplayTurn(CombatCharacter combatCharacter, float messageTime = 2f) {
			string msg = combatCharacter.GetName()+"'s turn";
			TMP_ColorGradient _grad = _colorGradients[0];
			switch (combatCharacter._alliance)
			{
				case CombatCharacter.Alliance.Friendly:
					_grad = _colorGradients[1];
					break;
				case CombatCharacter.Alliance.Enemy:
					_grad = _colorGradients[2];
					break;
				case CombatCharacter.Alliance.Player:
					_grad = _colorGradients[3];
					break;
				default:
					_grad = _colorGradients[0];
					break;
			}

			GameObject go = Instantiate(_prefabMessage, transform);
			TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
			text. colorGradientPreset = _grad;
			text.text = msg;
			text.transform.position = _leftEdge.position;
			GetComponent<Image>().enabled = true;
			StartCoroutine(HandleMessage(go, 1/messageTime));
		}

		IEnumerator HandleMessage(GameObject go, float speed)
		{
			float lerpStart = go.transform.position.x;
			float lerpEnd = -lerpStart;
			Vector3 pos = go.transform.position;
			float t = 0;
			while (t < 1)
			{
				t += CodeTools.Tm.GetUIDelta() * speed;
				go.transform.position = new Vector3(Mathf.Lerp(lerpStart, lerpEnd, _animationCurveSideToSide.Evaluate(t)), pos.y,pos.z);
				yield return null;
			}
			if (transform.childCount <= 2)
				GetComponent<Image>().enabled = false;
			Destroy(go);
		
		}
	}
}
