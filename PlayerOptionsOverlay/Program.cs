using ImGuiNET;
using ClickableTransparentOverlay;
using System.Numerics;
using System.Runtime.InteropServices;
using Memory;
using System.Diagnostics;

namespace PlayerOptions
{
    public class Program : Overlay
    {
        // Process names for both Steam & WinStore
        public string mccProcessSteam = "MCC-Win64-Shipping";
        public string mccProcessWinstore = "MCCWinStore-Win64-Shipping"; // Will fix this later.

        //
        // Pointer for the Halo: reach's 'Melee Depth' (float) option.
        // Just find this pointer and every other game should work.
        public string BaseAddress = "mcc-win64-shipping.exe+03F66B20,0x08,0x00,0x28,0x50,0x1A8,0x00,";
        //
        // Add the pointer with all offsets excluding the final offset.
        // The final offset goes to 'GameOffset' in 'CheckGameIndex()'
        // If it is wildly fucked up you might have to adjust each game in 'CheckGameIndex()' which is at the bottom of '#region Memory'.
        //   

        #region Overlay

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        bool showWindow = true;
        bool startup = false;

        string[] GameComboItems = { "Halo: Reach", "Halo CE", "Halo 2", "Halo 3", "Halo 3: ODST", "Halo 4" };
        int GameComboIndex = 0;

        float FOV = 0;
        float VFOV = 0;
        float Gamma = 0;
        float meleeDepth = 0;
        float meleeHorizontal = 0;
        float meleeVertical = 0;
        float pistolDepth = 0;
        float pistolHorizontal = 0;
        float pistolVertical = 0;
        float rifleDepth = 0;
        float rifleHorizontal = 0;
        float rifleVertical = 0;
        float heavyDepth = 0;
        float heavyHorizontal = 0;
        float heavyVertical = 0;

        protected override void Render()
        {
            if (showWindow)
            {
                ImGui.Begin("Player Options");

                ImGui.SetWindowSize(new Vector2(850, 580));
                ImGui.BeginChild("Game Selection", new Vector2(240, 20));
                ImGui.Combo("Game", ref GameComboIndex, GameComboItems, GameComboItems.Length);
                ImGui.EndChild();

                ImGui.Text(" ");
                ImGui.BeginChild("Camera options", new Vector2(500, 85));
                ImGui.Text("Camera options");
                ImGui.SliderFloat("Player field of View", ref FOV, 1, 150);
                ImGui.SliderFloat("Vehicle field of view", ref VFOV, 1, 150);
                ImGui.SliderFloat("Gamma", ref Gamma, -16, 16);
                ImGui.EndChild();

                ImGui.BeginChild("Melee view offset", new Vector2(1080, 100));
                ImGui.Text(" ");
                ImGui.Text("Melee view offsets");
                ImGui.SliderFloat("Melee depth", ref meleeDepth, -500, 500);
                ImGui.SliderFloat("Melee horizontal", ref meleeHorizontal, -500, 500);
                ImGui.SliderFloat("Melee vertical", ref meleeVertical, -500, 500);
                ImGui.EndChild();

                ImGui.BeginChild("Pistol view offset", new Vector2(1080, 100));
                ImGui.Text(" ");
                ImGui.Text("Pistol view offsets");
                ImGui.SliderFloat("Pistol depth", ref pistolDepth, -500, 500);
                ImGui.SliderFloat("Pistol horizontal", ref pistolHorizontal, -500, 500);
                ImGui.SliderFloat("Pistol vertical", ref pistolVertical, -500, 500);
                ImGui.EndChild();

                ImGui.BeginChild("Rifle view offset", new Vector2(1080, 100));
                ImGui.Text(" ");
                ImGui.Text("Rifle view offsets");
                ImGui.SliderFloat("Rifle depth", ref rifleDepth, -500, 500);
                ImGui.SliderFloat("Rifle horizontal", ref rifleHorizontal, -500, 500);
                ImGui.SliderFloat("Rifle vertical", ref rifleVertical, -500, 500);
                ImGui.EndChild();

                ImGui.BeginChild("Heavy view offset", new Vector2(1080, 100));
                ImGui.Text(" ");
                ImGui.Text("Heavy view offsets");
                ImGui.SliderFloat("Heavy depth", ref heavyDepth, -500, 500);
                ImGui.SliderFloat("Heavy horizontal", ref heavyHorizontal, -500, 500);
                ImGui.SliderFloat("Heavy vertical", ref heavyVertical, -500, 500);
                ImGui.EndChild();

                ImGui.End();
            }
        }
        #endregion

