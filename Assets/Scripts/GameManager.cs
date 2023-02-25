using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace WordWrap {

	public enum GameState {
		GameSetup,
		PlayerLookingForWord,
		WordFound,
		HighlightDelay,
		CollectFoundWord,
		StartMoveAllWordsToLeft,
		MovingAllWordsToLeft,
		AddNewWordsToGrid
	}

	public class GameManager : MonoBehaviour {
		public const byte MAX_ROWS = 7;
		public const byte MAX_COLS = 9;
		private const float OUTSIDE_RIGHT_X_POS = 7.0f;
		public const float GRID_SPACING = 1.1f;
		public const float EDGE_SPACING = 0.1f;
		public const byte GRID_COL_OFFSET = 4;

		public const float EDGE_ROTATION_ANGLE = 15f;

		private System.Random Rnd;

		GameState CurrentGameState;

		public GameObject PrefabLetter;
		public GameObject WordUsedNotification;
		public ScoreCounter ScoreDisplay;
		public TMP_Text WordCountDisplay;
		public TMP_Text GameTimeDisplay;

		private DictionaryManager DictionaryFull;
		private DictionaryManager DictionaryCommonWords;

		private float HighlightTimer;
		private const float HightlightTimerDelay = 0.5f;

		private int WordCount;

		private const float GAME_START_SECONDS = 20f;
		private float GameTimer;

		private List<List<GameObject>> WordObjectsInPlay = new List<List<GameObject>>();
		private List<string>           WordStringsInPlay = new List<string>();
		private List<GameObject>       SelectedWordLetters = new List<GameObject>();
		private string                 SelectedWordString = "";

		private List<string>           AllWordStringsFound = new List<string>();
		private int                    AllWordStringsFoundIndex = 0;

		void Start() {
			if(Rnd==null) Rnd = new System.Random();
			DictionaryFull = new DictionaryManager(path:"Assets/Dictionaries/sorted_words.txt", low:3, high:MAX_COLS);
			DictionaryCommonWords = new DictionaryManager(path:"Assets/Dictionaries/common_words_eu_com.txt", low:3, high:MAX_ROWS);
//			DictionaryCommonWords = new DictionaryManager(path: "Assets/Dictionaries/animals.txt", low: 3, high: MAX_ROWS);
			Debug.Assert(PrefabLetter, "A prefab letter has not been assigned!");
			Debug.Assert(WordUsedNotification, "Word used notification has not been assigned!");
			Debug.Assert(ScoreDisplay, "Score display not assigned!");
			WordUsedNotification.SetActive(false);
			CurrentGameState = GameState.GameSetup;
			Debug.Assert(WordCountDisplay, "WordCountDisplay not assigned!");
			WordCount = 0;
			WordCountDisplay.text = WordCount.ToString();
		}

		void Update() {
			switch(CurrentGameState) {
				case GameState.GameSetup:
					IsNewWordsAddedToGrid();
					ScoreDisplay.SetScore(0);
					CurrentGameState = GameState.PlayerLookingForWord;
					GameTimer = Time.time + GAME_START_SECONDS;
					break;

				case GameState.PlayerLookingForWord:
					if (IsWordFound()) {
						CurrentGameState = GameState.WordFound;
						Debug.Log($"Word found: {SelectedWordString}");
					}
					UpdateGameTime();
					break;

				case GameState.WordFound:
					if(IsWordAlreadyUsed(SelectedWordString)) {
						WordUsedNotification.SetActive(true);
						CurrentGameState = GameState.PlayerLookingForWord;
					} else {
						WordFound();
						HighlightTimer = Time.time + HightlightTimerDelay;
						CurrentGameState = GameState.HighlightDelay;
					}
					break;

				case GameState.HighlightDelay:
					if(IsHightlightTimerDone()) {
						StartExplodeSelectedWords();
						ScoreDisplay.AddScore(SelectedWordString);
						AddTime(SelectedWordString);
						CurrentGameState = GameState.CollectFoundWord;
					}
					break;

				case GameState.CollectFoundWord:
					if(IsWordFoundCollected()) {
						RemoveCollectedWords();
						CurrentGameState = GameState.StartMoveAllWordsToLeft;
					}
					break;

				case GameState.StartMoveAllWordsToLeft:
					if(StartedMovingWordsToLeft()) {
						CurrentGameState = GameState.MovingAllWordsToLeft;
					}
					break;

				case GameState.MovingAllWordsToLeft:
					if(HasAllWordsMovedToLeft()) {
						CurrentGameState = GameState.AddNewWordsToGrid;
					}
					break;
				
				case GameState.AddNewWordsToGrid:
					if(IsNewWordsAddedToGrid()) {
						CurrentGameState = GameState.PlayerLookingForWord;
					}
					break;
			}
		}

		private void UpdateGameTime() {
			GameTimeDisplay.text = Mathf.Max(0f, GameTimer - Time.time).ToString("00.00");
		}

		private void AddTime(string s) {
			GameTimer = GameTimer + s.Length * 10f;
		}

		private bool StartedMovingWordsToLeft() {
			int column=0;
			int delayCounter = 0;
			foreach(List<GameObject> goList in WordObjectsInPlay) {
				foreach(GameObject go in goList) {
					Letter l = go.GetComponent<Letter>();
					Vector3 destination = new Vector3(CalculateXPos(column), go.transform.position.y, 0f);
					delayCounter++;
					l.SetDestinationTarget(destination, delayCounter*0.01f);
				}
				column++;
			}
			
			return true;
		}

		private bool HasAllWordsMovedToLeft() {
			foreach(List<GameObject> goList in WordObjectsInPlay) {
				foreach(GameObject go in goList) {
					Letter l = go.GetComponent<Letter>();
					if(l.GetIsMoving()) {
						return false;
					}
				}
			}
			return true;
		}

		private string GetNextWord(int col, out bool isRandomWord) {
			string word = "";
			isRandomWord = false;
			if(AllWordStringsFound.Count >= MAX_COLS) {
				word = AllWordStringsFound[AllWordStringsFoundIndex];
				AllWordStringsFoundIndex++;
				if(AllWordStringsFoundIndex>=AllWordStringsFound.Count) AllWordStringsFoundIndex = 0;
			} else {
				if(col == 0 && SelectedWordString.Length > 0) {
					word = SelectedWordString;
				} else {
					word = DictionaryCommonWords.GetRandomWord();
					isRandomWord = true;
				}
			}
			return word;
		}

		private bool IsNewWordsAddedToGrid() {
			int nCols = MAX_COLS-WordObjectsInPlay.Count;
			int colOffset = MAX_COLS-nCols;
			for (int col = 0; col < nCols; col++) {
				bool isRandomWord = false;
				string word = GetNextWord(col, out isRandomWord);
				Debug.Log($"Random word {col+1}: {word}");
				List<GameObject> myWord = new List<GameObject>();
				for (int letterIndex = 0; letterIndex < word.Length; letterIndex++) {	
					char letter = char.ToUpper(word[letterIndex]);
					GameObject letterObject = AddLetterToScene(letter, col+colOffset, letterIndex, word.Length);
					Letter letterProperties = letterObject.GetComponent<Letter>();
					letterProperties.SetMyWord(myWord);
					if(isRandomWord) letterProperties.SetIsRandom(true);
					bool isLetterInFocus = (letterIndex == word.Length/2);
					if(isLetterInFocus) letterProperties.SetInFocus();
					ColorBlock(letterProperties);
					myWord.Add(letterObject);
				}
				WordObjectsInPlay.Add(myWord);
				WordStringsInPlay.Add(word);
			}

			return true;
		}

		private void ColorBlock(Letter letterProperties) {
			bool isRandomWord = letterProperties.IsRandom();
			bool isLetterInFocus = letterProperties.GetIsInFocus();
			if(isLetterInFocus) {
				if(isRandomWord) {
					letterProperties.SetBaseColor((int)GameColors.RANDOM_WORD_FOCUS);
				} else {
					letterProperties.SetBaseColor((int)GameColors.WORD_FOCUS);
				}
			} else {
				if(isRandomWord) {
					letterProperties.SetIsRandom(true);
					letterProperties.SetBaseColor((int)GameColors.RANDOM_WORD);
				} else {
					letterProperties.SetBaseColor((int)GameColors.WORD);
				}
			}
		}

		private bool IsLetterInFocus(int letterIndex, int wordLength) {
			return (letterIndex == wordLength/2);
		}

		private void RemoveCollectedWords() {
			int wordLength = SelectedWordLetters.Count;
			for(int i=0; i<wordLength; i++) {
				foreach(GameObject go in WordObjectsInPlay[i]) {
					Destroy(go);
				}
			}
			WordObjectsInPlay.RemoveRange(0, Math.Min(wordLength, WordObjectsInPlay.Count));
			WordStringsInPlay.RemoveRange(0, Math.Min(wordLength, WordStringsInPlay.Count));
			SelectedWordLetters.Clear();
		}

		private bool IsHightlightTimerDone() {
			return(Time.time >= HighlightTimer);
		}

		private bool IsWordFound() {
			if( Input.GetMouseButtonUp(0) ) {
				Vector3 mouseClickPos = Input.mousePosition;
				Ray ray = Camera.main.ScreenPointToRay(mouseClickPos);
				RaycastHit mouseClickHit;
				if( Physics.Raycast(ray, out mouseClickHit) ) {
					if(mouseClickHit.collider.tag == "Letter") {
						Transform letter = mouseClickHit.transform;
						return MoveWordAndCenterLetter(letter);
					}
				} 
			}
			return false;
		}

		private bool IsLetterAtEdge(int centerIndex, int wordLength, int index) {
			if (index == centerIndex) return false; // center block itself never needs to rotate
			int centerOffset = centerIndex - index;
			if (centerOffset < 0) { // bottom edge
				centerOffset = -centerOffset;
				if (IsAtEdgeButNotLastChar(wordLength, index, centerOffset)) return true;
				if (IsOutsideEdge(centerOffset)) return true;
			} else { // top edge
				if (IsAtEdgeButNotFirstChar(index, centerOffset)) return true;
				if (IsOutsideEdge(centerOffset)) return true;
			}
			return false;

			static bool IsAtEdgeButNotLastChar(int wordLength, int index, int centerOffset) {
				return centerOffset == 3 && wordLength - 1 > index;
			}

			static bool IsOutsideEdge(int centerOffset) {
				return centerOffset > 3;
			}

			static bool IsAtEdgeButNotFirstChar(int index, int centerOffset) {
				return centerOffset == 3 && index > 0;
			}
		}

		private bool MoveWordAndCenterLetter(Transform letter) {
			SelectedWordLetters.Clear();
			Letter letterProperties = letter.GetComponent<Letter>();

			List<GameObject> myWord = letterProperties.GetMyWord();
			int wordLength = myWord.Count;
			float currentXPos = letter.transform.position.x;
			float rot;
			int centerIndex = letterProperties.LetterIndex;
			for (int i = 0; i < wordLength; i++) {
				Letter currentLetterProperties = myWord[i].transform.GetComponent<Letter>();
				bool isLetterAtEdge = IsLetterAtEdge(centerIndex, wordLength, i);
				// position
				int offset = i - centerIndex;
				int clampedOffset = Math.Clamp(offset, -3, 3);
				float currentYPos = -GRID_SPACING * clampedOffset + EDGE_SPACING * (clampedOffset - offset);
				float currentZPos;
				if (isLetterAtEdge) currentZPos = Math.Max(0, Math.Abs(offset) - 2) * EDGE_SPACING;
				else currentZPos = 0f;
				Vector3 pos = new Vector3(currentXPos, currentYPos, currentZPos);
				currentLetterProperties.SetDestinationTarget(pos);

				// rotation
				rot = 0f;
				if (isLetterAtEdge) {
					if (i < centerIndex) rot = EDGE_ROTATION_ANGLE;
					else rot = -EDGE_ROTATION_ANGLE;
				}
				currentLetterProperties.SetRotationTargetX(rot);

				// color
				if (currentLetterProperties.IsRandom()) {
					currentLetterProperties.SetBaseColor((int)GameColors.RANDOM_WORD);
				} else {
					currentLetterProperties.SetBaseColor((int)GameColors.WORD);
				}
			}

			if(letterProperties.GetIsInFocus()) {
				return GetSelectedWord(letter);
			} else {
				letterProperties.SetInFocus();
				ColorBlock(letterProperties);
			}
			return false;
		}

		private bool GetSelectedWord(Transform letterObject) {
			Letter letterProperties = letterObject.GetComponent<Letter>();
			List<GameObject> word = letterProperties.GetMyWord();
			int wordIndex = WordObjectsInPlay.IndexOf(word); // ! Potential problem if the same word is present more than once?
			SelectedWordString = "";
			List<Letter> selectedWordLetterList = new List<Letter>();
			for (int i = 0; i <= wordIndex; i++) {
				List<GameObject> wordObject = WordObjectsInPlay[i];
				Letter selectedLetter = null;
				for (int j = 0; j < wordObject.Count; j++) {
					Letter lp = wordObject[j].GetComponent<Letter>();
					if (lp.GetIsInFocus()) {
						selectedLetter = lp;
						selectedWordLetterList.Add(lp);
						SelectedWordLetters.Add(wordObject[j]);
						break;
					}
				}
				Debug.Assert(selectedLetter != null, $"Word {WordStringsInPlay[i]} does not have a selected letter!");
				SelectedWordString += selectedLetter.GetLetter().ToString();
				if(selectedLetter.IsRandom()) {
					selectedLetter.SetBaseColor((int)GameColors.RANDOM_WORD_FOCUS);
				} else {
					selectedLetter.SetBaseColor((int)GameColors.WORD_FOCUS);
				}
			}

			if (DictionaryFull.SearchString(SelectedWordString) > 0)
				return true;

			return false;
		}

		private void WordFound() {
			for (int i = 0; i < SelectedWordLetters.Count; i++) {
				Letter le = SelectedWordLetters[i].GetComponent<Letter>();
				le.SetIsSelected(true);
				le.SetBaseColor((int)GameColors.SUCCESS_GREEN);
			}
			AllWordStringsFound.Add(SelectedWordString);
			WordCount = WordCount + 1;
			WordCountDisplay.text = WordCount.ToString();
		}

		private bool IsWordAlreadyUsed(string word) {
			if (AllWordStringsFound.Contains(word)) return true;
			return false;
		}

		private bool IsWordFoundCollected() {
			for(int i=0; i<SelectedWordLetters.Count; i++) {
				Letter letter = SelectedWordLetters[i].GetComponent<Letter>();
				List<GameObject> w = letter.GetMyWord();
				for (int j = 0; j < w.Count; j++) {
					Letter l = w[j].GetComponent<Letter>();
					if(l.GetIsMoving()) return false;
				}
			}
			return true;
		}

		private GameObject AddLetterToScene(char letter, int col, int letterIndex, int wordLength) {
			Vector3 outside = new Vector3(OUTSIDE_RIGHT_X_POS, (float)Rnd.Next(-5,5), -2.5f*col-0.1f*letterIndex);
			Vector3 pos = new Vector3(CalculateXPos(column:col), CalculateYPos(wordLength:wordLength, letterIndex:letterIndex), 0f);
			GameObject letterObject = Instantiate(PrefabLetter, outside, Quaternion.identity) as GameObject;
			letterObject.GetComponent<Letter>().SetDestinationTarget(pos, 0.01f);
			InitLetterObject(letterIndex, letterObject);
			SetLetter(letter, letterObject);
			return letterObject;
		}

		private static void InitLetterObject(int letterIndex, GameObject letterObject) {
			Letter letterProperties = letterObject.GetComponent<Letter>();
			letterProperties.LetterIndex = letterIndex;
		}

		private static void SetLetter(char c, GameObject letterObject) {
			Letter l = letterObject.GetComponent<Letter>();
			l.SetLetter(c);
		}

		private void StartExplodeSelectedWords() {
			float spread = 1.2f;
			float distance = 7.5f;
			for(int i=0; i<SelectedWordLetters.Count; i++) {
				Letter letter = SelectedWordLetters[i].GetComponent<Letter>();
				List<GameObject> w = letter.GetMyWord();
				for (int j = 0; j < w.Count; j++) {
					Letter l = w[j].GetComponent<Letter>();
					Transform t = w[j].transform;
					Vector3 p = t.position;
					float x = p.x - distance - p.x;
					float y = p.y + p.y * spread;
					Vector3 v = new Vector3(x,y,p.z);
					l.SetDestinationTarget(v, i*0.1f);
				}
			}
		}

		private float CalculateXPos(int column) {
			return GRID_SPACING * (column - GRID_COL_OFFSET);
		}

		private float CalculateYPos(int wordLength, int letterIndex) {
			int offset = wordLength / 2;
			return -GRID_SPACING * (letterIndex - offset);
		}


	}
}