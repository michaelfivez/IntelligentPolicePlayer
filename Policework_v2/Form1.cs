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
using System.Threading;
using System.Diagnostics;

namespace Policework_v2
{
    public partial class Form1 : Form
    {
        Form2 SettingsWindow;
        // if busy == 1, accept no button actions
        private int Busy;
        // Classes that are used for processing (Brain)
        private Brain brain;
        // For drawing the box on pictureframe
        private Point RectStartPoint;
        private Rectangle Rect = new Rectangle();
        private Brush selectionBrush = new SolidBrush(Color.FromArgb(128, 72, 145, 220));
        // store loadfolder and savefolder (individual files are given directly to the brain)
        private String loadFolderLocation = "";
        private String saveFolderLocation = "";

        public Form1()
        {
            InitializeComponent();
            initializeBrain();
        }

        private void closeBrain()
        {
            // remove variables
            Rect = new Rectangle();
            loadFolderLocation = "";
            saveFolderLocation = "";
            // clean gui
            pictureBox1.Image = null;
            updateAreaLabels();
            updateSourceLocation();
            updateOutputLocation();
            updateConvertLabel(2);
            updateLoadLabel(2);
            updateProcessLabel(2);
        }

        private void initializeBrain()
        {
            SettingsWindow = new Form2(); // contains the settings
            int result = SettingsWindow.loadSettings();
            if (result == 0)
            {
                writeLineToConsole("Location of ffmpeg and folder for temporary converted files loaded from previous session");
            }
            else
            {
                writeLineToConsole("Initial values set for location of ffmpeg and folder for temporary converted files");
                writeLineToConsole("You can change these in the 'settings' menu");
            }
            // load all settings and add them to brain
            int valueOfObjectSize = SettingsWindow.getValueOfObjectSize();
            int valueofMovementWindow = SettingsWindow.getValueofMovementWindow();
            String convertedOutputFolder = SettingsWindow.getConvertedOutputFolder();
            String ffmpegLocation = SettingsWindow.getFfmpegLocation();

            ffmpegLocation = Path.GetDirectoryName(ffmpegLocation) + "\\" + Path.GetFileNameWithoutExtension(ffmpegLocation);

            brain = new Brain();
            // Settings:
            updateThresholdLabel();
            brain.updateThreshold(trackBar1.Value);
            brain.addSeriousSettings(ffmpegLocation, convertedOutputFolder);
            brain.addLessSeriousSettings(valueOfObjectSize, valueofMovementWindow);

            // not busy
            Busy = 0;
        }

        private void updateThresholdLabel()
        {
            label8.Text = "Threshold = " + trackBar1.Value;
        }

        private void updateAreaLabels()
        {
            label6.Text = "Selected Area : " +  Rect.Size.ToString();
            label7.Text = "Start Point : " + Rect.Location.ToString();
        }

        private void updateSourceLocation()
        {
            textBox2.Text = loadFolderLocation;
        }

        private void updateOutputLocation()
        {
            textBox3.Text = saveFolderLocation;
        }

        private void updateConvertLabel(int result)
        {
            // 0 is succes, 1 is error, and 2 is reset
            if (result == 0)
            {
                label1.Text = "Succes";
            }
            else if(result == 1)
            {
                label1.Text = "Failed";
            }
            else if (result == 2)
            {
                label1.Text = "";
            }
        }

        private void updateLoadLabel(int result)
        {
            // 0 is succes, 1 is error, and 2 is reset
            if (result == 0)
            {
                label2.Text = "Succes";
            }
            else if (result == 1)
            {
                label2.Text = "Failed";
            }
            else if (result == 2)
            {
                label2.Text = "";
            }
        }

        private void updateProcessLabel(int result)
        {
            // 0 is succes, 1 is error, and 2 is reset
            if (result == 0)
            {
                label3.Text = "Succes";
            }
            else if (result == 1)
            {
                label3.Text = "Failed";
            }
            else if (result == 2)
            {
                label3.Text = "";
            }
        }

