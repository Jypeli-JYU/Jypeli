using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
	"MonoDevelop.Jypeli.Windows",
	Namespace = "Jypeli.Windows",
	Version = "6.8.0"
)]

[assembly:AddinDependency ("::MonoDevelop.Core", MonoDevelop.BuildInfo.Version)]
[assembly:AddinDependency ("::MonoDevelop.Ide", MonoDevelop.BuildInfo.Version)]

[assembly: AddinName("MonoDevelop.Jypeli.Windows")]
[assembly: AddinCategory("Jypeli")]
[assembly: AddinDescription("MonoDevelop.Jypeli.Windows")]
[assembly: AddinAuthor("Jyväskylän yliopisto")]
