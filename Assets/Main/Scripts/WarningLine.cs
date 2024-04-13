using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor.Build;
using UnityEngine;

public class WarningLine : MonoBehaviour
{
    private float stayTime;
    private bool isGameOver = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ShapeInBox"))
            Debug.Log("GameOver");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isGameOver)
            if (other.CompareTag("Shape"))
                if (stayTime < 3)
                {
                    stayTime += Time.deltaTime;
                }
                else
                {
                    stayTime = 0;
                    Debug.Log("GameOver");
                    isGameOver = true;
                }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Shape"))
        {
            stayTime = 0;
            other.tag = "ShapeInBox";
        }
    }
}
