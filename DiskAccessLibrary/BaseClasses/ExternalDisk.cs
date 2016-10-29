using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISCSIDisk
{
    public abstract class ExternalDisk : Disk
    {
        public virtual void SetParameters(object parameters)
        {
        }
    }
}
