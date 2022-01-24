using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

namespace WordWrap {
	class DictionaryManager {

		private WordNode RootNode;
		//private string Path = "Assets/Dictionaries/common_words_eu_com.txt";
		private string Path = "Assets/Dictionaries/sorted_words.txt";
		private System.Random Rnd;

		public void Setup() {
			Rnd = new System.Random();
			CreateWordTree(Path, lowLimit:2, highLimit:7);
		}

		public void CreateWordTree(string sortedWordFileName, int lowLimit, int highLimit) {

			RootNode = new WordNode(isRoot:true);
			
			long milliseconds;
			if (File.Exists(sortedWordFileName)) { /// make sure "copy to output directory" is set for file ///
				using (StreamReader file = new StreamReader(sortedWordFileName)) {
					int counter = 0;
					string ln;

					milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

					while ((ln = file.ReadLine()) != null) {
						if (ln.Length >= lowLimit && ln.Length <= highLimit) {
							RootNode.AddWord(ln);
							counter++;
						}
					}

					milliseconds = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - milliseconds;

					file.Close();
					Debug.Log($"Read {counter} words from text file in {milliseconds} milliseconds.");
				}
			} else {
				throw new Exception($"word list: {sortedWordFileName}, is missing");
			}
		}

		public int SearchString(string s) {
			if (RootNode == null) return 0;
			return RootNode.ReadWord(s); // 0 == not found
		}

		public void PrintNRandomWords(int n) {
			if (RootNode == null) return;
			string[] wordArr = new string[n];
			for (int i = 0; i < n; i++) {
				string randomWord = GetRandomWord();
				wordArr[i] = randomWord;
			}
			Array.Sort(wordArr, StringComparer.InvariantCulture);
			for (int i = 0; i < n; i++) {
				Debug.Log(wordArr[i]);
			}
		}

		public string GetRandomWord() {
			string word = "";

			WordNode currentNode = RootNode;

			// do this until we find a node that is end of a word and does not have any child nodes			
			while (!(currentNode.IsEndOfWord && currentNode.NodeList.Count == 0)) {
				int randomValue;
				int weightSum = 0;

				// calculate weight sum
				for (int i = 0; i < currentNode.NodeList.Count; i++) {
					weightSum += currentNode.NodeList[i].weight;
				}

				randomValue = Rnd.Next(0, weightSum + 1);

				if (currentNode.IsEndOfWord)
					if (randomValue == 0) return word;

				// find node to step down into
				int step = 0;
				for (int i = 0; i < currentNode.NodeList.Count; i++) {
					if (randomValue <= currentNode.NodeList[i].weight + step) {
						currentNode = currentNode.NodeList[i];
						word += currentNode.Letter;
						break;
					} else {
						step += currentNode.NodeList[i].weight;
					}
				}
			}
			return word;
		}
	}
}
