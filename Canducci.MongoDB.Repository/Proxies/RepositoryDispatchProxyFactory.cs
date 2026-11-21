using System;
using System.Reflection;

namespace Canducci.MongoDB.Repository
{
    internal static class RepositoryDispatchProxyFactory
    {
        private static readonly MethodInfo CreateMethod = typeof(RepositoryDispatchProxyFactory)
            .GetMethod(nameof(CreateGeneric), BindingFlags.NonPublic | BindingFlags.Static);

        public static object Create(Type interfaceType, Type entityType, IConnect connect)
        {
            if (interfaceType is null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            if (entityType is null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (connect is null)
            {
                throw new ArgumentNullException(nameof(connect));
            }

            try
            {
                MethodInfo method = CreateMethod.MakeGenericMethod(interfaceType, entityType);
                return method.Invoke(null, new object[] { connect });
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }

        private static TInterface CreateGeneric<TInterface, TEntity>(IConnect connect)
            where TInterface : class
            where TEntity : class, new()
        {
            TInterface proxy = DispatchProxy.Create<TInterface, RepositoryDispatchProxy>();
            RepositoryDispatchProxy dispatchProxy = (RepositoryDispatchProxy)(object)proxy;
            dispatchProxy.Initialize(new RepositoryProxy<TEntity>(connect));
            return proxy;
        }
    }
}
