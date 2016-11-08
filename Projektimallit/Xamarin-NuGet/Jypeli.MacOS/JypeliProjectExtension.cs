using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Jypeli.Projects
{
	public class JypeliProjectExtension : DotNetProjectExtension
	{
		private FilePath openFile = null;

		public JypeliProjectExtension()
			: base()
		{
		}

		private bool IsJypeliProject(Project project)
		{
			foreach (var item in project.Items)
			{
				if (item.ItemName == "Reference" && item.Include == "Jypeli")
					return true;
			}

			return false;
		}

		private bool IsLibraryFile(FilePath file)
		{
			return file.Extension == ".dll" || file.Extension == ".dylib" ||
		           file.FileName.EndsWith(".dll.config", StringComparison.InvariantCulture);
		}

		private bool IsContentFile(FilePath file)
		{
			return !file.IsDirectory && file.IsChildPathOf(Project.ItemDirectory.Combine("Content"));
		}

		protected override void OnItemsAdded(System.Collections.Generic.IEnumerable<ProjectItem> objs)
		{
			if (IsJypeliProject(Project))
			{
				if (this.openFile != null)
				{
					var file = this.openFile;
					this.openFile = null;
                    Runtime.RunInMainThread(() => IdeApp.Workbench.OpenDocument(file, Project, true));
				}
			}

			base.OnItemsAdded(objs);
		}

		protected override Task<BuildResult> OnBuild(ProgressMonitor monitor, ConfigurationSelector configuration, OperationContext operationContext)
		{
			if (IsJypeliProject(Project))
			{
#if !NOMGCB
				var contentDir = Project.ItemDirectory.Combine("Content");
				var outputDir = Project.GetOutputFileName(configuration).ParentDirectory.CanonicalPath.Combine("Content").ToString();
                MGCB mgcb = null;

				try
				{
					mgcb = new MGCB(contentDir, outputDir);
				}
				catch (NotSupportedException e)
				{
					monitor.Log.WriteLine("Could not initialize content builder - " + e.Message);
				}
#endif

				foreach (var item in Project.Items)
				{
					var fileItem = item as ProjectFile;
					if (fileItem == null)
						continue;

					// Copy DLL files to output directory
					if (IsLibraryFile(fileItem.FilePath))
						fileItem.CopyToOutputDirectory = FileCopyMode.PreserveNewest;

					// Mark content files as content and copy to output directory
					if (IsContentFile(fileItem.FilePath))
					{
                        fileItem.BuildAction = "Content";
                        fileItem.CopyToOutputDirectory = FileCopyMode.PreserveNewest;

#if !NOMGCB
                        if (mgcb != null && fileItem.Name.EndsWith (".wav", StringComparison.InvariantCulture)) {
                            // Sound effect
                            fileItem.CopyToOutputDirectory = FileCopyMode.None;
                            mgcb.BuildContent (monitor, fileItem.FilePath, "WavImporter", "SoundEffectProcessor");
                        } else if (mgcb != null && fileItem.Name.EndsWith (".mp3", StringComparison.InvariantCulture)) {
                            // Music
                            fileItem.CopyToOutputDirectory = FileCopyMode.None;
                            mgcb.BuildContent (monitor, fileItem.FilePath, "Mp3Importer", "SongProcessor");
						}
                        else
                        {
                            fileItem.CopyToOutputDirectory = FileCopyMode.None;
                            mgcb.BuildContent (monitor, fileItem.FilePath);
                        }
#endif
					}
				}
			}

			return base.OnBuild(monitor, configuration, operationContext);
		}

		protected override ProjectItem OnCreateProjectItem(MonoDevelop.Projects.MSBuild.IMSBuildItemEvaluated item)
		{
			if (item.Include == Project.Name + ".cs")
			{
				// Open the main .cs file in editor
				this.openFile = Project.ItemDirectory.Combine(new FilePath(item.Include));
			}

			return base.OnCreateProjectItem(item);
		}
	}
}
