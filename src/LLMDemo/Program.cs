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

bool isInteractive = AnsiConsole.Profile.Capabilities.Interactive;

// ── Demo selection ────────────────────────────────────────────────────────────
IConceptDemo selected;

if (isInteractive)
{
    selected = AnsiConsole.Prompt(
        new SelectionPrompt<IConceptDemo>()
            .Title("[green]Select a demo to run:[/]")
            .UseConverter(d => $"{d.Name} - {d.Description}")
            .AddChoices(demos));
}
else if (demos.Count == 1)
{
    selected = demos[0];
    Console.WriteLine($"Non-interactive terminal: auto-selecting the only demo: {selected.Name}");
}
else
{
    Console.WriteLine("Non-interactive terminal. Available demos:");
    for (int i = 0; i < demos.Count; i++)
        Console.WriteLine($"  [{i + 1}] {demos[i].Name} - {demos[i].Description}");

    Console.Write("Enter number: ");
    var input = Console.ReadLine()?.Trim();

    if (!int.TryParse(input, out int choice) || choice < 1 || choice > demos.Count)
    {
        Console.Error.WriteLine($"Invalid selection '{input}'. Exiting.");
        return;
    }

    selected = demos[choice - 1];
}

AnsiConsole.MarkupLine($"\n[bold]Running:[/] {selected.Name}\n");

// ── Run the selected demo ─────────────────────────────────────────────────────
if (selected is IChatConceptDemo chatDemo)
{
    // Interactive chat loop
    if (isInteractive)
        AnsiConsole.MarkupLine("[grey]Type your message and press Enter. Type [bold]exit[/] or [bold]quit[/] to stop.[/]\n");
    else
        Console.WriteLine("Type your message and press Enter. Type 'exit' or 'quit' to stop.\n");

    var defaultPrompt = chatDemo.DefaultPrompt;

    while (true)
    {
        string? userInput;

        if (isInteractive)
        {
            var prompt = new TextPrompt<string>("[blue]You:[/]")
                .AllowEmpty();

            if (defaultPrompt is not null)
            {
                prompt.DefaultValue(defaultPrompt).ShowDefaultValue(false);
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

        string response = string.Empty;

        if (isInteractive)
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Thinking...", async _ =>
                {
                    response = await chatDemo.ProcessMessageAsync(userInput);
                });
        }
        else
        {
            Console.WriteLine("Thinking...");
            response = await chatDemo.ProcessMessageAsync(userInput);
        }

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
