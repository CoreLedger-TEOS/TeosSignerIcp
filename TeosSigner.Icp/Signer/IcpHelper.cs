using System.Formats.Cbor;
using EdjCase.ICP.Agent.Identities;
using EdjCase.ICP.Agent.Models;
using EdjCase.ICP.Agent.Requests;
using EdjCase.ICP.Candid.Models;

namespace TeosSigner.Icp.Signer;

public static class IcpHelper
{
	public static IIdentity BuildIdentity(string identityPEMFilePath)
	{
		IIdentity identity = null;
		if (identityPEMFilePath != null) identity = IdentityUtil.FromPemFile(identityPEMFilePath, null);
		return identity;
	}

	public static CallRequest BuildRequest(Principal sender, ICTimestamp now, string method, CandidArg arg,
		Principal canisterId)
	{
		return new CallRequest(canisterId, method, arg, sender, now);
	}

	public static byte[] SerializeSignedContent(SignedContent signedContent)
	{
		var writer = new CborWriter();
		writer.WriteTag(CborTag.SelfDescribeCbor);
		signedContent.WriteCbor(writer);
		return writer.Encode();
	}
}

public class IcpSignedTransactionMessage
{
	public string TxId { get; set; }
	public byte[] ReadStateRequest { get; set; }
	public byte[] CallRequest { get; set; }
}
