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
            var ctr = (CosmosRuntimeType)typeof(T);

            // Till  `Activator.CreateInstance(Type T)` is correctly plugged to support structs we will handle it's creationg here, and redirect object creation there.
            if (!ctr.IsValueType)
            {
                return (T)Activator.CreateInstance(ctr)!; // The Plugged version should always return an object or cause stack corruption (if ctor is missing)
            }

            var mType = VTablesImpl.mTypes[ctr.mTypeId];
            var gcType = VTablesImpl.gcTypes[ctr.mTypeId];

            uint dSize = 0u;
            for (int i = 0; i < gcType.GCFieldTypes.Length; i++)
            {
                dSize += VTablesImpl.GetSize(gcType.GCFieldTypes[i]);
            }

            uint ptr;
            var bptr = stackalloc byte[(int)dSize];
            ptr = (uint)bptr;

            // Set Fields
            var vptr = (uint*)ptr;
            var obj = Unsafe.Read<T>(vptr)!;
            var ctoraddress = mType.MethodAddresses[0];

            // Struct Ctor Call
            var cctor = (delegate*<T*, void>)ctoraddress;
            cctor(&obj);

            return obj;
            //return (T)Activator.CreateInstance(typeof(T))!;
        }

        public unsafe static object CreateInstance([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] CosmosRuntimeType ctr)
        {
            // Object Allocation
            var mType = VTablesImpl.mTypes[ctr.mTypeId];
            var gcType = VTablesImpl.gcTypes[ctr.mTypeId];

            uint dSize = ctr.IsValueType ? 0u : ObjectUtils.FieldDataOffset;
            //uint dSize = ObjectUtils.FieldDataOffset;
            for (int i = 0; i < gcType.GCFieldTypes.Length; i++)
            {
                dSize += VTablesImpl.GetSize(gcType.GCFieldTypes[i]);
            }

            uint ptr;
            //ptr = GCImplementation.AllocNewObject(dSize);

            if (ctr.IsValueType)
            {
                var bptr = stackalloc byte[(int)dSize];
                ptr = (uint)bptr;
            }
            else
            {
                ptr = GCImplementation.AllocNewObject(dSize);
            }

            // Set Fields

            var vptr = (uint*)ptr; 
            if (!ctr.IsValueType)
            {
                vptr[0] = ctr.mTypeId;  // Type
                vptr[1] = ptr;          // Address/Handler?
                vptr[2] = mType.Size;   // Data Area Size
            }
            object obj = Unsafe.Read<object>(vptr)!;

            var ctoraddress = mType.MethodAddresses[0];

            if (ctr.IsValueType)
            {
                // Struct Ctor Call
                var cctor = (delegate*<void*, void>)ctoraddress;
                cctor(&obj);
                //cctor((void*)ptr);
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