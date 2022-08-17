using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WordWrap {

	public enum GameColors {
		Letter,
		Black,
		Background,
		Block,
		BlockFocus,
		BlockWord
	}

	public class Letter : MonoBehaviour {
		private Text UIText;
		public int Col;
		public int LetterIndex;
		private float MotionSpeed = 0.1f;
		private Vector3 ScaleDefault;
		private float ScaleMultiplier = 1.0f;
		private List<GameObject> MyWord;
		private Vector3 Destination;
		private bool IsMoving = false;
		private bool IsInFocus = false;

		void Start() {
			Transform transform = this.transform.Find("letter");
			UIText = transform.GetComponent<Text>();
			ScaleDefault = this.transform.localScale;
		}

		void Update() {
			if(IsMoving) {
				if(transform.position == Destination) {
					IsMoving = false;
				} else {
					float l = Mathf.Lerp(transform.position.y, Destination.y, MotionSpeed);
					Vector3 p = transform.position;
					p.y = l;
					transform.position = p;
				}
			}
		}

		public void SetScale(float scaleMultiplier) {
			ScaleMultiplier = scaleMultiplier;
			Vector3 sc = this.transform.localScale;
			sc.x = ScaleDefault.x * ScaleMultiplier;
			sc.y = ScaleDefault.y * ScaleMultiplier;
			this.transform.localScale = sc;
		}

		public void SetDestination(Vector3 d) {
			if(d != transform.position) {
				Destination = d;
				IsMoving = true;
			}
		}

		public void SetInFocus() {
			if(!IsInFocus) {
				for(int i=0; i<MyWord.Count; i++) {
					MyWord[i].GetComponent<Letter>().RemoveInFocus();
				}
				IsInFocus = true;
				//SetBaseColor((int)GameColors.BlueLight);
			}
		}

		public void RemoveInFocus() {
			IsInFocus = false;
			//SetBaseColor((int)GameColors.Blue);
		}

		public bool GetIsInFocus() {
			return IsInFocus;
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

		public void SetBaseColor(int col) {
			Transform square = this.transform.Find("square");
			Colorize colorizer = square.GetComponent<Colorize>();
			colorizer.colorIndex = col;
		}

	}

}