using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class ColdClearAgent : MonoBehaviour
{
    private IntPtr bot = IntPtr.Zero;
    private Board board;

    void Start()
    {
        // 取得棋盤資料
        bool[] field = BoardToColdClear.instance.GetFieldBoolArray();
        CCPiece[] queue = BoardToColdClear.instance.GetQueue();
        CCPiece? hold = BoardToColdClear.instance.GetHoldPiece();

        // 計算 bag_remain（包含 queue 內所有 piece）
        uint bag_remain = 0;
        foreach (var p in queue)
            bag_remain |= 1u << (int)p;

        // 其他狀態
        bool b2b = false;
        uint combo = 0;

        // 預設設定
        CCOptions options;
        CCWeights weights;
        ColdClearNative.cc_default_options(out options);
        ColdClearNative.cc_default_weights(out weights);

        // 準備 field 指標
        IntPtr fieldPtr = Marshal.AllocHGlobal(field.Length);
        byte[] fieldBytes = new byte[field.Length];
        for (int i = 0; i < field.Length; i++)
            fieldBytes[i] = (byte)(field[i] ? 1 : 0);
        Marshal.Copy(fieldBytes, 0, fieldPtr, fieldBytes.Length);

        // 準備 hold 指標
        IntPtr holdPtr = IntPtr.Zero;
        if (hold.HasValue)
        {
            holdPtr = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(holdPtr, (int)hold.Value);
        }

        // 準備 queue 指標
        IntPtr queuePtr = IntPtr.Zero;
        uint queueCount = (uint)queue.Length;
        if (queueCount > 0)
        {
            queuePtr = Marshal.AllocHGlobal(sizeof(int) * queue.Length);
            for (int i = 0; i < queue.Length; i++)
                Marshal.WriteInt32(queuePtr + i * sizeof(int), (int)queue[i]);
        }

        // 啟動 bot
        bot = ColdClearNative.cc_launch_with_board_async(
            ref options, ref weights, IntPtr.Zero,
            fieldPtr, bag_remain, holdPtr, b2b, combo, queuePtr, queueCount
        );

        // 釋放 unmanaged 記憶體
        Marshal.FreeHGlobal(fieldPtr);
        if (holdPtr != IntPtr.Zero) Marshal.FreeHGlobal(holdPtr);
        if (queuePtr != IntPtr.Zero) Marshal.FreeHGlobal(queuePtr);

        Debug.Log($"field.Length={field.Length}, queueCount={queueCount}, bag_remain={Convert.ToString(bag_remain, 2)}");
        for (int i = 0; i < queue.Length; i++)
            Debug.Log($"queue[{i}]={queue[i]}");
        Debug.Log($"hold={hold}");

        CCMove? move = RequestNextMove();
        board = GetComponent<Board>();
    }

    void Update()
    {
        
    }

    public CCMove? RequestNextMove()
    {
        if (bot == IntPtr.Zero) return null;

        // 請求下一步
        ColdClearNative.cc_request_next_move(bot, 0);

        CCMove move;
        var status = ColdClearNative.cc_poll_next_move(bot, out move, IntPtr.Zero, IntPtr.Zero);

        if (status == CCBotPollStatus.CC_MOVE_PROVIDED)
        {
            string moveStr = $"hold={move.hold}, x={move.expected_x[0]}, y={move.expected_y[0]}, movement_count={move.movement_count}, nodes={move.nodes}";
            moveStr += "\nmovements: ";
            for (int i = 0; i < move.movement_count; i++)
            {
                moveStr += move.movements[i] + " ";
            }
            Debug.Log("[ColdClear] " + moveStr);
            return move;
        }
        Debug.LogWarning("[ColdClear] Bot dead.");
        return null;
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

