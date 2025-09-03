using System;
using System.Runtime.InteropServices;

public enum CCPiece
{
    I, O, T, L, J, S, Z
}

public enum CCBotPollStatus
{
    MOVE_PROVIDED,
    WAITING,
    BOT_DEAD
}

[StructLayout(LayoutKind.Sequential)]
public struct CCMove
{
    [MarshalAs(UnmanagedType.I1)]
    public bool hold;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] expected_x;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] expected_y;

    public byte movement_count;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public int[] movements;

    public uint nodes;
    public uint depth;
    public uint original_rank;
}

[StructLayout(LayoutKind.Sequential)]
public struct CCOptions
{
    public int mode;
    public int spawn_rule;
    public int pcloop;
    public uint min_nodes;
    public uint max_nodes;
    public uint threads;

    [MarshalAs(UnmanagedType.I1)]
    public bool use_hold;

    [MarshalAs(UnmanagedType.I1)]
    public bool speculate;
}

[StructLayout(LayoutKind.Sequential)]
public struct CCWeights
{
    public int back_to_back;
    public int bumpiness;
    public int bumpiness_sq;
    public int row_transitions;
    public int height;
    public int top_half;
    public int top_quarter;
    public int jeopardy;
    public int cavity_cells;
    public int cavity_cells_sq;
    public int overhang_cells;
    public int overhang_cells_sq;
    public int covered_cells;
    public int covered_cells_sq;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public int[] tslot;

    public int well_depth;
    public int max_well_depth;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public int[] well_column;

    public int b2b_clear;
    public int clear1;
    public int clear2;
    public int clear3;
    public int clear4;
    public int tspin1;
    public int tspin2;
    public int tspin3;
    public int mini_tspin1;
    public int mini_tspin2;
    public int perfect_clear;
    public int combo_garbage;
    public int move_time;
    public int wasted_t;

    [MarshalAs(UnmanagedType.I1)]
    public bool use_bag;
    [MarshalAs(UnmanagedType.I1)]
    public bool timed_jeopardy;
    [MarshalAs(UnmanagedType.I1)]
    public bool stack_pc_damage;
}

