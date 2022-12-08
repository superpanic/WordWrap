using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.FilePathAttribute;

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

		private float RotationTimeStamp = 1.0f;
		private readonly float RotationDuration = 0.3f;
		private float RotationStartAngleX;

		private float MotionTimeStamp;
		private readonly float MotionDuration = 0.01f;
		private float CurrentMotionDuration = 0f;
		private Vector3 MotionStart;
		private float MotionDistance;

		private List<GameObject> MyWord;
		private Vector3 MotionTarget;
		private float RotationTargetX;
		private float MoveDelay = 0f;
		private bool IsMoving = false;
		private bool IsRotating = false;
		private bool IsInFocus = false;
		private bool IsSelected = false;
		private bool IsRandomWord = false;

		private int ColorListLength;

		void Awake() { // LetterText and MultiText needs to be set before Start() event
			Transform letterTransform = this.transform.Find("letter");
			LetterText = letterTransform.GetComponent<Text>();
			Debug.Assert(LetterText, "Letter text object is not assigned!");

			Transform multiTransform = this.transform.Find("multiplier");
			MultiText = multiTransform.GetComponent<Text>();
			Debug.Assert(MultiText, "Multiplier text object is not assigned!");
		}

		private void Start() {
			ColorListLength = Enum.GetNames(typeof(GameColors)).Length;
		}

		void Update() {
			MoveToTargetPos();
			RotateToTargetX();
		}

		public void MoveToTargetPos() {
			if(IsMoving) {
				if (MoveDelay > 0f) {
					MotionTimeStamp = Time.time; // update time stamp until it's time to move
					MoveDelay = Math.Max(0f, MoveDelay - Time.deltaTime);
					return;
				}
				float timeDiff = Time.time - MotionTimeStamp;
				float timePart = timeDiff / CurrentMotionDuration;
				if (timePart >= 1.0f) {
					IsMoving = false;
					transform.position = MotionTarget;
				} else {
					Vector3 moveTo = Vector3.Lerp(MotionStart, MotionTarget, Mathf.SmoothStep(0.0f, 1.0f, timePart));
					transform.position = moveTo;
				}
			}
		}

		public void RotateToTargetX() {
			if (IsRotating) {
				float timeDiff = Time.time - RotationTimeStamp;
				float timePart = timeDiff / RotationDuration;
				Vector3 r = transform.localEulerAngles;
				if (timePart >= 1.0f) {
					IsRotating = false;
					r.x = RotationTargetX;
				} else {
					float rot = Mathf.LerpAngle(RotationStartAngleX, RotationTargetX, timePart);
					r.x = rot;
				}
				transform.localEulerAngles = r;
			}
		}

		public void SetDestinationTarget(Vector3 destination, float delay = 0f) {
			if(destination != transform.position) {
				MotionTarget = destination;
				MotionStart = transform.position;
				MotionDistance = Vector3.Distance(MotionStart, MotionTarget);
				CurrentMotionDuration = 0.5f + MotionDistance * MotionDuration;
				MoveDelay = delay;
				MotionTimeStamp = Time.time;
				IsMoving = true;
			}
		}

		public void SetRotationTargetX(float rotationTargetX) {
			if(rotationTargetX != transform.localEulerAngles.x) {
				RotationTargetX = rotationTargetX;
				RotationStartAngleX = transform.localEulerAngles.x;
				RotationTimeStamp = Time.time;
				IsRotating = true;
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
			if (index != -1 && index < Common.Values.Length) {
				val = Common.Values[index];
			}
			if (val <= 1) {
				MultiText.text = "";
			} else {
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

		public bool IsRandom() {
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