        #region Memory

        public Mem memory = new Mem();
        Process p;

        private string selectedProcessName;

        public bool modulesUpdated = false;

        public string FovAddress;
        public string VFovAddress;
        public string GammaAddress;

        // Dont change this unless you know what you are doing
        string GameOffset = "0x00";
        bool isNegitive = false;

        public async Task GetProcess()
        {
            try
            {
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    if (process.ProcessName.Equals(mccProcessSteam, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedProcessName = mccProcessSteam;
                        break;
                    }
                    else if (process.ProcessName.Equals(mccProcessWinstore, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedProcessName = mccProcessWinstore;
                        break;
                    }
                }

                p = Process.GetProcessesByName(selectedProcessName)[0];
                memory.OpenProcess(p.Id);

                if (memory == null || memory.theProc == null || selectedProcessName == null) return;

                if (startup == false)
                {
                    Console.WriteLine("Found: " + selectedProcessName.ToString() + " (" + p.Id + ")");
                    startup = true;
                }

                memory.theProc.Refresh();
                memory.modules.Clear();

                foreach (ProcessModule Module in memory.theProc.Modules)
                {
                    if (!string.IsNullOrEmpty(Module.ModuleName) && !memory.modules.ContainsKey(Module.ModuleName)) memory.modules.Add(Module.ModuleName, Module.BaseAddress);
                }
            }
            catch
            {
                Console.WriteLine("The MCC process was not found... Please open MCC and try again.");

                while (true)
                {
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }
        }

        public async Task GetValues() // Gonna do another rewrite at somepoint 
        {
            try
            {
                int ConvertedOffset;
                ConvertedOffset = Convert.ToInt32(GameOffset, 16);

                FOV = memory.ReadFloat(FovAddress);
                Gamma = memory.ReadFloat(GammaAddress);

                if (isNegitive == false)
                {
                    string meleeDepthString = "0x" + ConvertedOffset.ToString("X");
                    meleeDepth = memory.ReadFloat(BaseAddress + meleeDepthString);

                    ConvertedOffset += 4;
                    string meleeHorizontalString = "0x" + ConvertedOffset.ToString("X");
                    meleeHorizontal = memory.ReadFloat(BaseAddress + meleeHorizontalString);

                    ConvertedOffset += 4;
                    string meleeVerticalString = "0x" + ConvertedOffset.ToString("X");
                    meleeVertical = memory.ReadFloat(BaseAddress + meleeVerticalString);

                    ConvertedOffset += 4;
                    string pistolDepthString = "0x" + ConvertedOffset.ToString("X");
                    pistolDepth = memory.ReadFloat(BaseAddress + pistolDepthString);

                    ConvertedOffset += 4;
                    string pistolHorizontalString = "0x" + ConvertedOffset.ToString("X");
                    pistolHorizontal = memory.ReadFloat(BaseAddress + pistolHorizontalString);

                    ConvertedOffset += 4;
                    string pistolVerticalString = "0x" + ConvertedOffset.ToString("X");
                    pistolVertical = memory.ReadFloat(BaseAddress + pistolVerticalString);

                    ConvertedOffset += 4;
                    string rifleDepthString = "0x" + ConvertedOffset.ToString("X");
                    rifleDepth = memory.ReadFloat(BaseAddress + rifleDepthString);

                    ConvertedOffset += 4;
                    string rifleHorizontalString = "0x" + ConvertedOffset.ToString("X");
                    rifleHorizontal = memory.ReadFloat(BaseAddress + rifleHorizontalString);

                    ConvertedOffset += 4;
                    string rifleVerticalString = "0x" + ConvertedOffset.ToString("X");
                    rifleVertical = memory.ReadFloat(BaseAddress + rifleVerticalString);

                    ConvertedOffset += 4;
                    string heavyDepthString = "0x" + ConvertedOffset.ToString("X");
                    heavyDepth = memory.ReadFloat(BaseAddress + heavyDepthString);

                    ConvertedOffset += 4;
                    string heavyHorizontalString = "0x" + ConvertedOffset.ToString("X");
                    heavyHorizontal = memory.ReadFloat(BaseAddress + heavyHorizontalString);

                    ConvertedOffset += 4;
                    string heavyVerticalString = "0x" + ConvertedOffset.ToString("X");
                    heavyVertical = memory.ReadFloat(BaseAddress + heavyVerticalString);

                    ConvertedOffset -= 44;
                }
                else
                {
                    string meleeDepthString = "0x-" + ConvertedOffset.ToString("X");
                    meleeDepth = memory.ReadFloat(BaseAddress + meleeDepthString);

                    ConvertedOffset -= 4;
                    string meleeHorizontalString = "0x-" + ConvertedOffset.ToString("X");
                    meleeHorizontal = memory.ReadFloat(BaseAddress + meleeHorizontalString);

                    ConvertedOffset -= 4;
                    string meleeVerticalString = "0x-" + ConvertedOffset.ToString("X");
                    meleeVertical = memory.ReadFloat(BaseAddress + meleeVerticalString);

                    ConvertedOffset -= 4;
                    string pistolDepthString = "0x-" + ConvertedOffset.ToString("X");
                    pistolDepth = memory.ReadFloat(BaseAddress + pistolDepthString);

                    ConvertedOffset -= 4;
                    string pistolHorizontalString = "0x-" + ConvertedOffset.ToString("X");
                    pistolHorizontal = memory.ReadFloat(BaseAddress + pistolHorizontalString);

                    ConvertedOffset -= 4;
                    string pistolVerticalString = "0x-" + ConvertedOffset.ToString("X");
                    pistolVertical = memory.ReadFloat(BaseAddress + pistolVerticalString);

                    ConvertedOffset -= 4;
                    string rifleDepthString = "0x-" + ConvertedOffset.ToString("X");
                    rifleDepth = memory.ReadFloat(BaseAddress + rifleDepthString);

                    ConvertedOffset -= 4;
                    string rifleHorizontalString = "0x-" + ConvertedOffset.ToString("X");
                    rifleHorizontal = memory.ReadFloat(BaseAddress + rifleHorizontalString);

                    ConvertedOffset -= 4;
                    string rifleVerticalString = "0x-" + ConvertedOffset.ToString("X");
                    rifleVertical = memory.ReadFloat(BaseAddress + rifleVerticalString);

                    ConvertedOffset -= 4;
                    string heavyDepthString = "0x-" + ConvertedOffset.ToString("X");
                    heavyDepth = memory.ReadFloat(BaseAddress + heavyDepthString);

                    ConvertedOffset -= 4;
                    string heavyHorizontalString = "0x-" + ConvertedOffset.ToString("X");
                    heavyHorizontal = memory.ReadFloat(BaseAddress + heavyHorizontalString);

                    ConvertedOffset -= 4;
                    string heavyVerticalString = "0x-" + ConvertedOffset.ToString("X");
                    heavyVertical = memory.ReadFloat(BaseAddress + heavyVerticalString);

                    ConvertedOffset += 44;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine();
                Console.WriteLine("-=-=-=-=-=-=-");
                Console.WriteLine();
                Console.WriteLine("An error ocurred while trying to get values.\n- Try relaunching with EAC off.\n- Did MCC get an update recently?");
                Console.ReadKey();
            }
        }

        public async Task SetValues()
        {
            float[] previousValues = new float[] { FOV, Gamma, meleeDepth, meleeHorizontal, meleeVertical, pistolDepth, pistolHorizontal, pistolVertical, rifleDepth, rifleHorizontal, rifleVertical, heavyDepth, heavyHorizontal, heavyVertical, VFOV };

            while (true)
            {
                await Task.Delay(1);

                int ConvertedOffset;
                ConvertedOffset = Convert.ToInt32(GameOffset, 16);

                float[] currentValues = new float[] { FOV, Gamma, meleeDepth, meleeHorizontal, meleeVertical, pistolDepth, pistolHorizontal, pistolVertical, rifleDepth, rifleHorizontal, rifleVertical, heavyDepth, heavyHorizontal, heavyVertical, VFOV };

                for (int i = 0; i < previousValues.Length; i++)
                {
                    if (currentValues[i] != previousValues[i])
                    {
                        if (isNegitive == false)
                        {
                            switch (i)
                            {
                                case 0:
                                    if (GameComboIndex == 1 || GameComboIndex == 2)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        string fovValue = FOV.ToString();
                                        memory.WriteMemory(FovAddress, "float", fovValue);
                                    }
                                    break;
                                case 1:
                                    string gammaValue = Gamma.ToString();
                                    memory.WriteMemory(GammaAddress, "float", gammaValue);
                                    break;
                                case 2:
                                    ConvertedOffset += 0;
                                    string meleeDepthValue = meleeDepth.ToString();
                                    string meleeDepthString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + meleeDepthString, "float", meleeDepthValue);
                                    break;
                                case 3:
                                    ConvertedOffset += 4;
                                    string meleeHorizontalValue = meleeHorizontal.ToString();
                                    string meleeHorizontalString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + meleeHorizontalString, "float", meleeHorizontalValue);
                                    break;
                                case 4:
                                    ConvertedOffset += 8;
                                    string meleeVerticalValue = meleeVertical.ToString();
                                    string meleeVerticalString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + meleeVerticalString, "float", meleeVerticalValue);
                                    break;
                                case 5:
                                    ConvertedOffset += 12;
                                    string pistolDepthValue = pistolDepth.ToString();
                                    string pistolDepthString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + pistolDepthString, "float", pistolDepthValue);
                                    break;
                                case 6:
                                    ConvertedOffset += 16;
                                    string pistolHorizontalValue = pistolHorizontal.ToString();
                                    string pistolHorizontalString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + pistolHorizontalString, "float", pistolHorizontalValue);
                                    break;
                                case 7:
                                    ConvertedOffset += 20;
                                    string pistolVerticalValue = pistolVertical.ToString();
                                    string pistolVerticalString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + pistolVerticalString, "float", pistolVerticalValue);
                                    break;
                                case 8:
                                    ConvertedOffset += 24;
                                    string rifleDepthValue = rifleDepth.ToString();
                                    string rifleDepthString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + rifleDepthString, "float", rifleDepthValue);
                                    break;
                                case 9:
                                    ConvertedOffset += 28;
                                    string rifleHorizontalValue = rifleHorizontal.ToString();
                                    string rifleHorizontalString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + rifleHorizontalString, "float", rifleHorizontalValue);
                                    break;
                                case 10:
                                    ConvertedOffset += 32;
                                    string rifleVerticalValue = rifleVertical.ToString();
                                    string rifleVerticalString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + rifleVerticalString, "float", rifleVerticalValue);
                                    break;
                                case 11:
                                    ConvertedOffset += 36;
                                    string heavyDepthValue = heavyDepth.ToString();
                                    string heavyDepthString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + heavyDepthString, "float", heavyDepthValue);
                                    break;
                                case 12:
                                    ConvertedOffset += 40;
                                    string heavyHorizontalValue = heavyHorizontal.ToString();
                                    string heavyHorizontalString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + heavyHorizontalString, "float", heavyHorizontalValue);
                                    break;
                                case 13:
                                    ConvertedOffset += 44;
                                    string heavyVerticalValue = heavyVertical.ToString();
                                    string heavyVerticalString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + heavyVerticalString, "float", heavyVerticalValue);
                                    break;
                                case 14:
                                    if (GameComboIndex == 1 || GameComboIndex == 2)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        string fovValue = VFOV.ToString();
                                        memory.WriteMemory(VFovAddress, "float", fovValue);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            switch (i)
                            {
                                case 0:
                                    if (GameComboIndex == 1 || GameComboIndex == 2)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        string fovValue = FOV.ToString();
                                        memory.WriteMemory(FovAddress, "float", fovValue);
                                    }
                                    break;
                                case 1:
                                    string gammaValue = Gamma.ToString();
                                    memory.WriteMemory(GammaAddress, "float", gammaValue);
                                    break;
                                case 2:
                                    ConvertedOffset -= 0;
                                    string meleeDepthValue = meleeDepth.ToString();
                                    string meleeDepthString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + meleeDepthString, "float", meleeDepthValue);
                                    break;
                                case 3:
                                    ConvertedOffset -= 4;
                                    string meleeHorizontalValue = meleeHorizontal.ToString();
                                    string meleeHorizontalString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + meleeHorizontalString, "float", meleeHorizontalValue);
                                    break;
                                case 4:
                                    ConvertedOffset -= 8;
                                    string meleeVerticalValue = meleeVertical.ToString();
                                    string meleeVerticalString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + meleeVerticalString, "float", meleeVerticalValue);
                                    break;
                                case 5:
                                    ConvertedOffset -= 12;
                                    string pistolDepthValue = pistolDepth.ToString();
                                    string pistolDepthString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + pistolDepthString, "float", pistolDepthValue);
                                    break;
                                case 6:
                                    ConvertedOffset -= 16;
                                    string pistolHorizontalValue = pistolHorizontal.ToString();
                                    string pistolHorizontalString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + pistolHorizontalString, "float", pistolHorizontalValue);
                                    break;
                                case 7:
                                    ConvertedOffset -= 20;
                                    string pistolVerticalValue = pistolVertical.ToString();
                                    string pistolVerticalString = "0x" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + pistolVerticalString, "float", pistolVerticalValue);
                                    break;
                                case 8:
                                    ConvertedOffset -= 24;
                                    string rifleDepthValue = rifleDepth.ToString();
                                    string rifleDepthString = "0x-" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + rifleDepthString, "float", rifleDepthValue);
                                    break;
                                case 9:
                                    ConvertedOffset -= 28;
                                    string rifleHorizontalValue = rifleHorizontal.ToString();
                                    string rifleHorizontalString = "0x-" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + rifleHorizontalString, "float", rifleHorizontalValue);
                                    break;
                                case 10:
                                    ConvertedOffset -= 32;
                                    string rifleVerticalValue = rifleVertical.ToString();
                                    string rifleVerticalString = "0x-" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + rifleVerticalString, "float", rifleVerticalValue);
                                    break;
                                case 11:
                                    ConvertedOffset -= 36;
                                    string heavyDepthValue = heavyDepth.ToString();
                                    string heavyDepthString = "0x-" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + heavyDepthString, "float", heavyDepthValue);
                                    break;
                                case 12:
                                    ConvertedOffset -= 40;
                                    string heavyHorizontalValue = heavyHorizontal.ToString();
                                    string heavyHorizontalString = "0x-" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + heavyHorizontalString, "float", heavyHorizontalValue);
                                    break;
                                case 13:
                                    ConvertedOffset -= 44;
                                    string heavyVerticalValue = heavyVertical.ToString();
                                    string heavyVerticalString = "0x-" + ConvertedOffset.ToString("X");
                                    memory.WriteMemory(BaseAddress + heavyVerticalString, "float", heavyVerticalValue);
                                    break;
                                case 14:
                                    if (GameComboIndex == 1 || GameComboIndex == 2)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        string fovValue = VFOV.ToString();
                                        memory.WriteMemory(VFovAddress, "float", fovValue);
                                    }
                                    break;
                            }
                        }
                        

                        previousValues[i] = currentValues[i];
                    }

                }
                

                CheckGameIndex();

                if (GetAsyncKeyState(0x43) < 0)
                {
                    showWindow = !showWindow;
                    Thread.Sleep(150);
                }
            }
        }

        public async Task CheckGameIndex()
        {
            GammaAddress = "mcc-win64-shipping.exe+3EF26A4";

            switch (GameComboIndex)
            {
                case 0: // Halo: Reach
                    FovAddress = "haloreach.dll+2A03CDC";
                    isNegitive = false;
                    GameOffset = "0xB3C";
                    break;
                case 1: // Halo CE
                    FovAddress = "";
                    isNegitive = true;
                    GameOffset = "0x358C";
                    break;
                case 2: // Halo 2
                    FovAddress = "";
                    isNegitive = true;
                    GameOffset = "0x2AC0";
                    break;
                case 3: // Halo 3
                    FovAddress = "halo3.dll+2D3DDE4";
                    isNegitive = true;
                    GameOffset = "0x1FF4";
                    break;
                case 4: // Halo 3: ODST
                    FovAddress = "halo3odst.dll+2D818D8";
                    isNegitive = false;
                    GameOffset = "0x70";
                    break;
                case 5: // Halo 4
                    FovAddress = "halo4.dll+30EBEEC";
                    isNegitive = true;
                    GameOffset = "0x1528";
                    break;
            }

            await GetValues();
        }
        #endregion

        #region Program
        public Program()
        {
            Task task = Task.Run(async () =>
            {
                while (true)
                {
                    await GetProcess();
                    await CheckGameIndex();
                    await GetValues();
                    await SetValues();

                    await Task.Delay(1);
                }
            });
        }

        public static void Main(string[] args)
        { 
            Console.WriteLine("");
            Console.WriteLine();
            Console.WriteLine("Message me on Discord if there are any issues: @HybridsEgo\n(Your mileage may vary with FOV and Gamma options.)");
            Console.WriteLine();
            Console.WriteLine("Use 'C' key to show/hide the overlay!");
            Console.WriteLine("Closing this window will kill the overlay.");
            Console.WriteLine();
            Program program = new Program();
            program.Start().Wait();
        }
        #endregion
    }
}
