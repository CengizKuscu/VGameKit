## GKConfig (Configuration) Reference

English reference for GKConfig singleton configuration.

## Overview
GKConfig is a ScriptableObject-based singleton managing global configuration (e.g., LogState).

## Key Features
- `LogState` configuration
- Editor/runtime asset loading via Resources or preloaded assets
- Editor helpers to create GKConfig assets

## Usage
```csharp
GKConfig.Instance.LogState = LogState.Info;
```

## See Also
- GKLog
