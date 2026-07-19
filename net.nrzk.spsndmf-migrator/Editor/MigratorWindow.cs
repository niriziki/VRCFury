using System;
using UnityEditor;
using UnityEngine;
using Nrzk.SpsMigrator.Convert;
using Nrzk.SpsMigrator.Reflection;

namespace Nrzk.SpsMigrator {
    internal sealed class MigratorWindow : EditorWindow {
        private enum Direction { VrcfToSpsNdmf, SpsNdmfToVrcf }

        private Direction _direction = Direction.VrcfToSpsNdmf;
        private bool _convertGlobalCollider = true;
        private bool _convertTouchReceiver = true;
        private bool _convertTouchSender = true;

        private ConversionPlan _lastPlan;
        private bool _lastPlanWasExecuted;
        private Vector2 _scroll;

        [MenuItem("Tools/SPSNDMF/Migrator")]
        public static void Open() {
            var w = GetWindow<MigratorWindow>("SPSNDMF Migrator");
            w.minSize = new Vector2(420, 500);
            w.Show();
        }

        private void OnEnable() {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable() {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged() => Repaint();

        private void OnGUI() {
            DrawDescription();
            EditorGUILayout.Space();

            var target = Selection.activeGameObject;
            DrawTargetLabel(target);
            EditorGUILayout.Space();

            if (!PackageBinding.VrcfAvailable && !PackageBinding.SpsNdmfAvailable) {
                EditorGUILayout.HelpBox(
                    "VRCFury / スタブ / SPSNDMF のいずれもインストールされていません。",
                    MessageType.Warning);
                return;
            }

            DrawDirection();
            EditorGUILayout.Space();

            if (_direction == Direction.VrcfToSpsNdmf) {
                DrawExtraCheckboxes();
                EditorGUILayout.Space();
            }

            DrawButtons(target);
            EditorGUILayout.Space();

            DrawResultArea();
        }

        private void DrawDescription() {
            EditorGUILayout.HelpBox(
                "このツールは現在 Hierarchy で選択している GameObject とその子孫を対象に、" +
                "SPS 関連コンポーネントを変換します。\n\n" +
                "- 変換元コンポーネントは削除されます（Ctrl+Z で取り消せます）。\n" +
                "- 変換先が既に存在する GameObject はスキップし、完了後に一覧表示します。\n" +
                "- VRCFury 本体またはそのスタブパッケージ、SPSNDMF のいずれか、" +
                "あるいは両方がインストールされている必要があります。",
                MessageType.Info);
        }

        private void DrawTargetLabel(GameObject target) {
            if (target == null) {
                var style = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.red } };
                EditorGUILayout.LabelField("対象を選択してください", style);
                return;
            }
            var path = GetPath(target);
            var descendantCount = target.GetComponentsInChildren<Transform>(true).Length;
            EditorGUILayout.LabelField($"対象: {path}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"（配下 {descendantCount} オブジェクト）");
        }

        private void DrawDirection() {
            using (new EditorGUI.DisabledScope(false)) {
                EditorGUILayout.LabelField("変換方向", EditorStyles.boldLabel);

                var canVrcfToSps = PackageBinding.VrcfAvailable && PackageBinding.SpsNdmfAvailable;
                var canSpsToVrcf = PackageBinding.SpsNdmfAvailable && PackageBinding.VrcfAvailable;

                using (new EditorGUI.DisabledScope(!canVrcfToSps)) {
                    if (GUILayout.Toggle(_direction == Direction.VrcfToSpsNdmf,
                            "VRCFury → SPSNDMF", EditorStyles.radioButton)) {
                        _direction = Direction.VrcfToSpsNdmf;
                    }
                }
                using (new EditorGUI.DisabledScope(!canSpsToVrcf)) {
                    if (GUILayout.Toggle(_direction == Direction.SpsNdmfToVrcf,
                            "SPSNDMF → VRCFury", EditorStyles.radioButton)) {
                        _direction = Direction.SpsNdmfToVrcf;
                    }
                }

                if (!canVrcfToSps || !canSpsToVrcf) {
                    var missing = !PackageBinding.VrcfAvailable
                        ? "VRCFury / スタブパッケージ"
                        : "SPSNDMF";
                    EditorGUILayout.HelpBox($"{missing} がインストールされていません。", MessageType.Warning);
                }
            }
        }

        private void DrawExtraCheckboxes() {
            EditorGUILayout.LabelField("追加変換", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!PackageBinding.GlobalColliderConvertible)) {
                _convertGlobalCollider = EditorGUILayout.ToggleLeft(
                    "VRCFury Global Collider を MA Global Collider に変換", _convertGlobalCollider);
            }
            using (new EditorGUI.DisabledScope(!PackageBinding.TouchReceiverConvertible)) {
                _convertTouchReceiver = EditorGUILayout.ToggleLeft(
                    "VRCFury Haptic Touch Receiver を VRC Contact Receiver に変換", _convertTouchReceiver);
            }
            using (new EditorGUI.DisabledScope(!PackageBinding.TouchSenderConvertible)) {
                _convertTouchSender = EditorGUILayout.ToggleLeft(
                    "VRCFury Haptic Touch Sender を VRC Contact Sender に変換", _convertTouchSender);
            }
        }

