using LLMDemo.Core.Models;

namespace LLMDemo.Core.Abstractions;

public interface ITool
{
    ToolDefinition Definition { get; }
    Task<string> InvokeAsync(string argumentsJson, CancellationToken cancellationToken = default);
}
