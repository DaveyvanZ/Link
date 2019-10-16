using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace Link_application
{
    public partial class Linkform : Form
    {
        // Kenmerken
        private ConnectionARD connectionARD;
        private Database db;
        private WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();

        // Lists
        private List<Tuple<string, string>> settinglist;
        private List<Tuple<string, int>> thresholdlist;
        private List<Tuple<string, string>> muzieklist;

        // Waardes van instellingen
        private string settingLamp;
        private string settingTv;
        private string muziekNormaal;
        private string muziekRust;
        private string muziekStress;
        private string muziekActief;
        private string muziekUitgeput;
        private bool Lamp;
        private bool Muziek;
        private bool Tv;
        private bool lampaan;
        private bool tvaan;
        private int focusth;
        private int meditatieth;

        // Waardes van gemiddelde
        public int focustotaal;
        public int focusaantal;
        public int meditatietotaal;
        public int meditatieaantal;

        // Waardes gemiddelde muziek
        public int muziekfocustotaal;
        public int muziekfocusaantal;
        public int muziekmeditatietotaal;
        public int muziekmeditatieaantal;

        // Properties
        public int Focus { private get; set; }
        public int Meditatie { private get; set; }

        private int Focusgem
        {
            get
            {
                if (focustotaal != 0 && focusaantal != 0)
                {
                    return (focustotaal/focusaantal);
                }
                else
                {
                    return 0;
                }
            }
        }

        private int Meditatiegem
        {
            get
            {
                if (meditatietotaal != 0 && meditatieaantal != 0)
                {
                    return (meditatietotaal/meditatieaantal);
                }
                else
                {
                    return 0;
                }
            }
        }

        private string GemToestand
        {
            get
            {
                if (Focus == 0 && Meditatie == 0)
                {
                    lbMuziekMedi.Text = "0";
                    lbMuziekFocus.Text = "0";
                    return "";
                }
                else
                {
                    if (muziekfocusaantal > 100 && muziekmeditatieaantal > 100)
                    {
                        int focusgem = muziekfocustotaal/muziekfocusaantal;
                        int meditatiegem = muziekmeditatietotaal/muziekmeditatieaantal;
                        lbMuziekMedi.Text   = Convert.ToString(meditatiegem);
                        lbMuziekFocus.Text = Convert.ToString(focusgem);
                        muziekfocustotaal = muziekfocusaantal = muziekmeditatietotaal = muziekmeditatieaantal = 0;

                        if (focusgem > 70 && meditatiegem < 50)
                        {
                            return "stress";
                        }
                        else if (focusgem < 40 && meditatiegem > 65)
                        {
                            return "rust";
                        }
                        else if (focusgem > 70 && meditatiegem > 70)
                        {
                            return "actief";
                        }
                        else if (focusgem < 40 && meditatiegem < 40 && focusgem > 0 && meditatiegem > 0)
                        {
                            return "uitgeput";
                        }
                        else if (focusgem == 0 && meditatiegem == 0)
                        {
                            return "";
                        }
                        else
                        {
                            return "normaal";
                        }
                    }
                    else
                    {
                        muziekfocusaantal ++;
                        muziekmeditatieaantal ++;
                        muziekfocustotaal += Focus;
                        muziekmeditatietotaal += Meditatie;
                        return "calculating";
                    }
                }
            }
        }


        // Constructor
        public Linkform()
        {
            InitializeComponent();

            connectionARD = new ConnectionARD();

            string bestand = Application.StartupPath + "\\Link.accdb";
            db = new Database(bestand);

            Verbinden();
            UpdateUserInterface();

            wplayer.URL = "";
        }

        // Timers
        private void timerARDmessage_Tick(object sender, EventArgs e)
        {
            if (connectionARD.serialPort.IsOpen && connectionARD.serialPort.BytesToRead > 0)
            {
                try
                {
                    connectionARD.ProcessMessages();
                    lblGemiddeldeFocus.Text = Convert.ToString(Focusgem);
                    lblGemiddeldeMeditatie.Text = Convert.ToString(Meditatiegem);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("De verbinding met Link is niet optimaal: " + exception.Message);
                    Verbinden();
                }
            }
        }

        private void timerARDsend_Tick(object sender, EventArgs e)
        {
            if (connectionARD.serialPort.IsOpen && connectionARD.serialPort.BytesToRead > 0)
            {
                try
                {
                    if (Lamp)
                    {
                        switch (settingLamp)
                        {
                            case "focus":
                                if (focusth <= Focus)
                                {
                                    connectionARD.SendMessage("+LampAAN");
                                }
                                else
                                {
                                    connectionARD.SendMessage("+LampUIT");
                                }
                                break;
                            case "meditatie":
                                if (meditatieth <= Meditatie)
                                {
                                    connectionARD.SendMessage("+LampAAN");
                                }
                                else
                                {
                                    connectionARD.SendMessage("+LampUIT");
                                }
                                break;
                        }
                    }
                    if (Tv)
                    {
                        switch (settingTv)
                        {
                            case "focus":
                                if (focusth <= Focus)
                                {
                                    connectionARD.SendMessage("+TvAAN");
                                }
                                else
                                {
                                    connectionARD.SendMessage("+TvUIT");
                                }
                                break;
                            case "meditatie":
                                if (meditatieth <= Meditatie)
                                {
                                    connectionARD.SendMessage("+TvAAN");
                                }
                                else
                                {
                                    connectionARD.SendMessage("+TvUIT");
                                }
                                break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("De verbinding met Link is niet optimaal: " + exception.Message);
                    Verbinden();
                }
            }
        }

        private void timerMuziek_Tick(object sender, EventArgs e)
        {
            if (Muziek)
            {
                switch (GemToestand)
                {
                    case "calculating":
                        break;
                    case "":
                        wplayer.URL = "";
                        lbNormaal.Font = new Font(lbNormaal.Font.Name, 8);
                        lbStress.Font = new Font(lbStress.Font.Name, 8);
                        lbRust.Font = new Font(lbRust.Font.Name, 8);
                        lbUitgeput.Font = new Font(lbUitgeput.Font.Name, 8);
                        lbActief.Font = new Font(lbActief.Font.Name, 8);
                        break;
                    case "normaal":
                        lbNormaal.Font = new Font(lbNormaal.Font.Name, 8, FontStyle.Bold);
                        lbStress.Font = new Font(lbStress.Font.Name, 8);
                        lbRust.Font = new Font(lbRust.Font.Name, 8);
                        lbUitgeput.Font = new Font(lbUitgeput.Font.Name, 8);
                        lbActief.Font = new Font(lbActief.Font.Name, 8);
                        if (wplayer.URL != (Application.StartupPath + "\\" + muziekNormaal + ".mp3"))
                        {
                            if (wplayer.playState.ToString() != "wmppsPlaying")
                            {
                                wplayer.controls.stop();
                                wplayer.URL = muziekNormaal + ".mp3";
                                wplayer.controls.play();
                            }
                            else
                            {
                                wplayer.URL = muziekNormaal + ".mp3";
                                wplayer.controls.play();
                            }
                        }
                        else
                        {
                            if (wplayer.playState.ToString() != "wmppsPlaying")
                            {
                                wplayer.controls.play();
                            }
                        }
                        break;
                    case "stress":
                        lbStress.Font = new Font(lbStress.Font.Name, 8, FontStyle.Bold);
                        lbRust.Font = new Font(lbRust.Font.Name, 8);
                        lbUitgeput.Font = new Font(lbUitgeput.Font.Name, 8);
                        lbActief.Font = new Font(lbActief.Font.Name, 8);
                        lbNormaal.Font = new Font(lbNormaal.Font.Name, 8);
                        if (wplayer.URL != (Application.StartupPath + "\\" + muziekStress + ".mp3"))
                        {
                            if (wplayer.playState.ToString() != "wmppsPlaying")
                            {
                                wplayer.controls.stop();
                                wplayer.URL = muziekStress + ".mp3";
                                wplayer.controls.play();
                            }
                            else
                            {
                                wplayer.URL = muziekStress + ".mp3";
                                wplayer.controls.play();
                            }
                        }
                        else
                        {
                            if (wplayer.playState.ToString() != "wmppsPlaying")
                            {
                                wplayer.controls.play();
                            }
                        }
                        break;
                    case "rust":
                        lbRust.Font = new Font(lbRust.Font.Name, 8, FontStyle.Bold);
                        lbUitgeput.Font = new Font(lbUitgeput.Font.Name, 8);
                        lbActief.Font = new Font(lbActief.Font.Name, 8);
                        lbNormaal.Font = new Font(lbNormaal.Font.Name, 8);
                        lbStress.Font = new Font(lbStress.Font.Name, 8);
                        if (wplayer.URL != (Application.StartupPath + "\\" + muziekRust + ".mp3"))
                        {
                            if (wplayer.playState.ToString() != "wmppsPlaying")
                            {
                                wplayer.controls.stop();
                                wplayer.URL = muziekRust + ".mp3";
                                wplayer.controls.play();
                            }
                            else
                            {
                                wplayer.URL = muziekRust + ".mp3";
                                wplayer.controls.play();
                            }
                        }
                        else
                        {
                            if (wplayer.playState.ToString() != "wmppsPlaying")
                            {
                                wplayer.controls.play();
                            }
                        }
                        break;
                    case "uitgeput":
                        lbUitgeput.Font = new Font(lbUitgeput.Font.Name, 8, FontStyle.Bold);
                        lbActief.Font = new Font(lbActief.Font.Name, 8);
                        lbNormaal.Font = new Font(lbNormaal.Font.Name, 8);
                        lbStress.Font = new Font(lbStress.Font.Name, 8);
                        lbRust.Font = new Font(lbRust.Font.Name, 8);
                        if (wplayer.URL != (Application.StartupPath + "\\" + muziekUitgeput + ".mp3"))
                        {
                            if (wplayer.playState.ToString() != "wmppsPlaying")
                            {
                                wplayer.controls.stop();
                                wplayer.URL = muziekUitgeput + ".mp3";
                                wplayer.controls.play();
                            }
                            else
                            {
                                wplayer.URL = muziekUitgeput + ".mp3";
                                wplayer.controls.play();
                            }
                        }
                        else
                        {
                            if (wplayer.playState.ToString() != "wmppsPlaying")
                            {
                                wplayer.controls.play();
                            }
                        }
                        break;
                    case "actief":
                        lbActief.Font = new Font(lbActief.Font.Name, 8, FontStyle.Bold);
                        lbNormaal.Font = new Font(lbNormaal.Font.Name, 8);
                        lbStress.Font = new Font(lbStress.Font.Name, 8);
                        lbRust.Font = new Font(lbRust.Font.Name, 8);
                        lbUitgeput.Font = new Font(lbUitgeput.Font.Name, 8);
                        if (wplayer.URL != (Application.StartupPath + "\\" + muziekActief + ".mp3"))
                        {
                            if (wplayer.playState.ToString() != "wmppsPlaying")
                            {
                                wplayer.controls.stop();
                                wplayer.URL = muziekActief + ".mp3";
                                wplayer.controls.play();
                            }
                            else
                            {
                                wplayer.URL = muziekActief + ".mp3";
                                wplayer.controls.play();
                            }
                        }
                        else
                        {
                            if (wplayer.playState.ToString() != "wmppsPlaying")
                            {
                                wplayer.controls.play();
                            }
                        }
                        break;
                }
            }
        }

        // Methodes
        private void rbLampFocus_CheckedChanged(object sender, EventArgs e)
        {
            // Zorgt voor verspringen van radiobuttons (niet 2 in dezelfde GB actief)
        }

        private void Verbinden()
        {
            if (connectionARD.serialPort.IsOpen)
            {
                timerARDmessage.Enabled = false;
                timerARDsend.Enabled = false;
                connectionARD.serialPort.Close();
            }
            else
            {
                String[] ports = SerialPort.GetPortNames();
                Array.Sort(ports);
                if (ports.Length != 0)
                {
                    foreach (String port in ports)
                    {
                        try
                        {
                            if (!connectionARD.serialPort.IsOpen)
                            {
                                connectionARD.serialPort.PortName = port;
                                connectionARD.serialPort.Open();
                                if (connectionARD.serialPort.IsOpen)
                                {
                                    connectionARD.ClearAllMessageData();
                                    connectionARD.serialPort.DiscardInBuffer();
                                    connectionARD.serialPort.DiscardOutBuffer();

                                    if (connectionARD.SendCheck())
                                    {
                                        if (connectionARD.CheckLink())
                                        {
                                            timerARDmessage.Enabled = true;
                                            timerARDsend.Enabled = true;
                                            break;
                                        }
                                    }
                                    connectionARD.ClearAllMessageData();
                                    connectionARD.serialPort.DiscardInBuffer();
                                    connectionARD.serialPort.DiscardOutBuffer();
                                    connectionARD.serialPort.Close();
                                }
                            }
                        }
                        catch
                        {
                            
                        }
                    }
                }
                if (!connectionARD.serialPort.IsOpen)
                {
                    MessageBox.Show("Kan niet verbinden met de Link");
                }
            }
        }

        private void UpdateUserInterface()
        {
            GetDatabaseSettings();
            bool isConnected = connectionARD.serialPort.IsOpen;
            if (isConnected)
            {
                btVerbinden.Text = "Verbinding verbreken";
            }
            else
            {
                btVerbinden.Text = "Verbinden met Link";
            }
            btPasToe.Enabled = gbLamp.Enabled = gbTv.Enabled = gbWaardes.Enabled = lbFocus.Enabled = lbMeditatie.Enabled = lbNormaal.Enabled = gbNormaal.Enabled = gbStress.Enabled = gbRust.Enabled = gbActief.Enabled = gbUitgeput.Enabled = btPasToeMuziek.Enabled = btConnectieTest.Enabled = cbMuziek.Enabled = cbLamp.Enabled = cbTv.Enabled = isConnected;
            cbLamp.Checked = Lamp;
            cbTv.Checked = Tv;
            cbMuziek.Checked = Muziek;
            gbLamp.Enabled = Lamp;
            gbTv.Enabled = Tv;
			gbNormaal.Enabled = gbStress.Enabled = gbRust.Enabled = gbActief.Enabled = gbUitgeput.Enabled = btPasToeMuziek.Enabled = Muziek;
            switch (settingTv)
            {
                case "focus": rbTvFocus.Checked = true; rbTvMeditatie.Checked = rbTvWink.Checked = false; break;
                case "meditatie": rbTvMeditatie.Checked = true; rbTvFocus.Checked = rbTvWink.Checked = false; break;
                case "wink": rbTvWink.Checked = true; rbTvMeditatie.Checked = rbLampFocus.Checked = false; break;
            }
            switch (settingLamp)
            {
                case "focus": rbLampFocus.Checked = true; rbLampMeditatie.Checked = rbLampWink.Checked = false; break;
                case "meditatie": rbLampMeditatie.Checked = true; rbLampFocus.Checked = rbLampWink.Checked = false; break;
                case "wink": rbLampWink.Checked = true; rbLampMeditatie.Checked = rbLampFocus.Checked = false; break;
            }
            switch (muziekRust)
            {
                case "klassiek": rbRustKlas.Checked = true; rbRustCountry.Checked = rbRustDance.Checked = rbRustDub.Checked = rbRustHip.Checked = rbRustMetal.Checked = rbRustPop.Checked = rbRustRock.Checked = rbRustTechno.Checked = false; break;
                case "pop": rbRustPop.Checked = true; rbRustCountry.Checked = rbRustDance.Checked = rbRustDub.Checked = rbRustHip.Checked = rbRustMetal.Checked = rbRustKlas.Checked = rbRustRock.Checked = rbRustTechno.Checked = false; break;
                case "rock": rbRustRock.Checked = true; rbRustCountry.Checked = rbRustDance.Checked = rbRustDub.Checked = rbRustHip.Checked = rbRustMetal.Checked = rbRustPop.Checked = rbRustKlas.Checked = rbRustTechno.Checked = false; break;
                case "metal": rbRustMetal.Checked = true; rbRustCountry.Checked = rbRustDance.Checked = rbRustDub.Checked = rbRustHip.Checked = rbRustKlas.Checked = rbRustPop.Checked = rbRustRock.Checked = rbRustTechno.Checked = false; break;
                case "dance": rbRustDance.Checked = true; rbRustCountry.Checked = rbRustKlas.Checked = rbRustDub.Checked = rbRustHip.Checked = rbRustMetal.Checked = rbRustPop.Checked = rbRustRock.Checked = rbRustTechno.Checked = false; break;
                case "hiphop": rbRustHip.Checked = true; rbRustCountry.Checked = rbRustDance.Checked = rbRustDub.Checked = rbRustKlas.Checked = rbRustMetal.Checked = rbRustPop.Checked = rbRustRock.Checked = rbRustTechno.Checked = false; break;
                case "techno": rbRustTechno.Checked = true; rbRustCountry.Checked = rbRustDance.Checked = rbRustDub.Checked = rbRustHip.Checked = rbRustMetal.Checked = rbRustPop.Checked = rbRustRock.Checked = rbRustKlas.Checked = false; break;
                case "country": rbRustCountry.Checked = true; rbRustKlas.Checked = rbRustDance.Checked = rbRustDub.Checked = rbRustHip.Checked = rbRustMetal.Checked = rbRustPop.Checked = rbRustRock.Checked = rbRustTechno.Checked = false; break;
                case "dubstep": rbRustDub.Checked = true; rbRustCountry.Checked = rbRustDance.Checked = rbRustKlas.Checked = rbRustHip.Checked = rbRustMetal.Checked = rbRustPop.Checked = rbRustRock.Checked = rbRustTechno.Checked = false; break;
            }
            switch (muziekNormaal)
            {
                case "klassiek": rbNormaalKlas.Checked = true; rbNormaalCountry.Checked = rbNormaalDance.Checked = rbNormaalDub.Checked = rbNormaalHip.Checked = rbNormaalMetal.Checked = rbNormaalPop.Checked = rbNormaalRock.Checked = rbNormaalTechno.Checked = false; break;
                case "pop": rbNormaalPop.Checked = true; rbNormaalCountry.Checked = rbNormaalDance.Checked = rbNormaalDub.Checked = rbNormaalHip.Checked = rbNormaalMetal.Checked = rbNormaalKlas.Checked = rbNormaalRock.Checked = rbNormaalTechno.Checked = false; break;
                case "rock": rbNormaalRock.Checked = true; rbNormaalCountry.Checked = rbNormaalDance.Checked = rbNormaalDub.Checked = rbNormaalHip.Checked = rbNormaalMetal.Checked = rbNormaalPop.Checked = rbNormaalKlas.Checked = rbNormaalTechno.Checked = false; break;
                case "metal": rbNormaalMetal.Checked = true; rbNormaalCountry.Checked = rbNormaalDance.Checked = rbNormaalDub.Checked = rbNormaalHip.Checked = rbNormaalKlas.Checked = rbNormaalPop.Checked = rbNormaalRock.Checked = rbNormaalTechno.Checked = false; break;
                case "dance": rbNormaalDance.Checked = true; rbNormaalCountry.Checked = rbNormaalKlas.Checked = rbNormaalDub.Checked = rbNormaalHip.Checked = rbNormaalMetal.Checked = rbNormaalPop.Checked = rbNormaalRock.Checked = rbNormaalTechno.Checked = false; break;
                case "hiphop": rbNormaalHip.Checked = true; rbNormaalCountry.Checked = rbNormaalDance.Checked = rbNormaalDub.Checked = rbNormaalKlas.Checked = rbNormaalMetal.Checked = rbNormaalPop.Checked = rbNormaalRock.Checked = rbNormaalTechno.Checked = false; break;
                case "techno": rbNormaalTechno.Checked = true; rbNormaalCountry.Checked = rbNormaalDance.Checked = rbNormaalDub.Checked = rbNormaalHip.Checked = rbNormaalMetal.Checked = rbNormaalPop.Checked = rbNormaalRock.Checked = rbNormaalKlas.Checked = false; break;
                case "country": rbNormaalCountry.Checked = true; rbNormaalKlas.Checked = rbNormaalDance.Checked = rbNormaalDub.Checked = rbNormaalHip.Checked = rbNormaalMetal.Checked = rbNormaalPop.Checked = rbNormaalRock.Checked = rbNormaalTechno.Checked = false; break;
                case "dubstep": rbNormaalDub.Checked = true; rbNormaalCountry.Checked = rbNormaalDance.Checked = rbNormaalKlas.Checked = rbNormaalHip.Checked = rbNormaalMetal.Checked = rbNormaalPop.Checked = rbNormaalRock.Checked = rbNormaalTechno.Checked = false; break;
            }
            switch (muziekStress)
            {
                case "klassiek": rbStressKlas.Checked = true; rbStressCountry.Checked = rbStressDance.Checked = rbStressDub.Checked = rbStressHip.Checked = rbStressMetal.Checked = rbStressPop.Checked = rbStressRock.Checked = rbStressTechno.Checked = false; break;
                case "pop": rbStressPop.Checked = true; rbStressCountry.Checked = rbStressDance.Checked = rbStressDub.Checked = rbStressHip.Checked = rbStressMetal.Checked = rbStressKlas.Checked = rbStressRock.Checked = rbStressTechno.Checked = false; break;
                case "rock": rbStressRock.Checked = true; rbStressCountry.Checked = rbStressDance.Checked = rbStressDub.Checked = rbStressHip.Checked = rbStressMetal.Checked = rbStressPop.Checked = rbStressKlas.Checked = rbStressTechno.Checked = false; break;
                case "metal": rbStressMetal.Checked = true; rbStressCountry.Checked = rbStressDance.Checked = rbStressDub.Checked = rbStressHip.Checked = rbStressKlas.Checked = rbStressPop.Checked = rbStressRock.Checked = rbStressTechno.Checked = false; break;
                case "dance": rbStressDance.Checked = true; rbStressCountry.Checked = rbStressKlas.Checked = rbStressDub.Checked = rbStressHip.Checked = rbStressMetal.Checked = rbStressPop.Checked = rbStressRock.Checked = rbStressTechno.Checked = false; break;
                case "hiphop": rbStressHip.Checked = true; rbStressCountry.Checked = rbStressDance.Checked = rbStressDub.Checked = rbStressKlas.Checked = rbStressMetal.Checked = rbStressPop.Checked = rbStressRock.Checked = rbStressTechno.Checked = false; break;
                case "techno": rbStressTechno.Checked = true; rbStressCountry.Checked = rbStressDance.Checked = rbStressDub.Checked = rbStressHip.Checked = rbStressMetal.Checked = rbStressPop.Checked = rbStressRock.Checked = rbStressKlas.Checked = false; break;
                case "country": rbStressCountry.Checked = true; rbStressKlas.Checked = rbStressDance.Checked = rbStressDub.Checked = rbStressHip.Checked = rbStressMetal.Checked = rbStressPop.Checked = rbStressRock.Checked = rbStressTechno.Checked = false; break;
                case "dubstep": rbStressDub.Checked = true; rbStressCountry.Checked = rbStressDance.Checked = rbStressKlas.Checked = rbStressHip.Checked = rbStressMetal.Checked = rbStressPop.Checked = rbStressRock.Checked = rbStressTechno.Checked = false; break;
            }
            switch (muziekActief)
            {
                case "klassiek": rbActiefKlas.Checked = true; rbActiefCountry.Checked = rbActiefDance.Checked = rbActiefDub.Checked = rbActiefHip.Checked = rbActiefMetal.Checked = rbActiefPop.Checked = rbActiefRock.Checked = rbActiefTechno.Checked = false; break;
                case "pop": rbActiefPop.Checked = true; rbActiefCountry.Checked = rbActiefDance.Checked = rbActiefDub.Checked = rbActiefHip.Checked = rbActiefMetal.Checked = rbActiefKlas.Checked = rbActiefRock.Checked = rbActiefTechno.Checked = false; break;
                case "rock": rbActiefRock.Checked = true; rbActiefCountry.Checked = rbActiefDance.Checked = rbActiefDub.Checked = rbActiefHip.Checked = rbActiefMetal.Checked = rbActiefPop.Checked = rbActiefKlas.Checked = rbActiefTechno.Checked = false; break;
                case "metal": rbActiefMetal.Checked = true; rbActiefCountry.Checked = rbActiefDance.Checked = rbActiefDub.Checked = rbActiefHip.Checked = rbActiefKlas.Checked = rbActiefPop.Checked = rbActiefRock.Checked = rbActiefTechno.Checked = false; break;
                case "dance": rbActiefDance.Checked = true; rbActiefCountry.Checked = rbActiefKlas.Checked = rbActiefDub.Checked = rbActiefHip.Checked = rbActiefMetal.Checked = rbActiefPop.Checked = rbActiefRock.Checked = rbActiefTechno.Checked = false; break;
                case "hiphop": rbActiefHip.Checked = true; rbActiefCountry.Checked = rbActiefDance.Checked = rbActiefDub.Checked = rbActiefKlas.Checked = rbActiefMetal.Checked = rbActiefPop.Checked = rbActiefRock.Checked = rbActiefTechno.Checked = false; break;
                case "techno": rbActiefTechno.Checked = true; rbActiefCountry.Checked = rbActiefDance.Checked = rbActiefDub.Checked = rbActiefHip.Checked = rbActiefMetal.Checked = rbActiefPop.Checked = rbActiefRock.Checked = rbActiefKlas.Checked = false; break;
                case "country": rbActiefCountry.Checked = true; rbActiefKlas.Checked = rbActiefDance.Checked = rbActiefDub.Checked = rbActiefHip.Checked = rbActiefMetal.Checked = rbActiefPop.Checked = rbActiefRock.Checked = rbActiefTechno.Checked = false; break;
                case "dubstep": rbActiefDub.Checked = true; rbActiefCountry.Checked = rbActiefDance.Checked = rbActiefKlas.Checked = rbActiefHip.Checked = rbActiefMetal.Checked = rbActiefPop.Checked = rbActiefRock.Checked = rbActiefTechno.Checked = false; break;
            }
            switch (muziekUitgeput)
            {
                case "klassiek": rbUitgeputKlas.Checked = true; rbUitgeputCountry.Checked = rbUitgeputDance.Checked = rbUitgeputDub.Checked = rbUitgeputHip.Checked = rbUitgeputMetal.Checked = rbUitgeputPop.Checked = rbUitgeputRock.Checked = rbUitgeputTechno.Checked = false; break;
                case "pop": rbUitgeputPop.Checked = true; rbUitgeputCountry.Checked = rbUitgeputDance.Checked = rbUitgeputDub.Checked = rbUitgeputHip.Checked = rbUitgeputMetal.Checked = rbUitgeputKlas.Checked = rbUitgeputRock.Checked = rbUitgeputTechno.Checked = false; break;
                case "rock": rbUitgeputRock.Checked = true; rbUitgeputCountry.Checked = rbUitgeputDance.Checked = rbUitgeputDub.Checked = rbUitgeputHip.Checked = rbUitgeputMetal.Checked = rbUitgeputPop.Checked = rbUitgeputKlas.Checked = rbUitgeputTechno.Checked = false; break;
                case "metal": rbUitgeputMetal.Checked = true; rbUitgeputCountry.Checked = rbUitgeputDance.Checked = rbUitgeputDub.Checked = rbUitgeputHip.Checked = rbUitgeputKlas.Checked = rbUitgeputPop.Checked = rbUitgeputRock.Checked = rbUitgeputTechno.Checked = false; break;
                case "dance": rbUitgeputDance.Checked = true; rbUitgeputCountry.Checked = rbUitgeputKlas.Checked = rbUitgeputDub.Checked = rbUitgeputHip.Checked = rbUitgeputMetal.Checked = rbUitgeputPop.Checked = rbUitgeputRock.Checked = rbUitgeputTechno.Checked = false; break;
                case "hiphop": rbUitgeputHip.Checked = true; rbUitgeputCountry.Checked = rbUitgeputDance.Checked = rbUitgeputDub.Checked = rbUitgeputKlas.Checked = rbUitgeputMetal.Checked = rbUitgeputPop.Checked = rbUitgeputRock.Checked = rbUitgeputTechno.Checked = false; break;
                case "techno": rbUitgeputTechno.Checked = true; rbUitgeputCountry.Checked = rbUitgeputDance.Checked = rbUitgeputDub.Checked = rbUitgeputHip.Checked = rbUitgeputMetal.Checked = rbUitgeputPop.Checked = rbUitgeputRock.Checked = rbUitgeputKlas.Checked = false; break;
                case "country": rbUitgeputCountry.Checked = true; rbUitgeputKlas.Checked = rbUitgeputDance.Checked = rbUitgeputDub.Checked = rbUitgeputHip.Checked = rbUitgeputMetal.Checked = rbUitgeputPop.Checked = rbUitgeputRock.Checked = rbUitgeputTechno.Checked = false; break;
                case "dubstep": rbUitgeputDub.Checked = true; rbUitgeputCountry.Checked = rbUitgeputDance.Checked = rbUitgeputKlas.Checked = rbUitgeputHip.Checked = rbUitgeputMetal.Checked = rbUitgeputPop.Checked = rbUitgeputRock.Checked = rbUitgeputTechno.Checked = false; break;
            }
            nudFocus.Value = focusth;
            nudMeditatie.Value = meditatieth;
        }

        private void GetDatabaseSettings()
        {
            if (db.SettingsOphalen() != null && db.ThresholdsOphalen() != null && db.MuziekOphalen() != null)
            {
                settinglist = db.SettingsOphalen();
                foreach (Tuple<string, string> setting in settinglist)
                {
                    switch (setting.Item1)
                    {
                        case "lamp":
                            settingLamp = setting.Item2;
                            break;
                        case "tv":
                            settingTv = setting.Item2;
                            break;
                    }
                }

                thresholdlist = db.ThresholdsOphalen();
                foreach (Tuple<string, int> threshold in thresholdlist)
                {
                    switch (threshold.Item1)
                    {
                        case "focus":
                            focusth = threshold.Item2;
                            break;
                        case "meditatie":
                            meditatieth = threshold.Item2;
                            break;
                    }
                }

                muzieklist = db.MuziekOphalen();
                foreach (Tuple<string, string> muziek in muzieklist)
                {
                    switch (muziek.Item1)
                    {
                        case "normaal":
                            muziekNormaal = muziek.Item2;
                            break;
                        case "stress":
                            muziekStress = muziek.Item2;
                            break;
                        case "rust":
                            muziekRust = muziek.Item2;
                            break;
                        case "actief":
                            muziekActief = muziek.Item2;
                            break;
                        case "uitgeput":
                            muziekUitgeput = muziek.Item2;
                            break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Kan niet verbinden met de database");
				Environment.Exit(0);
            }
        }

        public void Winker()
        {
            lbxWink.Items.Insert(0, "Wink");
            lblWinkAantal.Text = Convert.ToString(Convert.ToInt32(lblWinkAantal.Text) + 1);
            if (Lamp)
            {
                if (settingLamp == "wink")
                {
                    if (lampaan) { connectionARD.SendMessage("+LampUIT"); lampaan = false; }
                    else { connectionARD.SendMessage("+LampAAN"); lampaan = true; }
                }
            }
            if (Tv)
            {
                if (settingTv == "wink")
                {
                    if (tvaan) { connectionARD.SendMessage("+TvUIT"); tvaan = false; }
                    else { connectionARD.SendMessage("+TvAAN"); tvaan = true; }
                }
            }
        }

        private void btPasToe_Click(object sender, EventArgs e)
        {
            if (rbTvFocus.Checked)
            {
                settingTv = "focus";
            }
            else if (rbTvMeditatie.Checked)
            {
                settingTv = "meditatie";
            }
            else if (rbTvWink.Checked)
            {
                settingTv = "wink";
            }
            else
            {
                MessageBox.Show("U heeft geen instelling voor de Tv gekozen.");
            }

            Tv = cbTv.Checked;
            db.UpdateSetting("tv", settingTv);

            if (rbLampFocus.Checked)
            {
                settingLamp = "focus";
            }
            else if (rbLampMeditatie.Checked)
            {
                settingLamp = "meditatie";
            }
            else if (rbLampWink.Checked)
            {
                settingLamp = "wink";
            }
            else
            {
                MessageBox.Show("U heeft geen instelling voor de lamp gekozen.");
            }

            Lamp = cbLamp.Checked;
            db.UpdateSetting("lamp", settingLamp);

            focusth = Convert.ToInt32(nudFocus.Value);
            db.UpdateThreshold("focus", focusth);

            meditatieth = Convert.ToInt32(nudMeditatie.Value);
            db.UpdateThreshold("meditatie", meditatieth);

            UpdateUserInterface();
        }
     
        private void btVerbinden_Click(object sender, EventArgs e)
        {
            Verbinden();
            UpdateUserInterface();
        }

        private void btConnectieTest_Click(object sender, EventArgs e)
        {
            if (connectionARD.serialPort.IsOpen)
            {
                MessageBox.Show("Verbonden met: " + connectionARD.serialPort.PortName);
            }
            else
            {
                MessageBox.Show("Link is niet verbonden.");
            }
        }
       
        private void btPasToeMuziek_Click(object sender, EventArgs e)
        {
            if (rbNormaalCountry.Checked)
            {
                muziekNormaal = "country";
            }
            else if (rbNormaalDance.Checked)
            {
                muziekNormaal = "dance";
            }
            else if (rbNormaalDub.Checked)
            {
                muziekNormaal = "dubstep";
            }
            else if (rbNormaalHip.Checked)
            {
                muziekNormaal = "hiphop";
            }
            else if (rbNormaalKlas.Checked)
            {
                muziekNormaal = "klassiek";
            }
            else if (rbNormaalMetal.Checked)
            {
                muziekNormaal = "metal";
            }
            else if (rbNormaalPop.Checked)
            {
                muziekNormaal = "pop";
            }
            else if (rbNormaalRock.Checked)
            {
                muziekNormaal = "rock";
            }
            else if (rbNormaalTechno.Checked)
            {
                muziekNormaal = "techno";
            }
            else
            {
                MessageBox.Show("U heeft geen muziek voor de normale gemoedstoestand gekozen.");
            }
            db.UpdateMuziek("normaal", muziekNormaal);

            if (rbRustCountry.Checked)
            {
                muziekRust = "country";
            }
            else if (rbRustDance.Checked)
            {
                muziekRust = "dance";
            }
            else if (rbRustDub.Checked)
            {
                muziekRust = "dubstep";
            }
            else if (rbRustHip.Checked)
            {
                muziekRust = "hiphop";
            }
            else if (rbRustKlas.Checked)
            {
                muziekRust = "klassiek";
            }
            else if (rbRustMetal.Checked)
            {
                muziekRust = "metal";
            }
            else if (rbRustPop.Checked)
            {
                muziekRust = "pop";
            }
            else if (rbRustRock.Checked)
            {
                muziekRust = "rock";
            }
            else if (rbRustTechno.Checked)
            {
                muziekRust = "techno";
            }
            else
            {
                MessageBox.Show("U heeft geen muziek voor de rust gemoedstoestand gekozen.");
            }
            db.UpdateMuziek("rust", muziekRust);

            if (rbStressCountry.Checked)
            {
                muziekStress = "country";
            }
            else if (rbStressDance.Checked)
            {
                muziekStress = "dance";
            }
            else if (rbStressDub.Checked)
            {
                muziekStress = "dubstep";
            }
            else if (rbStressHip.Checked)
            {
                muziekStress = "hiphop";
            }
            else if (rbStressKlas.Checked)
            {
                muziekStress = "klassiek";
            }
            else if (rbStressMetal.Checked)
            {
                muziekStress = "metal";
            }
            else if (rbStressPop.Checked)
            {
                muziekStress = "pop";
            }
            else if (rbStressRock.Checked)
            {
                muziekStress = "rock";
            }
            else if (rbStressTechno.Checked)
            {
                muziekStress = "techno";
            }
            else
            {
                MessageBox.Show("U heeft geen muziek voor de stress gemoedstoestand gekozen.");
            }
            db.UpdateMuziek("stress", muziekStress);

            if (rbActiefCountry.Checked)
            {
                muziekActief = "country";
            }
            else if (rbActiefDance.Checked)
            {
                muziekActief = "dance";
            }
            else if (rbActiefDub.Checked)
            {
                muziekActief = "dubstep";
            }
            else if (rbActiefHip.Checked)
            {
                muziekActief = "hiphop";
            }
            else if (rbActiefKlas.Checked)
            {
                muziekActief = "klassiek";
            }
            else if (rbActiefMetal.Checked)
            {
                muziekActief = "metal";
            }
            else if (rbActiefPop.Checked)
            {
                muziekActief = "pop";
            }
            else if (rbActiefRock.Checked)
            {
                muziekActief = "rock";
            }
            else if (rbActiefTechno.Checked)
            {
                muziekActief = "techno";
            }
            else
            {
                MessageBox.Show("U heeft geen muziek voor de actief gemoedstoestand gekozen.");
            }
            db.UpdateMuziek("actief", muziekActief);

            if (rbUitgeputCountry.Checked)
            {
                muziekUitgeput = "country";
            }
            else if (rbUitgeputDance.Checked)
            {
                muziekUitgeput = "dance";
            }
            else if (rbUitgeputDub.Checked)
            {
                muziekUitgeput = "dubstep";
            }
            else if (rbUitgeputHip.Checked)
            {
                muziekUitgeput = "hiphop";
            }
            else if (rbUitgeputKlas.Checked)
            {
                muziekUitgeput = "klassiek";
            }
            else if (rbUitgeputMetal.Checked)
            {
                muziekUitgeput = "metal";
            }
            else if (rbUitgeputPop.Checked)
            {
                muziekUitgeput = "pop";
            }
            else if (rbUitgeputRock.Checked)
            {
                muziekUitgeput = "rock";
            }
            else if (rbUitgeputTechno.Checked)
            {
                muziekUitgeput = "techno";
            }
            else
            {
                MessageBox.Show("U heeft geen muziek voor de uitgeput gemoedstoestand gekozen.");
            }
            db.UpdateMuziek("uitgeput", muziekUitgeput);

            UpdateUserInterface();
        }

        private void Linkform_FormClosed(object sender, FormClosedEventArgs e)
        {
            timerARDmessage.Enabled = false;
            timerARDsend.Enabled = false;
            
            if (connectionARD.serialPort.IsOpen)
            {
                connectionARD.SendMessage("+AllesUIT");
                connectionARD.serialPort.Close();
            }
        }

		private void cbMuziek_CheckedChanged(object sender, EventArgs e)
		{
			if (Muziek)
			{
				Muziek = false;
			    timerMuziek.Enabled = false;
                if (wplayer.playState.ToString() == "wmppsPlaying")
                {
                    wplayer.controls.stop();
                }
			}
			else
			{
				Muziek = true;
                timerMuziek.Enabled = true;
			}
            UpdateUserInterface();
        }

        private void cbTv_CheckedChanged(object sender, EventArgs e)
        {
            if (Tv)
            {
                Tv = false;
                connectionARD.SendMessage("+TvUIT");
            }
            else
            {
                Tv = true;
            }
            UpdateUserInterface();
        }

        private void cbLamp_CheckedChanged(object sender, EventArgs e)
        {
            if (Lamp)
            {
                Lamp = false;
                connectionARD.SendMessage("+LampUIT");
            }
            else
            {
                Lamp = true;
            }
            UpdateUserInterface();
        }
	}
}
