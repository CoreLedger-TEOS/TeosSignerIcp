namespace TeosSigner.Icp.UI;

class Spinnner
{
	private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(500);
	private static readonly string[] Dots = { ".", "..", "..." };

	private readonly string _text;

	public Spinnner(string text)
	{
		_text = text;
	}

	public async Task ShowSpinner(CancellationToken token)
	{
		int i = 0;

		try
		{
			while (!token.IsCancellationRequested)
			{
				TryDraw(Dots[i]);
				i = (i + 1) % Dots.Length;
				await Task.Delay(Interval, token);
			}
		}
		catch (OperationCanceledException) when (token.IsCancellationRequested)
		{
			// Normal spinner shutdown.
		}
		finally
		{
			TryClear();
		}
	}

	private void TryDraw(string dots)
	{
		try
		{
			(int left, int top) = Console.GetCursorPosition();
			int width = GetAvailableWidth();
			string text = $"{_text} {dots}";

			if (text.Length > width)
			{
				text = text[..width];
			}

			Console.SetCursorPosition(0, top);
			Console.Write(text.PadRight(width));
			Console.SetCursorPosition(Math.Min(left, width - 1), top);
		}
		catch (IOException)
		{
			// The terminal may be between two sizes while being resized.
		}
		catch (ArgumentOutOfRangeException)
		{
			// Cursor coordinates may become invalid during resize.
		}
	}

	private static void TryClear()
	{
		try
		{
			(_, int top) = Console.GetCursorPosition();
			int width = GetAvailableWidth();
			Console.SetCursorPosition(0, top);
			Console.Write(new string(' ', width));
			Console.SetCursorPosition(0, top);
		}
		catch (IOException)
		{
			// Ignore terminal shutdown or resize races.
		}
		catch (ArgumentOutOfRangeException)
		{
			// Ignore terminal shutdown or resize races.
		}
	}

	private static int GetAvailableWidth()
	{
		return Math.Max(1, Console.WindowWidth - 1);
	}
}
