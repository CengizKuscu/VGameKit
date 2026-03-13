## App Manager Reference

English documentation for the App Manager component.

## Overview
AbsAppManager serves as the base class to manage the application lifecycle. It publishes AppReadyEvent when the application is ready and supports subscription to AppReadyEvent.

## Key Features
- Asynchronous startup via StartAsync
- Publish application-ready events (AppReadyEvent)
- Start/initialize sequence via InitializeGame

## Core Methods
- StartAsync(CancellationToken token): starts the app and emits AppReadyEvent when complete
- InitializeGame(CancellationToken token): abstract method to implement game initialization logic
- OnAppReady(AppReadyEvent @event): handler for AppReadyEvent

## Usage Example
```csharp
public class AppManager : AbsAppManager
{
    protected override UniTask InitializeGame(CancellationToken token)
    {
        // Initialize game resources here
        return UniTask.CompletedTask;
    }

    protected override void OnAppReady(AppReadyEvent @event)
    {
        // Actions after app is ready
    }
}
```

## See Also
- AbsAppManager
- AppReadyEvent
