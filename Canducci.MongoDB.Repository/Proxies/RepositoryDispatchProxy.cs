using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Canducci.MongoDB.Repository
{
    public class RepositoryDispatchProxy : DispatchProxy
    {
        private object _target;

        public RepositoryDispatchProxy()
        {
        }

        internal void Initialize(object target)
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            _target = target;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod is null)
            {
                throw new ArgumentNullException(nameof(targetMethod));
            }

            if (_target is null)
            {
                throw new InvalidOperationException("The repository proxy has not been initialized.");
            }

            try
            {
                return targetMethod.Invoke(_target, args);
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }
    }
}
