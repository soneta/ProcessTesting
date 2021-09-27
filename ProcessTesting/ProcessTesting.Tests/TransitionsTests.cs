using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Soneta.Business.Db;
using Soneta.Test;
using Soneta.Workflow;
using Soneta.Workflow.Dms;
using static ProcessTesting.Tests.Tools;

namespace ProcessTesting.Tests
{
    [TestDatabasePlatynowa]
    internal class TransitionsTests : DbTransactionTestBase
    {
        public override void ClassSetup()
        {
            LoadAssembly("ProcessTesting");
            base.ClassSetup();
        }

        [Test]
        public void ShouldGoThroughAutomaticTransition() {
            var (process, basicDoc) = StartBasicDocProcess();

            process.Should().NotBeNull();
            process.Tasks.Count.Should().Be(1);
            basicDoc.Should().NotBeNull();

            InUITransaction(() => basicDoc.ForeignSign = "xyz");
            SaveDispose();

            var tasks = Get(process).Tasks.OfType<Task>().ToList();
            tasks.Count.Should().Be(2);
            tasks.Count(t => t.Progress == TaskProgress.Realized).Should().Be(1);
            tasks.Count(t => t.Progress == TaskProgress.Active && t.Name == "FirstTask").Should().Be(1);
        }

        private (WFWorkflow, BasicDocument) StartBasicDocProcess() {
            var wfDef = ImportWfDefinition(this, Guids[Names.WfTest], "WfTest.xml");
            var basicDoc = AddBasicDocument(this);

            var processes = Session.GetWorkflow().WFWorkflows.WgWorkflowDefinition[wfDef];

            return (processes.GetFirst(), basicDoc);
        }
    }
}
