namespace net.nrzk.spsndmf.migrator.Copy {
    internal readonly struct CopyWarning {
        public readonly string Path;
        public readonly string Reason;

        public CopyWarning(string path, string reason) {
            Path = path;
            Reason = reason;
        }

        public override string ToString() => $"{Path}: {Reason}";
    }
}
