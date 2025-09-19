using System.Text.RegularExpressions;

namespace UltimateMessengerSuggestions.Telegram.Extensions;

internal static class FilterParser
{
	/// <summary>
	/// Maps short aliases to full property names.
	/// </summary>
	public static readonly Dictionary<string, string> Aliases = new()
	{
		{ "d", "description" },
		{ "t", "tags" },
		{ "p", "isPublic" },
		{ "i", "id" }
	};

	/// <summary>
	/// Defines the order of positional arguments for each command type.
	/// </summary>
	public static readonly Dictionary<CommandType, string[]> PositionalOrder = new()
	{
		{ CommandType.Get, new[] { "description", "tags", "id" } },
		{ CommandType.Add, new[] { "description", "tags", "isPublic" } },
		{ CommandType.Edit, new[] { "description", "tags", "isPublic" } }
	};

	/// <summary>
	/// Parses the input string into a dictionary of key-value pairs.
	/// </summary>
	/// <param name="input">Input user string without command.</param>
	/// <param name="commandType">Type of command to determine parsing mode and positional argument order.</param>
	/// <returns>Key-value dictionary of filters.</returns>
	public static Dictionary<string, string> Parse(string input, CommandType commandType)
	{
		var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (string.IsNullOrWhiteSpace(input)) return result;

		if (ContainsExplicitKeys(input))
			return ParseKeyValueMode(input);

		return ParsePositionalMode(input, commandType);
	}

	/// <summary>
	/// Checks if the input contains explicit key-value pairs.
	/// </summary>
	/// <param name="input">Input string.</param>
	/// <returns><see langword="true"/> if the input contains explicit key-value pairs; otherwise <see langword="false"/>.</returns>
	private static bool ContainsExplicitKeys(string input)
	{
		// search for d:, t:, p:, i: that are not escaped by a backslash
		return Regex.IsMatch(input, @"(?<!\\)\b(d|t|p|i)\s*:", RegexOptions.IgnoreCase);
	}

	/// <summary>
	/// Parses input in key-value mode.
	/// </summary>
	/// <param name="input">Input string.</param>
	/// <returns>Key-value dictionary.</returns>
	private static Dictionary<string, string> ParseKeyValueMode(string input)
	{
		var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		var pattern = new Regex(@"(?<!\\)\b([a-zA-Z]+)\s*:", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		var matches = pattern.Matches(input);

		if (matches.Count == 0)
		{
			// nothing found - treat entire input as description
			result[Aliases["d"]] = UnescapeAndTrim(input);
			return result;
		}

		for (int i = 0; i < matches.Count; i++)
		{
			var keyAlias = matches[i].Groups[1].Value.ToLower();
			var valueStart = matches[i].Index + matches[i].Length;
			var valueEnd = (i + 1 < matches.Count) ? matches[i + 1].Index : input.Length;
			var rawValue = input.Substring(valueStart, valueEnd - valueStart).Trim();

			// remove surrounding quotes and unescape colons
			rawValue = TrimSurroundingQuotes(rawValue);
			rawValue = UnescapeAndTrim(rawValue);

			if (Aliases.TryGetValue(keyAlias, out var fullKey))
			{
				result[fullKey] = rawValue;
			}
			else
			{
				// unknown key - put it in description as a fallback (to not lose anything)
				if (!result.ContainsKey(Aliases["d"])) 
					result[Aliases["d"]] = rawValue;
				else 
					result[Aliases["d"]] = $"{result[Aliases["d"]]} {rawValue}";
			}
		}

		return result;
	}

	/// <summary>
	/// Parses input in positional mode based on command type.
	/// </summary>
	/// <param name="input">Input string.</param>
	/// <param name="commandType">Type of command to determine parsing mode and positional argument order.</param>
	/// <returns>Key-value dictionary.</returns>
	private static Dictionary<string, string> ParsePositionalMode(string input, CommandType commandType)
	{
		var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// remove empty lines and trim each line
		var lines = input
			.Split(["\r\n", "\n"], StringSplitOptions.None)
			.Select(l => l.Trim())
			.Where(l => !string.IsNullOrEmpty(l))
			.ToArray();

		var order = PositionalOrder[commandType];

		for (int i = 0; i < lines.Length && i < order.Length; i++)
		{
			result[order[i]] = UnescapeAndTrim(TrimSurroundingQuotes(lines[i]));
		}

		return result;
	}

	/// <summary>
	/// Trims surrounding quotes from a string if they exist.
	/// </summary>
	/// <param name="s">Input string.</param>
	/// <returns>Substring without quotes.</returns>
	private static string TrimSurroundingQuotes(string s)
	{
		if (string.IsNullOrEmpty(s)) 
			return s;
		if (s.Length >= 2 && s.StartsWith('\"') && s.EndsWith('\"')) 
			return s.Substring(1, s.Length - 2);
		return s;
	}

	/// <summary>
	/// Replaces escaped colons (\:) with actual colons (:) and trims whitespace.
	/// </summary>
	/// <param name="s">Input string.</param>
	/// <returns>String without escaped colons.</returns>
	private static string UnescapeAndTrim(string s)
	{
		return string.IsNullOrEmpty(s)
			? s 
			: s.Replace("\\:", ":").Trim();
	}

	/// <summary>
	/// Type of command to determine parsing mode and positional argument order.
	/// </summary>
	internal enum CommandType
	{
		Get,
		Add,
		Edit
	}
}
