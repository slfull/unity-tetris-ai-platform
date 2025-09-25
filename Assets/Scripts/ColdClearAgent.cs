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
    private CCMove move;
    private CCBotPollStatus status;
    private bool isPrevMoveHold = false;
    private int prevScore;
    private float agentStepTime = 0;
    private float waitingTime = 0.1f;
    private bool isRequest = false;
    public enum Movement
    {
        LEFT, RIGHT,
        DROP, HARDDROP,
        CW, CCW,
        HOLD
    }
    [SerializeField] private List<Movement> movementQueue;

    //monoBhaviour

    void Start()
    {
        BotInitialize();
        board = GetComponent<Board>();
        agentStepTime = Time.time;
        status = CCBotPollStatus.CC_WAITING;
    }

    void Update()
    {
        if (Time.time >= agentStepTime)
        {
            HandleMovement();
            agentStepTime = Time.time + agentSpeed;
        }
    }

    //movement handle

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
        else if (status == CCBotPollStatus.CC_MOVE_PROVIDED)
        {
            FillMovementQueue();
            status = CCBotPollStatus.CC_WAITING;
            isRequest = false;
        }
        else if (status == CCBotPollStatus.CC_BOT_DEAD)
        {
            Debug.LogWarning("[Cold Clear] Bot Dead");
        }
        else
        {
            if (!isRequest)
            {
                Invoke("RequestNextMove", waitingTime);
            }
            Invoke("PollNextMove", waitingTime * 2);
            //Debug.Log("[Cold Clear] Bot Waiting");
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
        options.mode = CCMovementMode.CC_HARD_DROP_ONLY;
        options.spawn_rule = CCSpawnRule.CC_ROW_19_OR_20;
        options.pcloop = CCPcPriority.CC_PC_FASTEST;
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
        status = ColdClearNative.cc_poll_next_move(bot, out move, IntPtr.Zero, IntPtr.Zero);
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

        if (move.movement_count == 0)
        {
            Debug.LogWarning("[Cold Clear] movement count is 0");
            return;
        }

        for (int i = 0; i < move.movement_count; i++)
        {
            switch (move.movements[i])
            {
                case CCMovement.CC_LEFT: movementQueue.Add(Movement.LEFT); break;
                case CCMovement.CC_RIGHT: movementQueue.Add(Movement.RIGHT); break;
                case CCMovement.CC_CW: movementQueue.Add(Movement.CW); break;
                case CCMovement.CC_CCW: movementQueue.Add(Movement.CCW); break;
                case CCMovement.CC_DROP: movementQueue.Add(Movement.DROP); break;
                default: break;
            }
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

