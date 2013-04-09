SemVerHarvester
====================

SemVerHarvester is a MSBuild task library that harvests version numbers from
tags in source control repositories.  

SemVerHarvester expects version tags in the format "v1.1.1" It parces the numeric 
triple (1.1.1) into major version, minor version, and patch version.  The number of 
commits between the last version tag and HEAD for the current branch is returned in
revision version, and if untracked changes are found, a modified property is set to
 " (Modified)".

## Instructions

Add the package from NuGet by running the following command in the Package
Manager Console:

    Install-Package SemVerHarvester

Next, add the build target to your project file:

    <Import Project="$(MSBuildProjectDirectory)\..\packages\SemVerHarvester.0.3.0\msbuild\SemVerHarvester.Targets" />


Then, add the following to your BeforeBuild target in your project file:

    <Target Name="BeforeBuild">
        <SemVerGitHarvester GitPath="C:\Program Files\Git\bin\git.exe">
            <Output TaskParameter="MajorVersion" PropertyName="MajorVersion" />
            <Output TaskParameter="MinorVersion" PropertyName="MinorVersion" />
            <Output TaskParameter="PatchVersion" PropertyName="PatchVersion" />
            <Output TaskParameter="RevisionVersion" PropertyName="RevisionVersion" />
            <Output TaskParameter="ModifiedString" PropertyName="ModifiedString" />
        </SemVerGitHarvester>
    </Target>

You will then have $(MajorVersion), $(MinorVersion), etc., available to use in your
project file. If you're using MSBuildCommunityTasks, you can use the AssemblyFile
task to generate assembly information:

    <AssemblyInfo OutputFile="Properties\VersionAssemblyInfo.cs"
                  CodeLanguage="CS"
                  AssemblyVersion="$(MajorVersion).$(MinorVersion).$(PatchVersion).$(RevisionVersion)"
                  AssemblyFileVersion="$(MajorVersion).$(MinorVersion).$(PatchVersion).$(RevisionVersion)"
                  AssemblyInformationalVersion="$(MajorVersion).$(MinorVersion).$(PatchVersion).$(RevisionVersion)$(ModifiedString)" />


The output of git-describe will be transformed as follows:

    git-describe                     Major  Minor  Patch  Revision  ModifiedString    Explanation
    --------------------------       ------ ------ ------ --------- ---------------   -----------------------------
    v1.2.3-0-g1ab2cd3           ==>  "1"    "2"    "3"    "0"       ""                tag="v1.2.3" found, no commits after tag
    v1.2.3-4-g1ab2cd3           ==>  "1"    "2"    "3"    "4"       ""                tag="v1.2.3" found, 4 commits after tag
    v1.2.3-0-g1ab2cd3-modified  ==>  "1"    "2"    "3"    "0"       " (Modified)"     tag="v1.2.3" found, no commits after tag, untracked changes found
    v1.2.3-4-g1ab2cd3-modified  ==>  "1"    "2"    "3"    "4"       " (Modified)"     tag="v1.2.3" found, 4 commits after tag, untracked changes found
    1ab2cd3                     ==>  "0"    "0"    "0"    "0"       ""                no semVer tag found
    1ab2cd3-modified            ==>  "0"    "0"    "0"    "0"       " (Modified)"     no semVer tag found, untracked changes found


## Source Code

SemVerHarvester is on GitHub:

    http://github.com/jennings/SemVerHarvester


## License

Copyright 2011 Stephen Jennings

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
