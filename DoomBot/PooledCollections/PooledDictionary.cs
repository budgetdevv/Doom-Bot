using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DoomBot.Enumerators;

namespace DoomBot.PooledCollections
{
    public partial struct PooledDictionarySlim<T, F>: IDisposable
    {
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IndexArr.Count;
        }
        
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Arr.Length;
        }

        public RefEnumerator<T> GetEnumerator()
        {
            var Span = Arr.AsSpan(0, Count);
            
            return new RefEnumerator<T>(ref Span);
        }
        
        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(Arr);
        }
    }

    public partial struct PooledDictionarySlim<T, F>
    {
        public struct TFNode
        {
            public T Key;

            public F Value;
        }

        private TFNode[] Arr;

        private PooledList<int> IndexArr;
        
        public PooledDictionarySlim(int InitSize)
        {
            Arr = ArrayPool<TFNode>.Shared.Rent(InitSize);

            IndexArr = new PooledList<int>(InitSize);

            count = 0;
        }

        public void Add()
        {
            if (unchecked(count + 1) == Arr.Length)
            {
                Resize();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Resize()
        {
            
        }
    }
}