using System.Collections.Generic;
using DeepCopy;

namespace AssemblyToProcess
{
    [AddDeepCopyConstructor]
    public class ClassWithDictionary
    {
        public IDictionary<int, int> Dictionary { get; set; }
    }

    [AddDeepCopyConstructor]
    public class ClassWithDictionaryString
    {
        public IDictionary<string, string> Dictionary { get; set; }
    }

    [AddDeepCopyConstructor]
    public class ClassWithDictionaryObject
    {
        public IDictionary<SomeKey, SomeObject> Dictionary { get; set; }
    }

    [AddDeepCopyConstructor]
    public class ClassWithDictionaryInstance
    {
        public Dictionary<SomeKey, SomeObject> Dictionary { get; set; }
    }
}