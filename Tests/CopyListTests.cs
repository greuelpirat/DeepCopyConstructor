using System;
using System.Collections.Generic;
using AssemblyToProcess;
using Xunit;

namespace Tests
{
    public partial class WeaverTests
    {
        [Fact]
        public void TestClassWithList()
        {
            var type = TestResult.Assembly.GetType(typeof(ClassWithList).FullName);
            dynamic instance = Activator.CreateInstance(type);
            instance.List = new List<int> { 42, 84 };

            var copy = Activator.CreateInstance(type, instance);
            Assert.Equal(instance.List.Count, copy.List.Count);
            Assert.Equal(instance.List[0], copy.List[0]);
            Assert.Equal(instance.List[1], copy.List[1]);

            Assert.False(ReferenceEquals(instance.List, copy.List));
            Assert.False(ReferenceEquals(instance.List[0], copy.List[0]));
            Assert.False(ReferenceEquals(instance.List[1], copy.List[1]));
        }

        [Fact]
        public void TestClassWithStringList()
        {
            var type = TestResult.Assembly.GetType(typeof(ClassWithListString).FullName);
            dynamic instance = Activator.CreateInstance(type);
            instance.List = new List<string> { "Hello", "World", null };

            var copy = Activator.CreateInstance(type, instance);
            Assert.Equal(instance.List.Count, copy.List.Count);
            Assert.Equal(instance.List[0], copy.List[0]);
            Assert.Equal(instance.List[1], copy.List[1]);

            Assert.False(ReferenceEquals(instance.List, copy.List));
            Assert.False(ReferenceEquals(instance.List[0], copy.List[0]));
            Assert.False(ReferenceEquals(instance.List[1], copy.List[1]));

            Assert.Null(copy.List[2]);
        }

        [Fact]
        public void TestClassWithObjectList()
        {
            var someClass1 = CreateSomeClassInstance(out var someClassType);

            var type = TestResult.Assembly.GetType(typeof(ClassWithListObject).FullName);

            dynamic instance = Activator.CreateInstance(type);

            dynamic list = Activator.CreateInstance(typeof(List<>).MakeGenericType(someClassType));
            instance.List = list;
            instance.List.Add(someClass1);
            instance.List.Add(CreateSomeClassInstance(out _));
            instance.List.Add(null);

            var copy = Activator.CreateInstance(type, instance);
            Assert.Equal(instance.List.Count, copy.List.Count);
            AssertCopyOfSomeClass(instance.List[0], copy.List[0]);
            AssertCopyOfSomeClass(instance.List[1], copy.List[1]);
            Assert.Null(copy.List[2]);
        }
    }
}