using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TinyDotNet.Sync;

namespace System
{
    [StructLayout(LayoutKind.Sequential)]
    public class Object
    {
        private uint _vtable;
        private uint _type;
        internal Mutex _mutex;
        internal Condition _condition;
        internal ushort _lockThreadId;
        internal uint _typeFlags;
        
        [MethodImpl(MethodCodeType = MethodCodeType.Native)]
        public extern Type GetType();
        
        public virtual bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }
        
        public static bool Equals(object objA, object objB)
        {
            if (objA == objB)
            {
                return true;
            }
            
            if (objA == null || objB == null)
            {
                return false;
            }
            
            return objA.Equals(objB);
        }

        public virtual int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(this);
        }

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Native)]
        protected extern object MemberwiseClone();
        
        public static bool ReferenceEquals(object objA, object objB)
        {
            return objA == objB;
        }

        public virtual string ToString()
        {
            return GetType().ToString();
        }

    }
}