# Postal.NET

## Introduction
Postal.NET is a .NET Standard library for writing decoupled applications. It is loosely based upon the [Postal.js](https://github.com/postaljs) JavaScript library and follows somewhat the [Domain Events](http://martinfowler.com/eaaDev/DomainEvent.html) and [Pub/Sub](https://en.wikipedia.org/wiki/Publish%E2%80%93subscribe_pattern) patterns.
It was written by [Ricardo Peres](https://github.com/rjperes) ([@rjperes75](https://twitter.com/rjperes75)).
As of version 2 it now targets .NET 8.

## Concepts
Postal.NET uses the concepts of **channels** and **topics**. We subscribe to a topic of a channel, and we send messages to other (or possibly the same) channels and topics. The **"\*"** character means anything, so, for example, **"a.b"** and **"a.\*"** or even **"\*"** will match. There can be several simultaneous subscriptions, even to the same channel/topic pair. Postal.NET guarantees the delivery.

## Usage

    //create a subscription to a single named channel and topic pair
    using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data)))
    {        
        //publish asynchronously (does not block)
        await Postal.Box.PublishAsync("channel", "topic", "Hello, Async World!");
    }

    //no consequences, since the subscription was terminated
    await Postal.Box.PublishAsync("channel", "topic", "Does not appear because the subscription was disposed!");

A message can only be sent asynchronously and if there are subscribers to it, it will raise an event (one or more subscriptions being triggered). Subscriptions do not prevent the garbage collection of the subscriber. Messages are always wrapped in an envelope.
There are some handy extensions for common tasks:

- **Once**: only handles an event once, then unsubcribes from it
- **MultiPublish**: publishes a number of events at once
- **SubscribeMulti**: subscribes to one or more channels, separated by commas, at a time
- **Subscribe\<T\>**: subscribes to an event where its payload is of a given type

The public interface is decoupled from the actual implementation and it can be switched (but don't do it!).

You can find more examples in the [GitHub repository](https://github.com/rjperes/Postal.NET) in the **Postal.NET.Test** project.

## Extensibility
Most of the inner workings of Postal.NET can be configured by injection an implementation of the core interfaces:
- *IBox*: The core contract for Postal.NET.
- *IChannel*: An event channel.
- *IChannelTopicMatcher*: How to match channels and topics.
- *IChannelTopicMatcherProvider*: Injects a channel and topic matcher.
- *IPublisher*:Basic contract for a message publisher.
- *ISubscriberStore*:Actual implementation contract for Postal.NET.
- *ITopic*: An event topic.

## Installation
You can either:

- Clone from GitHub: [http://github.com/rjperes/Postal.NET](http://github.com/rjperes/Postal.NET)
- Install via Nuget: [https://www.nuget.org/packages/Postal.NET](https://www.nuget.org/packages/Postal.NET).

## Other Projects

Other projects you can find in the GitHub repository and in Nuget are:

- **PostalConventions.NET**: conventions for channels and topics ([Nuget](https://www.nuget.org/packages/PostalConventions.NET))
- **PostalCqrs.NET**: [CQRS](http://martinfowler.com/bliki/CQRS.html) extensions ([Nuget](https://www.nuget.org/packages/PostalCqrs.NET))
- **PostalRequestResponse.NET**: implementation of request-response pattern ([Nuget](https://www.nuget.org/packages/PostalRequestResponse.NET))
- **PostalRX.NET**: Reactive Extensions ([RX.NET](https://github.com/Reactive-Extensions/Rx.NET)) adapter ([Nuget](https://www.nuget.org/packages/PostalRX.NET))
- **PostalWhen.NET**: composition of events (e.g., "do this when you receive this and that") ([Nuget](https://www.nuget.org/packages/PostalWhen.NET))
- **PostalInterceptor.NET**: interception of messages (e.g., before and after) ([Nuget](https://www.nuget.org/packages/PostalInterceptor.NET))
- **Postal.NET.Test**: working examples
- **Postal.NET.UnitTests**: unit tests

## Contacts
If you see any value in this and wish to send me your comments, please do so through [GitHub](https://github.com/rjperes/Postal.NET). Questions and suggestions are welcome too!

## Licenses
This software is distributed under the terms of the Free Software Foundation Lesser GNU Public License (LGPL), version 2.1 (see lgpl.txt).

## Copyright
You are free to use this as you wish, but I ask you to please send me a note about it.
