using System;
using System.Diagnostics;
using System.IO;

namespace ViruBoy
{
    public class Cartridge
    {
        public enum CartridgeType
        {
            Rom = 0x0,
            Mbc1 = 0x1,
            Mbc1Ram = 0x2,
            Mbc1RamBattery = 0x3,
            Mbc2 = 0x5,
            Mbc2Battery = 0x6,
            Ram = 0x8,
            RamBattery = 0x9,
            Mmm1 = 0xB,
            Mmm1Sram = 0xC,
            Mmm1SramBattery = 0xD,
            Mbc3TimerBattery = 0xF,
            Mbc3TimeRamBattery = 0x10,
            Mbc3 = 0x11,
            Mbc3Ram = 0x12,
            Mbc3RamBattery = 0x13,
            Mbc5 = 0x19,
            Mbc5Ram = 0x1A,
            Mbc5RamBattery = 0x1B,
            Mbc5Rumble = 0x1C,
            Mbc5RumbleSram = 0x1D,
            Mbc5RumbleSramBattery = 0x1E,
            PocketCamera = 0x1F, // If you actually somehow connect this or anything below this you need to rethink life decisions
            BandaiTAMA5 = 0xFD,
            HudsonHuC3 = 0xFE,
            HudsonHuC1 = 0xFF
        }

        public enum OldLicenseeCode
        {
            Other = 0x33,
            Accolade = 0x79,
            Konami = 0xA4
        }

        public enum RamSize
        {
            None = 0,
            Kb2 = 1,
            Kb8 = 2,
            Kb4X8 = 3,
            Kb16X8 = 4,
            Kb8X8 = 5
        }

        public const ushort CodeStartAddress = 0x100;
        public const ushort NintendoGraphicsAddress = 0x104;
        public const ushort GameNameAddress = 0x134;
        public const ushort GameBoyColorSupportAddress = 0x143;
        public const ushort NewLicenseeCodeAddress = 0x144;
        public const ushort CartridgeTypeAddress = 0x147;
        public const ushort CartridgeRomSizeAddress = 0x148;
        public const ushort CartridgeRamSizeAddress = 0x149;
        public const ushort DestinationCodeAddress = 0x14A;
        public const ushort OldLicenseeCodeAddress = 0x14B;
        public const ushort MaskRomVersionAddress = 0x14C;
        public const ushort ComplementCheckAddress = 0x14D;
        public const ushort CheckSumAddress = 0x14E;

        public const ushort RomBankSize = 0x4000;
        public const ushort RamBankSize = 0x2000;

        public const ushort Mbc1RamEnable = 0x0000;
        public const ushort Mbc1RomBankSelect = 0x2000;
        public const ushort Mbc1RamBankSelectOrRomBankUpperSelect = 0x4000;
        public const ushort Mbc1RomRamModeSelect = 0x6000;

        public byte[] Bank0 => _banks[0];
        public byte[] SwitchBank => _banks[SelectedRomBank];
        public byte[] RamBank => _ramBanks[SelectedRamBank];

        public byte RomBankCount { get; private set; } = 0;
        public uint RomSize { get; private set; } = 0;
        
        public bool RamEnabled { get; private set; } = false;
        public bool RomModeSelected { get; private set; } = true;
        public byte SelectedRomBank { get; private set; } = 1;
        public byte SelectedRamBank { get; private set; } = 0;

        public byte RamBankCount
        {
            get
            {
                switch (Bank0[CartridgeRamSizeAddress])
                {
                    case 0:
                        return 0;
                    case 1:
                        return 1;
                    case 2:
                        return 1;
                    case 3:
                        return 4;
                    case 4:
                        return 16;
                    default:
                        throw new InvalidDataException();
                }
            }
        }

        public string GameName => System.Text.Encoding.ASCII.GetString(Bank0, GameNameAddress, 16);
        public bool IsColorGame => Bank0[GameBoyColorSupportAddress] == 0x80;

        public bool IsJapanese => Bank0[DestinationCodeAddress] == 0x0; // TODO: Maybe check if it isnt random garbo?

        public CartridgeType Type => (CartridgeType)Bank0[CartridgeTypeAddress];

        public static readonly byte[] ScrollingGraphic = { 0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B, 0x03, 0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D, 0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E, 0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99, 0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC, 0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E }; // TODO: This is why I need an ultrawide

        private readonly byte[][] _banks;
        private readonly byte[][] _ramBanks;

        public Cartridge()
        {
            _banks = new byte[1][];
            _banks[0] = new byte[RomBankSize];

            RomSize = RomBankSize;
            RomBankCount = 1;
        }

