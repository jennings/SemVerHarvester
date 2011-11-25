//------------------------------------------------------------------------------------
// <copyright file="Harvester.cs" company="Stephen Jennings">
//   Copyright 2011 Stephen Jennings. Licensed under the Apache License, Version 2.0.
// </copyright>
//------------------------------------------------------------------------------------

namespace SemVerHarvester
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Build.Utilities;
    using Microsoft.Build.Framework;

    public abstract class Harvester : Task
    {
        private bool dirty;

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
        ///     Gets the unique commit ID from the repository (e.g., a7cd1c4).
        /// </summary>
        [Output]
        public string CommitId { get; private set; }

        /// <summary>
        ///     Gets a string that is empty on clean checkout, or " (Modified)" on
        ///     a dirty checkout.
        /// </summary>
        [Output]
        public string ModifiedString
        {
            get
            {
                return this.dirty ? " (Modified)" : String.Empty;
            }
        }

    }
}
