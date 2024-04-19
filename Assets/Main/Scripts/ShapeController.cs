using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class ShapeController : MonoBehaviour
{
    public static ShapeController Instance { get; private set; }
    [SerializeField] List<ShapeBody> shapePrefabs;
    [SerializeField] List<Color> shapeColors;

    public Transform shapeBoxParent;
    public List<Transform> shapePlaces;
    public List<ParticleSystem> particles;

    public int moveCount;
    public int candyCount;

    public TextMeshProUGUI candyAmountText;
    public CanvasGroup gameOverPanel;
    public CanvasGroup nextLevelPanel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        candyCount = Random.Range(30, 50);
        candyAmountText.text = $"{candyCount}";
        CreateShapes(0);
    }

    public void CreateShapes(float second)
    {
        StartCoroutine(WaitForSecond(second));
    }

    IEnumerator WaitForSecond(float sec)
    {
        yield return new WaitForSeconds(sec);
        int shapeRandom = Random.Range(0, 3);

        for (int i = 0; i < shapePlaces.Count; i++)
        { 
            shapePlaces[i].localScale = Vector3.zero;
                
            int randomShapeBody = Random.Range(0, shapePrefabs[0].shapeBody.Count);
            Transform newShape = Instantiate(shapePrefabs[0].shapeBody[randomShapeBody], shapePlaces[i]).transform;

            for (int j = 0; j < newShape.childCount - 1; j++)
                newShape.GetChild(j).GetComponent<Renderer>().material.color = shapeColors[Random.Range(0, 5)];

            yield return new WaitForSeconds(0.02f);
            shapePlaces[i].DOScale(Vector3.one, .35f);
            
            moveCount = 3;
        }
    }

    public void CandyCounter()
    {
        candyCount--;
        if (candyCount == 0)
            ShowNextLevelPanel();
        if (candyCount >= 0)
            candyAmountText.text = $"{candyCount}";
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
