using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionContainerLib.Config
{
    public class DependencyConfig 
    {
        private Dictionary<Type, List<InterfaceImplementation>> Dependencies { get; set; } = new Dictionary<Type, List<InterfaceImplementation>>();


        public void Register<TDependency, TImpl>(ImplementationsTTL time)
            where TDependency : class
            where TImpl : TDependency
        {
            Register(typeof(TDependency), typeof(TImpl), time);

        }

        public void Register(Type dependencyType, Type implementationType, ImplementationsTTL time)
        {
            //generic
            if (dependencyType.IsGenericTypeDefinition && implementationType.IsGenericTypeDefinition) 
            {
                if (dependencyType.GetGenericArguments().Length== implementationType.GetGenericArguments().Length) 
                {
                  
                    var dep = dependencyType.GetGenericArguments();
                    var imp = implementationType.GetGenericArguments();
                    bool IsAssignable = true;
                    for (int i = 0; i < dep.Length; i++) 
                    {
                        var depContr = dep[i].GetGenericParameterConstraints();
                        var inpContr = imp[i].GetGenericParameterConstraints();
                        if (depContr.Length != inpContr.Length) 
                        {
                            IsAssignable = false;
                        }
                        for (int j = 0; j< depContr.Length && IsAssignable; j++) 
                        {
                            IsAssignable = depContr[i].IsAssignableFrom(inpContr[i]);
                        }
                    
                    }
                    if (IsAssignable) 
                    {
                        if (!Dependencies.ContainsKey(dependencyType))
                        {
                            Dependencies.Add(dependencyType, new List<InterfaceImplementation>());
                        }
                        var tmp = Dependencies[dependencyType].Where((InterfaceImplementation impl) => (impl.Type == implementationType)) ;
                        if (tmp.Count() == 0)
                        {
                            Dependencies[dependencyType].Add(new InterfaceImplementation(implementationType, time, Dependencies[dependencyType].Count));
                        }

                        return;
                    }

                }
            
            
            }
            
            if (!dependencyType.IsAssignableFrom(implementationType))
            {
                throw new ArgumentException("Incompatible parameters type");
            }


            if (!Dependencies.ContainsKey(dependencyType))
            {
                Dependencies.Add(dependencyType, new List<InterfaceImplementation>());
            }
            var l = Dependencies[dependencyType].Where((InterfaceImplementation imp) => (imp.Type == implementationType));
            if (l.Count() == 0)
            {
                Dependencies[dependencyType].Add(new InterfaceImplementation(implementationType, time, Dependencies[dependencyType].Count));
            }
        
        }

        internal List<InterfaceImplementation> GetImplementationInfo(Type type, int ind)
        {
            if (Dependencies.ContainsKey(type))
            {
                if (ind >= 0)
                {
                    var res = Dependencies[type].Where((InterfaceImplementation im) => (im.Index == ind));
                    List<InterfaceImplementation> dependencies = new List<InterfaceImplementation>();
                    foreach (InterfaceImplementation interfaceImplementation in res)
                    {
                        dependencies.Add(interfaceImplementation);
                    }
                    return dependencies;
                }
                    return Dependencies[type];
            }
            else if (type.IsGenericType) 
            {
                var list =new List<InterfaceImplementation>(); ;
                if (Dependencies.ContainsKey(type.GetGenericTypeDefinition())) 
                {
                    var res = Dependencies[type.GetGenericTypeDefinition()];
                    var resElement = res.FirstOrDefault();
                    InterfaceImplementation implementation = new InterfaceImplementation( resElement.Type.MakeGenericType(type.GetGenericArguments()), resElement.TTL);

                    list.Add(implementation);
                }
                return list;

            }
            else
            {
                return new List<InterfaceImplementation>();
            }

        }



    }
}
