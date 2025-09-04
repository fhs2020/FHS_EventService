# FHS_EventService

User Events Service Kafka .NET 8 backend that ingests user activity via POST events, publishes to Kafka, consumes the topic in a background service, and stores processed events in a thread safe in memory store.