        public Cartridge(string fileName)
        {
            if (File.Exists(fileName + ".gb")) fileName = fileName + ".gb";
            else if (!File.Exists(fileName)) throw new FileNotFoundException();

            RomSize = (uint)new FileInfo(fileName).Length;
            RomBankCount = (byte)(RomSize / RomBankSize);
            _banks = new byte[RomBankCount][];

            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                for (int i = 0; i < RomBankCount; i++)
                {
                    _banks[i] = new byte[RomBankSize];
                    stream.Read(_banks[i], 0, RomBankSize);
                }

                if (!CheckGraphics()) throw new InvalidDataException();
                Debug.WriteLine("Nintendo graphic matches");

                if (!CheckRomSize()) throw new InvalidDataException();
                Debug.WriteLine("Rom size matches supplied file");

                Debug.WriteLine("Loading game: " + GameName);

                if (IsColorGame) Debug.WriteLine("Detected GameBoy Color game");
                else Debug.WriteLine("Detected GameBoy non-color game");
            }

            RamSize ramSize = (RamSize)Bank0[CartridgeRamSizeAddress];

            switch (ramSize)
            {
                case RamSize.None:
                    break;
                case RamSize.Kb2:
                    _ramBanks = new byte[1][];
                    _ramBanks[1] = new byte[RamBankSize / 4];
                    break;
                case RamSize.Kb8:
                    _ramBanks = new byte[1][];
                    _ramBanks[1] = new byte[RamBankSize];
                    break;
                case RamSize.Kb4X8:
                    _ramBanks = new byte[4][];
                    for (int i = 0; i < 4; i++) _ramBanks[i] = new byte[RamBankSize];
                    break;
                case RamSize.Kb16X8:
                    _ramBanks = new byte[16][];
                    for (int i = 0; i < 16; i++) _ramBanks[i] = new byte[RamBankSize];
                    break;
                case RamSize.Kb8X8:
                    _ramBanks = new byte[8][];
                    for (int i = 0; i < 8; i++) _ramBanks[i] = new byte[RamBankSize];
                    break;
                default:
                    throw new InvalidDataException("RAM size outside supported range");
            }
        }

        public void WriteMemory(ushort address, byte data)
        {
            // TODO: Ram writing is done in the CPU, maybe move?
            switch (Type)
            {
                case CartridgeType.Rom:
                    Debug.WriteLine("Attempted to change memory bank on ROM only");
                    return;
                case CartridgeType.Mbc1:
                    if (address >= Mbc1RomRamModeSelect)
                    {
                        if (data == 0) RomModeSelected = true;
                        else if (data == 1) RomModeSelected = false;
                        else throw new ArgumentOutOfRangeException(nameof(data), "Attempted to change ROM/RAM mode to an unsupported value");
                    }
                    else if (address >= Mbc1RamBankSelectOrRomBankUpperSelect)
                    {
                        if (RomModeSelected)
                        {
                            SelectedRomBank = (byte) ((SelectedRomBank & 0b0001_1111) | (data << 5));
                        }
                        else
                        {
                            SelectedRamBank = (byte)(data & 0b0000_0011);
                        }
                    }
                    else if (address >= Mbc1RomBankSelect)
                    {
                        SelectedRomBank = (byte)((SelectedRomBank & 0b1110_0000) | data);
                        if (SelectedRomBank == 0) SelectedRomBank = 1;
                        else if (SelectedRomBank == 20) SelectedRomBank = 21;
                        else if (SelectedRomBank == 40) SelectedRomBank = 41;
                        else if (SelectedRomBank == 60) SelectedRomBank = 61;
                    }
                    else
                    {
                        RamEnabled = (data & 0x0A) == 0x0A;
                    }
                    return;
                default:
                    Debug.WriteLine("Attempted to change memory bank on a unsupported cartridge");
                    return;
            }
        }

        private bool CheckGraphics()
        {
            for (int i = 0; i < ScrollingGraphic.Length; i++)
            {
                if (ScrollingGraphic[i] != Bank0[i + NintendoGraphicsAddress]) return false;
            }

            return true;
        }

        private bool CheckRomSize()
        {
            byte romSize = Bank0[CartridgeRomSizeAddress];

            switch (romSize)
            {
                case 0:
                    return RomBankCount == 2;
                case 1:
                    return RomBankCount == 4;
                case 2:
                    return RomBankCount == 8;
                case 3:
                    return RomBankCount == 16;
                case 4:
                    return RomBankCount == 32;
                case 5:
                    return RomBankCount == 64;
                case 6:
                    return RomBankCount == 128;
                case 0x52:
                    return RomBankCount == 72;
                case 0x53:
                    return RomBankCount == 80;
                case 0x54:
                    return RomBankCount == 96;
                default:
                    return false;
            }
        }
    }
}