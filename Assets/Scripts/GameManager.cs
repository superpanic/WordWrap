using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace WordWrap {

	public enum GameState {
		GameSetup,
		PlayerLookingForWord,
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
		public const byte GRID_COL_OFFSET = 4;

		private System.Random Rnd;

		GameState gameState = GameState.PlayerLookingForWord;

		public GameObject PrefabLetter;

		private DictionaryManager DictionaryFull;
		private DictionaryManager DictionaryCommonWords;


		private List<List<GameObject>> WordObjectsInPlay = new List<List<GameObject>>();
		private List<string>           WordStringsInPlay = new List<string>();
		private List<GameObject>       SelectedWordLetters = new List<GameObject>();
		private string                 SelectedWordString = "";

		void Start() {
			if(Rnd==null) Rnd = new System.Random();
			DictionaryFull = new DictionaryManager(path:"Assets/Dictionaries/sorted_words.txt", low:3, high:MAX_COLS);
			DictionaryCommonWords = new DictionaryManager(path:"Assets/Dictionaries/common_words_eu_com.txt", low:3, high:MAX_ROWS);
			Debug.Assert(PrefabLetter, "A prefab letter has not been assigned!");
			gameState = GameState.GameSetup;
		}

		void Update() {
			switch(gameState) {
				case GameState.GameSetup:
					IsNewWordsAddedToGrid();
					gameState = GameState.PlayerLookingForWord;
					break;

				case GameState.PlayerLookingForWord:
					if(IsWordFound()) gameState = GameState.CollectFoundWord;
					break;

				case GameState.CollectFoundWord:
					if(IsWordFoundCollected()) {
						RemoveCollectedWords();
						gameState = GameState.StartMoveAllWordsToLeft;
					}
					break;

				case GameState.StartMoveAllWordsToLeft:
					if(StartedMovingWordsToLeft()) {
						gameState = GameState.MovingAllWordsToLeft;
					}
					break;

				case GameState.MovingAllWordsToLeft:
					if(HasAllWordsMovedToLeft()) {
						gameState = GameState.AddNewWordsToGrid;
					}
					break;
				
				case GameState.AddNewWordsToGrid:
					if(IsNewWordsAddedToGrid()) {
						gameState = GameState.PlayerLookingForWord;
					}
					break;
			}
		}

		private bool StartedMovingWordsToLeft() {
			int column=0;
			int delayCounter = 0;
			foreach(List<GameObject> goList in WordObjectsInPlay) {
				foreach(GameObject go in goList) {
					Letter l = go.GetComponent<Letter>();
					Vector3 destination = new Vector3(CalculateXPos(column), go.transform.position.y, 0f);
					delayCounter++;
					l.SetDestination(destination, delayCounter*0.01f);
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

		private bool IsNewWordsAddedToGrid() {
			int nCols = MAX_COLS-WordObjectsInPlay.Count;
			int colOffset = MAX_COLS-nCols;
			for (int col = 0; col < nCols; col++) {
				bool isSelectedWord = false;
				string word = "";
				if(col == 0 && SelectedWordString.Length > 0) {
					word = SelectedWordString;
					isSelectedWord = true;
				} else {
					word = DictionaryCommonWords.GetRandomWord();
				}
				Debug.Log($"Random word {col+1}: {word}");
				List<GameObject> myWord = new List<GameObject>();
				for (int letterIndex = 0; letterIndex < word.Length; letterIndex++) {	
					char letter = char.ToUpper(word[letterIndex]);
					GameObject letterObject = AddLetterToScene(letter, col+colOffset, letterIndex, word.Length);
					Letter letterProperties = letterObject.GetComponent<Letter>();
					letterProperties.SetMyWord(myWord);
					if(isSelectedWord) letterProperties.SetBaseColor((int)GameColors.BlockBonus);
					if(letterIndex == word.Length/2) {
						letterProperties.SetInFocus();
						if(isSelectedWord) {
							letterProperties.SetBaseColor((int)GameColors.BlockFocusBonus);
						} else {
							letterProperties.SetBaseColor((int)GameColors.BlockFocus);
						}
					}
					myWord.Add(letterObject);
				}

				WordObjectsInPlay.Add(myWord);
				WordStringsInPlay.Add(word);
			}

			return true;
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

		private bool IsWordFound() {
			if( Input.GetMouseButtonUp(0) ) {
				Vector3 mouseClickPos = Input.mousePosition;
				Ray ray = Camera.main.ScreenPointToRay(mouseClickPos);
				RaycastHit mouseClickHit;
				if( Physics.Raycast(ray, out mouseClickHit) ) {
					if(mouseClickHit.collider.tag == "Letter") {
						Transform letter = mouseClickHit.transform;
						return MoveWordToCenterLetter(letter);
					}
				} 
			}
			return false;
		}

		private bool MoveWordToCenterLetter(Transform letter) {
			ClearFoundWord();
			Letter letterProperties = letter.GetComponent<Letter>();

			List<GameObject> myWord = letterProperties.GetMyWord();
			int wordLength = myWord.Count;
			int letterIndex = letterProperties.LetterIndex;
			float currentXPos = letter.transform.position.x;
			int offset = letterIndex;
			for (int i = 0; i < wordLength; i++) {
				Letter currentLetterProperties = myWord[i].transform.GetComponent<Letter>();
				Vector3 pos = new Vector3(currentXPos, -GRID_SPACING * (i - offset), 0);
				currentLetterProperties.SetDestination(pos);
				currentLetterProperties.SetBaseColor((int)GameColors.Block);
			}

			if(letterProperties.GetIsInFocus()) {
				return GetSelectedWord(letter);
			} else {
				letterProperties.SetInFocus();
				letterProperties.SetBaseColor((int)GameColors.BlockFocus);
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
				selectedLetter.SetBaseColor((int)GameColors.BlockFocus);
			}

			if (DictionaryFull.SearchString(SelectedWordString) > 0) {
				for (int i = 0; i < selectedWordLetterList.Count; i++) {
					selectedWordLetterList[i].SetIsSelected(true);
					selectedWordLetterList[i].SetBaseColor((int)GameColors.BlockWord);
				}
				StartExplodeSelectedWords();
				return true;
			}
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
			Vector3 outside = new Vector3(OUTSIDE_RIGHT_X_POS, (float)Rnd.Next(-5,5), 0f); // TODO: make the transition effect work in Z also
			Vector3 pos = new Vector3(CalculateXPos(column:col), CalculateYPos(wordLength:wordLength, letterIndex:letterIndex), 0f);
			GameObject letterObject = Instantiate(PrefabLetter, outside, Quaternion.identity) as GameObject;
			letterObject.GetComponent<Letter>().SetDestination(pos, 0.01f);
			InitLetterObject(letterIndex, letterObject);
			SetLetter(letter, letterObject);
			return letterObject;
		}

		private static void InitLetterObject(int letterIndex, GameObject letterObject) {
			Letter letterProperties = letterObject.GetComponent<Letter>();
			letterProperties.LetterIndex = letterIndex;
		}

		private static void SetLetter(char c, GameObject letterObject) {
			Transform transform = letterObject.transform.Find("letter");
			Text textObject = transform.GetComponent<Text>();
			textObject.text = "" + c;
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
					l.SetDestination(v);
				}
			}
		}

		private void ClearFoundWord() {
			if (SelectedWordLetters.Count > 0) {
				for (int i = 0; i < SelectedWordLetters.Count; i++) {
					Letter l = SelectedWordLetters[i].GetComponent<Letter>();
					l.SetIsSelected(false);
					l.SetBaseColor((int)GameColors.BlockFocus);
				}
			}
			SelectedWordLetters.Clear();
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