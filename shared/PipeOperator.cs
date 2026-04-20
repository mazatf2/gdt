namespace gdt.shared;

//https://www.reddit.com/r/csharp/comments/1p1kwqk/implementing_the_pipe_operator_in_c_14/
//|
public static class PipeOperator {
	extension<T, TResult>(T) {
		public static TResult operator |(T source, Func<T, TResult> func) {
			return func(source);
		}
	}
}
