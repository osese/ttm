using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTM
{
    class CurrentTask
    {
        public List<Func<int>> funclist = new List<Func<int>>();

        public void SetTask(DataSet.TasksRow row)
        {
            foreach(var f in funclist)
            {
                f();
            }
        }

        public void AddF(Func<int> f)
        {
            if (!funclist.Contains(f))
                funclist.Add(f);
        }
    }
}
