using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DoomBot.Enumerators;

namespace DoomBot.PooledCollections
{
    public partial struct PooledList<T>: IDisposable
    {
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => unchecked(ReadPos + 1);
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

    public partial struct PooledList<T>
    {
        private T[] Arr;

        private int ReadPos;

        public PooledList(int InitialSize = 10)
        {
            Arr = ArrayPool<T>.Shared.Rent(InitialSize);

            ReadPos = -1;
        }

        public ref T this[int Index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(Arr), Index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T Item)
        {
            var WritePos = unchecked(++ReadPos);

            var arr = Arr;

            if ((uint) WritePos >= (uint) arr.Length)
            {
                Resize();
            }

            arr[WritePos] = Item;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Resize()
        {
            var OldArrSpan = Arr.AsSpan();

            var NewArr = ArrayPool<T>.Shared.Rent(unchecked(Arr.Length * 2));
            
            OldArrSpan.CopyTo(Arr);

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                OldArrSpan.Fill(default);
            }

            ArrayPool<T>.Shared.Return(Arr);

            Arr = NewArr;
        }

        public void Remove(T Item)
        {
            if (ReadPos == -1)
            {
                return;
            }

            ref var RefFirstOffsetByOne = ref Unsafe.Subtract(ref MemoryMarshal.GetArrayDataReference(Arr), 1);

            ref var RefLast = ref Unsafe.Add(ref RefFirstOffsetByOne, Count);

            ref var RefCurrent = ref RefLast;
            
            while (Unsafe.IsAddressGreaterThan(ref RefCurrent, ref RefFirstOffsetByOne))
            {
                if (!EqualityComparer<T>.Default.Equals(RefCurrent, Item))
                {
                    Unsafe.Subtract(ref RefCurrent, 1);
                    
                    continue;
                }

                unchecked
                {
                    ReadPos--;
                }

                int Offset;

                //Fast Path
                
                if ((Offset = (int)Unsafe.ByteOffset(ref RefCurrent, ref RefLast)) == 0)
                {
                    return;
                }
                
                //Slow Path

                Offset /= Unsafe.SizeOf<T>();

                var DestSpan = MemoryMarshal.CreateSpan(ref RefCurrent, Offset);
                
                var SourceSpan = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref RefCurrent, 1), Offset);
                
                SourceSpan.CopyTo(DestSpan);

                return;
            }
        }
        
        public void RemoveLast()
        {
            if (ReadPos == -1)
            {
                return;
            }

            unchecked
            {
                ReadPos--;
            }
        }

        public void RemoveAt(int Index)
        {
            var Diff = unchecked(ReadPos - Index);

            var DestSpan = Arr.AsSpan(Index, Diff);
            
            var SourceSpan = Arr.AsSpan(unchecked(Index + 1), Diff);
            
            SourceSpan.CopyTo(DestSpan);

            unchecked
            {
                ReadPos--;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            ReadPos = -1;
        }
    }
}