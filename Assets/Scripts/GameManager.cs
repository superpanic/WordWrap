using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WordWrap {

	public class GameManager : MonoBehaviour {

		public GameObject PrefabLetter;

		private DictionaryManager Dictionary;

		public byte MaxRows = 9;
		public byte MaxCols = 9;

		public float GridSpacing = 1.1f;
		public byte GridColOffset = 4;

		private GameObject[][]         LetterObjects;
		private List<List<GameObject>> WordObjects = new List<List<GameObject>>();
		private List<string>           WordStrings = new List<string>();

		// touch drag properties
		private float     TouchDist;
		private bool      TouchDragging = false;
		private Vector3   TouchOffset;
		private Transform TouchObject;

		void Start() {
			Dictionary = new DictionaryManager();
			Dictionary.Setup();

			if (!PrefabLetter) Debug.Log("A prefab letter has not been assigned!");
			
			LetterObjects = new GameObject[MaxCols][];
			AddWordsToScene();
		}

		void Update() {
			InputHandler();
		}

		private void InputHandler() {
			if(Input.touchCount > 0) {
				TouchHandler();
			} else {
				MouseClickHandler();
			}
		}

		private void TouchHandler() {
	    		Vector3 v3;

			Touch touch = Input.touches[0];
			
			Vector3 pos = touch.position;

			if (touch.phase == TouchPhase.Began) {
				Debug.Log("Touch began");
				Ray ray = Camera.main.ScreenPointToRay(pos);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit)) {
					if (hit.collider.tag == "Letter") {
						TouchObject = hit.transform;
						TouchDist = hit.transform.position.z - Camera.main.transform.position.z;
						v3 = new Vector3(pos.x, pos.y, TouchDist);
						v3 = Camera.main.ScreenToWorldPoint(v3);
						TouchOffset = TouchObject.position - v3;
						TouchDragging = true;
					}
				}
			}

			if(TouchDragging && touch.phase == TouchPhase.Moved) {
				v3 = new Vector3(pos.x, pos.y, TouchDist);
				v3 = Camera.main.ScreenToWorldPoint(v3);
				MoveWordToPos(v3+TouchOffset);
			}

			if(TouchDragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)) {
				Debug.Log("Touch ended");
				TouchDragging = false;
			}

		}

		private void MouseClickHandler() {
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

		private void MouseHander() {
			Vector3 v3;
			if (Input.GetMouseButton(0) && TouchDragging == false) {
				Debug.Log("Touch began (mouse)");
				Vector3 pos = Input.mousePosition;
				Ray ray = Camera.main.ScreenPointToRay(pos);
				RaycastHit hit;

				if(Physics.Raycast(ray, out hit)) {
					if(hit.collider.tag == "Letter") {
						TouchObject = hit.transform;
						TouchDist = hit.transform.position.z - Camera.main.transform.position.z;
						v3 = new Vector3(pos.x, pos.y, TouchDist);
						v3 = Camera.main.ScreenToWorldPoint(v3);
						TouchOffset = TouchObject.position - v3;
						TouchDragging = true;
					}
				}
			} else if(!Input.GetMouseButton(0) && TouchDragging) {
				Debug.Log("Touch ended (mouse)");
				TouchDragging = false;
				return;
			}

			if(TouchDragging) {
				v3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, TouchDist);
				v3 = Camera.main.ScreenToWorldPoint(v3);
				MoveWordToPos(v3+TouchOffset);
			}
		}

		private void MoveWordToCenterLetter(Transform letter) {
			Letter letterProperties = letter.GetComponent<Letter>();
			List<GameObject> myWord = letterProperties.GetMyWord();
			int wordLength = myWord.Count;
			int letterIndex = letterProperties.LetterIndex;
			float currentXPos = letter.transform.position.x;
			int offset = letterIndex;
			for (int i = 0; i < wordLength; i++) {
				Vector3 pos = new Vector3(currentXPos, -GridSpacing * (i - offset), 0);
				Letter currentLetterProperties = myWord[i].transform.GetComponent<Letter>();
				currentLetterProperties.SetDestination(pos);
			}
			letterProperties.SetInFocus();
		}

		private void MoveWordToPos(Vector3 p) {
			if(TouchDragging) {
				Letter letterProperties = TouchObject.GetComponent<Letter>();
				List<GameObject> activeWord = letterProperties.GetMyWord();
				float firstLetterOffset = (letterProperties.LetterIndex+1) * GridSpacing;
				p.y += firstLetterOffset;
				for(int i=0; i<activeWord.Count; i++) {
					p.y -= GridSpacing;
					activeWord[i].transform.position = p;
				}
			}
		}

		private void AddWordsToScene() {
			for (int col = 0; col < MaxCols; col++) {
				string word = Dictionary.GetRandomWord();
				Debug.Log($"Random word {col+1}: {word}");

				List<GameObject> MyWord = new List<GameObject>();
				int offset = word.Length / 2;

				for (int letterIndex = 0; letterIndex < word.Length; letterIndex++) {	
					char letter = char.ToUpper(word[letterIndex]);
					GameObject letterObject = AddLetterToScene(letter, col, letterIndex, offset);
					Letter letterProperties = letterObject.GetComponent<Letter>();
					letterProperties.SetMyWord(MyWord);
					if(letterIndex == word.Length/2) letterProperties.SetInFocus();
					MyWord.Add(letterObject);
				}

				WordObjects.Add(MyWord);
				WordStrings.Add(word);
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
			Letter LetterProperties = letterObject.GetComponent<Letter>();
			LetterProperties.Col = col;
			LetterProperties.LetterIndex = letterIndex;
		}

		private static void SetLetter(char c, GameObject letterObject) {
			Transform transform = letterObject.transform.Find("letter");
			Text textObject = transform.GetComponent<Text>();
			textObject.text = "" + c;
		}

	}
}