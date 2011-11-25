//------------------------------------------------------------------------------------
// <copyright file="SemVerGitHarvester.cs" company="Stephen Jennings">
//   Copyright 2011 Stephen Jennings. Licensed under the Apache License, Version 2.0.
// </copyright>
//------------------------------------------------------------------------------------

namespace SemVerHarvester
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    ///     Parses the output of git-describe into semantic versioning components.
    /// </summary>
    public class SemVerGitHarvester : Harvester
    {
        private GitDescribeRunner gitDescribeRunner;

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
        [Required]
        public string GitPath { get; set; }

        /// <summary>
        ///     Executes the task.
        /// </summary>
        /// <returns>True on success.</returns>
        public override bool Execute()
        {
            if (this.GitPath == null)
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
    }
}
