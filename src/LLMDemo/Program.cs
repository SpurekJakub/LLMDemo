using LLMDemo.Core.Abstractions;
using LLMDemo.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

var builder = Host.CreateApplicationBuilder(args);

// Register core LM Studio services
builder.Services.AddLlmDemoCore();

// ----- Register concept demo projects here -----
// builder.Services.AddSingleton<IConceptDemo, MyConceptDemo>();

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
var swDemo = System.Diagnostics.Stopwatch.StartNew();
await selected.RunAsync();
swDemo.Stop();
AnsiConsole.MarkupLine($"[green]Demo finished in {swDemo.ElapsedMilliseconds} ms.[/] ");
