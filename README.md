ASP.NET Redis Providers
=======================
This repository contains code for Session State and Output Cache providers for Redis.
1) Azure Redis Session State Provider can be use to store your session state in a Redis Cache rather than in-memory or in a SQL Server database.
2) Azure Redis Output Cache Provider is an out-of-process storage mechanism for output cache data. This data is specifically for full HTTP responses (page output caching). 

## Documentation

See [Azure Redis Session State Provider Documentation](https://msdn.microsoft.com/en-us/library/azure/dn690522.aspx) and [Azure Redis Output Cache Provider Documentation](https://msdn.microsoft.com/en-us/library/azure/dn798898.aspx)

## License

This project is under the umbrella of the [.NET Foundation](http://www.dotnetfoundation.org/) and is licensed under [the MIT License](https://github.com/Azure/aspnet-redis-providers/blob/master/License.txt)

## Build and Test
Open Microsoft.CacheProviders.sln with Visual Studio. You should install [xUnit.net runner for Visual Studio](https://visualstudiogallery.msdn.microsoft.com/463c5987-f82b-46c8-a97e-b1cde42b9099) in visual studio using "TOOLS -> Extensions and Updates..."
Right click on solution in "Solution Explorer" and click on "Build Solution" in visual studio to build all projects. Open "Test Explorer" in visual studio and run all tests by clicking on "Run All".

## Release Notes
[Release Notes](https://github.com/Azure/aspnet-redis-providers/wiki/Release-Notes)

## Questions?

* [Azure Cache Forum](https://social.msdn.microsoft.com/Forums/en-US/home?forum=azurecache)
* [StackOverflow for Azure Redis Cache](http://stackoverflow.com/questions/tagged/azure-redis-cache)
