using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static MusicApp.Common.ExceptionMessages;

namespace MusicApp.Web.Infrastructure
{
    public static class ServiceCollectionExtencions
    {
        private static readonly string ServiceInterfacePrefix="I";
        private static readonly string ServiceTypeSuffix = "Service";

        public static IServiceCollection AddUserDefinedServices(this IServiceCollection serviceCollection, Assembly serviceAssembly)
        {
            Type[] serviceClasses = serviceAssembly
                .GetTypes()
                .Where(t => !t.IsInterface &&
                                 t.Name.EndsWith(ServiceTypeSuffix))
                .ToArray();
            foreach (Type serviceClass in serviceClasses)
            {
                Type? serviceInterface = serviceClass
                    .GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"{ServiceInterfacePrefix}{serviceClass.Name}");
                if (serviceInterface == null)
                {
                    throw new ArgumentException(InterfaceNotFoundMessage, serviceClass.Name);
                }

                serviceCollection.AddScoped(serviceInterface, serviceClass);
            }

            return serviceCollection;
        }
    }
}
