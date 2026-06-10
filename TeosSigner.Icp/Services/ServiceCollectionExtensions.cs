using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeosSigner.Icp.Services;

static class ServiceCollectionExtensions
{
	public static IServiceCollection AddIcpSignerWorker(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddOptions<IcpTransactionProcessingOptions>()
			.Bind(configuration)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.AddSingleton<IcpSignService>();
		services.AddSingleton<IcpTransactionProcessor>();
		services.AddHostedService<IcpSignerWorker>();

		return services;
	}
}
