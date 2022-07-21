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
		private int[] RowOffsets;
		private int[] ColOffsets;

		public float GridSpacing = 1.1f;
		public byte GridColOffset = 4;

		private GameObject[][] LetterObjects;

		// touch drag properties
		private float touchDist;
		private bool touchDragging = false;
		private Vector3 touchOffset;
		private Transform touchObject;

		private bool platformIsMobile;

		void Start() {
			platformIsMobile = (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer);

			Dictionary = new DictionaryManager();
			Dictionary.Setup();

			if (!PrefabLetter) Debug.Log("A prefab letter has not been assigned!");
			
			LetterObjects = new GameObject[MaxCols][];
			RowOffsets = new int[MaxRows];
			ColOffsets = new int[MaxCols];
			FillGrid();
		}

		void Update() {
			InputHandler();
			
		}

		private void InputHandler() {
			if(Input.touchCount > 0) {
				TouchHandler();
			} else {
				MouseHander();
			}
		}

		private void TouchHandler() {
	    		Vector3 v3;

			Touch touch = Input.touches[0];
			
			Vector3 pos = touch.position;

			if (touch.phase == TouchPhase.Began) {
				Debug.Log("touch began");
				Ray ray = Camera.main.ScreenPointToRay(pos);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit)) {
					if (hit.collider.tag == "Letter") {
						touchObject = hit.transform;
						touchDist = hit.transform.position.z - Camera.main.transform.position.z;
						v3 = new Vector3(pos.x, pos.y, touchDist);
						v3 = Camera.main.ScreenToWorldPoint(v3);
						touchOffset = touchObject.position - v3;
						touchDragging = true;
					}
				}
			}

			if(touchDragging && touch.phase == TouchPhase.Moved) {
				v3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, touchDist);
				v3 = Camera.main.ScreenToWorldPoint(v3);
				touchObject.position = v3 + touchOffset;
			}

			if(touchDragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)) {
				touchDragging = false;
			}

		}

		private void MouseHander() {
			Vector3 v3;
			if (Input.GetMouseButton(0) && touchDragging == false) {
				Debug.Log("touch began (mouse)");
				Vector3 pos = Input.mousePosition;
				Ray ray = Camera.main.ScreenPointToRay(pos);
				RaycastHit hit;

				if(Physics.Raycast(ray, out hit)) {
					if(hit.collider.tag == "Letter") {
						touchObject = hit.transform;
						touchDist = hit.transform.position.z - Camera.main.transform.position.z;
						v3 = new Vector3(pos.x, pos.y, touchDist);
						v3 = Camera.main.ScreenToWorldPoint(v3);
						touchOffset = touchObject.position - v3;
						touchDragging = true;
					}
				}
			} else if(!Input.GetMouseButton(0) && touchDragging == true) {
				Debug.Log("touch ended (mouse)");
				touchDragging = false;
				return;
			}

			if(touchDragging) {
				v3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, touchDist);
				v3 = Camera.main.ScreenToWorldPoint(v3);
				touchObject.position = v3 + touchOffset;
			}
		}

		private void FillGrid() {
			for (int col = 0; col < MaxCols; col++) {
				string word = Dictionary.GetRandomWord();
				Debug.Log(word);
				LetterObjects[col] = new GameObject[word.Length];
				for (int letterIndex = 0; letterIndex < word.Length; letterIndex++) {
					int offset = word.Length / 2;
					char letter = char.ToUpper(word[letterIndex]);
					AddLetterObjectToScene(letter, col, letterIndex, offset);
				}
			}
		}

		private void AddLetterObjectToScene(char letter, int col, int letterIndex, int offset) {
			Vector3 pos = new Vector3(GridSpacing * (col - GridColOffset), -GridSpacing * (letterIndex - offset), 0);
			GameObject letterObject = Instantiate(PrefabLetter, pos, Quaternion.identity) as GameObject;
			InitLetterObject(col, letterIndex, letterObject);
			SetLetter(letter, letterObject);
			LetterObjects[col][letterIndex] = letterObject;
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