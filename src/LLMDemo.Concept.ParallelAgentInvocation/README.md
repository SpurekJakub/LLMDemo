# LLMDemo.Concept.ParallelAgentInvocation

> **Note:** The project folder is currently named `ParallelAgentsInvokation` (typo). Rename to `ParallelAgentInvocation` before implementation.

## Overview

An **Orchestrator Agent** that can invoke **sub-agents in parallel** via a tool-calling mechanism. The orchestrator plans which sub-agents to call, determines which can run concurrently, and batches them accordingly. For the demo, a **Jokes sub-agent** generates random short jokes — asking for "5 jokes" triggers 5 parallel sub-agent invocations.

### Flow

```
User
  → CLI chat loop (LLMDemo host)
    → Orchestrator Agent (via AgentRunner)
      → LLM decides to call `invoke_subagents` tool
        → Tool runs N sub-agents in parallel (each via AgentRunner)
        → Results aggregated as JSON
      → Returned to Orchestrator
    → Final response to user
```

---

## Implementation Plan

### Phase 1: Core — New DTOs and Abstractions

All new types go in `LLMDemo.Core` so they're reusable across concept demos.

#### New files

| File | Description |
|---|---|
| `Core/Models/ToolDefinition.cs` | `record ToolDefinition(string Name, string Description, BinaryData ParametersSchema)` — provider-agnostic tool metadata sent to the LLM |
| `Core/Models/ToolCallInfo.cs` | `record ToolCallInfo(string Id, string FunctionName, string Arguments)` — represents the LLM requesting a tool call |
| `Core/Models/AgentDefinition.cs` | `record AgentDefinition(string Name, string SystemPrompt, IReadOnlyList<ITool> Tools)` — defines an agent with its identity, instructions, and available tools |
| `Core/Abstractions/ITool.cs` | Interface: `Name`, `Description`, `ParametersSchema` (JSON schema as `BinaryData`), `Task<string> ExecuteAsync(string arguments, CancellationToken)` |
| `Core/Abstractions/IAgentRunner.cs` | Interface: `Task<string> RunAsync(AgentDefinition agent, string userMessage, CancellationToken)` — encapsulates the full agent loop |
| `Core/Abstractions/IChatConceptDemo.cs` | Extends `IConceptDemo` with `Task<string> ProcessMessageAsync(string userMessage, CancellationToken)` — allows the CLI host to drive the chat loop externally |

#### Modified files

| File | Changes |
|---|---|
| `Core/Models/ConversationMessage.cs` | Add `MessageRole.Tool` enum value. Add optional `string? ToolCallId` (for tool result messages) and `IReadOnlyList<ToolCallInfo>? ToolCalls` (for assistant messages containing tool calls). |
| `Core/Models/CompletionResult.cs` | Make `Content` nullable (`string?`). Add `IReadOnlyList<ToolCallInfo>? ToolCalls` and convenience `bool IsToolCall => ToolCalls is { Count: > 0 }`. |
| `Core/Abstractions/IChatCompletionService.cs` | Add second overload: `CompleteAsync(messages, tools, model?, cancellationToken)` accepting `IReadOnlyList<ToolDefinition>`. Keep existing overload for backward compat. |
| `Core/Clients/LmStudioChatCompletionService.cs` | Implement the new overload: convert `ToolDefinition` → OpenAI SDK `ChatTool.CreateFunctionTool()`, handle `MessageRole.Tool` → `ChatMessage.CreateToolMessage()`, parse `ChatToolCall` from response into `ToolCallInfo`, handle `ChatFinishReason.ToolCalls`. |
| `Core/Extensions/ServiceCollectionExtensions.cs` | Register `IAgentRunner` → `AgentRunner` as transient. |

---

### Phase 2: Core — Agent Runner

New file: `Core/Services/AgentRunner.cs`

- Dependencies: `IChatCompletionService`, `ILogger<AgentRunner>`
- Algorithm:
  1. Build conversation: `[SystemMessage(agent.SystemPrompt), UserMessage(userMessage)]`
  2. Extract `ToolDefinition` list from `agent.Tools`
  3. Call `IChatCompletionService.CompleteAsync(conversation, tools)`
  4. If `result.IsToolCall`:
     - Add assistant message (with tool calls) to conversation
     - For each tool call, find matching `ITool` by name, call `ExecuteAsync(arguments)`
     - Add tool result messages to conversation
     - Loop back to step 3
  5. If content response, return `result.Content`
- **Safety:** max 10 iterations to prevent infinite loops

---

### Phase 3: Rename & Setup Concept Project

