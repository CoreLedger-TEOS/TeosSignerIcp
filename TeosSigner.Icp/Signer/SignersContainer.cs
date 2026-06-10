using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace TeosSigner.Icp.Signer;

class SignersContainer
{
	public SignersContainer(IOptions<IcpSignerOptions> options, IHostEnvironment environment)
	{
		IEnumerable<IcpSigner> signers = options.Value.IdentityFiles
			.Select(identityFile => new
			{
				identity = IcpHelper.BuildIdentity(GetIdentityPath(identityFile, environment.ContentRootPath)),
				name = Path.GetFileName(identityFile)
			})
			.Select(i => new IcpSigner(i.identity, i.name));
		_signers.AddRange(signers);
	}

	private static string GetIdentityPath(string identityFile, string contentRootPath)
	{
		return Path.IsPathRooted(identityFile)
			? identityFile
			: Path.Combine(contentRootPath, identityFile);
	}

	private readonly List<IcpSigner> _signers = new();
	public IEnumerable<IcpSigner> Signers => _signers;
}
