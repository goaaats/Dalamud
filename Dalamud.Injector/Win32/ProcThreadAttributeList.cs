using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.System.Threading;

namespace Dalamud.Injector.Win32;

internal sealed class ProcThreadAttributeList : IDisposable
{
    private IntPtr mAttributeListData;

    public unsafe LPPROC_THREAD_ATTRIBUTE_LIST AsPointer() => (LPPROC_THREAD_ATTRIBUTE_LIST)(void*)this.mAttributeListData;

    [SupportedOSPlatform("windows6.0.6000")]
    public ProcThreadAttributeList(int count)
    {
        nuint attributeAllocSize = default;

        unsafe
        {
            // First call is to ask for its size
            PInvoke.InitializeProcThreadAttributeList(
                (LPPROC_THREAD_ATTRIBUTE_LIST)null,
                (uint)count,
                0,
                &attributeAllocSize);

            this.mAttributeListData = Marshal.AllocCoTaskMem((int)attributeAllocSize);

            // Initialize it for real this time
            var ok = PInvoke.InitializeProcThreadAttributeList(
                this.AsPointer(),
                (uint)count,
                0,
                &attributeAllocSize);
            if (!ok)
            {
                throw new Win32Exception();
            }
        }
    }

    ~ProcThreadAttributeList()
    {
        this.DisposeUnmanaged();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        this.DisposeUnmanaged();
    }

    [SupportedOSPlatform("windows6.0.6000")]
    private void DisposeUnmanaged()
    {
        PInvoke.DeleteProcThreadAttributeList(this.AsPointer());
        Marshal.FreeCoTaskMem(this.mAttributeListData);
    }

    [SupportedOSPlatform("windows6.0.6000")]
    public unsafe void Add(nuint attribute, void* value, int cbSize)
    {
        var ok = PInvoke.UpdateProcThreadAttribute(
            this.AsPointer(),
            0, // reserved and must be 0 at all times
            attribute,
            value,
            (nuint)cbSize);
        if (!ok)
        {
            throw new Win32Exception();
        }
    }
}
