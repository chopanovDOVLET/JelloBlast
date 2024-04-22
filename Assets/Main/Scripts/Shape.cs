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
    private LineRenderer _lr;
    private Camera _camera;
    
    private Vector3 mouseDownPos;
    private Vector3 direction;
    private bool colliderEnter;
    
    [SerializeField] float forceMagnitude = 5f;
    [SerializeField] List<Transform> childInShape;

    public bool isShoot;
    public bool isSelected;
    public int mouseDownCount;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _mc = GetComponent<MeshCollider>();
        _lr = GetComponent<LineRenderer>();
        _camera = Camera.main;
    }

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
            childInShape.Add(transform.GetChild(i));

        _mc.sharedMesh = childInShape[0].GetComponent<MeshCollider>().sharedMesh;
        direction = transform.forward;
        colliderEnter = false;
        isSelected = false;
        mouseDownCount = 0;
    }

    #region DragAndShoot
        private void OnMouseDown()
        {
            mouseDownCount++;
            isSelected = (mouseDownCount >= 2) ? true : false;
            mouseDownPos = GetMousePosition();
            if (!isShoot)
                ShapeController.Instance.ChangeSelected(transform);
        }

        private void OnMouseDrag()
        {
            direction = mouseDownPos - GetMousePosition();
            
            if (direction.z > 0 && isSelected)
            {
                Vector3 lineDir = new Vector3(
                    direction.x, 
                    -Mathf.Abs(direction.z), 
                    Mathf.Abs(direction.y) + 0.1f);
                
                RenderLine(Vector3.zero, lineDir);
            }
        }

        private void OnMouseUp()
        {
            _lr.positionCount = 0;
            if (!isShoot && (mouseDownPos - GetMousePosition()).z > 0 && isSelected)
            {
                Shoot(mouseDownPos - GetMousePosition());
            }
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
            Quaternion rotDir = Quaternion.AngleAxis(7f, Vector3.right);
            Vector3 dir = rotDir * new Vector3(Force.x, 0, Force.z);
            
            _rb.AddForce(dir * forceMagnitude, ForceMode.Impulse);
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            isShoot = true;

            
            ShapeController.Instance.hasSelected = false;
            foreach (var shape in ShapeController.Instance.shapePlaces)
            {
                if (shape.childCount > 0)
                {
                    StartCoroutine(Wait(shape));
                    break;
                }
            }


            ShapeController.Instance.moveCount--;
            ShapeController.Instance.levelMoveCount--;

            if (ShapeController.Instance.levelMoveCount == 0)
            {
                ShapeController.Instance.ShowGameOverPanel();
            }
            ShapeController.Instance.levelMoveCountTxt.text = $"{ShapeController.Instance.levelMoveCount}";

            if (ShapeController.Instance.moveCount < 1)
                StartCoroutine(ShapeController.Instance.CreateShapes(.5f));
            
            transform.SetParent(ShapeController.Instance.shapeBoxParent);
        }

        IEnumerator Wait(Transform shape)
        {
            yield return new WaitForSeconds(.5f);
            ShapeController.Instance.ChangeSelected(shape.GetChild(0));
        }
    #endregion

    private void OnCollisionExit(Collision other)
    {
        if (other.collider.CompareTag("ShapeInBox"))
        {
            colliderEnter = false;
            other.collider.GetComponent<Shape>().colliderEnter = false;
        }
    }

    IEnumerator CheckCollision(Shape other)
    {
        yield return new WaitForSeconds(.3f);
        colliderEnter = false;
        other.colliderEnter = false;
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
                    //StartCoroutine(CheckCollision(otherShape));
                    
                    ShapeController.Instance.particles[0].transform.position = transform.position;
                    ShapeController.Instance.particles[0].Play();
                    Destroy(childInShape[0].gameObject);
                    Destroy(otherShape.childInShape[0].gameObject);
                    
                    childInShape.RemoveAt(0);
                    otherShape.childInShape.RemoveAt(0);

                    if (childInShape.Count == 1)
                    {
                        //Destroy(gameObject);
                        CollectCandy(transform);
                    }

                    if (otherShape.childInShape.Count == 1)
                    {
                        //Destroy(otherShape.gameObject);
                        CollectCandy(otherShape.transform);
                    }
                }
            }
        }
    }

    private void CollectCandy(Transform candy)
    {
        ShapeController.Instance.CandyCounter();
        Vector3 rotationAmount = new Vector3(0, -180, 0);
        candy.GetComponent<Rigidbody>().isKinematic = true;
        candy.DOLocalJump(candy.localPosition + Vector3.forward * 5, .7f, 1, .6f);
        candy.DORotate(rotationAmount, 0.6f).SetEase(Ease.OutQuad);
        candy.DOShakeScale(.6f, 0.1f).OnComplete(() =>
        {
            candy.DOScale(Vector3.zero, 0.15f).OnComplete(() =>
            {
                candy.DOKill();
                Destroy(candy.gameObject);
            });
        });
    }

    private void RenderLine(Vector3 startPoint, Vector3 endPoint)
    {
        _lr.positionCount = 2;
        Vector3[] points = new Vector3[2];
        points[0] = startPoint;
        points[1] = endPoint;
        
        _lr.SetPositions(points);
    }
}