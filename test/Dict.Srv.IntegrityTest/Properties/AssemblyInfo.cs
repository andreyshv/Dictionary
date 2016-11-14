using Xunit;
using Xunit.Extensions.TestOrdering;

[assembly: TestCaseOrderer(DependencyTestCaseOrderer.Name, DependencyTestCaseOrderer.Assembly)]
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]
[assembly: TestCollectionOrderer(DependencyTestCollectionOrderer.Name, DependencyTestCollectionOrderer.Assembly)]