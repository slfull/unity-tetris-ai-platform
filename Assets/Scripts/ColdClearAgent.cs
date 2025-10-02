using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditorInternal;
using UnityEngine;

public class ColdClearAgent : MonoBehaviour
{
    [SerializeField] private float agentSpeed = 0.5f;
    public IntPtr bot { get; private set; } = IntPtr.Zero;
    private Board board;
    private CCMove move;
    private CCBotPollStatus status;
    private float agentStepTime = 0;
    private float waitingTimeInterval = 0.1f;
    private float waitingTime = 0f;
    private bool isRequest = false;
    public int pieceCounter = 0;
    private bool needReset = false;
    private List<CCPiece> bagRemain;
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
        bagRemain = new List<CCPiece>()
        {
            CCPiece.CC_I, CCPiece.CC_O, CCPiece.CC_T, CCPiece.CC_L, CCPiece.CC_J, CCPiece.CC_S, CCPiece.CC_Z
        };
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

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            needReset = true;
        }


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
            if (movementQueue.Count > 0)
            {
                movementQueue.RemoveAt(0);
            }
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

        foreach (var piece in queue)
        {
            bagRemain.Remove(piece);
        }

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
        options.use_hold = false;
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
            if (needReset)
            {
                ResetBot();
                needReset = false;
                Debug.Log("[Cold Clear] Bot Reset");
                pieceCounter = 0;
                return;
            }
            pieceCounter = 0;
            CCPiece[] queue = BoardToColdClear.instance.GetQueue();
            List<CCPiece> tempList = new List<CCPiece>();
            for (int i = 0; i < queue.Length; i++)
            {
                if (bagRemain.Count == 0)
                {
                    RefillBagRemain();
                }
                if (bagRemain.Contains(queue[i]))
                {
                    bagRemain.Remove(queue[i]);
                }
                else
                {
                    tempList.Add(queue[i]);
                }
                AddNewPiece(queue[i]);
            }

            if (bagRemain.Count == 0)
            {
                RefillBagRemain();
            }
            if (tempList.Count > 0)
            {
                foreach (var piece in tempList)
                {
                    bagRemain.Remove(piece);
                }
            }
        }
    }

    private void RefillBagRemain()
    {
        bagRemain = new List<CCPiece>()
        {
            CCPiece.CC_I, CCPiece.CC_O, CCPiece.CC_T, CCPiece.CC_L, CCPiece.CC_J, CCPiece.CC_S, CCPiece.CC_Z
        };
    }
    private uint GetBagRemain()
    {
        uint bag_remain = 0;
        for (int i = 0; i < bagRemain.Count; i++)
        {
            bag_remain |= 1u << (int)bagRemain[i];
        }
        return bag_remain;
    }

    private void ResetBot()
    {
        if (bot != IntPtr.Zero)
        {
            ColdClearNative.cc_destroy_async(bot);
            bot = IntPtr.Zero;
        }

        bool[] field = BoardToColdClear.instance.GetFieldBoolArray();
        byte[] fieldBytes = new byte[field.Length];
        for (int i = 0; i < field.Length; i++)
        {
            fieldBytes[i] = (byte)(field[i] ? 1 : 0);
        }
        IntPtr fieldPtr = Marshal.AllocHGlobal(fieldBytes.Length);
        Marshal.Copy(fieldBytes, 0, fieldPtr, fieldBytes.Length);

        CCPiece? hold = BoardToColdClear.instance.GetHoldPiece();
        IntPtr holdPtr = IntPtr.Zero;
        if (hold.HasValue)
        {
            holdPtr = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(holdPtr, (int)hold.Value);
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
            {
                Marshal.WriteInt32(queuePtr + i * sizeof(int), (int)queue[i]);
            }
        }
        uint bag_remain = GetBagRemain();

        Debug.Log($"ResetBot queue: {string.Join(",", queue)}");
        Debug.Log($"ResetBot bag_remain: {Convert.ToString(bag_remain, 2)}");

        bot = ColdClearNative.cc_launch_with_board_async(ref options, ref weights, IntPtr.Zero,
            fieldPtr, bag_remain, holdPtr, board.b2b, (uint)board.combo, queuePtr, queueCount
        );

        Debug.Log($"queueCount={queue.Length}");
        for (int i = 0; i < queue.Length; i++)
            Debug.Log($"queue[{i}]={queue[i]}");

        if (holdPtr != IntPtr.Zero) Marshal.FreeHGlobal(holdPtr);
        if (queuePtr != IntPtr.Zero) Marshal.FreeHGlobal(queuePtr);
        isRequest = false;
        status = CCBotPollStatus.CC_WAITING;
        pieceCounter = 0;
        movementQueue.Clear();
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

