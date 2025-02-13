using TC.EventBus;

Console.WriteLine("Creating EventBus");

var eventBus = new EventBus();

Console.WriteLine("Subscribing to events");

eventBus.Subscribe<TestEvent>(_ => Console.WriteLine("TestEvent occured"));

Console.WriteLine("Publishing event");

eventBus.Publish(new TestEvent());

class TestEvent { }
