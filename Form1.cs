using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Ticket_Gen
{
    public partial class TicketGen : Form
    {
        bool MT4TargetMode = false;
        bool[] FlatFlat = new bool[7];
        string id = "";
        bool[] loaded = new bool[7];
        System.Threading.Thread CADThread;
        System.Threading.Thread EURThread;
        System.Threading.Thread GBPThread;
        System.Threading.Thread AUDThread;
        System.Threading.Thread NZDThread;
        System.Threading.Thread JPYThread;
        System.Threading.Thread CHFThread;
        System.Threading.Thread FlatFlatThread;
        //CAD
        //EUR
        //GBP
        //AUD
        //NZD
        //JPY
        //CHF

        public TicketGen()
        {
            InitializeComponent();

            //FlatFlatThread = new System.Threading.Thread(() => FlatFlatChecker("CAD"));

            if (File.Exists(@"D:\ProgramData\StrataGem571\TicketGen\Settings\Currency Pair.txt") == true)
            {
                string[] fr = File.ReadAllLines(@"D:\ProgramData\StrataGem571\TicketGen\Settings\Currency Pair.txt");
                SourceBox.Text = fr[0];
                DestinationBox.Text = fr[1];
                MT4Box.Text = fr[2];
                eSigBox.Text = fr[3];
                if (fr[4].Contains('P')) PriceGridButton.Checked = true;
                else if (fr[4].Contains('M'))
                {
                    MT4Button.Checked = true;
                    if (fr[4].Contains('Y')) TargetMode.Checked = true;
                }
                else if (fr[4].Contains('E')) eSigButton.Checked = true;
                string[] s = fr[5].Split(',');
                foreach (DataGridViewRow row in CADPriceGrid.Rows)
                {
                    row.Cells[0].Value = s[0];
                    row.Cells[1].Value = s[1];
                    row.Cells[2].Value = s[2];
                }
                s = fr[6].Split(',');
                foreach (DataGridViewRow row in EURPriceGrid.Rows)
                {
                    row.Cells[0].Value = s[0];
                    row.Cells[1].Value = s[1];
                    row.Cells[2].Value = s[2];
                }
                s = fr[7].Split(',');
                foreach (DataGridViewRow row in GBPPriceGrid.Rows)
                {
                    row.Cells[0].Value = s[0];
                    row.Cells[1].Value = s[1];
                    row.Cells[2].Value = s[2];
                }
                s = fr[8].Split(',');
                foreach (DataGridViewRow row in AUDPriceGrid.Rows)
                {
                    row.Cells[0].Value = s[0];
                    row.Cells[1].Value = s[1];
                    row.Cells[2].Value = s[2];
                }
                s = fr[9].Split(',');
                foreach (DataGridViewRow row in NZDPriceGrid.Rows)
                {
                    row.Cells[0].Value = s[0];
                    row.Cells[1].Value = s[1];
                    row.Cells[2].Value = s[2];
                }
                s = fr[10].Split(',');
                foreach (DataGridViewRow row in JPYPriceGrid.Rows)
                {
                    row.Cells[0].Value = s[0];
                    row.Cells[1].Value = s[1];
                    row.Cells[2].Value = s[2];
                }
                s = fr[11].Split(',');
                foreach (DataGridViewRow row in CHFPriceGrid.Rows)
                {
                    row.Cells[0].Value = s[0];
                    row.Cells[1].Value = s[1];
                    row.Cells[2].Value = s[2];
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            String save = "";
            save += SourceBox.Text + "\n" + DestinationBox.Text + "\n" + MT4Box.Text + "\n" + eSigBox.Text + "\n";
            if (PriceGridButton.Checked == true) save += "P\n";
            else if (MT4Button.Checked == true)
            {
                save += "M";
                if (TargetMode.Checked == true) save += "Y\n";
                else save += "\n";
            }
            else if (eSigButton.Checked == true) save += "E\n";
            foreach (DataGridViewRow row in CADPriceGrid.Rows)
            {
                save += row.Cells[0].Value + "," + row.Cells[1].Value + "," + row.Cells[2].Value + "\n";
            }
            foreach (DataGridViewRow row in EURPriceGrid.Rows)
            {
                save += row.Cells[0].Value + "," + row.Cells[1].Value + "," + row.Cells[2].Value + "\n";
            }
            foreach (DataGridViewRow row in GBPPriceGrid.Rows)
            {
                save += row.Cells[0].Value + "," + row.Cells[1].Value + "," + row.Cells[2].Value + "\n";
            }
            foreach (DataGridViewRow row in AUDPriceGrid.Rows)
            {
                save += row.Cells[0].Value + "," + row.Cells[1].Value + "," + row.Cells[2].Value + "\n";
            }
            foreach (DataGridViewRow row in NZDPriceGrid.Rows)
            {
                save += row.Cells[0].Value + "," + row.Cells[1].Value + "," + row.Cells[2].Value + "\n";
            }
            foreach (DataGridViewRow row in JPYPriceGrid.Rows)
            {
                save += row.Cells[0].Value + "," + row.Cells[1].Value + "," + row.Cells[2].Value + "\n";
            }
            foreach (DataGridViewRow row in CHFPriceGrid.Rows)
            {
                save += row.Cells[0].Value + "," + row.Cells[1].Value + "," + row.Cells[2].Value + "\n";
            }

            if (File.Exists(@"D:\ProgramData\StrataGem571\TicketGen\Settings\Currency Pair.txt"))
            {
                File.Delete(@"D:\ProgramData\StrataGem571\TicketGen\Settings\Currency Pair.txt");
            }
            using (FileStream fs = File.Create(@"D:\ProgramData\StrataGem571\TicketGen\Settings\Currency Pair.txt"))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(save);
                fs.Write(info, 0, info.Length);
            }
        }

        private void Load(string cp)
        {
            if (SourceBox.Text == "")
            {
                MessageBox.Show("Source Directory is invalid", "Error");
                return;
            }
            if (DestinationBox.Text == "")
            {
                MessageBox.Show("Destination Directory is invalid", "Error");
                return;
            }

            DataGridView Source = null;
            DataGridView Processed = null;
            DataGridView Destination = null;

            string[] fr = null;
            try
            {
                fr = File.ReadAllLines(MT4Box.Text);
            }
            catch (System.IO.IOException)
            {
                int l = 0;
                while (l == 0)
                {
                    try
                    {
                        fr = File.ReadAllLines(MT4Box.Text);
                        l = 1;
                    }
                    catch (System.IO.IOException)
                    {
                        l = 0;
                    }
                    if (l != 0)
                    {
                        l = 1;
                    }
                }
            }
            foreach (string s in fr)
            {
                if (s.Contains("Time")) continue;
                string[] split = s.Split(',');
                string[] temp1 = split[2].Split('.');
                char[] c = temp1[1].ToCharArray();
                string final;
                if (split[4] == "FLAT" && split[5] == "FLAT")
                {
                    //CAD
                    //EUR
                    //GBP
                    //AUD
                    //NZD
                    //JPY
                    //CHF
                    if (split[1].Contains("CAD"))
                    {
                        FlatFlat[0] = true;
                    }
                    if (split[1].Contains("EUR"))
                    {
                        FlatFlat[1] = true;
                    }
                    if (split[1].Contains("GBP"))
                    {
                        FlatFlat[2] = true;
                    }
                    if (split[1].Contains("AUD"))
                    {
                        FlatFlat[3] = true;
                    }
                    if (split[1].Contains("NZD"))
                    {
                        FlatFlat[4] = true;
                    }
                    if (split[1].Contains("JPY"))
                    {
                        FlatFlat[5] = true;
                    }
                    if (split[1].Contains("CHF"))
                    {
                        FlatFlat[6] = true;
                    }
                }
                if (split[4] != "FLAT" || split[5] != "FLAT")
                {
                    //CAD
                    //EUR
                    //GBP
                    //AUD
                    //NZD
                    //JPY
                    //CHF
                    if (split[1].Contains("CAD"))
                    {
                        FlatFlat[0] = false;
                    }
                    if (split[1].Contains("EUR"))
                    {
                        FlatFlat[1] = false;
                    }
                    if (split[1].Contains("GBP"))
                    {
                        FlatFlat[2] = false;
                    }
                    if (split[1].Contains("AUD"))
                    {
                        FlatFlat[3] = false;
                    }
                    if (split[1].Contains("NZD"))
                    {
                        FlatFlat[4] = false;
                    }
                    if (split[1].Contains("JPY"))
                    {
                        FlatFlat[5] = false;
                    }
                    if (split[1].Contains("CHF"))
                    {
                        FlatFlat[6] = false;
                    }
                }
                final = String.Join(".", temp1);
                split[2] = final;
                if (split[1].Contains("CAD"))
                {
                    CADPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[0]) CADPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") CADPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") CADPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
                else if (split[1].Contains("EUR"))
                {
                    EURPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[1]) EURPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") EURPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") EURPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
                else if (split[1].Contains("GBP"))
                {
                    GBPPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[2]) GBPPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") GBPPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") GBPPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
                else if (split[1].Contains("AUD"))
                {
                    AUDPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[3]) AUDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") AUDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") AUDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
                else if (split[1].Contains("NZD"))
                {
                    NZDPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[4]) NZDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") NZDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") NZDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
                else if (split[1].Contains("JPY"))
                {
                    JPYPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[5]) JPYPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") JPYPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") JPYPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
                else if (split[1].Contains("CHF"))
                {
                    CHFPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[6]) CHFPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") CHFPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") CHFPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
            }
            switch (cp)
            {
                case "CAD":
                    Source = CADSourceGrid;
                    Processed = CADProcessedGrid;
                    Destination = CADDestinationGrid;
                    loaded[0] = true;
                    break;
                case "EUR":
                    Source = EURSourceGrid;
                    Processed = EURProcessedGrid;
                    Destination = EURDestinationGrid;
                    loaded[1] = true;
                    break;
                case "GBP":
                    Source = GBPSourceGrid;
                    Processed = GBPProcessedGrid;
                    Destination = GBPDestinationGrid;
                    loaded[2] = true;
                    break;
                case "AUD":
                    Source = AUDSourceGrid;
                    Processed = AUDProcessedGrid;
                    Destination = AUDDestinationGrid;
                    loaded[3] = true;
                    break;
                case "NZD":
                    Source = NZDSourceGrid;
                    Processed = NZDProcessedGrid;
                    Destination = NZDDestinationGrid;
                    loaded[4] = true;
                    break;
                case "JPY":
                    Source = JPYSourceGrid;
                    Processed = JPYProcessedGrid;
                    Destination = JPYDestinationGrid;
                    loaded[5] = true;
                    break;
                case "CHF":
                    Source = CHFSourceGrid;
                    Processed = CHFProcessedGrid;
                    Destination = CHFDestinationGrid;
                    loaded[6] = true;
                    break;
            }

            Source.AllowUserToAddRows = true;
            Source.Columns.Clear();
            Processed.Columns.Clear();
            Destination.Columns.Clear();
            Source.Refresh();
            Processed.Refresh();
            Destination.Refresh();
            Source.DataSource = null;
            Processed.DataSource = null;
            Destination.DataSource = null;

            DirectoryInfo sf = new DirectoryInfo(SourceBox.Text);
            FileInfo[] files = sf.GetFiles();
            int fileNum = 0;
            Source.RowCount = 1000;
            Source.ColumnCount = 10;

            foreach (FileInfo file in files)
            {
                if (!file.Name.Contains("A0FX") || !file.Name.Contains(cp + "A0FX")) continue;
                String[] s = file.Name.Split('_');
                if(s[5].Contains(".trade"))
                {
                    string[] temp = s[5].Split('.');
                    Source[4, fileNum].Value = temp[0];
                }
                else
                {
                    Source[4, fileNum].Value = s[5];
                }
                Source[0, fileNum].Value = s[0];
                Source[1, fileNum].Value = s[3];
                Source[2, fileNum].Value = s[1];
                Source[3, fileNum].Value = s[4];
                fileNum += 1;
            }
            Source.RowCount = fileNum + 1;

            if (!Directory.Exists(SourceBox.Text + "\\Processed")) Directory.CreateDirectory(SourceBox.Text + "\\Processed");
            sf = new DirectoryInfo(SourceBox.Text + "\\Processed");
            files = sf.GetFiles();
            fileNum = 0;
            Processed.RowCount = 1000;
            Processed.ColumnCount = 10;
            foreach (FileInfo file in files)
            {
                if (!file.Name.Contains("A0FX") || !file.Name.Contains(cp + "A0FX")) continue;
                String[] s = file.Name.Split('_');
                if (s[5].Contains(".trade"))
                {
                    string[] temp = s[5].Split('.');
                    Processed[4, fileNum].Value = temp[0];
                }
                else
                {
                    Processed[4, fileNum].Value = s[5];
                }
                Processed[0, fileNum].Value = s[0];
                Processed[1, fileNum].Value = s[3];
                Processed[2, fileNum].Value = s[1];
                Processed[3, fileNum].Value = s[4];
                fileNum += 1;
            }
            Processed.RowCount = fileNum + 1;
            
            if (!Directory.Exists(DestinationBox.Text)) Directory.CreateDirectory(DestinationBox.Text);
            sf = new DirectoryInfo(DestinationBox.Text);
            files = sf.GetFiles();
            fileNum = 0;
            Destination.RowCount = 1000;
            Destination.ColumnCount = 10;
            foreach (FileInfo file in files)
            {
                if (!file.Name.Contains("A0FX") || !file.Name.Contains(cp + "A0FX")) continue;
                String[] s = file.Name.Split('_');
                if (s[5].Contains(".csv"))
                {
                    string[] temp = s[5].Split('.');
                    Destination[4, fileNum].Value = temp[0];
                }
                else
                {
                    Destination[4, fileNum].Value = s[5];
                }
                Destination[0, fileNum].Value = s[0];
                Destination[1, fileNum].Value = s[3];
                Destination[2, fileNum].Value = s[1];
                Destination[3, fileNum].Value = s[4];
                fileNum += 1;
            }
            Destination.RowCount = fileNum + 1;

            DataTable sourceTable = new DataTable();
            sourceTable.Columns.Add("Currency Pair");
            sourceTable.Columns.Add("Trade ID");
            sourceTable.Columns.Add("Order Type");
            sourceTable.Columns.Add("Ticket Date");
            sourceTable.Columns.Add("Ticket Time");
            int t = 0;
            foreach (DataGridViewRow row in Source.Rows)
            {
                sourceTable.Rows.Add(Source[0, t].Value, Source[1, t].Value, Source[2, t].Value, Source[3, t].Value, Source[4, t].Value);
                t++;
            }
            sourceTable.DefaultView.Sort = ("Ticket Date, Ticket Time, Trade ID");
            Source.Columns.Clear();
            Source.DataSource = sourceTable;
            Source.Rows.RemoveAt(0);

            DataTable processedTable = new DataTable();
            processedTable.Columns.Add("Currency Pair");
            processedTable.Columns.Add("Trade ID");
            processedTable.Columns.Add("Order Type");
            processedTable.Columns.Add("Ticket Date");
            processedTable.Columns.Add("Ticket Time");
            t = 0;
            foreach (DataGridViewRow row in Processed.Rows)
            {
                processedTable.Rows.Add(Processed[0, t].Value, Processed[1, t].Value, Processed[2, t].Value, Processed[3, t].Value, Processed[4, t].Value);
                t++;
            }
            processedTable.DefaultView.Sort = ("Ticket Date, Ticket Time, Trade ID");
            Processed.Columns.Clear();
            Processed.DataSource = processedTable;
            if (!Processed.Rows[0].IsNewRow) Processed.Rows.RemoveAt(0);

            DataTable destinationTable = new DataTable();
            destinationTable.Columns.Add("Currency Pair");
            destinationTable.Columns.Add("Trade ID");
            destinationTable.Columns.Add("Order Type");
            destinationTable.Columns.Add("Ticket Date");
            destinationTable.Columns.Add("Ticket Time");
            t = 0;
            foreach (DataGridViewRow row in Destination.Rows)
            {
                destinationTable.Rows.Add(Destination[0, t].Value, Destination[1, t].Value, Destination[2, t].Value, Destination[3, t].Value, Destination[4, t].Value);
                t++;
            }
            destinationTable.DefaultView.Sort = ("Ticket Date, Ticket Time, Trade ID");
            Destination.Columns.Clear();
            Destination.DataSource = destinationTable;
            if (!Destination.Rows[0].IsNewRow) Destination.Rows.RemoveAt(0);
            
            foreach (DataGridViewRow row in Destination.Rows)
            {
                row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
            }
            foreach (DataGridViewRow row in Processed.Rows)
            {
                row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
            }
            foreach (DataGridViewRow row in Source.Rows)
            {
                row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
            }
            Source.Refresh();
            Processed.Refresh();
            Destination.Refresh();
        }

        private void SourceLabel_Click(object sender, EventArgs e)
        {
            if (SourceBox.Text != "") System.Diagnostics.Process.Start(Convert.ToString(SourceBox.Text));
        }

        private void ProcessedLabel_Click(object sender, EventArgs e)
        {
            if (SourceBox.Text != "") System.Diagnostics.Process.Start(Convert.ToString(SourceBox.Text + "\\Processed"));
        }

        private void DestinationLabel_Click(object sender, EventArgs e)
        {
            if (DestinationBox.Text != "") System.Diagnostics.Process.Start(Convert.ToString(DestinationBox.Text));
        }

        private void MT4Box_MouseDoubleClick(object sender, EventArgs e)
        {
            if (MT4Box.Text != "") openFileDialog1.FileName = Convert.ToString(MT4Box.Text);
            if (openFileDialog1.ShowDialog() == DialogResult.OK) MT4Box.Text = openFileDialog1.FileName;
        }

        private void eSigBox_MouseDoubleClick(object sender, EventArgs e)
        {
            if (eSigBox.Text != "") folderBrowserDialog1.SelectedPath = Convert.ToString(eSigBox.Text);
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) eSigBox.Text = folderBrowserDialog1.SelectedPath;
        }

        private void MT4Label_Click(object sender, EventArgs e)
        {
            if (MT4Box.Text != "") System.Diagnostics.Process.Start(Convert.ToString(MT4Box.Text));
        }

        private void eSigLabel_Click(object sender, EventArgs e)
        {
            if (eSigBox.Text != "") System.Diagnostics.Process.Start(Convert.ToString(eSigBox.Text));
        }

        private void SourceBox_MouseDoubleClick(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = Convert.ToString(SourceBox.Text);
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) SourceBox.Text = folderBrowserDialog1.SelectedPath;
        }

        private void DestinationBox_MouseDoubleClick(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = Convert.ToString(DestinationBox.Text);
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) DestinationBox.Text = folderBrowserDialog1.SelectedPath;
        }

        private void TargetMode_CheckChanged(object sender, EventArgs e)
        {
            if (MT4TargetMode == true)
            {
                MT4TargetMode = false;
            }
            else
            {
                MT4TargetMode = true;
                MT4Button.Checked = true;
            }
        }

        private void CADLoadButton_Click(object sender, EventArgs e)
        {
            Load("CAD");
        }

        private void EURLoadButton_Click(object sender, EventArgs e)
        {
            Load("EUR");
        }
        
        private void GBPLoadButton_Click(object sender, EventArgs e)
        {
            Load("GBP");
        }

        private void AUDLoadButton_Click(object sender, EventArgs e)
        {
            Load("AUD");
        }

        private void NZDLoadButton_Click(object sender, EventArgs e)
        {
            Load("NZD");
        }

        private void JPYLoadButton_Click(object sender, EventArgs e)
        {
            Load("JPY");
        }

        private void CHFLoadButton_Click(object sender, EventArgs e)
        {
            Load("CHF");
        }

        private void CADPlay_Click(object sender, EventArgs e)
        {
            if (CADAutomaticButton.Checked != true)
            {
                MessageBox.Show("These button require automatic mode to be enabled", "Error");
                return;
            }
            CADPlay.BackColor = Color.Black;
            CADTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(CADSecondsPerTicket.Text));
            CADTimer.Enabled = true;
        }

        private void CADPause_Click(object sender, EventArgs e)
        {
            CADTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(CADSecondsPerTicket.Text));
            CADTimer.Enabled = false;
        }

        private void CADStop_Click(object sender, EventArgs e)
        {
            CADTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(CADSecondsPerTicket.Text));
            CADTimer.Enabled = false;
        }

        private void CADTimer_Tick(object sender, EventArgs e)
        {
            CADThread = new System.Threading.Thread(() => Manual("CAD"));
            CADThread.Start();
            Load("CAD");
        }

        private void EURPlayButton_Click(object sender, EventArgs e)
        {
            if (EURAutomaticButton.Checked != true)
            {
                MessageBox.Show("These button require automatic mode to be enabled", "Error");
                return;
            }
            EURTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(EURSecondsPerTicket.Text));
            EURTimer.Enabled = true;
        }

        private void EURPauseButton_Click(object sender, EventArgs e)
        {
            EURTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(EURSecondsPerTicket.Text));
            EURTimer.Enabled = false;
        }

        private void EURStopButton_Click(object sender, EventArgs e)
        {
            EURTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(EURSecondsPerTicket.Text));
            EURTimer.Enabled = false;
        }

        private void EURTimer_Tick(object sender, EventArgs e)
        {
            EURThread = new System.Threading.Thread(() => Manual("EUR"));
            EURThread.Start();
            Load("EUR");
        }

        private void GBPPlayButton_Click(object sender, EventArgs e)
        {
            if (GBPAutomaticButton.Checked != true)
            {
                MessageBox.Show("These button require automatic mode to be enabled", "Error");
                return;
            }
            GBPTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(GBPSecondsPerTicket.Text));
            GBPTimer.Enabled = true;
        }

        private void GBPPauseButton_Click(object sender, EventArgs e)
        {
            GBPTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(GBPSecondsPerTicket.Text));
            GBPTimer.Enabled = false;
        }

        private void GBPStopButton_Click(object sender, EventArgs e)
        {
            GBPTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(GBPSecondsPerTicket.Text));
            GBPTimer.Enabled = false;
        }

        private void GBPTimer_Tick(object sender, EventArgs e)
        {
            GBPThread = new System.Threading.Thread(() => Manual("GBP"));
            GBPThread.Start();
            Load("GBP");
        }

        private void AUDPlayButton_Click(object sender, EventArgs e)
        {
            if (AUDAutomaticButton.Checked != true)
            {
                MessageBox.Show("These button require automatic mode to be enabled", "Error");
                return;
            }
            AUDTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(AUDSecondsPerTicket.Text));
            AUDTimer.Enabled = true;
        }

        private void AUDPauseButton_Click(object sender, EventArgs e)
        {
            AUDTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(AUDSecondsPerTicket.Text));
            AUDTimer.Enabled = false;
        }

        private void AUDStopButton_Click(object sender, EventArgs e)
        {
            AUDTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(AUDSecondsPerTicket.Text));
            AUDTimer.Enabled = false;
        }

        private void AUDTimer_Tick(object sender, EventArgs e)
        {
            AUDThread = new System.Threading.Thread(() => Manual("AUD"));
            AUDThread.Start();
            Load("AUD");
        }

        private void NZDPlayButton_Click(object sender, EventArgs e)
        {
            if (NZDAutomaticButton.Checked != true)
            {
                MessageBox.Show("These button require automatic mode to be enabled", "Error");
                return;
            }
            NZDTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(NZDSecondsPerTicket.Text));
            NZDTimer.Enabled = true;
        }

        private void NZDPauseButton_Click(object sender, EventArgs e)
        {
            NZDTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(NZDSecondsPerTicket.Text));
            NZDTimer.Enabled = false;
        }

        private void NZDStopButton_Click(object sender, EventArgs e)
        {
            NZDTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(NZDSecondsPerTicket.Text));
            NZDTimer.Enabled = false;
        }

        private void NZDTimer_Tick(object sender, EventArgs e)
        {
            NZDThread = new System.Threading.Thread(() => Manual("NZD"));
            NZDThread.Start();
            Load("NZD");
        }

        private void JPYPlayButton_Click(object sender, EventArgs e)
        {
            if (JPYAutomaticButton.Checked != true)
            {
                MessageBox.Show("These button require automatic mode to be enabled", "Error");
                return;
            }
            JPYTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(JPYSecondsPerTicket.Text));
            JPYTimer.Enabled = true;
        }

        private void JPYPauseButton_Click(object sender, EventArgs e)
        {
            JPYTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(JPYSecondsPerTicket.Text));
            JPYTimer.Enabled = false;
        }

        private void JPYStopButton_Click(object sender, EventArgs e)
        {
            JPYTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(JPYSecondsPerTicket.Text));
            JPYTimer.Enabled = false;
        }

        private void JPYTimer_Tick(object sender, EventArgs e)
        {
            JPYThread = new System.Threading.Thread(() => Manual("JPY"));
            JPYThread.Start();
            Load("JPY");
        }

        private void CHFPlayButton_Click(object sender, EventArgs e)
        {
            if (CHFAutomaticButton.Checked != true)
            {
                MessageBox.Show("These button require automatic mode to be enabled", "Error");
                return;
            }
            CHFTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(CHFSecondsPerTicket.Text));
            CHFTimer.Enabled = true;
        }

        private void CHFPauseButton_Click(object sender, EventArgs e)
        {
            CHFTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(CHFSecondsPerTicket.Text));
            CHFTimer.Enabled = false;
        }

        private void CHFStopButton_Click(object sender, EventArgs e)
        {
            CHFTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(CHFSecondsPerTicket.Text));
            CHFTimer.Enabled = false;
        }

        private void CHFTimer_Tick(object sender, EventArgs e)
        {
            CHFThread = new System.Threading.Thread(() => Manual("CHF"));
            CHFThread.Start();
            Load("CHF");
        }

        private void CADExecuteButton_Click(object sender, EventArgs e)
        {
            if(loaded[0] == false)
            {
                MessageBox.Show("Please load tickets before executing", "Error");
                return;
            }
            else if (CADManualButton.Checked)
            {
                CADThread = new System.Threading.Thread(() => Manual("CAD"));
                CADThread.Start();
                //FlatFlatThread = new System.Threading.Thread(() => FlatFlatChecker("CAD"));
                //FlatFlatThread.Start();
                Load("CAD");
            }
            else
            {
                CADTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(CADSecondsPerTicket.Text));
                CADTimer.Enabled = true;
            }
        }

        private void EURExecuteButton_Click(object sender, EventArgs e)
        {
            if (loaded[1] == false)
            {
                MessageBox.Show("Please load tickets before executing", "Error");
                return;
            }
            else if (EURManualButton.Checked)
            {
                EURThread = new System.Threading.Thread(() => Manual("EUR"));
                EURThread.Start();
                //FlatFlatThread = new System.Threading.Thread(() => FlatFlatChecker("EUR"));
                //FlatFlatThread.Start();
                Load("EUR");
            }
            else
            {
                EURTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(EURSecondsPerTicket.Text));
                EURTimer.Enabled = true;
            }
        }

        private void GBPExecuteButton_Click(object sender, EventArgs e)
        {
            if (loaded[2] == false)
            {
                MessageBox.Show("Please load tickets before executing", "Error");
                return;
            }
            else if (GBPManualButton.Checked)
            {
                GBPThread = new System.Threading.Thread(() => Manual("GBP"));
                GBPThread.Start();
                //FlatFlatThread = new System.Threading.Thread(() => FlatFlatChecker("GBP"));
                //FlatFlatThread.Start();
                Load("GBP");
            }
            else
            {
                GBPTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(EURSecondsPerTicket.Text));
                GBPTimer.Enabled = true;
            }
        }

        private void AUDExecuteButton_Click(object sender, EventArgs e)
        {
            if (loaded[3] == false)
            {
                MessageBox.Show("Please load tickets before executing", "Error");
                return;
            }
            else if (AUDManualButton.Checked)
            {
                AUDThread = new System.Threading.Thread(() => Manual("AUD"));
                AUDThread.Start();
                //FlatFlatThread = new System.Threading.Thread(() => FlatFlatChecker("AUD"));
                //FlatFlatThread.Start();
                Load("AUD");
            }
            else
            {
                AUDTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(EURSecondsPerTicket.Text));
                AUDTimer.Enabled = true;
            }
        }

        private void NZDExecuteButton_Click(object sender, EventArgs e)
        {
            if (loaded[4] == false)
            {
                MessageBox.Show("Please load tickets before executing", "Error");
                return;
            }
            else if (NZDManualButton.Checked)
            {
                NZDThread = new System.Threading.Thread(() => Manual("NZD"));
                NZDThread.Start();
                //FlatFlatThread = new System.Threading.Thread(() => FlatFlatChecker("NZD"));
                //FlatFlatThread.Start();
                Load("NZD");
            }
            else
            {
                NZDTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(EURSecondsPerTicket.Text));
                NZDTimer.Enabled = true;
            }
        }

        private void JPYExecuteButton_Click(object sender, EventArgs e)
        {
            if (loaded[5] == false)
            {
                MessageBox.Show("Please load tickets before executing", "Error");
                return;
            }
            else if (JPYManualButton.Checked)
            {
                JPYThread = new System.Threading.Thread(() => Manual("JPY"));
                JPYThread.Start();
                //FlatFlatThread = new System.Threading.Thread(() => FlatFlatChecker("JPY"));
                //FlatFlatThread.Start();
                Load("JPY");
            }
            else
            {
                JPYTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(EURSecondsPerTicket.Text));
                JPYTimer.Enabled = true;
            }
        }

        private void CHFExecuteButton_Click(object sender, EventArgs e)
        {
            if (loaded[6] == false)
            {
                MessageBox.Show("Please load tickets before executing", "Error");
                return;
            }
            else if (EURManualButton.Checked)
            {
                CHFThread = new System.Threading.Thread(() => Manual("CHF"));
                CHFThread.Start();
                //FlatFlatThread = new System.Threading.Thread(() => FlatFlatChecker("CHF"));
                //FlatFlatThread.Start();
                Load("CHF");
            }
            else
            {
                CHFTimer.Interval = Convert.ToInt32(1000 * Convert.ToDouble(EURSecondsPerTicket.Text));
                CHFTimer.Enabled = true;
            }
        }

        private void Manual(string cp)
        {
            string sigFig = "";
            if (MT4Button.Checked)
            {
                if (MT4Box.Text == "")
                {
                    MessageBox.Show("MT4 location cannot be empty", "Error");
                    return;
                }
                string[] fr = null;
                try
                {
                    fr = File.ReadAllLines(MT4Box.Text);
                }
                catch (System.IO.IOException)
                {
                    int l = 0;
                    while (l == 0)
                    {
                        try
                        {
                            fr = File.ReadAllLines(MT4Box.Text);
                            l = 1;
                        }
                        catch (System.IO.IOException)
                        {
                            l = 0;
                        }
                        if (l != 0)
                        {
                            l = 1;
                        }
                    }
                }
                foreach (string s in fr)
                {
                    if (s.Contains("Time")) continue;
                    string[] split = s.Split(',');
                    string[] temp1 = split[2].Split('.');
                    char[] c = temp1[1].ToCharArray();
                    string final;
                    sigFig = "";
                    if (split[4] == "FLAT" && split[5] == "FLAT")
                    {
                        //CAD
                        //EUR
                        //GBP
                        //AUD
                        //NZD
                        //JPY
                        //CHF
                        if (split[1].Contains("CAD"))
                        {
                            FlatFlat[0] = true;
                        }
                        if (split[1].Contains("EUR"))
                        {
                            FlatFlat[1] = true;
                        }
                        if (split[1].Contains("GBP"))
                        {
                            FlatFlat[2] = true;
                        }
                        if (split[1].Contains("AUD"))
                        {
                            FlatFlat[3] = true;
                        }
                        if (split[1].Contains("NZD"))
                        {
                            FlatFlat[4] = true;
                        }
                        if (split[1].Contains("JPY"))
                        {
                            FlatFlat[5] = true;
                        }
                        if (split[1].Contains("CHF"))
                        {
                            FlatFlat[6] = true;
                        }
                    }
                    if (split[4] != "FLAT" || split[5] != "FLAT")
                    {
                        //CAD
                        //EUR
                        //GBP
                        //AUD
                        //NZD
                        //JPY
                        //CHF
                        if (split[1].Contains("CAD"))
                        {
                            FlatFlat[0] = false;
                        }
                        if (split[1].Contains("EUR"))
                        {
                            FlatFlat[1] = false;
                        }
                        if (split[1].Contains("GBP"))
                        {
                            FlatFlat[2] = false;
                        }
                        if (split[1].Contains("AUD"))
                        {
                            FlatFlat[3] = false;
                        }
                        if (split[1].Contains("NZD"))
                        {
                            FlatFlat[4] = false;
                        }
                        if (split[1].Contains("JPY"))
                        {
                            FlatFlat[5] = false;
                        }
                        if (split[1].Contains("CHF"))
                        {
                            FlatFlat[6] = false;
                        }
                    }
                    for (int l = 0; l < 5; l++)
                    {
                        if (c.Length >= 5) sigFig += c[l];
                        else
                        {
                            if (l < c.Length) sigFig += c[l];
                            else sigFig += "0";
                        }
                    }
                    temp1[1] = sigFig;
                    final = String.Join(".", temp1);
                    split[2] = final;
                    if (split[1].Contains("CAD"))
                    {
                        CADPriceGrid.Rows[0].Cells[1].Value = split[2];
                        if (FlatFlat[0]) CADPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                        else if (split[5] == "BUY") CADPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                        else if (split[5] == "SELL") CADPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                    }
                    else if (split[1].Contains("EUR"))
                    {
                        EURPriceGrid.Rows[0].Cells[1].Value = split[2];
                        if (FlatFlat[1]) EURPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                        else if (split[5] == "BUY") EURPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                        else if (split[5] == "SELL") EURPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                    }
                    else if (split[1].Contains("GBP"))
                    {
                        GBPPriceGrid.Rows[0].Cells[1].Value = split[2];
                        if (FlatFlat[2]) GBPPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                        else if (split[5] == "BUY") GBPPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                        else if (split[5] == "SELL") GBPPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                    }
                    else if (split[1].Contains("AUD"))
                    {
                        AUDPriceGrid.Rows[0].Cells[1].Value = split[2];
                        if (FlatFlat[3]) AUDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                        else if (split[5] == "BUY") AUDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                        else if (split[5] == "SELL") AUDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                    }
                    else if (split[1].Contains("NZD"))
                    {
                        NZDPriceGrid.Rows[0].Cells[1].Value = split[2];
                        if (FlatFlat[4]) NZDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                        else if (split[5] == "BUY") NZDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                        else if (split[5] == "SELL") NZDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                    }
                    else if (split[1].Contains("JPY"))
                    {
                        JPYPriceGrid.Rows[0].Cells[1].Value = split[2];
                        if (FlatFlat[5]) JPYPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                        else if (split[5] == "BUY") JPYPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                        else if (split[5] == "SELL") JPYPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                    }
                    else if (split[1].Contains("CHF"))
                    {
                        CHFPriceGrid.Rows[0].Cells[1].Value = split[2];
                        if (FlatFlat[6]) CHFPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                        else if (split[5] == "BUY") CHFPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                        else if (split[5] == "SELL") CHFPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                    }
                }
            }
            else if (eSigButton.Checked)
            {
                if (eSigBox.Text == "")
                {
                    MessageBox.Show("eSig location cannot be empty", "Error");
                    return;
                }
                FileInfo[] eSigFiles = new DirectoryInfo(eSigBox.Text).GetFiles();

                foreach (FileInfo f in eSigFiles)
                {
                    if (!f.Name.Contains("A0-FX")) continue;
                    string s = f.Name;
                    string[] fr = null;
                    try
                    {
                        fr = File.ReadAllLines(f.FullName);
                    }
                    catch (System.IO.IOException)
                    {
                        int l = 0;
                        while (l == 0)
                        {
                            try
                            {
                                fr = File.ReadAllLines(f.FullName);
                                l = 1;
                            }
                            catch (System.IO.IOException)
                            {
                                l = 0;
                            }
                            if (l != 0)
                            {
                                l = 1;
                            }
                        }
                    }
                    string[] split = fr[0].Split(',');
                    sigFig = "";
                    string[] temp1 = split[2].Split('.');
                    char[] c = temp1[1].ToCharArray();
                    string final;
                    for (int l = 0; l < 5; l++)
                    {
                        if (c.Length >= 5) sigFig += c[l];
                        else
                        {
                            if (l < c.Length) sigFig += c[l];
                            else sigFig += "0";
                        }
                    }
                    temp1[1] = sigFig;
                    final = String.Join(".", temp1);
                    split[2] = final;
                    if (split[1].Contains("CAD")) CADPriceGrid.Rows[0].Cells[1].Value = split[2];
                    else if (split[1].Contains("EUR")) EURPriceGrid.Rows[0].Cells[1].Value = split[2];
                    else if (split[1].Contains("GBP")) GBPPriceGrid.Rows[0].Cells[1].Value = split[2];
                    else if (split[1].Contains("AUD")) AUDPriceGrid.Rows[0].Cells[1].Value = split[2];
                    else if (split[1].Contains("NZD")) NZDPriceGrid.Rows[0].Cells[1].Value = split[2];
                    else if (split[1].Contains("JPY")) JPYPriceGrid.Rows[0].Cells[1].Value = split[2];
                    else if (split[1].Contains("CHF")) CHFPriceGrid.Rows[0].Cells[1].Value = split[2];
                }
            }
            DirectoryInfo sf = new DirectoryInfo(SourceBox.Text);
            FileInfo[] files = sf.GetFiles();
            string currency = "";
            string name = "";
            string date = "";
            string time = "";

            foreach (FileInfo file in files)
            {
                if (!file.Name.Contains("A0FX") || !file.Name.Contains(cp)) continue;
                switch (cp)
                {
                    case "CAD":
                        currency = Convert.ToString(CADSourceGrid.Rows[0].Cells[0].Value);
                        id = Convert.ToString(CADSourceGrid.Rows[0].Cells[1].Value);
                        name = Convert.ToString(CADSourceGrid.Rows[0].Cells[2].Value);
                        time = Convert.ToString(CADSourceGrid.Rows[0].Cells[4].Value);
                        date = Convert.ToString(CADSourceGrid.Rows[0].Cells[3].Value);
                        break;
                    case "EUR":
                        currency = Convert.ToString(EURSourceGrid.Rows[0].Cells[0].Value);
                        id = Convert.ToString(EURSourceGrid.Rows[0].Cells[1].Value);
                        name = Convert.ToString(EURSourceGrid.Rows[0].Cells[2].Value);
                        time = Convert.ToString(EURSourceGrid.Rows[0].Cells[4].Value);
                        date = Convert.ToString(EURSourceGrid.Rows[0].Cells[3].Value);
                        break;
                    case "GBP":
                        currency = Convert.ToString(GBPSourceGrid.Rows[0].Cells[0].Value);
                        id = Convert.ToString(GBPSourceGrid.Rows[0].Cells[1].Value);
                        name = Convert.ToString(GBPSourceGrid.Rows[0].Cells[2].Value);
                        time = Convert.ToString(GBPSourceGrid.Rows[0].Cells[4].Value);
                        date = Convert.ToString(GBPSourceGrid.Rows[0].Cells[3].Value);
                        break;
                    case "AUD":
                        currency = Convert.ToString(AUDSourceGrid.Rows[0].Cells[0].Value);
                        id = Convert.ToString(AUDSourceGrid.Rows[0].Cells[1].Value);
                        name = Convert.ToString(AUDSourceGrid.Rows[0].Cells[2].Value);
                        time = Convert.ToString(AUDSourceGrid.Rows[0].Cells[4].Value);
                        date = Convert.ToString(AUDSourceGrid.Rows[0].Cells[3].Value);
                        break;
                    case "NZD":
                        currency = Convert.ToString(NZDSourceGrid.Rows[0].Cells[0].Value);
                        id = Convert.ToString(NZDSourceGrid.Rows[0].Cells[1].Value);
                        name = Convert.ToString(NZDSourceGrid.Rows[0].Cells[2].Value);
                        time = Convert.ToString(NZDSourceGrid.Rows[0].Cells[4].Value);
                        date = Convert.ToString(NZDSourceGrid.Rows[0].Cells[3].Value);
                        break;
                    case "JPY":
                        currency = Convert.ToString(JPYSourceGrid.Rows[0].Cells[0].Value);
                        id = Convert.ToString(JPYSourceGrid.Rows[0].Cells[1].Value);
                        name = Convert.ToString(JPYSourceGrid.Rows[0].Cells[2].Value);
                        time = Convert.ToString(JPYSourceGrid.Rows[0].Cells[4].Value);
                        date = Convert.ToString(JPYSourceGrid.Rows[0].Cells[3].Value);
                        break;
                    case "CHF":
                        currency = Convert.ToString(CHFSourceGrid.Rows[0].Cells[0].Value);
                        id = Convert.ToString(CHFSourceGrid.Rows[0].Cells[1].Value);
                        name = Convert.ToString(CHFSourceGrid.Rows[0].Cells[2].Value);
                        time = Convert.ToString(CHFSourceGrid.Rows[0].Cells[4].Value);
                        date = Convert.ToString(CHFSourceGrid.Rows[0].Cells[3].Value);
                        break;
                }
                if (!file.Name.Contains(time) || !file.Name.Contains(date) || !file.Name.Contains(name)) continue;
                char[] ch = file.Name.ToCharArray();
                string fileName = file.Name;
                string[] newName;
                if (MT4TargetMode)
                {
                    DataGridView price = null;
                    int i = 0;
                    switch (cp)
                    {
                        case "CAD":
                            price = CADPriceGrid;
                            i = 0;
                            break;
                        case "EUR":
                            price = EURPriceGrid;
                            i = 1;
                            break;
                        case "GBP":
                            price = GBPPriceGrid;
                            i = 2;
                            break;
                        case "AUD":
                            price = AUDPriceGrid;
                            i = 3;
                            break;
                        case "NZD":
                            price = NZDPriceGrid;
                            i = 4;
                            break;
                        case "JPY":
                            price = JPYPriceGrid;
                            i = 5;
                            break;
                        case "CHF":
                            price = CHFPriceGrid;
                            i = 6;
                            break;
                    }
                    if (name.Contains("OpenBuy")) price.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (name.Contains("OpenSell")) price.Rows[0].Cells[3].Style.BackColor = Color.Red;
                    else if (name.Equals("CloseBuy") || name.Equals("CloseSell"))
                    {
                        Console.WriteLine("wrong name");
                        if (FlatFlat[i])
                        {
                            price.Rows[0].Cells[3].Style.BackColor = Color.Black;
                            foreach (FileInfo file2 in files)
                            {
                                if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                {
                                    file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                    file2.Delete();
                                }
                            }
                        }
                        else
                        {
                            do
                            {
                                string[] fr = null;
                                try
                                {
                                    fr = File.ReadAllLines(MT4Box.Text);
                                }
                                catch (System.IO.IOException)
                                {
                                    int l = 0;
                                    while (l == 0)
                                    {
                                        try
                                        {
                                            fr = File.ReadAllLines(MT4Box.Text);
                                            l = 1;
                                        }
                                        catch (System.IO.IOException)
                                        {
                                            l = 0;
                                        }
                                        if (l != 0)
                                        {
                                            l = 1;
                                        }
                                    }
                                }
                                foreach (string s in fr)
                                {
                                    if (s.Contains("Time")) continue;
                                    string[] split = s.Split(',');
                                    string[] temp1 = split[2].Split('.');
                                    char[] c = temp1[1].ToCharArray();
                                    sigFig = "";
                                    if (split[4] == "FLAT" && split[5] == "FLAT")
                                    {
                                        //CAD
                                        //EUR
                                        //GBP
                                        //AUD
                                        //NZD
                                        //JPY
                                        //CHF
                                        if (split[1].Contains("CAD"))
                                        {
                                            FlatFlat[0] = true;
                                        }
                                        if (split[1].Contains("EUR"))
                                        {
                                            FlatFlat[1] = true;
                                        }
                                        if (split[1].Contains("GBP"))
                                        {
                                            FlatFlat[2] = true;
                                        }
                                        if (split[1].Contains("AUD"))
                                        {
                                            FlatFlat[3] = true;
                                        }
                                        if (split[1].Contains("NZD"))
                                        {
                                            FlatFlat[4] = true;
                                        }
                                        if (split[1].Contains("JPY"))
                                        {
                                            FlatFlat[5] = true;
                                        }
                                        if (split[1].Contains("CHF"))
                                        {
                                            FlatFlat[6] = true;
                                        }
                                    }
                                    if (split[4] != "FLAT" || split[5] != "FLAT")
                                    {
                                        //CAD
                                        //EUR
                                        //GBP
                                        //AUD
                                        //NZD
                                        //JPY
                                        //CHF
                                        if (split[1].Contains("CAD"))
                                        {
                                            FlatFlat[0] = false;
                                        }
                                        if (split[1].Contains("EUR"))
                                        {
                                            FlatFlat[1] = false;
                                        }
                                        if (split[1].Contains("GBP"))
                                        {
                                            FlatFlat[2] = false;
                                        }
                                        if (split[1].Contains("AUD"))
                                        {
                                            FlatFlat[3] = false;
                                        }
                                        if (split[1].Contains("NZD"))
                                        {
                                            FlatFlat[4] = false;
                                        }
                                        if (split[1].Contains("JPY"))
                                        {
                                            FlatFlat[5] = false;
                                        }
                                        if (split[1].Contains("CHF"))
                                        {
                                            FlatFlat[6] = false;
                                        }
                                    }
                                }
                            } while (!FlatFlat[i]);
                            foreach (FileInfo file2 in files)
                            {
                                if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                {
                                    file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                    file2.Delete();
                                }
                            }
                            price.Rows[0].Cells[3].Style.BackColor = Color.Black;
                        }
                        break;
                    }
                }
                newName = fileName.Split('.');
                newName[1] = ".csv";
                file.CopyTo(SourceBox.Text + "\\Processed\\" + file.Name, true);
                file.CopyTo(DestinationBox.Text + "\\" + newName[0] + newName[1], true);
                if (name == "OpenBuy" || name == "OpenSell" || name == "ReverseBuy")
                {
                    string[] fileOverwrite = System.IO.File.ReadAllText(DestinationBox.Text + "\\" + newName[0] + newName[1]).Split(',');
                    double currencyNum = 0;
                    double limitNum = 0;
                    switch (cp)
                    {
                        case "CAD":
                            currencyNum = (Convert.ToDouble(CADPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(CADPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "EUR":
                            currencyNum = (Convert.ToDouble(EURPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(EURPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "GBP":
                            currencyNum = (Convert.ToDouble(GBPPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(GBPPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "AUD":
                            currencyNum = (Convert.ToDouble(AUDPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(AUDPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "NZD":
                            currencyNum = (Convert.ToDouble(NZDPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(NZDPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "JPY":
                            currencyNum = (Convert.ToDouble(JPYPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(JPYPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "CHF":
                            currencyNum = (Convert.ToDouble(CHFPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(CHFPriceGrid.Rows[0].Cells[2].Value));
                            break;
                    }
                    fileOverwrite[9] = currencyNum.ToString();
                    fileOverwrite[10] = currencyNum.ToString();
                    fileOverwrite[11] = (currencyNum - limitNum).ToString();
                    fileOverwrite[12] = (currencyNum + limitNum).ToString();
                    using (FileStream fs = File.Create(DestinationBox.Text + "\\" + newName[0] + newName[1]))
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes(String.Join(",", fileOverwrite));
                        fs.Write(info, 0, info.Length);
                    }
                }
                else if (name == "partialCloseBuy")
                {
                    string[] fileOverwrite = System.IO.File.ReadAllText(DestinationBox.Text + "\\" + newName[0] + newName[1]).Split(',');
                    double currencyNum = 0;
                    double limitNum = 0;
                    switch (cp)
                    {
                        case "CAD":
                            currencyNum = (Convert.ToDouble(CADPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(CADPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "EUR":
                            currencyNum = (Convert.ToDouble(EURPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(EURPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "GBP":
                            currencyNum = (Convert.ToDouble(GBPPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(GBPPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "AUD":
                            currencyNum = (Convert.ToDouble(AUDPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(AUDPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "NZD":
                            currencyNum = (Convert.ToDouble(NZDPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(NZDPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "JPY":
                            currencyNum = (Convert.ToDouble(JPYPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(JPYPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "CHF":
                            currencyNum = (Convert.ToDouble(CHFPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(CHFPriceGrid.Rows[0].Cells[2].Value));
                            break;
                    }
                    fileOverwrite[9] = currencyNum.ToString();
                    fileOverwrite[15] = (currencyNum - limitNum).ToString();
                    using (FileStream fs = File.Create(DestinationBox.Text + "\\" + newName[0] + newName[1]))
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes(String.Join(",", fileOverwrite));
                        fs.Write(info, 0, info.Length);
                    }
                }
                else if (name == "partialCloseSell")
                {
                    string[] fileOverwrite = System.IO.File.ReadAllText(DestinationBox.Text + "\\" + newName[0] + newName[1]).Split(',');
                    double currencyNum = 0;
                    double limitNum = 0;
                    switch (cp)
                    {
                        case "CAD":
                            currencyNum = (Convert.ToDouble(CADPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(CADPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "EUR":
                            currencyNum = (Convert.ToDouble(EURPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(EURPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "GBP":
                            currencyNum = (Convert.ToDouble(GBPPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(GBPPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "AUD":
                            currencyNum = (Convert.ToDouble(AUDPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(AUDPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "NZD":
                            currencyNum = (Convert.ToDouble(NZDPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(NZDPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "JPY":
                            currencyNum = (Convert.ToDouble(JPYPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(JPYPriceGrid.Rows[0].Cells[2].Value));
                            break;
                        case "CHF":
                            currencyNum = (Convert.ToDouble(CHFPriceGrid.Rows[0].Cells[1].Value));
                            limitNum = (Convert.ToDouble(CHFPriceGrid.Rows[0].Cells[2].Value));
                            break;
                    }
                    fileOverwrite[9] = currencyNum.ToString();
                    fileOverwrite[15] = (currencyNum + limitNum).ToString();
                    using (FileStream fs = File.Create(DestinationBox.Text + "\\" + newName[0] + newName[1]))
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes(String.Join(",", fileOverwrite));
                        fs.Write(info, 0, info.Length);
                    }
                }
                file.Delete();
                currency = "";
                name = "";
                date = "";
                time = "";
                break;
            }
        }

        private void CADReset_Click(object sender, EventArgs e)
        {
            CADPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.White;

            FileInfo[] files = new DirectoryInfo(DestinationBox.Text).GetFiles();
            FileInfo[] files2 = new DirectoryInfo(SourceBox.Text + "\\Processed").GetFiles();

            foreach (FileInfo f in files)
            {
                if (f.Name.Contains("A0FX") && f.Name.Contains("CAD")) f.Delete();
                else continue;
            }
            foreach (FileInfo f in files2)
            {
                if (!f.Name.Contains("A0FX") || !f.Name.Contains("CAD")) continue;
                f.CopyTo(SourceBox.Text + "\\" + f.Name, true);
                f.Delete();
            }
            Load("CAD");
        }

        private void EURResetButton_Click(object sender, EventArgs e)
        {
            EURPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.White;

            FileInfo[] files = new DirectoryInfo(DestinationBox.Text).GetFiles();
            FileInfo[] files2 = new DirectoryInfo(SourceBox.Text + "\\Processed").GetFiles();

            foreach (FileInfo f in files)
            {
                if (f.Name.Contains("A0FX") && f.Name.Contains("EUR")) f.Delete();
                else continue;
            }
            foreach (FileInfo f in files2)
            {
                if (!f.Name.Contains("A0FX") || !f.Name.Contains("EUR")) continue;
                f.CopyTo(SourceBox.Text + "\\" + f.Name, true);
                f.Delete();
            }
            Load("EUR");
        }

        private void GBPResetButton_Click(object sender, EventArgs e)
        {
            GBPPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.White;

            FileInfo[] files = new DirectoryInfo(DestinationBox.Text).GetFiles();
            FileInfo[] files2 = new DirectoryInfo(SourceBox.Text + "\\Processed").GetFiles();

            foreach (FileInfo f in files)
            {
                if (f.Name.Contains("A0FX") && f.Name.Contains("GBP")) f.Delete();
                else continue;
            }
            foreach (FileInfo f in files2)
            {
                if (!f.Name.Contains("A0FX") || !f.Name.Contains("GBP")) continue;
                f.CopyTo(SourceBox.Text + "\\" + f.Name, true);
                f.Delete();
            }
            Load("GBP");
        }

        private void AUDResetButton_Click(object sender, EventArgs e)
        {
            AUDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.White;

            FileInfo[] files = new DirectoryInfo(DestinationBox.Text).GetFiles();
            FileInfo[] files2 = new DirectoryInfo(SourceBox.Text + "\\Processed").GetFiles();

            foreach (FileInfo f in files)
            {
                if (f.Name.Contains("A0FX") && f.Name.Contains("AUD")) f.Delete();
                else continue;
            }
            foreach (FileInfo f in files2)
            {
                if (!f.Name.Contains("A0FX") || !f.Name.Contains("AUD")) continue;
                f.CopyTo(SourceBox.Text + "\\" + f.Name, true);
                f.Delete();
            }
            Load("AUD");
        }

        private void NZDResetButton_Click(object sender, EventArgs e)
        {
            NZDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.White;

            FileInfo[] files = new DirectoryInfo(DestinationBox.Text).GetFiles();
            FileInfo[] files2 = new DirectoryInfo(SourceBox.Text + "\\Processed").GetFiles();

            foreach (FileInfo f in files)
            {
                if (f.Name.Contains("A0FX") && f.Name.Contains("NZD")) f.Delete();
                else continue;
            }
            foreach (FileInfo f in files2)
            {
                if (!f.Name.Contains("A0FX") || !f.Name.Contains("NZD")) continue;
                f.CopyTo(SourceBox.Text + "\\" + f.Name, true);
                f.Delete();
            }
            Load("NZD");
        }

        private void JPYResetButton_Click(object sender, EventArgs e)
        {
            JPYPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.White;

            FileInfo[] files = new DirectoryInfo(DestinationBox.Text).GetFiles();
            FileInfo[] files2 = new DirectoryInfo(SourceBox.Text + "\\Processed").GetFiles();

            foreach (FileInfo f in files)
            {
                if (f.Name.Contains("A0FX") && f.Name.Contains("JPY")) f.Delete();
                else continue;
            }
            foreach (FileInfo f in files2)
            {
                if (!f.Name.Contains("A0FX") || !f.Name.Contains("JPY")) continue;
                f.CopyTo(SourceBox.Text + "\\" + f.Name, true);
                f.Delete();
            }
            Load("JPY");
        }

        private void CHFResetButton_Click(object sender, EventArgs e)
        {
            CHFPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.White;

            FileInfo[] files = new DirectoryInfo(DestinationBox.Text).GetFiles();
            FileInfo[] files2 = new DirectoryInfo(SourceBox.Text + "\\Processed").GetFiles();

            foreach (FileInfo f in files)
            {
                if (f.Name.Contains("A0FX") && f.Name.Contains("CHF")) f.Delete();
                else continue;
            }
            foreach (FileInfo f in files2)
            {
                if (!f.Name.Contains("A0FX") || !f.Name.Contains("CHF")) continue;
                f.CopyTo(SourceBox.Text + "\\" + f.Name, true);
                f.Delete();
            }
            Load("CHF");
        }

        /*private void FlatFlatChecker(string cp)
        {
            Console.WriteLine("Running");
            DirectoryInfo sf = new DirectoryInfo(SourceBox.Text);
            FileInfo[] files = sf.GetFiles();
            string currency = "";
            string name = "";
            string date = "";
            string time = "";
            string[] fr = null;
            try
            {
                fr = File.ReadAllLines(MT4Box.Text);
            }
            catch (System.IO.IOException)
            {
                int l = 0;
                while (l == 0)
                {
                    try
                    {
                        fr = File.ReadAllLines(MT4Box.Text);
                        l = 1;
                    }
                    catch (System.IO.IOException)
                    {
                        l = 0;
                    }
                    if (l != 0)
                    {
                        l = 1;
                    }
                }
            }
            foreach (string s in fr)
            {
                if (s.Contains("Time")) continue;
                string[] split = s.Split(',');
                if (split[4] == "FLAT" && split[5] == "FLAT")
                {
                    //CAD
                    //EUR
                    //GBP
                    //AUD
                    //NZD
                    //JPY
                    //CHF
                    if (split[1].Contains("CAD"))
                    {
                        FlatFlat[0] = true;
                    }
                    if (split[1].Contains("EUR"))
                    {
                        FlatFlat[1] = true;
                    }
                    if (split[1].Contains("GBP"))
                    {
                        FlatFlat[2] = true;
                    }
                    if (split[1].Contains("AUD"))
                    {
                        FlatFlat[3] = true;
                    }
                    if (split[1].Contains("NZD"))
                    {
                        FlatFlat[4] = true;
                    }
                    if (split[1].Contains("JPY"))
                    {
                        FlatFlat[5] = true;
                    }
                    if (split[1].Contains("CHF"))
                    {
                        FlatFlat[6] = true;
                    }
                }
                if (split[4] != "FLAT" || split[5] != "FLAT")
                {
                    //CAD
                    //EUR
                    //GBP
                    //AUD
                    //NZD
                    //JPY
                    //CHF
                    if (split[1].Contains("CAD"))
                    {
                        FlatFlat[0] = false;
                    }
                    if (split[1].Contains("EUR"))
                    {
                        FlatFlat[1] = false;
                    }
                    if (split[1].Contains("GBP"))
                    {
                        FlatFlat[2] = false;
                    }
                    if (split[1].Contains("AUD"))
                    {
                        FlatFlat[3] = false;
                    }
                    if (split[1].Contains("NZD"))
                    {
                        FlatFlat[4] = false;
                    }
                    if (split[1].Contains("JPY"))
                    {
                        FlatFlat[5] = false;
                    }
                    if (split[1].Contains("CHF"))
                    {
                        FlatFlat[6] = false;
                    }
                }
                if (split[1].Contains("CAD"))
                {
                    CADPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[0]) CADPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") CADPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") CADPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
                else if (split[1].Contains("EUR"))
                {
                    EURPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[1]) EURPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") EURPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") EURPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
                else if (split[1].Contains("GBP"))
                {
                    GBPPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[2]) GBPPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") GBPPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") GBPPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
                else if (split[1].Contains("AUD"))
                {
                    AUDPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[3]) AUDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") AUDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") AUDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
                else if (split[1].Contains("NZD"))
                {
                    NZDPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[4]) NZDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") NZDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") NZDPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
                else if (split[1].Contains("JPY"))
                {
                    JPYPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[5]) JPYPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") JPYPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") JPYPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
                else if (split[1].Contains("CHF"))
                {
                    CHFPriceGrid.Rows[0].Cells[1].Value = split[2];
                    if (FlatFlat[6]) CHFPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Black;
                    else if (split[5] == "BUY") CHFPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Green;
                    else if (split[5] == "SELL") CHFPriceGrid.Rows[0].Cells[3].Style.BackColor = Color.Red;
                }
            }
            switch (cp)
            {
                case "CAD":
                    if (FlatFlat[0] == true) return; 
                    currency = Convert.ToString(CADSourceGrid.Rows[0].Cells[0].Value);
                    id = Convert.ToString(CADSourceGrid.Rows[0].Cells[1].Value);
                    name = Convert.ToString(CADSourceGrid.Rows[0].Cells[2].Value);
                    time = Convert.ToString(CADSourceGrid.Rows[0].Cells[4].Value);
                    date = Convert.ToString(CADSourceGrid.Rows[0].Cells[3].Value);
                    if (FlatFlat[0])
                    {
                        foreach (FileInfo file in files)
                        {
                            if (!file.Name.Contains("A0FX") || !file.Name.Contains(cp)) continue;
                            if (!file.Name.Contains(time) || !file.Name.Contains(date) || !file.Name.Contains(name)) continue;
                            char[] ch = file.Name.ToCharArray();
                            string fileName = file.Name;
                            if (MT4TargetMode)
                            {
                                DataGridView price = null;
                                int i = 0;
                                switch (cp)
                                {
                                    case "CAD":
                                        price = CADPriceGrid;
                                        i = 0;
                                        break;
                                    case "EUR":
                                        price = EURPriceGrid;
                                        i = 1;
                                        break;
                                    case "GBP":
                                        price = GBPPriceGrid;
                                        i = 2;
                                        break;
                                    case "AUD":
                                        price = AUDPriceGrid;
                                        i = 3;
                                        break;
                                    case "NZD":
                                        price = NZDPriceGrid;
                                        i = 4;
                                        break;
                                    case "JPY":
                                        price = JPYPriceGrid;
                                        i = 5;
                                        break;
                                    case "CHF":
                                        price = CHFPriceGrid;
                                        i = 6;
                                        break;
                                }
                                foreach (FileInfo file2 in files)
                                {
                                    if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                    {
                                        file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                        file2.Delete();
                                        price.Rows[0].Cells[3].Style.BackColor = Color.Black;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "EUR":
                    if (FlatFlat[1] == true) return;
                    currency = Convert.ToString(EURSourceGrid.Rows[0].Cells[0].Value);
                    id = Convert.ToString(EURSourceGrid.Rows[0].Cells[1].Value);
                    name = Convert.ToString(EURSourceGrid.Rows[0].Cells[2].Value);
                    time = Convert.ToString(EURSourceGrid.Rows[0].Cells[4].Value);
                    date = Convert.ToString(EURSourceGrid.Rows[0].Cells[3].Value);
                    if (FlatFlat[0])
                    {
                        foreach (FileInfo file in files)
                        {
                            if (!file.Name.Contains("A0FX") || !file.Name.Contains(cp)) continue;
                            if (!file.Name.Contains(time) || !file.Name.Contains(date) || !file.Name.Contains(name)) continue;
                            char[] ch = file.Name.ToCharArray();
                            string fileName = file.Name;
                            if (MT4TargetMode)
                            {
                                DataGridView price = null;
                                int i = 0;
                                switch (cp)
                                {
                                    case "CAD":
                                        price = CADPriceGrid;
                                        i = 0;
                                        break;
                                    case "EUR":
                                        price = EURPriceGrid;
                                        i = 1;
                                        break;
                                    case "GBP":
                                        price = GBPPriceGrid;
                                        i = 2;
                                        break;
                                    case "AUD":
                                        price = AUDPriceGrid;
                                        i = 3;
                                        break;
                                    case "NZD":
                                        price = NZDPriceGrid;
                                        i = 4;
                                        break;
                                    case "JPY":
                                        price = JPYPriceGrid;
                                        i = 5;
                                        break;
                                    case "CHF":
                                        price = CHFPriceGrid;
                                        i = 6;
                                        break;
                                }
                                foreach (FileInfo file2 in files)
                                {
                                    if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                    {
                                        file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                        file2.Delete();
                                        price.Rows[0].Cells[3].Style.BackColor = Color.Black;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "GBP":
                    if (FlatFlat[2] == true) return;
                    currency = Convert.ToString(GBPSourceGrid.Rows[0].Cells[0].Value);
                    id = Convert.ToString(GBPSourceGrid.Rows[0].Cells[1].Value);
                    name = Convert.ToString(GBPSourceGrid.Rows[0].Cells[2].Value);
                    time = Convert.ToString(GBPSourceGrid.Rows[0].Cells[4].Value);
                    date = Convert.ToString(GBPSourceGrid.Rows[0].Cells[3].Value);
                    if (FlatFlat[2])
                    {
                        foreach (FileInfo file in files)
                        {
                            if (!file.Name.Contains("A0FX") || !file.Name.Contains(cp)) continue;
                            if (!file.Name.Contains(time) || !file.Name.Contains(date) || !file.Name.Contains(name)) continue;
                            char[] ch = file.Name.ToCharArray();
                            string fileName = file.Name;
                            if (MT4TargetMode)
                            {
                                DataGridView price = null;
                                int i = 0;
                                switch (cp)
                                {
                                    case "CAD":
                                        price = CADPriceGrid;
                                        i = 0;
                                        break;
                                    case "EUR":
                                        price = EURPriceGrid;
                                        i = 1;
                                        break;
                                    case "GBP":
                                        price = GBPPriceGrid;
                                        i = 2;
                                        break;
                                    case "AUD":
                                        price = AUDPriceGrid;
                                        i = 3;
                                        break;
                                    case "NZD":
                                        price = NZDPriceGrid;
                                        i = 4;
                                        break;
                                    case "JPY":
                                        price = JPYPriceGrid;
                                        i = 5;
                                        break;
                                    case "CHF":
                                        price = CHFPriceGrid;
                                        i = 6;
                                        break;
                                }
                                foreach (FileInfo file2 in files)
                                {
                                    if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                    {
                                        file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                        file2.Delete();
                                        price.Rows[0].Cells[3].Style.BackColor = Color.Black;
                                        Console.WriteLine(FlatFlat[2]);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "AUD":
                    if (FlatFlat[3] == true) return;
                    currency = Convert.ToString(AUDSourceGrid.Rows[0].Cells[0].Value);
                    id = Convert.ToString(AUDSourceGrid.Rows[0].Cells[1].Value);
                    name = Convert.ToString(AUDSourceGrid.Rows[0].Cells[2].Value);
                    time = Convert.ToString(AUDSourceGrid.Rows[0].Cells[4].Value);
                    date = Convert.ToString(AUDSourceGrid.Rows[0].Cells[3].Value);
                    if (FlatFlat[0])
                    {
                        foreach (FileInfo file in files)
                        {
                            if (!file.Name.Contains("A0FX") || !file.Name.Contains(cp)) continue;
                            if (!file.Name.Contains(time) || !file.Name.Contains(date) || !file.Name.Contains(name)) continue;
                            char[] ch = file.Name.ToCharArray();
                            string fileName = file.Name;
                            if (MT4TargetMode)
                            {
                                DataGridView price = null;
                                int i = 0;
                                switch (cp)
                                {
                                    case "CAD":
                                        price = CADPriceGrid;
                                        i = 0;
                                        break;
                                    case "EUR":
                                        price = EURPriceGrid;
                                        i = 1;
                                        break;
                                    case "GBP":
                                        price = GBPPriceGrid;
                                        i = 2;
                                        break;
                                    case "AUD":
                                        price = AUDPriceGrid;
                                        i = 3;
                                        break;
                                    case "NZD":
                                        price = NZDPriceGrid;
                                        i = 4;
                                        break;
                                    case "JPY":
                                        price = JPYPriceGrid;
                                        i = 5;
                                        break;
                                    case "CHF":
                                        price = CHFPriceGrid;
                                        i = 6;
                                        break;
                                }
                                foreach (FileInfo file2 in files)
                                {
                                    if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                    {
                                        file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                        file2.Delete();
                                        price.Rows[0].Cells[3].Style.BackColor = Color.Black;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "NZD":
                    if (FlatFlat[4] == true) return;
                    currency = Convert.ToString(NZDSourceGrid.Rows[0].Cells[0].Value);
                    id = Convert.ToString(NZDSourceGrid.Rows[0].Cells[1].Value);
                    name = Convert.ToString(NZDSourceGrid.Rows[0].Cells[2].Value);
                    time = Convert.ToString(NZDSourceGrid.Rows[0].Cells[4].Value);
                    date = Convert.ToString(NZDSourceGrid.Rows[0].Cells[3].Value);
                    if (FlatFlat[0])
                    {
                        foreach (FileInfo file in files)
                        {
                            if (!file.Name.Contains("A0FX") || !file.Name.Contains(cp)) continue;
                            if (!file.Name.Contains(time) || !file.Name.Contains(date) || !file.Name.Contains(name)) continue;
                            char[] ch = file.Name.ToCharArray();
                            string fileName = file.Name;
                            if (MT4TargetMode)
                            {
                                DataGridView price = null;
                                int i = 0;
                                switch (cp)
                                {
                                    case "CAD":
                                        price = CADPriceGrid;
                                        i = 0;
                                        break;
                                    case "EUR":
                                        price = EURPriceGrid;
                                        i = 1;
                                        break;
                                    case "GBP":
                                        price = GBPPriceGrid;
                                        i = 2;
                                        break;
                                    case "AUD":
                                        price = AUDPriceGrid;
                                        i = 3;
                                        break;
                                    case "NZD":
                                        price = NZDPriceGrid;
                                        i = 4;
                                        break;
                                    case "JPY":
                                        price = JPYPriceGrid;
                                        i = 5;
                                        break;
                                    case "CHF":
                                        price = CHFPriceGrid;
                                        i = 6;
                                        break;
                                }
                                foreach (FileInfo file2 in files)
                                {
                                    if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                    {
                                        file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                        file2.Delete();
                                        price.Rows[0].Cells[3].Style.BackColor = Color.Black;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "JPY":
                    if (FlatFlat[5] == true) return;
                    currency = Convert.ToString(JPYSourceGrid.Rows[0].Cells[0].Value);
                    id = Convert.ToString(JPYSourceGrid.Rows[0].Cells[1].Value);
                    name = Convert.ToString(JPYSourceGrid.Rows[0].Cells[2].Value);
                    time = Convert.ToString(JPYSourceGrid.Rows[0].Cells[4].Value);
                    date = Convert.ToString(JPYSourceGrid.Rows[0].Cells[3].Value);
                    if (FlatFlat[0])
                    {
                        foreach (FileInfo file in files)
                        {
                            if (!file.Name.Contains("A0FX") || !file.Name.Contains(cp)) continue;
                            if (!file.Name.Contains(time) || !file.Name.Contains(date) || !file.Name.Contains(name)) continue;
                            char[] ch = file.Name.ToCharArray();
                            string fileName = file.Name;
                            if (MT4TargetMode)
                            {
                                DataGridView price = null;
                                int i = 0;
                                switch (cp)
                                {
                                    case "CAD":
                                        price = CADPriceGrid;
                                        i = 0;
                                        break;
                                    case "EUR":
                                        price = EURPriceGrid;
                                        i = 1;
                                        break;
                                    case "GBP":
                                        price = GBPPriceGrid;
                                        i = 2;
                                        break;
                                    case "AUD":
                                        price = AUDPriceGrid;
                                        i = 3;
                                        break;
                                    case "NZD":
                                        price = NZDPriceGrid;
                                        i = 4;
                                        break;
                                    case "JPY":
                                        price = JPYPriceGrid;
                                        i = 5;
                                        break;
                                    case "CHF":
                                        price = CHFPriceGrid;
                                        i = 6;
                                        break;
                                }
                                foreach (FileInfo file2 in files)
                                {
                                    if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                    {
                                        file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                        file2.Delete();
                                        price.Rows[0].Cells[3].Style.BackColor = Color.Black;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "CHF":
                    if (FlatFlat[6] == true) return;
                    currency = Convert.ToString(CHFSourceGrid.Rows[0].Cells[0].Value);
                    id = Convert.ToString(CHFSourceGrid.Rows[0].Cells[1].Value);
                    name = Convert.ToString(CHFSourceGrid.Rows[0].Cells[2].Value);
                    time = Convert.ToString(CHFSourceGrid.Rows[0].Cells[4].Value);
                    date = Convert.ToString(CHFSourceGrid.Rows[0].Cells[3].Value);
                    if (FlatFlat[0])
                    {
                        foreach (FileInfo file in files)
                        {
                            if (!file.Name.Contains("A0FX") || !file.Name.Contains(cp)) continue;
                            if (!file.Name.Contains(time) || !file.Name.Contains(date) || !file.Name.Contains(name)) continue;
                            char[] ch = file.Name.ToCharArray();
                            string fileName = file.Name;
                            if (MT4TargetMode)
                            {
                                DataGridView price = null;
                                int i = 0;
                                switch (cp)
                                {
                                    case "CAD":
                                        price = CADPriceGrid;
                                        i = 0;
                                        break;
                                    case "EUR":
                                        price = EURPriceGrid;
                                        i = 1;
                                        break;
                                    case "GBP":
                                        price = GBPPriceGrid;
                                        i = 2;
                                        break;
                                    case "AUD":
                                        price = AUDPriceGrid;
                                        i = 3;
                                        break;
                                    case "NZD":
                                        price = NZDPriceGrid;
                                        i = 4;
                                        break;
                                    case "JPY":
                                        price = JPYPriceGrid;
                                        i = 5;
                                        break;
                                    case "CHF":
                                        price = CHFPriceGrid;
                                        i = 6;
                                        break;
                                }
                                foreach (FileInfo file2 in files)
                                {
                                    if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                    {
                                        file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                        file2.Delete();
                                        price.Rows[0].Cells[3].Style.BackColor = Color.Black;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }
        */

        private void FlatTimer_Tick(object sender, EventArgs e)
        {
            //FlatFlatThread.Start();
        }
    }
}
        /*
        private void Execute_Click(object sender, EventArgs e)
        {
            
            timer1.Interval = Convert.ToInt32(1000 * Convert.ToDouble(ProcessesPerSecond.Text));
            executing = true;
            if (dataToSort == "") return;
            Executer();
            Loader();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
            {
                row.Cells[4].Style.BackColor = Color.White;
            }
            dataToSort = "";
            foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
            {
                if (Convert.ToBoolean(row.Cells[3].Value) == true)
                {
                    dataToSort += Convert.ToString(row.Cells[0].Value);
                }
            }
            if (dataToSort == "") return;
            Loader();
        }

        private void Loader()
        {
            DataGridSource.AllowUserToAddRows = true;
            if (SourceBox.Text == "") return;
            string save = "";
            for (int i = 0; i < DataGridCurrencyPair.RowCount; i++)
            {
                for (int j = 0; j < DataGridCurrencyPair.ColumnCount; j++)
                {
                    save += DataGridCurrencyPair[j, i].Value + ",";
                }
                save += "\n";
            }
            save += '>';
            save += "\n";
            save += SourceBox.Text + "\n" + DestinationBox.Text + "\n" + MT4Box.Text + "\n" + eSigBox.Text + "\n";
            if (PriceGridButton.Checked == true) save += "P";
            else if (MT4Button.Checked == true) save += "M";
            else if (eSigButton.Checked == true) save += "E";
            if (File.Exists(@"C:\ProgramData\StrataGem571\TicketGen\Settings\Currency Pair.txt"))
            {
                File.Delete(@"C:\ProgramData\StrataGem571\TicketGen\Settings\Currency Pair.txt");
            }
            using (FileStream fs = File.Create(@"C:\ProgramData\StrataGem571\TicketGen\Settings\Currency Pair.txt"))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(save);
                fs.Write(info, 0, info.Length);
            }
            DataGridSource.Columns.Clear();
            DataGridProcessed.Columns.Clear();
            DataGridDestination.Columns.Clear();
            DataGridSource.Refresh();
            DataGridProcessed.Refresh();
            DataGridDestination.Refresh();
            DataGridSource.DataSource = null;
            DataGridProcessed.DataSource = null;
            DataGridDestination.DataSource = null;

            if (Directory.Exists(SourceBox.Text))
            {
                string currency = "";
                string name = "";
                string number = "";
                string date = "";
                string time = "";
                int data = 1;
                DirectoryInfo sf = new DirectoryInfo(SourceBox.Text);
                FileInfo[] files = sf.GetFiles();
                int fileNum = 0;
                DataGridSource.RowCount = 1000;
                DataGridSource.ColumnCount = 10;
                bool temp = false;
                foreach (FileInfo file in files)
                {
                    if (file.Name.Contains("A0FX"));
                    else continue;
                    char[] ch = file.Name.ToCharArray();
                    for (int i = 0; i < ch.Length; i++)
                    {
                        if (ch[i] == '_')
                        {
                            data += 1;
                        }
                        else
                        {
                            switch (data)
                            {
                                case 1:
                                    currency += ch[i];
                                    break;
                                case 2:
                                    name += ch[i];
                                    break;
                                case 4:
                                    number += ch[i];
                                    break;
                                case 5:
                                    date += ch[i];
                                    break;
                                case 6:
                                    if (ch[i] == '.')
                                    {
                                        temp = true;
                                        break;
                                    }
                                    if (temp == false)
                                    {
                                        time += ch[i];
                                    }
                                    break;
                            }
                        }
                    }
                    temp = false;
                    DataGridSource[0, fileNum].Value = currency;
                    DataGridSource[1, fileNum].Value = number;
                    DataGridSource[2, fileNum].Value = name;
                    DataGridSource[3, fileNum].Value = date;
                    DataGridSource[4, fileNum].Value = time;
                    fileNum += 1;
                    data = 1;
                    currency = "";
                    number = "";
                    name = "";
                    date = "";
                    time = "";
                }
                DataGridSource.RowCount = fileNum + 1;
            }
            else
            {
                MessageBox.Show("Source Directory is invalid", "Error");
            }
            if (Directory.Exists(SourceBox.Text + "\\Processed"))
            {
                string currency = "";
                string number = "";
                string name = "";
                string date = "";
                string time = "";
                int data = 1;
                DirectoryInfo sf = new DirectoryInfo(SourceBox.Text + "\\Processed");
                FileInfo[] files = sf.GetFiles();
                int fileNum = 0;
                DataGridProcessed.RowCount = 1000;
                DataGridProcessed.ColumnCount = 10;
                bool temp = false;
                foreach (FileInfo file in files)
                {
                    if (file.Name.Contains("A0FX")) ;
                    else continue;
                    char[] ch = file.Name.ToCharArray();
                    for (int i = 0; i < ch.Length; i++)
                    {
                        if (ch[i] == '_')
                        {
                            data += 1;
                        }
                        else
                        {
                            switch (data)
                            {
                                case 1:
                                    currency += ch[i];
                                    break;
                                case 2:
                                    name += ch[i];
                                    break;
                                case 4:
                                    number += ch[i];
                                    break;
                                case 5:
                                    date += ch[i];
                                    break;
                                case 6:
                                    if (ch[i] == '.')
                                    {
                                        temp = true;
                                        break;
                                    }
                                    if (temp == false)
                                    {
                                        time += ch[i];
                                    }
                                    break;
                            }
                        }
                    }
                    temp = false;
                    DataGridProcessed[0, fileNum].Value = currency;
                    DataGridProcessed[1, fileNum].Value = number;
                    DataGridProcessed[2, fileNum].Value = name;
                    DataGridProcessed[3, fileNum].Value = date;
                    DataGridProcessed[4, fileNum].Value = time;
                    fileNum += 1;
                    data = 1;
                    currency = "";
                    number = "";
                    name = "";
                    date = "";
                    time = "";
                }
                DataGridProcessed.RowCount = fileNum + 1;
            }
            else Directory.CreateDirectory(SourceBox.Text + "\\Processed");
            if (Directory.Exists(DestinationBox.Text))
            {
                string currency = "";
                string number = "";
                string name = "";
                string date = "";
                string time = "";
                int data = 1;
                DirectoryInfo sf = new DirectoryInfo(DestinationBox.Text);
                FileInfo[] files = sf.GetFiles();
                int fileNum = 0;
                DataGridDestination.RowCount = 1000;
                DataGridDestination.ColumnCount = 10;
                bool temp = false;
                foreach (FileInfo file in files)
                {
                    if (file.Name.Contains("A0FX")) ;
                    else continue;
                    char[] ch = file.Name.ToCharArray();
                    for (int i = 0; i < ch.Length; i++)
                    {
                        if (ch[i] == '_')
                        {
                            data += 1;
                        }
                        else
                        {
                            switch (data)
                            {
                                case 1:
                                    currency += ch[i];
                                    break;
                                case 2:
                                    name += ch[i];
                                    break;
                                case 4:
                                    number += ch[i];
                                    break;
                                case 5:
                                    date += ch[i];
                                    break;
                                case 6:
                                    if (ch[i] == '.')
                                    {
                                        temp = true;
                                        break;
                                    }
                                    if (temp == false)
                                    {
                                        time += ch[i];
                                    }
                                    break;
                            }
                        }
                    }
                    temp = false;
                    DataGridDestination[0, fileNum].Value = currency;
                    DataGridDestination[1, fileNum].Value = number;
                    DataGridDestination[2, fileNum].Value = name;
                    DataGridDestination[3, fileNum].Value = date;
                    DataGridDestination[4, fileNum].Value = time;
                    fileNum += 1;
                    data = 1;
                    currency = "";
                    number = "";
                    name = "";
                    date = "";
                    time = "";
                }
                DataGridDestination.RowCount = fileNum + 1;
            }
            else
            {
                if (DestinationBox.Text != "") Directory.CreateDirectory(DestinationBox.Text);
            }
            DataTable table = new DataTable();
            table.Columns.Add("Currency Pair");
            table.Columns.Add("Trade ID");
            table.Columns.Add("Order Type");
            table.Columns.Add("Ticket Date");
            table.Columns.Add("Ticket Time");
            int t = 0;
            foreach(DataGridViewRow row in DataGridSource.Rows)
            {
                table.Rows.Add(DataGridSource[0, t].Value, DataGridSource[1, t].Value, DataGridSource[2, t].Value, DataGridSource[3, t].Value, DataGridSource[4, t].Value);
                t++;
            }
            table.DefaultView.Sort = ("Ticket Date, Ticket Time, Trade ID");
            DataGridSource.Columns.Clear();
            DataGridSource.DataSource = table;
            DataGridSource.Rows.RemoveAt(0);

            table = new DataTable();
            table.Columns.Add("Currency Pair");
            table.Columns.Add("Trade ID");
            table.Columns.Add("Order Type");
            table.Columns.Add("Ticket Date");
            table.Columns.Add("Ticket Time");
            t = 0;
            foreach (DataGridViewRow row in DataGridProcessed.Rows)
            {
                table.Rows.Add(DataGridProcessed[0, t].Value, DataGridProcessed[1, t].Value, DataGridProcessed[2, t].Value, DataGridProcessed[3, t].Value, DataGridProcessed[4, t].Value);
                t++;
            }
            table.DefaultView.Sort = ("Ticket Date, Ticket Time, Trade ID");
            DataGridProcessed.Columns.Clear();
            DataGridProcessed.DataSource = table;
            if(!DataGridProcessed.Rows[0].IsNewRow) DataGridProcessed.Rows.RemoveAt(0);

            table = new DataTable();
            table.Columns.Add("Currency Pair");
            table.Columns.Add("Trade ID");
            table.Columns.Add("Order Type");
            table.Columns.Add("Ticket Date");
            table.Columns.Add("Ticket Time");
            t = 0;
            foreach (DataGridViewRow row in DataGridDestination.Rows)
            {
                table.Rows.Add(DataGridDestination[0, t].Value, DataGridDestination[1, t].Value, DataGridDestination[2, t].Value, DataGridDestination[3, t].Value, DataGridDestination[4, t].Value);
                t++;
            }
            table.DefaultView.Sort = ("Ticket Date, Ticket Time, Trade ID");
            DataGridDestination.Columns.Clear();
            DataGridDestination.DataSource = table;
            if(!DataGridDestination.Rows[0].IsNewRow) DataGridDestination.Rows.RemoveAt(0);

            executing = false;
            
            int l = 0;
            DataGridViewRow[] rowsForRemoval = new DataGridViewRow[1000];
            foreach (DataGridViewRow row in DataGridSource.Rows)
            {
                if (!dataToSort.Contains(Convert.ToString(DataGridSource[0, l].Value)) && row.IsNewRow == false)
                {
                    rowsForRemoval[l] = row;
                }
                l++;
            }
            foreach (DataGridViewRow row in rowsForRemoval)
            {
                if (row == null) continue;
                DataGridSource.Rows.Remove(row);
            }
            l = 0;
            rowsForRemoval = new DataGridViewRow[1000];
            foreach (DataGridViewRow row in DataGridProcessed.Rows)
            {
                if (!dataToSort.Contains(Convert.ToString(DataGridProcessed[0, l].Value)) && row.IsNewRow == false)
                {
                    rowsForRemoval[l] = row;
                }
                l++;
            }
            foreach (DataGridViewRow row in rowsForRemoval)
            {
                if (row == null) continue;
                DataGridProcessed.Rows.Remove(row);
            }
            l = 0;
            rowsForRemoval = new DataGridViewRow[1000];
            foreach (DataGridViewRow row in DataGridDestination.Rows)
            {
                if (!dataToSort.Contains(Convert.ToString(DataGridDestination[0, l].Value)) && row.IsNewRow == false)
                {
                    rowsForRemoval[l] = row;
                }
                l++;
            }
            foreach (DataGridViewRow row in rowsForRemoval)
            {
                if (row == null) continue;
                DataGridDestination.Rows.Remove(row);
            }
            foreach (DataGridViewRow row in DataGridDestination.Rows)
            {
                row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
            }
            foreach (DataGridViewRow row in DataGridProcessed.Rows)
            {
                row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
            }
            foreach (DataGridViewRow row in DataGridSource.Rows)
            {
                row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
            }
            DataGridSource.Refresh();
            DataGridProcessed.Refresh();
            DataGridDestination.Refresh();
        }
        
        public void Executer()
        {
            if (MT4Button.Checked)
            {
                if(MT4Box.Text == "")
                {
                    MessageBox.Show("MT4 location cannot be empty", "Error");
                    return;
                }
                string[] fr = null;
                try
                {
                    fr = File.ReadAllLines(MT4Box.Text);
                }
                catch (System.IO.IOException)
                {
                    int l = 0;
                    while (l == 0)
                    {
                        try
                        {
                            fr = File.ReadAllLines(MT4Box.Text);
                            l = 1;
                        }
                        catch (System.IO.IOException)
                        {
                            l = 0;
                        }
                        if (l != 0)
                        {
                            l = 1;
                        }
                    }
                }
                foreach(string s in fr)
                {
                    if (s.Contains("Time")) continue;
                    string[] split = s.Split(',');
                    string temp = "";
                    string[] temp1 = split[2].Split('.');
                    char[] c = temp1[1].ToCharArray();
                    string final;
                    if(split[4] == "FLAT" && split[5] == "FLAT")
                    {
                        //CAD
                        //EUR
                        //GBP
                        //AUD
                        //NZD
                        //JPY
                        //CHF
                        if (split[1].Contains("CAD"))
                        {
                            FlatFlat[0] = true;
                        }
                        if (split[1].Contains("EUR"))
                        {
                            FlatFlat[1] = true;
                        }
                        if (split[1].Contains("GBP"))
                        {
                            FlatFlat[2] = true;
                        }
                        if (split[1].Contains("AUD"))
                        {
                            FlatFlat[3] = true;
                        }
                        if (split[1].Contains("NZD"))
                        {
                            FlatFlat[4] = true;
                        }
                        if (split[1].Contains("JPY"))
                        {
                            FlatFlat[5] = true;
                        }
                        if (split[1].Contains("CHF"))
                        {
                            FlatFlat[6] = true;
                        }
                    }
                    if (split[4] != "FLAT" || split[5] != "FLAT")
                    {
                        //CAD
                        //EUR
                        //GBP
                        //AUD
                        //NZD
                        //JPY
                        //CHF
                        if (split[1].Contains("CAD"))
                        {
                            FlatFlat[0] = false;
                        }
                        if (split[1].Contains("EUR"))
                        {
                            FlatFlat[1] = false;
                        }
                        if (split[1].Contains("GBP"))
                        {
                            FlatFlat[2] = false;
                        }
                        if (split[1].Contains("AUD"))
                        {
                            FlatFlat[3] = false;
                        }
                        if (split[1].Contains("NZD"))
                        {
                            FlatFlat[4] = false;
                        }
                        if (split[1].Contains("JPY"))
                        {
                            FlatFlat[5] = false;
                        }
                        if (split[1].Contains("CHF"))
                        {
                            FlatFlat[6] = false;
                        }
                    }
                    for (int l = 0; l < 5; l++)
                    {
                        if (c.Length >= 5) temp += c[l];
                        else
                        {
                            if (l < c.Length) temp += c[l];
                            else temp += "0";
                        }
                    }
                    temp1[1] = temp;
                    final = String.Join(".", temp1);
                    split[2] = final;
                    foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                    {
                        if (Convert.ToString(row.Cells[0].Value).Contains("CAD") && split[1].Contains("CAD")) row.Cells[1].Value = split[2];
                        if (Convert.ToString(row.Cells[0].Value).Contains("EUR") && split[1].Contains("EUR")) row.Cells[1].Value = split[2];
                        if (Convert.ToString(row.Cells[0].Value).Contains("GBP") && split[1].Contains("GBP")) row.Cells[1].Value = split[2];
                        if (Convert.ToString(row.Cells[0].Value).Contains("AUD") && split[1].Contains("AUD")) row.Cells[1].Value = split[2];
                        if (Convert.ToString(row.Cells[0].Value).Contains("NZD") && split[1].Contains("NZD")) row.Cells[1].Value = split[2];
                        if (Convert.ToString(row.Cells[0].Value).Contains("JPY") && split[1].Contains("JPY")) row.Cells[1].Value = split[2];
                        if (Convert.ToString(row.Cells[0].Value).Contains("CHF") && split[1].Contains("CHF")) row.Cells[1].Value = split[2];
                    }
                }
            }
            else if (eSigButton.Checked)
            {
                if (eSigBox.Text == "")
                {
                    MessageBox.Show("eSig location cannot be empty", "Error");
                    return;
                }
                FileInfo[] files = new DirectoryInfo(eSigBox.Text).GetFiles();

                foreach (FileInfo f in files)
                {
                    if (!f.Name.Contains("A0-FX")) continue;
                    string s = f.Name;
                    string[] fr = null;
                    try
                    {
                        fr = File.ReadAllLines(f.FullName);
                    }
                    catch (System.IO.IOException)
                    {
                        int l = 0;
                        while (l == 0)
                        {
                            try
                            {
                                fr = File.ReadAllLines(f.FullName);
                                l = 1;
                            }
                            catch (System.IO.IOException)
                            {
                                l = 0;
                            }
                            if (l != 0)
                            {
                                l = 1;
                            }
                        }
                    }
                    string[] split = fr[0].Split(',');
                    string temp = "";
                    string[] temp1 = split[2].Split('.');
                    char[] c = temp1[1].ToCharArray();
                    string final;
                    for (int l = 0; l < 5; l++)
                    {
                        if (c.Length >= 5) temp += c[l];
                        else
                        {
                            if (l < c.Length) temp += c[l];
                            else temp += "0";
                        }
                    }
                    temp1[1] = temp;
                    final = String.Join(".", temp1);
                    split[2] = final;
                    foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                    {
                        if (Convert.ToString(row.Cells[0].Value).Contains("CAD") && f.Name.Contains("CAD")) row.Cells[1].Value = split[5];
                        if (Convert.ToString(row.Cells[0].Value).Contains("EUR") && f.Name.Contains("EUR")) row.Cells[1].Value = split[5];
                        if (Convert.ToString(row.Cells[0].Value).Contains("GBP") && f.Name.Contains("GBP")) row.Cells[1].Value = split[5];
                        if (Convert.ToString(row.Cells[0].Value).Contains("AUD") && f.Name.Contains("AUD")) row.Cells[1].Value = split[5];
                        if (Convert.ToString(row.Cells[0].Value).Contains("NZD") && f.Name.Contains("NZD")) row.Cells[1].Value = split[5];
                        if (Convert.ToString(row.Cells[0].Value).Contains("JPY") && f.Name.Contains("JPY")) row.Cells[1].Value = split[5];
                        if (Convert.ToString(row.Cells[0].Value).Contains("CHF") && f.Name.Contains("CHF")) row.Cells[1].Value = split[5];
                    }
                }
            }
            else if (PriceGridButton.Checked);
            executing = true;
            if(BulkButton.Checked == true)
            {
                DirectoryInfo sf = new DirectoryInfo(SourceBox.Text);
                FileInfo[] files = sf.GetFiles();
                string currency = "";
                string name = "";
                string date = "";
                string time = "";
                int data = 1;
                bool temp = false;

                foreach (FileInfo file in files)
                {
                    if (file.Name.Contains("A0FX"));
                    else continue;
                    char[] ch = file.Name.ToCharArray();
                    string fileName = file.Name;
                    string[] newName;
                    for (int i = 0; i < ch.Length; i++)
                    {
                        if (ch[i] == '.')
                        {
                            temp = true;
                        }
                        if (ch[i] == '_')
                        {
                            data += 1;
                        }
                        else
                        {
                            switch (data)
                            {
                                case 1:
                                    currency += ch[i];
                                    break;
                                case 2:
                                    name += ch[i];
                                    break;
                                case 5:
                                    date += ch[i];
                                    break;
                                case 6:
                                    if (temp == false)
                                    {
                                        time += ch[i];
                                    }
                                    break;
                            }
                        }
                    }
                    newName = fileName.Split('.');
                    newName[1] = ".csv";
                    file.CopyTo(SourceBox.Text + "\\Processed\\" + file.Name, true);
                    file.CopyTo(DestinationBox.Text + "\\" + newName[0] + newName[1], true);
                    if (name == "OpenBuy" || name == "OpenSell" || name == "ReverseBuy")
                    {
                        string[] fileOverwrite = System.IO.File.ReadAllText(DestinationBox.Text + "\\" + newName[0] + newName[1]).Split(',');
                        double currencyNum = 0;
                        double limitNum = 0;
                        foreach(DataGridViewRow row in DataGridCurrencyPair.Rows)
                        {
                            if(Convert.ToString(row.Cells[0].Value) == currency)
                            {
                                currencyNum = (Convert.ToDouble(row.Cells[1].Value));
                                limitNum = (Convert.ToDouble(row.Cells[2].Value));
                            }
                        }
                        fileOverwrite[9] = currencyNum.ToString();
                        fileOverwrite[10] = currencyNum.ToString();
                        fileOverwrite[11] = (currencyNum - limitNum).ToString();
                        fileOverwrite[12] = (currencyNum + limitNum).ToString();
                        using (FileStream fs = File.Create(DestinationBox.Text + "\\" + newName[0] + newName[1]))
                        {
                            Byte[] info = new UTF8Encoding(true).GetBytes(String.Join(",", fileOverwrite));
                            fs.Write(info, 0, info.Length);
                        }
                    }
                    else if (name == "partialCloseBuy")
                    {
                        string[] fileOverwrite = System.IO.File.ReadAllText(DestinationBox.Text + "\\" + newName[0] + newName[1]).Split(',');
                        double currencyNum = 0;
                        double limitNum = 0;
                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                        {
                            if (Convert.ToString(row.Cells[0].Value) == currency)
                            {
                                currencyNum = (Convert.ToDouble(row.Cells[1].Value));
                                limitNum = (Convert.ToDouble(row.Cells[2].Value));
                            }
                        }
                        fileOverwrite[9] = currencyNum.ToString();
                        fileOverwrite[15] = (currencyNum - limitNum).ToString();
                        using (FileStream fs = File.Create(DestinationBox.Text + "\\" + newName[0] + newName[1]))
                        {
                            Byte[] info = new UTF8Encoding(true).GetBytes(String.Join(",", fileOverwrite));
                            fs.Write(info, 0, info.Length);
                        }
                    }
                    else if (name == "partialCloseSell")
                    {
                        string[] fileOverwrite = System.IO.File.ReadAllText(DestinationBox.Text + "\\" + newName[0] + newName[1]).Split(',');
                        double currencyNum = 0;
                        double limitNum = 0;
                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                        {
                            if (Convert.ToString(row.Cells[0].Value) == currency)
                            {
                                currencyNum = (Convert.ToDouble(row.Cells[1].Value));
                                limitNum = (Convert.ToDouble(row.Cells[2].Value));
                            }
                        }
                        fileOverwrite[9] = currencyNum.ToString();
                        fileOverwrite[15] = (currencyNum + limitNum).ToString();
                        using (FileStream fs = File.Create(DestinationBox.Text + "\\" + newName[0] + newName[1]))
                        {
                            Byte[] info = new UTF8Encoding(true).GetBytes(String.Join(",", fileOverwrite));
                            fs.Write(info, 0, info.Length);
                        }
                    }
                    file.Delete();
                    temp = false;
                    data = 1;
                    currency = "";
                    name = "";
                    date = "";
                    time = "";
                }
            }
            else if(ManualButton.Checked || AutomaticButton.Checked)
            {
                DirectoryInfo sf = new DirectoryInfo(SourceBox.Text);
                files = sf.GetFiles();
                currency = "";
                id = "";
                string name = "";
                string date = "";
                string time = "";
                int data = 1;
                bool temp = false;

                foreach (FileInfo file in files)
                {
                    if (!file.Name.Contains("A0FX")) continue;
                    char[] ch = file.Name.ToCharArray();
                    string fileName = file.Name;
                    string[] newName;
                    bool stop = true;
                    for (int i = 0; i < ch.Length; i++)
                    {
                        if (ch[i] == '.')
                        {
                            temp = true;
                        }
                        if (ch[i] == '_')
                        {
                            data += 1;
                        }
                        else
                        {
                            switch (data)
                            {
                                case 1:
                                    currency += ch[i];
                                    break;
                                case 2:
                                    name += ch[i];
                                    break;
                                case 4:
                                    if(ch[i] == '0' || ch[i] == '1' || ch[i] == '2' || ch[i] == '3' || ch[i] == '4' || ch[i] == '5' || ch[i] == '6' || ch[i] == '7' || ch[i] == '8' || ch[i] == '9') id += ch[i];
                                    break;
                                case 5:
                                    date += ch[i];
                                    break;
                                case 6:
                                    if (temp == false)
                                    {
                                        time += ch[i];
                                    }
                                    break;
                            }
                        }
                    }
                    if (!fileName.Contains(Convert.ToString(DataGridSource[0, 0].Value)) || !fileName.Contains(Convert.ToString(DataGridSource[1, 0].Value)) || !fileName.Contains(Convert.ToString(DataGridSource[2, 0].Value)) || !fileName.Contains(Convert.ToString(DataGridSource[3, 0].Value)) || !fileName.Contains(Convert.ToString(DataGridSource[4, 0].Value)))
                    {
                        currency = "";
                        name = "";
                        date = "";
                        id = "";
                        time = "";
                        data = 1;
                        temp = false;
                        continue;
                    }
                    else stop = false;
                    newName = fileName.Split('.');
                    newName[1] = ".csv";
                    if (dataToSort.Contains(currency))
                    {
                        if (MT4TargetMode)
                        {
                            if (name == "OpenBuy" || name == "ReverseBuy")
                            {
                                foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                {
                                    if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Green;
                                }
                            }
                            else if (name == "OpenSell" || name == "ReverseSell")
                            {
                                foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                {
                                    if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Red;
                                }
                            }
                            else if (name == "CloseBuy" || name == "CloseSell")
                            {
                                //CAD
                                //EUR
                                //GBP
                                //AUD
                                //NZD
                                //JPY
                                //CHF
                                if (currency.Contains("CAD"))
                                {
                                    if (FlatFlat[0])
                                    {
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                    else
                                    {
                                        bool kill = false;
                                        timer1.Enabled = false;
                                        do
                                        {
                                            kill = Loop(0);
                                        } while (!kill);
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                                timer1.Enabled = true;
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                }
                                else if (currency.Contains("EUR"))
                                {
                                    if (FlatFlat[1])
                                    {
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                    else
                                    {
                                        bool kill = false;
                                        timer1.Enabled = false;
                                        do
                                        {
                                            kill = Loop(1);
                                        } while (!kill);
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                                timer1.Enabled = true;
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                }
                                else if (currency.Contains("GBP"))
                                {
                                    if (FlatFlat[2])
                                    {
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                    else
                                    {
                                        bool kill = false;
                                        timer1.Enabled = false;
                                        do
                                        {
                                            kill = Loop(2);
                                        } while (!kill);
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                                timer1.Enabled = true;
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                }
                                else if (currency.Contains("AUD"))
                                {
                                    if (FlatFlat[3])
                                    {
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                    else
                                    {
                                        bool kill = false;
                                        timer1.Enabled = false;
                                        do
                                        {
                                            kill = Loop(3);
                                        } while (!kill);
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                                timer1.Enabled = true;
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                }
                                else if (currency.Contains("NZD"))
                                {
                                    if (FlatFlat[4])
                                    {
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                    else
                                    {
                                        bool kill = false;
                                        timer1.Enabled = false;
                                        do
                                        {
                                            kill = Loop(4);
                                        } while (!kill);
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                                timer1.Enabled = true;
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                }
                                else if (currency.Contains("JPY"))
                                {
                                    if (FlatFlat[5])
                                    {
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                    else
                                    {
                                        bool kill = false;
                                        timer1.Enabled = false;
                                        do
                                        {
                                            kill = Loop(5);
                                        } while (!kill);
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                                timer1.Enabled = true;
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                }
                                else if (currency.Contains("CHF"))
                                {
                                    if (FlatFlat[6])
                                    {
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                    else
                                    {
                                        bool kill = false;
                                        timer1.Enabled = false;
                                        do
                                        {
                                            kill = Loop(6);
                                        } while (!kill);
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                                timer1.Enabled = true;
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                    }
                                }
                                break;
                            }
                            else
                            {
                                //CAD
                                //EUR
                                //GBP
                                //AUD
                                //NZD
                                //JPY
                                //CHF
                                if (currency.Contains("CAD"))
                                {
                                    if (FlatFlat[0])
                                    {
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                        break;
                                    }
                                }
                                if (currency.Contains("EUR"))
                                {
                                    if (FlatFlat[1])
                                    {
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                        break;
                                    }
                                }
                                if (currency.Contains("GBP"))
                                {
                                    if (FlatFlat[2])
                                    {
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                        break;
                                    }
                                }
                                if (currency.Contains("AUD"))
                                {
                                    if (FlatFlat[3])
                                    {
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                        break;
                                    }
                                }
                                if (currency.Contains("NZD"))
                                {
                                    if (FlatFlat[4])
                                    {
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                        break;
                                    }
                                }
                                if (currency.Contains("JPY"))
                                {
                                    if (FlatFlat[5])
                                    {
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                        break;
                                    }
                                }
                                if (currency.Contains("CHF"))
                                {
                                    if (FlatFlat[6])
                                    {
                                        foreach (FileInfo file2 in files)
                                        {
                                            if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                                            {
                                                file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                                                file2.Delete();
                                            }
                                        }
                                        foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                                        {
                                            if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        file.CopyTo(SourceBox.Text + "\\Processed\\" + file.Name, true);
                        file.CopyTo(DestinationBox.Text + "\\" + newName[0] + newName[1], true);
                        if (name == "OpenBuy" || name == "OpenSell" || name == "ReverseBuy")
                        {
                            string[] fileOverwrite = System.IO.File.ReadAllText(DestinationBox.Text + "\\" + newName[0] + newName[1]).Split(',');
                            double currencyNum = 0;
                            double limitNum = 0;
                            foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                            {
                                if (Convert.ToString(row.Cells[0].Value) == currency)
                                {
                                    currencyNum = (Convert.ToDouble(row.Cells[1].Value));
                                    limitNum = (Convert.ToDouble(row.Cells[2].Value));
                                }
                            }
                            fileOverwrite[9] = currencyNum.ToString();
                            fileOverwrite[10] = currencyNum.ToString();
                            fileOverwrite[11] = (currencyNum - limitNum).ToString();
                            fileOverwrite[12] = (currencyNum + limitNum).ToString();
                            using (FileStream fs = File.Create(DestinationBox.Text + "\\" + newName[0] + newName[1]))
                            {
                                Byte[] info = new UTF8Encoding(true).GetBytes(String.Join(",", fileOverwrite));
                                fs.Write(info, 0, info.Length);
                            }
                        }
                        else if (name == "partialCloseBuy")
                        {
                            string[] fileOverwrite = System.IO.File.ReadAllText(DestinationBox.Text + "\\" + newName[0] + newName[1]).Split(',');
                            double currencyNum = 0;
                            double limitNum = 0;
                            foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                            {
                                if (Convert.ToString(row.Cells[0].Value) == currency)
                                {
                                    currencyNum = (Convert.ToDouble(row.Cells[1].Value));
                                    limitNum = (Convert.ToDouble(row.Cells[2].Value));
                                }
                            }
                            fileOverwrite[9] = currencyNum.ToString();
                            fileOverwrite[15] = (currencyNum - limitNum).ToString();
                            using (FileStream fs = File.Create(DestinationBox.Text + "\\" + newName[0] + newName[1]))
                            {
                                Byte[] info = new UTF8Encoding(true).GetBytes(String.Join(",", fileOverwrite));
                                fs.Write(info, 0, info.Length);
                            }
                        }
                        else if (name == "partialCloseSell")
                        {
                            string[] fileOverwrite = System.IO.File.ReadAllText(DestinationBox.Text + "\\" + newName[0] + newName[1]).Split(',');
                            double currencyNum = 0;
                            double limitNum = 0;
                            foreach(DataGridViewRow row in DataGridCurrencyPair.Rows)
                            {
                                if(Convert.ToString(row.Cells[0].Value) == currency)
                                {
                                    currencyNum = (Convert.ToDouble(row.Cells[1].Value));
                                    limitNum = (Convert.ToDouble(row.Cells[2].Value));
                                }
                            }
                            fileOverwrite[9] = currencyNum.ToString();
                            fileOverwrite[15] = (currencyNum + limitNum).ToString();
                            using (FileStream fs = File.Create(DestinationBox.Text + "\\" + newName[0] + newName[1]))
                            {
                                Byte[] info = new UTF8Encoding(true).GetBytes(String.Join(",", fileOverwrite));
                                fs.Write(info, 0, info.Length);
                            }
                        }
                        file.Delete();
                    }
                    temp = false;
                    data = 1;
                    currency = "";
                    name = "";
                    id = "";
                    date = "";
                    time = "";
                }
            }
            if(AutomaticButton.Checked)
            {
                timer1.Interval = Convert.ToInt32(1000 * Convert.ToDouble(ProcessesPerSecond.Text));
                timer1.Enabled = true;
            }
            running = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Executer();
            Loader();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            timer1.Interval = Convert.ToInt32(1000 * Convert.ToDouble(ProcessesPerSecond.Text));
            timer1.Enabled = false;
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            timer1.Interval = Convert.ToInt32(1000 * Convert.ToDouble(ProcessesPerSecond.Text));
            timer1.Enabled = false;
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            running = true;
            timer1.Interval = Convert.ToInt32(1000 * Convert.ToDouble(ProcessesPerSecond.Text));
            timer1.Enabled = true;
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
            {
                row.Cells[4].Style.BackColor = Color.White;
            }
            FileInfo[] files = new DirectoryInfo(DestinationBox.Text).GetFiles();
            FileInfo[] files2 = new DirectoryInfo(SourceBox.Text + "\\Processed").GetFiles();

            foreach (FileInfo f in files)
            {
                if (f.Name.Contains("A0FX")) f.Delete();
                else continue;
            }
            foreach (FileInfo f in files2)
            {
                if (f.Name.Contains("A0FX")) ;
                else continue;
                f.CopyTo(SourceBox.Text + "\\" + f.Name, true);
                f.Delete();
            }
            Loader();
        }
        
        private bool Loop(int line)
        {
            string[] fr = null;
            try
            {
                fr = File.ReadAllLines(MT4Box.Text);
            }
            catch (System.IO.IOException)
            {
                int l = 0;
                while (l == 0)
                {
                    try
                    {
                        fr = File.ReadAllLines(MT4Box.Text);
                        l = 1;
                    }
                    catch (System.IO.IOException)
                    {
                        l = 0;
                    }
                    if (l != 0)
                    {
                        l = 1;
                    }
                }
            }
            foreach (string s in fr)
            {
                if (s.Contains("Time")) continue;
                string[] split = s.Split(',');
                string temp = "";
                string[] temp1 = split[2].Split('.');
                char[] c = temp1[1].ToCharArray();
                string final;
                if (split[4] == "FLAT" && split[5] == "FLAT")
                {
                    //CAD
                    //EUR
                    //GBP
                    //AUD
                    //NZD
                    //JPY
                    //CHF
                    if (split[1].Contains("CAD"))
                    {
                        FlatFlat[0] = true;
                    }
                    if (split[1].Contains("EUR"))
                    {
                        FlatFlat[1] = true;
                    }
                    if (split[1].Contains("GBP"))
                    {
                        FlatFlat[2] = true;
                    }
                    if (split[1].Contains("AUD"))
                    {
                        FlatFlat[3] = true;
                    }
                    if (split[1].Contains("NZD"))
                    {
                        FlatFlat[4] = true;
                    }
                    if (split[1].Contains("JPY"))
                    {
                        FlatFlat[5] = true;
                    }
                    if (split[1].Contains("CHF"))
                    {
                        FlatFlat[6] = true;
                    }
                }
                for (int l = 0; l < 5; l++)
                {
                    if (c.Length >= 5) temp += c[l];
                    else
                    {
                        if (l < c.Length) temp += c[l];
                        else temp += "0";
                    }
                }
                temp1[1] = temp;
                final = String.Join(".", temp1);
                split[2] = final;
                foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                {
                    if (Convert.ToString(row.Cells[0].Value).Contains("CAD") && split[1].Contains("CAD")) row.Cells[1].Value = split[2];
                    if (Convert.ToString(row.Cells[0].Value).Contains("EUR") && split[1].Contains("EUR")) row.Cells[1].Value = split[2];
                    if (Convert.ToString(row.Cells[0].Value).Contains("GBP") && split[1].Contains("GBP")) row.Cells[1].Value = split[2];
                    if (Convert.ToString(row.Cells[0].Value).Contains("AUD") && split[1].Contains("AUD")) row.Cells[1].Value = split[2];
                    if (Convert.ToString(row.Cells[0].Value).Contains("NZD") && split[1].Contains("NZD")) row.Cells[1].Value = split[2];
                    if (Convert.ToString(row.Cells[0].Value).Contains("JPY") && split[1].Contains("JPY")) row.Cells[1].Value = split[2];
                    if (Convert.ToString(row.Cells[0].Value).Contains("CHF") && split[1].Contains("CHF")) row.Cells[1].Value = split[2];
                }
            }
            return (FlatFlat[line]);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            bool[] t = FlatFlat;
            
            string[] fr = null;
            try
            {
                fr = File.ReadAllLines(MT4Box.Text);
            }
            catch (System.IO.IOException)
            {
                int l = 0;
                while (l == 0)
                {
                    try
                    {
                        fr = File.ReadAllLines(MT4Box.Text);
                        l = 1;
                    }
                    catch (System.IO.IOException)
                    {
                        l = 0;
                    }
                    if (l != 0)
                    {
                        l = 1;
                    }
                }
            }
            foreach (string s in fr)
            {
                if (s.Contains("Time")) continue;
                string[] split = s.Split(',');
                string temp = "";
                string[] temp1 = split[2].Split('.');
                char[] c = temp1[1].ToCharArray();
                string final;
                if (split[4] == "FLAT" && split[5] == "FLAT")
                {
                    //CAD
                    //EUR
                    //GBP
                    //AUD
                    //NZD
                    //JPY
                    //CHF
                    if (split[1].Contains("CAD"))
                    {
                        FlatFlat[0] = true;
                    }
                    if (split[1].Contains("EUR"))
                    {
                        FlatFlat[1] = true;
                    }
                    if (split[1].Contains("GBP"))
                    {
                        FlatFlat[2] = true;
                    }
                    if (split[1].Contains("AUD"))
                    {
                        FlatFlat[3] = true;
                    }
                    if (split[1].Contains("NZD"))
                    {
                        FlatFlat[4] = true;
                    }
                    if (split[1].Contains("JPY"))
                    {
                        FlatFlat[5] = true;
                    }
                    if (split[1].Contains("CHF"))
                    {
                        FlatFlat[6] = true;
                    }
                }
                for (int l = 0; l < 5; l++)
                {
                    if (c.Length >= 5) temp += c[l];
                    else
                    {
                        if (l < c.Length) temp += c[l];
                        else temp += "0";
                    }
                }
                temp1[1] = temp;
                final = String.Join(".", temp1);
                split[2] = final;
                foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                {
                    if (Convert.ToString(row.Cells[0].Value).Contains("CAD") && split[1].Contains("CAD")) row.Cells[1].Value = split[2];
                    if (Convert.ToString(row.Cells[0].Value).Contains("EUR") && split[1].Contains("EUR")) row.Cells[1].Value = split[2];
                    if (Convert.ToString(row.Cells[0].Value).Contains("GBP") && split[1].Contains("GBP")) row.Cells[1].Value = split[2];
                    if (Convert.ToString(row.Cells[0].Value).Contains("AUD") && split[1].Contains("AUD")) row.Cells[1].Value = split[2];
                    if (Convert.ToString(row.Cells[0].Value).Contains("NZD") && split[1].Contains("NZD")) row.Cells[1].Value = split[2];
                    if (Convert.ToString(row.Cells[0].Value).Contains("JPY") && split[1].Contains("JPY")) row.Cells[1].Value = split[2];
                    if (Convert.ToString(row.Cells[0].Value).Contains("CHF") && split[1].Contains("CHF")) row.Cells[1].Value = split[2];
                }
                int i = 0;
                foreach(bool f in FlatFlat)
                {
                    if(f != t[i])
                    {
                        FlatFlatProcessor(currency, id, files);
                    }
                    i++;
                }
            }
        }
        private void FlatFlatProcessor(String currency, string id, FileInfo[] files)
        {
            //CAD
            //EUR
            //GBP
            //AUD
            //NZD
            //JPY
            //CHF
            if (currency.Contains("CAD"))
            {
                if (FlatFlat[0])
                {
                    foreach (FileInfo file2 in files)
                    {
                        if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                        {
                            file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                            file2.Delete();
                        }
                    }
                    foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                    {
                        if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                    }
                }
            }
            if (currency.Contains("EUR"))
            {
                if (FlatFlat[1])
                {
                    foreach (FileInfo file2 in files)
                    {
                        if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                        {
                            file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                            file2.Delete();
                        }
                    }
                    foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                    {
                        if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                    }
                }
            }
            if (currency.Contains("GBP"))
            {
                if (FlatFlat[2])
                {
                    foreach (FileInfo file2 in files)
                    {
                        if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                        {
                            file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                            file2.Delete();
                        }
                    }
                    foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                    {
                        if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                    }
                }
            }
            if (currency.Contains("AUD"))
            {
                if (FlatFlat[3])
                {
                    foreach (FileInfo file2 in files)
                    {
                        if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                        {
                            file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                            file2.Delete();
                        }
                    }
                    foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                    {
                        if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                    }
                }
            }
            if (currency.Contains("NZD"))
            {
                if (FlatFlat[4])
                {
                    foreach (FileInfo file2 in files)
                    {
                        if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                        {
                            file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                            file2.Delete();
                        }
                    }
                    foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                    {
                        if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                    }
                }
            }
            if (currency.Contains("JPY"))
            {
                if (FlatFlat[5])
                {
                    foreach (FileInfo file2 in files)
                    {
                        if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                        {
                            file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                            file2.Delete();
                        }
                    }
                    foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                    {
                        if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                    }
                }
            }
            if (currency.Contains("CHF"))
            {
                if (FlatFlat[6])
                {
                    foreach (FileInfo file2 in files)
                    {
                        if (file2.Name.Contains(currency) && file2.Name.Contains(id))
                        {
                            file2.CopyTo(SourceBox.Text + "\\Processed\\" + file2.Name, true);
                            file2.Delete();
                        }
                    }
                    foreach (DataGridViewRow row in DataGridCurrencyPair.Rows)
                    {
                        if (currency == Convert.ToString(row.Cells[0].Value)) row.Cells[4].Style.BackColor = Color.Black;
                    }
                }
            }
        }
    }
}
*/