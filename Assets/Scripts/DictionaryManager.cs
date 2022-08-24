using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

namespace WordWrap {
	class DictionaryManager {

		private WordNode RootNode;
		private System.Random Rnd;
		private int LowLimit = 2;
		private int HighLimit = 7;

		public DictionaryManager(string path, int high, int low) {
			LowLimit = low;
			HighLimit = high;
			CreateWordTree(path, lowLimit:LowLimit, highLimit:HighLimit);
		}

		public void CreateWordTree(string sortedWordFilePath, int lowLimit, int highLimit) {
			RootNode = new WordNode(isRoot:true);
			
			long milliseconds;
			if (File.Exists(sortedWordFilePath)) { /// make sure "copy to output directory" is set for file ///
				using (StreamReader file = new StreamReader(sortedWordFilePath)) {
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
				throw new Exception($"word list: {sortedWordFilePath}, is missing");
			}
		}

		public int SearchString(string s) {
			s = s.ToLower();
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
			Debug.Assert(RootNode!=null, "Failed to get random word: Node tree not created!");
			if(Rnd==null) Rnd = new System.Random();

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
