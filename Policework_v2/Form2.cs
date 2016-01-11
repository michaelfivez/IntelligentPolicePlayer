using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Policework_v2
{
    public partial class Form2 : Form
    {
        // ConvertOutputFolder and ffmpegLocation persist if program gets closed
        private String convertedOutputFolder;
        private String ffmpegLocation; // need to remove the .exe to use in the commands
        private int valueOfObjectSize = 50;
        private int valueofMovementWindow = 5;
        // Old
        private String convertedOutputFolderold;
        private String ffmpegLocationold; 
        private int valueOfObjectSizeold;
        private int valueofMovementWindowold;

        public Form2()
        {
            InitializeComponent();
            trackBar1.Value = valueOfObjectSize;
            trackBar2.Value = valueofMovementWindow;
        }

        // Loads the settings from the settingsfile. return 0: settings loaded, 1: default settings set
        public int loadSettings()
        {
            if(Properties.Settings.Default.ConvertOutput == "none") 
            {
                // calculate the defaults on first startup
                Properties.Settings.Default.ConvertOutput = defaultConverted();
                Properties.Settings.Default.ffmpeglocation = defaultFFMpeg();
                // copy the settings
                convertedOutputFolder = Properties.Settings.Default.ConvertOutput;
                ffmpegLocation = Properties.Settings.Default.ffmpeglocation;
                updateConvertOutputLabel();
                updateFfmpegLocationLabel();
                updateObjectSizeLabel();
                updateMovementWindowLabel();
                return 1;
            }
            else
            {
                // copy the settings
                convertedOutputFolder = Properties.Settings.Default.ConvertOutput;
                ffmpegLocation = Properties.Settings.Default.ffmpeglocation;
                updateConvertOutputLabel();
                updateFfmpegLocationLabel();
                updateObjectSizeLabel();
                updateMovementWindowLabel();
                return 0;
            }           
        }

        // four getters to get the settings
        public int getValueOfObjectSize()
        {
            return valueOfObjectSize;
        }
        public int getValueofMovementWindow()
        {
            return valueofMovementWindow;
        }
        public String getConvertedOutputFolder()
        {
            return convertedOutputFolder;
        }
        public String getFfmpegLocation()
        {
            return ffmpegLocation;
        }

        public void storeSettings()
        {
            convertedOutputFolderold = convertedOutputFolder;
            ffmpegLocationold = ffmpegLocation;
            valueOfObjectSizeold = valueOfObjectSize;
            valueofMovementWindowold = valueofMovementWindow;
        }
        public void restoreSettings()
        {
            convertedOutputFolder = convertedOutputFolderold;
            ffmpegLocation = ffmpegLocationold;
            valueOfObjectSize = valueOfObjectSizeold;
            valueofMovementWindow = valueofMovementWindowold;
            trackBar1.Value = valueOfObjectSizeold;
            trackBar2.Value = valueofMovementWindowold;
            updateObjectSizeLabel();
            updateMovementWindowLabel();
            updateConvertOutputLabel();
            updateFfmpegLocationLabel();
        }

        public void restoreSettingsOfStrings()
        {
            convertedOutputFolder = convertedOutputFolderold;
            ffmpegLocation = ffmpegLocationold;
            updateConvertOutputLabel();
            updateFfmpegLocationLabel();
        }

        private void updateObjectSizeLabel()
        {
            label3.Text = "Pixels : " + trackBar1.Value;
            valueOfObjectSize = trackBar1.Value;
        }

        private void updateMovementWindowLabel()
        {
            label4.Text = "Seconds : " + trackBar2.Value;
            valueofMovementWindow = trackBar2.Value;
        }

        private void updateConvertOutputLabel()
        {
            textBox1.Text = convertedOutputFolder;
            Properties.Settings.Default.ConvertOutput = convertedOutputFolder;
        }

        private void updateFfmpegLocationLabel()
        {
            textBox2.Text = ffmpegLocation;
            Properties.Settings.Default.ffmpeglocation = ffmpegLocation;
        }

        // Function to find the directory the file is, to be able to get relative paths
        private String getBaseDirectory()
        {
            String baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return baseDirectory;
        }

        // To select the output for the converted files
        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                convertedOutputFolder = folderBrowserDialog1.SelectedPath;
                updateConvertOutputLabel();
            }
        }

        // To select the location of ffmpeg
        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Select ffmpeg in /ffmpeg/bin/ffmpeg.exe";
            openFileDialog1.Multiselect = false;
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                ffmpegLocation = openFileDialog1.FileName;
                updateFfmpegLocationLabel();
            }
        }

        // To put output for converted files in default
        private void button6_Click(object sender, EventArgs e)
        {
            convertedOutputFolder = defaultConverted();
            updateConvertOutputLabel();
        }

        public String defaultConverted()
        {
            String path = getBaseDirectory();
            path += "ConvertOutput";
            return path;
        }

        public String defaultFFMpeg()
        {
            String path = getBaseDirectory();
            path += "ffmpeg\\bin\\ffmpeg.exe";
            return path;
        }

        // To put ffmpeg location in default
        private void button5_Click(object sender, EventArgs e)
        {
            ffmpegLocation = defaultFFMpeg();
            updateFfmpegLocationLabel();
        }

        // Value of object size changed
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            updateObjectSizeLabel();
        }

        // Value of movement window changed
        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            updateMovementWindowLabel();
        }
    }
}
