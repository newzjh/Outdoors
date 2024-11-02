using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using GoShared;

public class UIJoystick : BasePanel, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Action<PointerEventData> onClickDown;
    public Action<PointerEventData> onClickUp;
    public Action<PointerEventData> onDrag;
    public Action<PointerEventData> onClick;

    public Image imgDirBg;//大圆
    public Image imgDirPoint;//小圆
    private float pointDis;//当前屏幕下小球可以移动的最大距离
    private Vector2 startPos = Vector2.zero;//在按下时大圆的位置，即玩家触碰处

    private CameraPanel cp;
    private Track currenttrack;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        cp = GameObject.FindFirstObjectByType<CameraPanel>(FindObjectsInactive.Include);

        pointDis = Screen.height * 1.0f / 1440 * 96;//1440是我canvas分辨率的高度，90是小圆可以移动的最大距离
        RegisterTouchEvts();
    }

    // Update is called once per frame
    void Update()
    {
        if (currenttrack != null)
        {
            if (tg.PassedLength >= tg.LengthOfCurve)
            {
                //HideTrack();
            }
            else
            {
                Vector3 worldpos = tg.GetCurrentPosition();
                if (ma.goMap.useElevation)
                    worldpos = GoMap.GOMap.AltitudeToPoint(worldpos);
                worldpos.y += 1.0f;
                loc.moveTo(worldpos);
            }
        }
    }

    public void ShowTrack(Track track)
    {
        currenttrack = track;
        cp.Show();
    }

    public void HideTrack()
    {
        currenttrack = null;
        //cp.Hide();
    }

    [System.NonSerialized]
    public float speed = 0.125f;

    private void RegisterTouchEvts()
    {
        this.onClickDown = (PointerEventData evt) =>
        {
            startPos = evt.position;//此处evt.position是玩家按下的位置的屏幕坐标
            imgDirPoint.gameObject.SetActive(true);//显示小圆
        };
        this.onClickUp = (PointerEventData evt) =>
        {
            imgDirPoint.transform.localPosition = Vector2.zero;//小圆回到中心
            imgDirPoint.gameObject.SetActive(false);//隐藏小圆
        };

        this.onDrag = (PointerEventData evt) =>
        {
            Vector2 dir = evt.position - startPos;
            float len = dir.magnitude;//小圆此时移动的距离，向量相减求模长
            if (len > pointDis)
            {
                Vector2 clampDir = Vector2.ClampMagnitude(dir, 90);//返回dir，最大值为90
                imgDirPoint.transform.position = startPos + clampDir;//小圆无法超出其最大距离
            }
            else
            {
                imgDirPoint.transform.position = evt.position;//小圆在玩家按下的位置
            }

            HideTrack();
            loc.motionMode = LocationManagerEnums.MotionMode.Avatar;
            loc.move(dir.x * speed, dir.y * speed);

        };


    }

    public void OnPointerDown(PointerEventData eventData)
    {
        onClickDown?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onClickUp?.Invoke(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        onDrag?.Invoke(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(eventData);
    }
}
