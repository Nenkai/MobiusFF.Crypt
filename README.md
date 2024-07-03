# MobiusFF.Crypt

Decrypts/Encrypts Mobius Final Fantasy's `Assembly-CSharp.dll` & decrypts `.dat` files.

## Usage

Drag-drop `Assembly-CSharp.dll` or `.dat` files. `mainData` must be present in the parent directory for the assembly file (encryption key depends on hash of it).

## Running decrypted `Assembly-CSharp.dll`

Open `mobiusff.exe` with a hex-editor, replace the first `C6 45 60 01` with `C6 45 60 00`. Since encryption is enforced, this will simply skip it.

If you'd like to use dnSpyEx *and* debug, follow [this](https://github.com/dnSpyEx/dnSpy/wiki/Debugging-Unity-Games) and grab [unity-5.0.1.rar here](https://github.com/wh0am15533/Patched-Unity-Mono/tree/main/CustomBuilds/Unity-debugging-5.x). Use `win64`.

Entrypoint is `MainLoop.Start`.

## Documentation & Additional notes

ALL documentation for all steps is in the code.

If the game is restarting when booted outside steam this is because `SteamAPI.RestartAppIfNecessary` is called inside `Mevius.Steam.SteamManager.Awake`.

Debug menu code is for most part stripped.

`SystemConfig` reading is stripped, but if re-implemented could allow some internal game settings.

## Credits

Archival data retrieved from [archive.org](https://archive.org/details/@mobius_final_fantasy_backup).
