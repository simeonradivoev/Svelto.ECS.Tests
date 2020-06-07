using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.DataStructures.Internal
{
    /// <summary>
    /// todo: while at the moment is not strictly necessary, I will probably need to redesign this struct so that it can be shared over the time, otherwise it should be used as a ref struct (which is not possible) 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>

    //Todo: this must be internal
    public unsafe struct NativeFasterDictionary<TKey, TValue>
        where TKey : unmanaged, IEquatable<TKey> where TValue : struct
    {
        internal NativeFasterDictionary(IntPtr bufferBuckets, uint bucketSize, 
                                        IntPtr bufferValues, IntPtr bufferNodes, uint count, uint capacity) : this()
        {
            _values = bufferValues;
            _bucketsPointer = bufferBuckets;

            _valuesInfo = bufferNodes;

            _bucketsSize = bucketSize;
            _count = count;
            _capacity = capacity;
        }

        public IntPtr unsafeValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values;
        }

        public uint count => _count;
        public uint capacity => _capacity;

        public ref T GetDirectValue<T>(uint findIndex) where T:unmanaged
        {
            return ref ((T *)_values)[findIndex];
        }

        //I store all the index with an offset + 1, so that in the bucket list 0 means actually not existing.
        //When read the offset must be offset by -1 again to be the real one. In this way
        //I avoid to initialize the array to -1

        public bool TryFindIndex(TKey key, out uint findIndex)
        {
            int hash =  Unsafe.As<TKey, int>(ref key);
            int* gcHandle = (int*) _bucketsPointer;
            uint bucketIndex = Reduce((uint) hash, (uint) _bucketsSize);

            int valueIndex = gcHandle[bucketIndex] - 1;
            void* valuesInfo = (void*) _valuesInfo;

            //even if we found an existing value we need to be sure it's the one we requested
            while (valueIndex != -1)
            {
                //for some reason this is way faster than using Comparer<TKey>.default, should investigate
                ref var valueInfo =
                    ref Unsafe.AsRef<FasterDictionaryNode<TKey>>(
                        (Unsafe.Add<FasterDictionaryNode<TKey>>(valuesInfo, valueIndex)));
                if (valueInfo.hashcode == hash && valueInfo.key.Equals(key) == true)
                {
                    //this is the one
                    findIndex = (uint) valueIndex;
                    return true;
                }

                valueIndex = valueInfo.previous;
            }

            findIndex = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Reduce(uint x, uint N)
        {
            if (x >= N)
                return x % N;

            return x;
        }

#if UNITY_COLLECTIONS
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        readonly IntPtr        _valuesInfo;
#if UNITY_COLLECTIONS
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        readonly IntPtr _bucketsPointer;
#if UNITY_COLLECTIONS

        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        readonly IntPtr _values;

        readonly uint      _bucketsSize;
        readonly uint     _count;
        readonly uint      _capacity;
    }
}