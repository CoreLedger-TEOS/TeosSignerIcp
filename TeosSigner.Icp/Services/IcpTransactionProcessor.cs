using Microsoft.Extensions.Options;
using TeosSigner.Icp.Signer;
using TeosSigner.Icp.TeosApi;

namespace TeosSigner.Icp.Services;

delegate Task TransactionsEventHandler(
	IReadOnlyCollection<Guid> transactionIds,
	CancellationToken cancellationToken);

class IcpTransactionProcessor(
	TeosApiClient teosApiClient,
	IcpSignService signService,
	SignersContainer signers,
	IOptions<IcpTransactionProcessingOptions> options)
{
	private readonly TimeSpan _transactionSignDelay = options.Value.TransactionSignDelay;
	private readonly HashSet<Guid> _processed = new();

	public event TransactionsEventHandler PendingTransactionsReceived;
	public event TransactionsEventHandler PendingTransactionsProcessed;

	public async Task ProcessPendingTransactionsAsync(CancellationToken cancellationToken)
	{
		List<Guid> transactionIds = await GetPendingTransactionIdsAsync(cancellationToken);
		if (transactionIds.Count == 0)
		{
			return;
		}

		try
		{
			await InvokeAsync(PendingTransactionsReceived, transactionIds, cancellationToken);
			await ProcessTransactionsAsync(transactionIds, cancellationToken);
		}
		finally
		{
			await InvokeAsync(PendingTransactionsProcessed, transactionIds, cancellationToken);
		}
	}

	private async Task<List<Guid>> GetPendingTransactionIdsAsync(CancellationToken cancellationToken)
	{
		IEnumerable<string> signerAddresses = signers.Signers.Select(s => s.Principal.ToText());
		IEnumerable<Guid> allPending = await teosApiClient.GetPendingTransactionIdsAsync(signerAddresses, cancellationToken);
		var result = allPending.Where(t => !_processed.Contains(t)).ToList();

		return result;
	}

	private async Task ProcessTransactionsAsync(
		IEnumerable<Guid> transactionIds,
		CancellationToken cancellationToken)
	{
		foreach (var id in transactionIds)
		{
			cancellationToken.ThrowIfCancellationRequested();

			_processed.Add(id);
			await signService.SignAndSubmitAsync(id, cancellationToken);

			await Task.Delay(_transactionSignDelay, cancellationToken);
		}
	}

	private static async Task InvokeAsync(
		TransactionsEventHandler handlers,
		IReadOnlyCollection<Guid> transactionIds,
		CancellationToken cancellationToken)
	{
		if (handlers is null)
		{
			return;
		}

		foreach (TransactionsEventHandler handler in handlers.GetInvocationList())
		{
			await handler(transactionIds, cancellationToken);
		}
	}
}
