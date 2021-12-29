using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WordWrap {

	public class GameManager : MonoBehaviour {

		public GameObject PrefabLetter;
		public byte MaxRows = 9;
		public byte RowOffset = 4;
		public float GridSpacing = 1.1f;

		private GameObject[][] LetterObjects;

		void Start() {
			if (!PrefabLetter) Debug.Log("A prefab letter has not been assigned!");
			LetterObjects = new GameObject[MaxRows][];
			FillGrid();
		}

		private void FillGrid() {
			for (int x = 0; x < MaxRows; x++) {
				string word = Common.GetRandomWord();
				LetterObjects[x] = new GameObject[word.Length];
				for (int y = 0; y < word.Length; y++) {
					int offset = word.Length / 2;
					char letter = char.ToUpper(word[y]);
					AddLetterObjectToScene(letter, x, y, offset);
				}
			}
		}

		private void AddLetterObjectToScene(char letter, int x, int y, int offset) {
			Vector3 pos = new Vector3(GridSpacing * (x - RowOffset), -GridSpacing * (y - offset), 0);
			GameObject letterObject = Instantiate(PrefabLetter, pos, Quaternion.identity) as GameObject;
			InitLetterObject(x, y, letterObject);
			SetLetter(letter, letterObject);
			LetterObjects[x][y] = letterObject;
		}

		private static void InitLetterObject(int x, int y, GameObject letterObject) {
			Letter LetterProperties = letterObject.GetComponent<Letter>();
			LetterProperties.Row = x;
			LetterProperties.CharNum = y;
		}
		private static void SetLetter(char c, GameObject letterObject) {
			Transform transform = letterObject.transform.Find("letter");
			Text textObject = transform.GetComponent<Text>();
			textObject.text = "" + c;
		} 

	}
}