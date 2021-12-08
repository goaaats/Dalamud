using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface.Animation.EasingFunctions;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using Serilog;

namespace Dalamud.Interface.Internal.Windows
{
    internal class LogoWindow : Window, IDisposable
    {
        private InOutQuint easeInOut = new(TimeSpan.FromMilliseconds(1000));
        private const int LOGO_FADEIN_MS = 2500;

        private readonly TextureWrap logoTexture;

        public LogoWindow() : base("LogoWindow",
                                   ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize |
                                   ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar |
                                   ImGuiWindowFlags.NoBackground)
        {
            var dalamud = Service<Dalamud>.Get();
            var interfaceManager = Service<InterfaceManager>.Get();

            this.easeInOut.Start();
            this.logoTexture = interfaceManager.LoadImage(Path.Combine(dalamud.AssetDirectory.FullName, "UIRes", "logo.png"))!;
        }

        public override void Draw()
        {
            this.easeInOut.Duration = TimeSpan.FromMilliseconds(2500);
            var p = ImGui.GetCursorScreenPos();
            var pm = new Vector2(p.X + this.logoTexture.Width / 3f, p.Y + this.logoTexture.Height / 3f);
            ImGui.GetWindowDrawList().AddImage(this.logoTexture.ImGuiHandle, p, pm, Vector2.Zero,
                                               Vector2.One, ((uint)(this.easeInOut.Value * 0xFF) << 24) | 0xFFFFFF);

            this.easeInOut.Update();
            if (this.easeInOut.IsDone)
                this.easeInOut.Stop();

            if (ImGui.Button("Restart sequence"))
            {
                this.easeInOut.Restart();
            }
        }

        public void Dispose()
        {
            this.logoTexture.Dispose();
        }
    }
}
