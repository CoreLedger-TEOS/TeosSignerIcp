using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using TeosSigner.Icp.Signer;
using TeosSigner.Icp.TeosApi.Json;
using TeosSigner.Icp.TeosApi.Model;
using TeosSigner.Icp.TeosApi.OData;

namespace TeosSigner.Icp.TeosApi;

class TeosApiClient
{
	private readonly ApiClientOptions _options;

	public TeosApiClient(IOptions<ApiClientOptions> options, SignersContainer signers)
	{
		_options = options.Value;
	}

	public async Task<IEnumerable<Guid>> GetPendingTransactionIdsAsync(IEnumerable<string> addresses)
	{
		// filter
		var signedBy = new ConditionBuilder(ConditionOperand.Or);
		foreach (var address in addresses)
		{
			var condition = new FilterCondition("SignedBy", "eq", $"'{address}'");
			signedBy.AddCondition(condition);
		}

		var state = new FilterCondition("State", "eq", "1");

		var filterConditionsBuilder = new ConditionBuilder(ConditionOperand.And);
		filterConditionsBuilder.AddCondition(signedBy);
		filterConditionsBuilder.AddCondition(state);
		var filter = $"$filter={filterConditionsBuilder.Compile()}";

		// select
		var select = "$select=Id";

		var uri = new Uri($"Transactions?{filter}&{select}", UriKind.Relative);

		var response = await BuildClient().GetAsync(uri);
		var responseBody = await response.Content.ReadAsStringAsync();

		var ids = TeosJson.Deserialize<GetTransactionsResponse>(responseBody);
		return ids.Value.Select(x => x.Id);
	}

	public async Task<IcpSigningParameters> GetSiginingParametersAsync(Guid txId)
	{
		var uri = new Uri($"Transactions({txId})/GetSigningParameters", UriKind.Relative);

		var response = await BuildClient().GetAsync(uri);
		var responseBody = await response.Content.ReadAsStringAsync();

		var parameters = TeosJson.Deserialize<IcpSigningParameters>(responseBody);

		return parameters;
	}

	public async Task SubmitSignedAsync(Guid txId, SubmitSignedTransactionInput request)
	{
		var uri = new Uri($"Transactions({txId})/Submit", UriKind.Relative);

		var json = TeosJson.Serialize(request);
		var requestBody = new StringContent(json, new MediaTypeHeaderValue("application/json"));

		var response = await BuildClient().PostAsync(uri, requestBody);

		Console.WriteLine(response.StatusCode);
	}

	private HttpClient BuildClient()
	{
		return new HttpClient()
		{
			BaseAddress = new Uri(_options.BaseAddress),
			DefaultRequestHeaders =
			{
				Authorization = new AuthenticationHeaderValue("Bearer", _options.BearerToken),
			}
		};
	}
}

class ApiClientOptions
{
	public string BearerToken { get; set; }
	public string BaseAddress { get; set; }
}
