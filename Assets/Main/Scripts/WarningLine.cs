using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WarningLine : MonoBehaviour
{
    public static WarningLine Instance { get; private set; }
    
    [SerializeField] Material warningMaterial;
    private Material defaultMaterial;
    private float stayTime;
    public bool isGameOver = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        defaultMaterial = GetComponent<Renderer>().sharedMaterial;
    }

    private void OnTriggerEnter(Collider other)
    {
        // if (other.CompareTag("Shape"))
        //     other.tag = "ShapeInBox";
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isGameOver)
            if (other.CompareTag("ShapeInBox"))
            {
                if (stayTime < 3)
                {
                    UIController.Instance.warningCounterTxt.text = $"{1 + (int)stayTime}";
                    stayTime += Time.deltaTime;
                    GetComponent<Renderer>().sharedMaterial = warningMaterial;
                }
                else
                {
                    UIController.Instance.warningCounterTxt.text = "";
                    GetComponent<Renderer>().sharedMaterial = defaultMaterial;
                    stayTime = 0;
                    UIController.Instance.ShowOutOfSpacePanel();
                    isGameOver = true;
                }
            } 
            else if (other.CompareTag("Shape"))
            {
                StartCoroutine(Wait(other));
            }
    }

    IEnumerator Wait(Collider other)
    {
        yield return new WaitForSeconds(0.5f);
        other.tag = "ShapeInBox";
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Shape") || other.CompareTag("ShapeInBox"))
        {
            stayTime = 0;
            UIController.Instance.warningCounterTxt.text = "";
            GetComponent<Renderer>().sharedMaterial = defaultMaterial;
            other.tag = "ShapeInBox";
        }
    }
}
