#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

// MLWorkshopTroubleshooter v0.03
// for MLTK-Brain Online Workshop
// naschenbach@magicleap.com

// todo
// use try catch rather than returning Rule.Result
// links to resources and longer explinations for fixes 
// host this in the package manager?
// import Unity Template feature?
// Zero Iteration support (Check for MLRemote -> import support libraries( when using Zero Iteration)

namespace MagicLeap.Troubleshooter
{
    public class Rules
    {
        static class ExpectedVersion
        {
            public static string UnityVersion = "2019.3";
            public static string LuminSdk = "0.24.1";
        }

        public Rule.Result UnityVer()
        {
            if (!Application.unityVersion.StartsWith(ExpectedVersion.UnityVersion))
                return Rule.Fail($"{Application.unityVersion} ({ExpectedVersion.UnityVersion} is required)");
            return Rule.Pass(Application.unityVersion);
        }

        public Rule.Result BuildTarget()
        {
            var activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            if (activeBuildTarget != UnityEditor.BuildTarget.Lumin)
                return Rule.Fail(
                    $"is currently {activeBuildTarget} ({UnityEditor.BuildTarget.Lumin} is required)");
            return Rule.Pass(activeBuildTarget.ToString());
        }

        public Rule.Result CertPath()
        {
            if (!PlayerSettings.Lumin.signPackage)
                return Rule.Fail("Publishing Settings -> Sign Package must be enabled");

            var certPath = PlayerSettings.Lumin.certificatePath;
            if (string.IsNullOrEmpty(certPath))
                return Rule.Fail(
                    $"ML Certificate not set in BuildSettings -> PlayerSettings -> Project -> Player -> PublishingSettings");
            if (!File.Exists(certPath))
                return Rule.Fail($".cert file Not Found: ({certPath})");

            var creationTime = File.GetCreationTime(certPath);
            if (creationTime < new DateTime(2020, 3, 19))
            {
                return Rule.Fail(
                    $".cert file was created on {creationTime.ToShortDateString()} and needs to be regenerated");
            }

            var privkeyPath = Path.ChangeExtension(certPath, "privkey");
            if (!File.Exists(privkeyPath)) return Rule.Fail($"Missing privkey file (should be {privkeyPath})");

            return Rule.Pass($".cert and .privkey appear valid");
        }

        public Rule.Result LuminSdk()
        {
            var luminSdkRoot = Troubleshooter.LuminSdkRoot;
            if (luminSdkRoot == null || !Directory.Exists(luminSdkRoot))
                return Rule.Fail($"sdk not found ({luminSdkRoot})");
            if (!luminSdkRoot.EndsWith(ExpectedVersion.LuminSdk))
                return Rule.Fail(
                    $"sdk is set to {Path.GetFileName(luminSdkRoot.TrimEnd('/').TrimEnd('\\'))} (should be {ExpectedVersion.LuminSdk})");
            return Rule.Pass(luminSdkRoot);
        }

        public Rule.Result DeviceOs()
        {
            var (output, err) = Troubleshooter.MldbCall("getprop ro.ml.build.version.release");
            if (err != string.Empty)
                return Rule.Fail($"{err} - is your ML1 active and connected?");
            return output == "0.98.10"
                ? Rule.Pass(output)
                : Rule.Fail($"{output} - please upgrade to 0.98.10");
        }

        public Rule.Result DeviceSn()
        {
            var (output, err) = Troubleshooter.MldbCall("getprop ro.serialno");
            return err != string.Empty
                ? Rule.Fail($"{err} - is your ML1 active and connected?")
                : Rule.Pass(output);
        }

        public Rule.Result ActiveScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path == string.Empty)
                return Rule.Fail($"Currently active scene must be saved");

            EditorBuildSettingsScene firstEnabledScene = EditorBuildSettings.scenes.FirstOrDefault(s => s.enabled);
            if (firstEnabledScene != null && activeScene.path.Equals(firstEnabledScene.path))
                return Rule.Pass(firstEnabledScene.path);

