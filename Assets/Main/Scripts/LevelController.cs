using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; set; }

    [SerializeField] private Transform shapeBoxParent;
    [SerializeField] private Transform environment;
    public List<Level> levels;
    public int currentLevel;
    public int coinAmount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        coinAmount = PlayerPrefs.GetInt("Coin", 0);
        currentLevel = PlayerPrefs.GetInt("Level", 0);
        ShapeController.Instance.candyCount = levels[currentLevel].collectCandyAmount;
        ShapeController.Instance.levelMoveCount = levels[currentLevel].levelMoveCount;
        
        UIController.Instance.currentLevelTxt.text = $"LEVEL {currentLevel + 1}";
        UIController.Instance.candyAmountTxt.text = $"{ShapeController.Instance.candyCount}";
        UIController.Instance.levelMoveCountTxt.text = $"{ShapeController.Instance.levelMoveCount}";
        UIController.Instance.coinAmountTxt.text = $"{coinAmount}";

        foreach (var bigShape in levels[currentLevel].bigShapes)
            Instantiate(bigShape, shapeBoxParent);

        foreach (var obstacle in levels[currentLevel].obstacles)
            Instantiate(obstacle, environment);
    }

    public void LevelUp()
    {
        currentLevel++;
        coinAmount += 10;
        currentLevel = (currentLevel > 9) ? Random.Range(5, 10) : currentLevel;
        PlayerPrefs.SetInt("Coin", coinAmount);
        PlayerPrefs.SetInt("Level", currentLevel);
    }

    public void ContinueClearSpace()
    {
        if (coinAmount >= 100)
        {
            coinAmount -= 100;
            PlayerPrefs.SetInt("Coin", coinAmount);
            UIController.Instance.HideOutOfSpacePanel();
            WarningLine.Instance.isGameOver = false;
            for (int i = 0; i < (int)(ShapeController.Instance.shapeBoxParent.childCount / 3); i++)
            {
                ShapeController.Instance.shapeBoxParent.GetChild(i).DOScale(new Vector3(.01f, .01f, .01f), .35f).OnComplete(() =>
                {
                    Destroy(ShapeController.Instance.shapeBoxParent.GetChild(i).gameObject);
                });
            }
        }
    }
    
    public void ContinueAddMove()
    {
        if (coinAmount >= 100)
        {
            coinAmount -= 100;
            PlayerPrefs.SetInt("Coin", coinAmount);
            UIController.Instance.HideOutOfMovePanel();
            ShapeController.Instance.levelMoveCount = 10;
            WarningLine.Instance.isGameOver = false;
        }
    }
}

[Serializable]
public class Level
{
    public int collectCandyAmount;
    public int levelMoveCount;
    public int colorsAmount;
    public List<GameObject> obstacles;
    public List<GameObject> bigShapes;
}
