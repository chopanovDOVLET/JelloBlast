using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeChild : MonoBehaviour
{
    private Renderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("ShapeChild"))
        {
            
        }
    }
}
