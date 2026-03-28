using LLMDemo.Concept.ParallelAgentInvocation;
using LLMDemo.Core.Abstractions;
using LLMDemo.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

var builder = Host.CreateApplicationBuilder(args);

// Register core LM Studio services (IChatCompletionService, IAgentRunner, etc.)
builder.Services.AddLlmDemoCore();

// ----- Register concept demo projects here -----
builder.Services.AddTransient<IConceptDemo, ParallelAgentInvocationDemo>();

var host = builder.Build();

// Discover all registered demos
var demos = host.Services.GetServices<IConceptDemo>().ToList();

if (demos.Count == 0)
{
    AnsiConsole.MarkupLine("[yellow]No concept demos registered.[/] Add IConceptDemo implementations and register them in Program.cs.");
    return;
}

// Interactive menu
var selected = AnsiConsole.Prompt(
    new SelectionPrompt<IConceptDemo>()
        .Title("[green]Select a demo to run:[/]")
        .UseConverter(d => $"{d.Name} - {d.Description}")
        .AddChoices(demos));

AnsiConsole.MarkupLine($"\n[bold]Running:[/] {selected.Name}\n");

if (selected is IChatConceptDemo chatDemo)
{
    // Interactive chat loop
    AnsiConsole.MarkupLine("[grey]Type your message and press Enter. Type [bold]exit[/] or [bold]quit[/] to stop.[/]\n");

    var defaultPrompt = chatDemo.DefaultPrompt;

    while (true)
    {
        var prompt = new TextPrompt<string>("[blue]You:[/]")
            .AllowEmpty();

        // Pre-fill the default prompt (only for the very first turn if set)
        if (defaultPrompt is not null)
        {
            prompt.DefaultValue(defaultPrompt).ShowDefaultValue(false);
            defaultPrompt = null; // only pre-fill once
        }

        var userInput = AnsiConsole.Prompt(prompt);

        if (string.IsNullOrWhiteSpace(userInput))
            continue;

        if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
            userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine("[grey]Exiting chat. Goodbye![/]");
            break;
        }

        string response = string.Empty;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Thinking...", async _ =>
            {
                response = await chatDemo.ProcessMessageAsync(userInput);
            });

        AnsiConsole.MarkupLine($"[green]Assistant:[/] {Markup.Escape(response)}\n");
    }
}
else
{
    // Standard one-shot demo
    var swDemo = System.Diagnostics.Stopwatch.StartNew();
    await selected.RunAsync();
    swDemo.Stop();
    AnsiConsole.MarkupLine($"[green]Demo finished in {swDemo.ElapsedMilliseconds} ms.[/]");
}
