using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BKMove : MonoBehaviour
{
    public float spend=500;

    public RectTransform otherTransform;

    public RectTransform rectTransform;

    private void Start()
    {
        rectTransform=this.transform as RectTransform;
    }

    void Update()
    {
        this.transform.Translate(Vector3.left*spend*Time.deltaTime);
        if(rectTransform.anchoredPosition.x<= -1920)
            rectTransform.anchoredPosition = otherTransform.anchoredPosition+ Vector2.right*1920;
    }
}
