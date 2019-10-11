using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTM
{
    class GorevTamamlanmisException : Exception
    {
        public GorevTamamlanmisException(string message) : base(message)
        {
        }
    }
}
