using System.Collections;
using System.Net;
using System.Text;

namespace SkillIssue.Templates;

using RenderState = (
    IReadOnlyDictionary<string, object?> Model,
    StringBuilder Output,
    HashSet<string> ActiveFiles,
    Dictionary<string, object?> Locals,
    CancellationToken CancellationToken
);

public sealed class TemplateEngine
{
    public static async Task<string> RenderAsync(
        string fileName,
        IReadOnlyDictionary<string, object?> model,
        CancellationToken cancellationToken
    )
    {
        var output = new StringBuilder();
        var activeFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var locals = new Dictionary<string, object?>();

        await RenderFileAsync(fileName, (model, output, activeFiles, locals, cancellationToken));

        return output.ToString();
    }

    private static async Task RenderFileAsync(string fileName, RenderState render)
    {
        if (!TemplateSyntax.IsTemplatePath(fileName))
        {
            throw new InvalidOperationException($"Invalid template path: {fileName}");
        }

        if (!render.ActiveFiles.Add(fileName))
        {
            throw new InvalidOperationException($"Circular include: {fileName}");
        }

        string filePath = Path.Combine(AppContext.BaseDirectory, "Views", fileName);
        string template = await File.ReadAllTextAsync(filePath, render.CancellationToken);
        var tags = TemplateSyntax.ReadTags(template, render.CancellationToken);
        await RenderBlockAsync(fileName, template, tags, render, 0, true);

        render.ActiveFiles.Remove(fileName);
    }

    private static async Task<int> RenderBlockAsync(
        string fileName,
        string template,
        List<TemplateTag> tags,
        RenderState render,
        int blockStart,
        bool visible
    )
    {
        var (model, output, _, locals, cancellationToken) = render;

        TemplateTag? openingTag = null;
        int offset = 0;
        if (blockStart > 0)
        {
            var previousTag = tags[blockStart - 1];
            openingTag = previousTag;
            offset = previousTag.Index + previousTag.Length;
        }

        var block = openingTag?.Kind;
        int index = blockStart;
        for (; index < tags.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tag = tags[index];

            if (visible)
            {
                output.Append(template, offset, tag.Index - offset);
            }

            switch (tag.Kind)
            {
                case TemplateTagKind.If:
                    bool showIf = visible && ReadCondition(tag);
                    int branchEnd = await RenderBodyAsync(index, showIf);
                    if (tags[branchEnd].Kind == TemplateTagKind.Else)
                    {
                        bool showElse = visible && !showIf;
                        branchEnd = await RenderBodyAsync(branchEnd, showElse);
                    }

                    index = branchEnd;
                    break;

                case TemplateTagKind.ForEach when tag.Argument is string collectionName:
                    int loopStart = index;
                    if (visible)
                    {
                        object? collection = ReadValue(collectionName, tag);
                        if (collection is string || collection is not IEnumerable items)
                        {
                            throw InvalidTag(tag);
                        }

                        bool hadLocal = locals.TryGetValue(tag.Name, out object? previous);
                        foreach (object? item in items)
                        {
                            locals[tag.Name] = item;
                            index = await RenderBodyAsync(loopStart, true);
                        }

                        if (hadLocal)
                        {
                            locals[tag.Name] = previous;
                        }
                        else
                        {
                            locals.Remove(tag.Name);
                        }
                    }

                    if (index == loopStart)
                    {
                        index = await RenderBodyAsync(loopStart, false);
                    }
                    break;

                case TemplateTagKind.Else when block == TemplateTagKind.If:
                case TemplateTagKind.EndIf when block is TemplateTagKind.If or TemplateTagKind.Else:
                case TemplateTagKind.EndForEach when block == TemplateTagKind.ForEach:
                    return index;

                case TemplateTagKind.Include:
                    if (visible)
                    {
                        await RenderFileAsync(tag.Name, render);
                    }
                    break;

                case TemplateTagKind.Variable:
                    if (visible)
                    {
                        object? value = ReadValue(tag.Name, tag);
                        string? encodedValue = WebUtility.HtmlEncode(value?.ToString());
                        output.Append(encodedValue);
                    }
                    break;

                default:
                    throw InvalidTag(tag);
            }

            offset = tags[index].Index + tags[index].Length;
        }

        if (openingTag is TemplateTag unclosedTag)
        {
            throw InvalidTag(unclosedTag);
        }

        output.Append(template, offset, template.Length - offset);
        return index;

        Task<int> RenderBodyAsync(int openingTagIndex, bool showBody)
        {
            return RenderBlockAsync(
                fileName,
                template,
                tags,
                render,
                openingTagIndex + 1,
                showBody
            );
        }

        bool ReadCondition(TemplateTag tag)
        {
            object? value = ReadValue(tag.Name, tag);
            if (tag.Argument is string literal)
            {
                return value is string text ? text == literal : throw InvalidTag(tag);
            }

            return value is bool condition ? condition : throw InvalidTag(tag);
        }

        object? ReadValue(string name, TemplateTag tag)
        {
            return locals.TryGetValue(name, out object? value) || model.TryGetValue(name, out value)
                ? value
                : throw InvalidTag(tag);
        }

        InvalidOperationException InvalidTag(TemplateTag tag)
        {
            string source = template.Substring(tag.Index, tag.Length);
            return new InvalidOperationException(
                $"Invalid tag or value in {fileName} at offset {tag.Index}: {source}"
            );
        }
    }
}
