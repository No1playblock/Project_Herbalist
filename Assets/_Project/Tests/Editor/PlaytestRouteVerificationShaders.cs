using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEngine;

public sealed class PlaytestRouteVerificationShaders : IPreprocessShaders, IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    private static bool _enabled;
    public int callbackOrder => int.MaxValue;

    public void OnPreprocessBuild(BuildReport report) =>
        _enabled = report.summary.outputPath.Replace('\\', '/').Contains("Builds/PlaytestRoute_20261112/");

    public void OnPostprocessBuild(BuildReport report) => _enabled = false;

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> variants)
    {
        if (!_enabled) return;
        var platforms = new HashSet<int>();
        for (int i = variants.Count - 1; i >= 0; i--)
            if (!platforms.Add((int)variants[i].shaderCompilerPlatform))
                variants.RemoveAt(i);
    }
}
