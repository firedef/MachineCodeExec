using System;
using Asm.code;
using Asm.utils;
using Asm.utils.structures;

namespace Asm {
	public class Program {
		public static void Main(string[] args) {
			TestAsm();
		}

		// you can use VirtualAllocEx(), VirtualFreeEx(), Marshal.WriteByte() and Marshal.GetDelegateForFunctionPointer<>() for managed version
		// (Marshal.GetDelegateForFunctionPointer<>() does not support generics!)
		public static unsafe void TestAsm() {
			// C# using StdCall on windows and Cdecl on linux
			// this code uses StdCall calling convention
			// calling conventions:			https://docs.microsoft.com/en-us/cpp/build/x64-calling-convention?view=msvc-160
			// good assembler:			https://defuse.ca/online-x86-assembler.htm
			// good compiler (use it for asm gen):	https://godbolt.org/z/bqjeejGKM
			
			// you can add using keyword for automatic dispose
			AnyFunction func = new();
			
			// stack will reallocate on overflow
			// x64 disallow code execution from standard memory, so allocate memory for current process
			UnsafeStack<byte> src = new(64, Allocator.AllocType.virtualEx, Allocator.AllocFlags.COMMIT, Allocator.MemoryProtection.EXECUTE_READWRITE);
			func.SetData(src);
			
			// write specific code for x64 or x86
			if (Environment.Is64BitProcess) {
				// 0x40 = sizeOf(IntPtr) * (argCountInStack + currentStackSize) = 8 * (5 + 3) = 64
				// 
				// to access input arguments use:
				// RCX RDX R8D R9D EBP+0x40 EBP+0x48 EBP+0x50 EBP+0x58 EBP+0x60 EBP+0x68 ...
				// floats and vectors stored in XMM0 - XMM3 registers, instead of RCX - R9D
				// if return value is struct and sizeOf > 64bit, pass pointer in RCX to allocated struct by caller and shift passing arguments (RCX RDX R8D R9D -> RDX R8D R9D stack)
				//
				// to return value use RAX register and XMM0 for floats and vectors
				// if return value is struct and sizeOf > 64bit, return pointer in RAX to allocated struct by caller
				
				//!						x64
				// prolog
				src.Push(0x55);						// push rbp
				src.Push(0x57);						// push rdi
				src.Push(0x56);						// push rsi
				src.Push(0x48, 0x8b, 0xec);				// mov rbp,rsp
			
				// body
				src.Push(0x89, 0xc8);					// mov eax,ecx
				src.Push(0x01, 0xd0);					// add eax,edx
				src.Push(0x44, 0x01, 0xc0);				// add eax,r8d
				src.Push(0x44, 0x01, 0xc8);				// add eax,r9d
				src.Push(0x03, 0x45, 0x40);				// add eax,[ebp+0x40]
				src.Push(0x03, 0x45, 0x48);				// add eax,[ebp+0x48]
				src.Push(0x03, 0x45, 0x50);				// add eax,[ebp+0x50]
				src.Push(0x03, 0x45, 0x58);				// add eax,[ebp+0x58]
				src.Push(0x03, 0x45, 0x60);				// add eax,[ebp+0x60]
			
				// epilogue
				src.Push(0x5e);						// pop rsi
				src.Push(0x5f);						// pop rdi
				src.Push(0x5d);						// pop rbp
				src.Push(0xc3);						// ret
			}
			else 
			{
				//!						x86
				// prolog
				src.Push(0x55);						// push ebp
				src.Push(0x89, 0xe5);					// mov ebp,esp
				src.Push(0x57);						// push edi
				src.Push(0x56);						// push esi

				// body
				src.Push(0x89, 0xc8);					// mov eax,ecx
				src.Push(0x01, 0xd0);					// add eax,edx
				src.Push(0x03, 0x45, 0x08);				// add eax,ebp+8
				src.Push(0x03, 0x45, 0x0c);				// add eax,ebp+12
				src.Push(0x03, 0x45, 0x10);				// add eax,ebp+16
				src.Push(0x03, 0x45, 0x14);				// add eax,ebp+20
				src.Push(0x03, 0x45, 0x18);				// add eax,ebp+24
				src.Push(0x03, 0x45, 0x1c);				// add eax,ebp+28
				src.Push(0x03, 0x45, 0x20);				// add eax,ebp+32
			
				// epilogue
				src.Push(0x5e);						// pop esi
				src.Push(0x5f);						// pop edi
				src.Push(0x5d);						// pop ebp
				src.Push(0xc2, 0x1c, 0x00);				// ret 28
			}
			
			// cast data pointer to delegate and invoke
			var funDelegate = (delegate*<int, int, int, int, int, int, int, int, int, int>) func.Ptr;
			int result = funDelegate(3, 5, 7, 9, 11, 13, 15, 17, 19);
			Console.WriteLine(result);

			// same as previous
			result = func.Invoke<int, int, int, int, int, int, int, int, int, int>(3, 5, 7, 9, 11, 13, 15, 17, 19);
			Console.WriteLine(result);

			// AnyFunction will dispose UnsafeStack too
			func.Dispose();
		}
	}
}
