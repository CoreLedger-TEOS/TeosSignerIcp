using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeosSigner.Icp.Signer;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSigners(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<IcpSignerOptions>(configuration);
		services.AddSingleton<SignersContainer>();

		return services;
	}
}
