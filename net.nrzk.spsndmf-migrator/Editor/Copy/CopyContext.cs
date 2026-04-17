using System;
using System.Collections.Generic;
using System.Reflection;

namespace net.nrzk.spsndmf.migrator.Copy {
    internal sealed class CopyContext {
        public readonly List<CopyWarning> Warnings = new List<CopyWarning>();
        private readonly Dictionary<(Type src, string name), Type> _targetTypeCache
            = new Dictionary<(Type, string), Type>();

        public Type ResolveTargetType(Type sourceType, Type hintType) {
            if (sourceType == null) return null;

            if (hintType != null && !hintType.IsAbstract && !hintType.IsInterface
                && (hintType.IsAssignableFrom(sourceType) || sourceType.Name == hintType.Name)) {
                return hintType;
            }

            var preferredRoot = GetRootNamespace(hintType);
            var key = (sourceType, preferredRoot ?? "");
            if (_targetTypeCache.TryGetValue(key, out var cached)) return cached;

            var candidate = FindByName(sourceType, preferredRoot);
            _targetTypeCache[key] = candidate;
            return candidate;
        }

        private static string GetRootNamespace(Type t) {
            if (t?.Namespace == null) return null;
            var dot = t.Namespace.IndexOf('.');
            return dot >= 0 ? t.Namespace.Substring(0, dot) : t.Namespace;
        }

        private static Type FindByName(Type source, string preferredRoot) {
            Type fallback = null;
            var declaringName = source.DeclaringType?.Name;
            var targetName = source.Name;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types; }
                catch { continue; }

                foreach (var t in types) {
                    if (t == null) continue;
                    if (t == source) continue;
                    if (t.Name != targetName) continue;
                    if (declaringName != null && t.DeclaringType?.Name != declaringName) continue;
                    if (declaringName == null && t.DeclaringType != null) continue;

                    if (preferredRoot != null && GetRootNamespace(t) == preferredRoot) return t;
                    if (fallback == null) fallback = t;
                }
            }
            return fallback;
        }
    }
}
