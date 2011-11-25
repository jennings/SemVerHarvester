//------------------------------------------------------------------------------------
// <copyright file="SemVerHgHarvesterTests.cs" company="Stephen Jennings">
//   Copyright 2011 Stephen Jennings. Licensed under the Apache License, Version 2.0.
// </copyright>
//------------------------------------------------------------------------------------

namespace SemVerHarvester.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Build.Framework;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    ///     Tests the SemVerHgHarvester class.
    /// </summary>
    public class SemVerHgHarvesterTests
    {
        private const string HgPath = @"C:\Program Files\TortoiseHg\hg.exe";

        #region Error handling tests

        /// <summary>
        ///     Confirms that Execute fails if HgPath is not set.
        /// </summary>
        [Test]
        public void Execute_returns_false_if_hg_path_is_not_set()
        {
            var runner = this.CreateMockLogRunner("v1.2.3", "4", "1a2b3c4d", false);

            var harvester = new SemVerHgHarvester(runner);
            harvester.BuildEngine = this.CreateMockBuildEngine();
            var returnValue = harvester.Execute();

            Assert.AreEqual(false, returnValue);
        }

        /// <summary>
        ///     Verifies that if hg-log fails for whatever reason (defined by the Run
        ///     method throwing an exception), then false is returned.
        /// </summary>
        [Test]
        public void Execute_returns_false_if_hg_log_fails()
        {
            var mockRunner = new Mock<HgLogRunner>();
            mockRunner.Setup(r => r.Run(It.IsAny<string>())).Throws<Exception>();
            var runner = mockRunner.Object;

            var harvester = new SemVerHgHarvester(runner);
            harvester.BuildEngine = this.CreateMockBuildEngine();
            harvester.HgPath = SemVerHgHarvesterTests.HgPath;
            var returnValue = harvester.Execute();

            Assert.AreEqual(false, returnValue);
        }

        #endregion

        #region Empty version tests

        /// <summary>
        ///     Verifies that the version number is set to 0.0.0.0 when no
        ///     tags are in the source repository (that is, hg-log returns
        ///     just a commit sha1).
        /// </summary>
        [Test]
        public void Execute_sets_version_to_0_0_0_0_when_no_tag_is_found_and_clean_checkout()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute(String.Empty, String.Empty, "1a2b3c4", false, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("0", harvester.MajorVersion);
            Assert.AreEqual("0", harvester.MinorVersion);
            Assert.AreEqual("0", harvester.PatchVersion);
            Assert.AreEqual("0", harvester.RevisionVersion);
            Assert.AreEqual(String.Empty, harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies that the version number is set to 0.0.0.0 when no
        ///     tags are in the source repository (that is, hg-log returns
        ///     just a commit sha1) and the working directory is dirty.
        /// </summary>
        [Test]
        public void Execute_sets_version_to_0_0_0_0_when_no_tag_is_found_and_dirty_checkout()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute(String.Empty, String.Empty, "1a2b3c4", true, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("0", harvester.MajorVersion);
            Assert.AreEqual("0", harvester.MinorVersion);
            Assert.AreEqual("0", harvester.PatchVersion);
            Assert.AreEqual("0", harvester.RevisionVersion);
            Assert.AreEqual(" (Modified)", harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        #endregion

        #region Clean, tagged checkout tests

        /// <summary>
        ///     Verifies a clean checkout of a version supplies the correct
        ///     version numbers.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_clean_tag_checkout_1()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v1.2.3", "0", "1a2b3c4", false, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("1", harvester.MajorVersion);
            Assert.AreEqual("2", harvester.MinorVersion);
            Assert.AreEqual("3", harvester.PatchVersion);
            Assert.AreEqual("0", harvester.RevisionVersion);
            Assert.AreEqual(String.Empty, harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a clean checkout of a version supplies the correct
        ///     version numbers, even with two-digit version components.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_clean_tag_checkout_2()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v10.20.30", "0", "1a2b3c4", false, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("10", harvester.MajorVersion);
            Assert.AreEqual("20", harvester.MinorVersion);
            Assert.AreEqual("30", harvester.PatchVersion);
            Assert.AreEqual("0", harvester.RevisionVersion);
            Assert.AreEqual(String.Empty, harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a clean checkout of a version supplies the correct
        ///     version numbers, even if the tag has leading zeroes.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_clean_tag_checkout_3()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v01.02.03", "0", "1a2b3c4", false, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("1", harvester.MajorVersion);
            Assert.AreEqual("2", harvester.MinorVersion);
            Assert.AreEqual("3", harvester.PatchVersion);
            Assert.AreEqual("0", harvester.RevisionVersion);
            Assert.AreEqual(String.Empty, harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a clean checkout of a version supplies the correct
        ///     version numbers, even if the patch component is zero.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_clean_tag_checkout_4()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v2.1.0", "0", "1a2b3c4", false, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("2", harvester.MajorVersion);
            Assert.AreEqual("1", harvester.MinorVersion);
            Assert.AreEqual("0", harvester.PatchVersion);
            Assert.AreEqual("0", harvester.RevisionVersion);
            Assert.AreEqual(String.Empty, harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        #endregion

        #region Clean, non-tagged checkout tests

        /// <summary>
        ///     Verifies a clean checkout of a derived version supplies
        ///     the correct version numbers.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_clean_nontagged_checkout_1()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v1.2.3", "4", "1a2b3c4", false, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("1", harvester.MajorVersion);
            Assert.AreEqual("2", harvester.MinorVersion);
            Assert.AreEqual("3", harvester.PatchVersion);
            Assert.AreEqual("4", harvester.RevisionVersion);
            Assert.AreEqual(String.Empty, harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a clean checkout of a derived version supplies
        ///     the correct version numbers, even with two-digit numbers.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_clean_nontagged_checkout_2()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v10.20.30", "15", "1a2b3c4", false, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("10", harvester.MajorVersion);
            Assert.AreEqual("20", harvester.MinorVersion);
            Assert.AreEqual("30", harvester.PatchVersion);
            Assert.AreEqual("15", harvester.RevisionVersion);
            Assert.AreEqual(String.Empty, harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a clean checkout of a derived version supplies
        ///     the correct version numbers, even with leading zeroes.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_clean_nontagged_checkout_3()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v01.02.03", "4", "1a2b3c4", false, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("1", harvester.MajorVersion);
            Assert.AreEqual("2", harvester.MinorVersion);
            Assert.AreEqual("3", harvester.PatchVersion);
            Assert.AreEqual("4", harvester.RevisionVersion);
            Assert.AreEqual(String.Empty, harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a clean checkout of a derived version supplies
        ///     the correct version numbers.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_clean_nontagged_checkout_4()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v2.1.0", "8", "1a2b3c4", false, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("2", harvester.MajorVersion);
            Assert.AreEqual("1", harvester.MinorVersion);
            Assert.AreEqual("0", harvester.PatchVersion);
            Assert.AreEqual("8", harvester.RevisionVersion);
            Assert.AreEqual(String.Empty, harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }
        #endregion

        #region Dirty, tagged checkout tests

        /// <summary>
        ///     Verifies a dirty checkout of a version supplies the correct
        ///     version numbers.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_dirty_tag_checkout_1()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v1.2.3", "0", "1a2b3c4", true, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("1", harvester.MajorVersion);
            Assert.AreEqual("2", harvester.MinorVersion);
            Assert.AreEqual("3", harvester.PatchVersion);
            Assert.AreEqual("0", harvester.RevisionVersion);
            Assert.AreEqual(" (Modified)", harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a dirty checkout of a version supplies the correct
        ///     version numbers, even with two-digit version components.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_dirty_tag_checkout_2()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v10.20.30", "0", "1a2b3c4", true, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("10", harvester.MajorVersion);
            Assert.AreEqual("20", harvester.MinorVersion);
            Assert.AreEqual("30", harvester.PatchVersion);
            Assert.AreEqual("0", harvester.RevisionVersion);
            Assert.AreEqual(" (Modified)", harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a dirty checkout of a version supplies the correct
        ///     version numbers, even if the tag has leading zeroes.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_dirty_tag_checkout_3()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v01.02.03", "0", "1a2b3c4", true, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("1", harvester.MajorVersion);
            Assert.AreEqual("2", harvester.MinorVersion);
            Assert.AreEqual("3", harvester.PatchVersion);
            Assert.AreEqual("0", harvester.RevisionVersion);
            Assert.AreEqual(" (Modified)", harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a dirty checkout of a version supplies the correct
        ///     version numbers, even if the patch component is zero.
        ///     Revision number is 0 when a specific version is checked out.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_dirty_tag_checkout_4()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v2.1.0", "0", "1a2b3c4", true, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("2", harvester.MajorVersion);
            Assert.AreEqual("1", harvester.MinorVersion);
            Assert.AreEqual("0", harvester.PatchVersion);
            Assert.AreEqual("0", harvester.RevisionVersion);
            Assert.AreEqual(" (Modified)", harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        #endregion

        #region Dirty, non-tagged checkout tests

        /// <summary>
        ///     Verifies a dirty checkout of a derived version supplies
        ///     the correct version numbers.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_dirty_nontagged_checkout_1()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v1.2.3", "4", "1a2b3c4", true, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("1", harvester.MajorVersion);
            Assert.AreEqual("2", harvester.MinorVersion);
            Assert.AreEqual("3", harvester.PatchVersion);
            Assert.AreEqual("4", harvester.RevisionVersion);
            Assert.AreEqual(" (Modified)", harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a dirty checkout of a derived version supplies
        ///     the correct version numbers, even with two-digit numbers.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_dirty_nontagged_checkout_2()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v10.20.30", "15", "1a2b3c4", true, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("10", harvester.MajorVersion);
            Assert.AreEqual("20", harvester.MinorVersion);
            Assert.AreEqual("30", harvester.PatchVersion);
            Assert.AreEqual("15", harvester.RevisionVersion);
            Assert.AreEqual(" (Modified)", harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a dirty checkout of a derived version supplies
        ///     the correct version numbers, even with leading zeroes.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_dirty_nontagged_checkout_3()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v01.02.03", "4", "1a2b3c4", true, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("1", harvester.MajorVersion);
            Assert.AreEqual("2", harvester.MinorVersion);
            Assert.AreEqual("3", harvester.PatchVersion);
            Assert.AreEqual("4", harvester.RevisionVersion);
            Assert.AreEqual(" (Modified)", harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        /// <summary>
        ///     Verifies a dirty checkout of a derived version supplies
        ///     the correct version numbers.
        /// </summary>
        [Test]
        public void Execute_sets_version_on_dirty_nontagged_checkout_4()
        {
            SemVerHgHarvester harvester;
            var returnValue = this.StandardExecute("v2.1.0", "8", "1a2b3c4", true, out harvester);

            Assert.AreEqual(true, returnValue);
            Assert.AreEqual("2", harvester.MajorVersion);
            Assert.AreEqual("1", harvester.MinorVersion);
            Assert.AreEqual("0", harvester.PatchVersion);
            Assert.AreEqual("8", harvester.RevisionVersion);
            Assert.AreEqual(" (Modified)", harvester.ModifiedString);
            Assert.AreEqual("1a2b3c4", harvester.CommitId);
        }

        #endregion

        #region Helper methods

        private HgLogRunner CreateMockLogRunner(string latestTag, string latestTagDistance, string node, bool dirty)
        {
            var mockRunner = new Mock<HgLogRunner>();
            mockRunner.SetupGet(r => r.LatestTag).Returns(latestTag);
            mockRunner.SetupGet(r => r.LatestTagDistance).Returns(latestTagDistance);
            mockRunner.SetupGet(r => r.Node).Returns(node);
            mockRunner.SetupGet(r => r.Dirty).Returns(dirty);
            return mockRunner.Object;
        }

        private IBuildEngine CreateMockBuildEngine()
        {
            var mockEngine = new Mock<IBuildEngine>();
            return mockEngine.Object;
        }

        private bool StandardExecute(string latestTag, string latestTagDistance, string node, bool dirty, out SemVerHgHarvester harvester)
        {
            var runner = this.CreateMockLogRunner(latestTag, latestTagDistance, node, dirty);

            harvester = new SemVerHgHarvester(runner);
            harvester.BuildEngine = this.CreateMockBuildEngine();
            harvester.HgPath = SemVerHgHarvesterTests.HgPath;
            return harvester.Execute();
        }

        #endregion
    }
}
