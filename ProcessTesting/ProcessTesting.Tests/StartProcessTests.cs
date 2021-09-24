using Soneta.Test;
using System.Reflection;

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
    }
}
