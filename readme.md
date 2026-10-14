# Nova.DI

Nova.DI is a lightweight Inversion of Control (IoC) container for .NET, built from scratch.

This project is **currently under development** and is not ready for use yet.

## About

I use dependency injection every day with ASP.NET Core, but the container itself has always been a bit of a black box to me. Nova.DI is my attempt to understand how it really works by writing one myself, inspired by `Microsoft.Extensions.DependencyInjection`.

The goal is to support the usual features of a DI container: service registration, Transient / Scoped / Singleton lifetimes, scopes, and proper disposal of resources.

## Note

This is primarily a learning project and is not intended for production use.