        private void DrawButtons(GameObject target) {
            using (new EditorGUILayout.HorizontalScope()) {
                using (new EditorGUI.DisabledScope(target == null)) {
                    if (GUILayout.Button("プレビュー", GUILayout.Height(28))) {
                        _lastPlan = BuildPlan(target);
                        _lastPlanWasExecuted = false;
                    }
                    if (GUILayout.Button("実行", GUILayout.Height(28))) {
                        ExecuteConversion(target);
                    }
                }
            }
        }

        private void DrawResultArea() {
            if (_lastPlan == null) {
                EditorGUILayout.LabelField("プレビューまたは実行するとここに結果が表示されます。");
                return;
            }

            var title = _lastPlanWasExecuted ? "実行結果" : "変換プレビュー";
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                $"変換: {_lastPlan.ConvertedCount} 件 / スキップ: {_lastPlan.SkippedCount} 件");

            _scroll = EditorGUILayout.BeginScrollView(_scroll,
                EditorStyles.helpBox, GUILayout.ExpandHeight(true));
            foreach (var e in _lastPlan.Entries) {
                DrawEntry(e);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawEntry(ConversionEntry e) {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                var ico = e.Outcome == ConversionOutcome.Converted
                    ? EditorGUIUtility.IconContent("d_Toolbar Plus@2x")
                    : EditorGUIUtility.IconContent("console.warnicon");
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.Label(ico, GUILayout.Width(20), GUILayout.Height(20));
                    if (GUILayout.Button(GetPath(e.Target), EditorStyles.linkLabel)) {
                        Selection.activeGameObject = e.Target;
                        EditorGUIUtility.PingObject(e.Target);
                    }
                }
                EditorGUILayout.LabelField(
                    $"{ShortName(e.SourceTypeName)} → {e.TargetDescription}",
                    EditorStyles.miniLabel);
                if (!string.IsNullOrEmpty(e.Reason)) {
                    EditorGUILayout.LabelField("スキップ: " + e.Reason, EditorStyles.miniLabel);
                }
                foreach (var w in e.Warnings) {
                    EditorGUILayout.LabelField("  警告: " + w, EditorStyles.miniLabel);
                }
            }
        }

        private ConversionPlan BuildPlan(GameObject target) {
            var plan = new ConversionPlan();
            PlugSocketConverter.Plan(target, _direction == Direction.VrcfToSpsNdmf, plan);
            if (PackageBinding.SpsMenusConvertible)
                SpsMenusConverter.Plan(target, _direction == Direction.VrcfToSpsNdmf, plan);
            if (_direction == Direction.VrcfToSpsNdmf) {
                if (_convertGlobalCollider && PackageBinding.GlobalColliderConvertible)
                    GlobalColliderConverter.Plan(target, plan);
                if (_convertTouchReceiver && PackageBinding.TouchReceiverConvertible)
                    TouchConverter.PlanReceiver(target, plan);
                if (_convertTouchSender && PackageBinding.TouchSenderConvertible)
                    TouchConverter.PlanSender(target, plan);
            }
            return plan;
        }

        private void ExecuteConversion(GameObject target) {
            var plan = BuildPlan(target);
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("SPSNDMF Migration");
            var group = Undo.GetCurrentGroup();

            var ops = new UndoConversionOps();
            try {
                PlugSocketConverter.Execute(target, _direction == Direction.VrcfToSpsNdmf, plan, ops);
                if (PackageBinding.SpsMenusConvertible)
                    SpsMenusConverter.Execute(target, _direction == Direction.VrcfToSpsNdmf, plan, ops);
                if (_direction == Direction.VrcfToSpsNdmf) {
                    if (_convertGlobalCollider && PackageBinding.GlobalColliderConvertible)
                        GlobalColliderConverter.Execute(target, plan, ops);
                    if (_convertTouchReceiver && PackageBinding.TouchReceiverConvertible)
                        TouchConverter.ExecuteReceiver(target, plan, ops);
                    if (_convertTouchSender && PackageBinding.TouchSenderConvertible)
                        TouchConverter.ExecuteSender(target, plan, ops);
                }
            } finally {
                Undo.CollapseUndoOperations(group);
            }

            _lastPlan = plan;
            _lastPlanWasExecuted = true;
        }

        private static string GetPath(GameObject go) {
            if (go == null) return "";
            var path = go.name;
            var t = go.transform.parent;
            while (t != null) {
                path = t.name + "/" + path;
                t = t.parent;
            }
            return path;
        }

        private static string ShortName(string fullName) {
            if (string.IsNullOrEmpty(fullName)) return "";
            var dot = fullName.LastIndexOf('.');
            return dot >= 0 ? fullName.Substring(dot + 1) : fullName;
        }
    }
}
