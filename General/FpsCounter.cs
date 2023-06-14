using TMPro;
using UnityEngine;

namespace RD
{
	public class FpsCounter : MonoBehaviour
	{
		TextMeshProUGUI _tm;
		float updateTime = 0.5f;
		// Start is called before the first frame update
		void Start()
		{
			_tm = GetComponent<TextMeshProUGUI>();
		}

		// Update is called once per frame
		void Update()
		{
			updateTime -= Time.deltaTime;
			if (updateTime < 0)
			{
				updateTime+=0.5f;
				_tm.text = ((int)(1f / Time.deltaTime)).ToString();
			}
		}
	}
}
