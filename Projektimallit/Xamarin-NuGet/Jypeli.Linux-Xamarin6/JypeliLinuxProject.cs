using MonoDevelop.Core;
using MonoDevelop.Projects;
using System;
using System.Xml;

namespace Jypeli.Linux
{
	public class JypeliLinuxProject : DotNetProject
	{
		public JypeliLinuxProject()
			: base()
		{
		}

		public JypeliLinuxProject(string languageName)
			: base(languageName)
		{
		}

		public JypeliLinuxProject(string languageName, ProjectCreateInformation projectCreateInformation, XmlElement projectOptions)
			: base(languageName, projectCreateInformation, projectOptions)
		{
		}

		protected override BuildResult OnBuild(IProgressMonitor monitor, ConfigurationSelector configuration)
		{
			return base.OnBuild(monitor, configuration);
		}
	}
}
	