using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csharp.CardanoCLI.Interfaces
{
    public interface ILogger
    {
        public void Log(string message);
        public void Warn(string message);

        public void Err(string message, Exception ex = null);
    }
}
