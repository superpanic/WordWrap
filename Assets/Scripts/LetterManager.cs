using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WordWrap {

	public class LetterManager : MonoBehaviour {

		public GameObject PrefabLetter;
		public byte Rows = 9;
		public byte WordLength = 8;
		public float GridSpacing = 1.1f;
		public byte RowOffset = 4;

		private GameObject[][] LetterObjects;

		void Start() {
			if (!PrefabLetter) Debug.Log("A prefab letter has not been assigned!");
			LetterObjects = new GameObject[Rows][];
			FillGrid();
		}

		private void FillGrid() {
			for (int x = 0; x < Rows; x++) {
				string word = Common.GetRandomWord();
				LetterObjects[x] = new GameObject[word.Length];
				for (int y = 0; y < word.Length; y++) {
					int offset = word.Length / 2;
					char letter = char.ToUpper(word[y]);
					PlaceLetter(letter, x, y, offset);
				}
			}
		}

		private void PlaceLetter(char letter, int x, int y, int offset) {
			Vector3 pos = new Vector3(GridSpacing * (x - RowOffset), -GridSpacing * (y - offset), 0);
			GameObject letterObject = Instantiate(PrefabLetter, pos, Quaternion.identity) as GameObject;
			SetLetter(letter, letterObject);
			LetterObjects[x][y] = letterObject;
		}

		private static void SetLetter(char c, GameObject letterObject) {
			Transform transform = letterObject.transform.Find("letter");
			Text textObject = transform.GetComponent<Text>();
			textObject.text = "" + c;
		}

		void Update() { }

	}
}