using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Nrzk.SpsMigrator.Copy;
using Nrzk.SpsMigrator.Reflection;

namespace Nrzk.SpsMigrator.Convert {
    /// <summary>
    /// VRCFury(SpsOptions feature) ⇄ SPSNDMF SPS Menus component.
    /// Used by the manual migrator window. Stub build-time migration keeps
    /// using SpsOptionsConverter (menuPath is preserved verbatim there).
    /// </summary>
    internal static class SpsMenusConverter {
        private const BindingFlags FieldFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static void Plan(GameObject root, bool vrcfToSpsNdmf, ConversionPlan plan) {
            if (!PackageBinding.SpsMenusConvertible) return;
            if (vrcfToSpsNdmf) {
                var first = true;
                foreach (var src in FindSpsOptionsContainers(root)) {
                    var entry = new ConversionEntry {
                        Target = src.gameObject,
                        SourceTypeName = PackageBinding.VrcfSpsOptionsType.FullName,
                        TargetDescription = PackageBinding.SpsNdmfSpsMenusType.FullName,
                    };
                    if (!first) {
                        entry.Outcome = ConversionOutcome.Skipped;
                        entry.Reason = "only the first SPS Options is converted";
                    } else if (root.GetComponentInChildren(PackageBinding.SpsNdmfSpsMenusType, true) != null) {
                        entry.Outcome = ConversionOutcome.Skipped;
                        entry.Reason = "SPS Menus already exists on this avatar";
                    } else {
                        entry.Outcome = ConversionOutcome.Converted;
                    }
                    first = false;
                    plan.Entries.Add(entry);
                }
            } else {
                var first = true;
                foreach (var src in root.GetComponentsInChildren(PackageBinding.SpsNdmfSpsMenusType, true)) {
                    var entry = new ConversionEntry {
                        Target = src.gameObject,
                        SourceTypeName = PackageBinding.SpsNdmfSpsMenusType.FullName,
                        TargetDescription = PackageBinding.VrcfSpsOptionsType.FullName,
                    };
                    if (!first) {
                        entry.Outcome = ConversionOutcome.Skipped;
                        entry.Reason = "only the first SPS Menus is converted";
                    } else if (FindSpsOptionsContainers(root).Count > 0) {
                        entry.Outcome = ConversionOutcome.Skipped;
                        entry.Reason = "SPS Options already exists on this avatar";
                    } else {
                        entry.Outcome = ConversionOutcome.Converted;
                    }
                    first = false;
                    plan.Entries.Add(entry);
                }
            }
        }

        public static void Execute(GameObject root, bool vrcfToSpsNdmf, ConversionPlan plan, IConversionOps ops) {
            if (!PackageBinding.SpsMenusConvertible) return;
            if (vrcfToSpsNdmf) {
                foreach (var src in FindSpsOptionsContainers(root)) {
                    var entry = FindEntry(plan, src.gameObject, PackageBinding.VrcfSpsOptionsType.FullName);
                    if (entry != null && entry.Outcome != ConversionOutcome.Skipped) {
                        ExecuteToSpsMenus(root, src, entry, ops);
                    }
                    // Same-GameObject containers all match the first plan entry (FindEntry
                    // cannot distinguish them by GameObject+type alone), so only the first
                    // container may ever be processed here; Plan already marked the rest Skipped.
                    break;
                }
            } else {
                foreach (var src in root.GetComponentsInChildren(PackageBinding.SpsNdmfSpsMenusType, true)) {
                    var entry = FindEntry(plan, src.gameObject, PackageBinding.SpsNdmfSpsMenusType.FullName);
                    if (entry != null && entry.Outcome != ConversionOutcome.Skipped) {
                        ExecuteToSpsOptions(root, (Component)src, entry, ops);
                    }
                    // Same as above: only the first SPS Menus component is ever processed.
                    break;
                }
            }
        }

        private static void ExecuteToSpsMenus(GameObject root, Component srcContainer, ConversionEntry entry, IConversionOps ops) {
            var srcOpts = GetField(srcContainer, "content");
            var menuPath = GetField(srcOpts, "menuPath") as string ?? "";
            var segments = menuPath.Split('/').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            var name = segments.Length > 0 ? segments[segments.Length - 1] : "SPS";

            var go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            ops.RegisterCreated(go);

            var spsMenus = ops.AddComponent(go, PackageBinding.SpsNdmfSpsMenusType);
            SetField(spsMenus, "createParentMenu", true);
            var menuIconWrapper = GetField(srcOpts, "menuIcon");
            var iconObj = menuIconWrapper != null ? GetField(menuIconWrapper, "objRef") as Texture2D : null;
            SetField(spsMenus, "menuIcon", iconObj);
            CopyBool(srcOpts, spsMenus, "saveSockets", entry);
            CopyBool(srcOpts, spsMenus, "legacyModeEnabledOnAvatarLoad", entry);

            ops.AddComponent(go, PackageBinding.MaMenuInstallerType);

            if (segments.Length > 1) {
                entry.Warnings.Add(new CopyWarning(
                    go.name + ".menuPath",
                    $"parent path '{string.Join("/", segments.Take(segments.Length - 1))}' cannot be converted;" +
                    " place the SPS Menus object under the desired MA Menu Item instead" +
                    " (and remove the MA Menu Installer)"));
            }

            ops.Destroy(srcContainer);
        }

