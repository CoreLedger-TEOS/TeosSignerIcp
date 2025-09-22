using Microsoft.Extensions.Options;

namespace TeosSigner.Icp.Signer;

class SignersContainer
{
	public SignersContainer(IOptions<IcpSignerOptions> options)
	{
		IEnumerable<IcpSigner> signers = options.Value.IdentityFiles
			.Select(identityFile => IcpHelper.BuildIdentity(identityFile))
			.Select(identity => new IcpSigner(identity));
		_signers.AddRange(signers);
	}

	private readonly List<IcpSigner> _signers = new();
	public IEnumerable<IcpSigner> Signers => _signers;
}

class IcpSignerOptions
{
	public string[] IdentityFiles { get; set; } = Array.Empty<string>();
}
