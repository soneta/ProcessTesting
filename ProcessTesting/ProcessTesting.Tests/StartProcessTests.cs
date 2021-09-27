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
            var wfDef = StartCostLetterProcessManually(this);

            AssertStartedProcess(Get(wfDef));
        }

        internal static WFDefinition StartCostLetterProcessManually(TestBase testBase) {
            var wfDefGuid = Guids[Names.CostLetterProcess];
            var wfDef = ImportWfDefinition(testBase, wfDefGuid, "DokKosztowy.xml");
            SetRightsOnTupleDef(testBase);

            testBase.InUITransaction(() => {
                var cx = Context.Empty.Clone(testBase.Session);
                WorkflowTools.StartWorkflowProcess(wfDef, true,
                    testBase.Session.CurrentTransaction,
                    null, ref cx, out var taskCreated);
                taskCreated.Should().BeTrue();
            });
            testBase.SaveDispose();

            return testBase.Get(wfDef);
        }

        private static void SetRightsOnTupleDef(TestBase testBase) {
            var dbTupleDef = testBase.GetConfig<DbTupleDefinition>(Guids[Names.CostLetterTupleDef]);
            dbTupleDef.Should().NotBeNull();
            dbTupleDef.SetRightSource(testBase.ConfigEditSession, AccessRights.Granted, testBase.SaveDisposeConfig);
        }
    }
}
