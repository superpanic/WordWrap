using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WordWrap {
	public class Letter : MonoBehaviour {

		private Text UIText;
		public int Row;
		public int CharNum;

		void Start() {
			Transform transform = this.transform.Find("letter");
			UIText = transform.GetComponent<Text>();
		}

		private void OnMouseDown() {
			Debug.Log(UIText.text);
			Debug.Log(Row);
			Debug.Log(CharNum);
		}

		public void SetLetter(char c) {
			UIText.text = "" + c;
		}
		public char GetLetter() {
			return UIText.text[0];
		}
	}
}