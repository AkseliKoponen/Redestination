using UnityEngine;
using UnityEngine.SceneManagement;

namespace RD
{
	public class Loader : MonoBehaviour
	{
		public string _mainMenuSceneName = "MainMenu";
		float waitTime = 1;
		// Start is called before the first frame update
		void Start()
		{
		}
		private void Update()
		{
			waitTime -= Time.deltaTime;
			if (waitTime < 0)
			{
				SceneManager.LoadSceneAsync(_mainMenuSceneName, LoadSceneMode.Single);
				Destroy(gameObject);
			}
		}

	}
}
