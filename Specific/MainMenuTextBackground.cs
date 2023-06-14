using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace RD {
	public class MainMenuTextBackground : MonoBehaviour
	{
		public TextAsset _wordList;
		TextMeshProUGUI _textMesh;
		// Start is called before the first frame update
		void Start()
		{
			_textMesh = GetComponent<TextMeshProUGUI>();
			StartCoroutine(LoadWordSet(_wordList));
			
		}
		public IEnumerator LoadWordSet(TextAsset ta, string style1 = "<style=MainMenuPrim>", string style2 = "<style=MainMenuAlt>")
		{
			string textblock = ta.text;
			List<string> words = new List<string>();
			int limit = 10000;
			while (textblock.Contains(" ") && limit > 0)
			{

				string word = Capitalize(textblock.Substring(0, textblock.IndexOf(" ")));
				if (word.Length > 0 && !words.Contains(word))
					words.Add(word);
				if (textblock.Length > 0)
					textblock = textblock.Substring(textblock.IndexOf(" ") + 1);
				else
					textblock = "";
				limit--;
			}
			//Debug.Log("wordCount = "+words.Count);
			_textMesh.text = "";
			List<string> wordBackUp = new List<string>();
			wordBackUp.AddRange(words);
			int wordCount = 0;
			while (!_textMesh.isTextOverflowing) //(words.Count > 0 && limit > 0)
			{
				int i = Random.Range(0, words.Count);
				_textMesh.text += (wordCount % 2 == 0 ?style1:style2) + words[i]+"</s>";
				words.RemoveAt(i);
				if (words.Count == 0)
					words.AddRange(wordBackUp);
				wordCount++;
				if (wordCount%100 == 0 )
				{
					_textMesh.ForceMeshUpdate();
					yield return null;
				}
			}
			//Debug.Log("Took " + wordCount + " words to fill the screen");
			string Capitalize(string wrd)
			{
				if (wrd.Length > 0)
					return wrd.Substring(0, 1).ToUpper() + wrd.Substring(1);
				else
					return wrd;
			}
		}
		IEnumerator Populate(List<string> words)
		{
			int limit = 1500;
			float delay = 0.01f;
			float temptime = 0;
			
			while (words.Count > 0 && limit > 0)
			{
				temptime += delay;
				int i = Random.Range(0, words.Count);
				_textMesh.text += words[i];
				words.RemoveAt(i);
				limit--;
				while (temptime > 0)
				{
					temptime -= Time.deltaTime;
					yield return null;
				}
			}
		}

	}
}
