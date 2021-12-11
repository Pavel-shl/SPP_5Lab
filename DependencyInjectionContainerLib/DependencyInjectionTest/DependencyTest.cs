using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DependencyInjectionContainerLib.Config;
using DependencyInjectionContainerLib;
using System.Collections.Generic;

namespace DependencyInjectionTest
{
    [TestClass]
    public class DependencyTest
    {
        private DependencyConfig dependencyConfig;
        [TestInitialize]
        public void Initialize()
        {
            dependencyConfig = new DependencyConfig();
        }

        [TestMethod]
        public void TestDependencyInjection()
        {
            dependencyConfig.Register<IDepend2, TestClass>(ImplementationsTTL.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(dependencyConfig);
            var res = provider.Resolve<IDepend2>();
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(TestClass));
        }

        // тип зависимости может совпадать с типом реализации
        // иногда это называют регистрацией "as self":
        [TestMethod]
        public void TestRegistrationAsSelf()
        {
            dependencyConfig.Register<TestClass, TestClass>(ImplementationsTTL.InstancePerDependency);

            DependencyProvider provider = new DependencyProvider(dependencyConfig);
            var res = provider.Resolve<TestClass>();
            Assert.IsInstanceOfType(res, typeof(TestClass));
        }

        [TestMethod]
        public void TestNestedDependency()
        {
            dependencyConfig.Register<IDepend2, TestNestedDependencyClass>(ImplementationsTTL.Singleton);
            dependencyConfig.Register<IDepend3, TestClass>(ImplementationsTTL.Singleton);

            DependencyProvider provider = new DependencyProvider(dependencyConfig);

            var res = provider.Resolve<IDepend2>();
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(TestNestedDependencyClass));
            Assert.IsNotNull(((TestNestedDependencyClass)res).Depend3);
            Assert.IsInstanceOfType(((TestNestedDependencyClass)res).Depend3, typeof(TestClass));
        }

        [TestMethod]
        public void TestNotregisteredType()
        {
            dependencyConfig.Register<IDepend2, TestClass2>(ImplementationsTTL.Singleton);
            DependencyProvider provider = new DependencyProvider(dependencyConfig);
            Action action = delegate () { provider.Resolve<IDepend3>(); };
            Assert.ThrowsException<ArgumentException>(action);

        }

        [TestMethod]
        public void TestSingleton()
        {
            dependencyConfig.Register<IDepend3, TestClass>(ImplementationsTTL.Singleton);
            DependencyProvider provider = new DependencyProvider(dependencyConfig);
            var res1 = provider.Resolve<IDepend3>();
            var res2 = provider.Resolve<IDepend3>();
            Assert.IsInstanceOfType(res1, typeof(TestClass));
            Assert.AreSame(res1, res2);
        }

        [TestMethod]
        public void TestInstancePerDependency()
        {
            dependencyConfig.Register<IDepend3, TestClass>(ImplementationsTTL.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(dependencyConfig);
            var res1 = provider.Resolve<IDepend3>();
            var res2 = provider.Resolve<IDepend3>();
            Assert.IsInstanceOfType(res1, typeof(TestClass));
            Assert.AreNotSame(res1, res2);
        }

        [TestMethod]
        public void TestGenericType()
        {
            dependencyConfig.Register<IDepend<IDepend2>, Realization<IDepend2>>(ImplementationsTTL.Singleton);
            dependencyConfig.Register<IDepend2, TestClass>(ImplementationsTTL.Singleton);
            DependencyProvider provider = new DependencyProvider(dependencyConfig);
            var res = provider.Resolve<IDepend<IDepend2>>();
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(Realization<IDepend2>));

        }

        [TestMethod]
        public void TestOpenGenericRealization()
        {
            dependencyConfig.Register<IDepend2, TestClass>(ImplementationsTTL.InstancePerDependency);
            dependencyConfig.Register<IDepend3, TestClass>(ImplementationsTTL.InstancePerDependency);

            dependencyConfig.Register(typeof(IDepend<>), typeof(Realization<>), ImplementationsTTL.InstancePerDependency);

            DependencyProvider provider = new DependencyProvider(dependencyConfig);
            var res = provider.Resolve<IDepend<IDepend3>>();
            Assert.IsInstanceOfType(res, typeof(Realization<IDepend3>));
        }

        [TestMethod]
        public void TestTwoImplementations()
        {
            dependencyConfig.Register<IDepend2, TestClass>(ImplementationsTTL.Singleton);
            dependencyConfig.Register<IDepend2, TestClass2>(ImplementationsTTL.Singleton);

            DependencyProvider provider = new DependencyProvider(dependencyConfig);
            var res = provider.Resolve<IEnumerable<IDepend2>>();
            int count = 0;
            foreach (IDepend2 depend2 in res)
            {
                count++;
            }
            Assert.AreEqual(count, 2);
        }

        [TestMethod]
        public void TestTwoImplementationsWithDifference()
        {
            dependencyConfig.Register<IDepend2, TestClass>(ImplementationsTTL.Singleton);
            dependencyConfig.Register<IDepend2, TestClass2>(ImplementationsTTL.Singleton);

            DependencyProvider provider = new DependencyProvider(dependencyConfig);

            int? test2Numb = provider.GetRealizationNumber(typeof(IDepend2), typeof(TestClass2)),
                test1Numb = provider.GetRealizationNumber(typeof(IDepend2), typeof(TestClass));
            Assert.IsNotNull(test2Numb);
            Assert.IsNotNull(test1Numb);
            var res = provider.Resolve<IDepend2>(test2Numb.Value);
            var res2 = provider.Resolve<IDepend2>(test1Numb.Value);
            Assert.IsInstanceOfType(res, typeof(TestClass2));
            Assert.IsInstanceOfType(res2, typeof(TestClass));
        }

    }

    


    interface IDepend<TDep> where TDep:IDepend2
    {
    
    }
    interface IDepend2
    {

    }
    interface IDepend3 : IDepend2 { }

    class Realization<TDepend2> : IDepend<TDepend2> where TDepend2:IDepend2
    {
        private TDepend2 depend;
        public Realization(TDepend2 depend1)
        {
            depend = depend1;
        }

    }

    class TestClass :IDepend2, IDepend3
    {
        //private IDepend depend;
        //public TestClass(IDepend depend1) 
        //{
        //   depend = depend1;
        // }
    }
    class TestClass2 : IDepend2, IDepend3
    {
        //private IDepend depend;
        //public TestClass(IDepend depend1) 
        //{
        //   depend = depend1;
        // }
    }

    class TestNestedDependencyClass : IDepend2
    {
        public IDepend3 Depend3;
        public TestNestedDependencyClass(IDepend3 depend3)
        {
            Depend3 = depend3;
        }

    }

    class TestNestedEnumClass : IDepend2
    {
        public IEnumerable<IDepend3> Depend3;
        public TestNestedEnumClass(IEnumerable<IDepend3> depend3)
        {
            Depend3 = depend3;
        }

    }



}
