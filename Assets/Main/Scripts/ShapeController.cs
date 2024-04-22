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
    [SerializeField] List<Color> shapeColors;

    public List<Transform> shapePlaces;
    public Transform shapeBoxParent;
    public List<ParticleSystem> particles;

    public int moveCount;
    public int levelMoveCount;
    public int candyCount;
    private Transform selectedShape;
    public Transform selectedPlace;
    public Transform oldPlace;
    public bool hasSelected;

    public TextMeshProUGUI candyAmountText;
    public TextMeshProUGUI levelMoveCountTxt;
    public CanvasGroup gameOverPanel;
    public CanvasGroup nextLevelPanel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        StartCoroutine(CreateShapes(0));
        hasSelected = false;

        StartCoroutine(Wait(0.9f));
    }

    public IEnumerator CreateShapes(float sec)
    {
        yield return new WaitForSeconds(sec);
        
        int shapeRandom = Random.Range(0, 3);

        foreach (var place in shapePlaces)
        {
            place.localScale = new Vector3(.01f, .01f, .01f);
            
            int randomShapeBody = Random.Range(0, shapePrefabs[0].shapeBody.Count);
            Transform newShape = Instantiate(shapePrefabs[0].shapeBody[randomShapeBody], place).transform;

            for (int j = 0; j < newShape.childCount - 1; j++)
                newShape.GetChild(j).GetComponent<Renderer>().material.color = shapeColors[Random.Range(0, 8)];

            yield return new WaitForSeconds(.02f);
            place.DOScale(Vector3.one, .35f);
        }

        moveCount = 3;
        StartCoroutine(Wait(0.7f));
    }

    IEnumerator Wait(float sec)
    {
        yield return new WaitForSeconds(sec);
        if (shapePlaces[1].childCount > 0)
            ChangeSelected(shapePlaces[1].GetChild(0));
    }

    public void ChangeSelected(Transform newSelected)
    {
        if (!hasSelected)
        {
            hasSelected = true;
            oldPlace = newSelected.parent;
            newSelected.GetComponent<Shape>().mouseDownCount = 1;
            newSelected.SetParent(selectedPlace);
            newSelected.DOLocalMove(Vector3.zero, .35f);
            selectedShape = newSelected;
        } 
        else if (!newSelected.GetComponent<Shape>().isSelected)
        {
            // Unselect old Shape
            selectedShape.GetComponent<Shape>().mouseDownCount = 0;
            selectedShape.GetComponent<Shape>().isSelected = false;
            selectedShape.SetParent(oldPlace);
            selectedShape.DOLocalMove(Vector3.zero, .35f);
            
            // Select new Shape
            oldPlace = newSelected.parent;
            newSelected.SetParent(selectedPlace);
            newSelected.DOLocalMove(Vector3.zero, .35f);
            selectedShape = newSelected;
        }
    }

    public void CandyCounter()
    {
        candyCount--;
        
        if (candyCount >= 0)
            candyAmountText.text = $"{candyCount}";
        
        if (candyCount == 0)
        {
            LevelController.Instance.LevelUp();
            ShowNextLevelPanel();
        }
    }

    public void ShowGameOverPanel()
    {
        gameOverPanel.gameObject.SetActive(true);
        gameOverPanel.DOFade(1f, .35f).SetEase(Ease.Linear);
    }
    
    public void TryAgain()
    {
        gameOverPanel.DOFade(0f, .35f).SetEase(Ease.Linear).OnComplete(() =>
        {
            gameOverPanel.gameObject.SetActive(false);
            SceneManager.LoadScene(0);
        });
    }
    
    public void ShowNextLevelPanel()
    {
        nextLevelPanel.gameObject.SetActive(true);
        nextLevelPanel.DOFade(1f, .35f).SetEase(Ease.Linear);
    }
    
    public void NextLevel()
    {
        
        nextLevelPanel.DOFade(0f, .35f).SetEase(Ease.Linear).OnComplete(() =>
        {
            nextLevelPanel.gameObject.SetActive(false);
            SceneManager.LoadScene(0);
        });
    }
}

[Serializable]
public class ShapeBody
{
    public List<GameObject> shapeBody;
}