        // function so I can later add time for example
        // Each input send gets written to a new line
        private void writeLineToConsole(string input)
        {
            if (textBox1.Text.Length == 0)
                textBox1.Text = " - " + input;
            else
                textBox1.AppendText("\r\n - " + input);
        }

        // For drawing the box on pictureframe
        private void pictureBox1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Determine the initial rectangle coordinates...
            RectStartPoint = e.Location;
            Invalidate();
        }

        // Draw Rectangle
        //
        private void pictureBox1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            Point tempEndPoint = e.Location;
            Rect.Location = new Point(
                Math.Min(RectStartPoint.X, tempEndPoint.X),
                Math.Min(RectStartPoint.Y, tempEndPoint.Y));
            Rect.Size = new Size(
                Math.Abs(RectStartPoint.X - tempEndPoint.X),
                Math.Abs(RectStartPoint.Y - tempEndPoint.Y));
            // Make Sure that Start and size are in range
            if (Rect.X < 0)
            {
                Rect.X = 0;
            }
            if (Rect.Y < 0)
            {
                Rect.Y = 0;
            }
            if (Rect.X + Rect.Width > 320)
            {
                Rect.Width = 320 - Rect.X;
            }
            if (Rect.Y + Rect.Height > 240)
            {
                Rect.Height = 240 - Rect.Y;
            }
            updateAreaLabels();
            brain.setRectangle(Rect);
            pictureBox1.Invalidate();
        }

        // Draw Area
        //
        private void pictureBox1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Draw the rectangle...
            if (pictureBox1.Image != null)
            {
                if (Rect != null && Rect.Width > 0 && Rect.Height > 0)
                {
                    e.Graphics.FillRectangle(selectionBrush, Rect);
                }
            }
        }

        // Open Single File menu item
        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // busy check
            if(Busy == 1)
            {
                writeLineToConsole("ERROR: Application busy, wait untill current action is done");
                return;
            }

            // Only allow the selection of one file
            openFileDialog1.Title = "Open Video File";
            openFileDialog1.Multiselect = false;
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                String filename = openFileDialog1.FileName;
                // reset file list in brain and add this file to brain
                String[] filenames = new String[1];
                filenames[0] = filename;
                int okay = brain.newFiles(filenames);
                if(okay == 0)
                {
                    loadFolderLocation = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar; // get foldername
                    saveFolderLocation = loadFolderLocation;
                    // Write to console the folder and then file selected : 
                    writeLineToConsole("Source Folder: " + loadFolderLocation);
                    writeLineToConsole("Output Folder: " + saveFolderLocation);
                    brain.setResultFolder(saveFolderLocation);
                    writeLineToConsole("File selected: " + Path.GetFileName(filename));
                    updateConvertLabel(2);
                    updateLoadLabel(2);
                    updateProcessLabel(2);
                } else
                {
                    writeLineToConsole("ERROR: No new files selected, reset first");
                }
            }
            updateSourceLocation(); // update label
            updateOutputLocation();
        }

        // Open Multiple File menu item
        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // busy check
            if (Busy == 1)
            {
                writeLineToConsole("ERROR: Application busy, wait untill current action is done");
                return;
            }

            // Allow the selection of multiple files
            openFileDialog1.Title = "Open Video File(s)";
            openFileDialog1.Multiselect = true;
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                String[] filenames = openFileDialog1.FileNames;
                String[] filenamestoprint = new String[filenames.Length];
                // reset file list in brain and add these files to brain
                int okay = brain.newFiles(filenames);
                if(okay == 0)
                {
                    loadFolderLocation = Path.GetDirectoryName(filenames[0]) + Path.DirectorySeparatorChar; // get foldername
                    saveFolderLocation = loadFolderLocation;
                    // Write to console the folder and then all the files selected
                    writeLineToConsole("Source Folder: " + loadFolderLocation);
                    writeLineToConsole("Output Folder: " + saveFolderLocation);
                    brain.setResultFolder(saveFolderLocation);
                    for (int i = 0; i < filenamestoprint.Length; i++)
                    {
                        filenamestoprint[i] = Path.GetFileName(filenames[i]);
                    }
                    writeLineToConsole("File(s) selected: " + String.Join(",", filenamestoprint));
                    updateConvertLabel(2);
                    updateLoadLabel(2);
                    updateProcessLabel(2);
                } else
                {
                    writeLineToConsole("ERROR: No new files selected, reset first");
                }
            }
            updateSourceLocation(); // update label
            updateOutputLocation();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            // busy check
            if (Busy == 1)
            {
                writeLineToConsole("WARNING: Changing the settings during processing can give unexpected results and crashes");
            }

            updateThresholdLabel();
            brain.updateThreshold(trackBar1.Value);
        }

        // PROCESS button
        private void button3_Click(object sender, EventArgs e)
        {
            // busy check
            if (Busy == 1)
            {
                writeLineToConsole("ERROR: Application busy, wait untill current action is done");
                return;
            }

            // RUN ON NEW THREAD
            backgroundWorker2.RunWorkerAsync();

        }

        // Choose save folder menu
        private void chooseSaveLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // busy check
            if (Busy == 1)
            {
                writeLineToConsole("ERROR: Application busy, wait untill current action is done");
                return;
            }

            // Open File menu item
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                saveFolderLocation = folderBrowserDialog1.SelectedPath;
                // Pass path to brain
                brain.setResultFolder(saveFolderLocation);
                // Write to console the folder
                writeLineToConsole("Output Folder: " + saveFolderLocation);
            }
            updateOutputLocation(); // uodate label
        }

        // CONVERT button
        private void button1_Click(object sender, EventArgs e)
        {
            // busy check
            if (Busy == 1)
            {
                writeLineToConsole("ERROR: Application busy, wait untill current action is done");
                return;
            }

            // RUN ON NEW THREAD
            backgroundWorker1.RunWorkerAsync();
        }

        // LOAD BUTTON
        private void button2_Click(object sender, EventArgs e)
        {
            // busy check
            if (Busy == 1)
            {
                writeLineToConsole("ERROR: Application busy, wait untill current action is done");
                return;
            }

            // Get preview frame and print the videolengths to textbox
            if (brain.Status == 2)
            {
                writeLineToConsole("ERROR: Files already processed, press reset to start again with new files");
            } else
            {
                Bitmap preview = brain.getPreview();
                if (preview == null)
                {
                    writeLineToConsole("ERROR: Could not load videos, did you convert the files first?");
                    updateLoadLabel(1);
                }
                else
                {
                    writeLineToConsole("File(s) loaded");
                    pictureBox1.Image = preview;
                    // print videolengths
                    String[] lengthsarray = brain.getVideoLengths();
                    String lengths = String.Join(", ", lengthsarray);
                    writeLineToConsole("Video lengths = " + lengths);
                    updateLoadLabel(0);
                    updateProcessLabel(2);
                }
            }
        }

        // GET TXT BUTTON
        private void button4_Click(object sender, EventArgs e)
        {
            // busy check
            if (Busy == 1)
            {
                writeLineToConsole("ERROR: Application busy, wait untill current action is done");
                return;
            }

            int result = brain.writeToTxt();
            if (result == 0)
            {
                // succes
                writeLineToConsole("Output written to txt file");
                writeLineToConsole("File in " + brain.getResultFolder());
            }
            else if (result == 1)
            {
                writeLineToConsole("ERROR: No result to write, convert and process files first");
                // not yet processed/ converted
            }
            else if (result == 2)
            {
                writeLineToConsole("ERROR: No output folder selected, select one from 'choose save location' in the file menu");
                // no input files
            }
            else
            {
                writeLineToConsole("ERROR: Writing of txt file failed (perhaps a protected outputfolder ?)");
                // writing failed
            }
        }

        // every 500 ms the progressbars and labels below them get updated
        private void timer1_Tick(object sender, EventArgs e)
        {
            brain.progressStorage.updateGui(); // update progressbars
            // get all the parameters
            int totFiles = brain.progressStorage.totFiles;
            int currentFileConverting = brain.progressStorage.currentFileConverting;
            int currentFileProcessing = brain.progressStorage.currentFileProcessing;
            int currentFileGenerating = brain.progressStorage.currentFileGenerating;
            int ConvertingProgress = brain.progressStorage.ConvertingProgress;
            int ProcessingProgress = brain.progressStorage.ProcessingProgress;
            int GeneratingProgress = brain.progressStorage.GeneratingProgress;
            // update the progress bars and lables
            progressBar1.Value = ConvertingProgress;
            progressBar2.Value = ProcessingProgress;
            progressBar3.Value = GeneratingProgress;
            if(totFiles != 0)
            {
                label11.Text = currentFileConverting + " out of " + totFiles + " done";
                label12.Text = currentFileProcessing + " out of " + totFiles + " done";
                label13.Text = currentFileGenerating + " out of " + totFiles + " done";
                if (currentFileGenerating == totFiles)
                {
                    label13.Text = "Joining Subparts";
                }
                if (ConvertingProgress == 100) label11.Text = "Finished";
                if (ProcessingProgress == 100) label12.Text = "Finished";
                if (GeneratingProgress == 100) label13.Text = "Finished";
            } else
            {
                label11.Text = "";
                label12.Text = "";
                label13.Text = "";
            }
            progressBar1.Update();
            progressBar2.Update();
            progressBar3.Update();
            label11.Update();
            label12.Update();
            label13.Update();
        }

        // Here is the converting started
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Busy = 1;
            int result = brain.doConvert();
            e.Result = result;
        }

        // Here is the converting finished
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int result = (int)e.Result;
            if (result == 0)
            {
                // succes
                writeLineToConsole("Succesfull conversion");
                writeLineToConsole("Files converted to " + brain.getConvertOutput());
                updateConvertLabel(0);
                updateLoadLabel(2);
                updateProcessLabel(2);
            }
            else if (result == 1)
            {
                writeLineToConsole("ERROR: Files already converted, reset first if you want to convert new files");
                // already converted
            }
            else if (result == 2)
            {
                writeLineToConsole("ERROR: No input files selected, open files first from the open file menu");
                updateConvertLabel(1);
                // no input files
            }
            else
            {
                writeLineToConsole("ERROR: Conversion of some files failed (uncompatible video files or error in location/configuration of ffmpeg)");
                writeLineToConsole("All temporary converted files removed");
                updateConvertLabel(1);
                brain.progressStorage.reset();
                // converting failed
            }
            Busy = 0;
        }

        // Here is the processing started
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            Busy = 1;
            int result = brain.doProcess();
            e.Result = result;
        }

        // Here is the processing finished
        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int result = (int)e.Result;
            if (result == 0)
            {
                // succes
                writeLineToConsole("Succesfull processing");
                updateProcessLabel(0);
            }
            else if (result == 1)
            {
                writeLineToConsole("ERROR: No converted video files found, Convert the files first with the convert button");
                updateProcessLabel(1);
                // process first
            }
            else if (result == 2)
            {
                writeLineToConsole("ERROR: No area selected for analyzing, draw a rectangle on the preview picture first");
                updateProcessLabel(1);
                // no input files
            }
            else if (result == 3)
            {
                writeLineToConsole("ERROR: Processing of some files failed (maybe something went wrong during converting, relaunch program and try again)");
                updateProcessLabel(1);
                brain.progressStorage.resetProcessing();
                // converting failed
            }
            else
            {
                writeLineToConsole("Succesfull processing");
                writeLineToConsole("Files were already processed, the previous results have been overwritten");
                updateProcessLabel(0);
                // already processed
            }
            Busy = 0;
        }

        // get VIDEO button
        private void button5_Click(object sender, EventArgs e)
        {
            // busy check
            if (Busy == 1)
            {
                writeLineToConsole("ERROR: Application busy, wait untill current action is done");
                return;
            }

            // RUN ON NEW THREAD
            backgroundWorker3.RunWorkerAsync();
        }

        // Here is the final video generation started
        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            Busy = 1;
            int result = brain.writeToVideo();
            e.Result = result;
        }

        // Here is the final video generation finished
        private void backgroundWorker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int result = (int)e.Result;
            if (result == 0)
            {
                // succes
                writeLineToConsole("Output written to video file");
                writeLineToConsole("File in " + brain.getResultFolder());
            }
            else if (result == 1)
            {
                writeLineToConsole("ERROR: No result to write, convert and process files first");
                // not yet processed/ converted
            }
            else if (result == 2)
            {
                writeLineToConsole("ERROR: No output folder selected, select one from 'choose save location' in the file menu");
                // no input files
            }
            else if (result == 3)
            {
                writeLineToConsole("No movement in the file(s), no output video created");
                // no movement in file(s)
            }
            else
            {
                writeLineToConsole("ERROR: Writing of video file failed (perhaps a protected outputfolder ?)");
                // writing failed
            }
            Busy = 0;
        }

        // Settings button
        private void settingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // busy check
            if (Busy == 1)
            {
                writeLineToConsole("ERROR: Application busy, wait untill current action is done");
                return;
            }
            SettingsWindow.storeSettings();
            DialogResult dialogresult = SettingsWindow.ShowDialog();
            if (dialogresult == DialogResult.OK)
            {
                // load all settings and add them to brain
                int valueOfObjectSize = SettingsWindow.getValueOfObjectSize();
                int valueofMovementWindow = SettingsWindow.getValueofMovementWindow();
                brain.addLessSeriousSettings(valueOfObjectSize, valueofMovementWindow);
                // the two below are illegal to change after start
                String convertedOutputFolder = SettingsWindow.getConvertedOutputFolder();
                String ffmpegLocation = SettingsWindow.getFfmpegLocation();
                ffmpegLocation = Path.GetDirectoryName(ffmpegLocation) + "\\" + Path.GetFileNameWithoutExtension(ffmpegLocation);
                int result = brain.addSeriousSettings(ffmpegLocation, convertedOutputFolder);
                if(result == 0)
                {
                    writeLineToConsole("New Settings saved");                  
                }
                else
                {
                    writeLineToConsole("Only ObjectSize and MovementWindow changed");
                    writeLineToConsole("ffmpegLocationg and ConvertOutputFolder are only allowed to change before any converting, or after resetting");
                    // restore these in form2:
                    SettingsWindow.restoreSettingsOfStrings();
                }               
            }
            else if (dialogresult == DialogResult.Cancel)
            {
                writeLineToConsole("Settings not saved");
                SettingsWindow.restoreSettings();
            }
            Properties.Settings.Default.Save();
        }

        // RESET button
        private void button6_Click(object sender, EventArgs e)
        {
            // busy check
            if (Busy == 1)
            {
                writeLineToConsole("ERROR: Application busy, wait untill current action is done");
                return;
            }

            brain.discardConvertedFiles();
            writeLineToConsole("The temporary Converted files have been removed");
            closeBrain();
            writeLineToConsole("State resetted");
            initializeBrain();
        }

        // Detailed guide menu item
        private void detailedGuideToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // busy check
            if (Busy == 1)
            {
                writeLineToConsole("ERROR: Application busy, wait untill current action is done");
                return;
            }

            // Open the README txt
            if (File.Exists("README.txt"))
            {
                writeLineToConsole("Opening README.txt");
                Process process = new Process();
                process.StartInfo.FileName = "README.txt";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                process.Start();
            }
            else
            {
                writeLineToConsole("ERROR: The README is not found, did you remove it? You can find it again in the original installation-zip");
            }       
        }

        // quick guide menu item
        private void detailedGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // busy check
            if (Busy == 1)
            {
                writeLineToConsole("ERROR: Application busy, wait untill current action is done");
                return;
            }

            Form3 quickguide = new Form3();
            writeLineToConsole("Quick guide opened");
            quickguide.ShowDialog();
        }

        // hide terminal checkbox gets selected or deselected
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            brain.doHideTerminal(checkBox1.Checked);
            writeLineToConsole("Change will be applied next time ffmpeg gets called in the terminal");
        }
    }
}
