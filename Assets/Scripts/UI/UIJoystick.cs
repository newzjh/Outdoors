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

    public Image imgDirBg;//��Բ
    public Image imgDirPoint;//СԲ
    private float pointDis;//��ǰ��Ļ��С������ƶ���������
    private Vector2 startPos = Vector2.zero;//�ڰ���ʱ��Բ��λ�ã�����Ҵ�����

    private CameraPanel cp;
    private Track currenttrack;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        cp = GameObject.FindFirstObjectByType<CameraPanel>(FindObjectsInactive.Include);

        pointDis = Screen.height * 1.0f / 1440 * 96;//1440����canvas�ֱ��ʵĸ߶ȣ�90��СԲ�����ƶ���������
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
            startPos = evt.position;//�˴�evt.position����Ұ��µ�λ�õ���Ļ����
            imgDirPoint.gameObject.SetActive(true);//��ʾСԲ
        };
        this.onClickUp = (PointerEventData evt) =>
        {
            imgDirPoint.transform.localPosition = Vector2.zero;//СԲ�ص�����
            imgDirPoint.gameObject.SetActive(false);//����СԲ
        };

        this.onDrag = (PointerEventData evt) =>
        {
            Vector2 dir = evt.position - startPos;
            float len = dir.magnitude;//СԲ��ʱ�ƶ��ľ��룬���������ģ��
            if (len > pointDis)
            {
                Vector2 clampDir = Vector2.ClampMagnitude(dir, 90);//����dir�����ֵΪ90
                imgDirPoint.transform.position = startPos + clampDir;//СԲ�޷�������������
            }
            else
            {
                imgDirPoint.transform.position = evt.position;//СԲ����Ұ��µ�λ��
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
