using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using System.Linq.Expressions;

namespace ShiftOne.Core.Specifications
{
    public class Specification<T> : ISpecification<T>
        where T : class
    {
        public Specification(Expression<Func<T, bool>>? criteria)
        {
            Criteria = criteria;
        }

        public Expression<Func<T, bool>>? Criteria { get; private set; }
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public List<Func<IQueryable<T>, IQueryable<T>>> IncludeChains { get; } = new();
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDescending { get; private set; }
        public int? Skip { get; private set; }
        public int? Take { get; private set; }

        public void AddInclude(Expression<Func<T, object>> include) => Includes.Add(include);

        public void AddIncludeChain(Func<IQueryable<T>, IQueryable<T>> chain) => IncludeChains.Add(chain);

        public void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) => OrderBy = orderByExpression;

        public void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression) => OrderByDescending = orderByDescExpression;

        public void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
        }

        public void AddCriteria(Expression<Func<T, bool>> newCriteria)
        {
            Criteria = Criteria == null ? newCriteria : Criteria.CombineWithAnd(newCriteria);
        }
    }

    public static class Spec
    {
        public static Specification<T> For<T>(
            Expression<Func<T, bool>>? criteria,
            params Expression<Func<T, object>>[] includes)
            where T : class
        {
            var spec = new Specification<T>(criteria);
            foreach (var include in includes)
            {
                spec.AddInclude(include);
            }

            return spec;
        }

        public static Specification<T> ForChain<T>(
            Expression<Func<T, bool>>? criteria,
            params Func<IQueryable<T>, IQueryable<T>>[] chains)
            where T : class
        {
            var spec = new Specification<T>(criteria);
            foreach (var chain in chains)
            {
                spec.AddIncludeChain(chain);
            }

            return spec;
        }
    }

    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> CombineWithAnd<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(T));
            var left = new ReplaceParameterVisitor(first.Parameters[0], parameter).Visit(first.Body);
            var right = new ReplaceParameterVisitor(second.Parameters[0], parameter).Visit(second.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left!, right!), parameter);
        }

        private sealed class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParameter ? _newParameter : base.VisitParameter(node);
            }
        }
    }
}

