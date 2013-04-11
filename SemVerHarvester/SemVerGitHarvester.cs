//------------------------------------------------------------------------------------
// <copyright file="SemVerGitHarvester.cs" company="Stephen Jennings">
//   Copyright 2011 Stephen Jennings. Licensed under the Apache License, Version 2.0.
// </copyright>
//------------------------------------------------------------------------------------

namespace SemVerHarvester
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    ///     Parses the output of git-describe into semantic versioning components.
    /// </summary>
    public class SemVerGitHarvester : Harvester
    {
        private GitDescribeRunner gitDescribeRunner;
        private string gitPath;

        /// <summary>
        ///     Initializes a new instance of the SemVerGitHarvester class.
        /// </summary>
        public SemVerGitHarvester()
            : this(new GitDescribeRunner())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the SemVerGitHarvester class with the given
        ///     GitDescribeRunner, used for unit testing.
        /// </summary>
        /// <param name="gitDescribeRunner">The GitDescribeRunner to use.</param>
        public SemVerGitHarvester(GitDescribeRunner gitDescribeRunner)
        {
            this.gitDescribeRunner = gitDescribeRunner;
        }

        /// <summary>
        ///     Gets or sets the path to the Git executable.
        /// </summary>
        public string GitPath 
        { 
            get 
            {
                if (this.StringIsNullOrWhitespace(this.gitPath))
                {
                    this.gitPath = this.FindGitExe();
                }

                return this.gitPath;
            }

            set 
            { 
                this.gitPath = value; 
            }
        }

        /// <summary>
        ///     Executes the task.
        /// </summary>
        /// <returns>True on success.</returns>
        public override bool Execute()
        {
            if (this.StringIsNullOrWhitespace(this.GitPath))
            {
                this.Log.LogError("GitPath must be set to use SemVerGitHarvester.");
                return false;
            }

            this.Log.LogMessage("Git executable path: " + this.GitPath);
            this.Log.LogMessage("Git working path: " + System.Environment.CurrentDirectory);
            string gitDescribe;

            try
            {
                gitDescribe = this.gitDescribeRunner.Run(this.GitPath);
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex);
                return false;
            }

            this.Log.LogMessage("Git describe result: " + gitDescribe);

            this.SetPropertiesFromGitDescribe(gitDescribe);

            return true;
        }

        private void SetPropertiesFromGitDescribe(string gitDescribeResult)
        {
            var emptyCleanRx = new Regex(@"^([0-9a-f]{5,32})$");
            var emptyDirtyRx = new Regex(@"^([0-9a-f]{5,32})-modified$");
            var cleanRx = new Regex(@"^v([0-9]+)\.([0-9]+)\.([0-9]+)-([0-9]+)-g([0-9a-f]+)$");
            var dirtyRx = new Regex(@"^v([0-9]+)\.([0-9]+)\.([0-9]+)-([0-9]+)-g([0-9a-f]+)-modified$");

            if (emptyCleanRx.IsMatch(gitDescribeResult))
            {
                var match = emptyCleanRx.Match(gitDescribeResult);
                this.MajorVersion = "0";
                this.MinorVersion = "0";
                this.PatchVersion = "0";
                this.RevisionVersion = "0";
                this.Modified = false;
                this.CommitId = match.Groups[1].Value.ToString();
            }
            else if (emptyDirtyRx.IsMatch(gitDescribeResult))
            {
                var match = emptyDirtyRx.Match(gitDescribeResult);
                this.MajorVersion = "0";
                this.MinorVersion = "0";
                this.PatchVersion = "0";
                this.RevisionVersion = "0";
                this.Modified = true;
                this.CommitId = match.Groups[1].Value.ToString();
            }
            else if (cleanRx.IsMatch(gitDescribeResult))
            {
                var match = cleanRx.Match(gitDescribeResult);
                this.MajorVersion = Convert.ToInt32(match.Groups[1].Value).ToString();
                this.MinorVersion = Convert.ToInt32(match.Groups[2].Value).ToString();
                this.PatchVersion = Convert.ToInt32(match.Groups[3].Value).ToString();
                this.RevisionVersion = Convert.ToInt32(match.Groups[4].Value).ToString();
                this.Modified = false;
                this.CommitId = match.Groups[5].Value.ToString();
            }
            else if (dirtyRx.IsMatch(gitDescribeResult))
            {
                var match = dirtyRx.Match(gitDescribeResult);
                this.MajorVersion = Convert.ToInt32(match.Groups[1].Value).ToString();
                this.MinorVersion = Convert.ToInt32(match.Groups[2].Value).ToString();
                this.PatchVersion = Convert.ToInt32(match.Groups[3].Value).ToString();
                this.RevisionVersion = Convert.ToInt32(match.Groups[4].Value).ToString();
                this.Modified = true;
                this.CommitId = match.Groups[5].Value.ToString();
            }
            else
            {
                this.MajorVersion = "0";
                this.MinorVersion = "0";
                this.PatchVersion = "0";
                this.RevisionVersion = "1";
                this.Modified = true;
                this.CommitId = String.Empty;
            }
        }

        private string Exec(string wd, string command, string args)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.FileName = command;
            p.StartInfo.Arguments = args;
            p.StartInfo.WorkingDirectory = wd;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            
            p.WaitForExit();
            return output;
        }

        private string FindGitExe()
        {
            var checkDirs = new List<string>();
            checkDirs.AddRange(Environment.GetEnvironmentVariable("PATH").Split(';'));

            foreach (var x in this.GetProgramFilesFolders())
            {
                checkDirs.Add(Path.Combine(x, @"git\bin"));
            }

            foreach (var dir in checkDirs)
            {
                var checkPath = Path.Combine(dir, "git.exe");
                Log.LogMessage(MessageImportance.Low, "Searching for git.exe, probing location: '{0}'", checkPath);
                
                if (File.Exists(checkPath))
                {
                    return checkPath;
                }
            }

            Log.LogError("Could not find git.exe, please specify GitPath explicity, or ensure git.exe is in the PATH");
            throw new Exception("Could not find git.exe, make sure it's in path");
        }

        private bool StringIsNullOrWhitespace(string test)
        {
            if (string.IsNullOrEmpty(test))
            {
                return true;
            }

            if (test.Trim().Length == 0)
            {
                return true;
            }
            
            return false;
        }

        private IEnumerable<string> GetProgramFilesFolders()
        {
            var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            yield return pf;

            if (pf.Contains("(x86)"))
            {
                yield return pf.Replace(" (x86)", string.Empty);
            }
            else
            {
                yield return pf + " (x86)";
            }
        }
    }
}