            return Rule.Fail($"{activeScene.path} is not enabled as the first scene in BuildSettings");
        }

        public Rule.Result MltkExists()
        {
            // Check by namespace?
            var spotCheckMltkPrefab = "Assets/MagicLeap-Tools/Prefabs/Input/ControlPointer.prefab";
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(spotCheckMltkPrefab);
            return asset ? Rule.Pass("MLTK assets are available") : Rule.Fail("MLTK assets not found");
        }
    }

    [InitializeOnLoad]
    static class Launcher
    {
        static Launcher()
        {
            AssemblyReloadEvents.afterAssemblyReload += RefreshResults;
            EditorBuildSettings.sceneListChanged += RefreshResults;
            EditorSceneManager.activeSceneChangedInEditMode += (prevScene, newScene) => { RefreshResults(); };
        }

        [MenuItem("Magic Leap/Troubleshoot Project", priority = 5000)]
        public static void MenuItem()
        {
            Window.Hidden = false;
            RefreshResults();
        }

        private static double _lastRefreshTime = double.MinValue;
        private static void RefreshResults()
        {
            if (Window.Hidden || EditorApplication.timeSinceStartup < _lastRefreshTime + 0.5f) return;
            _lastRefreshTime = EditorApplication.timeSinceStartup;
            Troubleshooter.Validate();
            Type[] desiredDockNextTo = new Type[]
            {
                // typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow")
            };
            bool focus = false;
            Window window = EditorWindow.GetWindow<Window>(Window.TITLE, focus, desiredDockNextTo);
            window.Show();
        }
    }


    public class Window : EditorWindow
    {
        public const string TITLE = "Magic Leap Troubleshooter";
        private readonly GUIStyle _guiStyle = new GUIStyle();
        private Vector2 _scroll;

        public static bool Hidden
        {
            get => EditorPrefs.HasKey(TITLE) && EditorPrefs.GetBool(TITLE);
            set => EditorPrefs.SetBool(TITLE, value);
        }

        void OnGUI()
        {
            GUI.skin.scrollView.padding = new RectOffset(10, 10, 10, 10);
            _guiStyle.wordWrap = true;
            _guiStyle.richText = true;
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            GUILayout.Label(Troubleshooter.LastResult.GetGuiText(), _guiStyle);
            
            Troubleshooter.EnableLog = GUILayout.Toggle(Troubleshooter.EnableLog, "Log Results");
            
            if (GUILayout.Button("Rerun Validation"))
            {
                Troubleshooter.Validate();
            }
            if (GUILayout.Button("Copy Results to Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = Troubleshooter.LastResult.GetPlainText();
            }

            if (GUILayout.Button("Hide this Window"))
            {
                Hidden = true;
                Close();
            }

            EditorGUILayout.EndScrollView();
        }

        // void OnDestroy()
        // {
        //     Debug.Log("Disabling Window, launcher is quitting: " + Launcher.IsQuitting);
        //     Hidden = true;
        // }
    }

    public static class Troubleshooter
    {
        // UnityEditor.XR.MagicLeap.SDKUtility
        public static string LuminSdkRoot => EditorPrefs.GetString("LuminSDKRoot", null);

        public static (string, string) MldbCall(string args)
        {
            var mldbRoot = Path.Combine(LuminSdkRoot, "tools/mldb");
            if (!Directory.Exists(mldbRoot)) return ("", $"Missing mldb at: {mldbRoot}");
            var mldbFileName = Path.Combine(mldbRoot, "mldb");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = mldbFileName,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            string err = process.StandardError.ReadToEnd().Trim();
            process.WaitForExit();
            return (output, err);
        }

        private static ValidationResult _lastResult;

        public static ValidationResult LastResult
        {
            get
            {
                if (_lastResult == null) Validate();
                return _lastResult;
            }
        }

        private const string MLTroubleshooterEnableLog = "MLTroubleshooterEnableLog";
        public static bool EnableLog
        {
            get => EditorPrefs.HasKey(MLTroubleshooterEnableLog) && EditorPrefs.GetBool(MLTroubleshooterEnableLog);
            set => EditorPrefs.SetBool(MLTroubleshooterEnableLog, value);
        }


        public static void Validate()
        {
            var troubleshooterRules = new Rules();
            var rulesType = troubleshooterRules.GetType();
            var methodInfos = rulesType.GetMethods().Where(mi => mi.ReturnType == typeof(Rule.Result)).ToList();
            var results = methodInfos
                .Select(mi =>
                {
                    var ruleResult = mi.Invoke(troubleshooterRules, new object[] { }) as Rule.Result;
                    ruleResult.Name = mi.Name;
                    return ruleResult;
                }).ToList();
            _lastResult = new ValidationResult(results);
            if (EnableLog) Debug.Log(_lastResult.GetGuiText("See <i>Magic Leap -> Troubleshoot Project</i> for more details."));
        }
    }


    public class ValidationResult
    {
        private readonly DateTime _time = DateTime.Now;
        private readonly List<Rule.Result> _ruleResults;
        private readonly List<Rule.Result> _failures;

        public ValidationResult(List<Rule.Result> ruleResults)
        {
            _ruleResults = ruleResults;
            _failures = _ruleResults.Where(r => !r.Valid).ToList();
        }

        public string GetPlainText()
        {
            var formattedResults =
                _ruleResults.Select(r => $"{(r.Valid ? "Pass" : "Fail")} - {r.Name}:\t{r.Message}");
            return string.Join("\n", formattedResults.Prepend(_time.ToLongTimeString()));
        }

        public string GetGuiText(string info="")
        {
            var failing = _failures.Any();
            var color = failing ? $"red" : "green";
            var timeString = _time.ToLongTimeString();
            var header =
                $"<b><color={color}>{_failures.Count()} Issues in Magic Leap configuration</color></b> ({timeString} UTC)\n{info}\n";
            StringBuilder sb = new StringBuilder(header);
            if (failing)
            {
                sb.AppendLine(
                    string.Join("\n", _failures.Select(r => $"{r.Name}:\t<color=red>{r.Message}</color>")));
            }

            var valid = _ruleResults.Where(r => r.Valid);
            sb.AppendLine(string.Join("\n", valid.Select(r => $"{r.Name}:\t<color=green>{r.Message}</color>")));
            return sb.ToString();
        }
    }

    public static class Rule
    {
        public static Result Pass(string message)
        {
            return new Result(true, message);
        }

        public static Result Fail(string message)
        {
            return new Result(false, message);
        }

        public class Result
        {
            public readonly bool Valid;
            public readonly string Message;
            public string Name = "";

            public Result(bool valid, string message)
            {
                Valid = valid;
                Message = message;
            }
        }
    }
}
#endif