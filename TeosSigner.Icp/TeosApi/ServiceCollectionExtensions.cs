using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TeosSigner.Icp.TeosApi;

static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTeosApiClient(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddOptions<TeosApiClientOptions>()
			.Bind(configuration)
			.Validate(options => Uri.TryCreate(options.BaseAddress, UriKind.Absolute, out _),
				"BaseAddress must be an absolute URI")
			.Validate(options => !string.IsNullOrWhiteSpace(options.BearerToken),
				"BearerToken must not be empty")
			.ValidateOnStart();

		services.AddHttpClient<TeosApiClient>((serviceProvider, client) =>
		{
			var options = serviceProvider.GetRequiredService<IOptions<TeosApiClientOptions>>().Value;
			client.BaseAddress = new Uri(options.BaseAddress);
			client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Bearer", options.BearerToken);
		});

		return services;
	}
}
