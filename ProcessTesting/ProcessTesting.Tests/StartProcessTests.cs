using Soneta.Test;
using System.Reflection;
using NUnit.Framework;
using Soneta.Workflow;
using FluentAssertions;
using Soneta.Workflow.Dms;
using static ProcessTesting.Tests.Tools;

namespace ProcessTesting.Tests
{
    [TestDatabasePlatynowa]
    internal class StartProcessTests : TestBase
    {
        public override void ClassSetup()
        {
            // Konieczne jest wczytanie assembly dodatku
            Assembly.Load("ProcessTesting");
            base.ClassSetup();
        }

        [Test]
        public void ShouldAutomaticallyStartProcessTest() {
            var wfDefGuid = Guids[Names.WfTest];
            SetDefinition(this, wfDefGuid, "WfTest.xml");
            var wfDef = Session.GetWorkflow().WFDefs[wfDefGuid];
            wfDef.Should().NotBeNull();

            AddBasicDocument();

            wfDef = Get(wfDef);
            var processes = Session.GetWorkflow().WFWorkflows.WgWorkflowDefinition[wfDef];
            processes.Should().NotBeEmpty();
            processes.Count.Should().Be(1);
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
    }
}
