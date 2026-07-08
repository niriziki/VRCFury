using System;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Nrzk.SpsMigrator.Convert {
    internal interface IConversionOps {
        Component AddComponent(GameObject go, Type type);
        void Destroy(UObject obj);
        void RegisterCreated(GameObject go);
    }

    internal sealed class UndoConversionOps : IConversionOps {
        public Component AddComponent(GameObject go, Type type) => Undo.AddComponent(go, type);
        public void Destroy(UObject obj) => Undo.DestroyObjectImmediate(obj);
        public void RegisterCreated(GameObject go) => Undo.RegisterCreatedObjectUndo(go, "SPSNDMF Migration");
    }

    internal sealed class BuildConversionOps : IConversionOps {
        public Component AddComponent(GameObject go, Type type) => go.AddComponent(type);
        public void Destroy(UObject obj) => UObject.DestroyImmediate(obj);
        public void RegisterCreated(GameObject go) { }
    }
}
