using Microsoft.Extensions.Options;
using TeosSigner.Icp.Signer;
using TeosSigner.Icp.TeosApi;
using TeosSigner.Icp.TeosApi.Json;
using TeosSigner.Icp.TeosApi.Model;

namespace TeosSigner.Icp.Services;

class IcpSignService
{
	private readonly TeosApiClient _teosTeosApiClient;
	private readonly Dictionary<string, IcpSigner> _signers;
	private readonly bool _manualConfirmation;

	public IcpSignService(
		TeosApiClient teosTeosApiClient,
		SignersContainer signers,
		IOptions<IcpTransactionProcessingOptions> options)
	{
		_teosTeosApiClient = teosTeosApiClient;
		_signers = signers.Signers.ToDictionary(s => s.Principal.ToText(), s => s);
		_manualConfirmation = options.Value.ManualConfirmation;
	}

	public async Task SignAndSubmitAsync(Guid txId, CancellationToken cancellationToken)
	{
		IcpSigningParameters signingParameters = await _teosTeosApiClient.GetSiginingParametersAsync(txId, cancellationToken);
		var str = TeosJson.Serialize(signingParameters, o => o.WriteIndented = true);
		if (!ValidateSigningParameters(signingParameters))
		{
			Console.WriteLine($"Validation error: {txId}\n{str}");
			return;
		}

		Console.WriteLine($"Signing parameters: {txId}\n{str}");

		if (!_signers.TryGetValue(signingParameters.Sender.ToText(), out var signer))
		{
			Console.WriteLine($"Can't find signer with address '{signingParameters.Sender}'");
			return;
		}

		if (_manualConfirmation)
		{
			Console.Write("Press ENTER to sign and submit the transaction... ");
			await Console.In.ReadLineAsync(cancellationToken);
		}

		var signedTransaction = signer.SignTransaction(txId, signingParameters);
		var submitReq = new SubmitSignedTransactionInput
		{
			Description = "Signed in TeosSignerIcp utility",
			SignedTransaction = signedTransaction,
			SignerAddress = signer.Identity.GetPrincipal().ToText()
		};

		Console.WriteLine("Submitting transaction...");
		await _teosTeosApiClient.SubmitSignedAsync(txId, submitReq, cancellationToken);
	}

	private static bool ValidateSigningParameters(IcpSigningParameters signingParameters)
	{
		return signingParameters?.CanisterId is not null;
	}
}
