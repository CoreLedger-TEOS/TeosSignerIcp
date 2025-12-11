using System.Text.Json;
using EdjCase.ICP.Agent.Identities;
using EdjCase.ICP.Agent.Models;
using EdjCase.ICP.Agent.Requests;
using EdjCase.ICP.Candid.Crypto;
using EdjCase.ICP.Candid.Models;
using TeosSigner.Icp.TeosApi.Model;

namespace TeosSigner.Icp.Signer;

class IcpSigner
{
	public IIdentity Identity { get; }
	public string Name { get; }

	public IcpSigner(IIdentity identity, string name)
	{
		Identity = identity;
		Name = name;
	}

	public string SignTransaction(Guid txId, IcpSigningParameters signingParameters)
	{
		var signedCallRequest = SignCallRequest(signingParameters);
		byte[] callRequestCbor = IcpHelper.SerializeSignedContent(signedCallRequest);

		var signedReadStateRequest = SignStatusRequest(signedCallRequest);
		byte[] readStateRequestCbor = IcpHelper.SerializeSignedContent(signedReadStateRequest);

		var txMessage = new IcpSignedTransactionMessage
		{
			TxId = txId.ToString(),
			CallRequest = callRequestCbor,
			ReadStateRequest = readStateRequestCbor
		};
		var signedTransaction = JsonSerializer.Serialize(txMessage);
		return signedTransaction;
	}

	private SignedContent SignCallRequest(IcpSigningParameters signingParameters)
	{
		Principal canisterId = signingParameters.CanisterId;
		CandidArg arguments = signingParameters.Arguments;
		Principal sender = signingParameters.Sender;

		string method = signingParameters.Method;
		ICTimestamp expiry = ICTimestamp.Future(TimeSpan.FromSeconds(60));

		CallRequest callRequest = IcpHelper.BuildRequest(sender, expiry, method, arguments, canisterId);

		Dictionary<string, IHashable> content = callRequest.BuildHashableItem();
		SignedContent signedContent = Identity.SignContent(content);
		return signedContent;
	}

	private SignedContent SignStatusRequest(SignedContent signedCallRequest)
	{
		var sha256 = SHA256HashFunction.Create();
		var requestId = RequestId.FromObject(signedCallRequest.Content, sha256);
		var requestStatusPath = StatePath.FromSegments("request_status", requestId.RawValue);
		var timePath = StatePath.FromSegments("time");
		var paths = new List<StatePath> { requestStatusPath, timePath };
		var expiry = ICTimestamp.Future(TimeSpan.FromSeconds(60));

		var readStateRequest = new ReadStateRequest(paths, Identity.GetPrincipal(), expiry);

		var signedReadStateRequest = Identity.SignContent(readStateRequest.BuildHashableItem());
		return signedReadStateRequest;
	}
}
