using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using GoShared;
using System.IO;
using Unity.Mathematics;






#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CameraPanel))]
class CameraPanelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CameraPanel com = (CameraPanel)target;
        
        if (GUILayout.Button("Open Streetmap Cache Folder"))
        {
            EditorUtility.RevealInFinder(Application.temporaryCachePath);
        }
    }
}
#endif

public class CameraPanel : BasePanel, IDragHandler
{
    private CubeCamera cubecam;
    private Transform tpanel2;

    private Action<PointerEventData> onDrag;


    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        cubecam = GameObject.FindFirstObjectByType<CubeCamera>(FindObjectsInactive.Include);

        tpanel2 = transform.Find("CameraPanel2");

        if (!usecubemap)
        {
            cubeshader = Shader.Find("Skybox/Blend 6 Sided");
            cubemat = new Material(cubeshader);
        }
        else
        {
            cubeshader2 = Shader.Find("Skybox/Blend Cubemap Obj");
            cubeshader = Shader.Find("Skybox/Blend Cubemap");
            cubemat = new Material(cubeshader);
        }

        RegisterTouchEvts();

        loc.onLocationChanged.AddListener(OnLocationChanged);

        OnScreenSizeChanged();
    }

    private void OnDestroy()
    {
        loc.onLocationChanged.RemoveListener(OnLocationChanged);

        if (camrt != null)
            RenderTexture.Destroy(camrt);

        if (curviewcube != null)
            Cubemap.Destroy(curviewcube);

        if (lastviewcube != null)
            Cubemap.Destroy(lastviewcube);

        for(int i=0;i<6;i++)
        {
            if (lastviewtexs[i] != null)
                Texture2D.Destroy(lastviewtexs[i]);
            if (curviewtexs[i] != null)
                Texture2D.Destroy(curviewtexs[i]);
        }
    }

    private int sw = 0;
    private int sh = 0;
    private RenderTexture camrt;
    public void OnScreenSizeChanged()
    {
        int canvaswidth = 1440 * Screen.width / Screen.height;

        GetComponent<RectTransform>().sizeDelta = new Vector2(canvaswidth, 480);

        int panelwidth = canvaswidth * 5 / 6;
        tpanel2.GetComponent<RectTransform>().sizeDelta = new Vector2(panelwidth, 400);

        if (camrt != null)
            RenderTexture.Destroy(camrt);
        if (usecubemap)
        {
            camrt = new RenderTexture(panelwidth, 800, usecubemap ? 1 : 0, RenderTextureFormat.RGB111110Float, 0);
            camrt.name = "cubecamera_rt_" + panelwidth + "x" + 800;
            cubecam.GetComponent<Camera>().fieldOfView = 90.0f;
        }
        else
        {
            camrt = new RenderTexture(panelwidth, 400, usecubemap ? 1 : 0, RenderTextureFormat.RGB111110Float, 0);
            camrt.name = "cubecamera_rt_" + panelwidth + "x" + 400;
        }
        tpanel2.GetComponentInChildren<RawImage>().texture = camrt;
        cubecam.GetComponent<Camera>().targetTexture = camrt;
    }

    private bool usereplaceshader = true;
    private bool dirty = false;
    // Update is called once per frame
    void Update()
    {
        if (Screen.width != sw || Screen.height != sh)
        {
            OnScreenSizeChanged();
            sw = Screen.width;
            sh = Screen.height;
        }

        blend += Time.deltaTime * 0.5f;
        blend = Mathf.Clamp01(blend);
        if (usecubemap)
        {
            Shader.SetGlobalFloat("_GlobalCubemapBlend", blend);
            Shader.SetGlobalFloat("_GlobalCubemapRotation", 180.0f);
        }
        else
        {
            cubemat.SetFloat("_Blend", blend);
        }

        Material oldskybox = RenderSettings.skybox;
        RenderSettings.skybox = cubemat;
        Camera cam = cubecam.GetComponent<Camera>();
        if (!usecubemap)
        {
            if (usereplaceshader)
                cam.clearFlags = CameraClearFlags.Skybox;
            else
                cam.clearFlags = CameraClearFlags.SolidColor;
        }
        if (usecubemap || !usereplaceshader)
            cam.cullingMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("GOTerrain"));
        else
            cam.cullingMask = 0;
        if (usecubemap && usereplaceshader)
            cam.RenderWithShader(cubeshader2, "");
        else
            cam.Render();
        RenderSettings.skybox = oldskybox;
    }

    public void OnSwitchStyle()
    {
        usereplaceshader = !usereplaceshader;
    }

    private void RegisterTouchEvts()
    {


        this.onDrag = (PointerEventData evt) =>
        {
            cubecam.AngleOffset.y += evt.delta.x * 0.25f;
            cubecam.AngleOffset.x -= evt.delta.y * 0.25f;
            cubecam.AngleOffset.x = Mathf.Clamp(cubecam.AngleOffset.x, -90.0f, 90.0f);
            while (cubecam.AngleOffset.y > 360.0f)
                cubecam.AngleOffset.y -= 360.0f;
            while (cubecam.AngleOffset.y < -360.0f)
                cubecam.AngleOffset.y += 360.0f;
        };
    }


    public void OnDrag(PointerEventData eventData)
    {
        onDrag?.Invoke(eventData);
    }

    public const bool usecubemap = false;
    private Texture2D[] curviewtexs = new Texture2D[6];
    private Texture2D[] lastviewtexs = new Texture2D[6];
    private Cubemap curviewcube = null;
    private Cubemap lastviewcube = null;
    private string cachepath = string.Empty;
    private Shader cubeshader;
    private Shader cubeshader2;
    private Material cubemat;
    private float blend = 1.0f;

    public void OnToggle()
    {
        if (tpanel2!=null)
        {
            tpanel2.gameObject.SetActive(!tpanel2.gameObject.activeSelf);
        }
    }

    public void Show()
    {
        if (tpanel2 != null)
        {
            tpanel2.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (tpanel2 != null)
        {
            tpanel2.gameObject.SetActive(false);
        }
    }

    private float capturedelta = 0.0001f;
    private Coordinates oldcoord=new Coordinates(0,0);
    public void OnLocationChanged(Coordinates coord)
    {
        double2 delta;
        delta.y = coord.latitude - oldcoord.latitude;
        delta.x = coord.longitude - oldcoord.longitude;
        double deltalen = Math.Sqrt(delta.x * delta.x + delta.y * delta.y);
        if (deltalen > capturedelta)
        {
            oldcoord = coord;
            OnCapture();
        }
    }

    private int lastcapturingframe = -1;

    async void OnCapture()
    {
        if (lastcapturingframe!=-1 && Time.frameCount - lastcapturingframe < 30)
            return;

        int curcapturingframe = Time.frameCount;

        string[] views = new string[6];
        views[0] = "&heading=0&pitch=0";
        views[1] = "&heading=90&pitch=0";
        views[2] = "&heading=180&pitch=0";
        views[3] = "&heading=270&pitch=0";
        views[4] = "&heading=0&pitch=90";
        views[5] = "&heading=0&pitch=-90";

        string sbase = "https://maps.googleapis.com/maps/api/streetview?size=640x640&location=";
        string slonlat = loc.currentLocation.latitude + "," + loc.currentLocation.longitude;
        string key = "&key=AIzaSyBG7PfYmnki4wroPKo-7C4nArd089X8Zag";
        string url = sbase + slonlat + key;

        
        cachepath = Application.temporaryCachePath+"/";
        cachepath+=string.Format("{0:0.0000}_{1:0.0000}", loc.currentLocation.latitude, loc.currentLocation.longitude);

        blend = 1.0f;
        if (!usecubemap)
        {
            cubemat.SetTexture("_FrontTex1", Texture2D.grayTexture);
            cubemat.SetTexture("_RightTex1", Texture2D.grayTexture);
            cubemat.SetTexture("_BackTex1", Texture2D.grayTexture);
            cubemat.SetTexture("_LeftTex1", Texture2D.grayTexture);
            cubemat.SetTexture("_UpTex1", Texture2D.grayTexture);
            cubemat.SetTexture("_DownTex1", Texture2D.grayTexture);
            for (int i = 0; i < 6; i++)
            {
                if (lastviewtexs[i] != null)
                    Texture2D.Destroy(lastviewtexs[i]);
                lastviewtexs[i] = null;
            }
        }
        else
        {
            cubemat.SetTexture("_Tex1", null);
            if (lastviewcube!=null)
                Cubemap.Destroy(lastviewcube);
            lastviewcube = null;
        }

        var tasks = new List<UniTask<Texture2D>>();
        for (int i = 0; i < 6; i++)
        {
            tasks.Add(CaptureTexture(url + views[i], cachepath + "_" + i.ToString() + ".jpg", i));
        }
        var results = await UniTask.WhenAll(tasks);

        await UniTask.SwitchToMainThread();

        bool finish = true;
        for (int i = 0; i < 6; i++)
        {
            if (results[i]==null)
            {
                finish = false;
                break;
            }
        }

        //晚开始的先完成了，只能废弃当前的
        bool useless = false;
        if (lastcapturingframe != -1 && lastcapturingframe > curcapturingframe)
            useless = true;
        

        if (finish && !useless)
        {
            if (!usecubemap)
            {
                for (int i = 0; i < 6; i++)
                {
                    lastviewtexs[i] = curviewtexs[i];
                    curviewtexs[i] = results[i];
                }

                if (cubemat != null)
                {
                    cubemat.SetTexture("_FrontTex1", lastviewtexs[0] != null ? lastviewtexs[0] : curviewtexs[0]);
                    cubemat.SetTexture("_RightTex1", lastviewtexs[3] != null ? lastviewtexs[3] : curviewtexs[3]);
                    cubemat.SetTexture("_BackTex1", lastviewtexs[2] != null ? lastviewtexs[2] : curviewtexs[2]);
                    cubemat.SetTexture("_LeftTex1", lastviewtexs[1] != null ? lastviewtexs[1] : curviewtexs[1]);
                    cubemat.SetTexture("_UpTex1", lastviewtexs[4] != null ? lastviewtexs[4] : curviewtexs[4]);
                    cubemat.SetTexture("_DownTex1", lastviewtexs[5] != null ? lastviewtexs[5] : curviewtexs[5]);

                    cubemat.SetTexture("_FrontTex2", curviewtexs[0]);
                    cubemat.SetTexture("_RightTex2", curviewtexs[3]);
                    cubemat.SetTexture("_BackTex2", curviewtexs[2]);
                    cubemat.SetTexture("_LeftTex2", curviewtexs[1]);
                    cubemat.SetTexture("_UpTex2", curviewtexs[4]);
                    cubemat.SetTexture("_DownTex2", curviewtexs[5]);
                }
            }
            else
            {
                lastviewcube = curviewcube;
                curviewcube = new Cubemap(640, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None, 0);
                if (results[0] != null)
                    curviewcube.SetPixels(results[0].GetPixels(), CubemapFace.PositiveZ);
                if (results[3] != null)
                    curviewcube.SetPixels(results[3].GetPixels(), CubemapFace.NegativeX);
                if (results[2] != null)
                    curviewcube.SetPixels(results[2].GetPixels(), CubemapFace.NegativeZ);
                if (results[1] != null)
                    curviewcube.SetPixels(results[1].GetPixels(), CubemapFace.PositiveX);
                if (results[5] != null)
                    curviewcube.SetPixels(results[5].GetPixels(), CubemapFace.PositiveY);
                if (results[4] != null)
                    curviewcube.SetPixels(results[4].GetPixels(), CubemapFace.NegativeY);
                curviewcube.Apply();
                for (int i = 0; i < 6; i++)
                    if (results[i] != null)
                        Texture2D.Destroy(results[i]);

                if (cubemat != null)
                {
                    //cubemat.SetTexture("_Tex1", lastviewcube != null ? lastviewcube : curviewcube);
                    //cubemat.SetTexture("_Tex2", curviewcube);
                    Shader.SetGlobalTexture("_GlobalCubemap1", lastviewcube != null ? lastviewcube : curviewcube);
                    Shader.SetGlobalTexture("_GlobalCubemap2", curviewcube);
                }
            }

            blend = 0.0f;
            //cubecam.AngleOffset = new Vector3(15, 0, 0);

            lastcapturingframe = curcapturingframe;
        }

        else
        {
            if (Application.isPlaying)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (results[i] != null)
                        Texture2D.Destroy(results[i]);
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    if (results[i] != null)
                        Texture2D.DestroyImmediate(results[i]);
                }
            }
        }

        
    }

    private async UniTask<byte[]> Capture(string url,string filepath, int index)
    {
        byte[] data = null;
        if (System.IO.File.Exists(filepath))
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(filepath);
            if (fs.Length < 1024 * 1024)
            {
                data = new byte[fs.Length];
                await fs.ReadAsync(data, 0, (int)(fs.Length));
            }
            fs.Close();

            return data;
        }
        else
        {
            data = await GetDataFromURL(url);
            if (data != null)
            {
                await System.IO.File.WriteAllBytesAsync(filepath, data);
            }

            return data;
        }
        
    }

    private async UniTask<Texture2D> CaptureTexture(string url, string filepath, int index)
    {
        byte[] data = null;
        if (System.IO.File.Exists(filepath))
        {
            Texture2D tex = await GetTextureFromURL("file://"+ filepath, false, 120);
            return tex;
        }
        else
        {
            if (CanReachOversea)
            {
                Texture2D tex = await GetTextureFromURL(url, false, 120);
                if (tex != null)
                {
                    data = tex.EncodeToJPG();
                    if (data != null)
                    {
                        await System.IO.File.WriteAllBytesAsync(filepath, data);
                    }
                }

                return tex;
            }
            else
            {
                Texture2D tex = new Texture2D(256, 256);
                return tex;
            }
        }

    }
}
