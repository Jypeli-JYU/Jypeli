#region MIT License
/*
 * Copyright (c) 2018 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

#endregion

/*
 * Author: Rami Pasanen.
 */

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Jypeli.MSBuildExtensions
{
    /// <summary>
    /// A build task that runs the MonoGame Content Builder (MGCB.exe).
    /// </summary>
    public class MGCBTask : Task
    {
        private string _platform = "Windows";

        public string Platform
        {
            get { return _platform; }
            set { _platform = value; }
        }

        private string contentFilePath;

        [Required]
        public string ContentFilePath { get => contentFilePath; set => contentFilePath = value; }


        private string outputDir;

        [Required]
        public string OutputDir { get => outputDir; set => outputDir = value; }


        private string intermediateDir;

        [Required]
        public string IntermediateDir { get => intermediateDir; set => intermediateDir = value; }

        public override bool Execute()
        {
            // For some reason MGCB.exe expects to find the files defined in 
            // Content.mgcb from its current working directory instead of
            // looking for them from the same directory with Content.mgcb.

            // Changing the working directory might cause issues for other
            // parts of the build process though, so as a hack we'll change
            // the working directory and then revert it back to its original
            // value once we're done running MGCB.
            string originalWorkingDirectory = Environment.CurrentDirectory;

            string mgcbPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\MSBuild\MonoGame\v3.0\Tools\MGCB.exe";
            if (!File.Exists(mgcbPath))
            {
                Log.LogError("(Jypeli.MGCBTask): MonoGame Content Builder (MGCB.exe) not found! Jypeli expects to find it from " + mgcbPath);
                return false;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo(mgcbPath);
            string mgcbCommandLine = string.Format("/@:\"" + ContentFilePath + "\" " +
                "/platform:{0} /outputDir:\"{1}\" /intermediateDir:\"{2}\"",
                Platform, OutputDir, IntermediateDir);
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Jypeli.MGCBTask: Command line: " + mgcbCommandLine,
                string.Empty, "Jypeli.MGCBTask", MessageImportance.High));
            startInfo.Arguments = mgcbCommandLine;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            string output = string.Empty;

            Environment.CurrentDirectory = Path.GetDirectoryName(ContentFilePath);

            Process mgcbProcess = Process.Start(startInfo);
            output = mgcbProcess.StandardOutput.ReadToEnd();
            mgcbProcess.EnableRaisingEvents = true;
            mgcbProcess.WaitForExit();

            Environment.CurrentDirectory = originalWorkingDirectory;

            BuildEngine.LogMessageEvent(new BuildMessageEventArgs("MGCB Output: " + output, string.Empty, "Jypeli.MGCBTask", MessageImportance.High));
            Log.LogMessage("MGCB Output: " + output);

            int exitCode = mgcbProcess.ExitCode;
            mgcbProcess.Dispose();

            if (exitCode != 0)
            {
                Log.LogError("(Jypeli.MGCBTask): MGCB.exe exited with error code " + mgcbProcess.ExitCode);
                return false;
            }

            return true;
        }
    }
}
