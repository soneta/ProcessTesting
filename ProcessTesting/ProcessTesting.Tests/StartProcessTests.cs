using System;
using Soneta.Test;
using System.Reflection;
using NUnit.Framework;
using Soneta.Workflow;
using FluentAssertions;
using Soneta.Business;
using Soneta.Core.DbTuples;
using Soneta.Test.Helpers.Extensions;
using Soneta.Workflow.Config;
using Soneta.Workflow.Dms;
using static ProcessTesting.Tests.Tools;

namespace ProcessTesting.Tests
{
    [TestDatabasePlatynowa]
    internal class StartProcessTests : DbTransactionTestBase
    {
        public override void ClassSetup()
        {
            // Konieczne jest wczytanie assembly dodatku
            LoadAssembly("ProcessTesting");
            base.ClassSetup();
        }

        [Test]
        public void ShouldAutomaticallyStartProcessTest() {
            var wfDefGuid = Guids[Names.WfTest];
            var wfDef = ImportWfDefinition(wfDefGuid, "WfTest.xml");

            AddBasicDocument();

            AssertStartedProcess(Get(wfDef));
        }

        private WFDefinition ImportWfDefinition(Guid wfDefGuid, string resourceName) {
            SetDefinition(this, wfDefGuid, resourceName);
            var wfDef = Session.GetWorkflow().WFDefs[wfDefGuid];
            wfDef.Should().NotBeNull();

            return wfDef;
        }

        private void AddBasicDocument() {
            var dmsModule = Session.GetDms();
            var definition = dmsModule.BasicDocDefs.ByName["Pismo"];
            definition.Should().NotBeNull();
            var register = dmsModule.Registers.ByName["Przychodzące"];
            register.Should().NotBeNull();

            Add(new BasicDocument(definition, register));

            SaveDispose();
        }

        private void AssertStartedProcess(WFDefinition wfDef) {
            var processes = Session.GetWorkflow().WFWorkflows.WgWorkflowDefinition[wfDef];
            processes.Should().NotBeEmpty();
            processes.Count.Should().Be(1);
        }

        [Test]
        public void ShouldManuallyStartProcessTest() {
            var wfDefGuid = Guids[Names.CostLetterProcess];
            var wfDef = ImportWfDefinition(wfDefGuid, "DokKosztowy.xml");
            SetRightsOnTupleDef();

            InUITransaction(() => {
                var cx = Context.Empty.Clone(Session);
                WorkflowTools.StartWorkflowProcess(wfDef, true,
                    Session.CurrentTransaction,
                    null, ref cx, out var taskCreated);
                taskCreated.Should().BeTrue();
            });
            SaveDispose();

            AssertStartedProcess(Get(wfDef));
        }

        private void SetRightsOnTupleDef() {
            var dbTupleDef = GetConfig<DbTupleDefinition>(Guids[Names.CostLetterTupleDef]);
            dbTupleDef.Should().NotBeNull();
            dbTupleDef.SetRightSource(ConfigEditSession, AccessRights.Granted, SaveDisposeConfig);
        }
    }
}
