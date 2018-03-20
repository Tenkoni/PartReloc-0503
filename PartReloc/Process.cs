using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartReloc
{
    public class Process
    {
        string user; //nombre del proceso
        int size; //tamaño del proceso
        int duration_time; //tiempo de duraci´n
        int priority; //prioridad 1-5

        public string PrName
        {
            get
            {
                return Get_user(); //obtenemos el nombre de ususario
            }
        }

        public string Priority
        {
            get
            {
                return this.priority.ToString();
            }
        }

        public Process(string usr, int sz, int dr, int pr )
        {
            this.user = usr;
            this.size = sz;
            this.duration_time = dr;
            this.priority = pr;
        }

        public void Set_duration_time(int arg)
        {
            this.duration_time = arg;
        }

        public void Set_process(string arg)
        {
            this.user = arg;
        }

        public void Set_size(int arg)
        {
            this.size = arg;
        }

        public int Get_duration_time()
        {
            return this.duration_time;
        }

        public int Get_size()
        {
            return this.size;
        }

        public string Get_user()
        {
            return this.user;
        }
    }
}
