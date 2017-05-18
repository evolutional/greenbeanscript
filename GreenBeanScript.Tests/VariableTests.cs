using FluentAssertions;
using GreenBeanScript.VirtualMachine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GreenBeanScript.Tests
{
    [TestClass]
    public class VariableTests
    {
        [TestMethod]
        public void String_with_same_value_are_Equal()
        {
            var a = new Variable("vara");
            var b = new Variable("vara");

            var result = a.Equals(b);
            result.Should().BeTrue();
        }

        [TestMethod]
        public void String_with_different_values_are_not_Equal()
        {
            var a = new Variable("vara");
            var b = new Variable("varb");

            a.Equals(b).Should().BeFalse();
        }

        [TestMethod]
        public void String_IsString_True()
        {
            var a = new Variable("vara");
            a.IsString.Should().BeTrue();
        }

        [TestMethod]
        public void Integer_IsInt_True()
        {
            var a = new Variable(100);

            a.IsInt.Should().BeTrue();
        }

        [TestMethod]
        public void Integer_IsFloat_False()
        {
            var a = new Variable(100);

            a.IsFloat.Should().BeFalse();
        }

        [TestMethod]
        public void Integer_GetFloat_IsValid()
        {
            var a = new Variable(100);
            var f = a.GetFloat();
            f.Should().Be(100.0f);
        }

        [TestMethod]
        public void Integer_IsNumber_True()
        {
            var a = new Variable(100);

            a.IsNumber.Should().BeTrue();
        }

        [TestMethod]
        public void Integer_with_same_value_are_Equal()
        {
            var a = new Variable(100);
            var b = new Variable(100);

            a.Equals(b).Should().BeTrue();
        }

        [TestMethod]
        public void Float_with_same_value_are_Equal()
        {
            var a = new Variable(50.25f);
            var b = new Variable(50.25f);

            a.Equals(b).Should().BeTrue();
        }

        [TestMethod]
        public void Float_IsInt_False()
        {
            var a = new Variable(123.45f);

            a.IsInt.Should().BeFalse();
        }

        [TestMethod]
        public void Float_IsFloat_True()
        {
            var a = new Variable(123.45f);

            a.IsFloat.Should().BeTrue();
        }

        [TestMethod]
        public void Float_IsNumber_True()
        {
            var a = new Variable(123.45f);

            a.IsNumber.Should().BeTrue();
        }

        [TestMethod]
        public void Null_GetFloat_Is_Zero()
        {
            var a = new Variable();
            a.GetFloat().Should().Be(0);
        }

        [TestMethod]
        public void Null_GetInteger_Is_Zero()
        {
            var a = new Variable();
            a.GetInteger().Should().Be(0);
        }
    }
}