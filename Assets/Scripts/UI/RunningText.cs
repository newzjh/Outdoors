using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class RunningText : MonoBehaviour
{
    private Text textInfo;
    public float speed = 50;

    private RectTransform parentRectTrans;
    private RectTransform rectTrans;
    private float leftX, rightX;
    private bool canMove;
    private Vector2 vector2;

    private async void Start()
    {
        textInfo = GetComponent<Text>();

        await UniTask.NextFrame();

        parentRectTrans = transform.parent.GetComponent<RectTransform>();
        rectTrans = transform.GetComponent<RectTransform>();

        var anchorX = (rectTrans.anchorMin.x + rectTrans.anchorMax.x) / 2;

        rectTrans = transform.GetComponent<RectTransform>();

        leftX = -anchorX * parentRectTrans.rect.width * (-0.5f) - (1 - rectTrans.pivot.x) * rectTrans.rect.width;
        rightX = (1 - anchorX) * parentRectTrans.rect.width * (-0.5f) + rectTrans.pivot.x * rectTrans.rect.width;
        canMove = true;
    }

    private void Update()
    {
        if (canMove)
        {
            vector2 = rectTrans.anchoredPosition;
            vector2.x += speed * Time.deltaTime;
            if (vector2.x > rightX)
            {
                vector2.x = leftX;
            }
            rectTrans.anchoredPosition = vector2;
        }
    }
}
