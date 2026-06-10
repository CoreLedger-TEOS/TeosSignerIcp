using System.Net.Http.Headers;
using TeosSigner.Icp.TeosApi.Json;
using TeosSigner.Icp.TeosApi.Model;
using TeosSigner.Icp.TeosApi.OData;

namespace TeosSigner.Icp.TeosApi;

class TeosApiClient(HttpClient client)
{
	public async Task<IEnumerable<Guid>> GetPendingTransactionIdsAsync(IEnumerable<string> addresses, CancellationToken cancellationToken)
	{
		var filter = BuildFilter(addresses);
		var select = "$select=Id";

		var uri = new Uri($"Transactions?{filter}&{select}", UriKind.Relative);

		var response = await client.GetAsync(uri, cancellationToken);
		var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

		var ids = TeosJson.Deserialize<GetTransactionsResponse>(responseBody);
		return ids.Value.Select(x => x.Id);
	}

	private static string BuildFilter(IEnumerable<string> addresses)
	{
		var signedBy = new ConditionBuilder(ConditionOperand.Or);
		foreach (var address in addresses)
		{
			var condition = new FilterCondition("SignedBy", "eq", $"'{address}'");
			signedBy.AddCondition(condition);
		}

		var state = new ConditionBuilder(ConditionOperand.Or);
		state.AddCondition(new FilterCondition("State", "eq", "null"));
		state.AddCondition(new FilterCondition("State", "eq", "1"));

		var filterConditionsBuilder = new ConditionBuilder(ConditionOperand.And);
		filterConditionsBuilder.AddCondition(signedBy);
		filterConditionsBuilder.AddCondition(state);

		var filter = $"$filter={filterConditionsBuilder.Compile()}";

		return filter;
	}

	public async Task<IcpSigningParameters> GetSiginingParametersAsync(Guid txId, CancellationToken cancellationToken)
	{
		var uri = new Uri($"Transactions({txId})/GetSigningParameters", UriKind.Relative);

		var response = await client.GetAsync(uri, cancellationToken);
		var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

		var parameters = TeosJson.Deserialize<IcpSigningParameters>(responseBody);

		return parameters;
	}

	public async Task SubmitSignedAsync(Guid txId, SubmitSignedTransactionInput request, CancellationToken cancellationToken)
	{
		var uri = new Uri($"Transactions({txId})/Submit", UriKind.Relative);

		var json = TeosJson.Serialize(request);
		var requestBody = new StringContent(json, new MediaTypeHeaderValue("application/json"));

		var response = await client.PostAsync(uri, requestBody, cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine(response.StatusCode);
			Console.WriteLine(await response.Content.ReadAsStringAsync(cancellationToken));
		}
		else
		{
			Console.WriteLine($"Transaction '{txId}' successfully submitted.{Environment.NewLine}");
		}
	}
}
