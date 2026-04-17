using System.Collections.Generic;
using UnityEngine;
using net.nrzk.spsndmf.migrator.Copy;

namespace net.nrzk.spsndmf.migrator.Convert {
    internal enum ConversionOutcome { Converted, Skipped }

    internal sealed class ConversionEntry {
        public GameObject Target;
        public string SourceTypeName;
        public string TargetDescription;
        public ConversionOutcome Outcome;
        public string Reason;
        public List<CopyWarning> Warnings = new List<CopyWarning>();
    }

    internal sealed class ConversionPlan {
        public List<ConversionEntry> Entries = new List<ConversionEntry>();
        public int ConvertedCount {
            get {
                var n = 0;
                foreach (var e in Entries) if (e.Outcome == ConversionOutcome.Converted) n++;
                return n;
            }
        }
        public int SkippedCount => Entries.Count - ConvertedCount;
    }
}
