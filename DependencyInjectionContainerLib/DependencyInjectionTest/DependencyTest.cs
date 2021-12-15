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


        //Обычная ситуация
        [TestMethod]
        public void TestDependencyInjection()
        {
            dependencyConfig.Register<IDepend2, TestClass>(ImplementationsTTL.InstancePerDependency);
            DependencyProvider provider = new DependencyProvider(dependencyConfig);
            var res = provider.Resolve<IDepend2>();
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(TestClass));
        }

        // Интерфейс и реализация один и тот же класс класс
        [TestMethod]
        public void TestRegistrationAsSelf()
        {
            dependencyConfig.Register<TestClass, TestClass>(ImplementationsTTL.InstancePerDependency);

            DependencyProvider provider = new DependencyProvider(dependencyConfig);
            var res = provider.Resolve<TestClass>();
            Assert.IsInstanceOfType(res, typeof(TestClass));
        }


        //Вложенный и основной интерфейсы
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

        // Тестирование незарегистрированной зависимости
        // зависимость не регистрируется и получается ее реализация с помощью контейнера
        [TestMethod]
        public void TestNotregisteredType()
        {
            dependencyConfig.Register<IDepend2, TestClass2>(ImplementationsTTL.Singleton);
            DependencyProvider provider = new DependencyProvider(dependencyConfig);
            Action action = delegate () { provider.Resolve<IDepend3>(); };
            Assert.ThrowsException<ArgumentException>(action);

        }

        //Тестовая ситуация – регистрируется зависимость в необходимом фор-мате,
        //после этого дважды получается реализация этой зависимости с
        //помо-щью класса контейнера, c указанным временем жизни – singleton.
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

        // Тестовая ситуация – регистрируется зависимость в необходимом фор-мате,
        // после этого дважды получается реализация этой зависимости с помо-щью класса контейнера,
        // c указанным временем жизни – instance per depend-ency.

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


        //Тестирование получения всех реализаций зависимости
        // Тестовая ситуация – регистрируется несколько реализаций
        // для одного типа зависимости.После этого при получении
        // реализации в контейнер пере-дается IEnumerable<тип зависимости>.


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
