using System;
using MonoDevelop.Projects;
using System.Xml;
using MonoDevelop.Core.Assemblies;

namespace MonoDevelop.Jypeli
{
	public class JypeliProject : DotNetProject
	{
		public JypeliProject()
		{
			Init();
		}
		
		public JypeliProject (string languageName, params string[] flavorIds)
			: base (languageName, info, projectOptions)
		{
			Init();
		}
		
		private void Init()
		{
		}
		
		public override SolutionItemConfiguration CreateConfiguration(string name)
		{
			var conf = new JypeliProjectConfiguration(name);
			conf.CopyFrom (base.CreateConfiguration(name));
			return conf;
		}
  			
		public override bool SupportsFormat(FileFormat format)
		{
			return format.Id == "MSBuild10";
		}
		
		public override TargetFrameworkMoniker GetDefaultTargetFrameworkForFormat (FileFormat format)
		{
			return new TargetFrameworkMoniker("4.0");
		}
		
		public override bool SupportsFramework (MonoDevelop.Core.Assemblies.TargetFramework framework)
		{
			Type fw = typeof(MonoDevelop.Core.Assemblies.TargetFramework);
			Type[] ptypes = new Type[] { typeof(TargetFrameworkMoniker) };

			var method = fw.GetMethod ("CanReferenceAssembliesTargetingFramework", ptypes);
			if (method == null) {
				method = fw.GetMethod ("IsCompatibleWithFramework", ptypes);
				if (method == null)
					throw new MissingMethodException ("CanReferenceAssembliesTargetingFramework / IsCompatibleWithFramework");
			}

			object[] parameters = new object[] { MonoDevelop.Core.Assemblies.TargetFrameworkMoniker.NET_4_0 };
			return (bool)method.Invoke(framework, parameters) && base.SupportsFramework(framework);

			/*if (!framework.IsCompatibleWithFramework(MonoDevelop.Core.Assemblies.TargetFrameworkMoniker.NET_4_0))
			//if (!framework.CanReferenceAssembliesTargetingFramework(MonoDevelop.Core.Assemblies.TargetFrameworkMoniker.NET_4_0))
				return false;
			else
				return base.SupportsFramework (framework);*/
		}
		
		protected override System.Collections.Generic.IList<string> GetCommonBuildActions ()
		{			
			var actions = new System.Collections.Generic.List<string>(base.GetCommonBuildActions());
			actions.Add(JypeliBuildAction.Shader);
			return actions;
		}
		
		public override string GetDefaultBuildAction (string fileName)
		{
			if (System.IO.Path.GetExtension(fileName) == ".fx")
			{
				return JypeliBuildAction.Shader;
			}
			else if (IsContentDir(System.IO.Path.GetDirectoryName(fileName)))
			{
				return BuildAction.Content;
			}

			return base.GetDefaultBuildAction (fileName);
		}

		protected override void OnFileAddedToProject (ProjectFileEventArgs e)
		{
			if (Loading)
			{
				// Just deserializing
				base.OnFileAddedToProject (e);
			}

			foreach (ProjectFileEventInfo einfo in e)
			{
				if (IsContentDir(System.IO.Path.GetDirectoryName(einfo.ProjectFile.FilePath)))
				{
					// Set copy mode to Always for content files
					einfo.ProjectFile.CopyToOutputDirectory = FileCopyMode.Always;
				}
			}

			base.OnFileAddedToProject (e);
		}

		private bool IsContentDir( string dir )
		{
			if (dir.Length < 7)
				return false;

			String path = dir.ToLower();
			if (path[path.Length - 1] == '\\' || path[path.Length - 1] == '/')
			{
				// Remove trailing slash
				path = dir.Substring(0, dir.Length - 1);
			}

			char[] separators = {'\\', '/'};

			while (path.Length > 6)
			{
				int sep = path.LastIndexOfAny(separators);
				if (sep < 0)
				{
					return path == "content";
				}

				String cur = path.Substring(sep + 1);
				if (cur == "content")
					return true;

				path = path.Substring(0, sep);
			}

			return false;
		}

        protected override void PopulateSupportFileList(FileCopySet list, ConfigurationSelector solutionConfiguration)
        {
            base.PopulateSupportFileList(list, solutionConfiguration);
            //HACK: workaround for MD not local-copying package references
            foreach (var projectReference in References)
            {
				if (projectReference.Package != null && projectReference.Package.Name == "jypeli")
                {
					if (projectReference.ReferenceType == ReferenceType.Package)
                    {
                        foreach (var assem in projectReference.Package.Assemblies)
                        {
                            list.Add(assem.Location);
                            var cfg = (JypeliProjectConfiguration)solutionConfiguration.GetConfiguration(this);
                            if (cfg.DebugMode)
                            {
                                var mdbFile = TargetRuntime.GetAssemblyDebugInfoFile(assem.Location);
                                if (System.IO.File.Exists(mdbFile))
                                    list.Add(mdbFile);
                            }

							// Copy dll.config -file if one exists
							string cfgfile = assem.Location + ".config";
							if (System.IO.File.Exists(cfgfile))
								list.Add(cfgfile);
                        }
                    }
                    break;
                }
            }
        }
	}
}

