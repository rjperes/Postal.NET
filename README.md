#Postal.NET

##Introduction
Postal.NET is a .NET portable library for writing decoupled applications. It is loosely based upon the [Postal.js](https://github.com/postaljs) JavaScript library and follows the [Domain Events](http://martinfowler.com/eaaDev/DomainEvent.html) and [Pub/Sub](https://en.wikipedia.org/wiki/Publish%E2%80%93subscribe_pattern) patterns.
It was written by [Ricardo Peres](https://github.com/rjperes) ([@rjperes75](https://twitter.com/rjperes75)).

##Concepts
Postal.NET uses the concepts of **channels** and **topics**. We subscribe to a topic of a channel, and we send messages to other (or possibly the same) channels and topics. A * character means anything, so, for example, **"a.b"** and **"a.\*"** or even **"\*"** will match. There can be several simultaneous subscriptions, even to the same channel/topic pair. Postal.NET guarantees the delivery.

##Usage

    using (Postal.Box.Subscribe("channel", "topic", (env) => Console.WriteLine(env.Data)))
    {
        //sync
        Postal.Box.Publish("channel", "topic", "Hello, World!");

        //async
        await Postal.Box.PublishAsync("channel", "topic", "Hello, Async World!");
    }

    Postal.Box.Publish("channel", "topic", "Does not appear because the subscription was disposed!");

A message can either be sent synchronously or asynchronously. Subscriptions do not prevent garbage collection.
You can find more examples in the [GitHub repository](https://github.com/rjperes/Postal.NET).

##Contacts
If you see any value in this and wish to send me your comments, please do so through [GitHub](https://github.com/rjperes/Postal.NET). Questions and suggestions are welcome too!

##Licenses
This software is distributed under the terms of the Free Software Foundation Lesser GNU Public License (LGPL), version 2.1 (see lgpl.txt).

##Copyright
You are free to use this as you wish, but I ask you to please send me a note about it.