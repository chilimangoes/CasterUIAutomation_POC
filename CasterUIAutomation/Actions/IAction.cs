using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasterUIAutomation.Actions
{
    public interface IAction
    {
        void Initialize(Dictionary<string, string> parameters);
        void Execute();
    }
}
