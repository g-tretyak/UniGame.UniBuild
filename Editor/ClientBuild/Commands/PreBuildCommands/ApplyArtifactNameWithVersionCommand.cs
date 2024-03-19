﻿namespace UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands
{
    using System;
    using System.IO;
    using GitTools.Runtime;
    using global::UniGame.UniBuild.Editor.ClientBuild.Interfaces;
    using Interfaces;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class ApplyArtifactNameWithVersionCommand : UnitySerializablePreBuildCommand
    {
        private const string nameFormatTemplate = "{0}-{1}";

        public bool useProductName = true;
        public bool includeGitBranch;
        public bool includeBundleVersion = true;
        public bool useNameTemplate = false;

        public string artifactNameTemplate = string.Empty;

        [Header("Optional: Extension: use '.' before file extension")]
        public string artifactExtension = "";

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            var outputFilename = buildParameters.BuildParameters.outputFile;
            var artifactName = CreateArtifactLocation(outputFilename, PlayerSettings.productName);
            buildParameters.BuildParameters.outputFile = artifactName;
        }

        public string CreateArtifactLocation(string outputFilename, string productName)
        {
            var outputExtension = string.IsNullOrEmpty(artifactExtension)
                    ? GetExtension()
                    : artifactExtension;

            var fileName = Path.GetFileNameWithoutExtension(outputFilename);

            var artifactName = useProductName ? productName : fileName;

            if (useNameTemplate)
            {
                artifactName = string.Format(artifactNameTemplate, artifactName);
            }

            if (includeGitBranch)
            {
                var branch = GitCommands.GetGitBranch();
                if (string.IsNullOrEmpty(branch) == false)
                {
                    artifactName = string.Format(nameFormatTemplate, artifactName, branch);
                }
            }

            if (includeBundleVersion)
            {
                artifactName = string.Format(nameFormatTemplate, artifactName, PlayerSettings.bundleVersion);
            }

            artifactName = artifactName.Replace(":", "");
            artifactName += $"{outputExtension}";

            return artifactName;
        }

        private string GetExtension()
        {
            var extension = string.Empty;

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                    return extension;
                case BuildTarget.Android:
                    if (EditorUserBuildSettings.exportAsGoogleAndroidProject)
                        return string.Empty;
                    var isAppBundle = EditorUserBuildSettings.buildAppBundle;
                    extension = isAppBundle ? ".aab" : ".apk";
                    break;
                default:
                    return extension;
            }

            return extension;
        }
    }
}