SemVerParser
====================

SemVerParser is a MSBuild task library that harvests version numbers from
tags in source control repositories.


## Instructions

Add the package from NuGet by running the following command in the Package
Manager Console:

    Install-Package SemVerParser

Next, add the build target to your project file:

    <Import Project="$(MSBuildProjectDirectory)\..\packages\SemVerParser.0.1\msbuild\SemVerParser.Targets" />


Then, add the following to your BeforeBuild target in your project file:

    <Target Name="BeforeBuild">
        <SemVerGitParser GitPath="C:\Program Files\Git\bin\git.exe">
            <Output TaskParameter="MajorVersion" PropertyName="MajorVersion" />
            <Output TaskParameter="MinorVersion" PropertyName="MinorVersion" />
            <Output TaskParameter="PatchVersion" PropertyName="PatchVersion" />
            <Output TaskParameter="RevisionVersion" PropertyName="RevisionVersion" />
            <Output TaskParameter="ModifiedString" PropertyName="ModifiedString" />
        </SemVerGitParser>
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

    git-describe                     Major  Minor  Patch  Revision  ModifiedString
    --------------------------       ------ ------ ------ --------- ---------------
    v1.2.3-0-g1ab2cd3           ==>  "1"    "2"    "3"    "0"       ""
    v1.2.3-4-g1ab2cd3           ==>  "1"    "2"    "3"    "4"       ""
    v1.2.3-0-g1ab2cd3-modified  ==>  "1"    "2"    "3"    "0"       " (Modified)"
    v1.2.3-4-g1ab2cd3-modified  ==>  "1"    "2"    "3"    "4"       " (Modified)"
    1ab2cd3                     ==>  "0"    "0"    "0"    "0"       ""
    1ab2cd3-modified            ==>  "0"    "0"    "0"    "0"       " (Modified)"


## Source Code

SemVerParser is on GitHub:

    http://github.com/jennings/SemVerParser


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
