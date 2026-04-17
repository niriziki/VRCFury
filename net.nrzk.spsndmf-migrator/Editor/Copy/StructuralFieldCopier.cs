using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Nrzk.SpsMigrator.Copy {
    internal static class StructuralFieldCopier {
        private const BindingFlags FieldFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static void CopyFields(object source, object target, CopyContext ctx, string path) {
            if (source == null || target == null) return;

            foreach (var srcField in EnumerateFields(source.GetType())) {
                if (!ShouldCopy(srcField)) continue;

                var dstField = FindField(target.GetType(), srcField.Name);
                if (dstField == null || !ShouldCopy(dstField)) {
                    ctx.Warnings.Add(new CopyWarning(
                        $"{path}.{srcField.Name}",
                        $"target has no matching field"));
                    continue;
                }

                try {
                    var converted = ConvertValue(
                        srcField.GetValue(source),
                        dstField.FieldType,
                        ctx,
                        $"{path}.{srcField.Name}");
                    dstField.SetValue(target, converted);
                } catch (Exception e) {
                    ctx.Warnings.Add(new CopyWarning(
                        $"{path}.{srcField.Name}",
                        $"copy failed: {e.Message}"));
                }
            }
        }

        private static IEnumerable<FieldInfo> EnumerateFields(Type t) {
            var seen = new HashSet<string>();
            for (var cur = t; cur != null && cur != typeof(object); cur = cur.BaseType) {
                foreach (var f in cur.GetFields(FieldFlags | BindingFlags.DeclaredOnly)) {
                    if (seen.Add(f.Name)) yield return f;
                }
            }
        }

        private static FieldInfo FindField(Type t, string name) {
            for (var cur = t; cur != null && cur != typeof(object); cur = cur.BaseType) {
                var f = cur.GetField(name, FieldFlags | BindingFlags.DeclaredOnly);
                if (f != null) return f;
            }
            return null;
        }

        private static bool ShouldCopy(FieldInfo f) {
            if (f.IsStatic) return false;
            if (Attribute.IsDefined(f, typeof(NonSerializedAttribute))) return false;
            if (f.IsPublic) return true;
            if (Attribute.IsDefined(f, typeof(SerializeField))) return true;
            if (Attribute.IsDefined(f, typeof(SerializeReference))) return true;
            return false;
        }

        private static object ConvertValue(object value, Type targetType, CopyContext ctx, string path) {
            if (value == null) return null;

            var srcType = value.GetType();

            if (srcType == targetType) return value;

            if (targetType.IsPrimitive || targetType == typeof(string) || targetType == typeof(decimal)) {
                return Convert.ChangeType(value, targetType);
            }

            if (targetType.IsEnum) {
                if (srcType.IsEnum) return Enum.ToObject(targetType, Convert.ToInt64(value));
                return Enum.ToObject(targetType, Convert.ToInt64(value));
            }

            if (typeof(UObject).IsAssignableFrom(targetType)) {
                if (targetType.IsInstanceOfType(value)) return value;
                ctx.Warnings.Add(new CopyWarning(path, $"UnityObject type mismatch {srcType.Name} -> {targetType.Name}"));
                return null;
            }

            if (targetType.IsArray) {
                var srcList = (IList)value;
                var elementType = targetType.GetElementType();
                var arr = Array.CreateInstance(elementType, srcList.Count);
                for (var i = 0; i < srcList.Count; i++) {
                    arr.SetValue(ConvertValue(srcList[i], elementType, ctx, $"{path}[{i}]"), i);
                }
                return arr;
            }

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>)) {
                var elementType = targetType.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(targetType);
                var srcList = (IList)value;
                for (var i = 0; i < srcList.Count; i++) {
                    list.Add(ConvertValue(srcList[i], elementType, ctx, $"{path}[{i}]"));
                }
                return list;
            }

            var concreteTargetType = ctx.ResolveTargetType(srcType, targetType);
            if (concreteTargetType == null) {
                ctx.Warnings.Add(new CopyWarning(path, $"no target type for {srcType.FullName}"));
                return null;
            }

            var instance = Activator.CreateInstance(concreteTargetType);
            CopyFields(value, instance, ctx, path);
            return instance;
        }
    }
}
