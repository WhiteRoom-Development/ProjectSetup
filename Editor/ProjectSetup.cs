using System.IO;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

using static System.IO.Path;
using static UnityEditor.AssetDatabase;

namespace WhiteRoom.ProjectSetup
{
    public static class ProjectSetup
    {
        [MenuItem("WhiteRoom/Setup/Import Packages/Extenject", priority = 4)]
        public static void ImportExtenject()
        {
            Assets.ImportCustomAssets("Extenject.unitypackage", @"C:\Unity Packages");
        }

        [MenuItem("WhiteRoom/Setup/Import Packages/Hot Reload", priority = 5)]
        public static void ImportHotReload()
        {
            Assets.ImportCustomAssets("Hot Reload Edit Code Without Compiling v1.12.8.unitypackage", @"C:\Unity Packages\Editor");
        }
        
        
        [MenuItem("WhiteRoom/Setup/Import Essential Assets", priority = 1)]
        public static void ImportEssentials()
        {
            Assets.ImportCustomAssets("Color Studio v4.1.1.unitypackage", @"C:\Unity Packages\Editor");
            Assets.ImportCustomAssets("vFavorites 2 v2.0.7.unitypackage", @"C:\Unity Packages\Editor");
            
            Assets.ImportAssets("Editor Auto Save.unitypackage", "IntenseNation/Editor ExtensionsUtilities");
            Assets.ImportAssets("Better Transform - Size Notes Global-Local workspace child parent transform.unitypackage", "Tiny Giant Studio/Editor ExtensionsUtilities");
            Assets.ImportAssets("Audio Preview Tool.unitypackage", "Warped Imagination/Editor ExtensionsAudio");
            Assets.ImportAssets("Editor Console Pro.unitypackage", "FlyingWorm/Editor ExtensionsSystem");
            Assets.ImportAssets("Graphy - Ultimate FPS Counter - Stats Monitor Debugger.unitypackage", "Tayx/ScriptingGUI");
            
            Refresh();
        }

        [MenuItem("WhiteRoom/Setup/Reorganize Essential Asset folders", priority = 2)]
        public static void ReorganizeEssentialAssets()
        {
            Folders.Move("Packs", "Graphy - Ultimate Stats Monitor");
            Folders.Move("Packs", "IntenseNation");
            Folders.Move("Packs", "Color Studio");
            
            Folders.Move("Plugins", "WarpedImagination");
            Folders.Move("Plugins", "vFavorites");
            Folders.Move("Plugins", "ConsolePro");
            
            Folders.Delete("Tiny Giant Studio");
            
            Refresh();
        }

        [MenuItem("WhiteRoom/Setup/Install Essential Packages", priority = 3)]
        public static void InstallPackages()
        {
            Packages.Installation.InstallPackages(new[]
            {
                "com.unity.editorcoroutines",
                "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
                "https://github.com/AnnulusGames/LitMotion.git?path=src/LitMotion/Assets/LitMotion",
                "https://github.com/WhiteRoom-Development/Utilities.git"
            });
        }

        [MenuItem("WhiteRoom/Setup/Remove Unnecessary Packages", priority = 6)]
        public static void RemoveUnnecessaryPackages()
        {
            Packages.Removal.RemovePackages(new[]
            {
                "com.unity.visualscripting",
                "com.unity.ide.visualstudio",
                "com.unity.modules.tilemap",
                "com.unity.modules.vehicles",
                "com.unity.modules.wind",
                "com.unity.collab-proxy"
            });
        }

        [MenuItem("WhiteRoom/Setup/Create Folders", priority = 0)]
        public static void CreateFolders()
        {
            Folders.Create("_Project",
                "ART/Sprites",
                "ART/Materials",
                "ART/Models",
                "ART/Shaders",
                "ART/Textures",
                "ART/Visual Effects",
                "Animation/Clips",
                "Animation/Controllers",
                "Animation/Controllers/Overrides",
                "Scripts/Runtime",
                "Scripts/Editor",
                "Prefabs",
                "Resources",
                "Sounds",
                "Presents",
                "Fonts",
                "Inputs"
                );

            Folders.Create("Packs");
            Folders.Create("Plugins");
            
            Refresh();
            
            Folders.Move("_Project", "Scenes");
            Folders.Move("_Project", "Settings");
            
            Folders.Delete("TutorialInfo");

            MoveAsset("Assets/InputSystem_Actions.inputactions", "Assets/_Project/Inputs/InputSystem_Actions.inputactions");
            DeleteAsset("Assets/Readme.asset");
            
            Refresh();
        }

