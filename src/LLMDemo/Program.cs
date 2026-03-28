using LLMDemo.Concept.ParallelAgentInvocation;
using LLMDemo.Core.Abstractions;
using LLMDemo.Core.Configuration;
using LLMDemo.Core.Extensions;
using LLMDemo.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Spectre.Console;

var builder = Host.CreateApplicationBuilder(args);

// Register core LM Studio services (IChatCompletionService, IAgentRunner, etc.)
builder.Services.AddLlmDemoCore();

var host = builder.Build();

// ── Resolve model from config ────────────────────────────────────────────────
var lmOptions = host.Services.GetRequiredService<IOptions<LmStudioOptions>>().Value;
var model = lmOptions.DefaultModel;

if (string.IsNullOrWhiteSpace(model))
{
    AnsiConsole.MarkupLine("[red]Error:[/] No model configured. Set [bold]LmStudio:DefaultModel[/] in appsettings.json.");
    return;
}

// ── Hardcoded concept list — no DI registration needed ───────────────────────
IConceptDemo[] demos =
[
    ActivatorUtilities.CreateInstance<ParallelAgentInvocationDemo>(host.Services),
];

bool isInteractive = AnsiConsole.Profile.Capabilities.Interactive;

// ── Demo selection ────────────────────────────────────────────────────────────
IConceptDemo selected;

if (demos.Length == 1)
{
    selected = demos[0];
    if (!isInteractive)
        Console.WriteLine($"Auto-selecting the only demo: {selected.Name}");
}
else if (isInteractive)
{
    selected = AnsiConsole.Prompt(
        new SelectionPrompt<IConceptDemo>()
            .Title("[green]Select a demo to run:[/]")
            .UseConverter(d => d.Name)
            .AddChoices(demos));
}
else
{
    Console.WriteLine("Non-interactive terminal. Available demos:");
    for (int i = 0; i < demos.Length; i++)
        Console.WriteLine($"  [{i + 1}] {demos[i].Name}");

    Console.Write("Enter number: ");
    var input = Console.ReadLine()?.Trim();

    if (!int.TryParse(input, out int choice) || choice < 1 || choice > demos.Length)
    {
        Console.Error.WriteLine($"Invalid selection '{input}'. Exiting.");
        return;
    }

    selected = demos[choice - 1];
}

AnsiConsole.MarkupLine($"\n[bold]Running:[/] {selected.Name}  [grey](model: {Markup.Escape(model)})[/]\n");

// ── Chat loop ─────────────────────────────────────────────────────────────────
if (isInteractive)
    AnsiConsole.MarkupLine("[grey]Type your message and press Enter. Type [bold]exit[/] or [bold]quit[/] to stop.[/]\n");
else
    Console.WriteLine("Type your message and press Enter. Type 'exit' or 'quit' to stop.\n");

var defaultPrompt = selected.DefaultPrompt;

while (true)
{
    string? userInput;

    if (isInteractive)
    {
        var prompt = new TextPrompt<string>("[blue]You:[/]")
            .AllowEmpty();

        if (defaultPrompt is not null)
        {
            prompt.DefaultValue(defaultPrompt).ShowDefaultValue(true);
            defaultPrompt = null; // only pre-fill once
        }

        userInput = AnsiConsole.Prompt(prompt);
    }
    else
    {
        if (defaultPrompt is not null)
        {
            userInput = defaultPrompt;
            defaultPrompt = null;
            Console.WriteLine($"You: {userInput}");
        }
        else
        {
            Console.Write("You: ");
            userInput = Console.ReadLine();
        }
    }

    if (string.IsNullOrWhiteSpace(userInput))
        continue;

    if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        AnsiConsole.MarkupLine("[grey]Exiting chat. Goodbye![/]");
        break;
    }

    CompletionResponse response = null!;

    if (isInteractive)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Thinking...", async _ =>
            {
                response = await selected.ProcessAsync(userInput, model);
            });
    }
    else
    {
        Console.WriteLine("Thinking...");
        response = await selected.ProcessAsync(userInput, model);
    }

    AnsiConsole.MarkupLine($"[green]Assistant:[/] {Markup.Escape(response.Text)}\n");

    // ── Metrics ───────────────────────────────────────────────────────────────
    if (response.Steps.Count > 0)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[grey]Step[/]")
            .AddColumn("[grey]Agent[/]")
            .AddColumn("[grey]Requested At[/]")
            .AddColumn("[grey]Responded At[/]")
            .AddColumn("[grey]Duration[/]")
            .AddColumn("[grey]Prompt tokens[/]")
            .AddColumn("[grey]Completion tokens[/]")
            .AddColumn("[grey]Total tokens[/]");

        for (int i = 0; i < response.Steps.Count; i++)
        {
            var s = response.Steps[i];
            table.AddRow(
                $"[grey]{i + 1}[/]",
                $"[grey]{Markup.Escape(s.AgentName)}[/]",
                $"[grey]{s.RequestedAt.ToLocalTime():HH:mm:ss.fff}[/]",
                $"[grey]{s.RespondedAt.ToLocalTime():HH:mm:ss.fff}[/]",
                $"[grey]{s.Duration.TotalMilliseconds:F0} ms[/]",
                $"[grey]{s.PromptTokens?.ToString() ?? "-"}[/]",
                $"[grey]{s.CompletionTokens?.ToString() ?? "-"}[/]",
                $"[grey]{s.TotalTokens?.ToString() ?? "-"}[/]");
        }

        AnsiConsole.Write(table);

        // Aggregated totals
        var totalPrompt = response.Steps.Sum(s => s.PromptTokens ?? 0);
        var totalCompletion = response.Steps.Sum(s => s.CompletionTokens ?? 0);
        var totalTokens = response.Steps.Sum(s => s.TotalTokens ?? 0);

        AnsiConsole.MarkupLine(
            $"[grey]Total: {response.TotalDuration.TotalMilliseconds:F0} ms " +
            $"| {totalPrompt}p / {totalCompletion}c / {totalTokens}t tokens[/]\n");
    }
}
