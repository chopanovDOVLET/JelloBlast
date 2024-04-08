using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShapeController : MonoBehaviour
{
    public static ShapeController Instance { get; private set; }
    
    [SerializeField] GameObject shapePrefabs;
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

    public IEnumerator CreateShapes(float second)
    {
        yield return new WaitForSeconds(second);
        int shapeType = Random.Range(0, 3);

        for (int i = 0; i < shapePlaces.Count; i++)
        {
             Transform newShape = Instantiate(shapePrefabs, shapePlaces[i]).transform;

             for (int j = 0; j < newShape.childCount; j++)
             {
                 newShape.GetChild(j).GetComponent<Renderer>().material.color = shapeColors[Random.Range(0, 3)];
             }
             
             yield return new WaitForSeconds(.1f);
             shapePlaces[i].DOScale(Vector3.one, .35f);
        }
    }
}
