using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WordWrap {
	public class Letter : MonoBehaviour {

		private Text UIText;
		public int Col;
		public int LetterIndex;
		private List<GameObject> MyWord;
		private Vector3 Destination;
		private bool IsMoving = false;

		void Start() {
			Transform transform = this.transform.Find("letter");
			UIText = transform.GetComponent<Text>();
		}

		void Update() {
			if(IsMoving) {
				if(transform.position == Destination) {
					IsMoving = false;
				} else {
					float l = Mathf.Lerp(transform.position.y, Destination.y, 0.1f);
					Vector3 p = transform.position;
					p.y = l;
					transform.position = p;
				}
			}
		}

		public void SetDestination(Vector3 d) {
			if(d != transform.position) {
				Destination = d;
				IsMoving = true;
			}
		}

		public void SetLetter(char c) {
			UIText.text = "" + c;
		}
		public char GetLetter() {
			return UIText.text[0];
		}

		public void SetMyWord(List<GameObject> w) {
			MyWord = w;
		}

		public List<GameObject> GetMyWord() {
			return MyWord;
		}

	}
}