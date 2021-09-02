using System;
using Asm.utils.structures;

namespace Asm.code {
	public class AnyFunction : IDisposable {
		protected UnsafeStack<byte> stack;

		public unsafe byte* Ptr => stack.arrayPtr;
		
		public unsafe TResult Invoke<TResult>() => ((delegate*<TResult>) Ptr)();
		public unsafe TResult Invoke<T0, TResult>(T0 arg0) => ((delegate*<T0, TResult>) Ptr)(arg0);
		public unsafe TResult Invoke<T0, T1, TResult>(T0 arg0, T1 arg1) => ((delegate*<T0, T1, TResult>) Ptr)(arg0, arg1);
		public unsafe TResult Invoke<T0, T1, T2, TResult>(T0 arg0, T1 arg1, T2 arg2) => ((delegate*<T0, T1, T2, TResult>) Ptr)(arg0, arg1, arg2);
		public unsafe TResult Invoke<T0, T1, T2, T3, TResult>(T0 arg0, T1 arg1, T2 arg2, T3 arg3) => ((delegate*<T0, T1, T2, T3, TResult>) Ptr)(arg0, arg1, arg2, arg3);
		public unsafe TResult Invoke<T0, T1, T2, T3, T4, TResult>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) => ((delegate*<T0, T1, T2, T3, T4, TResult>) Ptr)(arg0, arg1, arg2, arg3, arg4);
		public unsafe TResult Invoke<T0, T1, T2, T3, T4, T5, TResult>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => ((delegate*<T0, T1, T2, T3, T4, T5, TResult>) Ptr)(arg0, arg1, arg2, arg3, arg4, arg5);
		public unsafe TResult Invoke<T0, T1, T2, T3, T4, T5, T6, TResult>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) => ((delegate*<T0, T1, T2, T3, T4, T5, T6, TResult>) Ptr)(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
		public unsafe TResult Invoke<T0, T1, T2, T3, T4, T5, T6, T7, TResult>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) => ((delegate*<T0, T1, T2, T3, T4, T5, T6, T7, TResult>) Ptr)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		public unsafe TResult Invoke<T0, T1, T2, T3, T4, T5, T6, T7, T8, TResult>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) => ((delegate*<T0, T1, T2, T3, T4, T5, T6, T7, T8, TResult>) Ptr)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		public unsafe TResult Invoke<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) => ((delegate*<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>) Ptr)(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
		
		public UnsafeStack<byte> GetData() => stack;
		public void SetData(UnsafeStack<byte> data) {
			stack?.Dispose();
			stack = data;
			if (stack.usingStackalloc) stack = stack.WithoutStackalloc();
		}

		public void Dispose() {
			stack?.Dispose();
		}
	}
}
