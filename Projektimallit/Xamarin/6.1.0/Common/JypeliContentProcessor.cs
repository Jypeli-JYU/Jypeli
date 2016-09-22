using System;
using MonoDevelop.Projects;
using System.Xml;
using MonoDevelop.Core.Assemblies;
using System.Text;
using System.Reflection;

namespace MonoDevelop.Jypeli
{
	public class JypeliContentProcessor 
	{		
		
		public static BuildResult Compile(ProjectFile file, ProgressMonitor monitor, BuildData buildData)
		{			
			switch (file.BuildAction) {
			case "JypeliShader" :
				var result = new BuildResult();
				monitor.Log.WriteLine("Compiling Shader");					
				monitor.Log.WriteLine("Shader : "+buildData.Configuration.OutputDirectory);
				monitor.Log.WriteLine("Shader : "+file.FilePath);
				monitor.Log.WriteLine("Shader : "+file.ToString());
				return result;
			default:
				return new BuildResult();
			}
			
		}
	}
}
