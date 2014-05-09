using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _0x10c
{
    class Executor
    {
        public static StringBuilder Output { get; private set; }

        public static int Read()
        {
            Output = new StringBuilder();
            var form = new InputForm();
            form.ShowDialog();
            Output.AppendFormat("Input value: {0}\n", form.Value);
            return form.Value;
        }

    }



}
