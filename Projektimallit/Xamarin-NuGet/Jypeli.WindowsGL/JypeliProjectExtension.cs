using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using System;
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

		private bool IsLibraryFile(string fileName)
		{
			return fileName.EndsWith(".dll", StringComparison.InvariantCulture) ||
		           fileName.EndsWith(".dylib", StringComparison.InvariantCulture) ||
		           fileName.EndsWith(".dll.config", StringComparison.InvariantCulture);
		}

		private bool IsContentFile(string fileName)
		{
			string[] parts = fileName.Split('\\', '/');
			return parts.Length > 1 && parts[parts.Length - 2].ToLower() == "content";
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

				foreach (var item in objs)
				{
					if (item is ProjectFile)
					{
						var fileItem = (ProjectFile)item;

						// Copy DLL files to output directory
						if (IsLibraryFile(fileItem.Name))
							fileItem.CopyToOutputDirectory = FileCopyMode.PreserveNewest;

						// Mark content files as content and copy to output directory
						if (IsContentFile(fileItem.Name))
						{
							fileItem.BuildAction = "Content";
							fileItem.CopyToOutputDirectory = FileCopyMode.PreserveNewest;
						}
					}
				}
			}

			base.OnItemsAdded(objs);
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
