using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace evaluacionServicios
{
    public partial class evaluacionServicios : ServiceBase
    {
        public Timer timer = new Timer();
        public bool iteracionInicial;
        public evaluacionServicios()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if(!iteracionInicial)
                timer.Interval = 10000;
            timer.Elapsed += new ElapsedEventHandler(OnTimer);
            timer.Enabled = true;
        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            timer.Interval = 300000;
            iteracionInicial = true;
            timer.Enabled = false;
            InicioEvaluacion();
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteLog("Evaluación de Servicios detenido");
        }
        private void InicioEvaluacion()
        {
            try
            {
                //Si existe el ServicioListas en la máquina
                if (ExisteServicio("ServicioListas"))
                {
                    ServiceController controllerListas = new ServiceController("ServicioListas");
                    if (controllerListas != null && controllerListas.Status == ServiceControllerStatus.Stopped)
                    {
                        WriteLog("Servicio Listas se detuvo");
                        controllerListas.Start();
                    }
                    controllerListas.WaitForStatus(ServiceControllerStatus.Running);
                    controllerListas.Close();
                }
                else
                    WriteLog("No existe el Servicio Listas");
            } catch(Exception ex) { WriteLog("No es posible iniciar el servicio de Listas"); }
            
            try
            {
                //Si existe el Web Service en la máquina
                if (ExisteServicio("Service1"))
                {
                    ServiceController controllerWebService = new ServiceController("Service1");
                    if (controllerWebService != null && controllerWebService.Status == ServiceControllerStatus.Stopped)
                    {
                        WriteLog("Web Service se detuvo");
                        controllerWebService.Start();
                    }
                    controllerWebService.WaitForStatus(ServiceControllerStatus.Running);
                    controllerWebService.Close();
                }
                else
                    WriteLog("No existe el Web Service");
            } catch(Exception ex) { WriteLog("No es posible iniciar el Web Service"); }
        }

        bool ExisteServicio(string nombreServicio)
        {
            return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals(nombreServicio));
        }
        private void WriteLog(string info)
        {
            if (File.Exists(@"C:\temporal\Log.txt"))
            {
                string[] lineas = File.ReadAllLines(@"C:\temporal\Log.txt");
                lineas[0] = $"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}. {info}.\n{lineas[0]}";
                File.Delete(@"C:\temporal\Log.txt");
                File.WriteAllLines(@"C:\temporal\Log.txt", lineas);
            }
            else File.WriteAllText(@"C:\temporal\Log.txt", $"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}. {info}.");
        }
    }
}
