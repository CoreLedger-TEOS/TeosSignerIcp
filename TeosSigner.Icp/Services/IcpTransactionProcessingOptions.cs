using System.ComponentModel.DataAnnotations;

namespace TeosSigner.Icp.Services;

class IcpTransactionProcessingOptions
{
	public const string SectionName = "TransactionProcessing";

	[PositiveTimeSpan]
	public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(3);

	[NonNegativeTimeSpan]
	public TimeSpan TransactionSignDelay { get; set; } = TimeSpan.FromMilliseconds(500);

	public bool ManualConfirmation { get; set; } = true;
}

class PositiveTimeSpanAttribute() : ValidationAttribute("{0} must be greater than zero")
{
	public override bool IsValid(object value)
	{
		return value is TimeSpan timeSpan && timeSpan > TimeSpan.Zero;
	}
}

class NonNegativeTimeSpanAttribute() : ValidationAttribute("{0} must not be negative")
{
	public override bool IsValid(object value)
	{
		return value is TimeSpan timeSpan && timeSpan >= TimeSpan.Zero;
	}
}
