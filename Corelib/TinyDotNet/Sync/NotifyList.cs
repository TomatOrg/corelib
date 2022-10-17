using System.Runtime.InteropServices;

namespace TinyDotNet.Sync;

[StructLayout(LayoutKind.Sequential)]
internal struct NotifyList
{

    private uint _wait;
    private uint _notify;
    private Spinlock _lock;
    private unsafe void* _head;
    private unsafe void* tail;

}