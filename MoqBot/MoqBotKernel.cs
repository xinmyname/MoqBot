using System;
using Moq;
using Ninject;
using Ninject.Activation;
using Ninject.Activation.Providers;
using Ninject.Planning.Bindings;

namespace MoqBot
{
    public class MoqBotKernel : StandardKernel
    {
        private readonly MockRepository _factory;
        private readonly IProvider _provider;

        public MoqBotKernel()
            : this(MockBehavior.Default)
        {
        }

        public MoqBotKernel(MockBehavior mockBehavior)
            : this(new MockRepository(mockBehavior))
        {
        }

        public MoqBotKernel(MockRepository factory)
        {
            _factory = factory;
            _provider = new MoqBotProvider(factory);
        }

        public Mock<T> Mock<T>(MockBehavior behavior) where T : class
        {
            Mock<T> mock = _factory.Create<T>(behavior);

            Bind<T>().ToConstant(mock.Object);

            return mock;
        }

        public Mock<T> Mock<T>() where T : class
        {
            return Mock<T>(MockBehavior.Default);
        }

        public Mock<T> Stub<T>() where T : class
        {
            return Mock<T>(MockBehavior.Loose);
        }

        public void Verify()
        {
            _factory.Verify();
        }

        public override void Dispose(bool disposing)
        {
            Verify();
            base.Dispose(disposing);
        }

        protected override bool HandleMissingBinding(IRequest request)
        {
            Type service = request.Service;

            var binding = new Binding(service)
                          {
                              ProviderCallback = TypeIsSelfBindable(service)
                                  ? StandardProvider.GetCreationCallback(service)
                                  : ctx => _provider,
                              ScopeCallback = x => null,
                              IsImplicit = true
                          };

            AddBinding(binding);

            return true;
        }

        protected new bool TypeIsSelfBindable(Type service)
        {
            return (((!service.IsInterface && !service.IsAbstract) && (!service.IsValueType && (service != typeof(string)))) && !service.ContainsGenericParameters);
        }
    }
}