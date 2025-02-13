# TC.EventBus

A simple in-process implementation of an EventBus.

## Getting started

Create an instance of `EventBus`, declare one or more event classes, subscribe to them and publish events of those types:

```
var eventBus = new EventBus();
eventBus.Subscribe<TestEvent>(_ => Console.WriteLine("TestEvent occured"));
eventBus.Publish(new TestEvent());

class TestEvent { }
```
