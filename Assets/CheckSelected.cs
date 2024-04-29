using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckSelected : MonoBehaviour
{
    public static CheckSelected Instance { get; private set; }

    public bool isTriggered;

    private void Awake()
    {
        Instance = this;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Shape") || other.CompareTag("ShapeInBox"))
        {
            isTriggered = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ShapeInBox") || other.CompareTag("Shape"))
        {
            isTriggered = false;
        }
    }
    
}
