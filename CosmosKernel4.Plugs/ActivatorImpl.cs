using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Cosmos.Core;
using IL2CPU.API;
using IL2CPU.API.Attribs;
using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;

namespace CosmosKernel4.Plugs
{

    [Plug(typeof(System.Activator))]
    public static class ActivatorImpl
    {
        public unsafe static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>()
        {
            return (T)Activator.CreateInstance(typeof(T))!;
        }

        public unsafe static object CreateInstance([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
        {
            // Object Allocation
            var ctr = (CosmosRuntimeType)type;
            var mType = VTablesImpl.mTypes[ctr.mTypeId];
            var gcType = VTablesImpl.gcTypes[ctr.mTypeId];

            uint dSize = ObjectUtils.FieldDataOffset;
            for (int i = 0; i < gcType.GCFieldTypes.Length; i++)
            {
                dSize += VTablesImpl.GetSize(gcType.GCFieldTypes[i]);
            }

            var ptr = GCImplementation.AllocNewObject(dSize);

            // Set Fields
            var vptr = (uint*)ptr;
            vptr[0] = ctr.mTypeId;  // Type
            vptr[1] = ptr;          // Address/Handler?
            vptr[2] = dSize;        // Data Area Size
            var obj = Unsafe.Read<object>(vptr)!;
            var ctoraddress = mType.MethodAddresses[0];

            if (ctr.IsValueType)
            {
                // Struct Ctor Call
                var cctor = (delegate*<void*, void>)ctoraddress;
                cctor(vptr);
            }
            else
            {
                // Object Ctor Call
                var cctor = (delegate*<object, void>)ctoraddress;
                cctor(obj);
            }

            return obj;
        }
    }
}