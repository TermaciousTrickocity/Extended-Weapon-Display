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
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        public Mem memory = new Mem();
        Process p;

        public string mccProcessSteam = "MCC-Win64-Shipping";
        public string mccProcessWinstore = "MCCWinStore-Win64-Shipping";
        private string selectedProcessName;

        public bool modulesUpdated = false;

        bool showWindow = true;
        bool startup = false;

        int GameComboIndex = 0;
        string[] GameComboItems = { "Halo: Reach", "Halo CE", "Halo 2", "Halo 3", "Halo 3: ODST", "Halo 4" };

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

        public string FovAddress;
        public string VFovAddress;
        public string GammaAddress = "mcc-win64-shipping.exe+3EF36A4";

        public string MeleeDepth;
        public string MeleeHorizontal;
        public string MeleeVertical;
        public string PistolsDepth;
        public string PistolsHorizontal;
        public string PistolsVertical;
        public string RiflesDepth;
        public string RiflesHorizontal;
        public string RiflesVertical;
        public string HeavyDepth;
        public string HeavyHorizontal;
        public string HeavyVertical;

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

                if (startup == false)
                {
                    Console.WriteLine("Found: " + selectedProcessName.ToString() + " (" + p.Id + ")");
                    startup = true;
                }

                if (memory == null) return;
                if (memory.theProc == null) return;

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

        public async Task GetValues()
        {
            try
            {
                FOV = memory.ReadFloat(FovAddress);
                VFOV = memory.ReadFloat(VFovAddress);
                Gamma = memory.ReadFloat(GammaAddress);

                meleeDepth = memory.ReadFloat(MeleeDepth);
                meleeHorizontal = memory.ReadFloat(MeleeHorizontal);
                meleeVertical = memory.ReadFloat(MeleeVertical);

                pistolDepth = memory.ReadFloat(PistolsDepth);
                pistolHorizontal = memory.ReadFloat(PistolsHorizontal);
                pistolVertical = memory.ReadFloat(PistolsVertical);

                rifleDepth = memory.ReadFloat(RiflesDepth);
                rifleHorizontal = memory.ReadFloat(RiflesHorizontal);
                rifleVertical = memory.ReadFloat(RiflesVertical);

                heavyDepth = memory.ReadFloat(HeavyDepth);
                heavyHorizontal = memory.ReadFloat(HeavyHorizontal);
                heavyVertical = memory.ReadFloat(HeavyVertical);
            }
            catch
            {
                Console.WriteLine("An error ocurred while trying to get values.\nDid you launch MCC with EAC turned off?");
            }
        }

        public async Task SetValues()
        {
            float[] previousValues = new float[] { FOV, Gamma, meleeDepth, meleeHorizontal, meleeVertical, pistolDepth, pistolHorizontal, pistolVertical, rifleDepth, rifleHorizontal, rifleVertical, heavyDepth, heavyHorizontal, heavyVertical, VFOV};

            while (true)
            {
                await Task.Delay(1);

                float[] currentValues = new float[] { FOV, Gamma, meleeDepth, meleeHorizontal, meleeVertical, pistolDepth, pistolHorizontal, pistolVertical, rifleDepth, rifleHorizontal, rifleVertical, heavyDepth, heavyHorizontal, heavyVertical, VFOV};

                for (int i = 0; i < previousValues.Length; i++)
                {
                    if (currentValues[i] != previousValues[i])
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
                                string meleeDepthValue = meleeDepth.ToString();
                                memory.WriteMemory(MeleeDepth, "float", meleeDepthValue);
                                break;
                            case 3:
                                string meleeHorizontalValue = meleeHorizontal.ToString();
                                memory.WriteMemory(MeleeHorizontal, "float", meleeHorizontalValue);
                                break;
                            case 4:
                                string meleeVerticalValue = meleeVertical.ToString();
                                memory.WriteMemory(MeleeVertical, "float", meleeVerticalValue);
                                break;
                            case 5:
                                string pistolDepthValue = pistolDepth.ToString();
                                memory.WriteMemory(PistolsDepth, "float", pistolDepthValue);
                                break;
                            case 6:
                                string pistolHorizontalValue = pistolHorizontal.ToString();
                                memory.WriteMemory(PistolsHorizontal, "float", pistolHorizontalValue);
                                break;
                            case 7:
                                string pistolVerticalValue = pistolVertical.ToString();
                                memory.WriteMemory(PistolsVertical, "float", pistolVerticalValue);
                                break; 
                            case 8:
                                string rifleDepthValue = rifleDepth.ToString();
                                memory.WriteMemory(RiflesDepth, "float", rifleDepthValue);
                                break;
                            case 9:
                                string rifleHorizontalValue = rifleHorizontal.ToString();
                                memory.WriteMemory(RiflesHorizontal, "float", rifleHorizontalValue);
                                break;
                            case 10:
                                string rifleVerticalValue = rifleVertical.ToString();
                                memory.WriteMemory(RiflesVertical, "float", rifleVerticalValue);
                                break;
                            case 11:
                                string heavyDepthValue = heavyDepth.ToString();
                                memory.WriteMemory(HeavyDepth, "float", heavyDepthValue);
                                break;
                            case 12:
                                string heavyHorizontalValue = heavyHorizontal.ToString();
                                memory.WriteMemory(HeavyHorizontal, "float", heavyHorizontalValue);
                                break;
                            case 13:
                                string heavyVerticalValue = heavyVertical.ToString();
                                memory.WriteMemory(HeavyVertical, "float", heavyVerticalValue);
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
            switch (GameComboIndex)
            {
                case 0: // Halo: Reach
                    SetReachAddresses();
                    break;
                case 1: // Halo CE
                    SetCEAddresses();
                    break;
                case 2: // Halo 2
                    SetH2Addresses();
                    break;
                case 3: // Halo 3
                    SetH3Addresses();
                    break;
                case 4: // Halo 3: ODST
                    SetH3ODSTAddresses();
                    break;
                case 5: // Halo 4
                    SetH4Addresses();
                    break;
            }
        }

        public void SetReachAddresses()
        {
            FovAddress = "haloreach.dll+2A03D4C";
            VFovAddress = "haloreach.dll+2A21890";
            MeleeDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0xB3C";
            MeleeHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0xB40";
            MeleeVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0xB44";
            PistolsDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0xB48";
            PistolsHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0xB4C";
            PistolsVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0xB50";
            RiflesDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0xB54";
            RiflesHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0xB58";
            RiflesVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0xB5C";
            HeavyDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0xB60";
            HeavyHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0xB64";
            HeavyVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0xB68";

            GetValues();
        }

        public void SetCEAddresses()
        {
            FovAddress = "";
            VFovAddress = "";
            MeleeDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x358C";
            MeleeHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x3588";
            MeleeVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x3584";
            PistolsDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x3580";
            PistolsHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x357C";
            PistolsVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x3578";
            RiflesDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x3574";
            RiflesHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x3570";
            RiflesVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x356C";
            HeavyDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x3568";
            HeavyHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x3564";
            HeavyVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x3560";

            GetValues();
        }

        public void SetH2Addresses()
        {
            FovAddress = "";
            VFovAddress = "";
            MeleeDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x2AC0";
            MeleeHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x2ABC";
            MeleeVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x2AB8";
            PistolsDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x2AB4";
            PistolsHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x2AB0";
            PistolsVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x2AAC";
            RiflesDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x2AA8";
            RiflesHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x2AA4";
            RiflesVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x2AA0";
            HeavyDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x2A9C";
            HeavyHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x2A98";
            HeavyVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x2A94";

            GetValues();
        }

        public void SetH3Addresses()
        {
            FovAddress = "halo3.dll+2D3DDE4";
            VFovAddress = "halo3.dll+2D3DDE8";
            MeleeDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1FF4";
            MeleeHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1FF0";
            MeleeVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1FEC";
            PistolsDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1FE8";
            PistolsHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1FE4";
            PistolsVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1FE0";
            RiflesDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1FDC";
            RiflesHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1FD8";
            RiflesVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1FD4";
            HeavyDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1FD0";
            HeavyHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1FCC";
            HeavyVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1FC8";

            GetValues();
        }

        public void SetH3ODSTAddresses()
        {
            FovAddress = "halo3odst.dll+2D818D8";
            VFovAddress = "halo3odst.dll+2D818DC";
            MeleeDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0x70";
            MeleeHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0x74";
            MeleeVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0x78";
            PistolsDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0x7C";
            PistolsHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0x80";
            PistolsVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0x84";
            RiflesDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0x88";
            RiflesHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0x8C";
            RiflesVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0x90";
            HeavyDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0x94";
            HeavyHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0x98";
            HeavyVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,0x9C";

            GetValues();
        }

        public void SetH4Addresses()
        {
            FovAddress = "halo4.dll+30EBEEC";
            VFovAddress = "halo4.dll+30EBEF0";
            MeleeDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1528";
            MeleeHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1524";
            MeleeVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1520";
            PistolsDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x151C";
            PistolsHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1518";
            PistolsVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1514";
            RiflesDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1510";
            RiflesHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x150C";
            RiflesVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1508";
            HeavyDepth = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1504";
            HeavyHorizontal = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x1500";
            HeavyVertical = "mcc-win64-shipping.exe+03FFDC50,0xAF8,0x20,0x1A8,0x60,-0x14FC";

            GetValues();
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("Use 'C' key to show/hide the overlay!");
            Console.WriteLine();
            Program program = new Program();
            program.Start().Wait();
        }
    }
}