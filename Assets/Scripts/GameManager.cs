using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace WordWrap {

	public enum GameState {
		LookingForWords,
		WordFound
	}

	public class GameManager : MonoBehaviour {

		private System.Random Rnd;

		GameState gameState = GameState.LookingForWords;

		public GameObject PrefabLetter;

		private DictionaryManager DictionaryFull;
		private DictionaryManager DictionaryCommonWords;

		public byte MaxRows = 7;
		public byte MaxCols = 9;

		public float GridSpacing = 1.1f;
		public byte GridColOffset = 4;

		private List<List<GameObject>> WordObjects = new List<List<GameObject>>();
		private List<string>           WordStrings = new List<string>();
		private List<GameObject>       SelectedWordObjects = new List<GameObject>();
		private string                 SelectedWordString;

		void Start() {
			if(Rnd==null) Rnd = new System.Random();
			DictionaryFull = new DictionaryManager(path:"Assets/Dictionaries/sorted_words.txt", low:3, high:9);
			DictionaryCommonWords = new DictionaryManager(path:"Assets/Dictionaries/common_words_eu_com.txt", low:3, high:7);
			Debug.Assert(PrefabLetter, "A prefab letter has not been assigned!");
			AddWordsToScene();
		}

		void Update() {
			switch(gameState) {
				case GameState.LookingForWords:
					RunBrowsing();
					break;
				case GameState.WordFound:
					RunWordFound();
					break;
			}
		}

		private void RunBrowsing() {
			if( Input.GetMouseButtonUp(0) ) {
				Vector3 mouseClickPos = Input.mousePosition;
				Ray ray = Camera.main.ScreenPointToRay(mouseClickPos);
				RaycastHit mouseClickHit;
				if( Physics.Raycast(ray, out mouseClickHit) ) {
					if(mouseClickHit.collider.tag == "Letter") {
						Transform letter = mouseClickHit.transform;
						MoveWordToCenterLetter(letter);
					}
				} 
			}
		}

		private void RunWordFound() {
			for(int i=0; i<SelectedWordObjects.Count; i++) {
				//for(int j=0; j<) TODO: check if all letters have finished movement! 
			}
		}

		private void StartExplodeSelectedWords() {
			float spread = 1.2f;
			float distance = 7.5f;
			for(int i=0; i<SelectedWordObjects.Count; i++) {
				Letter letter = SelectedWordObjects[i].GetComponent<Letter>();
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

		private void AddWordsToScene() {
			for (int col = 0; col < MaxCols; col++) {
				string word = DictionaryCommonWords.GetRandomWord();
				Debug.Log($"Random word {col+1}: {word}");

				List<GameObject> myWord = new List<GameObject>();
				int offset = word.Length / 2;

				for (int letterIndex = 0; letterIndex < word.Length; letterIndex++) {	
					char letter = char.ToUpper(word[letterIndex]);
					GameObject letterObject = AddLetterToScene(letter, col, letterIndex, offset);
					Letter letterProperties = letterObject.GetComponent<Letter>();
					letterProperties.SetMyWord(myWord);
					if(letterIndex == word.Length/2) {
						letterProperties.SetInFocus();
						letterProperties.SetBaseColor((int)GameColors.BlockFocus);
					}
					myWord.Add(letterObject);
				}

				WordObjects.Add(myWord);
				WordStrings.Add(word);
			}
		}

		private void MoveWordToCenterLetter(Transform letter) {
			ClearColorOfFoundWord();
			Letter letterProperties = letter.GetComponent<Letter>();

			List<GameObject> myWord = letterProperties.GetMyWord();
			int wordLength = myWord.Count;
			int letterIndex = letterProperties.LetterIndex;
			float currentXPos = letter.transform.position.x;
			int offset = letterIndex;
			for (int i = 0; i < wordLength; i++) {
				Letter currentLetterProperties = myWord[i].transform.GetComponent<Letter>();
				Vector3 pos = new Vector3(currentXPos, -GridSpacing * (i - offset), 0);
				currentLetterProperties.SetDestination(pos);
				currentLetterProperties.SetBaseColor((int)GameColors.Block);
			}

			if(letterProperties.GetIsInFocus()) {
				GetSelectedWord(letter);
			} else {
				letterProperties.SetInFocus();
				letterProperties.SetBaseColor((int)GameColors.BlockFocus);
			}
		}

		private GameObject AddLetterToScene(char letter, int col, int letterIndex, int offset) {
			Vector3 pos = new Vector3(GridSpacing * (col - GridColOffset), -GridSpacing * (letterIndex - offset), 0);
			GameObject letterObject = Instantiate(PrefabLetter, pos, Quaternion.identity) as GameObject;
			InitLetterObject(col, letterIndex, letterObject);
			SetLetter(letter, letterObject);
			return letterObject;
		}

		private static void InitLetterObject(int col, int letterIndex, GameObject letterObject) {
			Letter letterProperties = letterObject.GetComponent<Letter>();
			letterProperties.Col = col;
			letterProperties.LetterIndex = letterIndex;
		}

		private static void SetLetter(char c, GameObject letterObject) {
			Transform transform = letterObject.transform.Find("letter");
			Text textObject = transform.GetComponent<Text>();
			textObject.text = "" + c;
		}

		private void GetSelectedWord(Transform letterObject) {
			Letter letterProperties = letterObject.GetComponent<Letter>();
			List<GameObject> word = letterProperties.GetMyWord();
			int wordIndex = WordObjects.IndexOf(word); // ! Potential problem if the same word is present more than once?
			SelectedWordString = "";
			List<Letter> selectedWordLetterList = new List<Letter>();
			for (int i = 0; i <= wordIndex; i++) {
				List<GameObject> wordObject = WordObjects[i];
				Letter selectedLetter = null;
				for (int j = 0; j < wordObject.Count; j++) {
					Letter lp = wordObject[j].GetComponent<Letter>();
					if (lp.GetIsInFocus()) {
						selectedLetter = lp;
						selectedWordLetterList.Add(lp);
						SelectedWordObjects.Add(wordObject[j]);
						break;
					}
				}
				Debug.Assert(selectedLetter != null, $"Word {WordStrings[i]} does not have a selected letter!");
				SelectedWordString += selectedLetter.GetLetter().ToString();
				selectedLetter.SetBaseColor((int)GameColors.BlockFocus);
			}

			if (DictionaryFull.SearchString(SelectedWordString) > 0) {
				for (int i = 0; i < selectedWordLetterList.Count; i++) {
					selectedWordLetterList[i].SetBaseColor((int)GameColors.BlockWord);
				}
				StartExplodeSelectedWords();
				gameState = GameState.WordFound;
			}
		}

		private void ClearColorOfFoundWord() {
			if (SelectedWordObjects.Count > 0)
				for (int i = 0; i < SelectedWordObjects.Count; i++)
					SelectedWordObjects[i].GetComponent<Letter>().SetBaseColor((int)GameColors.BlockFocus);
			SelectedWordObjects.Clear();
		}
	}
}