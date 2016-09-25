using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;

namespace CasterUIAutomation
{
    static class ConditionExtensions
    {
        public static Condition And(this Condition @me, Condition you)
        {
            return new AndCondition(@me, you);
        }

        public static Condition Or(this Condition @me, Condition you)
        {
            return new OrCondition(@me, you);
        }
    }
}
