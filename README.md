# Jypeli

Jypeli is a simple C# game programming library. It is used as a tool for teaching programming in the University of Jyväskylä.

Installation instructions for regular users (in Finnish) <https://tim.jyu.fi/view/kurssit/tie/ohj1/tyokalut/windows>

Instructions for using the library (also in Finnish) <https://tim.jyu.fi/view/kurssit/jypeli/wiki>

Jypeli (C) University of Jyväskylä, 2009-2022.

## Source Code

The full source code is available here from GitHub:
```
git clone https://github.com/Jypeli-JYU/Jypeli.git
```

You need to have .NET 6 installed to build & run Jypeli.
For Android-projects, you also need to have `net6-android` workload installed:

```
dotnet workload install android
```

If you do not wish to run android projects, you can also edit `Jypeli.csproj` & `FarseerPhysics.csproj` and remove `net6-android` from targetframeworks.

Open the main `Jypeli.sln` with Visual Studio 2022 or JetBrains Rider.

`TestProjects/Tasohyppelypeli` is a test project ready for development purposes.
`TestProjects/Net6Android` is for Android development.