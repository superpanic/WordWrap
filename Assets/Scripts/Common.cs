using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WordWrap {
	public class Common {

		public static string GetRandomWord() {
			string[] words = new string[10];
			words[0] = "ape";
			words[1] = "apple";
			words[2] = "banana";
			words[3] = "band";
			words[4] = "bandana";
			words[5] = "car";
			words[6] = "cart";
			words[7] = "care";
			words[8] = "donkey";
			words[9] = "dragons";

			return words[Random.Range(0,10)];
		}

	}
}