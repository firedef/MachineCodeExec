using System;
using System.Text;

namespace Asm.utils.structures {
	public unsafe class UnsafeStack<T> : IDisposable where T : unmanaged
	{
		public T* arrayPtr;
		public int count;
		public int capacity;
		public bool usingStackalloc;
		public Allocator.AllocType defaultAllocType = Allocator.AllocType.hGlobal;
		public Allocator.AllocFlags defaultAllocFlags = Allocator.AllocFlags.COMMIT;
		public Allocator.MemoryProtection defaultProtectionFlags = Allocator.MemoryProtection.READWRITE;
		public Allocator.FreeFlags defaultFreeFlags = Allocator.FreeFlags.RELEASE;

		public int capacityLeft => capacity - count;

		public UnsafeStack() { }

		public UnsafeStack(int capacity, 
						   Allocator.AllocType defaultAllocType = Allocator.AllocType.hGlobal,
						   Allocator.AllocFlags defaultAllocFlags = Allocator.AllocFlags.COMMIT,
						   Allocator.MemoryProtection defaultProtectionFlags = Allocator.MemoryProtection.READWRITE,
						   Allocator.FreeFlags defaultFreeFlags = Allocator.FreeFlags.RELEASE) {
			this.defaultAllocType = defaultAllocType;
			this.defaultAllocFlags = defaultAllocFlags;
			this.defaultProtectionFlags = defaultProtectionFlags;
			this.defaultFreeFlags = defaultFreeFlags;
			
			ReAlloc(capacity);
		}
		
		public T Pop() => arrayPtr[--count];
		public T* PopPtr() => arrayPtr + --count;

		public void Push(T item) {
			EnsureCapacity(1);
			arrayPtr[count++] = item;
		}
		
		public void Push(params T[] item) {
			int c = item.Length;
			EnsureCapacity(c);
			for (int i = 0; i < c; i++) {
				arrayPtr[count + i] = item[i];
			}
			count += c;
		}

		public void EnsureCapacity(int left) {
			if (left <= capacityLeft) return;
			_Expand(capacity + left);
		}
		
		private void _Expand(int minNewSize = 4) => ReAlloc(Math.Max(minNewSize, capacity << 1));

		public void ReAlloc(int newCapacity) => ReplaceArray(Alloc(newCapacity), newCapacity);
		public T* Alloc(int newCapacity) => Allocator.Allocate<T>(defaultAllocType, newCapacity, defaultAllocFlags, defaultProtectionFlags);

		public void ReplaceArray(T* newArr, int newCapacity, bool useStackalloc = false) {
			if (arrayPtr != null) {
				Buffer.MemoryCopy(arrayPtr, newArr, newCapacity, Math.Min(newCapacity, count));
				ReleaseUnmanagedResources();
			}
			arrayPtr = newArr;
			capacity = newCapacity;
			usingStackalloc = useStackalloc;
		}

		public void CopyFrom(T* ptr) => Buffer.MemoryCopy(ptr, arrayPtr, capacity, capacity);
		public void CopyTo(T* ptr) => Buffer.MemoryCopy(arrayPtr, ptr, capacity, capacity);
		
		public UnsafeStack<T> WithoutStackalloc() {
			if (!usingStackalloc) return this;
			UnsafeStack<T> newCollection = new() {count = count, arrayPtr = Alloc(capacity)};
			Buffer.MemoryCopy(arrayPtr, newCollection.arrayPtr, capacity, capacity);
			return newCollection;
		}
		
		private void ReleaseUnmanagedResources() {
			if (arrayPtr != null && !usingStackalloc) Allocator.Free(defaultAllocType, arrayPtr, capacity, defaultFreeFlags);
		}

		public void Dispose() {
			ReleaseUnmanagedResources();
			GC.SuppressFinalize(this);
		}

		~UnsafeStack() => ReleaseUnmanagedResources();
	}
}
