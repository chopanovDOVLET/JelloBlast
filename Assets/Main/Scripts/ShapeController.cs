using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Collections;
using Unity.Properties;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShapeController : MonoBehaviour
{
    public static ShapeController Instance { get; private set; }
    
    [SerializeField] List<GameObject> shapePrefabs;
    [SerializeField] List<Color> shapeColors;

    public Transform shapeBoxParent;
    [SerializeField] Transform shapeParent;
    [SerializeField] List<Transform> shapePlaces;

    public int moveCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(CreateShapes(0));
    }

    public IEnumerator CreateShapes(int second)
    {
        yield return new WaitForSeconds(second);
        int shapeType = Random.Range(0, 3);
        foreach (var place in shapePlaces)
        {   
            place.localScale = Vector3.zero;
            Transform newShape = Instantiate(shapePrefabs[0], place).transform;
            for (int i = 0; i < newShape.childCount; i++)
            {
                newShape.GetChild(i).GetComponent<Renderer>().material.color = shapeColors[Random.Range(0, 3)];
            }
            
            place.DOScale(Vector3.one, .35f);
        }
    }
}
