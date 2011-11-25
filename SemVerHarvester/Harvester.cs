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
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    ///     Base class for all harvesters.
    /// </summary>
    public abstract class Harvester : Task
    {
        /// <summary>
        ///     Gets or sets the major version number (X.0.0.0).
        /// </summary>
        [Output]
        public string MajorVersion { get; protected set; }

        /// <summary>
        ///     Gets or sets the minor version number (0.Y.0.0).
        /// </summary>
        [Output]
        public string MinorVersion { get; protected set; }

        /// <summary>
        ///     Gets or sets the patch version number (0.0.Z.0).
        /// </summary>
        [Output]
        public string PatchVersion { get; protected set; }

        /// <summary>
        ///     Gets or sets the revision version number (0.0.0.W).
        /// </summary>
        [Output]
        public string RevisionVersion { get; protected set; }

        /// <summary>
        ///     Gets or sets the unique commit ID from the repository (e.g., a7cd1c4).
        /// </summary>
        [Output]
        public string CommitId { get; protected set; }

        /// <summary>
        ///     Gets a string that is empty on clean checkout, or " (Modified)" on
        ///     a dirty checkout.
        /// </summary>
        [Output]
        public string ModifiedString
        {
            get
            {
                return this.Modified ? " (Modified)" : String.Empty;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the current checkout
        ///     has been modified.
        /// </summary>
        public bool Modified { get; protected set; }
    }
}
