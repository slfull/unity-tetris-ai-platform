using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Board player1 = null;
    [SerializeField] private Board player2 = null;
    private PlayerInput playerInput;
    private InputAction p1Move;
    private InputAction p1HardDrop;
    private InputAction p1Hold;
    private InputAction p1Rotate;
    private InputAction p2Move;
    private InputAction p2HardDrop;
    private InputAction p2Hold;
    private InputAction p2Rotate;
    private Vector2 p1PrevFrameMove;
    private float p1PrevRotate;
    private Vector2 p2PrevFrameMove;
    private float p2PrevRotate;
    private float p1MovePressTime;
    private float p2MovePressTime;
    [SerializeField] private float holdMoveTime = 0.5f;
    [SerializeField] private float holdMoveStepTime = 0.1f;

    private void Initialize()
    {
        playerInput = InputRebindManager.Instance.Input;
        p1Move = playerInput.FindAction("P1Move");
        p1HardDrop = playerInput.FindAction("P1HardDrop");
        p1Hold = playerInput.FindAction("P1Hold");
        p1Rotate = playerInput.FindAction("P1Rotate");
        p2Move = playerInput.FindAction("P2Move");
        p2HardDrop = playerInput.FindAction("P2HardDrop");
        p2Hold = playerInput.FindAction("P2Hold");
        p2Rotate = playerInput.FindAction("P2Rotate");
        p1PrevFrameMove = new Vector2();
        p1PrevRotate = 0;
        p2PrevFrameMove = new Vector2();
        p2PrevRotate = 0;
        p1MovePressTime = Time.time;
        p2MovePressTime = Time.time;
        playerInput.Enable();
    }

    private void OnDisable()
    {
        if(playerInput != null)
        {
            playerInput.Disable();
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        p1InputManage();
        p2InputManage();
    }

    private void p1InputManage()
    {
        if (player1 == null)
        {
            return;
        }
        var p1InputMovement = p1Move.ReadValue<Vector2>();
        var p1InputRotate = p1Rotate.ReadValue<float>();

        if (p1HardDrop.triggered)
        {
            HardDrop(player1);
        }

        if (p1Hold.triggered)
        {
            Hold(player1);
        }

        if (p1Move.triggered)
        {
            Move(p1InputMovement, player1);
            p1MovePressTime = Time.time + holdMoveTime;
        }
        else if (p1MovePressTime <= Time.time)
        {
            Move(p1InputMovement, player1);
            p1MovePressTime = Time.time + holdMoveStepTime;
        }

        if (p1Rotate.triggered)
        {
            Rotate(p1InputRotate, player1);
        }

        p1PrevFrameMove = p1InputMovement;
        p1PrevRotate = p1InputRotate;
    }
    
    private void p2InputManage()
    {
        if (player2 == null)
        {
            return;
        }
        var p2InputMovement = p2Move.ReadValue<Vector2>();
        var p2InputRotate = p2Rotate.ReadValue<float>();

        if (p2HardDrop.triggered)
        {
            HardDrop(player2);
        }

        if (p2Hold.triggered)
        {
            Hold(player2);
        }

        if (p2Move.triggered)
        {
            Move(p2InputMovement, player2);
            p2MovePressTime = Time.time + holdMoveTime;
        }
        else if (p2MovePressTime <= Time.time)
        {
            Move(p2InputMovement, player2);
            p2MovePressTime = Time.time + holdMoveStepTime;
        }

        if (p2Rotate.triggered)
        {
            Rotate(p2InputRotate, player2);
        }
        p2PrevFrameMove = p2InputMovement;
        p2PrevRotate = p2InputRotate;
    }

    private void HardDrop(Board player)
    {
        player.PieceMove(Movement.HARDDROP);
    }
    
    private void Hold(Board player)
    {
        player.PieceMove(Movement.HOLD);
    }

    private void Move(Vector2 dir, Board player)
    {
        if (dir == Vector2.down)
        {
            player.PieceMove(Movement.SOFTDROP);
        }
        if (dir == Vector2.left)
        {
            player.PieceMove(Movement.LEFT);
        }
        if(dir == Vector2.right)
        {
            player.PieceMove(Movement.RIGHT);
        }
    }
    
    private void Rotate(float dir, Board player)
    {

        if (dir == -1f)
        {
            player.PieceMove(Movement.CCW);
        }
        
        if(dir == 1f)
        {
            player.PieceMove(Movement.CW);
        }
    }
}
