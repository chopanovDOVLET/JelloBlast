using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class Shape : MonoBehaviour
{
    private LaserBeam beam;
    private Rigidbody _rb; 
    private MeshCollider _mc;
    private Camera _camera;
    
    private Vector3 mouseDownPos;
    private Vector3 direction;
    private bool isShoot;
    private bool isTrajectory;
    private bool colliderEnter;

    [SerializeField] Material material;
    [SerializeField] float forceMagnitude = 5f;
    public int childCount;
    public List<Transform> childInShape;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _mc = GetComponent<MeshCollider>();
        _camera = Camera.main;

        childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
            childInShape[i] = transform.GetChild(i);

        int childShapeCount = childInShape.Count;
        for (int i = 0; i < childShapeCount - childCount; i++)
            childInShape.RemoveAt(childCount);
        
        _mc.sharedMesh = childInShape[0].GetComponent<MeshCollider>().sharedMesh;
        direction = transform.forward;
        colliderEnter = false;
        isTrajectory = false;
    }

    private void Update()
    {
        if (isTrajectory && !isShoot && direction.z > 0)
        {
            Destroy(GameObject.Find("Laser Beam"));
            beam = new LaserBeam(transform.position, new Vector3(direction.x, 0, direction.z), material);
        }
    }

    #region DragAndShoot
        private void OnMouseDown()
        {
            
            foreach (var child in childInShape)
                child.GetComponent<MeshCollider>().enabled = true;
            
            isTrajectory = true;
            mouseDownPos = GetMousePosition();
        }

        private void OnMouseDrag()
        {
            direction = mouseDownPos - GetMousePosition();
        }

        private void OnMouseUp()
        {
            isTrajectory = false;
            Destroy(GameObject.Find("Laser Beam"));
            Shoot(mouseDownPos - GetMousePosition());
        } 
        
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
            if (!isShoot && Force.z > 0)
            {
                Force = Vector3.ProjectOnPlane(Force, Vector3.up);
                Vector3 velocity = new Vector3(Force.x, 0, Force.z) * forceMagnitude;
                _rb.AddForce(velocity, ForceMode.Impulse);
                _rb.constraints = RigidbodyConstraints.FreezePositionY;
                _rb.freezeRotation = true;
                _mc.isTrigger = false;
                isShoot = true;

                ShapeController.Instance.moveCount--;
                
                if (ShapeController.Instance.moveCount < 1)
                    ShapeController.Instance.CreateShapes(1.5f);
                
                transform.SetParent(ShapeController.Instance.shapeBoxParent);
            }
        }
    #endregion

    private void OnCollisionExit(Collision other)
    {
        if (other.collider.CompareTag("ShapeInBox")) 
            colliderEnter = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("ShapeInBox"))
        {
            Shape otherShape = other.collider.GetComponent<Shape>();
            if (childInShape.Count > 0 && otherShape.childInShape.Count > 0)
            {
                Color ownColor = childInShape[0].GetComponent<Renderer>().material.color;
                Color otherColor = otherShape.childInShape[0].GetComponent<Renderer>().material.color;
                
                if (ownColor == otherColor && !otherShape.colliderEnter)
                {
                    _mc.sharedMesh = childInShape[1].GetComponent<MeshCollider>().sharedMesh;
                    otherShape._mc.sharedMesh = otherShape.childInShape[1].GetComponent<MeshCollider>().sharedMesh;
                    
                    colliderEnter = true;
                    otherShape.colliderEnter = true;
                    
                    ShapeController.Instance.particles[0].transform.position = transform.position;
                    ShapeController.Instance.particles[0].Play();
                    Destroy(childInShape[0].gameObject);
                    Destroy(otherShape.childInShape[0].gameObject);
                    
                    childInShape.RemoveAt(0);
                    otherShape.childInShape.RemoveAt(0);

                    if (childInShape.Count == 1) 
                        Destroy(gameObject);
                        
                    if (otherShape.childInShape.Count == 1) 
                        Destroy(otherShape.gameObject);
                }
            }
        }
    }
}