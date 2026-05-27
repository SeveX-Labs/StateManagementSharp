using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagementSharp.Exceptions
{
    public class MissingStateException : Exception
    {
        public string StateName { get; }

        public MissingStateException(string stateName)
        {
            StateName = stateName;
        }
    }
}
