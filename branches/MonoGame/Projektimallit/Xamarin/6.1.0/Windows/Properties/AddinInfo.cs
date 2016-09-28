using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
	"Jypeli",
	Namespace = "Jypeli.Windows",
	Version = "6.8.0"
)]

[assembly:AddinDependency ("::MonoDevelop.Core", MonoDevelop.BuildInfo.Version)]
[assembly:AddinDependency ("::MonoDevelop.Ide", MonoDevelop.BuildInfo.Version)]

[assembly: AddinName("Jypeli (DirectX, experimental)")]
[assembly: AddinCategory("Jypeli")]
[assembly: AddinDescription("Jypeli game programming library, experimental DirectX port")]
[assembly: AddinAuthor("Jyväskylän yliopisto")]
