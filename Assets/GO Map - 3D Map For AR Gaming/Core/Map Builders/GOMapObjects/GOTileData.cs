using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GoShared;
using Cysharp.Threading.Tasks;

namespace GoMap {

	public class GOTileData  {

		public enum GODataType  {

			VectorPBF,
			VectorJson,
			DEM,
			Normals,
			Texture,
			Satellite,
			Satellite4X,
		}

		public string filename;
		public string url;
		public bool useCache = true;
		public GODataType type;

		[HideInInspector] public byte [] data;
		[HideInInspector] public IDictionary jsonData;
		[HideInInspector] public GODEMTexture2D textureData;
		[HideInInspector] public GOTileObj goTile;
		[HideInInspector] public Vector2 tileCoords;

		public GOTileData (string url, GOTileObj tileObj, GODataType type, Vector2? tileCoords = null) {

			this.url = url;
			this.goTile = tileObj;
			this.useCache = tileObj.useCache;
			this.filename = string.Format("[{0}][{1}]{2}",tileObj.mapType.ToString (),type.ToString(),tileObj.name);
			if (tileCoords != null)
				this.filename = string.Format("[{0}][{1}]{2} - {3}",tileObj.mapType.ToString (),type.ToString(),tileObj.name,tileCoords);

			this.type = type;

		}

		~GOTileData()
        {
			OnDestroy();
		}

		public async void OnDestroy()
        {
			await UniTask.SwitchToMainThread();

			if (textureData != null && textureData.texture != null)
			{
				if (Application.isPlaying)
					Texture2D.Destroy(textureData.texture);
				else
					Texture2D.DestroyImmediate(textureData.texture);
				textureData.texture = null;
			}
		}

		public async UniTask Download(bool IsTexture, Action <Texture2D,byte[],string,string> action) {

            if (IsTexture)
			{
                await GOUrlRequest.getTextureRequest(url, useCache, filename, action);
			}
			else
			{
				await GOUrlRequest.getRequest(url, useCache, filename, action);
            }
		}

		public void prepareData (Texture2D tex, byte[] data) {

            if (tex!=null)
			{
				textureData = new GODEMTexture2D(tex, goTile.tileSize, goTile.elevationType, goTile.elevationMultiplier);
				this.data = tex.GetRawTextureData();
				return;
            }

			this.data = data;

            switch (type) {
			case GODataType.DEM:
			case GODataType.Normals:
			case GODataType.Texture: 
			case GODataType.Satellite: 
			case GODataType.Satellite4X: 
				textureData = new GODEMTexture2D (data, goTile.tileSize, goTile.elevationType, goTile.elevationMultiplier);
				break;
			}

		}
			
		public static Texture2D MergeSatellite2X (List<GOTileData> data) {

			Texture2D[] textures = new Texture2D[data.Count];
			for (int i = 0; i < textures.Length; i++) {
				textures [i] = data [i].textureData.ToTexture2D();
			}
			Texture2D texture2D = ImageHelpers.JoinTextures (textures);


			return texture2D;
		}

	}
}
