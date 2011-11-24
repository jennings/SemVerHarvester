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
            throw new NotImplementedException();
        }
    }
}
