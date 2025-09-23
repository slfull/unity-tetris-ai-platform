using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Edgegap.Editor.Api.Models.Results;
using UnityEngine;

public class ColdClearAgent : MonoBehaviour
{
    [SerializeField] private float agentSpeed = 0.5f;
    public IntPtr bot { get; private set; } = IntPtr.Zero;
    private Board board;
    private bool isPrevMoveHold = false;
    private int prevScore;
    private float agentStepTime = 0;
    public enum Movement
    {
        LEFT, RIGHT,
        DROP, HARDDROP,
        CW, CCW,
        HOLD
    }
    [SerializeField] private List<Movement> movementQueue;


    void Start()
    {
        BotInitialize();
        board = GetComponent<Board>();
        movementQueue = new List<Movement>();
        prevScore = board.score;
        MovementQueueRequest();
        agentStepTime = Time.time;
        board.activePiece.stepDelay = 100f;
    }

    void Update()
    {
        if (Time.time >= agentStepTime)
        {
            HandleMovement();
            agentStepTime = Time.time + agentSpeed;
        }
    }

    private void HandleMovement()
    {
        if (movementQueue.Count > 0)
        {
            switch (movementQueue[0])
            {
                case Movement.LEFT: MoveLeft(); break;
                case Movement.RIGHT: MoveRight(); break;
                case Movement.CW: ClockWise(); break;
                case Movement.CCW: CounterClockWise(); break;
                case Movement.HOLD: Hold(); break;
                case Movement.DROP: Drop(); break;
                case Movement.HARDDROP: HardDrop(); break;
                default: break;
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
        board.activePiece.NormalDrop();
    }

    private void HardDrop()
    {
        board.activePiece.HandleUpdateMove(3);
    }

    public void GetNewestPiece()
    {
        if (bot == IntPtr.Zero)
        {
            Debug.LogError("ColdClear bot not initialized!");
            return;
        }
        CCPiece newestPiece = BoardToColdClear.instance.GetNewestPiece();
        Debug.Log($"[ColdClear] Add next piece: {newestPiece}");
        ColdClearNative.cc_add_next_piece_async(bot, newestPiece);
        MovementQueueRequest();
    }

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

    public CCMove? RequestNextMove()
    {
        if (bot == IntPtr.Zero) return null;
        ColdClearNative.cc_request_next_move(bot, 0);
        CCMove move;

        CCBotPollStatus status = ColdClearNative.cc_poll_next_move(bot, out move, IntPtr.Zero, IntPtr.Zero);

        if (status == CCBotPollStatus.CC_MOVE_PROVIDED)
        {
            string moveStr = $"hold={move.hold}, movement_count={move.movement_count}, nodes={move.nodes}";
            moveStr += "\nmovements: ";
            for (int i = 0; i < move.movement_count; i++)
            {
                moveStr += move.movements[i] + " ";
            }

            moveStr += "\nexpected: ";

            for (int i = 0; i < move.expected_x.Length; i++)
            {
                moveStr += $"x={move.expected_x[i]}, y={move.expected_y[i]} | ";
            }
            Debug.Log("[ColdClear] " + moveStr);
            isPrevMoveHold = move.hold;
            return move;
        }
        Debug.LogWarning("[ColdClear] Bot dead.");
        return null;
    }

    private void MovementQueueRequest()
    {
        prevScore = board.score;

        CCMove? move = RequestNextMove();
        if (move == null)
        {
            return;
        }
        if (move.Value.hold)
        {
            movementQueue.Add(Movement.HOLD);
        }

        for (int i = 0; i < move.Value.movement_count; i++)
        {
            Movement movement = Movement.LEFT;

            switch (move.Value.movements[i])
            {
                case CCMovement.CC_LEFT: movement = Movement.LEFT; break;
                case CCMovement.CC_RIGHT: movement = Movement.RIGHT; break;
                case CCMovement.CC_CW: movement = Movement.CW; break;
                case CCMovement.CC_CCW: movement = Movement.CCW; break;
                case CCMovement.CC_DROP: movement = Movement.DROP; break;
                default: break;
            }
            movementQueue.Add(movement);
        }

        movementQueue.Add(Movement.HARDDROP);
    }

    private bool IsNeedToResetBot()
    {
        return isPrevMoveHold || prevScore != board.score;
    }

    private CCOptions DefaultOption()
    {
        CCOptions options;
        ColdClearNative.cc_default_options(out options);
        options.mode = CCMovementMode.CC_HARD_DROP_ONLY;
        options.spawn_rule = CCSpawnRule.CC_ROW_19_OR_20;
        options.pcloop = CCPcPriority.CC_PC_FASTEST;
        options.use_hold = false;
        return options;
    }

    void OnDestroy()
    {
        if (bot != IntPtr.Zero)
        {
            ColdClearNative.cc_destroy_async(bot);
            bot = IntPtr.Zero;
        }
    }
}

