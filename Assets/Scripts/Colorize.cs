using UnityEngine;
using UnityEngine.UI;

namespace WordWrap {	
	[ExecuteInEditMode]
	public class Colorize : MonoBehaviour {
		public Palette palette;

		public int colorIndex;
		public float intensity = 1;

		public bool recursive;

		void Apply(GameObject go) {
			Color color = Color.Lerp(palette.colors[colorIndex], Color.white, 1 - intensity);

			Graphic graphic = go.GetComponent<Graphic>();
			if (graphic) {
				graphic.color = color;
			}

			Text text = go.GetComponent<Text>();
			if (text) {
				text.color = color;
			}

			Camera camera = go.GetComponent<Camera>();
			if (camera) {
				camera.backgroundColor = color;
			}

			if (recursive) {
				foreach (Transform child in go.transform)
					Apply(child.gameObject);
			}
		}

		void Update() {
			if (!palette)
				return;

			colorIndex = Mathf.Clamp(colorIndex, 0, palette.colors.Length - 1);

			Apply(gameObject);
		}
	}
}