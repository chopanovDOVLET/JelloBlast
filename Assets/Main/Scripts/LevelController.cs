using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; set; }

    public Transform shapeBoxParent;
    public Transform environment;
    public List<Level> levels;
    public int currentLevel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentLevel = PlayerPrefs.GetInt("Level", 0);
        ShapeController.Instance.candyCount = levels[currentLevel].collectCandyAmount;
        ShapeController.Instance.levelMoveCount = levels[currentLevel].levelMoveCount;

        ShapeController.Instance.candyAmountText.text = $"{ShapeController.Instance.candyCount}";
        ShapeController.Instance.levelMoveCountTxt.text = $"{ShapeController.Instance.levelMoveCount}";

        foreach (var bigShape in levels[currentLevel].bigShapes)
        {
            Instantiate(bigShape, shapeBoxParent);
        }
        
        foreach (var obstacle in levels[currentLevel].obstacles)
        {
            Instantiate(obstacle, environment);
        }
    }

    public void LevelUp()
    {
        currentLevel++;
        currentLevel = (currentLevel > 10) ? Random.Range(5, 10) : currentLevel;
        PlayerPrefs.SetInt("Level", currentLevel);
    }
}

[Serializable]
public class Level
{
    public int collectCandyAmount;
    public int levelMoveCount;
    public List<GameObject> obstacles;
    public List<GameObject> bigShapes;
}
