using System;
using System.Collections.Generic;
using UnityEditor;

static class BuildCommand
{
    static string GetArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(name))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    static string[] GetEnabledScenes()
    {
        // Get enabled scenes.
        List<string> enabledScenesList = new List<string>();
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            if (EditorBuildSettings.scenes[i].enabled)
            {
                enabledScenesList.Add(EditorBuildSettings.scenes[i].path);
            }
        }

        // Transform enabled scenes list into an array to be returned.
        string[] enabledScenesArray = new string[enabledScenesList.Count];
        for (int i = 0; i < enabledScenesArray.Length; i++)
        {
            enabledScenesArray[i] = enabledScenesList[i];
        }

        return enabledScenesArray;
    }

    static BuildTarget GetBuildTarget()
    {
        string buildTargetName = GetArgument("customBuildTarget");
        Console.WriteLine(":: Received customBuildTarget " + buildTargetName);

        return ToEnum<BuildTarget>(buildTargetName, BuildTarget.NoTarget);
    }

    static string GetBuildPath()
    {
        string buildPath = GetArgument("customBuildPath");
        Console.WriteLine(":: Received customBuildPath " + buildPath);

        if (buildPath == "")
        {
            throw new Exception("customBuildPath argument is missing");
        }

        return buildPath;
    }

    static string GetBuildName()
    {
        string buildName = GetArgument("customBuildName");
        Console.WriteLine(":: Received customBuildName " + buildName);

        if (buildName == "")
        {
            throw new Exception("customBuildName argument is missing");
        }

        return buildName;
    }

    static string GetFixedBuildPath(BuildTarget buildTarget, string buildPath, string buildName)
    {
        if (buildTarget.ToString().ToLower().Contains("windows"))
        {
            buildName = buildName + ".exe";
        }
        else if (buildTarget.ToString().ToLower().Contains("webgl"))
        {
            // webgl produces a folder with index.html inside, there is no executable name for this buildTarget
            buildName = "";
        }

        return buildPath + buildName;
    }

    static BuildOptions GetBuildOptions()
    {
        string buildOptions = GetArgument("customBuildOptions");

        return buildOptions == "AcceptExternalModificationsToPlayer"
            ? BuildOptions.AcceptExternalModificationsToPlayer
            : BuildOptions.None;
    }

    // https://stackoverflow.com/questions/1082532/how-to-tryparse-for-enum-value
    static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
    {
        if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
        {
            return defaultValue;
        }

        return (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
    }

    static string getEnv(string key, bool secret = false, bool verbose = true)
    {
        var env_var = Environment.GetEnvironmentVariable(key);

        if (verbose)
        {
            if (env_var != null)
            {
                if (secret)
                {
                    Console.WriteLine(":: env['" + key + "'] set");
                }
                else
                {
                    Console.WriteLine(":: env['" + key + "'] set to '" + env_var + "'");
                }
            }
            else
            {
                Console.WriteLine(":: env['" + key + "'] is null");
            }
        }

        return env_var;
    }

    static void PerformBuild()
    {
        Console.WriteLine(":: Performing build");

        var buildTarget = GetBuildTarget();
        var buildPath = GetBuildPath();
        var buildName = GetBuildName();
        var fixedBuildPath = GetFixedBuildPath(buildTarget, buildPath, buildName);

        BuildPipeline.BuildPlayer(GetEnabledScenes(), fixedBuildPath, buildTarget, GetBuildOptions());
        Console.WriteLine(":: Build complete");
        Console.WriteLine(":: Applied Retina fix");
        Console.WriteLine(":: Done with build process");
    }
}