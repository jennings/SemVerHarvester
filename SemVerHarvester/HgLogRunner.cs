//------------------------------------------------------------------------------------
// <copyright file="HgLogRunner.cs" company="Stephen Jennings">
//   Copyright 2011 Stephen Jennings. Licensed under the Apache License, Version 2.0.
// </copyright>
//------------------------------------------------------------------------------------

namespace SemVerHarvester
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    ///     Runs hg-log and returns the resulting string.
    /// </summary>
    public class HgLogRunner
    {
        /// <summary>
        ///     Gets the latest tag from the Mercurial database.
        /// </summary>
        public string LatestTag { get; private set; }

        /// <summary>
        ///     Gets the number of commits since the latest tag in the Mercurial database.
        /// </summary>
        public string LatestTagDistance { get; private set; }

        /// <summary>
        ///     Gets the commit ID of the current commit.
        /// </summary>
        public string Node { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the working directory is dirty.
        /// </summary>
        public bool Dirty { get; private set; }

        /// <summary>
        ///     Runs hg-log.
        /// </summary>
        /// <param name="mercurialPath">The path to the hg executable.</param>
        public virtual void Run(string mercurialPath)
        {
            var latestTag = this.ResultFromMercurial(mercurialPath, "log -r . --template '{latesttag}'");
            this.LatestTag = latestTag;

            var latestTagDistance = this.ResultFromMercurial(mercurialPath, "log -r . --template '{latesttagdistance}'");
            this.LatestTagDistance = latestTagDistance;

            var node = this.ResultFromMercurial(mercurialPath, "log -r . --template '{node|short}'");
            this.Node = node;

            var changedFiles = this.ResultFromMercurial(mercurialPath, "status --added --modified --removed --deleted --subrepos");
            this.Dirty = !String.IsNullOrEmpty(changedFiles);
        }

        private string ResultFromMercurial(string mercurialPath, string arguments)
        {
            var psi = new ProcessStartInfo()
            {
                FileName = mercurialPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            var process = Process.Start(psi);
            if (!process.WaitForExit(10000))
            {
                throw new Exception("Hg-log did not return within 10 seconds.");
            }

            var stdout = process.StandardOutput.ReadToEnd();
            return stdout;
        }
    }
}
