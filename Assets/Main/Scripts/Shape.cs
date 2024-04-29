using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    [SerializeField] float forceMagnitude = 5f;
    [SerializeField] List<Transform> childInShape;

    public bool isClickable;
    public bool isShoot;
    public bool isSelected;
    public int mouseDownCount;
    public bool isColliderEnter;

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

        isClickable = true;
        isSelected = false;
        mouseDownCount = 0;
    }

    private void Update()
    {
        if (_rb.IsSleeping())
        {
            _rb.WakeUp();
        }
    }

    #region DragAndShoot
        private void OnMouseDown()
        {
            if (isClickable && !UIController.Instance.isShowingPanel)
            {
                mouseDownCount++;
                isSelected = (mouseDownCount >= 2) ? true : false;
                mouseDownPos = GetMousePosition();
                if (!isShoot)
                    ShapeController.Instance.ChangeSelected(transform);
            }
        }

        private void OnMouseDrag()
        {
            direction = mouseDownPos - GetMousePosition();
            
            // Trajectory
            // if (direction.z > 0 && isSelected)
            // {
            //     Vector3 lineDir = new Vector3(
            //         direction.x, 
            //         -Mathf.Abs(direction.z), 
            //         Mathf.Abs(direction.y) + 0.1f);
            //     
            //     RenderLine(Vector3.zero, lineDir);
            // }
        }

        private void OnMouseUp()
        {
            // Trajectory
            // _lr.positionCount = 0;
            
            if (!isShoot && (GetMousePosition() - mouseDownPos).z > 0 && isSelected && isClickable)
            {
                Shoot(GetMousePosition() - mouseDownPos);
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
            Vector3 swipeDirection = rotDir * new Vector3(Force.x, 0, Force.z);
            Vector3 targetVelocity = swipeDirection.normalized * forceMagnitude;
            Vector3 velocityChange = targetVelocity - _rb.velocity;
            // Vector3 dir = rotDir * new Vector3(Force.x, 0, Force.z);
            
            _rb.AddForce(velocityChange, ForceMode.VelocityChange);
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            isShoot = true;

            ShapeController.Instance.moveCount--;
            ShapeController.Instance.levelMoveCount--;
            UIController.Instance.levelMoveCountTxt.text = $"{ShapeController.Instance.levelMoveCount}";
            
            if (!UIController.Instance.isShowingPanel)
            {
                ShapeController.Instance.hasSelected = false;
                foreach (var shape in ShapeController.Instance.shapePlaces)
                {
                    if (shape.childCount > 0)
                    {
                        StartCoroutine(Wait(shape));
                        break;
                    }
                }
            }

            if (ShapeController.Instance.moveCount < 1)
                StartCoroutine(ShapeController.Instance.CreateShapes(.2f));
            
            transform.SetParent(ShapeController.Instance.shapeBoxParent);
            
            if (ShapeController.Instance.levelMoveCount == 0)
            {
                UIController.Instance.ShowOutOfMovePanel();
            }
        }

        IEnumerator Wait(Transform shape)
        {
            yield return new WaitForSeconds(.5f);
            ShapeController.Instance.ChangeSelected(shape.GetChild(0));
        }
    #endregion

    private void OnCollisionStay(Collision other)
    {
        if (other.collider.CompareTag("ShapeInBox") && isColliderEnter)
        {
            DestroySameColor(other);
            Time.timeScale = 1f;
            other.collider.GetComponent<Shape>().isColliderEnter = false;
        }
        isColliderEnter = true;
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("ShapeInBox") && isColliderEnter)
        {
            DestroySameColor(other);
            Time.timeScale = 1f;
            other.collider.GetComponent<Shape>().isColliderEnter = false;
        }
        isColliderEnter = true;
    }

    private void DestroySameColor(Collision other)
    {
        Time.timeScale = 0.01f;
        Shape otherShape = other.collider.GetComponent<Shape>();
        if (childInShape.Count > 0 && otherShape.childInShape.Count > 0)
        {
            Material ownMaterial = childInShape[0].GetComponent<Renderer>().sharedMaterial;
            Material otherMaterial = otherShape.childInShape[0].GetComponent<Renderer>().sharedMaterial;
                
            if (ownMaterial == otherMaterial)
            {
                int indexShape = (childInShape.Count == 1) ? 0 : 1;
                int indexOtherShape = (otherShape.childInShape.Count == 1) ? 0 : 1;
                _mc.sharedMesh = childInShape[indexShape].GetComponent<MeshCollider>().sharedMesh;
                otherShape._mc.sharedMesh = otherShape.childInShape[indexOtherShape].GetComponent<MeshCollider>().sharedMesh;
                
                ShapeController.Instance.particles[0].transform.position = transform.position;
                ShapeController.Instance.particles[0].Play();
                
                if (childInShape.Count > 1)
                {
                    Time.timeScale = 1;
                    Destroy(childInShape[0].gameObject);
                    childInShape.RemoveAt(0);
                }
                
                if (otherShape.childInShape.Count > 1)
                {
                    Destroy(otherShape.childInShape[0].gameObject);
                    otherShape.childInShape.RemoveAt(0);
                }

                if (childInShape.Count == 1)
                {
                    CollectCandy(transform);
                }
                    
                if (otherShape.childInShape.Count == 1)
                {
                    CollectCandy(otherShape.transform);
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