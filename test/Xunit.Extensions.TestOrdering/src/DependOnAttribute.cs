using System;

namespace Xunit.Extensions.TestOrdering
{
    /// <summary>
    /// Specifies on which test the current test is dependend for execution within the same fixture
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class DependOnAttribute : Attribute
    {
        public DependOnAttribute(string methodDependency)
        {
            this.MethodDependency = methodDependency;
        }

        public DependOnAttribute(Type dependency)
        {
            this.Dependency = dependency;
        }

        public string MethodDependency { get; }

        public Type Dependency { get; }

    }
}