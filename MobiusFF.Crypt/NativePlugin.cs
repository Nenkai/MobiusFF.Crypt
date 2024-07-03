using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusFF.Crypt;

public class NativePlugin
{
    private static readonly byte[] cryKey =
[
      0xE0, 0x06, 0xA7, 0xC0, 0x8D, 0x1E, 0xB1, 0xBB, 0x94, 0x19, 0xAD, 0xDC, 0x81, 0x08, 0xB6, 0xB5,
      0xE8, 0x18, 0xB3, 0xC3, 0x85, 0x19, 0xA7, 0xBB, 0x81, 0x05, 0xAB, 0xCE, 0xE9, 0x09, 0xA6, 0xA7,
      0xE1, 0x4B,
    ];

    private static byte[] decCryKey = new byte[0x22];

    public static byte[] getCryKey2(uint value)
    {
        byte[] bytes = BitConverter.GetBytes(value);

        for (int i = 0; i < cryKey.Length; i++)
        {
            decCryKey[i] = (byte)(cryKey[i] ^ bytes[(i + 1) % 4]);
        }

        return decCryKey;
    }

    public static uint GetBattleScoreHash(int score, uint salt)
    {
        return (uint)~(score + 2 * (5 * salt + 5));
    }

    public static long GetBattleScoreHashLong(int score, uint salt)
    {
        return ~(score + 2 * (5 * salt + 5));
    }
}
