using System.Text.Json.Serialization;

namespace TeosSigner.Icp.TeosApi.Model;

public class GetTransactionsResponse
{
	[JsonPropertyName("value")]
	public TxIdEntry[] Value { get; set; }
}

public class TxIdEntry
{
	public Guid Id { get; set; }
}
