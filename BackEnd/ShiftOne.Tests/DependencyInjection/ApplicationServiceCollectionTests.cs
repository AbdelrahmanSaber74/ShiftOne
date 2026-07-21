using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Application.Services.User;
using Microsoft.Extensions.DependencyInjection;

namespace ShiftOne.Tests.DependencyInjection
{
    public class ApplicationServiceCollectionTests
    {
        [Fact]
        public void AddApplicationServices_RegistersUserServiceFacade()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            ShiftOne.Application.ServiceCollection.AddApplicationServices(services);

            var descriptor = Assert.Single(services, service => service.ServiceType == typeof(IUserService));
            Assert.Equal(typeof(UserService), descriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }
    }
}
