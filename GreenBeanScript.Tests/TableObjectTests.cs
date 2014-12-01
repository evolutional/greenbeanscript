using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GreenBeanScript.Tests
{
    [TestClass]
    public class TableObjectTests
    {
        [TestMethod]
        public void When_Set_a_string_based_key_Then_it_can_be_retrieved()
        {
            var uut = new TableObject();
            var key = new Variable("thekey");
            var value = new Variable("value");

            uut.Set(ref key, ref value);

            var result = uut.Get(ref key);

            result.IsNull.Should().BeFalse();
        }

        [TestMethod]
        public void When_Set_an_index_based_key_Then_it_can_be_retrieved()
        {
            var uut = new TableObject();
            var key = new Variable(10);
            var value = new Variable("value");

            uut.Set(ref key, ref value);

            var result = uut.Get(ref key);

            result.IsNull.Should().BeFalse();
            result.GetString().Should().Be("value");
        }

    }
}