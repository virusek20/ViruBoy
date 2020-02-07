using System;

namespace ViruBoy
{
    [Flags]
    public enum FlagRegister : byte
    {
        N0 = 1,
        N1 = 2,
        N2 = 4,
        N3 = 8,
        Carry = 16,
        HalfCarry = 32,
        NSubstract = 64,
        Zero = 128
    }

    [Flags]
    public enum InterruptEnableRegister : byte
    {
        VBlank = 1,
        LCDC = 2,
        TimerOverflow = 4,
        SerialComplete = 8,
        TransitionHToLP10To13 = 16
    }

    [Flags]
    public enum P1 : byte
    {
        P10In = 1,
        P11In = 2,
        P12In = 4,
        P13In = 8,
        P14Out = 16,
        P15Out = 32
    }

    [Flags]
    public enum TAC : byte
    {
        ClockSelectSpeed1 = 1,
        ClockSelectSpeed2 = 2,
        TimeStop = 4
    }

    [Flags]
    public enum NR10 : byte
    {
        SweepShift1 = 1,
        SweepShift2 = 2,
        SweepShift3 = 4,
        SweepDecrease = 8, // 1 Decreases | 0 Increases
        SweepTime
    }

    [Flags]
    public enum LCDC : byte
    {
        BgAndWindowDisplay = 1,
        OBJDisplay = 2,
        OBJSize = 4, // 0 = 8x8 | 1 = 8x16
        BGTileMapDisplaySelect = 8, // 0 = 9800-9Bff | 1 = 9C00 - 9FFF
        BGWindowTileDataSelect = 16, // 0 = 8800 - 97FF | 1 = 8000 - 8FFF
        WindowDisplay = 32,
        WindowTileMapDisplaySelect = 64, // 0 = 98000 - 9BFF | 1 = 9C00 - 9FFF
        LCDControlOperation = 128
    }

    [Flags]
    public enum STAT : byte
    {
        ModeFlag0 = 1,
        ModeFlag1 = 2,
        CoincidenceFlag = 4,
        Mode00 = 8,
        Mode01 = 16,
        Mode10 = 32,
        LYCEqLYCoincidence = 64
    }

    [Flags]
    public enum BGP : byte
    {
        DotData00_0 = 1,
        DotData00_1 = 2,

        DotData01_0 = 4,
        DotData01_1 = 8,

        DotData10_0 = 16,
        DotData10_1 = 32,

        DotData11_0 = 64,
        DotData11_1 = 128
    }
}
