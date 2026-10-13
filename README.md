# Jypeli

Jypeli is a simple C# game programming library. It is used as a tool for teaching programming in the University of Jyväskylä.

Installation and using the library (in Finnish): <https://jypeli.it.jyu.fi>

Jypeli (C) University of Jyväskylä, 2009-2026.

## Source Code

The full source code is available here from GitHub:
```
git clone https://github.com/Jypeli-JYU/Jypeli.git
```

You need to have .NET 10 installed to build & run Jypeli.
For Android-projects, you also need to have `net10.0-android` workload installed:

```
dotnet workload install android
```

If you do not wish to run android projects, you can also edit `Jypeli.csproj` & `FarseerPhysics.csproj` and remove `net10.0-android` from targetframeworks.

Open the main `Jypeli.sln` with Visual Studio 2022 or JetBrains Rider.

`TestProjects/Tasohyppelypeli` is a test project ready for development purposes.
`TestProjects/Net6Android` is for Android development.