        private static class Assets
        {
            public static void ImportAssets(string asset, string folder)
            {
                var basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                var assetsFolder = Combine(basePath, "Unity/Asset Store-5.x");
                ImportPackage(Combine(assetsFolder, folder, asset), false);
            }

            public static void ImportCustomAssets(string asset, string folder)
            {
                ImportPackage(Combine(folder, asset), false);
            }
        }

        private static class Packages
        {
            public static class Removal
            {
                private static RemoveRequest _request;
                private static readonly Queue<string> _packagesToRemove = new();

                private static async void StartNextPackageRemoval()
                {
                    _request = Client.Remove(_packagesToRemove.Dequeue());

                    while (!_request.IsCompleted) await Task.Delay(10);

                    if (_request.Status == StatusCode.Success) Debug.Log($"Removed: {_request.PackageIdOrName}");
                    else if (_request.Status == StatusCode.Failure)
                        Debug.LogWarning($"Failed to Removal: {_request.Error.message}");

                    if (_packagesToRemove.Count > 0)
                    {
                        await Task.Delay(1000);
                        StartNextPackageRemoval();
                    }
                }

                public static void RemovePackages(string[] packages)
                {
                    foreach (var package in packages)
                    {
                        _packagesToRemove.Enqueue(package);
                    }

                    if (_packagesToRemove.Count > 0)
                        StartNextPackageRemoval();
                }
            }

            public static class Installation
            {
                private static AddRequest _request;
                private static readonly Queue<string> _packagesToInstall = new();

                private static async void StartNextPackageInstallation()
                {
                    _request = Client.Add(_packagesToInstall.Dequeue());

                    while (!_request.IsCompleted) await Task.Delay(10);

                    if (_request.Status == StatusCode.Success) Debug.Log($"Installed: {_request.Result.packageId}");
                    else if (_request.Status == StatusCode.Failure)
                        Debug.LogWarning($"Failed to Install: {_request.Error.message}");

                    if (_packagesToInstall.Count > 0)
                    {
                        await Task.Delay(1000);
                        StartNextPackageInstallation();
                    }
                }

                public static void InstallPackages(string[] packages)
                {
                    foreach (var package in packages)
                    {
                        _packagesToInstall.Enqueue(package);
                    }

                    if (_packagesToInstall.Count > 0)
                        StartNextPackageInstallation();
                }
            }
        }

        private static class Folders
        {
            public static void Delete(string folderName)
            {
                var pathToDelete = $"Assets/{folderName}";

                if (IsValidFolder(pathToDelete))
                    DeleteAsset(pathToDelete);
            }

            public static void Move(string newParent, string folderName)
            {
                var sourcePath = $"Assets/{folderName}";
                if (IsValidFolder(sourcePath))
                {
                    var destinationPath = $"Assets/{newParent}/{folderName}";
                    var error = MoveAsset(sourcePath, destinationPath);

                    if (!string.IsNullOrEmpty(error))
                        Debug.LogError($"Failed to Move: {folderName}: {error}");
                }
            }

            public static void Create(string root, params string[] folders)
            {
                var fullPath = Combine(Application.dataPath, root);
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                foreach (var folder in folders)
                {
                    CreateSubFolders(fullPath, folder);
                }
            }

            private static void CreateSubFolders(string rootPath, string folderHierarchy)
            {
                var folders = folderHierarchy.Split('/');
                var currentPath = rootPath;

                foreach (var folder in folders)
                {
                    currentPath = Combine(currentPath, folder);
                    if (!Directory.Exists(currentPath))
                        Directory.CreateDirectory(currentPath);
                }
            }
        }
    }
}