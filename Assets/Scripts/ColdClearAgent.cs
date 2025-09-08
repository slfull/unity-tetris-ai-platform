using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ColdClearAgent : MonoBehaviour
{
    private IntPtr bot;

    void Start()
    {
        // 預設設定
        CCOptions options;
        ColdClearNative.cc_default_options(out options);
        CCWeights weights;
        ColdClearNative.cc_default_weights(out weights);

        // 啟動 bot
        bot = ColdClearNative.cc_launch_async(ref options, ref weights, IntPtr.Zero, null, 0);
    }

    void Update()
    {
        if (bot == IntPtr.Zero) return;

        // 請求下一步
        ColdClearNative.cc_request_next_move(bot, 0);

        // 嘗試取得結果
        CCMove move;
        var status = ColdClearNative.cc_poll_next_move(bot, out move, IntPtr.Zero, IntPtr.Zero);
        if (status == CCBotPollStatus.CC_MOVE_PROVIDED)
        {
            Debug.Log($"ColdClear 建議: hold={move.hold}, x={move.expected_x[0]}, y={move.expected_y[0]}");
        }
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
