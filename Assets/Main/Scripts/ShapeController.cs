using System;
using System.Collections;
using System.Collections.Generic;
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
    
    [Header("Particles")]
    [SerializeField] List<ParticleSystem> appearParticles;
    [SerializeField] List<float> particleScale; 
    public List<ParticleSystem> particles;

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

        StartCoroutine(AutoSelect(0.9f));
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
                newShape.GetChild(j).GetComponent<Renderer>().sharedMaterial = shapeMaterials[Random.Range(0, colorsAmount)];

            float x = particleScale[newShape.childCount - 2];
            appearParticles[i].transform.localScale = new Vector3(x, x, x);
            appearParticles[i].Play();
            shapePlaces[i].DOScale(Vector3.one, .5f).SetEase(Ease.OutBack);
        }

        moveCount = 3;
        //yield return new WaitForSeconds(.5f);
        StartCoroutine(AutoSelect(0.7f));
    }

    IEnumerator AutoSelect(float sec)
    {
        yield return new WaitForSeconds(sec);
        if (shapePlaces[1].childCount > 0)
            ChangeSelected(shapePlaces[1].GetChild(0));
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
            oldPlace = newSelected.parent;
            newSelected.GetComponent<Shape>().mouseDownCount = 1;
            newSelected.SetParent(selectedPlace);
            
            ChangeShapesStatus(false);
            newSelected.DOLocalMove(Vector3.zero, .35f).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                ChangeShapesStatus(true);
            });
            selectedShape = newSelected;
        } 
        else if (!newSelected.GetComponent<Shape>().isSelected)
        {
            // Unselect old Shape
            selectedShape.GetComponent<Shape>().mouseDownCount = 0;
            selectedShape.SetParent(oldPlace);
            selectedShape.GetComponent<Shape>().isSelected = false;

            ChangeShapesStatus(false);
            
            selectedShape.DOLocalMove(Vector3.zero, .35f).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                ChangeShapesStatus(true);
            });
            
            // Select new Shape
            oldPlace = newSelected.parent;
            newSelected.SetParent(selectedPlace);
            
            ChangeShapesStatus(false);
            newSelected.DOLocalMove(Vector3.zero, .35f).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                ChangeShapesStatus(true);
                selectedShape = newSelected;
            });
        }
    }

    public void CandyCounter()
    {
        candyCount--;
        
        if (candyCount >= 0)
            UIController.Instance.candyAmountTxt.text = $"{candyCount}";
        
        if (candyCount == 0)
        {
            LevelController.Instance.LevelUp();
            UIController.Instance.ShowNextLevelPanel();
        }
    }
}

[Serializable]
public class ShapeBody
{
    public List<GameObject> shapeBody;
}
