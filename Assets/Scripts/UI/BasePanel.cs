using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using GoShared;
using MiniJSON;

public class BasePanel : MonoBehaviour
{

    protected LocationManager loc;
    protected MoveAvatar ma;
    protected TrackGroup tg;

    protected virtual void Awake()
    {
        loc = GameObject.FindFirstObjectByType<LocationManager>(FindObjectsInactive.Include);
        ma = GameObject.FindFirstObjectByType<MoveAvatar>(FindObjectsInactive.Include);
        tg = GameObject.FindFirstObjectByType<TrackGroup>(FindObjectsInactive.Include);
    }

    public class WebRequestSkipCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    public static bool CanReachOversea = false;

    protected async UniTask<Texture2D> GetTextureFromURL(string url, bool skipcertificate = false, int timeout = 120)
    {
        var www = UnityWebRequestTexture.GetTexture(url);
        //www.timeout = 30;
        if (skipcertificate)
            www.certificateHandler = new WebRequestSkipCertificate();

        Texture2D tex = null;

        try
        {
            var task = www.SendWebRequest().ToUniTask();
            if (www.downloadedBytes > 0 || www.result == UnityWebRequest.Result.InProgress || www.result == UnityWebRequest.Result.Success)
                await task;
            else
                await task.TimeoutWithoutException(TimeSpan.FromSeconds(timeout));
        }
        catch (Exception e)
        {
            //Debug.Log("request " + url + " exception :" + e);
        }

        if (www.result != UnityWebRequest.Result.ConnectionError && www.downloadHandler.isDone)
        {
            Debug.Log("[BaseUrlRequest] is successd for: " + url);
            tex = (www.downloadHandler as DownloadHandlerTexture).texture;
        }
        else if (!string.IsNullOrEmpty(www.error) && (www.error.Contains("429") || www.error.Contains("timed out")))
        {
            Debug.Log("[BaseUrlRequest] data reload : " + www.error + " " + url);
            await UniTask.WaitForSeconds(1);
            return await GetTextureFromURL(url);
        }
        else
        {
            Debug.Log("[BaseUrlRequest] data missing :" + www.error + " " + url);
        }

        www.Abort();

        return tex;
    }

    protected async UniTask<byte[]> GetDataFromURL(string url,bool skipcertificate=false, int timeout = 120)
    {
        var www = UnityWebRequest.Get(url);
        //www.timeout = 30;
        if (skipcertificate)
            www.certificateHandler = new WebRequestSkipCertificate();

        try
        {
            var task = www.SendWebRequest().ToUniTask();
            if (www.downloadedBytes > 0 || www.result == UnityWebRequest.Result.InProgress || www.result == UnityWebRequest.Result.Success)
                await task;
            else
                await task.TimeoutWithoutException(TimeSpan.FromSeconds(timeout));
        }
        catch (Exception e)
        {
            //Debug.Log("request " + url + " exception :" + e);
        }

        if (www.result != UnityWebRequest.Result.ConnectionError && www.downloadHandler.isDone && www.downloadHandler.data.Length > 0)
        {
            Debug.Log("[BaseUrlRequest] is successd for: " + url);
        }
        else if (!string.IsNullOrEmpty(www.error) && (www.error.Contains("429") || www.error.Contains("timed out")))
        {
            Debug.Log("[BaseUrlRequest] data reload :" + www.error + " " + url);
            await UniTask.WaitForSeconds(1);
            return await GetDataFromURL(url);
        }
        else
        {
            Debug.Log("[BaseUrlRequest] data missing :" + www.error + " " + url);
        }

        byte[] data = www.downloadHandler.data;

        www.Abort();

        return data;
    }

    protected async UniTask<string> GetTextFromURL(string url, bool skipcertificate = false, int timeout = 120)
    {
        var www = UnityWebRequest.Get(url);
        //www.timeout = 30;
        if (skipcertificate)
            www.certificateHandler = new WebRequestSkipCertificate();

        try
        {
            var task = www.SendWebRequest().ToUniTask();
            if (www.downloadedBytes > 0 || www.result == UnityWebRequest.Result.InProgress || www.result == UnityWebRequest.Result.Success)
                await task; 
            else
                await task.TimeoutWithoutException(TimeSpan.FromSeconds(timeout)); 
        }
        catch (Exception e)
        {
            //Debug.Log("request " + url + " exception :" + e);
        }

        if (www.result != UnityWebRequest.Result.ConnectionError && www.downloadHandler.isDone && www.downloadHandler.data.Length > 0)
        {
            Debug.Log("[BaseUrlRequest] is successd for: " + url);
        }
        else if (!string.IsNullOrEmpty(www.error) && (www.error.Contains("429") || www.error.Contains("timed out")))
        {
            Debug.Log("[BaseUrlRequest] data reload :" + www.error + " " + url);
            await UniTask.WaitForSeconds(1);
            return await GetTextFromURL(url);
        }
        else
        {
            Debug.Log("[BaseUrlRequest] data missing :" + www.error + " " + url);
        }

        byte[] data = www.downloadHandler.data;

        string text = string.Empty;
        if (data!=null)
        { 
            MemoryStream ms = new MemoryStream(data);
            StreamReader sr = new StreamReader(ms);
            text = await sr.ReadToEndAsync();
            sr.Dispose();
            ms.Dispose();
        }

        www.Abort();

        return text;
    }

