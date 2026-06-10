using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeosSigner.Icp;
using TeosSigner.Icp.Services;
using TeosSigner.Icp.Signer;
using TeosSigner.Icp.TeosApi;
using TeosSigner.Icp.UI;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
	Args = args,
	ContentRootPath = AppContext.BaseDirectory
});

builder.Configuration.AddJsonFile("appsettings.logging.json", optional: false, reloadOnChange: true);
builder.Logging.ConfigureLogging(builder.Configuration);

builder.Services.AddTeosApiClient(builder.Configuration.GetSection(TeosApiClientOptions.SectionName));
builder.Services.AddSigners(builder.Configuration);
builder.Services.AddIcpSignerWorker(builder.Configuration.GetSection(IcpTransactionProcessingOptions.SectionName));
builder.Services.AddSingleton<ConsoleUi>();

using var host = builder.Build();

host.Services.GetRequiredService<ConsoleUi>().DisplayStartupInformation();
await host.RunAsync();
