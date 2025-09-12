using System.Text;

namespace UltimateMessengerSuggestions.Telegram.Extensions;

public static class FilterParser
{
	private static readonly Dictionary<string, string> Aliases = new()
	{
		{ "d", "description" },
		{ "t", "tags" },
		{ "i", "id" }
	};

	public static List<string> ParseFilters(string input)
	{
		// input: "/get d:\"эмо\" t:рика i:jCtzJvqf"

		var result = new List<string>();

		// tokenization taking into account quotes
		var tokens = Tokenize(input);

		foreach (var token in tokens)
		{
			var parts = token.Split(':', 2);
			if (parts.Length != 2) continue;

			var key = parts[0];
			var value = parts[1].Trim('"');

			if (Aliases.TryGetValue(key, out var fullKey))
			{
				result.Add($"{fullKey}:{value}");
			}
		}

		return result;
	}

	private static List<string> Tokenize(string input)
	{
		var tokens = new List<string>();
		var sb = new StringBuilder();
		bool inQuotes = false;

		foreach (var c in input)
		{
			if (c == '"')
			{
				inQuotes = !inQuotes;
				continue;
			}

			if (char.IsWhiteSpace(c) && !inQuotes)
			{
				if (sb.Length > 0)
				{
					tokens.Add(sb.ToString());
					sb.Clear();
				}
			}
			else
			{
				sb.Append(c);
			}
		}

		if (sb.Length > 0)
			tokens.Add(sb.ToString());

		return tokens;
	}
}
