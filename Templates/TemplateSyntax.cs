using System.Text.RegularExpressions;

namespace SkillIssue.Templates;

internal enum TemplateTagKind
{
    Invalid,
    Variable,
    If,
    Else,
    EndIf,
    ForEach,
    EndForEach,
    Include,
}

internal readonly record struct TemplateTag(
    TemplateTagKind Kind,
    int Index,
    int Length,
    string Name = "",
    string? Argument = null
);

internal static partial class TemplateSyntax
{
    private const string Identifier = "[A-Za-z_][A-Za-z0-9_]*";

    public static bool IsFileName(string fileName) => FileNamePattern().IsMatch(fileName);

    public static List<TemplateTag> ReadTags(string template, CancellationToken cancellationToken)
    {
        var tags = new List<TemplateTag>();
        foreach (Match match in TagBoundaryPattern().Matches(template))
        {
            cancellationToken.ThrowIfCancellationRequested();
            tags.Add(ParseTag(match));
        }

        return tags;
    }

    private static TemplateTag ParseTag(Match match)
    {
        var tag = new TemplateTag(TemplateTagKind.Invalid, match.Index, match.Length);
        if (match.Length == 2)
        {
            return tag;
        }

        string content = match.Value[2..^2].Trim();
        int separator = 0;
        while (separator < content.Length && !char.IsWhiteSpace(content[separator]))
        {
            separator++;
        }

        string command = content[..separator];
        string arguments = content[separator..].TrimStart();
        switch (command)
        {
            case "#if":
                var condition = ConditionPattern().Match(arguments);
                if (condition.Success)
                {
                    var literal = condition.Groups["literal"];
                    return tag with
                    {
                        Kind = TemplateTagKind.If,
                        Name = condition.Groups["name"].Value,
                        Argument = literal.Success ? literal.Value : null,
                    };
                }
                break;

            case "#foreach":
                var loop = LoopPattern().Match(arguments);
                if (loop.Success)
                {
                    return tag with
                    {
                        Kind = TemplateTagKind.ForEach,
                        Name = loop.Groups["name"].Value,
                        Argument = loop.Groups["collection"].Value,
                    };
                }
                break;

            case "#include" when IsFileName(arguments):
                return tag with { Kind = TemplateTagKind.Include, Name = arguments };

            case "else" when arguments.Length == 0:
                return tag with { Kind = TemplateTagKind.Else };

            case "/if" when arguments.Length == 0:
                return tag with { Kind = TemplateTagKind.EndIf };

            case "/foreach" when arguments.Length == 0:
                return tag with { Kind = TemplateTagKind.EndForEach };

            default:
                if (arguments.Length == 0 && IdentifierPattern().IsMatch(command))
                {
                    return tag with { Kind = TemplateTagKind.Variable, Name = command };
                }
                break;
        }

        return tag;
    }

    [GeneratedRegex(@"\{\{(?:[^{}]*\}\})?")]
    private static partial Regex TagBoundaryPattern();

    [GeneratedRegex(@"\A[A-Za-z0-9_][A-Za-z0-9_.-]*\.html\z")]
    private static partial Regex FileNamePattern();

    [GeneratedRegex(@"\A" + Identifier + @"\z")]
    private static partial Regex IdentifierPattern();

    [GeneratedRegex(@"\A(?<name>" + Identifier + """)(?:\s*==\s*"(?<literal>[^"{}]*)")?\z""")]
    private static partial Regex ConditionPattern();

    [GeneratedRegex(@"\A(?<name>" + Identifier + @")\s+in\s+(?<collection>" + Identifier + @")\z")]
    private static partial Regex LoopPattern();
}
