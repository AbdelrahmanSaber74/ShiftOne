using ShiftOne.Core.Specifications;

namespace ShiftOne.Tests.Specifications
{
    public class SpecificationTests
    {
        [Fact]
        public void AddCriteria_CombinesPredicatesWithAnd()
        {
            var spec = Spec.For<TestItem>(item => item.IsActive);

            spec.AddCriteria(item => item.Score >= 10);

            var predicate = spec.Criteria!.Compile();
            Assert.True(predicate(new TestItem(true, 10)));
            Assert.False(predicate(new TestItem(true, 5)));
            Assert.False(predicate(new TestItem(false, 20)));
        }

        private sealed record TestItem(bool IsActive, int Score);
    }
}
