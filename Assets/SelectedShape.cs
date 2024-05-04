using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedShape : MonoBehaviour
{
    public bool isTriggered;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Shape") || other.CompareTag("ShapeInBox"))
        {
            isTriggered = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Shape") || other.CompareTag("ShapeInBox"))
        {
            isTriggered = false;
            
            if (!UIController.Instance.isShowingPanel && !ShapeController.Instance.hasSelected)
            {
                if (ShapeController.Instance.moveCount > 0) 
                    StartCoroutine(ShapeController.Instance.AutoSelect());
                else if (ShapeController.Instance.moveCount < 1)
                    StartCoroutine(ShapeController.Instance.CreateShapes(.2f));
            }
        }
    }
}
