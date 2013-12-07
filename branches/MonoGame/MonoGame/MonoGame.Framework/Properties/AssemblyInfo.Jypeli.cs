using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Jypeli.MonoGame.Framework")]
#if OUYA
[assembly: AssemblyDescription("MonoGame for Jypeli (OUYA)")]
#elif ANDROID
[assembly: AssemblyDescription("MonoGame for Jypeli (Android)")]
#elif WINDOWS
#if DIRECTX
[assembly: AssemblyDescription("MonoGame for Jypeli (Windows DirectX)")]
#else
[assembly: AssemblyDescription("MonoGame for Jypeli (Windows OpenGL)")]
#endif
#elif PSM
[assembly: AssemblyDescription("MonoGame for Jypeli (PlayStation Mobile)")]
#elif LINUX
[assembly: AssemblyDescription("MonoGame for Jypeli (Linux)")]
#elif MAC
[assembly: AssemblyDescription("MonoGame for Jypeli (Mac OS X)")]
#elif IOS
[assembly: AssemblyDescription("MonoGame for Jypeli (iOS)")]
#elif WINDOWS_STOREAPP
[assembly: AssemblyDescription("MonoGame for Jypeli (Windows Store)")]
#elif WINDOWS_PHONE
[assembly: AssemblyDescription("MonoGame for Jypeli (Windows Phone 8)")]
#endif
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Jypeli.MonoGame.Framework")]
[assembly: AssemblyCopyright("Copyright © 2011-2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Mark the assembly as CLS compliant so it can be safely used in other .NET languages
[assembly:CLSCompliant(true)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "8cd4f728-392f-4ae0-9def-a943201642c5" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("3.0.0.0")]
[assembly: AssemblyFileVersion("3.0.0.0")]
[assembly: NeutralResourcesLanguageAttribute("en-US")]