    public class PlaceInfo
    {
        public string country = string.Empty;
        public string province = string.Empty;
        public string city = string.Empty;
        public string id = string.Empty;
        public string text = string.Empty;
        public Coordinates center;
        public Bounds bounds = new Bounds();
    }

    protected async UniTask<PlaceInfo> GetInfoByWorldPos(Vector3 worldpos)
    {
        Coordinates coord = Coordinates.convertVectorToCoordinates(worldpos);
        return await GetInfoByCoord(coord);
    }

    protected async UniTask<PlaceInfo> GetInfoByCoord(Coordinates coord)
    { 
        string str = await GetTextFromURL("https://api.maptiler.com/geocoding/" + coord.longitude + "," + coord.latitude + ".json?key=d6qPzaurptRX6n3oM6eX");

        if (str != null && str.Length > 0)
        {
            var dict = Json.Deserialize(str) as Dictionary<string, object>;

            if (dict.ContainsKey("features"))
            {
                var list = dict["features"] as List<object>;

                if (list.Count > 0)
                {
                    PlaceInfo info = new PlaceInfo();

                    if (list.Count >= 1)
                    {
                        var subdict = list[0] as Dictionary<string, object>;

                        if (subdict.ContainsKey("bbox"))
                        {
                            var cc = subdict["bbox"] as List<object>;
                            if (cc.Count == 4)
                            {
                                Vector2 vmin = Vector2.zero;
                                Vector2 vmax = Vector2.zero;
                                bool ret1 = float.TryParse(cc[0].ToString(), out vmin.x);
                                bool ret2 = float.TryParse(cc[1].ToString(), out vmin.y);
                                bool ret3 = float.TryParse(cc[2].ToString(), out vmax.x);
                                bool ret4 = float.TryParse(cc[3].ToString(), out vmax.y);
                                info.bounds.SetMinMax(vmin, vmax);
                            }
                        }
                        if (subdict.ContainsKey("center"))
                        {
                            var cc = subdict["center"] as List<object>;
                            if (cc.Count == 2)
                            {
                                double dlong = 0;
                                double dlat = 0;
                                bool ret1 = double.TryParse(cc[0].ToString(), out dlong);
                                bool ret2 = double.TryParse(cc[1].ToString(), out dlat);
                                info.center = new Coordinates(dlat, dlong);
                            }
                        }
                        if (subdict.ContainsKey("text"))
                        {
                            object obj = subdict["text"];
                            info.text = obj.ToString();
                        }
                        if (subdict.ContainsKey("id"))
                        {
                            object obj = subdict["id"];
                            info.id = obj.ToString();
                        }
                    }
                    if (list.Count>=1)
                    {
                        var subdict = list[list.Count-1] as Dictionary<string, object>;
                        if (subdict.ContainsKey("text"))
                        {
                            object obj = subdict["text"];
                            info.country = obj.ToString();
                        }
                    }
                    if (list.Count >= 2)
                    {
                        var subdict = list[list.Count - 2] as Dictionary<string, object>;
                        if (subdict.ContainsKey("text"))
                        {
                            object obj = subdict["text"];
                            info.province = obj.ToString();
                        }
                    }
                    if (list.Count >= 3)
                    {
                        var subdict = list[list.Count - 3] as Dictionary<string, object>;
                        if (subdict.ContainsKey("text"))
                        {
                            object obj = subdict["text"];
                            info.city = obj.ToString();
                        }
                    }

                    return info;
                }

            }
        }

        return null;
    }
    public async UniTask<string> GetCurrentCityName()
    {
        PlaceInfo info = await GetInfoByCoord(loc.currentLocation);
        if (info!=null && !(string.IsNullOrEmpty(info.province)) && !(string.IsNullOrEmpty(info.city)))
        {
            if (info.province.Contains("Hong Kong"))
                info.city = info.province;
            return info.city;
        }
        return string.Empty;
    }
}
