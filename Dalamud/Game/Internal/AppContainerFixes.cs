using System;
using System.Text;

using Dalamud.Common;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.String;
using Serilog;
// ReSharper disable InconsistentNaming

namespace Dalamud.Game.Internal;

/// <summary>
/// Class containing fixes for app container compatibility.
/// </summary>
[ServiceManager.BlockingEarlyLoadedService("Necessary for early AppContainer compatibility fixes.")]
internal sealed class AppContainerFix : IServiceType
{
    private readonly Hook<TryGetMyDocumentsPath> pathHook;
    private readonly Hook<SHGetKnownFolderPathPrototype> getKnownFolderPathHook;

    [ServiceManager.ServiceConstructor]
    private unsafe AppContainerFix(DalamudStartInfo startInfo, SigScanner sigScanner)
    {
        // These fixes are not "security features" but are compatibility kludges to make sandboxing FFXIV work.
        if (startInfo.UseAppContainer)
        {
            var pGetMyDocuments = sigScanner.ScanText("4889?????? 57 4881EC??????00 488B05???????? 4833C4 48898424???????? 488BF9 32DB");
            this.pathHook = Hook<TryGetMyDocumentsPath>.FromAddress(pGetMyDocuments, this.FixTryGetMyDocumentsPath);
            this.getKnownFolderPathHook = Hook<SHGetKnownFolderPathPrototype>.FromSymbol("shell32.dll", "SHGetKnownFolderPath", this.SHGetKnownFolderPathDetour);

            this.pathHook.Enable();
            this.getKnownFolderPathHook.Enable();

            Log.Debug("AppContainer Fix Test1: {Path}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }
    }

    private unsafe delegate nint TryGetMyDocumentsPath(Utf8String* pPath);

    private delegate uint SHGetKnownFolderPathPrototype(IntPtr rfid, uint dwFlags, IntPtr hToken, IntPtr ppszPath);

    private unsafe nint FixTryGetMyDocumentsPath(Utf8String* pPath)
    {
        // This function internally calls SHGetSpecialFolderLocation which process running inside a container don't have access to.
        var rpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        var rpathUtf8Length = Encoding.UTF8.GetByteCount(rpath);
        var rpathUtf8 = new byte[rpathUtf8Length + 1 /* NIL */];
        Encoding.UTF8.GetBytes(rpath, rpathUtf8);

        Log.Verbose("Redirecting My Documents to '{MyDocument}'", rpath);

        fixed (byte* pRedirectedPathUtf8 = rpathUtf8)
        {
            pPath->SetString(pRedirectedPathUtf8);
        }

        return 1;
    }

    private uint SHGetKnownFolderPathDetour(IntPtr rfid, uint dwFlags, IntPtr hToken, IntPtr ppszPath)
    {
        // SHGetKnownFolderPath internally checks if the designated directory is actually accessible before
        // returning a path to the caller. This can cause troubles for the process running inside the container as it will
        // never be accessible by default.
        //
        // Flag we're adding tells API to "No, I don't care whether it's readable or not. Just give me the path I asked for".
        dwFlags |= 0x00004000; /* KF_FLAG_DONT_VERIFY */
        return this.getKnownFolderPathHook.Original(rfid, dwFlags, hToken, ppszPath);
    }
}
