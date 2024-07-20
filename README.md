# Extended weapon display overlay

Allows users to set their weapon display offsets well beyond the standard limits for all games within The Master Chief Collection.
- Install the [.NET 7.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-7.0.14-windows-x64-installer) and [.NET 7.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-7.0.14-windows-x64-installer) before trying to run/build.
- Launch The Master Chief Collection with EAC off.
- Use 'C' to toggle the overlay.

If you're trying to build this you'll also need the latest [Memory.dll](https://github.com/erfg12/memory.dll/) release.

<img src="https://github.com/TermaciousTrickocity/Extended-Weapon-Display/assets/62641541/c0253efa-3c7a-473c-832d-6e4fa994c0ec" width="800" height="450">
<img src="https://github.com/user-attachments/assets/47b0c763-4639-42de-a1bb-5d23988733bd" width="800" height="450">

## If this breaks for whatever reason, here's how to fix:
> Should be very simple unless 343 adds more things to the setting menus. 
- Open MCC with EAC off.
- While on the Main Menu, change the Reach Melee to the `min(-20.0)` or `max(15.0)` value and save.
- Exit back to the Main Menu.
- Float scan in Cheat Engine for the value set.
- Once you're down to around 100/250, start a Reach custom game.
- Modify each value until one of them visually changes the Weapon Offset.
- Once found, generate a pointermap and pointer scan for valid pointers.
- Double click the pointers that end with: `0xB3C`
- Inside `Program.cs` paste the pointer in the `BaseAddress` string leaving the `,` at the end.
- Compile and Test!
