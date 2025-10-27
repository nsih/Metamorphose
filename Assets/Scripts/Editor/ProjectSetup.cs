using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

using static System.Environment;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;

public static class ProjectSetup {
    [MenuItem("Tools/Setup/유니티 에셋 임포트")]
    public static void ImportEssentials() {        
        // Saints Field 인스펙터 확장 툴 
        Assets.ImportAsset("Saints Field.unitypackage", "TylerTemp/Editor ExtensionsUtilities");
        // FMOD 지원
        Assets.ImportAsset("FMOD for Unity 202.unitypackage", "FMOD/Editor ExtensionsAudio");
        // Animancer 코드로 애니메이션 제어
        Assets.ImportAsset("Animancer Pro v8.unitypackage", "Kybernetik/ScriptingAnimation");
        // Hierarchy Viewer 계층 구조 확장
        Assets.ImportAsset("Better Hierarchy.unitypackage", "Toaster Head/Editor ExtensionsUtilities");

        Debug.Log("유니티 에셋 임포트 완료");
    }

    [MenuItem("Tools/Setup/패키지 설치")]
    public static void InstallPackages() {
        Packages.InstallPackages(new[] {
            // Unity Util 함수들
            "git+https://github.com/adammyhre/Unity-Utils.git",
            // Unity 타이머 클래스
            "git+https://github.com/adammyhre/Unity-Improved-Timers.git",
            // Unity Cursor 지원
            "git+https://github.com/boxqkrtm/com.unity.ide.cursor.git",
            // NuGetForUnity 패키지
            "git+https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity",
            // LitMotion (Tween 패키지)
            "git+https://github.com/annulusgames/LitMotion.git?path=src/LitMotion/Assets/LitMotion",
        });
        
        Debug.Log("패키지 설치 완료");
    }

    [MenuItem("Tools/Setup/R3 설치(먼저 NuGet에서 설치 필요)")]
    public static void InstallR3Package() 
    {
        Packages.InstallPackages(new[] {
            // R3 패키지
            "git+https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity",
        });

        Debug.Log("R3 패키지 설치 완료");
    }

    [MenuItem("Tools/Setup/폴더 구조 템플릿 생성")]
    public static void CreateFolders() {
        Folders.Create("_Core", "Animation", "Art", "Materials", "Prefabs", "Audio", "Scripts/Editor", "Scripts/Runtime");
        Refresh();
        Folders.Move("_Core", "Scenes");
        Folders.Move("_Core", "Settings");
        Folders.Delete("TutorialInfo");
        Refresh();

        MoveAsset("Assets/InputSystem_Actions.inputactions", "Assets/_Core/Settings/InputSystem_Actions.inputactions");
        DeleteAsset("Assets/Readme.asset");
        Refresh();

        Debug.Log("폴더 구조 템플릿 생성 완료");
    }

    static class Assets {
        public static void ImportAsset(string asset, string folder) {
            string basePath;
            if (OSVersion.Platform is PlatformID.MacOSX or PlatformID.Unix) {
                string homeDirectory = GetFolderPath(SpecialFolder.Personal);
                basePath = Combine(homeDirectory, "Library/Unity/Asset Store-5.x");
            } else {
                string defaultPath = Combine(GetFolderPath(SpecialFolder.ApplicationData), "Unity");
                basePath = Combine(EditorPrefs.GetString("AssetStoreCacheRootPath", defaultPath), "Asset Store-5.x");
            }

            asset = asset.EndsWith(".unitypackage") ? asset : asset + ".unitypackage";

            string fullPath = Combine(basePath, folder, asset);

            if (!File.Exists(fullPath)) {
                throw new FileNotFoundException($"The asset package was not found at the path: {fullPath}");
            }

            ImportPackage(fullPath, false);
        }
    }

    static class Packages {
        static AddRequest request;
        static Queue<string> packagesToInstall = new Queue<string>();

        public static void InstallPackages(string[] packages) {
            foreach (var package in packages) {
                packagesToInstall.Enqueue(package);
            }

            if (packagesToInstall.Count > 0) {
                StartNextPackageInstallation();
            }
        }

        static async void StartNextPackageInstallation() {
            request = Client.Add(packagesToInstall.Dequeue());
            
            while (!request.IsCompleted) await Task.Delay(10);
            
            if (request.Status == StatusCode.Success) Debug.Log("Installed: " + request.Result.packageId);
            else if (request.Status >= StatusCode.Failure) Debug.LogError(request.Error.message);

            if (packagesToInstall.Count > 0) {
                await Task.Delay(1000);
                StartNextPackageInstallation();
            }
        }
    }

    static class Folders {
        public static void Create(string root, params string[] folders) {
            var fullpath = Combine(Application.dataPath, root);
            if (!Directory.Exists(fullpath)) {
                Directory.CreateDirectory(fullpath);
            }

            foreach (var folder in folders) {
                CreateSubFolders(fullpath, folder);
            }
        }
        
        static void CreateSubFolders(string rootPath, string folderHierarchy) {
            var folders = folderHierarchy.Split('/');
            var currentPath = rootPath;

            foreach (var folder in folders) {
                currentPath = Combine(currentPath, folder);
                if (!Directory.Exists(currentPath)) {
                    Directory.CreateDirectory(currentPath);
                }
            }
        }
        
        public static void Move(string newParent, string folderName) {
            var sourcePath = $"Assets/{folderName}";
            if (IsValidFolder(sourcePath)) {
                var destinationPath = $"Assets/{newParent}/{folderName}";
                var error = MoveAsset(sourcePath, destinationPath);

                if (!string.IsNullOrEmpty(error)) {
                    Debug.LogError($"Failed to move {folderName}: {error}");
                }
            }
        }
        
        public static void Delete(string folderName) {
            var pathToDelete = $"Assets/{folderName}";

            if (IsValidFolder(pathToDelete)) {
                DeleteAsset(pathToDelete);
            }
        }
    }
}