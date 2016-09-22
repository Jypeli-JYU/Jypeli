using System;
using MonoDevelop.Projects;
using System.Xml;
using MonoDevelop.Core.Assemblies;
using System.Text;
using System.Reflection;

namespace MonoDevelop.Jypeli
{
	public class JypeliBuildExtension : ProjectServiceExtension
	{
		
		protected override BuildResult Build (MonoDevelop.Core.IProgressMonitor monitor, SolutionEntityItem item, ConfigurationSelector configuration)
		{
#if DEBUG			
			monitor.Log.WriteLine("Jypeli Extension Build Called");	
#endif			
			try
			{
			  return base.Build (monitor, item, configuration);
			}
			finally
			{
#if DEBUG				
			   monitor.Log.WriteLine("Jypeli Extension Build Ended");	
#endif				
			}
		}
		
		protected override BuildResult Compile (MonoDevelop.Core.IProgressMonitor monitor, SolutionEntityItem item, BuildData buildData)
		{
#if DEBUG			
			monitor.Log.WriteLine("Jypeli Extension Compile Called");	
#endif			
			try
			{				
				var proj = item as JypeliProject;
				if (proj == null)
				{
				   return base.Compile (monitor, item, buildData);
				}
				var results = new System.Collections.Generic.List<BuildResult>();
				foreach(var file in proj.Files)
				{
					if (JypeliBuildAction.IsJypeliBuildAction(file.BuildAction))					
					{												
						buildData.Items.Add(file);
						var buildResult = JypeliContentProcessor.Compile(file, monitor, buildData);
						results.Add(buildResult);
					}
				}
				return base.Compile (monitor, item, buildData).Append(results);
			}
			finally
			{
#if DEBUG				
				monitor.Log.WriteLine("Jypeli Extension Compile Ended");	
#endif				
			}
		}
	}
}

