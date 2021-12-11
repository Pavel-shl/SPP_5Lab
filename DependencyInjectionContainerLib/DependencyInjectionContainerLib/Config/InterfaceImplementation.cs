using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionContainerLib.Config
{
    class InterfaceImplementation
    {
        public Type Type { get; private set; }
        public ImplementationsTTL TTL { get; private set; }
        public int Index { get; internal set; }
        public InterfaceImplementation(Type type, ImplementationsTTL tTL)
        {
            TTL = tTL;
            Type = type;
        }
        public InterfaceImplementation(Type type, ImplementationsTTL tTL, int ind)
        {
            TTL = tTL;
            Type = type;
            Index = ind;
        }

    }

}
