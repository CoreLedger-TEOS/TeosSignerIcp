using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Settings.Configuration;

namespace TeosSigner.Icp;

static class LoggerExtensions
{
	private const LogEventLevel None = (LogEventLevel)((int)LogEventLevel.Fatal + 1);

	public static ILoggingBuilder ConfigureLogging(this ILoggingBuilder loggingBuilder, IConfigurationRoot configuration)
	{
		loggingBuilder.ClearProviders();

		var loggerConfiguration = new LoggerConfiguration();

		ApplyLogLevels(loggerConfiguration, configuration.GetSection("Logging:LogLevel"));

		loggerConfiguration.ReadFrom.Configuration(configuration, new ConfigurationReaderOptions
			{
				SectionName = "Logging:Serilog"
			});
		loggingBuilder.AddSerilog(loggerConfiguration.CreateLogger(), dispose: true);

		return loggingBuilder;
	}

	private static void ApplyLogLevels(
		LoggerConfiguration loggerConfiguration,
		IConfigurationSection logLevels)
	{
		foreach (IConfigurationSection configuredLevel in logLevels.GetChildren())
		{
			LogEventLevel level = ParseLogLevel(configuredLevel.Value, configuredLevel.Path);
			if (configuredLevel.Key.Equals("Default", StringComparison.OrdinalIgnoreCase))
			{
				loggerConfiguration.MinimumLevel.Is(level);
			}
			else
			{
				loggerConfiguration.MinimumLevel.Override(configuredLevel.Key, level);
			}
		}
	}

	private static LogEventLevel ParseLogLevel(string value, string configurationPath)
	{
		return value?.ToUpperInvariant() switch
		{
			"TRACE" => LogEventLevel.Verbose,
			"DEBUG" => LogEventLevel.Debug,
			"INFORMATION" => LogEventLevel.Information,
			"WARNING" => LogEventLevel.Warning,
			"ERROR" => LogEventLevel.Error,
			"CRITICAL" => LogEventLevel.Fatal,
			"NONE" => None,
			_ => throw new InvalidOperationException(
				$"Unsupported log level '{value}' at '{configurationPath}'")
		};
	}
}
