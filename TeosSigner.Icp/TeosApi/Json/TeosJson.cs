using System.Text.Json;
using System.Text.Json.Serialization;
using EdjCase.ICP.Candid.Models;
using Nethereum.Hex.HexConvertors.Extensions;

namespace TeosSigner.Icp.TeosApi.Json;

public static class TeosJson
{
	private static readonly JsonSerializerOptions _options;

	static TeosJson()
	{
		_options = new JsonSerializerOptions();

		_options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		_options.PropertyNameCaseInsensitive = true;

		_options.Converters.Add(new PrincipalConverter());
		_options.Converters.Add(new CandidArgConverter());
		_options.Converters.Add(new JsonStringEnumConverter());
	}

	public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, _options);
	public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _options);

	public static string Serialize<T>(T value, Action<JsonSerializerOptions> optionsConfig)
	{
		var options = new JsonSerializerOptions(_options);
		optionsConfig.Invoke(options);
		return JsonSerializer.Serialize(value, options);
	}

	public static T Deserialize<T>(string json, Action<JsonSerializerOptions> optionsConfig)
	{
		var options = new JsonSerializerOptions(_options);
		optionsConfig.Invoke(options);
		return JsonSerializer.Deserialize<T>(json, options);
	}
}

public class PrincipalConverter : JsonConverter<Principal>
{
	public override Principal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var text = reader.GetString();
		return Principal.FromText(text);
	}

	public override void Write(Utf8JsonWriter writer, Principal value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToText());
	}
}

public class CandidArgConverter : JsonConverter<CandidArg>
{
	public override CandidArg Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var hex = reader.GetString();
		var bytes = hex.HexToByteArray();
		return CandidArg.FromBytes(bytes);
	}

	public override void Write(Utf8JsonWriter writer, CandidArg value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var v in value.Values)
		{
			writer.WriteStringValue($"{v.Value} ({v.Type})");
		}
		writer.WriteEndArray();
	}
}
