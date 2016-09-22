using System;
using MonoDevelop.Projects;
using System.Xml;
using MonoDevelop.Core.Assemblies;
using System.Text;
using System.Reflection;

namespace MonoDevelop.Jypeli
{	
	public static class JypeliBuildAction
	{
		public static readonly string Shader;
		
		public static bool IsJypeliBuildAction(string action)
		{
			return action == Shader;
		}
		
		static JypeliBuildAction ()
		{
			Shader = "JypeliShader";
		}
	}
}
