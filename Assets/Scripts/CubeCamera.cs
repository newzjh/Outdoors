using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCamera : MonoBehaviour
{
    public Transform eyes;
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        Application.targetFrameRate = 30;
        curq = Quaternion.LookRotation(eyes.forward);
    }

    [System.NonSerialized]
    public Vector3 AngleOffset = new Vector3(15, 0, 0);
    [System.NonSerialized]
    public Vector3 PostionOffset = new Vector3(0, 15, 0);
    [System.NonSerialized]
    public float rotatespeed = 10.0f;
    private Quaternion curq;

    // Update is called once per frame
    void Update()
    {
        Quaternion newq = Quaternion.LookRotation(eyes.forward);
        Vector3 lerpangle1 = Quaternion.Slerp(curq, newq, 0.01f).eulerAngles;
        Vector3 lerpangle2 = Quaternion.Slerp(curq, newq, 0.005f).eulerAngles;
        Vector3 lerpangle = lerpangle1;
        lerpangle.x = lerpangle2.x;
        lerpangle.z = 0;
        curq = Quaternion.Euler(lerpangle);

        cam.transform.eulerAngles = lerpangle + AngleOffset;
        cam.transform.localPosition = PostionOffset;
    }
}
