namespace TeosSigner.Icp.TeosApi;

class TeosApiClientOptions
{
	public const string SectionName = "TeosApi";

	public string BearerToken { get; set; }
	public string BaseAddress { get; set; }
}
