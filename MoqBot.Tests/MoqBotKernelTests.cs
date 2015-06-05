using Moq;
using Ninject;
using NUnit.Framework;

namespace MoqBot.Tests
{
    public class MoqBotKernelContext
    {
        public interface IServiceA
        {
            void RunA();
        }

        public interface IServiceB
        {
            void RunB();
        }

        public class ServiceA : IServiceA
        {
            public ServiceA()
            {
            }

            public ServiceA(int count)
            {
                Count = count;
            }

            public ServiceA(IServiceB b)
            {
                ServiceB = b;
            }

            public IServiceB ServiceB { get; private set; }
            public int Count { get; private set; }

            public string Value { get; set; }

            public void RunA() { }
        }

        public class TestComponent
        {
            public TestComponent(IServiceA serviceA, IServiceB serviceB)
            {
                ServiceA = serviceA;
                ServiceB = serviceB;
            }

            public IServiceA ServiceA { get; private set; }
            public IServiceB ServiceB { get; private set; }

            public void RunAll()
            {
                ServiceA.RunA();
                ServiceB.RunB();
            }

            public void RunA()
            {
                ServiceA.RunA();
            }

            public void RunB()
            {
                ServiceB.RunB();
            }
        }
    }

    [TestFixture]
    public class MoqBotKernelTests : MoqBotKernelContext
    {
        [Test]
        public void CreatesLooseMocksIfFactoryIsLoose()
        {
            var kernel = new MoqBotKernel(MockBehavior.Loose);
            var component = kernel.Get<TestComponent>();

            component.RunAll();
        }

        [Test]
        public void DefaultMockBehaviorIsLoose()
        {
            var kernel = new MoqBotKernel();
            var component = kernel.Get<TestComponent>();

            component.RunAll();
        }

        [Test]
        public void CreatesClassUsingActivator()
        {
            var kernel = new MoqBotKernel(MockBehavior.Loose);
            var service = new ServiceA();

            kernel.Bind<TestComponent>().ToMethod(x => new TestComponent(service, kernel.Get<IServiceB>()));
            var component = kernel.Get<TestComponent>();

            Assert.IsNotNull(component);
            Assert.AreSame(service, component.ServiceA);
            Assert.IsNotNull(component.ServiceB);
        }

        [Test]
        public void CanRegisterImplementationAndResolveIt()
        {
            var kernel = new MoqBotKernel(MockBehavior.Loose);
            kernel.Bind<IServiceA>().To<ServiceA>();

            var service = kernel.Get<IServiceA>();

            Assert.IsNotNull(service);
            Assert.IsFalse(service is IMocked<IServiceA>);
        }


        [Test]
        public void CanRegisterImplementationWithDelegateAndResolveIt()
        {
            var kernel = new MoqBotKernel(MockBehavior.Loose);
            kernel.Bind<IServiceA>().ToMethod(r => new ServiceA(5) { Value = "foo" });

            var service = kernel.Get<IServiceA>() as ServiceA;

            Assert.IsNotNull(service);
            Assert.IsFalse(service is IMocked<IServiceA>);
            Assert.AreEqual(5, service.Count);
            Assert.AreEqual("foo", service.Value);
        }

        [Test]
        public void CanRegisterImplementationWithDelegateResolveAndResolveIt()
        {
            var kernel = new MoqBotKernel(MockBehavior.Loose);
            kernel.Bind<IServiceA>().ToMethod(r => new ServiceA(r.Kernel.Get<IServiceB>()));

            var service = kernel.Get<IServiceA>() as ServiceA;

            Assert.IsNotNull(service);
            Assert.IsFalse(service is IMocked<IServiceA>);
            Assert.IsNotNull(service.ServiceB);
            Assert.IsTrue(service.ServiceB is IMocked<IServiceB>);
        }

        [Test]
        public void ResolveUnregisteredImplementationReturnsMock()
        {
            var kernel = new MoqBotKernel(MockBehavior.Loose);

            var service = kernel.Get<IServiceA>();

            Assert.IsNotNull(service);
            Assert.IsTrue(service is IMocked<IServiceA>);
        }

        [Test]
        public void DefaultConstructorWorksWithAllTests()
        {
            var kernel = new MoqBotKernel(MockBehavior.Loose);
            var a = false;
            var b = false;
            kernel.Mock<IServiceA>().Setup(x => x.RunA()).Callback(() => a = true);
            kernel.Mock<IServiceB>().Setup(x => x.RunB()).Callback(() => b = true);

            var component = kernel.Get<TestComponent>();
            component.RunAll();

            Assert.IsTrue(a);
            Assert.IsTrue(b);
        }

        [Test]
        public void ThrowsIfStrictMockWithoutExpectation()
        {
            var kernel = new MoqBotKernel(MockBehavior.Strict);
            kernel.Mock<IServiceB>().Setup(x => x.RunB());

            var component = kernel.Get<TestComponent>();

            Assert.Throws<MockException>(component.RunAll);
        }

        [Test]
        public void StrictWorksWithAllExpectationsMet()
        {
            var kernel = new MoqBotKernel(MockBehavior.Strict);
            kernel.Mock<IServiceA>().Setup(x => x.RunA());
            kernel.Mock<IServiceB>().Setup(x => x.RunB());

            var component = kernel.Get<TestComponent>();
            component.RunAll();
        }


        [Test]
        public void VerifyThrowsIfMockExpectationIsMissing()
        {
            var kernel = new MoqBotKernel();
            kernel.Mock<IServiceB>().Setup(x => x.RunB()).Verifiable();

            var component = kernel.Get<TestComponent>();

            component.RunA();

            Assert.Throws<MockException>(kernel.Verify);
        }

        [Test]
        public void VerifyWorksWithAllExpectationsMet()
        {
            var kernel = new MoqBotKernel();
            kernel.Mock<IServiceA>().Setup(x => x.RunA()).Verifiable();
            kernel.Mock<IServiceB>().Setup(x => x.RunB()).Verifiable();

            var component = kernel.Get<TestComponent>();
            component.RunAll();

            kernel.Verify();
        }

        [Test]
        public void ImplicitVerifyWithUsingWorksWithAllExpectationsMet()
        {
            using (var kernel = new MoqBotKernel())
            {
                kernel.Mock<IServiceA>().Setup(x => x.RunA()).Verifiable();
                kernel.Mock<IServiceB>().Setup(x => x.RunB()).Verifiable();

                var component = kernel.Get<TestComponent>();
                component.RunAll();
            }
        }

    }
}