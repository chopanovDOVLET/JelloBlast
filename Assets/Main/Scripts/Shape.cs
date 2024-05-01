using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Shape : MonoBehaviour
{
    private Rigidbody _rb; 
    private MeshCollider _mc;
    private Camera _camera;
    
    private Vector3 mouseDownPos;
    private Vector3 direction;
    
    [SerializeField] float forceStrength = 20f;
    
    [HideInInspector] public List<Transform> childInShape;
    [HideInInspector] public int mouseDownCount;
    [HideInInspector] public bool isClickable;
    [HideInInspector] public bool isShoot;
    [HideInInspector] public bool isSelected;

    #region IntializeAndUpdate  
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _mc = GetComponent<MeshCollider>();
            _camera = Camera.main;
        }

        private void Start()
        {
            // Get children
            for (int i = 0; i < transform.childCount; i++)
                childInShape.Add(transform.GetChild(i));
            
            // Get last OutSide Shape's Mesh
            _mc.sharedMesh = childInShape[0].GetComponent<MeshCollider>().sharedMesh;

            // Initialize 
            direction = transform.forward;
            isClickable = true;
            isSelected = false;
            mouseDownCount = 0;
        }

        private void FixedUpdate()
        {
            _rb.WakeUp();
        }
    #endregion

    #region SwipeControl
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
        }

        private void OnMouseUp()
        {
            if (!isShoot && (GetMousePosition() - mouseDownPos).z > 0 && isSelected && isClickable)
                Shoot(GetMousePosition() - mouseDownPos);
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
    #endregion

    #region ShootAndAutoSelect
        private void Shoot(Vector3 Force)
        {
            isShoot = true;
            transform.SetParent(ShapeController.Instance.shapeBoxParent);
            EnableGravityZ(0);
            
            // Add Force And Remove Freeze
            Quaternion rotDir = Quaternion.AngleAxis(7f, Vector3.right);
            Vector3 swipeDirection = rotDir * new Vector3(Force.x, 0, Force.z);
            Vector3 targetVelocity = swipeDirection.normalized * forceStrength;
            Vector3 velocityChange = targetVelocity - _rb.velocity;
            _rb.AddForce(velocityChange, ForceMode.VelocityChange);
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            
            // Update Swiped shapes amount and Move amount also UI text
            ShapeController.Instance.moveCount--;
            ShapeController.Instance.levelMoveCount--;
            UIController.Instance.levelMoveCountTxt.text = $"{ShapeController.Instance.levelMoveCount}";
            
            if (!UIController.Instance.isShowingPanel)
            {
                ShapeController.Instance.hasSelected = false;
                foreach (var shape in ShapeController.Instance.shapePlaces)
                    if (shape.childCount > 0)
                    {
                        StartCoroutine(AutoSelect(shape));
                        break;
                    }
            }

            if (ShapeController.Instance.moveCount < 1)
                StartCoroutine(ShapeController.Instance.CreateShapes(.2f));

            if (ShapeController.Instance.levelMoveCount == 0)
                UIController.Instance.ShowOutOfMovePanel();
        }

        IEnumerator AutoSelect(Transform shape)
        {
            yield return new WaitForSeconds(.5f);
            ShapeController.Instance.ChangeSelected(shape.GetChild(0));
        }
    #endregion

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("ShapeInBox"))
        {
            CheckShapesColor(other.collider.GetComponent<Shape>());
            EnableGravityZ(17f);
        }
    }

    private void EnableGravityZ(float z)
    {
        Physics.gravity = new Vector3(0f, Physics.gravity.y, z);
        _rb.AddForce(Physics.gravity, ForceMode.Acceleration);
    }

    IEnumerator EnableOutSide(Shape otherShape, int frameCount)
    {
        while (frameCount > 0)
        {
            frameCount--;
            yield return null;
        }
        childInShape[0].GetComponent<ShapeChild>().isOutSide = true;    
        otherShape.childInShape[0].GetComponent<ShapeChild>().isOutSide = true;
    }
    

    private void CheckShapesColor(Shape shape2)
    {
        if (childInShape.Count > 0 && shape2.childInShape.Count > 0)
        {
            Material shape1Material = childInShape[0].GetComponent<Renderer>().sharedMaterial;
            Material shape2Material = shape2.childInShape[0].GetComponent<Renderer>().sharedMaterial;
            bool isOutSide1 = childInShape[0].GetComponent<ShapeChild>().isOutSide;    
            bool isOutSide2 = shape2.childInShape[0].GetComponent<ShapeChild>().isOutSide;
            
            if (shape1Material == shape2Material && isOutSide1 && isOutSide2)
            {
                // Change Mesh to last OutSide Shape's Mesh and Add a bit Bounce effect
                ChangeMeshAndBounceEffect(shape2);
                
                // Enable last Shape isOutSide TRUE after 10 frame
                StartCoroutine(EnableOutSide(shape2, 10));

                // Play Particle effect Shapes collided each other
                PlayCollidedParticle();
                
                
                if (childInShape.Count > 1)
                {
                    Destroy(childInShape[0].gameObject);
                    childInShape.RemoveAt(0);
                }
                
                if (shape2.childInShape.Count > 1)
                {
                    Destroy(shape2.childInShape[0].gameObject);
                    shape2.childInShape.RemoveAt(0);
                }
                
                if (childInShape.Count == 1)
                {
                    StartCoroutine(CollectCandy(transform));
                }
                    
                if (shape2.childInShape.Count == 1)
                {
                    StartCoroutine(CollectCandy(shape2.transform));
                }
            }
        }
    }

    private void ChangeMeshAndBounceEffect(Shape shape2)
    {
        int strength1 = Random.Range(9, 13);
        _rb.velocity = Vector3.back * strength1;
                
        int strength2 = Random.Range(strength1, 13);
        shape2._rb.velocity = Vector3.back * strength2;
        
        int indexShape1 = (childInShape.Count == 1) ? 0 : 1;
        int indexShape2 = (shape2.childInShape.Count == 1) ? 0 : 1;
        _mc.sharedMesh = childInShape[indexShape1].GetComponent<MeshCollider>().sharedMesh;
        shape2._mc.sharedMesh = shape2.childInShape[indexShape2].GetComponent<MeshCollider>().sharedMesh;
    }

    private void PlayCollidedParticle()
    {
        // ShapeController.Instance.particles[0].transform.position = transform.position;
        // ShapeController.Instance.particles[0].Play();
    }

    IEnumerator CollectCandy(Transform candy)
    {
        ShapeController.Instance.CandyCounter();
        candy.DOLocalRotate(Vector3.zero, 0f);
        candy.DOJump(candy.position + new Vector3(0, 3, 0), .5f, 1, .7f);
        candy.DOLocalRotate(new Vector3(360f, 0, 0), .7f, RotateMode.FastBeyond360);
        candy.DOScale(Vector3.one * 1.5f, .7f);
        
        yield return new WaitForSeconds(.5f);
        candy.DOScale(Vector3.zero, .2f);

        yield return new WaitForSeconds(.2f);
        Destroy(candy.gameObject);
    }
}