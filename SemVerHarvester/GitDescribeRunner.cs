//------------------------------------------------------------------------------------
// <copyright file="GitDescribeRunner.cs" company="Stephen Jennings">
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
    ///     Runs git-describe and returns the resulting string.
    /// </summary>
    public class GitDescribeRunner
    {
        /// <summary>
        ///     Runs git-describe.
        /// </summary>
        /// <param name="gitPath">The path to the git executable.</param>
        /// <returns>Return value of git-describe.</returns>
        public virtual string Run(string gitPath)
        {
            var psi = new ProcessStartInfo()
            {
                FileName = gitPath,
                Arguments = @"describe --always --long --dirty=-modified --match v[0-9]*",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            var process = Process.Start(psi);
            if (!process.WaitForExit(10000))
            {
                throw new Exception("Git-describe did not return within 10 seconds.");
            }

            var stdout = process.StandardOutput.ReadToEnd();
            var length = stdout.Length;
            return stdout.Substring(0, length - 1);
        }
    }
}
