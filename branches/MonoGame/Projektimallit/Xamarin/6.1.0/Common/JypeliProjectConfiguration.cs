using System;
using MonoDevelop.Projects;
using System.Xml;
using MonoDevelop.Core.Assemblies;
using System.Text;
using System.Reflection;

namespace MonoDevelop.Jypeli
{
	public class JypeliProjectConfiguration : DotNetProjectConfiguration
	{
		public JypeliProjectConfiguration () : base ()
		{
		}
		
		public JypeliProjectConfiguration (string name) : base (name)
		{
		}
	}
}

