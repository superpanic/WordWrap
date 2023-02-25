using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

namespace WordWrap {

        public class ScoreCounter : MonoBehaviour {

		private TMP_Text ScoreTextUI;
		private int Score;
		private int DisplayScore;
		private int OldScore;

		public float ScrollSpeed = 1.0f;
		private float ScrollTimeStamp;

//		private int[] Values   = new  int[26] {  1,   1,   1,   1,   1,   1,   1,   2,   2,   2,   3,   3,   3,   3,   3,   3,   3,   3,   3,   3,   3,   3,   4,   4,   5,   5  };
//		private char[] Letters = new char[26] { 'E', 'A', 'R', 'I', 'O', 'T', 'N', 'S', 'L', 'C', 'U', 'D', 'P', 'M', 'H', 'G', 'B', 'F', 'Y', 'W', 'K', 'V', 'X', 'Z', 'J', 'Q' };


		void Start() {
                        ScoreTextUI = gameObject.GetComponent<TMP_Text>();
                        Debug.Assert(ScoreTextUI, "TMP_Text component missing!");
                }

		public void AddScore(string word) {
			OldScore = Score;
			DisplayScore = Score;
			Score = Score + CalculateScore(word);			
			ScrollTimeStamp = Time.time + ScrollSpeed;
		}

		public void SetScore(int s) {
			Score = s;
		}

                void Update() {
			if (DisplayScore == Score) return;
			float timeDiff = ScrollTimeStamp - Time.time;
			if (timeDiff <= ScrollSpeed) {
				float timeDelta = 1.0f - (timeDiff / ScrollSpeed);
				DisplayScore = (int)Mathf.Lerp((float)OldScore, (float)Score, timeDelta);
			} else {
				DisplayScore = Score;
			}
			ScoreTextUI.text = DisplayScore.ToString().PadLeft(8, '0');
		}

		public int CalculateScore(string word) {
			int score = word.Length * 10;
			for(int i=0; i<word.Length; i++) {
				char c = word[i];
				score = score * GetCharMultiplier(c);
			}
			return score;
		}

		private int GetCharMultiplier(char c) {
			int index = Array.IndexOf(Common.BonusLetters, c);
			if (index == -1) return 1;
			if (index >= Common.Values.Length) return 1;
			return Common.Values[index];
		}

	}
}