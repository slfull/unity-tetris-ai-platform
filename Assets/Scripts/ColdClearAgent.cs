using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ColdClearAgent : MonoBehaviour
{
    private IntPtr bot = IntPtr.Zero;
    private Board board;
    private Piece activePiece;
    private bool isPrevMoveHold = false;
    private int prevScore;
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
    }

    void Update()
    {
        HandleMovement();
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
        else
        {
            MovementQueueRequest();
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
        // 取得棋盤狀態
        bool[] field = BoardToColdClear.instance.GetFieldBoolArray();
        byte[] fieldBytes = new byte[field.Length];
        for (int i = 0; i < field.Length; i++)
            fieldBytes[i] = (byte)(field[i] ? 1 : 0);

        // 分配 unmanaged 記憶體（400 bytes）
        IntPtr fieldPtr = Marshal.AllocHGlobal(fieldBytes.Length);

        // 複製資料
        Marshal.Copy(fieldBytes, 0, fieldPtr, fieldBytes.Length);

        // 呼叫 API
        ColdClearNative.cc_reset_async(bot, fieldPtr, false, 0);

        // 釋放記憶體
        Marshal.FreeHGlobal(fieldPtr);

        // 加入新 piece 到 queue
        CCPiece newestPiece = BoardToColdClear.instance.GetNewestPiece();
        ColdClearNative.cc_add_next_piece_async(bot, newestPiece);
    }

    public void BotInitialize()
    {

        if(bot != IntPtr.Zero)
        {
            ColdClearNative.cc_destroy_async(bot);
            bot = IntPtr.Zero;
        }
        

        CCPiece[] queue = BoardToColdClear.instance.GetQueue();

        CCOptions options;
        CCWeights weights;
        ColdClearNative.cc_default_options(out options);
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
            queue, queueCount
        );
        if (queuePtr != IntPtr.Zero) Marshal.FreeHGlobal(queuePtr);
    }

    public CCMove? RequestNextMove()
    {
        if (bot == IntPtr.Zero) return null;

        // 請求下一步
        ColdClearNative.cc_request_next_move(bot, 0);

        CCMove move;

        var status = ColdClearNative.cc_poll_next_move(bot, out move, IntPtr.Zero, IntPtr.Zero);
        string moveSt = $"hold={move.hold}, x={move.expected_x[0]}, y={move.expected_y[0]}, movement_count={move.movement_count}, nodes={move.nodes}";
        moveSt += "\nmovements: ";
        for (int i = 0; i < move.movement_count; i++)
        {
            moveSt += move.movements[i] + " ";
        }
        Debug.Log("[ColdClear] Prev Move" + moveSt);

        if (status == CCBotPollStatus.CC_MOVE_PROVIDED)
        {
            string moveStr = $"hold={move.hold}, x={move.expected_x[0]}, y={move.expected_y[0]}, movement_count={move.movement_count}, nodes={move.nodes}";
            moveStr += "\nmovements: ";
            for (int i = 0; i < move.movement_count; i++)
            {
                moveStr += move.movements[i] + " ";
            }
            Debug.Log("[ColdClear] " + moveStr);

            isPrevMoveHold = move.hold;



            return move;
        }
        Debug.LogWarning("[ColdClear] Bot dead.");
        return null;
    }

    private void ResetBot()
    {
        // 先銷毀舊 bot
        if (bot != IntPtr.Zero)
        {
            ColdClearNative.cc_destroy_async(bot);
            bot = IntPtr.Zero;
        }

        // 重新取得最新資料
        bool[] field = BoardToColdClear.instance.GetFieldBoolArray();
        CCPiece[] queue = BoardToColdClear.instance.GetQueue();
        CCPiece? hold = BoardToColdClear.instance.GetHoldPiece();

        uint bag_remain = 0;
        foreach (var p in queue)
            bag_remain |= 1u << (int)p;

        bool b2b = false;
        uint combo = 0;

        CCOptions options;
        CCWeights weights;
        ColdClearNative.cc_default_options(out options);
        ColdClearNative.cc_default_weights(out weights);

        IntPtr fieldPtr = Marshal.AllocHGlobal(field.Length);
        byte[] fieldBytes = new byte[field.Length];
        for (int i = 0; i < field.Length; i++)
            fieldBytes[i] = (byte)(field[i] ? 1 : 0);
        Marshal.Copy(fieldBytes, 0, fieldPtr, fieldBytes.Length);

        IntPtr holdPtr = IntPtr.Zero;
        if (hold.HasValue)
        {
            holdPtr = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(holdPtr, (int)hold.Value);
        }

        IntPtr queuePtr = IntPtr.Zero;
        uint queueCount = (uint)queue.Length;
        if (queueCount > 0)
        {
            queuePtr = Marshal.AllocHGlobal(sizeof(int) * queue.Length);
            for (int i = 0; i < queue.Length; i++)
                Marshal.WriteInt32(queuePtr + i * sizeof(int), (int)queue[i]);
        }

        // 重啟 bot
        bot = ColdClearNative.cc_launch_with_board_async(
            ref options, ref weights, IntPtr.Zero,
            fieldPtr, bag_remain, holdPtr, b2b, combo, queuePtr, queueCount
        );

        Marshal.FreeHGlobal(fieldPtr);
        if (holdPtr != IntPtr.Zero) Marshal.FreeHGlobal(holdPtr);
        if (queuePtr != IntPtr.Zero) Marshal.FreeHGlobal(queuePtr);

        Debug.Log("[ColdClear] Bot reset.");
    }

    private void MovementQueueRequest()
    {

        if (IsNeedToResetBot())
        {
            //ResetBot();
        }
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

    void OnDestroy()
    {
        if (bot != IntPtr.Zero)
        {
            ColdClearNative.cc_destroy_async(bot);
            bot = IntPtr.Zero;
        }
    }
}

