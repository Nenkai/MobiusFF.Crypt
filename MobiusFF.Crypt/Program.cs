using System.Runtime;
using System.Security.Cryptography;
using System.Text;

namespace MobiusFF.Crypt;

internal class Program
{
    const string MainDataFileName = "mainData";
    const int CryptStart = 0x2EC;

    public static string AesIV;
    public static string AesKey;

    static void Main(string[] args)
    {
        Console.WriteLine("MobiusFF.Crypt by Nenkai");
        Console.WriteLine("- https://github.com/Nenkai");
        Console.WriteLine("- https://twitter.com/Nenkaai");

        if (args.Length != 1)
        {
            Console.WriteLine("Usage: <Assembly-CSharp.dll file or .dat files>");
            return;
        }

        string file = args[0];
        if (file.EndsWith(".dll") || file.EndsWith(".dec"))
        {
            DecryptAssembly(file);
        }
        else if (file.EndsWith(".dat"))
        {
            /* For Key + IV:
             * Mevius.App.Api.AesKey is used for file decryption, which is fetched at boot on the C# side (MainLoop.Start), and NativePlugin.getCryKey2(MainLoop._i.key) is called
             * MainLoop._i.key is set through mainData asset, 0xE24BC496 or 96 C4 4B E2
             * This calls a native lib (mobiusff_Data/Plugins/NativePlugin.dll)
             * 
             * On the native side, getCryKey2 returns the key/iv, but it's encrypted to start with so decrypt it.
             */

            byte[] keyIvBytes = NativePlugin.getCryKey2(0xE24BC496);
            string[] spl = Encoding.ASCII.GetString(keyIvBytes).TrimEnd('\0') // Marshal.PtrToStringAnsi
                .Split(',');

            // Both of these are used as "account key" (NetworkManager.accountKey/NetworkManager.accountIV) - also used for account data encryption?
            AesIV = spl[0]; // Mevius.App.Api.AesIV - Not used for file decryption, only used for "Old" account data decryption
            AesKey = spl[1]; // Mevius.App.Api.AesKey

            byte[] binary = File.ReadAllBytes(file);
            byte[] decrypted = Api.Decrypt(binary);

            File.WriteAllBytes(file + ".dec", decrypted);
        }
        else
        {
            Console.WriteLine("ERROR: Unknown file type");
            return;
        }
    }

    /* Useful Sigs (mobiusff.exe):
    * 
    * [Encryption Check]
    *   The game checks for dll extension for whether the file is encrypted or not
    *   You can disable the check at:
    *   - C6 45 ?? ?? 49 83 FC (or C6 45 60 01, first occurence)
    * 
    *   Set [C6 45 60 01] to [C6 45 60 00] 
    *   That simply sets the "isEncrypted" variable to false instead of true even if it's a dll
    *   
    * [Load Routine]
    *   FileLoadRoutine(void*, void*, char* assemblyPath)
    *   - 48 89 5C 24 ?? 4C 89 44 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 48 8B D9
    *     Called from MonoManager::LoadAssemblies. If the file ends with .dll, encryption occurs
    *   
    * [Decryption routine]
    *   - 48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 54 41 55 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 4C 8B 0D
    *     Called from above. Inits the key by opening and hashing mainData file. Decrypts from 0x2EC of assembly to end.
    *   
    * [Hashing]
    *   SHA1Hash(byte* toHash, int dataLEn, void* output)
    *   - 45 33 C9 E9 ?? ?? ?? ?? CC CC CC CC CC CC CC CC 48 83 EC
    *     Title. Function that gets called can also do MD5 but SHA1 is used (param 4 is 0).
    */
    static void DecryptAssembly(string file)
    {
        if (!File.Exists(file))
        {
            Console.WriteLine("ERROR: .dll file does not exist");
            return;
        }

        string currentDir = Path.GetDirectoryName(file)!;
        string parentDir = Directory.GetParent(currentDir).FullName;

        if (!File.Exists(Path.Combine(parentDir, MainDataFileName)))
        {
            Console.WriteLine($"ERROR: 'mainData' file not found in parent directory '{parentDir}'.");
            return;
        }

        // Key is based off hash of mainData.
        byte[] mainData = File.ReadAllBytes(Path.Combine(parentDir, MainDataFileName));

        // Key is essentially two identical SHA1 keys, except the second one has a byte swap
        byte[] fullKey = new byte[SHA1.HashSizeInBytes * 2];
        SHA1.HashData(mainData, fullKey);
        fullKey.AsSpan(0x00, SHA1.HashSizeInBytes).CopyTo(fullKey.AsSpan(SHA1.HashSizeInBytes));
        fullKey[fullKey[0] % fullKey.Length] = 0; // First byte determines which byte should be zero'ed, sneaky..

        // Decrypt.
        byte[] assembly = File.ReadAllBytes(file);
        bool isEncrypted = IsEncryptedAssembly(assembly);
        for (int i = CryptStart; i < assembly.Length; i++)
        {
            int idx = (i - CryptStart) % fullKey.Length;
            if (assembly[i] == 0 || (assembly[i] ^ fullKey[idx]) == 0) // xor into 0 is not allowed
                continue;

            assembly[i] ^= fullKey[idx];
        }

        if (isEncrypted)
            Console.WriteLine($"OK: Decrypted assembly '{file}'.");
        else
            Console.WriteLine($"OK: Encrypted assembly '{file}'.");

        File.WriteAllBytes(file, assembly);

    }

    public static bool IsEncryptedAssembly(byte[] assemblyBytes)
    {
        // Surprisingly works
        return assemblyBytes.AsSpan().IndexOf(Encoding.ASCII.GetBytes("Mevius")) == -1;
    }
}
