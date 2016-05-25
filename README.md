#Postal.NET

##Introduction
Postal.NET is a .NET portable library for writing decoupled applications. It is loosely based upon the [Postal.js](https://github.com/postaljs) JavaScript library and follows the [Domain Events](http://martinfowler.com/eaaDev/DomainEvent.html) and [Pub/Sub](https://en.wikipedia.org/wiki/Publish%E2%80%93subscribe_pattern) patterns.
It was written by [Ricardo Peres](https://github.com/rjperes) ([@rjperes75](https://twitter.com/rjperes75)).

##Concepts
Postal.NET uses the concepts of **channels** and **topics**. We subscribe to a topic of a channel, and we send messages to other (or possibly the same) channels and topics. The **"\*"** character means anything, so, for example, **"a.b"** and **"a.\*"** or even **"\*"** will match. There can be several simultaneous subscriptions, even to the same channel/topic pair. Postal.NET guarantees the delivery.

##Usage

    //create a subscription to a single named channel and topic pair
    using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data)))
    {
        //publish synchronously (will block)
        Postal.Box.Publish("channel", "topic", "Hello, World!");

        //publish asynchronously (does not block)
        await Postal.Box.PublishAsync("channel", "topic", "Hello, Async World!");
    }

    //no consequences, since the subscription was terminated
    Postal.Box.Publish("channel", "topic", "Does not appear because the subscription was disposed!");

A message can either be sent synchronously or asynchronously and if there are subscribers to it, it will raise an event (one or more subscriptions being triggered). Subscriptions do not prevent the garbage collection of the subscriber.
There are some handy extensions for common tasks:

- **Once**: only handles an event once, then unsubcribes from it
- **MultiPublish**: publishes a number of events at once
- **SubscribeMulti**: subscribes to one or more channels, separated by commas, at a time
- **Subscribe\<T\>**: subscribes to an event where its payload is of a given type

The public interface is decoupled from the actual implementation and it can be easily switched.

You can find more examples in the [GitHub repository](https://github.com/rjperes/Postal.NET) in the **Postal.NET.Test** project.


##Installation
You can either:

- Clone from GitHub: [http://github.com/rjperes/Postal.NET](http://github.com/rjperes/Postal.NET)
- Install via Nuget: [https://www.nuget.org/packages/Postal.NET](https://www.nuget.org/packages/Postal.NET).

##Other Projects

Other projects you can find in the GitHub repository and in Nuget are:

- **PostalConventions.NET**: conventions for channels and topics ([Nuget](https://www.nuget.org/packages/PostalConventions.NET))
- **PostalRX.NET**: Reactive Extensions ([RX.NET](https://github.com/Reactive-Extensions/Rx.NET)) adapter ([Nuget](https://www.nuget.org/packages/PostalRX.NET))
- **PostalWhen.NET**: composition of events (e.g., "do this when you receive this and that") ([Nuget](https://www.nuget.org/packages/PostalWhen.NET))
- **PostalRequestResponse.NET**: implementation of request-response pattern ([Nuget](https://www.nuget.org/packages/PostalRequestResponse.NET))
- **Postal.NET.Test**: working examples

##Contacts
If you see any value in this and wish to send me your comments, please do so through [GitHub](https://github.com/rjperes/Postal.NET). Questions and suggestions are welcome too!

##Licenses
This software is distributed under the terms of the Free Software Foundation Lesser GNU Public License (LGPL), version 2.1 (see lgpl.txt).

##Copyright
You are free to use this as you wish, but I ask you to please send me a note about it.
