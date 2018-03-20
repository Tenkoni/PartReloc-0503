using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartReloc
{
    public class QueueObj
    {
        private Process proc;
        private int cycles;

        public QueueObj(Process pr)
        {
            this.proc = pr;
            this.cycles = 0;
        }

        public Process proce //get y setter para el valor de la memoria, usado por Form2
        {
            get { return this.proc; } //getter
            set { this.proc = value; }//setter
        }

        public string procname //get y setter para el valor de la memoria, usado por Form2
        {
            get { return this.proc.Get_user(); } //getter
        }

        public int cycl //get y setter para el valor de la memoria, usado por Form2
        {
            get { return this.cycles; } //getter
            set { this.cycles = value; }//setter
        }

    }
}
