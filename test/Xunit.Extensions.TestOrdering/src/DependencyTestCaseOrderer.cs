using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.TestOrdering
{
    public sealed class DependencyTestCaseOrderer : ITestCaseOrderer
    {
        public const string Name = "Xunit.Extensions.TestOrdering." + nameof(DependencyTestCaseOrderer);
        public const string Assembly = "Xunit.Extensions.TestOrdering"; // , Version=1.0.0.0

        public DependencyTestCaseOrderer(IMessageSink diagnosticMessageSink)
        {
            //DiagnosticMessage("Create DependencyTestCaseOrderer");
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            return DependencySorter
                .Sort(testCases.Select(x => new TestCaseWrapper(x)))
                .Select(x => x.TestCase)
                .Cast<TTestCase>();
        }

        private static IMessageSink _diagnosticMessageSink;
        private static void DiagnosticMessage(string msg, params object[] arg)
        {
            _diagnosticMessageSink?.OnMessage(new DiagnosticMessage(msg, arg));
        }

        private sealed class TestCaseWrapper : IDependencyIndicator<TestCaseWrapper>
        {
            public TestCaseWrapper(ITestCase testCase)
            {
                this.TestCase = testCase;

                var attributeInfo =
                    testCase.TestMethod.Method.GetCustomAttributes(typeof(DependOnAttribute))
                        .OfType<ReflectionAttributeInfo>()
                        .SingleOrDefault();

                if (attributeInfo != null)
                {
                    //DependencyTestCaseOrderer.DiagnosticMessage("DependOn attibute found for: {0}", testCase.TestMethod.Method.Name);
                    var attribute = (DependOnAttribute)attributeInfo.Attribute;

                    this.TestMethodDependency = attribute.MethodDependency;
                }
                else
                {
                    //DependencyTestCaseOrderer.DiagnosticMessage("No DependOn attibute found for: {0}", testCase.TestMethod.Method.Name);
                }
            }

            public string TestMethod => this.TestCase.TestMethod.Method.Name;
            public string TestMethodDependency { get; }

            public ITestCase TestCase { get; }

            public bool IsDependencyOf(TestCaseWrapper other)
            {
                if (other.TestMethodDependency == null)
                {
                    return false;
                }

                return string.Equals(this.TestMethod, other.TestMethodDependency, StringComparison.Ordinal);
            }

            public bool HasDependencies => !String.IsNullOrEmpty(this.TestMethodDependency);

            public bool Equals(TestCaseWrapper other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(this.TestCase, other.TestCase);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is TestCaseWrapper && this.Equals((TestCaseWrapper)obj);
            }

            public override int GetHashCode()
            {
                return this.TestCase?.GetHashCode() ?? 0;
            }

            public override string ToString() => this.TestCase.DisplayName;
        }
    }
}