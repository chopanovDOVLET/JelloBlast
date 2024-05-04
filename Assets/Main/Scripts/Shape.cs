using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
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

    [HideInInspector] public Transform startPoint;
    [HideInInspector] public Transform endPoint;
    [HideInInspector] public Transform controlPoint;
    private float speed = 3f;
    private float progress = 0.0f;
    public bool isClick;
    public bool bigShape;
    
    
    #region Intialize
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _mc = GetComponent<MeshCollider>();
            _camera = Camera.main;
        }

        private void Start()
        {
            // If this is Big shape then Enable Gravity
            if (bigShape)
                EnableGravityZ(17f);
            
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
    
    #region UpdateAndBezier
        void Update()
        {
            if (isClick)
            {
                progress += Time.deltaTime * speed;
                transform.position = BezierFunction(startPoint.position, controlPoint.position, endPoint.position, progress);

                if (progress >= 1.0f)
                {
                    isClick = false;
                    progress = 0f;
                    endPoint = startPoint;
                    startPoint = transform.parent;
                    ShapeController.Instance.ChangeShapesStatus(true);
                }
            }
            else if (!isShoot && !bigShape)
            {
                transform.localPosition = Vector3.zero;
            }
        }

        Vector3 BezierFunction(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return Mathf.Pow(1 - t, 3) * p0 + 
                   3 * Mathf.Pow(1 - t, 2) * t * p1 + 
                   3 * (1 - t) * Mathf.Pow(t, 2) * p2 + 
                   Mathf.Pow(t, 3) * p2;
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

            ShapeController.Instance.hasSelected = false;

            if (ShapeController.Instance.levelMoveCount == 0)
                UIController.Instance.ShowOutOfMovePanel();
        }
    #endregion

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("ShapeInBox"))
        {
            CheckShapesColor(other.collider.GetComponent<Shape>(), other);
            EnableGravityZ(17f);
        } 
        
        if (other.collider.CompareTag("Backside"))
        {
            EnableGravityZ(7f);
        }
        
        if (other.collider.CompareTag("Obstacle"))
        {
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

        if (childInShape[0].GetComponent<ShapeChild>() != null && childInShape[0] != null)
            childInShape[0].GetComponent<ShapeChild>().isOutSide = true;

        if (otherShape.childInShape[0].GetComponent<ShapeChild>() != null && otherShape.childInShape[0] != null)
            otherShape.childInShape[0].GetComponent<ShapeChild>().isOutSide = true;
    }
    

    private void CheckShapesColor(Shape shape2, Collision collision)
    {
        if (childInShape.Count > 0 && shape2.childInShape.Count > 0)
        {
            Material shape1Material = childInShape[0].GetComponent<Renderer>().sharedMaterial;
            Material shape2Material = shape2.childInShape[0].GetComponent<Renderer>().sharedMaterial;
            bool isOutSide1 = false;
            bool isOutSide2 = false;
            
            if (childInShape[0].GetComponent<ShapeChild>() != null)
                isOutSide1 = childInShape[0].GetComponent<ShapeChild>().isOutSide;  
            
            if (shape2.childInShape[0].GetComponent<ShapeChild>() != null)
                isOutSide2 = shape2.childInShape[0].GetComponent<ShapeChild>().isOutSide;

            if (shape1Material == shape2Material && isOutSide1 && isOutSide2)
            {
                // Change Mesh to last OutSide Shape's Mesh and Add a bit Bounce effect
                ChangeMeshAndBounceEffect(shape2, collision);
                
                // Enable last Shape isOutSide TRUE after 10 frame
                StartCoroutine(EnableOutSide(shape2, 10));

                // Play Particle effect Shapes collided each other
                PlayCollidedParticle(shape2.transform, 
                    childInShape[0].GetComponent<ShapeChild>().destroyParticle, 
                    shape2.childInShape[0].GetComponent<ShapeChild>().destroyParticle,
                    shape1Material, shape2Material);

                // childInShape[0].DOScale(Vector3.zero, .7f).SetEase(Ease.InBack);
                // shape2.childInShape[0].DOScale(Vector3.zero, .7f).SetEase(Ease.InBack);
                
                if (childInShape.Count > 1)
                {
                    Destroy(childInShape[0].gameObject, .0f);
                    childInShape.RemoveAt(0);
                }
                
                if (shape2.childInShape.Count > 1)
                {
                    Destroy(shape2.childInShape[0].gameObject, .0f);
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

    private void ChangeMeshAndBounceEffect(Shape shape2, Collision collision)
    {
        EnableGravityZ(0);
        shape2.EnableGravityZ(0);
        Vector3 reflectedVelocity = Vector3.Reflect(_rb.velocity, collision.contacts[0].normal);
        
        int strength = Random.Range(7, 10);
        reflectedVelocity = new Vector3(0, 0, -strength);
        _rb.velocity = Vector3.back * strength;
        EnableGravityZ(100);
        
        shape2._rb.velocity = Vector3.back * strength;
        shape2.EnableGravityZ(100); 
        
        int indexShape1 = (childInShape.Count == 1) ? 0 : 1;
        int indexShape2 = (shape2.childInShape.Count == 1) ? 0 : 1;
        _mc.sharedMesh = childInShape[indexShape1].GetComponent<MeshCollider>().sharedMesh;
        shape2._mc.sharedMesh = shape2.childInShape[indexShape2].GetComponent<MeshCollider>().sharedMesh;
    }

    private void PlayCollidedParticle(
        Transform shape2, 
        ParticleSystem part1, ParticleSystem part2, 
        Material mat1, Material mat2)
    {
        part1.GetComponent<ParticleSystemRenderer>().material = mat1;
        part2.GetComponent<ParticleSystemRenderer>().material = mat2;
        part1.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = mat1;
        part2.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = mat2;
        
        ParticleSystem destroyParticle1 = Instantiate(part1, transform);
        ParticleSystem destroyParticle2 = Instantiate(part2, shape2);

        destroyParticle1.transform.SetParent(ShapeController.Instance.particleBox);
        destroyParticle2.transform.SetParent(ShapeController.Instance.particleBox);
        
        destroyParticle1.Play();
        destroyParticle2.Play();
        
        // Destroy the GameObject after the particle lifetime
        Destroy(destroyParticle1.gameObject, destroyParticle1.main.duration);
        Destroy(destroyParticle2.gameObject, destroyParticle2.main.duration);
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