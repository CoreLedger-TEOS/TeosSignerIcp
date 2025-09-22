using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeosSigner.Icp;
using TeosSigner.Icp.Services;
using TeosSigner.Icp.Signer;
using TeosSigner.Icp.TeosApi;

var pollingInterval = TimeSpan.FromSeconds(3);
var txSignDelay = TimeSpan.FromMilliseconds(500);
var services = BuildServices();

CancellationTokenSource cts = new();
var spinner = new Spinnner("Waiting for new transactions");
Task spinnerTask = null;

try
{
	var teosApi = services.GetRequiredService<TeosApiClient>();
	var signService = services.GetRequiredService<IcpSignService>();
	IEnumerable<string> signerAddresses = services.GetRequiredService<SignersContainer>()
		.Signers.Select(s => s.Identity.GetPrincipal().ToText());

	Console.WriteLine("Welcome to TeosSigner");
	Console.WriteLine();
	Console.WriteLine("Configured addresses:");
	foreach (var signerAddress in signerAddresses)
	{
		Console.WriteLine($"- {signerAddress}");
	}

	Console.WriteLine();

	spinnerTask = spinner.ShowSpinner(cts.Token);

	while (true)
	{
		IEnumerable<Guid> all = await teosApi.GetPendingTransactionIdsAsync(signerAddresses);
		List<Guid> toProcess = all.Where(t => !signService.Processed.Contains(t)).ToList();

		if (toProcess.Count != 0)
		{
			await cts.CancelAsync();
			await spinnerTask;

			Console.WriteLine($"To process {toProcess.Count} pending transactions");

			foreach (var id in toProcess)
			{
				Console.WriteLine($"Processing transaction '{id}'...");

				await signService.DoSigningStuff(id);
				await Task.Delay(txSignDelay);

				Console.WriteLine($"Transaction '{id}' successfully processed");
				Console.WriteLine();
			}

			cts.Dispose();
			cts = new CancellationTokenSource();
			spinnerTask = spinner.ShowSpinner(cts.Token);
		}

		await Task.Delay(pollingInterval);
	}
}
finally
{
	cts.Cancel();
	try
	{
		if (spinnerTask != null)
		{
			await spinnerTask;
		}
	}
	catch
	{
		/* ignore */
	}

	cts.Dispose();
	Console.WriteLine();
}

static IServiceProvider BuildServices()
{
	var configuration = new ConfigurationBuilder()
		.AddJsonFile("appsettings.json")
		.Build();

	var services = new ServiceCollection();
	services.Configure<ApiClientOptions>(configuration);
	services.AddTransient<TeosApiClient>();

	services.Configure<IcpSignerOptions>(configuration);
	services.AddSingleton<SignersContainer>();

	services.AddSingleton<IcpSignService>();
	return services.BuildServiceProvider();
}
