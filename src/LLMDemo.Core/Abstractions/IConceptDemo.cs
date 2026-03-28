namespace LLMDemo.Core.Abstractions;

/// <summary>
/// Contract that all concept demo projects must implement.
/// The CLI host discovers and runs implementations of this interface.
/// </summary>
public interface IConceptDemo
{
    /// <summary>Unique name shown in the CLI menu.</summary>
    string Name { get; }

    /// <summary>Short description of what this demo does.</summary>
    string Description { get; }

    /// <summary>Execute the demo.</summary>
    Task RunAsync(CancellationToken cancellationToken = default);
}
