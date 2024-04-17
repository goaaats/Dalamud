using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Principal;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Security.Authorization;

using Serilog;

namespace Dalamud.Injector.Container;

internal sealed class AppContainer : IDisposable
{
    private PSID sid;

    public PSID Sid => this.sid;

    [SupportedOSPlatform("windows8.0")]
    public AppContainer(string containerName, string? displayName, string? description)
    {
        HRESULT hresult;

        Span<SID_AND_ATTRIBUTES> capabilities = Span<SID_AND_ATTRIBUTES>.Empty;

        unsafe
        {
            PSID* capabilityGroupSids;
            uint capabilityGroupSidCount = 0;
            PSID* capabilitySids;
            uint capabilitySidCount = 0;
            if (!PInvoke.DeriveCapabilitySidsFromName("ID_CAP_INTERNET_CLIENT", out capabilityGroupSids, out capabilityGroupSidCount, out capabilitySids, out capabilitySidCount))
            {
                throw new Win32Exception("Failed to derive capability sid");
            }

            capabilities = new SID_AND_ATTRIBUTES[capabilitySidCount];

            for (var i = 0; i < capabilitySidCount; i++)
            {
                capabilities[i].Sid = capabilitySids[i];
                capabilities[i].Attributes = PInvoke.SE_GROUP_ENABLED;
            }
        }

        Log.Information("Have {NumCaps} capabilities", capabilities.Length);

        //PInvoke.DeleteAppContainerProfile(containerName);
        hresult = PInvoke.CreateAppContainerProfile(
            containerName,
            displayName ?? containerName,
            description ?? containerName,
            capabilities,
            out this.sid);

        if (hresult.Succeeded)
        {
            return;
        }

        if (hresult == PInvoke.HRESULT_FROM_WIN32(WIN32_ERROR.ERROR_ALREADY_EXISTS))
        {
            hresult = PInvoke.DeriveAppContainerSidFromAppContainerName(containerName, out this.sid);
        }

        hresult.ThrowOnFailure();
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="AppContainer"/> class.
    /// </summary>
    ~AppContainer()
    {
        this.DisposeUnmanaged();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        this.DisposeUnmanaged();
    }

    private unsafe void DisposeUnmanaged()
    {
        Debug.Assert(OperatingSystem.IsOSPlatformVersionAtLeast("windows", 8), "unsupported platform");
        PInvoke.FreeSid(this.sid);
    }

    [SupportedOSPlatform("windows5.1.2600")]
    private void AddNamedObjectDacl(SE_OBJECT_TYPE objectType, string path, ACCESS_MODE accessMode, uint accessMask)
    {
        unsafe
        {
            WIN32_ERROR errc;
            EXPLICIT_ACCESS_W access;
            ACL* pacl = null, newPacl = null;

            try
            {
                access.grfAccessMode = accessMode;
                access.grfAccessPermissions = accessMask;
                access.grfInheritance = ACE_FLAGS.OBJECT_INHERIT_ACE | ACE_FLAGS.CONTAINER_INHERIT_ACE;
                access.Trustee.pMultipleTrustee = null;
                access.Trustee.TrusteeForm = TRUSTEE_FORM.TRUSTEE_IS_SID;
                access.Trustee.TrusteeType = TRUSTEE_TYPE.TRUSTEE_IS_WELL_KNOWN_GROUP;
                access.Trustee.ptstrName = (PWSTR)(void*)this.sid;

                fixed (char* pPath = path)
                {
                    errc = PInvoke.GetNamedSecurityInfo(
                        pPath,
                        objectType,
                        OBJECT_SECURITY_INFORMATION.DACL_SECURITY_INFORMATION,
                        null,
                        null,
                        &pacl,
                        null,
                        default);
                    if (errc != WIN32_ERROR.ERROR_SUCCESS)
                    {
                        throw new Exception($"Failed to fetch DACL information on {path}");
                    }

                    errc = PInvoke.SetEntriesInAcl(1, &access, pacl, &newPacl);
                    if (errc != WIN32_ERROR.ERROR_SUCCESS)
                    {
                        throw new Exception($"Failed to add acl entries on {path}");
                    }

                    errc = PInvoke.SetNamedSecurityInfo(
                        pPath,
                        objectType,
                        OBJECT_SECURITY_INFORMATION.DACL_SECURITY_INFORMATION,
                        default,
                        default,
                        newPacl,
                        null);
                    if (errc != WIN32_ERROR.ERROR_SUCCESS)
                    {
                        throw new Exception($"Failed to update DACL information on {path} ({errc:x})");
                    }
                }
            }
            finally
            {
                if (newPacl is not null)
                {
                    PInvoke.LocalFree((HLOCAL)newPacl);
                }
            }
        }
    }
    
    public SecurityIdentifier ToIdentityReference()
    {
        unsafe
        {
            return new SecurityIdentifier((IntPtr)this.Sid.Value);
        }
    }

    /*
    [SupportedOSPlatform("windows5.1.2600")]
    public void GrantFileAccess(string path, FILE_ACCESS_RIGHTS accessMask)
    {
        this.AddNamedObjectDacl(SE_OBJECT_TYPE.SE_FILE_OBJECT, path, ACCESS_MODE.GRANT_ACCESS, (uint)accessMask);
    }

    [SupportedOSPlatform("windows5.1.2600")]
    public void DenyFileAccess(string path, FILE_ACCESS_RIGHTS accessMask)
    {
        this.AddNamedObjectDacl(SE_OBJECT_TYPE.SE_FILE_OBJECT, path, ACCESS_MODE.DENY_ACCESS, (uint)accessMask);
    }
    */
}
