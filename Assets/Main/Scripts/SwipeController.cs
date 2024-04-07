using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeController : MonoBehaviour
{
    [SerializeField] MoveMode moveMode;
    private delegate void MovementControl();
    private MovementControl MovePlayer;
    private Vector3 mouseStartPos;
    private Touch touch;
    
    [SerializeField] Transform player;
    [SerializeField] float limitArea;
    [SerializeField] float swipeSpeed;

    private void Start()
    {
        if (moveMode == MoveMode.Touch)
            MovePlayer = MovePlayerWithTouch; 
        else
            MovePlayer = MovePlayerWithMouse;
    }

    private void Update()
    {
        MovePlayer();
    }

    private void MovePlayerWithTouch() // Move Player with Touch Method
    {
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                float distance = Math.Clamp(player.position.x + touch.deltaPosition.x * swipeSpeed,
                    -limitArea, limitArea);
                player.position = new Vector3(distance, 0, 0);
            }
        }
    }
    
    private void MovePlayerWithMouse()  // Move Player with Mouse Method
    {
        if (Input.GetMouseButtonDown(0))  
            mouseStartPos = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            float deltaX = mousePos.x - mouseStartPos.x;

            float distance = Math.Clamp(player.position.x + deltaX * swipeSpeed,
                -limitArea, limitArea);
            player.position = new Vector3(distance, player.position.y, player.position.z);

            mouseStartPos = mousePos; // Update start position for continuous drag
        }
    }
}

enum MoveMode
{
    Mouse,
    Touch
}
