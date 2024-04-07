using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

public class Shape : MonoBehaviour
{
    private Rigidbody _rb;
    private MeshCollider _mc;
    private Camera _camera;
    
    private Vector3 mouseDownPos;
    private bool isShoot;

    [SerializeField] float forceMagnitude = 5f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _mc = GetComponent<MeshCollider>();
        _camera = Camera.main;

        _mc.sharedMesh = transform.GetChild(0).GetComponent<MeshCollider>().sharedMesh;
    }

    #region DragAndShoot
        private void OnMouseDown() => mouseDownPos = GetMousePosition();
        private void OnMouseUp() => Shoot(mouseDownPos - GetMousePosition());
        
        private Vector3 GetMousePosition()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
                return hit.point;
            else
                return mouseDownPos;
        }

        private void Shoot(Vector3 Force)
        {
            if (isShoot || Force.z <= 0) 
                return;

            Vector3 velocity = new Vector3(Force.x, 0, Force.z) * forceMagnitude;
            _rb.AddForce(velocity, ForceMode.Impulse);
            isShoot = true;

            ShapeController.Instance.moveCount++;
            if (ShapeController.Instance.moveCount == 3)
            {
                ShapeController.Instance.moveCount = 0;
                StartCoroutine(ShapeController.Instance.CreateShapes(2));
            }
        }
    #endregion

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Shape"))
        {
            Color otherColor = other.transform.GetChild(0).GetComponent<Renderer>().material.color;
            Color ownColor = transform.GetChild(0).GetComponent<Renderer>().material.color;
            
            if (ownColor == otherColor)
            {
                _mc.sharedMesh = transform.GetChild(1).GetComponent<MeshCollider>().sharedMesh;
                Destroy(transform.GetChild(0).gameObject);
                Destroy(other.transform.GetChild(0).gameObject);
            }
        }
    }
}
