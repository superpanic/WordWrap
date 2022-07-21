using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WordWrap {
	public class Letter : MonoBehaviour {

		private Text UIText;
		public int Col;
		public int LetterIndex;
		private WordManager Word;

		void Start() {
			Transform transform = this.transform.Find("letter");
			UIText = transform.GetComponent<Text>();
		}

		public void SetLetter(char c) {
			UIText.text = "" + c;
		}
		public char GetLetter() {
			return UIText.text[0];
		}

		public void SetWordManager(WordManager w) {
			Word = w;
		}
	}
}