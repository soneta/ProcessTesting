using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using FluentAssertions;
using Soneta.Business;
using Soneta.Business.App;
using Soneta.Test;
using Soneta.Test.Helpers.Extensions;
using Soneta.Types;
using Soneta.Workflow;
using Soneta.Workflow.Config;
using Soneta.Workflow.Dms;

namespace ProcessTesting.Tests {
    internal class Tools {
        internal enum Names {
            CostLetterProcess,
            CostLetterTupleDef,
            WfTest
        }

        internal static readonly IDictionary<Names, Guid> Guids = new Dictionary<Names, Guid> {
            { Names.CostLetterProcess, new Guid("00000000-0016-0004-0001-000000000000") },
            { Names.CostLetterTupleDef, new Guid("3B16A8B0-55F0-4C97-BE28-DDBE2B428E40") },
            { Names.WfTest, new Guid("96ca4160-acc6-4b89-a7c5-50f8f29aec2e") }
        };

        internal static void SetDefinition(TestBase testBase, Guid wfDefGuid, string resourceName) {
            if (testBase == null)
                throw new ArgumentNullException("testBase");

            ImportInSession(testBase.ConfigEditSession, resourceName, testBase.SaveDisposeConfig);
            SetRight(testBase, AccessRights.Granted, wfDefGuid);
        }

        internal static void ImportInSession(ISessionable session, string resourceName, Method saveSessionHandler) {
            if (session?.Session == null)
                throw new ArgumentNullException("session");

            using (var tran = session.Session.Logout(true)) {
                ImportXml(session.Session.Login, resourceName);
                tran.Commit();
            }

            saveSessionHandler?.Invoke();
        }

        private static void ImportXml(Login login, string resourceName) {
            if (login == null) return;

            using (var stream = GetResourceStream(resourceName)) {
                if (stream == null) return;

                using (var reader = new XmlTextReader(stream)) {
                    reader.WhitespaceHandling = WhitespaceHandling.Significant;
                    new SessionReader(login).Read(reader);
                }
            }
        }

        internal static Stream GetResourceStream(string resourceName) {
            if (string.IsNullOrWhiteSpace(resourceName))
                return null;

            var assembly = Assembly.GetExecutingAssembly();
            var resource = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(resourceName));
            return string.IsNullOrWhiteSpace(resource) ? null : assembly.GetManifestResourceStream(resource);
        }

        internal static void SetRight(TestBase testBase, AccessRights accessRight, Guid guid) {
            if (testBase == null)
                throw new ArgumentNullException("testBase");

            var def = testBase.GetConfig<WFDefinition>(guid);
            def.SetRightSource(testBase.ConfigEditSession, accessRight);

            testBase.SaveDisposeConfig();
        }

        internal static WFDefinition ImportWfDefinition(TestBase testBase, Guid wfDefGuid, string resourceName) {
            SetDefinition(testBase, wfDefGuid, resourceName);
            var wfDef = testBase.Session.GetWorkflow().WFDefs[wfDefGuid];
            wfDef.Should().NotBeNull();

            return wfDef;
        }

        internal static BasicDocument AddBasicDocument(TestBase testBase) {
            var dmsModule = testBase.Session.GetDms();
            var definition = dmsModule.BasicDocDefs.ByName["Pismo"];
            definition.Should().NotBeNull();
            var register = dmsModule.Registers.ByName["Przychodzące"];
            register.Should().NotBeNull();
            var basicDoc = new BasicDocument(definition, register);
            testBase.Add(basicDoc);

            testBase.SaveDispose();

            return testBase.Get(basicDoc);
        }
    }
}
