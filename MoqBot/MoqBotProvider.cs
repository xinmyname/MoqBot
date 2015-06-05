using System;
using System.Collections.Generic;
using System.Reflection;
using Moq;
using Ninject.Activation;

namespace MoqBot
{
    public class MoqBotProvider : IProvider
    {
        private readonly MockRepository _factory;
        private readonly Dictionary<Type, MethodInfo> _creators = new Dictionary<Type, MethodInfo>();

        public MoqBotProvider(MockRepository factory)
        {
            _factory = factory;
        }

        public object Create(IContext context)
        {
            MethodInfo mockCreateMethod;
            Type service = context.Request.Service;

            lock (_creators)
            {
                if (_creators.ContainsKey(service))
                    mockCreateMethod = _creators[service];
                else
                {
                    MethodInfo method = _factory.GetType().GetMethod("Create", Type.EmptyTypes);
                    mockCreateMethod = method.MakeGenericMethod(service);

                    _creators[service] = mockCreateMethod;
                }
            }

            return ((Mock)mockCreateMethod.Invoke(_factory, null)).Object;
        }

        public Type Type
        {
            get { return typeof(Mock<>); }
        }


    }
}