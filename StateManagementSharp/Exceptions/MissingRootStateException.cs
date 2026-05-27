using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagementSharp.Exceptions
{
    public class MissingRootStateException : Exception
    {
        public string StateName { get; }

        public MissingRootStateException(string stateName)
        {
            StateName = stateName;
        }
    }
}
