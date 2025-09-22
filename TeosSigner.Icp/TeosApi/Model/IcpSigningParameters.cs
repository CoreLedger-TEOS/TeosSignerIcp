using EdjCase.ICP.Candid.Models;

namespace TeosSigner.Icp.TeosApi.Model;

class IcpSigningParameters
{
	public Principal Sender { get; set; }
	public string Method { get; set; }
	public CandidArg Arguments { get; set; }
	public Principal CanisterId { get; set; }
	public string Expiry { get; set; }
}
