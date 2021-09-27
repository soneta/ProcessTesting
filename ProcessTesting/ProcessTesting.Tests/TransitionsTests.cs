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
            var process = GoToFirstTask();

            var tasks = process.Tasks.OfType<Task>().ToList();
            tasks.Count.Should().Be(2);
            tasks.Count(t => t.Progress == TaskProgress.Realized).Should().Be(1);
            tasks.Count(t => t.Progress == TaskProgress.Active && t.Name == "FirstTask").Should().Be(1);
        }

        private WFWorkflow GoToFirstTask() {
            var (process, basicDoc) = StartBasicDocProcess();

            process.Should().NotBeNull();
            process.Tasks.Count.Should().Be(1);
            basicDoc.Should().NotBeNull();

            InUITransaction(() => basicDoc.ForeignSign = "xyz");
            SaveDispose();

            return Get(process);
        }

        private (WFWorkflow, BasicDocument) StartBasicDocProcess() {
            var wfDef = ImportWfDefinition(this, Guids[Names.WfTest], "WfTest.xml");
            var basicDoc = AddBasicDocument(this);

            var processes = Session.GetWorkflow().WFWorkflows.WgWorkflowDefinition[wfDef];

            return (processes.GetFirst(), basicDoc);
        }

        [Test]
        public void ShouldGoThroughOperatorsChoiceTransitionTest() {
            var process = GoToMultitask();

            var tasks = process.Tasks.OfType<Task>().ToList();
            tasks.Count.Should().Be(4);
            tasks.Count(t => t.Progress == TaskProgress.Realized).Should().Be(2);
            tasks.Count(t => t.Progress == TaskProgress.Active && t.Name == "MultiTask").Should().Be(2);
        }

        private WFWorkflow GoToMultitask() {
            var process = GoToFirstTask();
            var firstTask = process.Tasks.OfType<Task>()
                .Single(t => t.Progress == TaskProgress.Active && t.Name == "FirstTask");
            firstTask.Should().NotBeNull();

            InTransaction(() => firstTask.GoThru("B"));
            SaveDispose();

            return Get(process);
        }

        [Test]
        public void ShouldEndMultitaskWhenOneOperatorMakesChoice() {
            var process = GoToMultitask();

            var task = process.Tasks.OfType<Task>().Single(t =>
                t.Progress == TaskProgress.Active && t.Name == "MultiTask" && t.Operator != null && t.Operator.Name == "Administrator");
            task.Should().NotBeNull();

            InTransaction(() => task.GoThru("C"));
            SaveDispose();

            process = Get(process);
            process.IsClosed.Should().BeTrue();

            var tasks = process.Tasks.OfType<Task>().ToList();
            tasks.Count.Should().Be(6);
            tasks.Count(t => t.Progress == TaskProgress.Realized).Should().Be(6);
        }
    }
}
