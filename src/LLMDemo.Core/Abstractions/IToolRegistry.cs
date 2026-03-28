using LLMDemo.Core.Models;

namespace LLMDemo.Core.Abstractions;

public interface IToolRegistry
{
    void Add(ITool tool);
    ITool Get(string name);
    IEnumerable<ToolDefinition> GetToolsForAgent(string agentName);
}
