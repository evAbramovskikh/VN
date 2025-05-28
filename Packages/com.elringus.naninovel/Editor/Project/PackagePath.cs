using System.IO;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Provides paths to various package-related directories and resources; all paths are relative to project root.
    /// </summary>
    public static class PackagePath
    {
        public static string PackageRootPath => GetPackageRootPath();
        public static string PackageMarkerPath => Path.Combine(cachedPackageRootPath, "Editor", marker);
        public static string EditorResourcesPath => Path.Combine(PackageRootPath, "Editor/Resources/Naninovel");
        public static string RuntimeResourcesPath => Path.Combine(PackageRootPath, "Resources/Naninovel");
        public static string PrefabsPath => Path.Combine(PackageRootPath, "Prefabs");
        public static string GeneratedDataPath => GetGeneratedDataPath();

        private const string marker = "Elringus.Naninovel.Editor.asmdef";
        private static string cachedPackageRootPath;

        private static string GetPackageRootPath ()
        {
            if (string.IsNullOrEmpty(cachedPackageRootPath) || !File.Exists(PackageMarkerPath))
                cachedPackageRootPath = FindRootInPackages() ?? FindRootInAssets();
            return cachedPackageRootPath ?? throw new Error("Failed to locate Naninovel package directory.");
        }

        private static string FindRootInPackages ()
        {
            // Even when package is installed as immutable (eg, local or git) and only physically
            // exists under Library/PackageCache/…, Unity will still symlink it to Packages/….
            const string packageDir = "Packages/com.elringus.naninovel";
            return Directory.Exists(packageDir) ? packageDir : null;
        }

        private static string FindRootInAssets ()
        {
            foreach (var path in Directory.EnumerateFiles(Application.dataPath, marker, SearchOption.AllDirectories))
                return PathUtils.AbsoluteToAssetPath(Directory.GetParent(path)?.Parent?.FullName ?? "");
            return null;
        }

        private static string GetGeneratedDataPath ()
        {
            var localPath = Configuration.GetOrDefault<EngineConfiguration>().GeneratedDataPath;
            var assetPath = PathUtils.Combine("Assets", localPath);
            if (!Directory.Exists(assetPath)) Directory.CreateDirectory(assetPath);
            return assetPath;
        }
    }
}
