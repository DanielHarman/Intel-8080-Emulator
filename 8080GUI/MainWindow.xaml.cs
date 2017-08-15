using System;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using _8080Emu;

namespace _8080GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : Window
    {

        public _8080CPU cpu;
        public bool running = false;

        private readonly BackgroundWorker worker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            cpu = new _8080CPU();
            cpu.memory[0x0] = 0b00111110;
            cpu.memory[0x1] = 0xFF;
            cpu.memory[0x2] = 0b00111110;
            cpu.memory[0x3] = 0xEE;
            cpu.memory[0x4] = 0b01000111;
            cpu.memory[0xff] = 0b11000011;

            updateDisplay();


        }


        public void updateDisplay()
        {
            reg_a_box.Text = String.Format("0x{0}", cpu.reg_a.ToString("X2"));
            reg_b_box.Text = String.Format("0x{0}", cpu.reg_b.ToString("X2"));
            reg_c_box.Text = String.Format("0x{0}", cpu.reg_c.ToString("X2"));
            reg_d_box.Text = String.Format("0x{0}", cpu.reg_d.ToString("X2"));
            reg_e_box.Text = String.Format("0x{0}", cpu.reg_e.ToString("X2"));
            reg_h_box.Text = String.Format("0x{0}", cpu.reg_h.ToString("X2"));
            reg_l_box.Text = String.Format("0x{0}", cpu.reg_l.ToString("X2"));
            txtMem.Text = String.Format("0x{0}", cpu.memory[cpu.programCounter].ToString("X2"));
            prog_box.Text = String.Format("0x{0}", cpu.programCounter.ToString("X4"));
        }

        private void btnstep_Click(object sender, RoutedEventArgs e)
        {
            cpu.execute();
            updateDisplay();
        }

        private void btnrun_Click(object sender, RoutedEventArgs e)
        {
            running = true;
        
            worker.RunWorkerAsync();

        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run all background tasks here
            while (cpu.running)
            {
                cpu.execute();
                this.Dispatcher.Invoke(() =>
                { updateDisplay(); });
            }
        }

        private void worker_RunWorkerCompleted(object sender,
                                               RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work
        }

    }
}
