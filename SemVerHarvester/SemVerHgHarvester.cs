//------------------------------------------------------------------------------------
// <copyright file="SemVerHgHarvester.cs" company="Stephen Jennings">
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
    ///     Parses the output of hg-log into semantic versioning components.
    /// </summary>
    public class SemVerHgHarvester : Harvester
    {
        private HgLogRunner mercurialLogRunner;

        /// <summary>
        ///     Initializes a new instance of the SemVerHgHarvester class.
        /// </summary>
        public SemVerHgHarvester()
            : this(new HgLogRunner())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the SemVerHgHarvester class with the given
        ///     HgLogRunner, used for unit testing.
        /// </summary>
        /// <param name="mercurialLogRunner">The HgLogRunner to use.</param>
        public SemVerHgHarvester(HgLogRunner mercurialLogRunner)
        {
            this.mercurialLogRunner = mercurialLogRunner;
        }

        /// <summary>
        ///     Gets or sets the path to the Git executable.
        /// </summary>
        [Required]
        public string HgPath { get; set; }

        /// <summary>
        ///     Executes the task.
        /// </summary>
        /// <returns>True on success.</returns>
        public override bool Execute()
        {
            if (this.HgPath == null)
            {
                this.Log.LogError("HgPath must be set to use SemVerHgHarvester.");
                return false;
            }

            this.Log.LogMessage("Hg executable path: " + this.HgPath);
            this.Log.LogMessage("Hg working path: " + System.Environment.CurrentDirectory);

            try
            {
                this.mercurialLogRunner.Run(this.HgPath);
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex);
                return false;
            }

            this.SetPropertiesFromHgDescribe();

            return true;
        }

        private void SetPropertiesFromHgDescribe()
        {
            var tagRx = new Regex(@"^v([0-9]+)\.([0-9]+)\.([0-9]+)$");
            if (tagRx.IsMatch(this.mercurialLogRunner.LatestTag))
            {
                var match = tagRx.Match(this.mercurialLogRunner.LatestTag);
                this.MajorVersion = Convert.ToInt32(match.Groups[1].Value).ToString();
                this.MinorVersion = Convert.ToInt32(match.Groups[2].Value).ToString();
                this.PatchVersion = Convert.ToInt32(match.Groups[3].Value).ToString();

                this.RevisionVersion = Convert.ToInt32(this.mercurialLogRunner.LatestTagDistance).ToString();
            }
            else
            {
                this.MajorVersion = "0";
                this.MinorVersion = "0";
                this.PatchVersion = "0";
                this.RevisionVersion = "0";
            }

            this.CommitId = this.mercurialLogRunner.Node;
            this.Modified = this.mercurialLogRunner.Dirty;
        }
    }
}
