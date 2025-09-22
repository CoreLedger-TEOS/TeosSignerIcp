using TeosSigner.Icp.Signer;
using TeosSigner.Icp.TeosApi;
using TeosSigner.Icp.TeosApi.Json;
using TeosSigner.Icp.TeosApi.Model;

namespace TeosSigner.Icp.Services;

class IcpSignService
{
	private readonly TeosApiClient _teosTeosApiClient;
	private readonly Dictionary<string, IcpSigner> _signers;
	private readonly List<Guid> _processed = new();
	public IEnumerable<Guid> Processed => _processed;

	public IcpSignService(TeosApiClient teosTeosApiClient, SignersContainer signers)
	{
		_teosTeosApiClient = teosTeosApiClient;
		_signers = signers.Signers.ToDictionary(s => s.Identity.GetPrincipal().ToText(), s => s);
	}

	public async Task DoSigningStuff(Guid txId)
	{
		_processed.Add(txId);

		IcpSigningParameters signingParameters = await _teosTeosApiClient.GetSiginingParametersAsync(txId);
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

		Console.Write("Press ENTER to sign and submit the transaction... ");
		Console.ReadLine();

		var signedTransaction = signer.SignTransaction(txId, signingParameters);
		var submitReq = new SubmitSignedTransactionInput
		{
			Description = "Signed in TeosSignerIcp utility",
			SignedTransaction = signedTransaction,
			SignerAddress = signer.Identity.GetPrincipal().ToText()
		};

		Console.WriteLine("Submitting transaction...");
		await _teosTeosApiClient.SubmitSignedAsync(txId, submitReq);
	}

	private static bool ValidateSigningParameters(IcpSigningParameters signingParameters)
	{
		return signingParameters?.CanisterId is not null;
	}
}
