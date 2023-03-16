using System;
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
using EngineIO;

namespace lumiere
{
   

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool ft1 = false;
        private bool ft2 = false;
        private bool X1 = true;
        private bool X2 = false;
        private bool front = false;
        private bool bpPrec = false;
        private bool bp;

        private MemoryBit lampe;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(20);
            dispatcherTimer.Start();

            //conditions initiales
            this.X1 = true;
            this.X2 = false;
            this.bpPrec = false;
            this.lampe = MemoryMap.Instance.GetBit(0, MemoryType.Output);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.runCycleApi();
            this.runCycleApiStores();
            this.runPorteGarage();
        }

        private bool bp1 = false;
        private bool bp1prec = false;
        private bool capteurIR = false;
        private bool porteOuverte = false;
        private bool porteFermee = false;
        private bool frontBp1 = false;


        private bool X1g = true;
        private bool X2g = false;
        private bool X3g = false;
        private bool X3gprec = false;
        private bool frontX3g = false;
        private bool X4g = false;
        private bool X5g = false;
        private bool ft1g = false;
        private bool ft2g = false;
        private bool ft3g = false;
        private bool ft4g = false;
        private bool ft5g = false;
        private bool ft6g = false;
        private bool finTempo = false;
        private System.Windows.Threading.DispatcherTimer dispatcherTimer5s;

        private void runPorteGarage()
        {
            //sauvegarde de l'état précédent
            bp1prec = bp1;
            X3gprec = X3g;

            //timer
            

            //lecture des entrées
            bp1 = MemoryMap.Instance.GetBit(274, MemoryType.Input).Value;
            capteurIR = MemoryMap.Instance.GetBit(102, MemoryType.Input).Value;
            porteOuverte = MemoryMap.Instance.GetBit(100, MemoryType.Input).Value;
            porteFermee = MemoryMap.Instance.GetBit(101, MemoryType.Input).Value;

            //calcul des fronts montants
            frontBp1 = !bp1prec && bp1;
           

            ft1g = X1g && frontBp1;
            ft2g = X2g && porteOuverte;
            ft3g = X3g && finTempo;
            ft4g = X4g && !porteOuverte;
            ft5g = X5g && porteFermee;
            ft6g = X5g && (frontBp1 || capteurIR) && !porteFermee;

            X1g = ft5g || X1g && !ft1g;
            X2g = (ft1g || ft6g) || X2g && !ft2g;
            X3g = ft2g || X3g && !ft3g;
            X4g = ft3g || X4g && !ft4g;
            X5g = ft4g || X5g && !(ft5g || ft6g);

            MemoryBit OuvrirPorte = MemoryMap.Instance.GetBit(72, MemoryType.Output);
            MemoryBit FermerPorte = MemoryMap.Instance.GetBit(73, MemoryType.Output);

            OuvrirPorte.Value = X2g;
            FermerPorte.Value = X4g || X5g;
            frontX3g = !X3gprec && X3g;

            System.Diagnostics.Debug.WriteLine(finTempo);
            System.Diagnostics.Debug.WriteLine(frontX3g);

            if (!X3g)
            {
                System.Diagnostics.Debug.WriteLine("Plus X3");
                if (dispatcherTimer5s != null)
                {
                    dispatcherTimer5s.Stop();
                    finTempo = false;
                }
            }

            if (frontX3g && finTempo == false)
            {
                dispatcherTimer5s = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer5s.Tick += new EventHandler(dispatcherTimer5S_Tick);
                dispatcherTimer5s.Interval = TimeSpan.FromSeconds(5);
                System.Diagnostics.Debug.WriteLine("X3");
                dispatcherTimer5s.Start();
            }

           

        }

        private void dispatcherTimer5S_Tick(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Tick 5S");
            finTempo = true;
        }

        private void runCycleApi()
        {
            //mise à jour HomeIO
            MemoryMap.Instance.Update();

            bpPrec = this.bp;
            //lecture des entrées
            this.bp = MemoryMap.Instance.GetBit(2, MemoryType.Input).Value;

            //calculs des FTs
            this.front = !this.bpPrec && this.bp;

            this.ft1 = this.X1 && this.front;
            this.ft2 = this.X2 && this.front;
            //calculs des étapes
            this.X1 = this.ft2 || this.X1 && !this.ft1;
            this.X2 = this.ft1 || this.X2 && !this.ft2;

            //écriture des sorties
            lampe.Value = X2;

            //mise à jour HomeIO
            MemoryMap.Instance.Update();
        }

        private bool bpMonter = false;
        private bool bpMonterPrec = false;
        private bool bpDescendrePrec = false;
        private bool bpDescendre = false;
        private bool frontMonter = false;
        private bool frontDescendre = false;

        private bool X0s = true;
        private bool X1s = false;
        private bool X2s = false;
        private bool ft1s = false;
        private bool ft2s = false;
        private bool ft3s = false;
        private bool ft4s = false;

        private void runCycleApiStores()
        {
            //sauvegarde de l'état précédent
            bpMonterPrec = bpMonter;
            bpDescendrePrec = bpDescendre;

            //lecture des entrées
            bpMonter = MemoryMap.Instance.GetBit(3, MemoryType.Input).Value;
            bpDescendre = MemoryMap.Instance.GetBit(4, MemoryType.Input).Value;

            //calcul des fronts montants
            frontMonter = !bpMonterPrec && bpMonter;
            frontDescendre = !bpDescendrePrec && bpDescendre;

            float openess = MemoryMap.Instance.GetFloat(3, MemoryType.Input).Value;

            bool voletEnBas = openess == 0 ? true : false;
            bool voletEnHaut = openess == 10 ? true : false;

            ft3s = (X0s || X1s) && frontDescendre && !voletEnBas;
            ft4s = X2s && voletEnBas;
            ft1s = (X0s || X2s) && frontMonter && !voletEnHaut;
            ft2s = X1s && voletEnHaut;


            X0s = (ft2s || ft4s) || X0s && !(ft1s || ft3s);
            X2s = ft3s || X2s && !(ft4s || ft1s);
            X1s = ft1s || X1s && !(ft2s || ft3s);

            MemoryBit MonterStore = MemoryMap.Instance.GetBit(1, MemoryType.Output);
            MemoryBit DescendreStore = MemoryMap.Instance.GetBit(2, MemoryType.Output);

            MonterStore.Value = X1s;
            DescendreStore.Value = X2s;

        }
    }
}
