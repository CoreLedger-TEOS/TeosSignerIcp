namespace TeosSigner.Icp.TeosApi.OData;

public enum ConditionOperand
{
	None = 0,
	And = 1,
	Or = 2
}

public interface ICompilableCondition
{
	string Compile();
}

public class ConditionBuilder : ICompilableCondition
{
	private readonly string _operand;

	public ConditionBuilder(ConditionOperand operand)
	{
		_operand = operand switch
		{
			ConditionOperand.And => "AND",
			ConditionOperand.Or => "OR",
			ConditionOperand.None => throw new ArgumentOutOfRangeException(nameof(operand)),
			_ => throw new ArgumentOutOfRangeException(nameof(operand))
		};
	}

	private readonly List<ICompilableCondition> _conditions = new List<ICompilableCondition>();

	public ConditionBuilder AddCondition(ICompilableCondition condition)
	{
		_conditions.Add(condition);
		return this;
	}

	public string Compile()
	{
		if (_conditions.Count == 0)
		{
			throw new InvalidOperationException("No conditions have been defined.");
		}

		var result = $"({_conditions.First().Compile()})";

		if (_conditions.Count == 1)
		{
			return result;
		}

		result = _conditions.Skip(1).Aggregate(result, (acc, condition) => acc + $" {_operand} ({condition.Compile()})");

		return result;
	}
}

public class FilterCondition : ICompilableCondition
{
	private readonly string _property;
	private readonly string _operat;
	private readonly string _value;

	public FilterCondition(string property, string operat, string value)
	{
		_property = property;
		_operat = operat;
		_value = value;
	}

	public string Compile()
	{
		return $"{_property} {_operat} {_value}";
	}
}
