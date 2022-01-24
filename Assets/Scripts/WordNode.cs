using System;
using System.Collections.Generic;

namespace WordWrap {
	// a trie data structure for a large wordlist
	// words need to be added in sorted order from a-z

	public class WordNode {
		private readonly bool IsRoot = false;
		public bool IsEndOfWord = false;
		public char Letter;
		public ushort weight = 0;
		public List<WordNode> NodeList;

		/* constructors */
		public WordNode(bool isRoot) {
			if (!isRoot) throw new ArgumentException("Root constructor can only have  true  as parameter!");
			IsRoot = isRoot;
			NodeList = new List<WordNode>();
		}

		public WordNode(string word) {
			if (word.Length == 0) throw new ArgumentException("Word string can't be empty!");
			NodeList = new List<WordNode>();
			Letter = word[0];
			AddWord(word);
		}

		/* methods */
		public void AddWord(string word) {
			if (word.Length == 0) throw new ArgumentException("Word string can't be empty!");

			if (!IsRoot) {
				if (word[0] != Letter) throw new ArgumentException("First char of word does not match node letter!");
				if (word.Length == 1) {
					IsEndOfWord = true;
					weight++;
					return;
				}
				word = word.Substring(1); // strip first char from word, it's already asigned to this nodes Letter (if not root)
			}
				
			char firstLetter = word[0];

			for (int i = 0; i < NodeList.Count; i++) {

				if (NodeList[i].Letter == firstLetter) { // any match with current char?
					NodeList[i].AddWord(word);
					weight++;
					return;
				}

				if (firstLetter < NodeList[i].Letter) {
					WordNode wn = new WordNode(word);
					NodeList.Insert(i, wn);
					weight++;
					return;
				}
					
			}

			// we reached the end of the node list (or node list is empty):
			WordNode w = new WordNode(word);
			NodeList.Add(w);
			weight++;
			return;
		}

		public int ReadWord(string word) { // returns word length if found, returns 0 if not found
			if (word.Length == 0) throw new ArgumentException("Word string can't be empty!");
			if (!IsRoot) {
				if (word[0] != Letter) throw new ArgumentException("First char of word does not match node letter!");
				if (word.Length == 1) {
					if (IsEndOfWord) return 1;
					else return 0;
				}
				word = word.Substring(1);
			}
			char firstLetter = word[0];
			for (int i = 0; i < NodeList.Count; i++) {
				if (NodeList[i].Letter == firstLetter) {
					int result = NodeList[i].ReadWord(word);
					if (result > 0) {
						if (IsRoot) return result;
						else return result + 1;
					} else return 0;
				}
			}
			return 0;
		}

	}
}
