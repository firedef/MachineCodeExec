using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Asm.utils {
	public static unsafe class Allocator {
		private static readonly Process currentProcess;

		static Allocator() {
			currentProcess = Process.GetCurrentProcess();
		}

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocFlags flAllocType, MemoryProtection flProtect);
		
		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		private static extern IntPtr VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, FreeFlags dwFreeType);
        
		[Flags]
        public enum FreeFlags
        {
        	DECOMMIT = 0x00004000,
        	RELEASE = 0x00008000,
        	COALESCE_PLACEHOLDERS = 0x00000001,
        	PRESERVE_PLACEHOLDER = 0x00000002,
        }
        		
        [Flags]
        public enum AllocFlags
        {
        	COMMIT = 0x00001000,
        	RESERVE = 0x00002000,
        	RESET = 0x00080000,
        	RESET_UNDO = 0x1000000,
        	LARGE_PAGES = 0x20000000,
        	PHYSICAL = 0x00400000,
        	TOP_DOWN = 0x00100000,
        	WriteWatch = 0x200000,
        	LargePages = 0x20000000
        }
        
        [Flags]
        public enum MemoryProtection
        {
        	EXECUTE = 0x10,
        	EXECUTE_READ = 0x20,
        	EXECUTE_READWRITE = 0x40,
        	EXECUTE_WRITECOPY = 0x80,
        	NOACCESS = 0x01,
        	READONLY = 0x02,
        	READWRITE = 0x04,
        	WRITECOPY = 0x08,
        	TARGETS_INVALID = 0x40000000,
        	TARGETS_NO_UPDATE = 0x40000000,
        	GUARD = 0x100,
        	NOCACHE = 0x200,
        	WRITECOMBINE = 0x400,
        }
		
		public enum AllocType {
			virtualEx,
			hGlobal,
		}

		public static T* Allocate<T>(AllocType type, int count, AllocFlags flags, MemoryProtection protection) where T : unmanaged {
			return type switch {
				AllocType.virtualEx => AllocateVirtualEx<T>(count, flags, protection), 
				AllocType.hGlobal => AllocateHGlobal<T>(count), 
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
		}
		public static T* AllocateVirtualEx<T>(int count, AllocFlags flags, MemoryProtection protection) where T : unmanaged => (T*) VirtualAllocEx(currentProcess.Handle, IntPtr.Zero, (uint) (count * sizeof(T)), flags, protection);
		public static T* AllocateHGlobal<T>(int count) where T : unmanaged => (T*) Marshal.AllocHGlobal(count * sizeof(T));
		
		public static void Free<T>(AllocType type, T* address, int count, FreeFlags flags) where T : unmanaged {
			switch (type) {
				case AllocType.virtualEx: FreeVirtualEx(address, count, flags); break;
				case AllocType.hGlobal: FreeHGlobal(address); break;
				default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
		public static void FreeVirtualEx<T>(T* address, int count, FreeFlags flags) where T : unmanaged => VirtualFreeEx(currentProcess.Handle, (IntPtr) address, (uint) (count * sizeof(T)), flags);
		public static void FreeHGlobal<T>(T* address) where T : unmanaged => Marshal.FreeHGlobal((IntPtr) address);
	}
}
