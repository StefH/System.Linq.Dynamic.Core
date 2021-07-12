### Library Powered By

This library is powered by [Entity Framework Extensions](https://entityframework-extensions.net/?z=github&y=system.linq.dynamic.core)

<a href="https://entityframework-extensions.net/?z=github&y=system.linq.dynamic.core">
<kbd>
<img src="https://zzzprojects.github.io/images/logo/entityframework-extensions-pub.jpg" alt="Entity Framework Extensions" />
</kbd>
</a>

---

# System.Linq.Dynamic.Core
This is a **.NET Core / Standard port** of the Microsoft assembly for the .Net 4.0 Dynamic language functionality.

## Overview
With this library it's possible to write Dynamic LINQ queries (string based) on an `IQueryable`:
``` c#
var query = db.Customers
    .Where("City == @0 and Orders.Count >= @1", "London", 10)
    .OrderBy("CompanyName")
    .Select("new(CompanyName as Name, Phone)");
```

Interpolated strings are supported on .NET 4.6(and above), .NET Core 2.1(and above), .NET Standard 1.3(and above) and UAP 10.0(and above).
For example:
```csharp
string cityName = "London";
int c = 10;
db.Customers
    .WhereInterpolated($"City == {cityName} and Orders.Count >= {c}");
	
users.FirstInterpolated($"Income > {value}");	

context.Blogs.CountInterpolatedAsync($"Name.Contains({search})");
```

## Useful links

- [Website](https://dynamic-linq.net/)
- [Documentation](https://dynamic-linq.net/overview)
- [Online examples](https://dynamic-linq.net/online-examples)
- [nuget](https://www.nuget.org/packages/System.Linq.Dynamic.Core/) 

## Info
| | |
| --- | --- |
| ***Project*** | &nbsp; |
| &nbsp;&nbsp;**Chat** | [![Gitter](https://img.shields.io/gitter/room/system-linq-dynamic-core/Lobby.svg)](https://gitter.im/system-linq-dynamic-core/Lobby) |
| &nbsp;&nbsp;**Issues** | [![GitHub issues](https://img.shields.io/github/issues/StefH/System.Linq.Dynamic.Core.svg)](https://github.com/StefH/System.Linq.Dynamic.Core/issues) |
| | |
| ***Quality*** | &nbsp; |
| &nbsp;&nbsp;**Main workflow** | ![Main workflow](https://github.com/zzzprojects/System.Linq.Dynamic.Core/workflows/Main%20workflow/badge.svg) |
| |
| ***NuGet*** | &nbsp; |
| &nbsp;&nbsp;**System.Linq.Dynamic.Core** | [![NuGet](https://buildstats.info/nuget/System.Linq.Dynamic.Core)](https://www.nuget.org/packages/System.Linq.Dynamic.Core) |
| &nbsp;&nbsp;**EntityFramework.DynamicLinq** | [![NuGet](https://buildstats.info/nuget/EntityFramework.DynamicLinq)](https://www.nuget.org/packages/EntityFramework.DynamicLinq) |
| &nbsp;&nbsp;**Microsoft.EntityFrameworkCore.DynamicLinq** | [![NuGet](https://buildstats.info/nuget/Microsoft.EntityFrameworkCore.DynamicLinq)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.DynamicLinq) |
| &nbsp;&nbsp;**Z.EntityFramework.Classic.DynamicLinq** | [![NuGet](https://buildstats.info/nuget/Z.EntityFramework.Classic.DynamicLinq)](https://www.nuget.org/packages/Z.EntityFramework.Classic.DynamicLinq) |

## Development Details

### Frameworks
The following frameworks are supported:
- net35, net40, net45, net46 and up
- netstandard1.3, netstandard2.0 and netstandard2.1
- netcoreapp3.1 and net5.0
- uap10.0

### Fork details
This fork takes the basic library to a new level. Contains XML Documentation and examples on how to use it. Also adds unit testing to help ensure that it works properly.

Some background:
I forked from https://github.com/NArnott/System.Linq.Dynamic and added some more functionality there.<br>My fork is still visible on github [https://github.com/StefH/System.Linq.Dynamic], however I decided to start a new project + nuget to avoid confusion and create the project according to the new VS2017 + .NET Core rules / standards.

However, currently there are multiple nuget packages and projects available:

| Project | NuGet | Author | Comment |
| ------- | ----- | ------ | ------- |
| [kahanu/System.Linq.Dynamic][2a] | [System.Linq.Dynamic][2b] | @kahanu | - |
| [kavun/System.Linq.Dynamic.3.5][3a] | [System.Linq.Dynamic.3.5/][3b] | @kavun | only 3.5 and VB.NET |
| [NArnott/System.Linq.Dynamic][4a] | [System.Linq.Dynamic.Library][4b]  | @NArnott | removed from github + nuget ? |
| [dynamiclinq.codeplex][5a] | - | dialectsoftware | - |
| [dynamic-linq][6a] | - | scottgu | - |

[2a]: https://github.com/kahanu/System.Linq.Dynamic
[2b]: https://www.nuget.org/packages/System.Linq.Dynamic
[3a]: https://github.com/kavun/System.Linq.Dynamic.3.5
[3b]: https://www.nuget.org/packages/System.Linq.Dynamic.3.5/
[4a]: https://github.com/NArnott/System.Linq.Dynamic
[4b]: https://www.nuget.org/packages/System.Linq.Dynamic.Library
[5a]: https://dynamiclinq.codeplex.com/
[6a]: http://weblogs.asp.net/scottgu/dynamic-linq-part-1-using-the-linq-dynamic-query-library

[doc-api]: http://zzzprojects.github.io/System.Linq.Dynamic.Core
[doc-wiki]: https://github.com/zzzprojects/System.Linq.Dynamic.Core/wiki/Dynamic-Expressions

## Contribute

Want to help us? Your donation directly helps us maintain and grow ZZZ Free Projects. 

We can't thank you enough for your support 🙏.

👍 [One-time donation](https://zzzprojects.com/contribute)

❤️ [Become a sponsor](https://github.com/sponsors/zzzprojects) 

### Why should I contribute to this free & open-source library?
We all love free and open-source libraries! But there is a catch... nothing is free in this world.

We NEED your help. Last year alone, we spent over **3000 hours** maintaining all our open source libraries.

Contributions allow us to spend more of our time on: Bug Fix, Development, Documentation, and Support.

### How much should I contribute?
Any amount is much appreciated. All our free libraries together have more than **100 million** downloads.

If everyone could contribute a tiny amount, it would help us make the .NET community a better place to code!

Another great free way to contribute is  **spreading the word** about the library.

A **HUGE THANKS** for your help!

## More Projects

- [EntityFramework Extensions](https://entityframework-extensions.net/)
- [Dapper Plus](https://dapper-plus.net/)
- [C# Eval Expression](https://eval-expression.net/)
- and much more! 

To view all our free and paid projects, visit our [website](https://zzzprojects.com/).
