//------------------------------------------------------------------------------------
// <copyright file="SemVerGitParser.cs" company="Stephen Jennings">
//   Copyright 2011 Stephen Jennings. Licensed under the Apache License, Version 2.0.
// </copyright>
//------------------------------------------------------------------------------------

namespace SemVerParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    ///     Parses the output of git-describe into semantic versioning components.
    /// </summary>
    public class SemVerGitParser : Task
    {
        private GitDescribeRunner gitDescribeRunner;

        /// <summary>
        ///     Initializes a new instance of the SemVerGitParser class.
        /// </summary>
        public SemVerGitParser()
            : this(new GitDescribeRunner())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the SemVerGitParser class with the given
        ///     GitDescribeRunner, used for unit testing.
        /// </summary>
        /// <param name="gitDescribeRunner">The GitDescribeRunner to use.</param>
        public SemVerGitParser(GitDescribeRunner gitDescribeRunner)
        {
            this.gitDescribeRunner = gitDescribeRunner;
        }

        /// <summary>
        ///     Gets or sets the path to the Git executable.
        /// </summary>
        [Required]
        public string GitPath { get; set; }

        /// <summary>
        ///     Gets the major version number (X.0.0.0).
        /// </summary>
        [Output]
        public string MajorVersion { get; private set; }

        /// <summary>
        ///     Gets the minor version number (0.Y.0.0).
        /// </summary>
        [Output]
        public string MinorVersion { get; private set; }

        /// <summary>
        ///     Gets the patch version number (0.0.Z.0).
        /// </summary>
        [Output]
        public string PatchVersion { get; private set; }

        /// <summary>
        ///     Gets the revision version number (0.0.0.W).
        /// </summary>
        [Output]
        public string RevisionVersion { get; private set; }

        /// <summary>
        ///     Executes the task.
        /// </summary>
        /// <returns>True on success.</returns>
        public override bool Execute()
        {
            if (this.GitPath == null)
            {
                this.Log.LogError("GitPath must be set to use SemVerGitParser.");
                return false;
            }

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

            this.SetPropertiesFromGitDescribe(gitDescribe);

            return true;
        }

        private void SetPropertiesFromGitDescribe(string gitDescribeResult)
        {
        }
    }
}
