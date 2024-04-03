using System.Linq;
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
            return (T)Activator.CreateInstance(typeof(T))!;
        }

        public unsafe static object CreateInstance(Type type)
        {
            // Object Allocation
            var ctr = (CosmosRuntimeType)type;
            var mType = VTablesImpl.mTypes[ctr.mTypeId];
            var gcType = VTablesImpl.gcTypes[ctr.mTypeId];

            uint dSize = 0;
            for (int i = 0; i < gcType.GCFieldTypes.Length; i++)
            {
                dSize += VTablesImpl.GetSize(gcType.GCFieldTypes[i]);
            }

            var ptr = GCImplementation.AllocNewObject(dSize + ObjectUtils.FieldDataOffset);

            // Set Fields
            var vptr = (uint*)ptr;
            vptr[0] = ctr.mTypeId;  // Type
            vptr[1] = ptr;          // Address/Handler?
            vptr[2] = dSize;        // Data Area Size

            // Ctor Call
            var obj = Unsafe.Read<object>(vptr)!;
            var cctor = (delegate*<object, void>)mType.MethodAddresses[0];
            cctor(obj);

            return obj;
        }

    }
}