using System;
using FluentAssertions;
using GreenBeanScript.VirtualMachine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GreenBeanScript.Tests
{
    [TestClass]
    public class ThreadStackTests
    {
        [TestMethod]
        public void Push_Increments_StackPointer()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.Null);
            uut.StackPointer.Should().Be(1);
        }

        [TestMethod]
        public void Can_Pop_a_pushed_item()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.One);
            var result = uut.Pop();
            result.Should().Be(Variable.One);
        }

        [TestMethod]
        public void Pop_decrements_StackPointer()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.One);
            uut.Pop();
            uut.StackPointer.Should().Be(0);
        }

        [TestMethod]
        public void Pop_with_param_decrements_StackPointer_by_same_value()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.One);
            uut.Push(2);
            uut.Pop(2);
            uut.StackPointer.Should().Be(0);
        }

        [TestMethod]
        public void Pop_with_param_returns_the_correct_item()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.One);
            uut.Push(2);
            var result = uut.Pop(2);
            result.Should().Be(Variable.One);
        }

        [TestMethod]
        public void Peek_with_param_returns_the_item_offset_from_StackPointer()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.One);
            uut.Push(2);
            var result = uut.Peek(-2);
            result.Should().Be(Variable.One);
        }

        [TestMethod]
        public void Peek_returns_the_top_item()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.One);
            uut.Push(2);
            var result = uut.Peek();
            result.Should().BeNull();
        }

        [TestMethod]
        public void Poke_modifies_top_value()
        {
            var uut = new ThreadStack();
            uut.Poke(Variable.One);
            var result = uut.Peek();
            result.Should().Be(Variable.One);
        }

        [TestMethod]
        public void Poke_does_not_modify_StackPointer()
        {
            var uut = new ThreadStack();
            uut.Poke(Variable.One);
            uut.StackPointer.Should().Be(0);
        }

        [TestMethod]
        public void Poke_with_param_modifies_the_item_offset_from_StackPointer()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.One);
            uut.Push(2);
            uut.Poke(-2, 15);
            var result = uut.Peek(-2);
            result.GetInteger().Should().Be(15);
        }

        [TestMethod]
        public void PeekAbsreturns_the_item_at_offset()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.One);
            uut.Push(2);
            var result = uut.PeekAbs(0);
            result.Should().Be(Variable.One);
        }

        [TestMethod]
        public void PokeAbs_modifies_the_item_at_offset()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.One);
            uut.Push(2);
            uut.PokeAbs(1, 15);
            var result = uut.PeekAbs(1);
            result.GetInteger().Should().Be(15);
        }

        [TestMethod]
        public void BasePointer_starts_at_zero()
        {
            var uut = new ThreadStack();
            uut.BasePointer.Should().Be(0);
        }

        [TestMethod]
        public void BasePointer_is_modifiable()
        {
            var uut = new ThreadStack();
            uut.BasePointer = 1;
            uut.BasePointer.Should().Be(1);
        }


        [TestMethod]
        public void PeekBase_returns_the_item_at_offset_from_BasePointer()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.One);
            uut.Push(2);
            uut.BasePointer = 1;
            var result = uut.PeekBase(-1);
            result.Should().Be(Variable.One);
        }

        [TestMethod]
        public void PeekBase_with_zero_returns_the_item_at_BasePointer()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.One);
            uut.Push(2);
            uut.BasePointer = 1;
            var result = uut.PeekBase(0);
            result.GetInteger().Should().Be(2);
        }

        [TestMethod]
        public void PokeBase_modifes_the_item_at_offset_from_BasePointer()
        {
            var uut = new ThreadStack();
            uut.Push(Variable.One);
            uut.Push(2);
            uut.BasePointer = 1;
            uut.PokeBase(-1, 5);
            var result = uut.PeekBase(-1);
            result.GetInteger().Should().Be(5);
        }

    }
}
