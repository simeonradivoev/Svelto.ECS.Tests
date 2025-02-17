﻿#if NEW_C_SHARP || !UNITY_5_3_OR_NEWER
using System;
using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    public struct UnmanagedStream
    {
        public unsafe UnmanagedStream(byte* ptr, int sizeInByte):this()
        {
            _ptr = ptr;
            _sveltoStream = new SveltoStream(sizeInByte);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged => _sveltoStream.Read<T>(ToSpan());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(in T value) where T : unmanaged => _sveltoStream.Write(ToSpan(), value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(in T value, int size) where T : struct => _sveltoStream.Write(ToSpan(), value, size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(in Span<T> valueSpan) where T : unmanaged => _sveltoStream.WriteSpan(ToSpan(), valueSpan);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _sveltoStream.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _sveltoStream.Reset();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanAdvance() => _sveltoStream.CanAdvance();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> ToSpan()
        {
            unsafe
            {
                return new Span<byte>(_ptr, _sveltoStream.capacity);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AdvanceCursor(int sizeOf) => _sveltoStream.AdvanceCursor(sizeOf);

        SveltoStream _sveltoStream; //CANNOT BE READ ONLY

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST    
#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        readonly unsafe byte* _ptr;
    }
}
#endif