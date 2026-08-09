using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEngine;
using VF.Component;
using VF.Plugin;

namespace VF.Tests {
    [Category("VRCFury")]
    public class SpsErrorsTest {
        private const string PlayModeContactsKey = "spsndmf.playModeContacts";

        private GameObject avatar;

        [SetUp]
        public void SetUp() {
            avatar = new GameObject("avatar");
        }

        [TearDown]
        public void TearDown() {
            if (avatar != null) Object.DestroyImmediate(avatar);
        }

        private GameObject NewChild(string name) {
            var obj = new GameObject(name);
            obj.transform.SetParent(avatar.transform);
            return obj;
        }

        [Test]
        public void OneOfEachPasses() {
            NewChild("plug").AddComponent<VRCFuryHapticPlug>();
            NewChild("socket").AddComponent<VRCFuryHapticSocket>();

            var ok = false;
            var errors = ErrorReport.CaptureErrors(() => ok = SpsErrors.CheckDuplicateComponents(avatar));

            Assert.That(ok, Is.True);
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void DuplicatePlugIsReported() {
            var obj = NewChild("plug");
            obj.AddComponent<VRCFuryHapticPlug>();
            obj.AddComponent<VRCFuryHapticPlug>();

            var ok = true;
            var errors = ErrorReport.CaptureErrors(() => ok = SpsErrors.CheckDuplicateComponents(avatar));

            Assert.That(ok, Is.False);
            Assert.That(errors.Count, Is.EqualTo(1));

            var error = (SimpleError)errors[0].TheError;
            Assert.That(error.Severity, Is.EqualTo(ErrorSeverity.Error));
            // Also proves the localization keys resolve; an unresolved key would
            // render as "<spsndmf.duplicateComponent>".
            Assert.That(error.ToMessage(), Does.Contain("SPS Plug").And.Contain("2"));
            Assert.That(error.References.Length, Is.EqualTo(1));
        }

        [Test]
        public void DuplicateSocketIsReported() {
            var obj = NewChild("socket");
            obj.AddComponent<VRCFuryHapticSocket>();
            obj.AddComponent<VRCFuryHapticSocket>();

            var ok = true;
            var errors = ErrorReport.CaptureErrors(() => ok = SpsErrors.CheckDuplicateComponents(avatar));

            Assert.That(ok, Is.False);
            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0].TheError.ToMessage(), Does.Contain("SPS Socket"));
        }

        [Test]
        public void PlayModeContactsIsReportedAsInformation() {
            var errors = ErrorReport.CaptureErrors(SpsErrors.ReportPlayModeContacts);

            Assert.That(errors.Count, Is.EqualTo(1));

            var error = (SimpleError)errors[0].TheError;
            Assert.That(error.Severity, Is.EqualTo(ErrorSeverity.Information));
            // An unresolved key would render as "<spsndmf.playModeContacts>". Asserting on the
            // rendered text itself is not an option, because it follows the editor language.
            Assert.That(error.ToMessage(), Does.Not.Contain(PlayModeContactsKey));
            // The menu path is spelled the same way in every translation
            Assert.That(error.FormatHint(), Does.Contain("Enable SPS contacts in play mode"));
        }
    }
}
