using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartReloc
{
    public class Partition
    {
        private int dir; //dirección de la aprticion
        private Process process; //proceso de la partición
        private bool status; //status de asignado o no

        public Partition()
        {

        }

        public string ProcName //get para process
        {
            get
            {
               return Get_process().Get_user();
            }
        }

        public Partition(int sz, int dr, int inst) //memoria vacía
        {
            var proc = new Process("Null", sz, inst, 5); 
            this.Set_process(proc);
            this.dir = dr;
            this.status = true;
        }

        public Partition(int sz, int dr, string x) //memoria vacía
        {
            var proc = new Process("EMPTY", sz ,999999999, 5); //un proceso infinito llamado Empty (memoria vacía), iniclizamos el bloque different
            this.Set_process(proc);
            this.dir = dr;
            this.status = true;
        }

        public Partition(int sz, int dr, Process prcs)
        {
            this.process = prcs;
            this.Get_process().Set_size(sz);
            this.dir = dr;
            this.status = false;
        }

        public void Set_dir(int arg)
        {
            this.dir = arg;
        }

        public void Set_process (Process arg)
        {
            this.process = arg;
        }

        public void Set_status (bool arg)
        {
            this.status = arg;
        }

        public int Get_dir()
        {
            return this.dir;
        }

        public int Get_end_dir() //obtenemos la dirección de final
        {
            return this.dir + this.Get_process().Get_size(); //incrementamos la dirección
        }

        public Process Get_process()
        {
            return this.process;
        }

        public bool Get_status()
        {
            return this.status;
        }

        public Partition OS() //la partición será el OS 
        {
            var proc = new Process("OS", 10 ,999999999, 5); //un proceso infinito llamado OS
            this.Set_process(proc);
            this.dir = 0;
            this.status = false;

            return this;
        }

    }
}
