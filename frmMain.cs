/*
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" by Poul-Henning Kamp (Revision 42):
 * <MaDOS> wrote this file.  As long as you retain this notice you
 * can do whatever you want with this stuff. If we meet some day, and you think
 * this stuff is worth it, you can buy me a beer in return. * 
 * ----------------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Media;
using NAudio.Wave;
using NAudio;
using System.Windows.Forms;
using System.Diagnostics;

namespace DSPADPCMUI
{
    public partial class frmMain : Form
    {
        string appPath = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName;

        FileInfo inputFile
        {
            get;
            set;
        }

        FileInfo outputFile
        {
            get;
            set;
        }

        public frmMain()
        {
            InitializeComponent();
    }

        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = this.appPath;
            ofd.Filter = "Wave Audio Files (*.wav)|*.wav";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                this.txtInput.Text = ofd.FileName;
                this.inputFile = new FileInfo(ofd.FileName);
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = this.appPath;
            sfd.Filter = "Nintendo GC Audio File (*.dsp)|*.dsp";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                this.txtOutput.Text = sfd.FileName;
                this.outputFile = new FileInfo(sfd.FileName);
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            //logfile output
            string output = "";

            output += "Initalizing...";

            //Handling some temporary file stuff because windows console and spaces suck and we want to be able to convert files from anywhere to anywhere
            FileInfo tempWav = new FileInfo($@"{appPath}\{inputFile.Name.Replace(inputFile.Extension, "")}_16_1.wav");
            FileInfo logFile = new FileInfo($@"{appPath}\log.txt");
            FileInfo tempDsp = new FileInfo($@"{appPath}\{tempWav.Name.Replace("_16_1.wav", ".dsp")}");
            
            //used for constructing the fixed header
            byte[] fixedWavHeader = new byte[44];

            output += "Reading and converting *.wav file";
            WaveStream converter = WaveFormatConversionStream.CreatePcmStream(new WaveFileReader(this.inputFile.FullName));
            WaveFormat target = new WaveFormat(11025, 16, 1);
            WaveFormatConversionStream cs = new WaveFormatConversionStream(target, converter);

            MemoryStream ms = new MemoryStream();
            WaveFileWriter.CreateWaveFile(ms, cs);
            ms = new MemoryStream(ms.ToArray());

            //Now we have a convertable wav file in memory except for a 46 byte file header (naudio always writes these) which will crash DSPADPCM 
            //the two extra bytes are included right before the data subchunck which is located at byte 25h (byte 37d)

            output += "|-> fixing wav header";

            BinaryReader br = new BinaryReader(ms);
            for (int i = 0; i < 44; i++)
            {
                if (i == 36)
                {
                    //Skip byte 37 and 38
                    br.BaseStream.Seek(2, SeekOrigin.Current);
                }
                //else if (i == 4)
                //{
                //    //Fix filesize
                //    uint filesize = br.ReadUInt32();
                //    filesize = filesize - 2;
                //    byte[] filesizebytes = BitConverter.GetBytes(filesize);
                //    fixedWavHeader[i] = filesizebytes[0];
                //    fixedWavHeader[i+1] = filesizebytes[1];
                //    fixedWavHeader[i+2] = filesizebytes[2];
                //    fixedWavHeader[i+3] = filesizebytes[3];
                //    i = i + 4;
                //    continue;
                //}
                //else if (i == 16)
                //{
                //    //Fix fmt subchunck size
                //    uint fmtchuncksize = br.ReadUInt32();
                //    fmtchuncksize = fmtchuncksize - 2;
                //    byte[] fmtchunckbytes = BitConverter.GetBytes(fmtchuncksize);
                //    fixedWavHeader[i] = fmtchunckbytes[0];
                //    fixedWavHeader[i + 1] = fmtchunckbytes[1];
                //    fixedWavHeader[i + 2] = fmtchunckbytes[2];
                //    fixedWavHeader[i + 3] = fmtchunckbytes[3];
                //    i = i + 4;
                //    continue;
                //}
                fixedWavHeader[i] = br.ReadByte();
            }

            output += "Writing convertible wav to disk";

            //Now we are ready to write our converted and fixed wav to the disk
            BinaryWriter bw = new BinaryWriter(new FileStream(tempWav.FullName, FileMode.Create));
            bw.Write(fixedWavHeader, 0, 44); //write header
            bw.Write(br.ReadBytes((int)br.BaseStream.Length - 46)); //write data

            br.Close();
            bw.Close();
            converter.Close();
            cs.Close();

            output += $"Starting DSPADPCM -E";

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = $@"DSPADPCM";
            p.StartInfo.Arguments = $@"-E {tempWav.Name} {tempDsp.Name}";
            p.Start();

            output += p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            output += "Cleaning up";

            File.Copy(tempDsp.FullName, outputFile.FullName);
            File.Delete(tempDsp.FullName);
            File.Delete(tempWav.FullName);

            output += "Done. Writing logfile";

            StreamWriter sw = new StreamWriter(new FileStream(logFile.FullName, FileMode.Create));
            sw.Write(output);
            sw.Close();

            MessageBox.Show("Done!");
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("DSPADPCM UI wrapper by MaDOS\n\r\n\r" +
                                "This will convert nearly any wav file to a dsp convertable 16 bit 11025 bitrate wav file and then use dspadpcm to convert it into a dsp\n\r" +
                                "This programm has to be in one directory with dspadpcm.exe, dsptool.dll and soundfile.dll found in the Gamecube SDK. It also needs NAudio.dll which it should ship with.\n\r\n\r" +
                                "Browse for an input *.wav file and specify and out *.dsp file then click convert. Done.\n\r\n\r" +
                                "Should you encounter any bugs please contact me on github. (https://github.com/MaDOS/DSPADPCMUI)");
        }
    }
}
