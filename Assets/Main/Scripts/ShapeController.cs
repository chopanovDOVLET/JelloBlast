using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class ShapeController : MonoBehaviour
{
    public static ShapeController Instance { get; private set; }
    
    [SerializeField] List<ShapeBody> shapePrefabs;
    
    [Header("Shape Materials")]
    [SerializeField] List<Material> shapeMaterials;
    
    [Header("Shape Places")]
    public Transform shapeBoxParent;
    public List<Transform> shapePlaces;
    public List<Transform> controlPoints;
    
    [Header("Particles")]
    [SerializeField] List<ParticleSystem> appearParticles;
    [SerializeField] List<float> particleScale;
    public List<ParticleSystem> destroyParticles;
    public Transform particleBox;

    private Transform selectedShape;
    private Transform oldShape;
    [HideInInspector] public int moveCount;
    [HideInInspector] public int levelMoveCount;
    [HideInInspector] public int candyCount;
    [HideInInspector] public Transform selectedPlace;
    [HideInInspector] public Transform oldPlace;
    public bool hasSelected;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        StartCoroutine(CreateShapes(0));
        hasSelected = false;

        StartCoroutine(AutoSelectCenter(0.9f));
    }

    public IEnumerator CreateShapes(float sec)
    {
        yield return new WaitForSeconds(sec);
        
        int shapeRandom = Random.Range(0, 3);
        for (int i = 0; i < shapePlaces.Count; i++)
        {
            shapePlaces[i].localScale = new Vector3(.01f, .01f, .01f);
            
            int randomShapeBody = Random.Range(0, shapePrefabs[0].shapeBody.Count);
            Transform newShape = Instantiate(shapePrefabs[0].shapeBody[randomShapeBody], shapePlaces[i]).transform;

            int colorsAmount = LevelController.Instance.levels[LevelController.Instance.currentLevel].colorsAmount;
            for (int j = 0; j < newShape.childCount - 1; j++)
            {
                int colorIndex = Random.Range(0, colorsAmount);
                newShape.GetChild(j).GetComponent<Renderer>().sharedMaterial = shapeMaterials[colorIndex];
                newShape.GetChild(j).GetComponent<ShapeChild>().destroyParticle = destroyParticles[j];
            }

            float x = particleScale[newShape.childCount - 2];
            appearParticles[i].transform.localScale = new Vector3(x, x, x);
            appearParticles[i].Play();
            shapePlaces[i].DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
            
            // Set StartPoint, ControlPoint, EndPoint
            Shape shape = newShape.GetComponent<Shape>();
            shape.controlPoint = controlPoints[i];
            shape.startPoint = shapePlaces[i];
            shape.endPoint = selectedPlace;

        }

        moveCount = 3;
        StartCoroutine(AutoSelectCenter(0.35f));
    }
    
    public IEnumerator AutoSelectCenter(float sec)
    {
        yield return new WaitForSeconds(sec);
        if (shapePlaces[1].childCount > 0)
            ChangeSelected(shapePlaces[1].GetChild(0));
    }

    public IEnumerator AutoSelect()
    {
        yield return new WaitForSeconds(.15f);
        foreach (var place in shapePlaces)
        {
            if (place.childCount > 0)
            {
                ChangeSelected(place.GetChild(0));
                break;
            }
        }
    }

    public void ChangeShapesStatus(bool status)
    {
        if (selectedPlace.childCount > 0)
        {
            selectedPlace.GetChild(0).GetComponent<Shape>().isClickable = status;
        }
        foreach (var place in shapePlaces)
        {
            if (place.childCount > 0)
            {
                place.GetChild(0).GetComponent<Shape>().isClickable = status;
            }
        }
    }
    
    public void ChangeSelected(Transform newSelected)
    {
        if (!hasSelected)
        {
            hasSelected = true;
            ChangeShapesStatus(false);
            newSelected.GetComponent<Shape>().isClick = true;
            
            oldPlace = newSelected.parent;
            newSelected.GetComponent<Shape>().mouseDownCount = 1;
            newSelected.SetParent(selectedPlace);
            selectedShape = newSelected;
        } 
        else if (!newSelected.GetComponent<Shape>().isSelected)
        {
            // Unselect old Shape
            ChangeShapesStatus(false);
            selectedShape.GetComponent<Shape>().isClick = true;
            selectedShape.GetComponent<Shape>().mouseDownCount = 0;
            selectedShape.SetParent(oldPlace);
            selectedShape.GetComponent<Shape>().isSelected = false;

            // Select new Shape
            ChangeShapesStatus(false);
            newSelected.GetComponent<Shape>().isClick = true;
            oldPlace = newSelected.parent;
            newSelected.SetParent(selectedPlace);
            selectedShape = newSelected;
        }
    }

    private IEnumerator DestroyRemainShapes()
    {
        yield return new WaitForSeconds(.5f);
        List<Transform> shapes = new List<Transform>(shapeBoxParent.childCount);
        for (int i = 0; i < shapeBoxParent.childCount; i++)
        {
            shapeBoxParent.GetChild(i).GetComponent<Rigidbody>().isKinematic = true;
            shapes.Add(shapeBoxParent.GetChild(i));
        }
        shapes.Reverse();
        
        for (int i = 0; i < shapes.Count; i++)
        {
            if (shapes[i] != null && shapes[i].GetChild(0).GetComponent<ShapeChild>() != null)
            {
                Material mat = shapes[i].GetChild(0).GetComponent<Renderer>().sharedMaterial;
                ParticleSystem part = shapes[i].GetChild(0).GetComponent<ShapeChild>().destroyParticle;
             
                part.GetComponent<ParticleSystemRenderer>().material = mat;
                part.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = mat;
        
                ParticleSystem destroyParticle = Instantiate(part, shapes[i]);
                destroyParticle.transform.SetParent(particleBox);
                destroyParticle.Play();
        
                // Destroy the GameObject after the particle lifetime
                Destroy(shapes[i].gameObject);
                Destroy(destroyParticle.gameObject, destroyParticle.main.duration);
                yield return new WaitForSeconds(.05f);
            }
        }
        yield return new WaitForSeconds(.5f);
        UIController.Instance.ShowNextLevelPanel();
    }

    public IEnumerator ClearHalfSpace()
    {
        yield return new WaitForSeconds(1f);
        List<Transform> shapes = new List<Transform>(shapeBoxParent.childCount);
        for (int i = 0; i < shapeBoxParent.childCount / 2; i++)
        {
            shapeBoxParent.GetChild(i).GetComponent<Rigidbody>().isKinematic = true;
            shapes.Add(shapeBoxParent.GetChild(i));
        }
        shapes.Reverse();
        
        for (int i = 0; i < shapes.Count; i++)
        {
            if (shapes[i] != null && shapes[i].GetChild(0).GetComponent<ShapeChild>() != null)
            {
                Material mat = shapes[i].GetChild(0).GetComponent<Renderer>().sharedMaterial;
                ParticleSystem part = shapes[i].GetChild(0).GetComponent<ShapeChild>().destroyParticle;
             
                part.GetComponent<ParticleSystemRenderer>().material = mat;
                part.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = mat;
        
                ParticleSystem destroyParticle = Instantiate(part, shapes[i]);
                destroyParticle.transform.SetParent(particleBox);
                destroyParticle.Play();
        
                // Destroy the GameObject after the particle lifetime
                Destroy(shapes[i].gameObject);
                Destroy(destroyParticle.gameObject, destroyParticle.main.duration);
                yield return new WaitForSeconds(.05f);
            }
        }
    }

    public void CandyCounter()
    {
        candyCount--;
        
        UIController.Instance.candyAmountTxt.text = $"{candyCount}";
        
        if (candyCount == 0)
        {
            StartCoroutine(DestroyRemainShapes());
        }
    }
}

[Serializable]
public class ShapeBody
{
    public List<GameObject> shapeBody;
}
