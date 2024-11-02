using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine.Networking;
using GoMap;
using Cysharp.Threading.Tasks;

namespace GoShared {

	public static class GOUrlRequest {

		public static bool verboseLogging = true;

		public static async UniTask testRequest (string url, System.Action done) {

            var www = UnityWebRequest.Get(url);
            await www.SendWebRequest();

			if(string.IsNullOrEmpty(www.error) && www.downloadHandler.isDone) {
				Debug.Log("Request Success: " + url);
				done ();
			}else{
				Debug.LogWarning ("Request Failed: " + url + " :" + www.error);
				done ();
			}
		}

		public static async UniTask<string> GetTextFromData(byte[] data)
        {
			MemoryStream ms = new MemoryStream(data);
			StreamReader sr = new StreamReader(ms);
			string text = await sr.ReadToEndAsync();
			sr.Dispose();
			ms.Dispose();
			return text;
		}

		public static async UniTask getRequest(string url, bool useCache, string filename ,Action <Texture2D, byte[],string,string> response)
		{
            bool exist = FileHandler.Exist(filename);
            if (useCache && exist)
            {
                url = "file://" + filename;
            }

			var www = UnityWebRequest.Get(url);

            try
            {
                await www.SendWebRequest();
            }
            catch (Exception e)
            {
                //Debug.Log(e);
            }

            if (www.result != UnityWebRequest.Result.ConnectionError && www.downloadHandler.isDone && www.downloadHandler.data.Length > 0)
			{
				Debug.Log("[GOUrlRequest] is success for : " + url);
				if (useCache && !exist)
					FileHandler.Save(filename, www.downloadHandler.data);
			}
			else if (!string.IsNullOrEmpty(www.error) && (www.error.Contains("429") || www.error.Contains("timed out")))
			{
				Debug.Log("[GOUrlRequest] data reload :" + www.error + " " + url);
				await UniTask.WaitForSeconds(1);
				await getRequest(url, useCache, filename, response);
				return;
			}
			else
			{
				Debug.Log("[GOUrlRequest] data missing :" + www.error + " " + url);
				response(null, null, null, www.error);
				return;
			}

			string text = await GetTextFromData(www.downloadHandler.data);
			response(null, www.downloadHandler.data, text, www.error);


			return;
		}

        public static async UniTask getTextureRequest(string url, bool useCache, string filename, Action<Texture2D, byte[], string, string> response)
        {
            bool exist = FileHandler.Exist(filename);
            if (useCache && exist)
            {
                url = "file://" + filename;
            }

			UnityWebRequest www = null;
            if (url.Contains(".webp"))
				www = UnityWebRequest.Get(url);
			else
                www = UnityWebRequestTexture.GetTexture(url);

            try
            {
                await www.SendWebRequest();
            }
            catch (Exception e)
            {
                //Debug.Log(e);
            }

            if (www.result != UnityWebRequest.Result.ConnectionError && www.downloadHandler.isDone)
			{
				Debug.Log("[GOUrlRequest] is success for: " + url);
				if (useCache && !exist)
					FileHandler.Save(filename, www.downloadHandler.data);
			}
			else if (!string.IsNullOrEmpty(www.error) && (www.error.Contains("429") || www.error.Contains("timed out")))
			{
				Debug.Log("[GOUrlRequest] data reload :" + www.error + " : " + url);
				await UniTask.WaitForSeconds(1);
                await getTextureRequest(url, useCache, filename, response);
				return;
			}
			else
			{
				Debug.Log("[GOUrlRequest] data missing :" + www.error + " : " + url);
				response(null, null, null, www.error);
				return;
			}

			Texture2D tex = null;
            string errormsg = www.error;
            if (url.Contains(".webp"))
			{
				if (www.downloadHandler.data != null && www.downloadHandler.data.Length > 0)
				{
					WebP.Error lError;
					tex = WebP.Texture2DExt.CreateTexture2DFromWebP(www.downloadHandler.data, false, false, out lError, null, false);
					if (tex != null)
						errormsg = string.Empty;
				}
			}
			else
			{
                tex = (www.downloadHandler as DownloadHandlerTexture).texture;
            }

			response(tex, null, null, errormsg);

            return;
        }


        public static async UniTask jsonRequest(string url, bool useCache ,string filename ,Action <Dictionary<string,object>,string> response)
		{

			ParseJob job = new ParseJob();


			if (useCache && FileHandler.Exist(filename))
			{
				job.InData = FileHandler.LoadText (filename);
				job.Start();
				await job.WaitFor();
				response((Dictionary<string,object>)job.OutData,null);
			}
			else
			{

                var www = UnityWebRequest.Get(url);

                try
				{
                    await www.SendWebRequest();
                }
                catch (Exception e)
                {
                    //Debug.Log(e);
                }

                if (string.IsNullOrEmpty(www.error) && www.downloadHandler.isDone && www.downloadHandler.data.Length > 0)
                {
                    Debug.Log("[GOUrlRequest] is success for : " + url);
                    if (useCache)
                        FileHandler.Save(filename, www.downloadHandler.data);
                }
                else if (!string.IsNullOrEmpty(www.error) && (www.error.Contains("429") || www.error.Contains("timed out")))
                {
                    Debug.Log("[GOUrlRequest] data reload :" + www.error + " : " + url);
                    await UniTask.WaitForSeconds(1);
                    await jsonRequest(url, useCache, filename, response);
                    return;
                }
                else
                {
                    Debug.Log("[GOUrlRequest] data missing :" + www.error + " : " + url);
                    response(null, www.error);
                    return;
                }

                job.InData = await GetTextFromData(www.downloadHandler.data); //FileHandler.LoadText (filename);
                job.Start();
                await job.WaitFor();
                response((Dictionary<string, object>)job.OutData, null);


			}


			return;
		}
	}
}