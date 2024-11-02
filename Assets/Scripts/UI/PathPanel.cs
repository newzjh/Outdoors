using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoShared;
using Cysharp.Threading.Tasks;

public class PathPanel : BasePanel
{
    public Sprite milestonesprite;
    public Font defaultfont;
    private List<Vector3> milestoneWorldpos = new List<Vector3>();
    private List<GameObject> milestoneButtons = new List<GameObject>();
    private RectTransform rt;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        rt = GetComponent<RectTransform>();
    }

    public class MileStoneButton : MonoBehaviour
    {
        public int buttonindex = -1;
        public float location = 0.0f;
    }

    public async UniTask ExitTrack()
    {
        List<GameObject> deletelist = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
            deletelist.Add(transform.GetChild(i).gameObject);
        foreach (GameObject go in deletelist)
            GameObject.Destroy(go);

        milestoneButtons.Clear();
        milestoneWorldpos.Clear();
    }

    public async UniTask ShowTrack(Track track)
    {
        float milestonedelta = 500.0f;

        List<GameObject> deletelist = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
            deletelist.Add(transform.GetChild(i).gameObject);
        foreach (GameObject go in deletelist)
            GameObject.Destroy(go);

        milestoneButtons.Clear();
        milestoneWorldpos.Clear();

        await UniTask.NextFrame();

        int milestonenum = Mathf.CeilToInt(tg.LengthOfCurve / milestonedelta);

        var tasks = new List<UniTask<PlaceInfo>>();

        for (int v = 0; v < milestonenum; v++)
        {
            float location = v * milestonedelta;
            if (v == milestonenum - 1)
                location = tg.LengthOfCurve;

            Vector3 worldpos = tg.GetPositionFromPathByLocation(location, 0.0f);
            tasks.Add(GetInfoByWorldPos(worldpos));
        }
        var results = await UniTask.WhenAll(tasks);

        if (this == null || !Application.isPlaying)
            return;

        for (int v = 0; v < milestonenum; v++)
        {
            float location = v * milestonedelta;
            if (v == milestonenum - 1)
                location = tg.LengthOfCurve;
            
            Vector3 worldpos = tg.GetPositionFromPathByLocation(location, 0.0f);
            if (ma.goMap.useElevation)
                worldpos = GoMap.GOMap.AltitudeToPoint(worldpos);
            worldpos.y += 1.0f;

            Vector3 screenpos = Camera.main.WorldToScreenPoint(worldpos);
            Vector2 localpt = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenpos, null, out localpt);
            GameObject goReg = new GameObject();
            goReg.name = "milestoneButton" + v;
            goReg.transform.parent = transform;
            goReg.transform.localPosition = localpt;
            goReg.transform.localScale = Vector3.one;
            Image img = goReg.AddComponent<Image>();
            img.sprite = milestonesprite;
            Button b = goReg.AddComponent<Button>();
            b.onClick.AddListener(
                () =>
                {
                    OnMileStoneClick(goReg);
                }
            );
            var r = goReg.AddComponent<MileStoneButton>();
            r.buttonindex = v;
            r.location = location;
            milestoneButtons.Add(goReg);
            milestoneWorldpos.Add(worldpos);

            {
                GameObject goText = new GameObject();
                goText.name = "milestoneText1:" + v.ToString();
                goText.transform.parent = goReg.transform;
                goText.transform.localPosition = Vector3.zero;
                goText.transform.localScale = Vector3.one;
                Text text = goText.AddComponent<Text>();
                text.font = defaultfont;
                text.text = location.ToString() + " mile";
                text.color = new Color(0, 0, 0.5f);
                text.fontSize = 24;
                text.fontStyle = FontStyle.Italic;
                text.alignment = TextAnchor.LowerCenter;
                RectTransform rtText = goText.GetComponent<RectTransform>();
                rtText.sizeDelta = new Vector2(128+64, 36);
                rtText.anchoredPosition = Vector2.right * 104 + Vector2.up * 16;
            }

            {
                GameObject goText = new GameObject();
                goText.name = "milestoneText2:" + v.ToString();
                goText.transform.parent = goReg.transform;
                goText.transform.localPosition = Vector3.zero;
                goText.transform.localScale = Vector3.one;
                Text text = goText.AddComponent<Text>();
                text.font = defaultfont;
                text.text = results[v].text;
                text.color = new Color(0.5f, 0, 0.5f);
                text.fontSize = 24;
                text.fontStyle = FontStyle.Italic;
                text.alignment = TextAnchor.LowerCenter;
                RectTransform rtText = goText.GetComponent<RectTransform>();
                rtText.sizeDelta = new Vector2(128+64, 36);
                rtText.anchoredPosition = Vector2.right * 120 - Vector2.up * 16;
            }
        }
    }

    void OnMileStoneClick(GameObject go)
    {
        var r = go.GetComponent<MileStoneButton>();
        if (!r)
            return;

        //if (!contextrect)
        //    return;

        //if (!regpanel2)
        //    return;
    }

    private Vector3 lastcampos = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
        Camera cam = Camera.main;
        if (!cam)
            return;

        bool changed = false;
        float dis = Vector3.Distance(cam.transform.position, lastcampos);
        if (dis > 0.001f)
        {
            lastcampos = cam.transform.position;
            changed = true;
        }
        if (!changed)
            return;

        for (int i = 0; i < milestoneButtons.Count; i++)
        {
            Vector3 worldpos = milestoneWorldpos[i];
            if (ma.goMap.useElevation)
                worldpos = GoMap.GOMap.AltitudeToPoint(worldpos);
            worldpos.y += 1.0f;

            Vector3 screenpos = cam.WorldToScreenPoint(worldpos);
            if (screenpos.z < 0)
            {
                Vector2 localpt = Vector2.zero;
                localpt.y += Screen.height;
                milestoneButtons[i].transform.localPosition = localpt;
            }
            else
            {
                Vector2 localpt = Vector2.zero;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenpos, null, out localpt);
                milestoneButtons[i].transform.localPosition = localpt;
            }
        }
    }

    void OnMilestoneClick()
    {
        Debug.Log("OnMilestoneClick");
    }
}
