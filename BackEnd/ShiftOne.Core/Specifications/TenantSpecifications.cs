using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using System.Linq.Expressions;

namespace ShiftOne.Core.Specifications
{
    public static class TenantSpecifications
    {
        public static Specification<T> ForCurrentCompany<T>(ITenantContext tenantContext)
            where T : class
        {
            return Spec.For<T>(CompanyCriteria<T>(tenantContext.RequireCompanyId()));
        }

        public static Specification<T> ForCompany<T>(Guid companyId)
            where T : class
        {
            return Spec.For<T>(CompanyCriteria<T>(companyId));
        }

        public static Expression<Func<T, bool>> CompanyCriteria<T>(Guid companyId)
            where T : class
        {
            var parameter = Expression.Parameter(typeof(T), "entity");
            var property = Expression.PropertyOrField(parameter, "CompanyId");
            var company = Expression.Constant(companyId);
            Expression body;

            if (property.Type == typeof(Guid?))
            {
                body = Expression.Equal(property, Expression.Convert(company, typeof(Guid?)));
            }
            else if (property.Type == typeof(Guid))
            {
                body = Expression.Equal(property, company);
            }
            else
            {
                throw new InvalidOperationException($"{typeof(T).Name}.CompanyId must be Guid or nullable Guid.");
            }

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
}