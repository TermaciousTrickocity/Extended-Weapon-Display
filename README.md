# Extended weapon display overlay

Allows users to set their weapon display offsets well beyond the standard limits for all games within The Master Chief Collection.
- Install the [.NET 7.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-7.0.14-windows-x64-installer) and [.NET 7.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-7.0.14-windows-x64-installer) before trying to run/build.
- Launch The Master Chief Collection with EAC off.
- Use 'C' to toggle the overlay.

If you're trying to build from source you'll also need the latest [Memory.dll](https://github.com/erfg12/memory.dll/) release.

<img src="https://github.com/TermaciousTrickocity/Extended-Weapon-Display/assets/62641541/c0253efa-3c7a-473c-832d-6e4fa994c0ec" width="800" height="450">
<img src="https://github.com/user-attachments/assets/47b0c763-4639-42de-a1bb-5d23988733bd" width="800" height="450">

## If this breaks for whatever reason, here's how to fix:
- Open MCC with EAC off.
- Find the float value using Cheat Engine.
- Once found, generate a pointermap and pointer scan for valid pointers.
- Double click the pointers that end with: `0xB3C`
- Inside `Program.cs` paste the pointer in the `BaseAddress` string leaving the `,` at the end.
