using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncCache.Specs
{
    public interface ITestable
    {
        string LongRunningProcess(string value);
    }
}
