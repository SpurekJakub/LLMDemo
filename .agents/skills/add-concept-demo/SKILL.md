---
name: add-concept-demo
description: "Scaffold a new LLMDemo.Concept.* demo project. Use when: creating a new concept demo, adding a new demo project, scaffolding a new LLM experiment."
---

# Add a New Concept Demo

## Procedure

Given a concept name `<Name>`:

1. **Create the project** as a class library under `src/`:
   ```bash
   dotnet new classlib -n LLMDemo.Concept.<Name> -o src/LLMDemo.Concept.<Name>
   ```

2. **Remove the generated `Class1.cs`** file.

3. **Edit the csproj** — remove `<TargetFramework>` (inherited from `Directory.Build.props`), add a project reference to Core:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <ItemGroup>
       <ProjectReference Include="..\LLMDemo.Core\LLMDemo.Core.csproj" />
     </ItemGroup>
   </Project>
   ```

4. **Implement `IConceptDemo`** in a new class:
   ```csharp
   using LLMDemo.Core.Abstractions;

   namespace LLMDemo.Concept.<Name>;

   public sealed class <Name>Demo : IConceptDemo
   {
       public string Name => "<Name>";
       public string Description => "<brief description>";

       private readonly IChatCompletionService _chatService;

       public <Name>Demo(IChatCompletionService chatService)
       {
           _chatService = chatService;
       }

       public async Task RunAsync(CancellationToken cancellationToken = default)
       {
           // TODO: implement demo logic
       }
   }
   ```

5. **Add to the solution**:
   ```bash
   dotnet sln add src/LLMDemo.Concept.<Name>
   ```

6. **Wire it up in the CLI host** (`src/LLMDemo/LLMDemo.csproj`):
   - Add `<ProjectReference Include="..\LLMDemo.Concept.<Name>\LLMDemo.Concept.<Name>.csproj" />`

7. **Register in `src/LLMDemo/Program.cs`**:
   ```csharp
   builder.Services.AddSingleton<IConceptDemo, <Name>Demo>();
   ```

8. **Verify**: `dotnet build` should succeed with zero errors.

## Conventions

- Project name must follow `LLMDemo.Concept.<Name>` pattern.
- No `Version` on `<PackageReference>` — use CPM (`Directory.Packages.props`).
- No `<TargetFramework>` in the csproj — inherited from `Directory.Build.props`.
- Use file-scoped namespaces and nullable reference types.