        private static void ExecuteToSpsOptions(GameObject root, Component srcMenus, ConversionEntry entry, IConversionOps ops) {
            var container = ops.AddComponent(root, PackageBinding.VrcfVrcFuryType);
            var opts = Activator.CreateInstance(PackageBinding.VrcfSpsOptionsType);

            var createParent = GetField(srcMenus, "createParentMenu") is bool b && b;
            SetField(opts, "menuPath", createParent ? srcMenus.gameObject.name : "");
            var iconTex = GetField(srcMenus, "menuIcon") as Texture2D;
            if (iconTex != null) {
                var menuIconField = PackageBinding.VrcfSpsOptionsType.GetField("menuIcon", FieldFlags);
                var opImplicit = menuIconField?.FieldType.GetMethod(
                    "op_Implicit", BindingFlags.Public | BindingFlags.Static, null,
                    new[] { typeof(Texture2D) }, null);
                if (menuIconField != null && opImplicit != null) {
                    menuIconField.SetValue(opts, opImplicit.Invoke(null, new object[] { iconTex }));
                } else {
                    entry.Warnings.Add(new CopyWarning("menuIcon", "could not convert icon to GuidTexture2d"));
                }
            }
            CopyBool(srcMenus, opts, "saveSockets", entry);
            CopyBool(srcMenus, opts, "legacyModeEnabledOnAvatarLoad", entry);
            SetField(container, "content", opts);

            var go = srcMenus.gameObject;
            if (go.transform.parent != null && PackageBinding.MaMenuSourceType != null
                && go.transform.parent.GetComponent(PackageBinding.MaMenuSourceType) != null) {
                entry.Warnings.Add(new CopyWarning(
                    go.name,
                    "menu position inside the menu tree cannot be converted to menuPath;" +
                    " review the SpsOptions menuPath manually"));
            }

            if (OnlyHasSpsMenusAndInstaller(go)) {
                ops.Destroy(go);
            } else {
                entry.Warnings.Add(new CopyWarning(
                    go.name,
                    "object kept because it has other components or children; removed only the SPS Menus component"));
                ops.Destroy(srcMenus);
            }
        }

        private static bool OnlyHasSpsMenusAndInstaller(GameObject go) {
            if (go.transform.childCount > 0) return false;
            foreach (var c in go.GetComponents<Component>()) {
                if (c is Transform) continue;
                var t = c.GetType();
                if (t == PackageBinding.SpsNdmfSpsMenusType) continue;
                if (t == PackageBinding.MaMenuInstallerType) continue;
                return false;
            }
            return true;
        }

        private static List<Component> FindSpsOptionsContainers(GameObject root) {
            var result = new List<Component>();
            foreach (var c in root.GetComponentsInChildren(PackageBinding.VrcfVrcFuryType, true)) {
                var content = GetField(c, "content");
                if (content != null && content.GetType() == PackageBinding.VrcfSpsOptionsType) {
                    result.Add(c);
                }
            }
            return result;
        }

        private static ConversionEntry FindEntry(ConversionPlan plan, GameObject go, string sourceTypeName) {
            return plan.Entries.Find(e => e.Target == go && e.SourceTypeName == sourceTypeName);
        }

        private static void CopyBool(object src, object dst, string fieldName, ConversionEntry entry) {
            var srcField = src.GetType().GetField(fieldName, FieldFlags);
            if (srcField == null) {
                entry.Warnings.Add(new CopyWarning(fieldName, "source has no such field; using default"));
                return;
            }
            SetField(dst, fieldName, srcField.GetValue(src));
        }

        private static object GetField(object obj, string name) {
            if (obj == null) return null;
            for (var t = obj.GetType(); t != null && t != typeof(object); t = t.BaseType) {
                var f = t.GetField(name, FieldFlags | BindingFlags.DeclaredOnly);
                if (f != null) return f.GetValue(obj);
            }
            return null;
        }

        private static void SetField(object obj, string name, object value) {
            for (var t = obj.GetType(); t != null && t != typeof(object); t = t.BaseType) {
                var f = t.GetField(name, FieldFlags | BindingFlags.DeclaredOnly);
                if (f != null) { f.SetValue(obj, value); return; }
            }
        }
    }
}
