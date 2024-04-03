using System.Runtime.CompilerServices;
using Cosmos.Core;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace CosmosKernel4.Plugs
{

    [Plug(typeof(System.Activator))]
    public static class ActivatorImpl
    {
        public unsafe static T CreateInstance<T>()
        {
            // Object Allocation
            var ctr = (CosmosRuntimeType)typeof(T);
            var mType = VTablesImpl.mTypes[ctr.mTypeId];
            var dSize = mType.Size;
            var ptr = GCImplementation.AllocNewObject(dSize + ObjectUtils.FieldDataOffset);

            // Set Fields
            var vptr = (uint*)ptr;
            vptr[0] = ctr.mTypeId;  // Type
            vptr[1] = ptr;          // Address/Handler?
            vptr[2] = dSize;        // Data Area Size

            // Ctor Call
            T obj = Unsafe.Read<T>(vptr)!;
            var cctor = (delegate*<object, void>)mType.MethodAddresses[0];
            cctor(obj);

            return obj;
        }
    }
}