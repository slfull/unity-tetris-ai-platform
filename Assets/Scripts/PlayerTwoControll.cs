using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTwoControll : MonoBehaviour
{
    [SerializeField] private KeyCode moveLeftInput;
    [SerializeField] private KeyCode moveRightInput;
    [SerializeField] private KeyCode moveDownInput;
    [SerializeField] private KeyCode hardDropInput;
    [SerializeField] private KeyCode holdInput;
    [SerializeField] private KeyCode clockwiseInput;
    [SerializeField] private KeyCode counterClockwiseInput;
    private Board board;
    private void Awake()
    {
        board = GetComponent<Board>();
    }

    private void Update()
    {
        board.activePiece.PlayerTwoExists();
        PlayerTwoInput();
    }

    private void PlayerTwoInput()
    {
        if (Input.GetKeyDown(moveLeftInput))
        {
            board.activePiece.HandleUpdateMove(0);
        }
        if (Input.GetKeyDown(moveRightInput))
        {
            board.activePiece.HandleUpdateMove(1);
        }
        if (Input.GetKeyDown(moveDownInput))
        {
            board.activePiece.HandleUpdateMove(2);
        }
        if (Input.GetKeyDown(hardDropInput))
        {
            board.activePiece.HandleUpdateMove(3);
        }
        if (Input.GetKeyDown(holdInput))
        {
            board.SwapPiece();
        }
        if (Input.GetKeyDown(clockwiseInput))
        {
            board.activePiece.HandleUpdateMove(5);
        }
        if (Input.GetKeyDown(clockwiseInput))
        {
            board.activePiece.HandleUpdateMove(4);
        }
    }
}
