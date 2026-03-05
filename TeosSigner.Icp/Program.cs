using System.Reflection;
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
	var signers = services
		.GetRequiredService<SignersContainer>()
		.Signers.Select(s => new { address = s.Identity.GetPrincipal().ToText(), name = s.Name });

	// DrawWelcome();
	DrawWelcomeOfficial();

	Console.WriteLine("Configured addresses:");
	foreach (var signer in signers)
	{
		Console.WriteLine($"- {signer.address} ({signer.name})");
	}

	Console.WriteLine();

	spinnerTask = spinner.ShowSpinner(cts.Token);

	while (true)
	{
		try
		{
			IEnumerable<Guid> all = await teosApi.GetPendingTransactionIdsAsync(signers.Select(s => s.address));

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
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
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
	var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

	var services = new ServiceCollection();
	services.Configure<ApiClientOptions>(configuration);
	services.AddTransient<TeosApiClient>();

	services.Configure<IcpSignerOptions>(configuration);
	services.AddSingleton<SignersContainer>();

	services.AddSingleton<IcpSignService>();
	return services.BuildServiceProvider();
}

static void DrawWelcomeOfficial()
{
	var ver = GetVersion();
	string banner = $"Welcome to TeosSigner.ICP (v.{ver})";

	Console.WriteLine(banner);
	Console.WriteLine();
}

static void DrawWelcome()
{
	var ver = GetVersion();
	string banner = $"""
		################################################################################
		#                                                                              #
		#                           W I L L K O M M E N                                #
		#                                 B E I M                                      #
		#                                                                              #
		#                           F A B E L H A F T E N                              #
		#                         U N T E R Z E I C H N E R                            #
		#                                                                              #
		#                                  /\_/\                                       #
		#                                 ( o.o )                                      #
		#                                  > ^ <                                       #
		#                                                                              #
		#                                                                              #
		#                                 v.{ver}                                      #
		################################################################################
		""";

	Console.WriteLine(banner);
	Console.WriteLine();
}

static string GetVersion()
{
	string version_ = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
	var version = Version.Parse(version_);

	var result = $"{version.Major}.{version.Minor}.{version.Build}";
	return result;
}
