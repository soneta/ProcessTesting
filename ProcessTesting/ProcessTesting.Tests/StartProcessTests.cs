using Soneta.Test;
using NUnit.Framework;
using Soneta.Workflow;
using FluentAssertions;
using Soneta.Business;
using Soneta.Core.DbTuples;
using Soneta.Test.Helpers.Extensions;
using Soneta.Workflow.Config;
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
            var wfDef = ImportWfDefinition(this, wfDefGuid, "WfTest.xml");

            AddBasicDocument(this);

            AssertStartedProcess(Get(wfDef));
        }

        private void AssertStartedProcess(WFDefinition wfDef) {
            var processes = Session.GetWorkflow().WFWorkflows.WgWorkflowDefinition[wfDef];
            processes.Should().NotBeEmpty();
            processes.Count.Should().Be(1);
        }

        [Test]
        public void ShouldManuallyStartProcessTest() {
            var wfDefGuid = Guids[Names.CostLetterProcess];
            var wfDef = ImportWfDefinition(this, wfDefGuid, "DokKosztowy.xml");
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
