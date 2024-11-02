using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace GoMap {

	public class GOPlacesPrefab : MonoBehaviour {

		public IDictionary placeInfo; 
		public SpriteRenderer spriteRenderer;
		public GOPlaces goPlaces;

		Sprite texture;

		async void Start () {

			spriteRenderer = GetComponentInChildren<SpriteRenderer> ();
			spriteRenderer.sprite = null;
		
			string url = (string)placeInfo["icon"];
			await getTextureWithUrl (url);

		}


		private async UniTask DownloadIcon (string url) {

            var www = UnityWebRequestTexture.GetTexture(url);
            await www.SendWebRequest();

            texture = Sprite.Create (((DownloadHandlerTexture)www.downloadHandler).texture, new Rect (0, 0, 71, 71), new Vector2 (0.5f, 0.5f));
		}

		public async UniTask getTextureWithUrl (string url) {

			if (goPlaces.iconsCache.Contains(url)) {
				texture = (Sprite)goPlaces.iconsCache [url];
				spriteRenderer.sprite = texture;
				return;
			}

			await DownloadIcon (url);
			spriteRenderer.sprite = texture;
			if (!goPlaces.iconsCache.Contains (url)) {
				goPlaces.iconsCache.Add (url, texture);
			}

			return;
		}

	}
}