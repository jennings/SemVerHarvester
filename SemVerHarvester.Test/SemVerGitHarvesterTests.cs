//------------------------------------------------------------------------------------
// <copyright file="SemVerGitHarvesterTests.cs" company="Stephen Jennings">
//   Copyright 2011 Stephen Jennings. Licensed under the Apache License, Version 2.0.
// </copyright>
//------------------------------------------------------------------------------------

namespace SemVerHarvester.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Build.Framework;
    using Moq;
    using Xunit;
    
    /// <summary>
    ///     Tests the SemVerGetParser class.
    /// </summary>
    public class SemVerGitHarvesterTests
    {
        private const string GitPath = @"C:\Program Files\Git\bin\git.exe";

        #region Error handling tests

        /// <summary>
        ///     Check that Execute works even if GitPath is not specified or null
        /// </summary>
        [Fact]
        public void Auto_detect_git_path()
        {
            var runner = this.CreateMockDescribeRunner("v1.2.3-4-g1a2b3c4d");
            var harvester = new SemVerGitHarvester(runner);
            harvester.BuildEngine = this.CreateMockBuildEngine();
            harvester.GitPath = null;

            var success = harvester.Execute();
            if (success)
            {
                Assert.True(File.Exists(harvester.GitPath));
            }
            else
            {
                Assert.True(harvester.GitPath == null);
            }
        }

        /// <summary>
        ///     Verifies that if git-describe fails for whatever reason (defined by the Run
        ///     method throwing an exception), then false is returned.
        /// </summary>
        [Fact]
        public void Execute_returns_false_if_git_describe_fails()
        {
            var mockRunner = new Mock<GitDescribeRunner>();
            mockRunner.Setup(r => r.Run(It.IsAny<string>())).Throws<Exception>();
            var runner = mockRunner.Object;

            var harvester = new SemVerGitHarvester(runner);
            harvester.BuildEngine = this.CreateMockBuildEngine();
            harvester.GitPath = SemVerGitHarvesterTests.GitPath;
            var returnValue = harvester.Execute();

            Assert.Equal(false, returnValue);
        }

        #endregion

        #region Empty version tests

        /// <summary>
        ///     Verifies that the version number is set to 0.0.0.0 when no
        ///     tags are in the source repository (that is, git-describe returns
        ///     just a commit sha1).
        /// </summary>
        [Fact]
        public void Execute_sets_version_to_0_0_0_0_when_no_tag_is_found_and_clean_checkout()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("1a2b3c4", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("0", harvester.MajorVersion);
            Assert.Equal("0", harvester.MinorVersion);
            Assert.Equal("0", harvester.PatchVersion);
            Assert.Equal("0", harvester.RevisionVersion);
            Assert.Equal(String.Empty, harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies that the version number is set to 0.0.0.0 when no
        ///     tags are in the source repository (that is, git-describe returns
        ///     just a commit sha1) and the working directory is dirty.
        /// </summary>
        [Fact]
        public void Execute_sets_version_to_0_0_0_0_when_no_tag_is_found_and_dirty_checkout()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("1a2b3c4-modified", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("0", harvester.MajorVersion);
            Assert.Equal("0", harvester.MinorVersion);
            Assert.Equal("0", harvester.PatchVersion);
            Assert.Equal("0", harvester.RevisionVersion);
            Assert.Equal(" (Modified)", harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        #endregion

        #region Clean, tagged checkout tests

        /// <summary>
        ///     Verifies a clean checkout of a version supplies the correct
        ///     version numbers.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_clean_tag_checkout_1()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v1.2.3-0-g1a2b3c4", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("1", harvester.MajorVersion);
            Assert.Equal("2", harvester.MinorVersion);
            Assert.Equal("3", harvester.PatchVersion);
            Assert.Equal("0", harvester.RevisionVersion);
            Assert.Equal(String.Empty, harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a clean checkout of a version supplies the correct
        ///     version numbers, even with two-digit version components.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_clean_tag_checkout_2()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v10.20.30-0-g1a2b3c4", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("10", harvester.MajorVersion);
            Assert.Equal("20", harvester.MinorVersion);
            Assert.Equal("30", harvester.PatchVersion);
            Assert.Equal("0", harvester.RevisionVersion);
            Assert.Equal(String.Empty, harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a clean checkout of a version supplies the correct
        ///     version numbers, even if the tag has leading zeroes.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_clean_tag_checkout_3()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v01.02.03-0-g1a2b3c4", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("1", harvester.MajorVersion);
            Assert.Equal("2", harvester.MinorVersion);
            Assert.Equal("3", harvester.PatchVersion);
            Assert.Equal("0", harvester.RevisionVersion);
            Assert.Equal(String.Empty, harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a clean checkout of a version supplies the correct
        ///     version numbers, even if the patch component is zero.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_clean_tag_checkout_4()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v2.1.0-0-g1a2b3c4", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("2", harvester.MajorVersion);
            Assert.Equal("1", harvester.MinorVersion);
            Assert.Equal("0", harvester.PatchVersion);
            Assert.Equal("0", harvester.RevisionVersion);
            Assert.Equal(String.Empty, harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        #endregion

        #region Clean, non-tagged checkout tests

        /// <summary>
        ///     Verifies a clean checkout of a derived version supplies
        ///     the correct version numbers.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_clean_nontagged_checkout_1()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v1.2.3-4-g1a2b3c4", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("1", harvester.MajorVersion);
            Assert.Equal("2", harvester.MinorVersion);
            Assert.Equal("3", harvester.PatchVersion);
            Assert.Equal("4", harvester.RevisionVersion);
            Assert.Equal(String.Empty, harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a clean checkout of a derived version supplies
        ///     the correct version numbers, even with two-digit numbers.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_clean_nontagged_checkout_2()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v10.20.30-15-g1a2b3c4", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("10", harvester.MajorVersion);
            Assert.Equal("20", harvester.MinorVersion);
            Assert.Equal("30", harvester.PatchVersion);
            Assert.Equal("15", harvester.RevisionVersion);
            Assert.Equal(String.Empty, harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a clean checkout of a derived version supplies
        ///     the correct version numbers, even with leading zeroes.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_clean_nontagged_checkout_3()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v01.02.03-4-g1a2b3c4", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("1", harvester.MajorVersion);
            Assert.Equal("2", harvester.MinorVersion);
            Assert.Equal("3", harvester.PatchVersion);
            Assert.Equal("4", harvester.RevisionVersion);
            Assert.Equal(String.Empty, harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a clean checkout of a derived version supplies
        ///     the correct version numbers.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_clean_nontagged_checkout_4()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v2.1.0-8-g1a2b3c4", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("2", harvester.MajorVersion);
            Assert.Equal("1", harvester.MinorVersion);
            Assert.Equal("0", harvester.PatchVersion);
            Assert.Equal("8", harvester.RevisionVersion);
            Assert.Equal(String.Empty, harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }
        #endregion

        #region Dirty, tagged checkout tests

        /// <summary>
        ///     Verifies a dirty checkout of a version supplies the correct
        ///     version numbers.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_dirty_tag_checkout_1()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v1.2.3-0-g1a2b3c4-modified", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("1", harvester.MajorVersion);
            Assert.Equal("2", harvester.MinorVersion);
            Assert.Equal("3", harvester.PatchVersion);
            Assert.Equal("0", harvester.RevisionVersion);
            Assert.Equal(" (Modified)", harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a dirty checkout of a version supplies the correct
        ///     version numbers, even with two-digit version components.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_dirty_tag_checkout_2()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v10.20.30-0-g1a2b3c4-modified", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("10", harvester.MajorVersion);
            Assert.Equal("20", harvester.MinorVersion);
            Assert.Equal("30", harvester.PatchVersion);
            Assert.Equal("0", harvester.RevisionVersion);
            Assert.Equal(" (Modified)", harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a dirty checkout of a version supplies the correct
        ///     version numbers, even if the tag has leading zeroes.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_dirty_tag_checkout_3()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v01.02.03-0-g1a2b3c4-modified", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("1", harvester.MajorVersion);
            Assert.Equal("2", harvester.MinorVersion);
            Assert.Equal("3", harvester.PatchVersion);
            Assert.Equal("0", harvester.RevisionVersion);
            Assert.Equal(" (Modified)", harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a dirty checkout of a version supplies the correct
        ///     version numbers, even if the patch component is zero.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_dirty_tag_checkout_4()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v2.1.0-0-g1a2b3c4-modified", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("2", harvester.MajorVersion);
            Assert.Equal("1", harvester.MinorVersion);
            Assert.Equal("0", harvester.PatchVersion);
            Assert.Equal("0", harvester.RevisionVersion);
            Assert.Equal(" (Modified)", harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        #endregion

        #region Dirty, non-tagged checkout tests

        /// <summary>
        ///     Verifies a dirty checkout of a derived version supplies
        ///     the correct version numbers.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_dirty_nontagged_checkout_1()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v1.2.3-4-g1a2b3c4-modified", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("1", harvester.MajorVersion);
            Assert.Equal("2", harvester.MinorVersion);
            Assert.Equal("3", harvester.PatchVersion);
            Assert.Equal("4", harvester.RevisionVersion);
            Assert.Equal(" (Modified)", harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a dirty checkout of a derived version supplies
        ///     the correct version numbers, even with two-digit numbers.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_dirty_nontagged_checkout_2()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v10.20.30-15-g1a2b3c4-modified", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("10", harvester.MajorVersion);
            Assert.Equal("20", harvester.MinorVersion);
            Assert.Equal("30", harvester.PatchVersion);
            Assert.Equal("15", harvester.RevisionVersion);
            Assert.Equal(" (Modified)", harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a dirty checkout of a derived version supplies
        ///     the correct version numbers, even with leading zeroes.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_dirty_nontagged_checkout_3()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v01.02.03-4-g1a2b3c4-modified", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("1", harvester.MajorVersion);
            Assert.Equal("2", harvester.MinorVersion);
            Assert.Equal("3", harvester.PatchVersion);
            Assert.Equal("4", harvester.RevisionVersion);
            Assert.Equal(" (Modified)", harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a dirty checkout of a derived version supplies
        ///     the correct version numbers.
        /// </summary>
        [Fact]
        public void Execute_sets_version_on_dirty_nontagged_checkout_4()
        {
            SemVerGitHarvester harvester;
            var returnValue = this.StandardExecute("v2.1.0-8-g1a2b3c4-modified", out harvester);

            Assert.Equal(true, returnValue);
            Assert.Equal("2", harvester.MajorVersion);
            Assert.Equal("1", harvester.MinorVersion);
            Assert.Equal("0", harvester.PatchVersion);
            Assert.Equal("8", harvester.RevisionVersion);
            Assert.Equal(" (Modified)", harvester.ModifiedString);
            Assert.Equal("1a2b3c4", harvester.CommitId);
        }

        #endregion

        #region Helper methods

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

        private bool StandardExecute(string gitDescribeReturnValue, out SemVerGitHarvester harvester)
        {
            var runner = this.CreateMockDescribeRunner(gitDescribeReturnValue);

            harvester = new SemVerGitHarvester(runner);
            harvester.BuildEngine = this.CreateMockBuildEngine();
            harvester.GitPath = SemVerGitHarvesterTests.GitPath;
            return harvester.Execute();
        }

        #endregion
    }
}
