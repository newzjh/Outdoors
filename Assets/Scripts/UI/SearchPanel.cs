using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoShared;
using MiniJSON;
using Cysharp.Threading.Tasks;
using NUnit.Framework.Internal;
using TMPro;

public class SearchPanel : BasePanel
{

    private TrackPanel trackpanel = null;
    private Transform tTipsPanel = null;
    private InputField inputfield = null;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        trackpanel = GameObject.FindFirstObjectByType<TrackPanel>(FindObjectsInactive.Include);

        var dd = GetComponentInChildren<Dropdown>();
        dd.options.Clear();
        for(int i=0;i<= (int)LocationManagerEnums.DemoLocation.HongKong;i++)
        {
            LocationManagerEnums.DemoLocation e = (LocationManagerEnums.DemoLocation)i;
            Dropdown.OptionData od = new Dropdown.OptionData();
            od.text = e.ToString();
            dd.options.Add(od);
        }

        tTipsPanel = transform.Find("SearchTips");

        inputfield = transform.GetComponentInChildren<InputField>();
    }

    public void OnSelectLocation()
    {
        var dd = GetComponentInChildren<Dropdown>();
        LocationManagerEnums.DemoLocation e = (LocationManagerEnums.DemoLocation)dd.value;
        Coordinates coord = LocationManagerEnums.LocationEnums.GetCoordinates(e);
        SearchByCoord(coord.toLongLatString());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private Queue<string> tasks = new Queue<string>();

    public async void OnInputKeywordsChanged()
    {
        if (tTipsPanel == null)
            return;

        tTipsPanel.gameObject.SetActive(false);

        string keywords = inputfield.text;
        if (string.IsNullOrEmpty(keywords))
            return;

        string cityname = await GetCurrentCityName();
        string locstr = loc.currentLocation.toLongLatString();

        string url = "https://restapi.amap.com/v3/assistant/inputtips?output=xml&city=" + cityname + "&keywords=" + keywords + "&location=" + locstr + "&output=JSON&key=e9b198eacdefaa58075429316cd7ad12";
        tasks.Enqueue(url);
        string str2 = await GetTextFromURL(url); ;
        var top = tasks.Dequeue();
        if (top != url)
            return;

        List<string> tipnames = new List<string>();
        List<string> tiplocations = new List<string>();
        if (str2 != null && str2.Length > 0)
        {
            var dict = Json.Deserialize(str2) as Dictionary<string, object>;

            if (dict!=null && dict.ContainsKey("tips"))
            {
                var list = dict["tips"] as List<object>;

                if (list.Count > 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var subdict = list[i] as Dictionary<string, object>;
                        if (subdict!=null && subdict.ContainsKey("name") && subdict.ContainsKey("location"))
                        {
                            string tipname = subdict["name"] as string;
                            string tiplocation = subdict["location"] as string;
                            if (!string.IsNullOrEmpty(tipname) && !string.IsNullOrEmpty(tiplocation))
                            {
                                tipnames.Add(tipname);
                                tiplocations.Add(tiplocation);
                            }
                        }
                    }
                }
            }
        }

        tTipsPanel.gameObject.SetActive(true);
        ScrollRect sr = tTipsPanel.GetComponentInChildren<ScrollRect>();
        if (sr == null)
            return;

        Transform tFirst = sr.content.GetChild(0);

        List<GameObject> deletelist = new List<GameObject>();
        for(int i=1;i< sr.content.childCount;i++)
        {
            deletelist.Add(sr.content.GetChild(i).gameObject);
        }
        foreach(var go in deletelist)
        {
            GameObject.DestroyImmediate(go);
        }
        tFirst.gameObject.SetActive(false);

        for(int i=1;i<tipnames.Count;i++)
        {
            GameObject go = GameObject.Instantiate(tFirst.gameObject);
            go.transform.parent = tFirst.parent;
            go.transform.localScale = Vector3.one;
        }

        if (tipnames.Count > 0)
        {
            for (int i = 0; i < sr.content.childCount; i++)
            {
                Transform tchild = sr.content.GetChild(i);
                tchild.gameObject.SetActive(true);
                var tf = tchild.GetComponentInChildren<TextMeshProUGUI>();
                tf.text = tipnames[i];
                var b = tchild.GetComponentInChildren<Button>();
                b.name = tiplocations[i];
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(delegate
                {
                    SearchByPath(loc.currentLocation, b.name);
                });
            }
        }
    }

    public async void SearchByCoord(string coordtext)
    {
        await trackpanel.OnExitTrack();

        string str = await GetTextFromURL("https://api.maptiler.com/geocoding/" + coordtext + ".json?key=d6qPzaurptRX6n3oM6eX");

        if (str != null && str.Length > 0)
        {
            var dict = Json.Deserialize(str) as Dictionary<string, object>;

            if (dict.ContainsKey("features"))
            {
                var list = dict["features"] as List<object>;

                if (list.Count > 0)
                {
                    var subdict = list[0] as Dictionary<string, object>;
                    if (subdict.ContainsKey("center"))
                    {
                        var cc = subdict["center"] as List<object>;
                        if (cc.Count == 2)
                        {
                            double dlong = 0;
                            double dlat = 0;
                            bool ret1 = double.TryParse(cc[0].ToString(), out dlong);
                            bool ret2 = double.TryParse(cc[1].ToString(), out dlat);
                            if (ret1 && ret2 && !(dlong == 0 && dlat == 0))
                            {
                                Coordinates coord = new Coordinates(dlat, dlong);
                                loc.motionMode = LocationManagerEnums.MotionMode.Avatar;
                                loc.SetLocation(coord);
                            }
                        }
                    }
                }

            }

        }

    }

    public async void SearchByPath(Coordinates src, string dest)
    {
        await trackpanel.OnExitTrack();

        string cityname = await GetCurrentCityName();

        List<Track> tracklist = new List<Track>();

        string str = await GetTextFromURL("https://restapi.amap.com/v5/direction/walking?isindoor=0&origin=" + src.toLongLatString()+"&destination="+dest+ "&show_fields=polyline&key=e9b198eacdefaa58075429316cd7ad12");

        if (str != null && str.Length > 0)
        {
            var dict = Json.Deserialize(str) as Dictionary<string, object>;

            if (dict.ContainsKey("route"))
            {
                var subdict = dict["route"] as Dictionary<string, object>;
                if (subdict.ContainsKey("paths"))
                {
                    var pathlist = subdict["paths"] as List<object>;

                    int trackcount = 0;

                    foreach(var path in pathlist)
                    {
                        Track track = new Track();
                        track.name = "track" + trackcount.ToString();

                        var subdict2 = path as Dictionary<string, object>;
                        if (subdict2.ContainsKey("steps"))
                        {
                            var steplist = subdict2["steps"] as List<object>;
                            foreach(var _step in steplist)
                            {
                                var step = _step as Dictionary<string, object>;
                                if (step.ContainsKey("polyline"))
                                {
                                    string polyline = step["polyline"] as string;
                                    if (polyline.Contains(';'))
                                    {
                                        string[] coords = polyline.Split(';');
                                        if (coords!=null && coords.Length > 0)
                                        {
                                            int startindex = 0;
                                            if (track.points.Count > 0)
                                                startindex = 1;
                                            for (int i = startindex; i < coords.Length; i++)
                                            {
                                                double dlong, dlat;
                                                string[] splitString = coords[i].Split(',');
                                                double.TryParse(splitString[0], out dlong);
                                                double.TryParse(splitString[1], out dlat);
                                                Coordinates c = new Coordinates(dlat, dlong);
                                                track.points.Add(c);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        tracklist.Add(track);
                        trackcount++;
                    }
                }

            }

        }

        trackpanel.LoadTracks(tracklist);
    }

    public void OnSearch()
    {
        var inputfield = GetComponentInChildren<InputField>();
        if (inputfield!=null)
        {
            SearchByPath(loc.currentLocation,inputfield.text);
        }
    }
}