1. **Rename** folder `LLMDemo.Concept.ParallelAgentsInvokation` → `LLMDemo.Concept.ParallelAgentInvocation` (fix typo)
2. Rename `.csproj` file accordingly
3. Update solution: `dotnet sln add src/LLMDemo.Concept.ParallelAgentInvocation`
4. Add `<ProjectReference>` from `LLMDemo.csproj` → concept project

---

### Phase 4: Concept Project — Parallel Sub-Agent Invocation

#### `InvokeSubAgentsTool.cs`

- Implements `ITool`
- Name: `"invoke_subagents"`
- Description: "Invoke one or more sub-agents in parallel. Provide an array of agent requests."
- Parameters JSON schema:
  ```json
  {
    "type": "object",
    "properties": {
      "requests": {
        "type": "array",
        "items": {
          "type": "object",
          "properties": {
            "agentName": { "type": "string" },
            "prompt": { "type": "string" }
          },
          "required": ["agentName", "prompt"]
        }
      }
    },
    "required": ["requests"]
  }
  ```
- Constructor: `IAgentRunner agentRunner, IDictionary<string, AgentDefinition> subAgents`
- `ExecuteAsync`:
  1. Parse JSON arguments → extract `requests` array
  2. For each request, look up `AgentDefinition` by name
  3. `Task.WhenAll` running all via `IAgentRunner.RunAsync` in parallel
  4. Return aggregated JSON: `[{ "agentName": "...", "response": "..." }]`
  5. If a sub-agent fails, include error in result rather than throwing

#### `ParallelAgentInvocationDemo.cs`

- Implements `IChatConceptDemo`
- `Name`: `"Parallel Agent Invocation"`
- `Description`: `"Orchestrator agent that invokes sub-agents in parallel"`
- Defines **Joke sub-agent**:
  ```
  AgentDefinition("jokes", "You are a comedian. Generate a single random short joke. Be creative and vary your style.", tools: [])
  ```
- Defines **Orchestrator agent** with system prompt:
  > You are an orchestrator agent. You have access to sub-agents that you can invoke using the `invoke_subagents` tool. Before executing, create a plan: determine which agents to call, how many times, and which can run in parallel in the same batch.
  >
  > Available sub-agents:
  > - **jokes** — generates a single random short joke
  >
  > When asked for multiple items (e.g. "tell me 5 jokes"), invoke the appropriate sub-agent multiple times in a single parallel batch. Present the collected results to the user.

- `ProcessMessageAsync` → `IAgentRunner.RunAsync(orchestratorAgent, userMessage)`

---

### Phase 5: Wire Up LLMDemo CLI Host

Modify `src/LLMDemo/Program.cs`:

1. Register: `builder.Services.AddSingleton<IConceptDemo, ParallelAgentInvocationDemo>()`
2. After demo selection, check if selected demo is `IChatConceptDemo`:
   - **Yes** → interactive chat loop (Spectre.Console): read input → `ProcessMessageAsync` → print response → repeat until user types "exit" or "quit"
   - **No** → call `RunAsync()` as before (backward compatible)

---

## Decisions

| Decision | Rationale |
|---|---|
| Rename project (fix typo) | `ParallelAgentsInvokation` → `ParallelAgentInvocation` |
| Agent runner lives in Core | Reusable across all concept demos |
| Chat loop lives in LLMDemo host | CLI behavior owned by the host project, not Core |
| `IChatConceptDemo` extends `IConceptDemo` | Backward compatible — existing demos still work |
| Max 10 tool-call iterations | Safety guard against infinite loops in AgentRunner |
| Sub-agent tool returns structured JSON | Orchestrator can reason about individual results |
| Partial results on sub-agent failure | Return error messages for failed agents instead of failing entirely |

## Prerequisites

- **LM Studio model must support tool/function calling** — not all models do. Use a recent model that advertises tool-use capability (e.g. Llama 3, Mistral, Qwen with function calling support).
- .NET 10 SDK
- LM Studio running locally with API server enabled

## Example Interaction

```
> tell me 5 jokes

[Orchestrator plans: invoke "jokes" agent 5 times in parallel]
[Calls invoke_subagents tool with 5 requests]
[5 sub-agents run concurrently via Task.WhenAll]
[Results aggregated and returned to orchestrator]

Here are 5 jokes for you:

1. Why don't scientists trust atoms? Because they make up everything!
2. I told my wife she was drawing her eyebrows too high. She looked surprised.
3. What do you call a fake noodle? An impasta!
4. Why did the scarecrow win an award? He was outstanding in his field!
5. I'm reading a book about anti-gravity. It's impossible to put down!

> exit
```
