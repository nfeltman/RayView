using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Topaz
{
    public class TopazException : Exception
    {
        public TopazException(string reason, params object[] args)
            : base(String.Format(reason,args))
        {

        }
    }
}
