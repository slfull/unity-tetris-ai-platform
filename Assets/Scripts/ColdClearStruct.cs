using System;
using System.Runtime.InteropServices;

public enum CCPiece
{
    CC_I, CC_O, CC_T, CC_L, CC_J, CC_S, CC_Z
}

public enum CCTspinStatus
{
    CC_NONE_TSPIN_STATUS,
    CC_MINI,
    CC_FULL,
}

public enum CCMovement
{
    CC_LEFT, CC_RIGHT,
    CC_CW, CC_CCW,
    CC_DROP
}

public enum CCMovementMode
{
    CC_0G,
    CC_20G,
    CC_HARD_DROP_ONLY
}

public enum CCSpawnRule
{
    CC_ROW_19_OR_20,
    CC_ROW_21_AND_FALL,
}
public enum CCBotPollStatus
{
    CC_MOVE_PROVIDED,
    CC_WAITING,
    CC_BOT_DEAD
}
public enum CCPcPriority
{
    CC_PC_OFF,
    CC_PC_FASTEST,
    CC_PC_ATTACK
}

[StructLayout(LayoutKind.Sequential)]
public struct CCPlanPlacement
{
    public CCPiece piece;
    public CCTspinStatus tspin;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] expected_x;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] expected_y;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public int[] cleared_lines;
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
    public CCMovement[] movements;

    public uint nodes;
    public uint depth;
    public uint original_rank;
}

[StructLayout(LayoutKind.Sequential)]
public struct CCOptions
{
    public CCMovementMode mode;
    public CCSpawnRule spawn_rule;
    public CCPcPriority pcloop;
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

