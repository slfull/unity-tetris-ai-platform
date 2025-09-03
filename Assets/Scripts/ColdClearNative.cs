using System;
using System.Runtime.InteropServices;
internal static class ColdClearNative
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    const string Dll = "cold_clear";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    const string Dll = "cold_clear";
#else
    const string Dll = "cold_clear";
#endif

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr cc_launch_async(ref CCOptions options, ref CCWeights weights,
        IntPtr book, CCPiece[] queue, uint count);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void cc_destroy_async(IntPtr bot);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void cc_request_next_move(IntPtr bot, uint incoming);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern CCBotPollStatus cc_poll_next_move(
        IntPtr bot,
        out CCMove move,
        IntPtr plan,
        IntPtr plan_length
    );

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void cc_default_options(out CCOptions options);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void cc_default_weights(out CCWeights weights);
}
