namespace TeosSigner.Icp;

class Spinnner
{
	private static readonly TimeSpan _interval = TimeSpan.FromMilliseconds(500);

	private readonly string _text;

	public Spinnner(string text)
	{
		_text = text;
	}

	public async Task ShowSpinner(CancellationToken token)
	{
		string[] dots = { ".", "..", "..." };
		int i = 0;

		int spinnerTop = Console.CursorTop;
		Console.WriteLine();
		int afterSpinnerTop = Console.CursorTop;
		if (afterSpinnerTop == spinnerTop)
		{
			spinnerTop -= 1;
		}

		try
		{
			while (!token.IsCancellationRequested)
			{
				Console.SetCursorPosition(0, spinnerTop);
				string text = $"{_text} {dots[i]}";

				int width = Math.Max(1, Console.WindowWidth - 1);
				if (text.Length < width) text = text.PadRight(width);

				Console.Write(text);

				Console.SetCursorPosition(0, afterSpinnerTop);

				i = (i + 1) % dots.Length;

				try
				{
					await Task.Delay(_interval, token);
				}
				catch (OperationCanceledException)
				{
					break;
				}
			}
		}
		catch
		{
			/* ignore */
		}
	}
}
