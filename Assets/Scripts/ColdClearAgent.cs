using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ColdClearAgent : MonoBehaviour
{
    [SerializeField] private float agentSpeed = 0.5f;
    public IntPtr bot { get; private set; } = IntPtr.Zero;
    private Board board;
    private CCMove move;
    private CCBotPollStatus status;
    private bool isPrevMoveHold = false;
    private int prevScore;
    private float agentStepTime = 0;
    private float waitingTimeInterval = 0.1f;
    private float waitingTime = 0f;
    private bool isRequest = false;
    public int pieceCounter = 0;
    public enum Movement
    {
        LEFT, RIGHT,
        DROP, HARDDROP,
        CW, CCW,
        HOLD
    }
    [SerializeField] private List<Movement> movementQueue;
    private Dictionary<Movement, Action> movementActions;
    private Dictionary<CCMovement, Movement> moveMap;

    //monoBhaviour

    void Start()
    {
        BotInitialize();
        board = GetComponent<Board>();
        agentStepTime = Time.time;
        waitingTime = Time.time;
        status = CCBotPollStatus.CC_WAITING;
        pieceCounter = 0;
        
        movementActions = new Dictionary<Movement, Action>()
        {
            { Movement.LEFT, MoveLeft },
            { Movement.RIGHT, MoveRight },
            { Movement.CW, ClockWise },
            { Movement.CCW, CounterClockWise },
            { Movement.HOLD, Hold },
            { Movement.DROP, Drop },
            { Movement.HARDDROP, HardDrop }
        };

        moveMap = new Dictionary<CCMovement, Movement>()
        {
            { CCMovement.CC_LEFT, Movement.LEFT },
            { CCMovement.CC_RIGHT, Movement.RIGHT },
            { CCMovement.CC_CW, Movement.CW },
            { CCMovement.CC_CCW, Movement.CCW },
            { CCMovement.CC_DROP, Movement.DROP}
        };
    }

    void Update()
    {
        AddPieceQueue();
        if (Time.time >= agentStepTime)
        {
            HandleMovement();
            agentStepTime = Time.time + agentSpeed;
        }

        if (status == CCBotPollStatus.CC_MOVE_PROVIDED)
        {
            FillMovementQueue();
            status = CCBotPollStatus.CC_WAITING;
            isRequest = false;
        }
        else if (status == CCBotPollStatus.CC_BOT_DEAD)
        {
            Debug.LogWarning("[Cold Clear] Bot Dead");
        }
        else if (!isRequest)
        {
            isRequest = true;
            RequestNextMove();
        }
        else
        {
            PollNextMove();
        }
    }

    //movement handle

    private void HandleMovement()
    {
        if (movementQueue.Count > 0)
        {
            if (movementActions.TryGetValue(movementQueue[0], out var action))
            {
                action.Invoke();
            }
            movementQueue.RemoveAt(0);
        }
    }

    private void MoveLeft()
    {
        board.activePiece.HandleUpdateMove(0);
    }

    private void MoveRight()
    {
        board.activePiece.HandleUpdateMove(1);
    }

    private void ClockWise()
    {
        board.activePiece.HandleUpdateMove(5);
    }

    private void CounterClockWise()
    {
        board.activePiece.HandleUpdateMove(4);
    }

    private void Hold()
    {
        board.SwapPiece();
    }

    private void Drop()
    {
        for (int i = 0; i < 20; i++)
        {
            SoftDrop();
        }
    }

    private void HardDrop()
    {
        board.activePiece.HandleUpdateMove(3);
    }

    private void SoftDrop()
    {
        board.activePiece.HandleUpdateMove(2);
    }

    //initialize

    public void BotInitialize()
    {

        if (bot != IntPtr.Zero)
        {
            ColdClearNative.cc_destroy_async(bot);
            bot = IntPtr.Zero;
        }


        CCPiece[] queue = BoardToColdClear.instance.GetQueue();

        CCOptions options = DefaultOption();
        CCWeights weights;
        ColdClearNative.cc_default_weights(out weights);
        IntPtr queuePtr = IntPtr.Zero;
        uint queueCount = (uint)queue.Length;
        if (queueCount > 0)
        {
            queuePtr = Marshal.AllocHGlobal(sizeof(int) * queue.Length);
            for (int i = 0; i < queue.Length; i++)
                Marshal.WriteInt32(queuePtr + i * sizeof(int), (int)queue[i]);
        }
        bot = ColdClearNative.cc_launch_async(
            ref options, ref weights, IntPtr.Zero,
            queuePtr, queueCount
        );
        if (queuePtr != IntPtr.Zero) Marshal.FreeHGlobal(queuePtr);

        Debug.Log($"queueCount={queue.Length}");
        for (int i = 0; i < queue.Length; i++)
            Debug.Log($"queue[{i}]={queue[i]}");
    }

    private CCOptions DefaultOption()
    {
        CCOptions options;
        ColdClearNative.cc_default_options(out options);
        options.spawn_rule = CCSpawnRule.CC_ROW_19_OR_20;
        return options;
    }

    //bot run
    private void RequestNextMove()
    {
        ColdClearNative.cc_request_next_move(bot, 0); // 之後擴充雙人對戰要改incoming
        isRequest = true;
    }

    private void PollNextMove()
    {
        if (Time.time >= waitingTime)
        {
            waitingTime = Time.time + waitingTimeInterval;
            status = ColdClearNative.cc_poll_next_move(bot, out move, IntPtr.Zero, IntPtr.Zero);
        }
    }

    public void AddNewPiece(CCPiece piece)
    {
        if (bot == IntPtr.Zero)
        {
            return;
        }
        ColdClearNative.cc_add_next_piece_async(bot, piece);
    }

    private void AddPieceQueue()
    {
        if (pieceCounter == 6)
        {
            pieceCounter = 0;
            CCPiece[] queue = BoardToColdClear.instance.GetQueue();
            for (int i = 0; i < queue.Length; i++)
            {
                AddNewPiece(queue[i]);
            }
        }
    }
    private uint GetBagRemain()
    {
        // 取得目前 Bag 內剩下的 pieces
        List<Tetromino> bagInfo = board.bag.GetBag();
        uint bag_remain = 0;
        for (int i = 1; i < bagInfo.Count; i++)
        {
            bag_remain |= 1u << (int)BoardToColdClear.instance.TetrominoToCCPiece(bagInfo[i]);
        }
        return bag_remain;
    }

    private void FillMovementQueue()
    {
        //debugger
        string moveStr = $"[Cold Clear] hold : {move.hold} movement_count : {move.movement_count} nodes : {move.nodes} depth : {move.depth} movement : ";
        for (int i = 0; i < move.movement_count; i++)
        {
            moveStr += $"{move.movements[i]} ";
        }
        for (int i = 0; i < 4; i++)
        {
            moveStr += $"x:{move.expected_x[i]} y:{move.expected_y[i]} ";
        }
  
        Debug.Log(moveStr);

        //make queue
        if (move.hold)
        {
            movementQueue.Add(Movement.HOLD);
        }

        for (int i = 0; i < move.movement_count; i++)
        {
            movementQueue.Add(moveMap[move.movements[i]]);
        }
        movementQueue.Add(Movement.HARDDROP);
    }


    // on destroy

    void OnDestroy()
    {
        if (bot != IntPtr.Zero)
        {
            ColdClearNative.cc_destroy_async(bot);
            bot = IntPtr.Zero;
        }
    }
}

