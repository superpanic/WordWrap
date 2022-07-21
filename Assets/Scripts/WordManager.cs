using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WordWrap {
	public class WordManager : MonoBehaviour {

		private string Word;
		private List<GameObject> Letters = new List<GameObject>();
		public GameObject PrefabLetter;

		public void Setup(string s, int col) {
			Word = s;
			Vector3 pos = new Vector3();
			if (!PrefabLetter) Debug.Log("A prefab letter has not been assigned!");
			GameObject letterObject = Instantiate(PrefabLetter, pos, Quaternion.identity) as GameObject;
			Letters.Add(letterObject);


			for (int letterIndex = 0; letterIndex < Word.Length; letterIndex++) {
				int offset = Word.Length / 2;
				char letter = char.ToUpper(Word[letterIndex]);
				AddLetter(letterObject, col, letterIndex, offset);
			}


		}

		private void AddLetter(GameObject l, int col, int letterIndex, int offset) {
			
		}

	}
}