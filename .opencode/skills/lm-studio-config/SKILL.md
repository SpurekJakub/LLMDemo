---
name: lm-studio-config
description: "Configure LM Studio connection settings, change endpoint, set default model, troubleshoot LM Studio connectivity. Use when: configuring LM Studio, changing the API endpoint, setting a model, connection issues."
---

# LM Studio Configuration

## Settings

Configuration lives in `src/LLMDemo/appsettings.json` under the `LmStudio` section:

```json
{
  "LmStudio": {
    "Endpoint": "http://localhost:1234/v1",
    "DefaultModel": null
  }
}
```

| Property | Default | Description |
|---|---|---|
| `Endpoint` | `http://localhost:1234/v1` | LM Studio API base URL (OpenAI-compatible) |
| `DefaultModel` | `null` (uses whatever LM Studio has loaded) | Model identifier passed in API requests |

## Options Class

`LmStudioOptions` in `LLMDemo.Core/Configuration/` is the strongly-typed config POCO. It is:
- Bound from the `LmStudio` config section via `BindConfiguration`.
- Validated on startup.
- Injected as `IOptions<LmStudioOptions>`.

## Overriding at Runtime

You can also configure options programmatically:

```csharp
builder.Services.AddLlmDemoCore(opts =>
{
    opts.Endpoint = new Uri("http://192.168.1.100:1234/v1");
    opts.DefaultModel = "my-model";
});
```

## Troubleshooting

1. Ensure LM Studio is running with the **API server** enabled.
2. Verify the endpoint is reachable: `curl http://localhost:1234/v1/models`
3. Check that a model is loaded in LM Studio.
4. Review logs — `LLMDemo` namespace logs at `Debug` level by default.
