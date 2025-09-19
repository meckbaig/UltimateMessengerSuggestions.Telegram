using static UltimateMessengerSuggestions.Telegram.Extensions.FilterParser;

namespace UltimateMessengerSuggestions.Telegram.Extensions;

internal static class CommandUsageFormatter
{
	public static string GetUsageTemplate(CommandType commandType)
	{
		var order = GetPositionalOrder(commandType);
		var cmd = commandType.ToString().ToLowerInvariant();

		return $@"Usage of /{cmd}:

1️. Positional input (each line = one parameter, order matters):
{string.Join(Environment.NewLine, order.Select(ExampleFor))}

2️. Explicit keys (quotes are optional, value continues until next key):
/{cmd} {string.Join(" ", order.Select(p => $"{AliasFor(p)}:<{p}>"))}

3️. Escaping colons inside text:
   Example: /{cmd} d:I knew exactly\: this will not pass

Legend:
- d|description — free text, can be multiline
- t|tags — comma separated list of tags
- i|id — card identifier
- p|isPublic — true/false
";
	}

	private static string[] GetPositionalOrder(CommandType commandType)
	{
		return PositionalOrder.TryGetValue(commandType, out var order) ? order : [];
	}

	private static string AliasFor(string fullKey)
	{
		return Aliases.FirstOrDefault(kv => kv.Value.Equals(fullKey, StringComparison.OrdinalIgnoreCase)).Key
			?? throw new InvalidOperationException($"No alias found for key '{fullKey}'");
	}

	private static string ExampleFor(string fullKey) => fullKey switch
	{
		"description" => "description text",
		"tags" => "tag1, tag2, tag3",
		"id" => "a1b2c3",
		"isPublic" => "true/false",
		_ => fullKey
	};
}
