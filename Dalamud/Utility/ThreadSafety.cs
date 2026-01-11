using System.Runtime.CompilerServices;
using System.Threading;

namespace Dalamud.Utility;

/// <summary>
/// Helpers for working with thread safety.
/// </summary>
public static class ThreadSafety
{
    [ThreadStatic]
    private static bool threadStaticIsMainThread;

    [ThreadStatic]
    private static bool threadStaticIsRenderThread;

    /// <summary>
    /// Gets a value indicating whether the current thread is the main thread.
    /// </summary>
    public static bool IsMainThread => threadStaticIsMainThread;

    /// <summary>
    /// Gets a value indicating whether the current thread is allowed to be used for rendering.
    /// </summary>
    public static bool IsRenderThread => threadStaticIsMainThread || threadStaticIsRenderThread;

    /// <summary>
    /// Throws an exception when the current thread is not the main thread.
    /// </summary>
    /// <param name="message">The message to be passed into the exception, if one is to be thrown.</param>
    /// <exception cref="InvalidOperationException">Thrown when the current thread is not the main thread.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertMainThread(string? message = null)
    {
        if (!threadStaticIsMainThread)
        {
            throw new InvalidOperationException(message ?? "Not on main thread!");
        }
    }

    /// <summary>
    /// Throws an exception when the current thread is the main thread.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the current thread is the main thread.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertNotMainThread()
    {
        if (threadStaticIsMainThread)
        {
            throw new InvalidOperationException("On main thread!");
        }
    }

    /// <summary><see cref="AssertMainThread"/>, but only on debug compilation mode.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DebugAssertMainThread()
    {
#if DEBUG
        AssertMainThread();
#endif
    }

    /// <summary>
    /// Throws an exception when the current thread is not a render thread.
    /// </summary>
    /// <param name="message">The message to be passed into the exception, if one is to be thrown.</param>
    /// <exception cref="InvalidOperationException">Thrown when the current thread is not the main thread.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertRenderThread(string? message = null)
    {
        if (!threadStaticIsRenderThread)
        {
            throw new InvalidOperationException(message ?? "Not on main thread!");
        }
    }

    /// <summary>
    /// Throws an exception when the current thread is a render thread.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the current thread is the main thread.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertNotRenderThread()
    {
        if (threadStaticIsRenderThread)
        {
            throw new InvalidOperationException("On main thread!");
        }
    }

    /// <summary><see cref="AssertRenderThread"/>, but only on debug compilation mode.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DebugAssertRenderThread()
    {
#if DEBUG
        AssertRenderThread();
#endif
    }

    /// <summary>
    /// Marks a thread as the main thread.
    /// </summary>
    internal static void MarkMainThread()
    {
        threadStaticIsMainThread = true;
    }

    /// <summary>
    /// Marks a thread as a render thread.
    /// </summary>
    internal static void MarkRenderThread()
    {
        threadStaticIsRenderThread = true;
    }
}
