using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace PartReloc
{
    public partial class Form1 : Form
    {
        private List<Partition> toDelete, toCompact; //Lista donde se almacenarán las particiones a borrar
        private List<Partition> memory; //Lista que funcionará como memoria
        private BindingList<Process> Failed; //lista con los procesos que no pudieron ser alojados aún después de la compactacion
        private BindingList<QueueObj> queue, queue2, queue3; //Lista que funcionará como cola de procesos
        private List<QueueObj> queuetoDelete, queue2toDelete, queue3toDelete, queuetoMove, queue2toMove; //Lista donde se almacenarán los procesos a borrar
        public int memorysize; //tamaño de la memoria
        public int fallido;
        public int numProcesos;
        public int exetime;
        public Int64 totalexetime;
        private long memorymngtime = 0;

        public Form1()
        {
            InitializeComponent(); //inicialización de los componentes (Form)
            Form2 frm = new Form2(this); //llamada a la Form2, ventana emergente donde se pide el tamaño de la memoria
            frm.ShowDialog(); //se muestra la ventana emergente
            memory = new List<Partition>(); //se inicializa la lista de memoria
            queue = new BindingList<QueueObj>(); //se inicializa la lista de cola
            queue2 = new BindingList<QueueObj>(); //se inicializa la lista de la segunda cola
            queue3 = new BindingList<QueueObj>(); //se inicializa la lista de la tercera cola
            toDelete = new List<Partition>(); //se inicializa la lista de particiones a borrar
            toCompact = new List<Partition>(); //se inicializa la lista de particiones a compactar
            queuetoDelete = new List<QueueObj>(); //se inicializa la lista de procesos a borrar de cola 1
            queue2toDelete = new List<QueueObj>(); //se inicializa la lista de procesos a borrar de cola 2
            queue3toDelete = new List<QueueObj>();
            queuetoMove = new List<QueueObj>();
            queue2toMove = new List<QueueObj>();
            Failed = new BindingList<Process>();
            StartingMemory(); //método que nos permite hacer la inicialización de la memoria, esto es, incluir las dos particiones iniciales, SO y EMPTY
            Update_Memory();
            listBox1.DataSource = queue2;
            listBox1.DisplayMember = "procname";
            listBox2.DataSource = queue; //vinculamos el cuadro de Cola de usuarios con la lista que funcionará como cola
            listBox2.DisplayMember = "procname";
            listBox3.DataSource = queue3;
            listBox3.DisplayMember = "procname";
            listBox4.DataSource = Failed;
            listBox4.DisplayMember = "PrName";
            listView1.Columns.Add("Partición");  //Insertaremos las respectivas columnas en la tabla de particiones
            listView1.Columns.Add("Tamaño");
            listView1.Columns.Add("Dir. Inicio");
            listView1.Columns.Add("Proceso");
            listView1.Columns.Add("Estado");
            listView3.Columns.Add("Partición");
            listView3.Columns.Add("Tamaño");
            listView3.Columns.Add("Dir.Inicio");
            listView3.Columns.Add("Estado");
            listView2.Columns[0].Width = 0;
            listView2.Columns[1].Width = 207;
            Update_Table(); //cargamos la tabla de particiones con lo encontrado en memoria

        }

        public void StartingMemory() //método que nos permitirá inicializar la memoria
        {
            var os = new Partition(); //partición de sistema operativo
            memory.Add(os.OS()); //agregamos el S0, método de partition
            this.memorysize -= os.Get_process().Get_size(); //reducimos el tamaño de memoria al remanente de inicializar el OS
            var empty = new Partition(this.memorysize, memory[0].Get_end_dir(), ""); //creamos una partición vacía y la igualamos con la memoria remanente
            memory.Add(empty); //añadimos la partición vacía a memoria
        }

        private int Size_Calc() // método que obtiene la dirección de la partición vacío 
        {
            var lastinlist = memory[memory.Count - 1]; //accedemos al último elemento
            return lastinlist.Get_dir(); //regresamos su dirección
        }

        private void New_Alloc (Process proc) //método que nos permitirá alojar nuevas particiones en memoria
        {
            var dummypart = new Partition(proc.Get_size(), Size_Calc(), proc); //creamos una nueva partición
            int placeholder = 0, tam = 0, dir = 0;
            foreach ( var element in memory)
            {
                if (element.Get_status() && element.Get_process().Get_size() >= proc.Get_size()) //esto es el algoritmo first-fit, encontraremos el primer elemento que sea de tamaño mayor o igual al proceso a insertar
                {
                    dir = element.Get_dir();
                    tam = element.Get_process().Get_size() - proc.Get_size();
                    placeholder = memory.IndexOf(element);
                    break;
                }

            }
            if(placeholder == 0 )
            {
                var tem = new QueueObj(proc);
                queue2.Add(tem);
                queue.Remove(tem);
                    //hacer nada o mover proceso a otra cola para mayor prioridad
            }
            else if(placeholder != 0 )
         
            {
                dummypart.Set_dir(dir);
                var temp = new Partition(tam, dummypart.Get_end_dir(), 0);
                temp.Set_status(true);
                memory.RemoveAt(placeholder);
                memory.Insert(placeholder, dummypart);
                if (tam != 0)
                    memory.Insert(placeholder + 1, temp);
            }
            /*
            var lastinlist = memory[memory.Count - 1]; //obtenemos la partición vacía
            lastinlist.Get_process().Set_size(lastinlist.Get_process().Get_size() - proc.Get_size()); //Modificamos el tamaño de la localidad vacía, se le resta el tamaño de la nueva localidad creada
            lastinlist.Set_dir(dummypart.Get_end_dir()); //modificamos la dirección de la memoria vacía, ahora está en donde termina la partición introducida
            memory.RemoveAt(memory.Count - 1); //eliminamos el último elemento de la lista, es decir, la partición vacía
            memory.Add(dummypart); //añadimos la nueva partición
            memory.Add(lastinlist); //volvemos a incluir la partición vacía
            this.Update_Table(); // actualizamos la tabla de particiones
            button1.Enabled = true; //activamos el botón para dar otro paso
            */
            this.Update_Table(); // actualizamos la tabla de particiones
            this.Update_Memory();
            button1.Enabled = true; //activamos el botón para dar otro paso
            button4.Enabled = true;

        }

        private void Compaction()
        {
            int totalSize = 0;
            foreach (var element in memory)
            {
                if (element.Get_status())
                {
                    toCompact.Add(element);
                    totalSize += element.Get_process().Get_size();
                }
            }
            foreach (var element in toCompact)
            {
                var index = memory.IndexOf(element);
                for (int i = index+1; i<memory.Count; i++) //ciclo for para recorrer todos los elementos después de este encontrado
                {
                    memory.ElementAt(i).Set_dir( memory.ElementAt(i).Get_dir() - element.Get_process().Get_size()); //modificará su dirección, esto es lo que realiza la relocalización
                }
                memory.Remove(element);
            }
            toCompact.Clear();
            if(totalSize !=0)
            {
                var dir = memory.ElementAt(memory.Count - 1).Get_end_dir();
                var dummy = new Partition(totalSize, dir, 0);
                memory.Add(dummy);
            }

        }

        private void Update_Table() //Método para actualizar la tabla de particiones
        {
            listView1.Items.Clear(); //limpiamos la tabla de particiones
            listView3.Items.Clear(); //limpiamos la tabla de áreas libres
            int i = 1; //nos servirá para llevar el número de la partición
            foreach (var element in memory) //para cada elemento (Partition) de la memoria
            {
                ListViewItem itempart, itemlib;
                bool status = element.Get_status(); //obtenemos su estatus
                string dum; //variable dummy
                if (!status)//esto nos servirá para mostrar si la partición está asignada
                {
                    dum = "Asig";
                     itempart = new ListViewItem(new[] { i.ToString(), element.Get_process().Get_size().ToString(),
                                                     element.Get_dir().ToString(), element.Get_process().Get_user(), dum});
                    itemlib = new ListViewItem(new[] { i.ToString(), "---", "---", dum });
                }
                else
                {
                    dum = "Disp";
                    itempart = new ListViewItem(new[] { i.ToString(), "---", "---", "---", dum});
                    itemlib = new ListViewItem(new[] { i.ToString(), element.Get_process().Get_size().ToString(),
                                                     element.Get_dir().ToString(), dum});
                }
                     //igualación para cargar un array de strings como un objeto listviewitem
                listView1.Items.Add(itempart); //añadiendo items organizados en columnas a la tabla de particiones
                listView3.Items.Add(itemlib); //añadiendo items organizados en columnas a la tabla de áreas libres
                i++; //incrementamos el índice
            }
        }

        private void Update_Memory()
        {
            listView2.Items.Clear();
            foreach(var element in memory)
            {
                ListViewItem item = new ListViewItem(new[] { null, element.Get_process().Get_size().ToString()});
                if (!element.Get_status()) //esto nos servirá para mostrar si la partición está asignada
                    item.SubItems[0].BackColor = Color.BlueViolet;
                else item.SubItems[0].BackColor = Color.Cyan;
                listView2.Items.Add(item);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form1 NewForm = new Form1();
            NewForm.Show();
            this.Dispose(false);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        
        public int memorysz //get y setter para el valor de la memoria, usado por Form2
        {
            get { return this.memorysize; } //getter
            set { this.memorysize = value; }//setter
        }



        private void button3_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(textBox4.Text)) //verificamos que las entradas no sean nulas
            {
                MessageBox.Show("Ingrese el número de procesos a simular");
                return;
            } 

            int.TryParse(textBox4.Text, out numProcesos);

            exetime = (10 * numProcesos) * 11; //falta algo, considerar tamaño de memoria quizá...
            totalexetime = exetime;
            //Debug.WriteLine(partition.Get_process().Get_duration_time()); //obtenemos el tiempo de duración
            Stopwatch s2 = new Stopwatch();
            s2.Start();
            while (s2.Elapsed < TimeSpan.FromMilliseconds(totalexetime))
                {
                    button1_Click(null, null);
                    totalexetime += memorymngtime;
                    memorymngtime = 0;
                }
            s2.Stop();
            fallido = Failed.Count;
            textBox5.Text = fallido.ToString();
            /*var stati = new FormStats(); //reemplazar FormStats 
            stati.Show(this);
            */
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(textBox4.Text)) //verificamos que las entradas no sean nulas
            {
                MessageBox.Show("Ingrese el número de procesos a simular");
                return;
            }

            button1_Click(null, null);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox4.Text)) //verificamos que las entradas no sean nulas
            {
                MessageBox.Show("Ingrese el número de procesos a simular");
                return;
            }

            int.TryParse(textBox4.Text, out int numProcesos);
                
            if ( numProcesos > 10000) //verificamos que las entradas no sean nulas
            {
                MessageBox.Show("Excede la cantidad máxima de procesos (10000)");
                return;
            }


            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            string posibles = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            int longitud = posibles.Length;
            char letra;
            int longitudnuevacadena = 3;

            for (int j = 0; j < numProcesos; j++)
            {
                string nuevacadena = "";
                for (int i = 0; i < longitudnuevacadena; i++)
                {
                    letra = posibles[rnd.Next(longitud)];
                    nuevacadena += letra.ToString();
                }


                var proc = new Process(nuevacadena, decimal.ToInt32(rnd.Next(40, 400)), decimal.ToInt32(rnd.Next(10000, 50000)), decimal.ToInt32(rnd.Next(1, 5))); //creamos un nuevo proceso con los datos introducidos
                var result = memory.Find(x => x.Get_process().Get_size() >= proc.Get_size() && x.Get_status() == true);
                if (result != null) //si hay suficiente memoria disponible
                {
                    New_Alloc(proc); //Alojamos en memoria el nuevo proceso
                }
                else
                {
                    var tem = new QueueObj(proc);
                    queue.Add(tem); //lo metemos a la cola
                }
                Update_Memory();
  
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form3 formulario = new Form3(this);
            formulario.Show();
        }

        public int Succ  //número de procesos éxitosos
        {
            get
            {
                return this.numProcesos - this.fallido;
            }
        }

        public int TotalProcesos //número de procesos totales
        {
            get
            {
                return this.numProcesos;
            }
        }

        public int ExecTime  //tiempo de ejecución
        {
            get
            {
                return Convert.ToInt32(this.totalexetime);
            }
        }
        private void End_Proc (int index) //terminamos el proceso cuyo tiempo ha suspirado
        {
            memory.ElementAt(index).Set_status(true); //obtenemos el objeto cuya ejecución ha terminado y ponemos la partición como disponible
            
            /*for (int i = index+1; i<memory.Count; i++) //cuclo for para recorrer todos los elementos después de este encontrado
            {
                memory.ElementAt(i).Set_dir( memory.ElementAt(i).Get_dir() - dummy.Get_process().Get_size()); //modificará su dirección, esto es lo que realiza la relocalización
            }
            memory.ElementAt(memory.Count - 1).Get_process().Set_size(dummy.Get_process().Get_size() + memory.ElementAt(memory.Count - 1).Get_process().Get_size()); // añadir el tamaño de la memoria borrada a la memoria vacía
            */ //indicamos que la partición no está ocupada más
            }

        private void button2_Click(object sender, EventArgs e) //cuando se detecte el click del botón Añadir, esto ocurrirá
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(textBox3.Text)) //verificamos que las entradas no sean nulas
            {
                MessageBox.Show("Asegúrese de llenar todos los campos");
                return;
            }
            if (textBox2.Text == "0" || textBox3.Text == "0") //verificamos que el tiempo ni tamaño sean cero
            {
                MessageBox.Show("No puede haber procesos de tamaño ni tiempo nulo");
                return;
            }
            int.TryParse(textBox2.Text, out int time); //convertimos a int tiempo y tamaño
            int.TryParse(textBox3.Text, out int tamaño);
            //Debug.WriteLine(tamaño);
            //Debug.WriteLine(this.memorysize);
            if (tamaño > this.memorysize ) //si se intenta alojar una partición más grande que la memoria
            {
                MessageBox.Show("Se está intentando alojar un proceso de tamaño mayor a la memoria disponible");
                return;
            }
            var proc = new Process(textBox1.Text, tamaño, time, decimal.ToInt32(numericUpDown1.Value)); //creamos un nuevo proceso con los datos introducidos
            var result = memory.Find(x => x.Get_process().Get_size() >= proc.Get_size() && x.Get_status() == true);
            if (result != null) //si hay suficiente memoria disponible
            {
                New_Alloc(proc); //Alojamos en memoria el nuevo proceso
            }
            else
            {
                var tem = new QueueObj(proc);
                queue.Add(tem); //lo metemos a la cola
            }
            Update_Memory();
            textBox1.Text = textBox2.Text = textBox3.Text = null; //limpiamos los cuadros de texto
            decimal.TryParse("1", out decimal dumm); //limpiamos el numericalupdown
            numericUpDown1.Value = dumm; //limpiamos el numericalupdown
        }

        private void CPU_cycle() //click del botón Siguiente Movimiento
        {
            foreach (var partition in memory)
            {

                if (!partition.Get_status())
                {
                    //Debug.WriteLine(partition.Get_process().Get_duration_time()); //obtenemos el tiempo de duración                        
                    //Debug.WriteLine(partition.Get_process().Get_duration_time());
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    while (s.Elapsed < TimeSpan.FromMilliseconds(5))
                    {
                        if (partition.Get_process().Get_duration_time() > 0)
                            partition.Get_process().Set_duration_time(partition.Get_process().Get_duration_time() - 1); //se reducirá en uno la duración durante 50 ms
                    }
                    s.Stop();
                    //Debug.WriteLine(partition.Get_process().Get_duration_time());
                }
                if (partition.Get_process().Get_duration_time() <= 0 && !partition.Get_status()) //si el tiempo de ejecución se ha acabado
                {
                    End_Proc(memory.IndexOf(partition)); //terminar proceso
                    //toDelete.Add(partition); //agregamos partition a la lsita de elementos por borrar
                    //Debug.WriteLine("DetectedEnd!"); //linea en consola 
                }
            }
            //foreach (var element in toDelete)
            //{
                //memory.Remove(element); //eliminamos elemento de memoria
                //memory.ElementAt(memory.IndexOf(element)).Set_status(false); //marcamos la partición como disponible
                this.Update_Table(); //actualizamos la tabla de particiones
                this.Update_Memory();
            //}
            //toDelete.Clear(); //limpiamos la tabla de particiones
            //if (memory.Count == 2 && queue.Count ==0) button1.Enabled = false; //si existen sólo dos elementos en memoria (OS y EMPTY) y la pila está vacía, deshabilitamos el botón de siguente

        }

        private void button1_Click(object sender, EventArgs e) //Boton de siguiente
        {
            Stopwatch chen = new Stopwatch();
            chen.Start();
            //Debug.WriteLine("IntoButton!");
            if (queue3.Count != 0) //si la pila no está vacía
            {
                List<QueueObj> SortedList = queue3.OrderByDescending(o => o.proce.Priority).ToList(); //ordenamos la lista en orden descendiente por prioridad, función lambda
                foreach (var element in SortedList) //Para cada elemento en la lista ordenada
                {
                    var result = memory.Find(x => x.Get_process().Get_size() >= element.proce.Get_size() && x.Get_status() == true);
                    if (result != null) //si el elemento es lo sufucientemente grande...
                    {
                        //element.Set_size(memory.ElementAt(memory.Count-1).Get_dir());
                        New_Alloc(element.proce); //alojamos la partición
                        queue3toDelete.Add(element); //eliminamos el elemento de la pila
                    }
                }
                foreach (var element in queue3toDelete)
                {
                    queue3.Remove(element); //eliminamos los procesos que ya no están en cola
                }
                queue3toDelete.Clear();
                if (queue3.Count != 0)
                {
                    this.Compaction();
                    List<QueueObj> SortedListFin = queue3.OrderByDescending(o => o.proce.Priority).ToList();
                    foreach (var element in SortedListFin) //Para cada elemento en la lista ordenada
                    {
                        var result = memory.Find(x => x.Get_process().Get_size() >= element.proce.Get_size() && x.Get_status() == true);
                        if (result != null) //si el elemento es lo sufucientemente grande...
                        {
                            //element.Set_size(memory.ElementAt(memory.Count-1).Get_dir());
                            New_Alloc(element.proce); //alojamos la partición
                            queue3toDelete.Add(element); //eliminamos el elemento de la pila
                        }
                    }
                    foreach (var element in queue3toDelete)
                    {
                        queue3.Remove(element); //eliminamos los procesos que ya no están en cola
                    }
                    foreach (var element in queue3)
                    {
                        element.cycl += 1;
                        if (element.cycl == 3) Failed.Add(element.proce);
                    }
                }
                queue3toDelete.Clear();

            }
            if (queue2.Count != 0) //si la pila no está vacía
            {
                List<QueueObj> SortedList = queue2.OrderByDescending(o => o.proce.Priority).ToList(); //ordenamos la lista en orden descendiente por prioridad, función lambda
                foreach (var element in SortedList) //Para cada elemento en la lista ordenada
                {
                    var result = memory.Find(x => x.Get_process().Get_size() >= element.proce.Get_size() && x.Get_status() == true);
                    if (result != null) //si el elemento es lo sufucientemente grande...
                    {
                        //element.Set_size(memory.ElementAt(memory.Count-1).Get_dir());
                        New_Alloc(element.proce); //alojamos la partición
                        queue2toDelete.Add(element); //eliminamos el elemento de la pila
                    }
                }
                foreach (var element in queue2toDelete)
                {
                    queue2.Remove(element); //eliminamos los procesos que ya no están en cola
                }
                foreach (var element in queue2)
                {
                    element.cycl += 1;
                    if (element.cycl == 3) queue2toMove.Add(element);
                }
                foreach (var element in queue2toMove)
                {
                    queue2.Remove(element);
                    element.cycl = 0;
                    queue3.Add(element);
                }
                queue2toMove.Clear();
            }
            if (queue.Count!=0) //si la pila no está vacía
            {
                List<QueueObj> SortedList = queue.OrderByDescending(o => o.proce.Priority).ToList(); //ordenamos la lista en orden descendiente por prioridad, función lambda
                foreach (var element in SortedList) //Para cada elemento en la lista ordenada
                {
                    var result = memory.Find(x => x.Get_process().Get_size() >= element.proce.Get_size() && x.Get_status() == true);
                    if (result != null) //si el elemento es lo sufucientemente grande...
                    {
                        //element.Set_size(memory.ElementAt(memory.Count-1).Get_dir());
                        New_Alloc(element.proce); //alojamos la partición
                        queuetoDelete.Add(element); //eliminamos el elemento de la pila
                    }

                }
                foreach (var element in queuetoDelete)
                {
                    queue.Remove(element); //eliminamos los procesos que ya no están en cola
                }
                foreach (var element in queue)
                {
                    element.cycl += 1;
                    if (element.cycl == 5) queuetoMove.Add(element);
                }
                foreach (var element in queuetoMove)
                {
                    queue.Remove(element);
                    element.cycl = 0;
                    queue2.Add(element);
                }
                queuetoMove.Clear();
            }
            chen.Stop();
            memorymngtime = chen.ElapsedMilliseconds;
            CPU_cycle();
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e) //método para evitar entrada de símbolos indeseados
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e) //método para evitar entrada de símbolos indeseados
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e) //método para evitar entrada de símbolos indeseados
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }


    }
}
