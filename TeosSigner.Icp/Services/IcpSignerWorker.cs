using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeosSigner.Icp.UI;

namespace TeosSigner.Icp.Services;

class IcpSignerWorker(
	IcpTransactionProcessor transactionProcessor,
	ConsoleUi consoleUi,
	ILogger<IcpSignerWorker> logger,
	IOptions<IcpTransactionProcessingOptions> options) : BackgroundService
{
	private readonly TimeSpan _pollingInterval = options.Value.PollingInterval;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		transactionProcessor.PendingTransactionsReceived += OnPendingTransactionsReceivedAsync;
		transactionProcessor.PendingTransactionsProcessed += OnPendingTransactionsProcessedAsync;
		consoleUi.DisplayWaitingForTransactions(stoppingToken);

		try
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await transactionProcessor.ProcessPendingTransactionsAsync(stoppingToken);
				}
				catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
				{
					logger.LogError(ex, "Failed to process pending transactions");
					await consoleUi.DisplayProcessingErrorAsync(ex, stoppingToken);
				}

				await Task.Delay(_pollingInterval, stoppingToken);
			}
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
			// Normal host shutdown.
		}
		finally
		{
			transactionProcessor.PendingTransactionsReceived -= OnPendingTransactionsReceivedAsync;
			transactionProcessor.PendingTransactionsProcessed -= OnPendingTransactionsProcessedAsync;
			await consoleUi.CompleteAsync();
		}
	}

	private Task OnPendingTransactionsReceivedAsync(
		IReadOnlyCollection<Guid> transactionIds,
		CancellationToken cancellationToken)
	{
		return consoleUi.StopDisplayingWaitingForTransactionsAsync();
	}

	private Task OnPendingTransactionsProcessedAsync(
		IReadOnlyCollection<Guid> transactionIds,
		CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			consoleUi.DisplayWaitingForTransactions(cancellationToken);
		}

		return Task.CompletedTask;
	}
}
