using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WordWrap {

	public enum GameColors {
		LETTER,
		BLACK,
		WHITE,
		WORD,
		WORD_FOCUS,
		RANDOM_WORD,
		RANDOM_WORD_FOCUS,
		SUCCESS_GREEN,
		FAILURE_RED
	}

	public class Letter : MonoBehaviour {
		private Text LetterText;
		private Text MultiText;
		public int LetterIndex;
		private float MotionSpeed = 0.03f;

		private Vector3 ScaleDefault;
		private float ScaleMultiplier = 1.0f;
		private List<GameObject> MyWord;
		private Vector3 Destination;
		private float moveDelay = 0f;
		private bool IsMoving = false;
		private bool IsInFocus = false;
		private bool IsSelected = false;
		private bool IsRandomWord = false;

		private int ColorListLength;

		void Awake() { // LetterText and MultiText needs to be set before Start() event
			Transform letterTransform = this.transform.Find("letter");
			LetterText = letterTransform.GetComponent<Text>();

			Transform multiTransform = this.transform.Find("multiplier");
			MultiText = multiTransform.GetComponent<Text>();
		}

		private void Start() {
			ScaleDefault = this.transform.localScale;
			ColorListLength = Enum.GetNames(typeof(GameColors)).Length;
		}

		void Update() {
			MoveXY();
		}

		public void MoveXY() {
			if(IsMoving) {
				Vector3 delta = transform.position - Destination;
				if(delta.magnitude < 0.01) {
					transform.position = Destination;
					IsMoving = false;
				} else {
					if(moveDelay > 0f) {
						moveDelay = Math.Max(0f, moveDelay-Time.deltaTime);
					} else {
						float x = Mathf.Lerp(transform.position.x, Destination.x, MotionSpeed);
						float y = Mathf.Lerp(transform.position.y, Destination.y, MotionSpeed);
						float z = Mathf.Lerp(transform.position.z, Destination.z, MotionSpeed);
						Vector3 p = transform.position;
						p.x = x;
						p.y = y;
						p.z = z;
						transform.position = p;
					}
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

		public void SetDestination(Vector3 destination, float delay = 0f) {
			if(destination != transform.position) {
				Destination = destination;
				moveDelay = delay;
				IsMoving = true;
			}
		}

		public void SetInFocus() {
			if(!IsInFocus) {
				for(int i=0; i<MyWord.Count; i++) {
					MyWord[i].GetComponent<Letter>().RemoveInFocus();
				}
				IsInFocus = true;
			}
		}

		public void RemoveInFocus() {
			IsInFocus = false;
		}

		public bool GetIsInFocus() {
			return IsInFocus;
		}

		public void SetLetter(char c) {
			LetterText.text = "" + c;
			SetMultiplierText(c);
		}

		private void SetMultiplierText(char c) {
			int val = 1;
			int index = Array.IndexOf(Common.Letters, c);
			if (index != -1 && index < Common.Values.Length)
			{
				val = Common.Values[index];
			}
			if (val <= 1)
			{
				MultiText.text = "";
			}
			else
			{
				MultiText.text = val.ToString();
			}
		}

		public char GetLetter() {
			return LetterText.text[0];
		}

		public bool GetIsMoving() {
			return IsMoving;
		}

		public void SetIsSelected(bool b) {
			IsSelected = b;
		}

		public void SetIsRandom(bool b) {
			IsRandomWord = b;
		}

		public bool GetRandom() {
			return IsRandomWord;
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

		public void SetBaseColorOfWord(int col) {
			if(col>=ColorListLength) return;
			for(int i=0; i<MyWord.Count; i++) {
				MyWord[i].GetComponent<Letter>().SetBaseColor(col);
			}
		}

	}

}