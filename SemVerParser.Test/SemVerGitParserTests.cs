//------------------------------------------------------------------------------------
// <copyright file="SemVerGitParserTests.cs" company="Stephen Jennings">
//   Copyright 2011 Stephen Jennings. Licensed under the Apache License, Version 2.0.
// </copyright>
//------------------------------------------------------------------------------------

namespace SemVerParser.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Moq;
    using NUnit.Framework;
    using Microsoft.Build.Framework;

    /// <summary>
    ///     Tests the SemVerGetParser class.
    /// </summary>
    public class SemVerGitParserTests
    {
        /// <summary>
        ///     Confirms that Execute fails if GitPath is not set.
        /// </summary>
        [Test]
        public void Execute_returns_false_if_git_path_is_not_set()
        {
            var runner = this.CreateMockDescribeRunner("v1.2.3-4-g1a2b3c4d");

            var parser = new SemVerGitParser(runner);
            parser.BuildEngine = this.CreateMockBuildEngine();
            var returnValue = parser.Execute();

            Assert.AreEqual(false, returnValue);
        }

        private GitDescribeRunner CreateMockDescribeRunner(string runReturnValue)
        {
            var mockRunner = new Mock<GitDescribeRunner>();
            mockRunner.Setup(r => r.Run(It.IsAny<string>())).Returns(runReturnValue);
            return mockRunner.Object;
        }

        private IBuildEngine CreateMockBuildEngine()
        {
            var mockEngine = new Mock<IBuildEngine>();
            return mockEngine.Object;
        }
    }
}
