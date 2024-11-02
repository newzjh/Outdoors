using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using GoShared;
using MiniJSON;
using System.Linq;
using LocationManagerEnums;

public class TrackPanel : BasePanel
{
    private Button btoogle;
    private Transform tPanel2;
    private Slider progressbar;
    private Text progresstext;

    private UIJoystick uijoystick;
    private PathPanel pathpanel;
    private RunningText messagetext;

    public Font fonttemplate;
    public Sprite buttonsprite;
    public Sprite menusprite;
    public Sprite stopsprite;
    public Sprite playsprite;
    public Sprite pauseprite;
    public Sprite speedupsprite;
    public Sprite slowdownsprite;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        uijoystick = GameObject.FindFirstObjectByType<UIJoystick>(FindObjectsInactive.Include);
        pathpanel = GameObject.FindFirstObjectByType<PathPanel>(FindObjectsInactive.Include);
        messagetext = gameObject.GetComponentInChildren<RunningText>(true);

        btoogle = transform.Find("ButtonToggle").GetComponent<Button>();
        btoogle.onClick.AddListener(
            () =>
            {
                OnToggle();
            }
        );

        tPanel2 = transform.Find("TrackPanel2");
        progressbar = tPanel2.GetComponentInChildren<Slider>(true);
        progressbar.gameObject.SetActive(false);
        progresstext = progressbar.GetComponentInChildren<Text>(true);

        ma.centerTileChanged = OnRefreshTracks;

        RefreshTracksImp(false);

