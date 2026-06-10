using System.Reflection;
using TeosSigner.Icp.Signer;

namespace TeosSigner.Icp.UI;

class ConsoleUi(SignersContainer signers)
{
	private readonly Spinnner _waitingSpinner = new("Waiting for new transactions");

	private CancellationTokenSource _spinnerCancellation;
	private Task _spinnerTask;

	public void DisplayStartupInformation()
	{
		DisplayWelcome();

		Console.WriteLine("Configured addresses:");
		foreach (var signer in signers.Signers)
		{
			Console.WriteLine($"- {signer.Principal} ({Path.GetFileName(signer.Name)})");
		}

		Console.WriteLine();
	}

	public void DisplayWaitingForTransactions(CancellationToken cancellationToken)
	{
		if (_spinnerCancellation is not null)
		{
			return;
		}

		_spinnerCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		_spinnerTask = _waitingSpinner.ShowSpinner(_spinnerCancellation.Token);
	}

	public async Task StopDisplayingWaitingForTransactionsAsync()
	{
		if (_spinnerCancellation is null)
		{
			return;
		}

		await _spinnerCancellation.CancelAsync();
		if (_spinnerTask is not null)
		{
			await _spinnerTask;
		}

		_spinnerCancellation.Dispose();
		_spinnerCancellation = null;
		_spinnerTask = null;
	}

	public async Task DisplayProcessingErrorAsync(Exception exception, CancellationToken cancellationToken)
	{
		await StopDisplayingWaitingForTransactionsAsync();

		string message = exception is HttpRequestException
			? $"Failed to connect to Teos API: {exception.Message}"
			: $"Failed to process pending transactions: {exception.Message}";

		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(message);
		Console.ResetColor();

		if (!cancellationToken.IsCancellationRequested)
		{
			DisplayWaitingForTransactions(cancellationToken);
		}
	}

	public async Task CompleteAsync()
	{
		await StopDisplayingWaitingForTransactionsAsync();
		Console.WriteLine();
	}

	private static void DisplayWelcome()
	{
		_ = bool.TryParse(Environment.GetEnvironmentVariable("SHOW_CAT"), out var showCat);

		if (showCat)
		{
			DisplayWelcomeWithCat();
		}
		else
		{
			DisplayWelcomeOfficial();
		}
	}

	private static void DisplayWelcomeOfficial()
	{

		var version = GetVersion();

		Console.WriteLine($"Welcome to TeosSigner.ICP (v.{version})");
		Console.WriteLine();
	}

	private static void DisplayWelcomeWithCat()
	{
		var ver = GetVersion();
		string banner = $"""
			################################################################################
			#                                                                              #
			#                           W I L L K O M M E N                                #
			#                                 B E I M                                      #
			#                                                                              #
			#                           F A B E L H A F T E N                              #
			#                         U N T E R Z E I C H N E R                            #
			#                                                                              #
			#                                  /\_/\                                       #
			#                                 ( o.o )                                      #
			#                                  > ^ <                                       #
			#                                                                              #
			#                                                                              #
			#                                 v.{ver}                                      #
			################################################################################
			""";

		Console.WriteLine(banner);
		Console.WriteLine();
	}

	private static string GetVersion()
	{
		string version_ = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
		var version = Version.Parse(version_);

		var result = $"{version.Major}.{version.Minor}.{version.Build}";
		return result;
	}
}
