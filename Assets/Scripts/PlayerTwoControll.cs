using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTwoControll : MonoBehaviour
{
    [Header("InputKeyCode")]
    [SerializeField] private KeyCode moveLeftInput;
    [SerializeField] private KeyCode moveRightInput;
    [SerializeField] private KeyCode moveDownInput;
    [SerializeField] private KeyCode hardDropInput;
    [SerializeField] private KeyCode holdInput;
    [SerializeField] private KeyCode clockwiseInput;
    [SerializeField] private KeyCode counterClockwiseInput;

    [Header("DPadVal")]
    [SerializeField] private float dpadX;
    [SerializeField] private float dpadY;
    [SerializeField] private float prevDpadX;
    [SerializeField] private float prevDpadY;

    private Board board;

    //Unity Function
    private void Awake()
    {
        board = GetComponent<Board>();
    }

    private void Update()
    {
        board.activePiece.PlayerTwoExists();
        PlayerTwoInput();
    }

    //Custom Function
    private void PlayerTwoInput()
    {
        GetDpadAxis();

        if (Input.GetKeyDown(moveLeftInput) || (dpadX < 0 && dpadX != prevDpadX))
        {
            board.PieceMove(Movement.LEFT);
        }
        if (Input.GetKeyDown(moveRightInput) || (dpadX > 0 && dpadX != prevDpadX))
        {
            board.PieceMove(Movement.RIGHT);
        }
        if (Input.GetKeyDown(moveDownInput) || (dpadY > 0 && dpadY != prevDpadY))
        {
            board.PieceMove(Movement.SOFTDROP);
        }
        if (Input.GetKeyDown(hardDropInput) || (dpadY < 0 && dpadY != prevDpadY))
        {
            board.PieceMove(Movement.HARDDROP);
        }
        if (Input.GetKeyDown(holdInput))
        {
            board.PieceMove(Movement.HOLD);
        }
        if (Input.GetKeyDown(clockwiseInput))
        {
            board.PieceMove(Movement.CW);
        }
        if (Input.GetKeyDown(counterClockwiseInput))
        {
            board.PieceMove(Movement.CCW);
        }

        prevDpadX = dpadX;
        prevDpadY = dpadY;
    }

    private void GetDpadAxis()
    {
        dpadX = Input.GetAxis("DPadHorizontal");
        dpadY = Input.GetAxis("DPadVertical");
    }


    //Debuger
    private void CheckDPad()
    {

    }
}