        Application.targetFrameRate = 5;
    }

    private bool progressbarsetting = false;
    // Update is called once per frame
    void Update()
    {
        if (progressbar.gameObject.activeSelf)
        {
            progressbarsetting = true;
            progressbar.value = Mathf.Clamp01(tg.PassedLength/tg.LengthOfCurve);
            progresstext.text = tg.PassedLength + "/" + tg.LengthOfCurve;
            progressbarsetting = false;
        }
    }

    public void OnToggle()
    {
        tPanel2.gameObject.SetActive(!tPanel2.gameObject.activeSelf);
    }

    private async UniTask<byte[]> GetGPXData()
    {
        if (ma.currentTileCenter == null)
            return null;

        var baseUrl = "https://api.openstreetmap.org/api/0.6/trackpoints?bbox=";

        //Download vector data
        Vector3 realPos = ma.currentTileCenter.convertCoordinateToVector();
        Coordinates minCoord = ma.currentTileCenter.tileOrigin(ma.goMap.zoomLevel);
        Vector3 minPos = minCoord.convertCoordinateToVector();
        Vector3 maxPos = minPos + (realPos - minPos) * 2;
        Coordinates maxCoord = Coordinates.convertVectorToCoordinates(maxPos);
        if (minCoord.latitude>maxCoord.latitude)
        {
            double t = maxCoord.latitude;
            maxCoord.latitude = minCoord.latitude;
            minCoord.latitude = t;
        }

        var tileurl = minCoord.longitude + "," + minCoord.latitude + "," + maxCoord.longitude + "," + maxCoord.latitude;

        var completeUrl = baseUrl + tileurl;

        var ret = await GetDataFromURL(completeUrl,true);

        return ret;
    }

    private async UniTask<string> GetInfoForTrack(Track track)
    {
        double clat = (track.points[0].latitude + track.points[track.points.Count-1].latitude)*0.5f;
        double clong = (track.points[0].longitude + track.points[track.points.Count - 1].longitude) * 0.5f;
        Coordinates coord = new Coordinates(clat, clong);
        
        PlaceInfo info = await GetInfoByCoord(coord);
        if (info != null)
            return info.text;
        else
            return string.Empty;
    }



    private List<Track> aviltracks;

    private async void OnRefreshTracks()
    {
        await RefreshTracksImp(true);
    }

    private async UniTask RefreshTracksImp(bool withgpx = true)
    {
        messagetext.gameObject.SetActive(false);

        ScrollRect srect = GetComponentInChildren<ScrollRect>(true);
        List<GameObject> deletelist = new List<GameObject>();
        for (int i = 0; i < srect.content.childCount; i++)
            deletelist.Add(srect.content.GetChild(i).gameObject);
        foreach (GameObject go in deletelist)
            GameObject.Destroy(go);

        string cityname = await GetCurrentCityName();


        await UniTask.NextFrame();

        List<Track> newtracks = new List<Track>();
        if (cityname.Contains("Hong Kong"))
        {
            TextAsset ta = Resources.Load<TextAsset>("GPXs/1983578.gpx");
            if (ta != null)
            {
                string str = ta.text;
                if (str != null && str.Length > 0)
                {
                    var tracks = GpxParser.ParseGPXContent(str);
                    if (tracks != null)
                        newtracks.Add(tracks[0]);
                }
            }
        }
        else if (cityname.Contains("π„÷› –"))
        {
            TextAsset ta = Resources.Load<TextAsset>("GPXs/1358760.gpx");
            if (ta != null)
            {
                string str = ta.text;
                if (str != null && str.Length > 0)
                {
                    var tracks = GpxParser.ParseGPXContent(str);
                    if (tracks != null)
                        newtracks.Add(tracks[0]);
                }
            }
        }

        await UniTask.NextFrame();

        if (this == null)
            return;

        if (withgpx && CanReachOversea)
        {
            messagetext.gameObject.SetActive(true);
            byte[] gpx = await GetGPXData();
            if (this == null)
                return;

            if (gpx != null && gpx.Length > 0)
            {
                MemoryStream ms = new MemoryStream(gpx);
                var tracks = GpxParser.ParseGPXContent(ms);
                ms.Dispose();
                if (tracks != null)
                {
                    foreach (var track in tracks)
                    {
                        if (track.points.Count > 4)
                        {
                            newtracks.Add(track);
                        }
                    }
                }
            }
            messagetext.gameObject.SetActive(false);
        }

        for (int i=0;i< newtracks.Count;i++)
        {
            string text = await GetInfoForTrack(newtracks[i]);
            newtracks[i].name = text;
        }

        await UniTask.SwitchToMainThread();

        aviltracks = newtracks;

        if (this!=null)
            RefreshTrackUIObjects();
    }

    public void LoadTracks(List<Track> newtracks)
    {
        aviltracks = newtracks;

        if (this != null)
            RefreshTrackUIObjects();
    }

    private GameObject currenttrackgo = null;

    public void OnProgressBarSetLocation()
    {
        if (progressbarsetting)
            return;


        tg.PassedLength = tg.LengthOfCurve * progressbar.value;
    }

    private void HideCurrentTrackUI()
    {
        if (currenttrackgo != null)
            GameObject.Destroy(currenttrackgo);
        currenttrackgo = null;
        progressbar.gameObject.SetActive(false);
    }

    private void ShowCurrentTrackUI(Track track)
    {
        if (currenttrackgo != null)
            GameObject.Destroy(currenttrackgo);
        currenttrackgo = null;

        progressbar.gameObject.SetActive(true);

        {
            GameObject go = new GameObject();
            go.transform.parent = tPanel2;
            var img = go.AddComponent<Image>();
            img.sprite = buttonsprite;
            Color col = Color.white;
            col.a = 0.3f;
            img.color = col;
            img.type = Image.Type.Sliced;
            img.fillCenter = true;
            img.pixelsPerUnitMultiplier = 0.3f;
            var b = go.AddComponent<Button>();
            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(250, 100);
            rt.anchoredPosition = new Vector2(-10, 310);

            GameObject subgo = new GameObject();
            subgo.transform.parent = go.transform;
            var text = subgo.AddComponent<Text>();
            text.fontSize = 24;
            text.font = fonttemplate;
            if (string.IsNullOrEmpty(track.name))
                text.text = "current track";
            else
                text.text = track.name;
            text.alignment = TextAnchor.UpperRight;
            text.color = Color.black;
            var subrt = subgo.GetComponent<RectTransform>();
            subrt.localScale = Vector3.one;
            subrt.anchoredPosition = Vector3.zero;
            subrt.sizeDelta = new Vector2(220,80);

            GameObject subgo2 = new GameObject();
            subgo2.transform.parent = go.transform;
            var img2 = subgo2.AddComponent<Image>();
            img2.sprite = stopsprite;
            var subrt2 = subgo2.GetComponent<RectTransform>();
            subrt2.localScale = Vector3.one;
            subrt2.anchoredPosition = -Vector2.right * 96 - Vector2.up * 20;
            subrt2.sizeDelta = Vector2.one * 48;
            var subb2 = subgo2.gameObject.AddComponent<Button>();

            GameObject subgo3 = new GameObject();
            subgo3.transform.parent = go.transform;
            var img3 = subgo3.AddComponent<Image>();
            img3.sprite = playsprite;
            var subrt3 = subgo3.GetComponent<RectTransform>();
            subrt3.localScale = Vector3.one;
            subrt3.anchoredPosition = -Vector2.right * 48 - Vector2.up * 20;
            subrt3.sizeDelta = Vector2.one * 48;
            var subb3 = subgo3.gameObject.AddComponent<Button>();

            GameObject subgo4 = new GameObject();
            subgo4.transform.parent = go.transform;
            var img4 = subgo4.AddComponent<Image>();
            img4.sprite = slowdownsprite;
            var subrt4 = subgo4.GetComponent<RectTransform>();
            subrt4.localScale = Vector3.one;
            subrt4.anchoredPosition = Vector2.right * 0 - Vector2.up * 20;
            subrt4.sizeDelta = Vector2.one * 48;
            var subb4 = subgo4.gameObject.AddComponent<Button>();

            GameObject subgo5 = new GameObject();
            subgo5.transform.parent = go.transform;
            var img5 = subgo5.AddComponent<Image>();
            img5.sprite = pauseprite;
            var subrt5 = subgo5.GetComponent<RectTransform>();
            subrt5.localScale = Vector3.one;
            subrt5.anchoredPosition = Vector2.right * 48 - Vector2.up * 20;
            subrt5.sizeDelta = Vector2.one * 48;
            var subb5 = subgo5.gameObject.AddComponent<Button>();

            GameObject subgo6 = new GameObject();
            subgo6.transform.parent = go.transform;
            var img6 = subgo6.AddComponent<Image>();
            img6.sprite = speedupsprite;
            var subrt6 = subgo6.GetComponent<RectTransform>();
            subrt6.localScale = Vector3.one;
            subrt6.anchoredPosition = Vector2.right * 96 - Vector2.up * 20;
            subrt6.sizeDelta = Vector2.one * 48;
            var subb6 = subgo6.gameObject.AddComponent<Button>();

            subb2.onClick.AddListener(
                () =>
                {
                    OnExitTrack();
                }
            );

            subb3.onClick.AddListener(
                 () =>
                 {
                     OnResumeTrack();
                 }
             );

            subb4.onClick.AddListener(
                 () =>
                 {
                     OnTrackDecreaseSpeed();
                 }
             );

            subb5.onClick.AddListener(
                 () =>
                 {
                     OnPauseTrack();
                 }
             );

            subb6.onClick.AddListener(
                 () =>
                 {
                     OnTrackIncreaseSpeed();
                 }
             );

            currenttrackgo = go;
        }
    }

    private void RefreshTrackUIObjects()
    {
        ScrollRect srect = GetComponentInChildren<ScrollRect>(true);
        List<GameObject> deletelist = new List<GameObject>();
        for (int i = 0; i < srect.content.childCount; i++)
            deletelist.Add(srect.content.GetChild(i).gameObject);
        foreach (GameObject go in deletelist)
            GameObject.Destroy(go);

        for (int i = 0; i < aviltracks.Count; i++)
        {
            GameObject go = new GameObject();
            go.name = i.ToString();
            go.transform.parent = srect.content;
            var img = go.AddComponent<Image>();
            img.sprite = buttonsprite;
            Color col = Color.white;
            col.r = UnityEngine.Random.Range(0.2f, 0.8f);
            col.g = UnityEngine.Random.Range(0.2f, 0.8f);
            col.b = UnityEngine.Random.Range(0.2f, 0.8f);
            col.a = 192.0f / 255.0f;
            img.color = col;
            img.type = Image.Type.Sliced;
            img.fillCenter = true;
            img.pixelsPerUnitMultiplier = 0.3f;
            var b = go.AddComponent<Button>();
            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(250, 100);
            rt.anchoredPosition = new Vector2(0, 670 - i * 100);

            GameObject subgo = new GameObject();
            subgo.transform.parent = go.transform;
            var text = subgo.AddComponent<Text>();
            text.fontSize = 24;
            text.font = fonttemplate;
            if (string.IsNullOrEmpty(aviltracks[i].name))
                text.text = "track" + i.ToString() + " ";
            else
                text.text = aviltracks[i].name;
            text.alignment = TextAnchor.UpperRight;
            text.color = Color.black;
            var subrt = subgo.GetComponent<RectTransform>();
            subrt.localScale = Vector3.one;
            subrt.anchoredPosition = Vector3.zero;
            subrt.sizeDelta = new Vector2(240, 80);

            b.onClick.AddListener(
                () =>
                {
                    int index = int.Parse(b.name);
                    OnPlayTrack(index);
                }
            );
        }
    }

    public async UniTask OnExitTrack()
    {
        await pathpanel.ExitTrack();
        uijoystick.HideTrack();
        HideCurrentTrackUI();
        await UniTask.NextFrame();
        Application.targetFrameRate = 5;
    }

    public void OnPlayTrack(int index)
    {
        if (aviltracks!=null && index>=0 && index< aviltracks.Count)
        {
            tg.LoadTracks(aviltracks);
            tg.ShowTrack(index);
            if (aviltracks.Count > 0)
            {
                pathpanel.ShowTrack(aviltracks[index]);
                uijoystick.ShowTrack(aviltracks[index]);
                ShowCurrentTrackUI(aviltracks[index]);
            }
        }
        Application.targetFrameRate = 30;
    }

    public void OnResumeTrack()
    {
        tg.PassedSpeed = 10.0f;
    }

    public void OnPauseTrack()
    {
        tg.PassedSpeed = 0.0f;
    }

    public void OnTrackDecreaseSpeed()
    {
        tg.PassedSpeed *= 0.5f;
        tg.PassedSpeed = Mathf.Clamp(tg.PassedSpeed, -200.0f, 200.0f);
    }

    public void OnTrackIncreaseSpeed()
    {
        tg.PassedSpeed *= 2.0f;
        tg.PassedSpeed = Mathf.Clamp(tg.PassedSpeed, -200.0f, 200.0f);
    }
}
