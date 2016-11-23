using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using System;
using System.Collections.Generic;
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
            return file.Extension == ".dll" ||
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
                var oldXnbFiles = new List<String>();
                var xnbFiles = new List<FilePath>();
                var contentDir = Project.ItemDirectory.Combine("Content");
                //var outputDir = Project.GetOutputFileName(configuration).ParentDirectory.CanonicalPath.Combine("Content").ToString();
                MGCB mgcb = null;

                try
                {
                    //mgcb = new MGCB(contentDir, outputDir);
                    mgcb = new MGCB(contentDir, contentDir);
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

                    // Mark dylib files as native references
                    if (fileItem.FilePath.Extension == ".dylib")
                        fileItem.BuildAction = "NativeReference";

                    // Mark content files as content and copy to output directory
                    if (IsContentFile(fileItem.FilePath))
                    {
                        fileItem.BuildAction = "Content";
                        fileItem.CopyToOutputDirectory = FileCopyMode.PreserveNewest;

#if !NOMGCB
                        if (fileItem.FilePath.Extension == ".xnb")
                        {
                            oldXnbFiles.Add(fileItem.FilePath.CanonicalPath.ToString());
                        }
                        else if (mgcb != null)
                        {
                            string importer = null;
                            string processor = null;
                            string assembly = null;

                            fileItem.BuildAction = BuildAction.None;
                            fileItem.CopyToOutputDirectory = FileCopyMode.None;

                            switch (fileItem.FilePath.Extension)
                            {
                                case ".wav":
                                    importer = "WavImporter";
                                    processor = "SoundEffectProcessor";
                                    break;
                                case ".mp3":
                                    importer = "Mp3Importer";
                                    processor = "SongProcessor";
                                    break;
                                case ".txt":
                                    importer = "TextFileImporter";
                                    processor = "TextFileContentProcessor";
                                    assembly = "TextFileContentExtension.dll";
                                    break;
                            }

                            var xnbFile = mgcb.BuildContent(monitor, fileItem.FilePath, importer, processor, assembly);
                            xnbFiles.Add(xnbFile);
                        }
#endif
                    }
                }

#if !NOMGCB
                xnbFiles.RemoveAll((f) => oldXnbFiles.Contains(f.CanonicalPath.ToString()));
                foreach (var pf in Project.AddFiles(xnbFiles, "Content"))
                    pf.CopyToOutputDirectory = FileCopyMode.PreserveNewest;
#endif
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
