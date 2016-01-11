using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Policework_v2
{
    class WriteToVideo
    {
        // input parameters
        private String[] filelist;
        private String outputfolder;
        private String ffmpegLocation;
        private Tracker[] trackerlist;
        private Boolean hidden;

        // for easy removal
        private List<String> temporaryFiles;

        public WriteToVideo(String[] filelistin, Tracker[] trackerlistin, String outputfolderin, String ffmpegLocationin, Boolean hiddenIn)
        {
            filelist = filelistin;
            trackerlist = trackerlistin;
            outputfolder = outputfolderin;
            ffmpegLocation = ffmpegLocationin;
            hidden = hiddenIn;
        }

        // 0: succes, 1: error, 2: no movement in file
        public int WriteVideo(ProgressStorage progressStorage)
        {
            int movementSomewhere = 1;
            // create txt file that will be used to join all videos
            temporaryFiles = new List<String>();
            String txtPath = outputfolder + "\\output_temptxtfile.txt";
            temporaryFiles.Add(txtPath); // add to files that will be removed after execution
            StreamWriter outputtxt = new StreamWriter(txtPath, false);
            // convert to mp4 in original quality, create subparts and add then to the outputtxt
            for (int i = 0; i < filelist.Length; i++)
            {
                String toDeleteFile = convertPart(filelist[i]); // no result needed, because if this failed, next one will fail as well
                int result = cutParts(toDeleteFile, trackerlist[i], outputtxt);
                if(result == 1) // it failed
                {
                    // remove all temporary objects here (close txt first)
                    outputtxt.Close();
                    clearFiles(temporaryFiles);
                    progressStorage.resetGeneration(); // update progressStorage
                    return 1; // something failed
                } else if (result == 0)
                {
                    // if we get in to this part at least once, it means there is movement in some of the files
                    movementSomewhere = 0;
                }
                clearFile(toDeleteFile); // delete temp file
                progressStorage.generationFileFinished(); // update progressStorage
            }
            int result2;
            outputtxt.Close();
            if(movementSomewhere == 0)
            {
                // execute the command to join the subfiles
                result2 = joinParts(txtPath);
            } else
            {
                result2 = 2;
            }
            // remove all temporary files
            clearFiles(temporaryFiles);
            if(result2 != 1)
            {
                progressStorage.generationFileFinished(); // update progressStorage
            }
            else
            {
                progressStorage.resetGeneration(); // update progressStorage
            }
            return result2;
        }

        // Convert the part with libx264
        private String convertPart(String file)
        {
            // ffmpeg -i input.avi -c:v libx264 -preset ultrafast out.mp4
            String path = outputfolder + "\\Temp_File_" + Path.GetFileNameWithoutExtension(file) + ".mp4";
            String command = "/C "; // quit terminal after executing
            //command += ffmpegLocation; // add ffmpeglocation
            command += "cd \"" + Path.GetDirectoryName(ffmpegLocation) + "\" && ";
            command += Path.GetFileName(ffmpegLocation);
            command += " -i " + "\"" + file + "\""; // add file location
            command += " -c:v libx264 -preset ultrafast ";
            command += "\"" + path + "\"";
            clearFile(path);
            // execute the command
            int result = runCommand(command);
            return path;
        }

        // Joins all the parts specified in the txt file
        private int joinParts(String txtPath)
        {
            // ffmpeg -f concat -i myfiles.txt -c copy output.mp4
            String time = DateTime.Now.Day.ToString("00") + DateTime.Now.Month.ToString("00") + DateTime.Now.Year + "_" + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00");
            String path = outputfolder + "\\output_" + Path.GetFileNameWithoutExtension(filelist[0]) + "_" + time + ".mp4";
            String command = "/C "; // quit terminal after executing
            //command += ffmpegLocation; // add ffmpeglocation
            command += "cd \"" + Path.GetDirectoryName(ffmpegLocation) + "\" && ";
            command += Path.GetFileName(ffmpegLocation);
            command += " -f concat"; // add concat command
            command += " -i " + "\"" + txtPath + "\""; // add file location
            command += " -c copy "; // add rest of command
            command += "\"" + path + "\""; // add output location
            // execute the command
            int result = runCommand(command);
            return result; // 0 for succes, 1 for fail
        }

        // create command
        // add to txt
        // remove files that will be created
        // execute command
        private int cutParts(String file, Tracker tracker, StreamWriter txtfile)
        {
            String[] outputFiles;
            List < int[] > timelist = tracker.getList();
            // check if tracker empty
            if (timelist == null)
            {
                return 2; // make it return 2 instead of 0, so we can detect if no movement in whole file
            }
            // loop over all elements and create command string and add to txt
            outputFiles = new String[timelist.Count];
            for (int i = 0; i < timelist.Count; i++)
            {
                String outputfile = outputfolder + "\\" + Path.GetFileNameWithoutExtension(file) + "_" + i + ".mp4";
                outputFiles[i] = outputfile;
                temporaryFiles.Add(outputfile);
                // Command = ffmpeg -i testvid.avi -ss 00:01:34 -to 00:02:22 -c:v libx264 -preset ultrafast newStream1.mp4
                String command = "/C "; // quit terminal after executing
                //command += ffmpegLocation; // add ffmpeglocation
                command += "cd \"" + Path.GetDirectoryName(ffmpegLocation) + "\" && ";
                command += Path.GetFileName(ffmpegLocation);
                command += " -i " + "\"" + file + "\""; // add file location
                command += " -ss " + convertLength(timelist[i][0]);
                command += " -to " + convertLength(timelist[i][1]);
                //command += " -vcodec copy -preset ultrafast "; -- use this for speed but lower accuracy
                command += " -preset ultrafast ";
                command += "\"" + outputfile + "\""; // add outputfile
                // add to txt: "file 'C:\Users\michael\Documents\newtestvids\newStream1.mp4'"
                String toAddToTxt = "file '" + outputfile + "'";
                txtfile.WriteLine(toAddToTxt);
                // execute command
                clearFile(outputfile);
                int result = runCommand(command);
                if(result != 0)
                {
                    return 1;
                }
            }
            // add outputfiles to the temporaryfiles list
            temporaryFiles.AddRange(outputFiles);
            return 0; // 0 for succes, 1 for fail
        }

        // remove all files specified in the input String[]
        private void clearFiles(String[] toBeRemoved)
        {
            for(int i = 0; i < toBeRemoved.Length; i++)
            {
                clearFile(toBeRemoved[i]);
            }
        }

        private void clearFiles(List<String> toBeRemoved)
        {
            for (int i = 0; i < toBeRemoved.Count; i++)
            {
                clearFile(toBeRemoved[i]);
            }
        }

        // remove all files specified in the string
        public void clearFile(String file)
        {
            // clearFile file from folder in case it already exists, so it doesn't block the command
            // and for later removal
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        // Function to convert frames in to hours, minutes, seconds
        private String convertLength(int length)
        {
            int hour = length / 7200;
            length -= hour * 7200;
            int minute = length / 120;
            length -= minute * 120;
            int second = length / 2;
            return (hour.ToString("00") + ":" + minute.ToString("00") + ":" + second.ToString("00"));
        }

        private int runCommand(String command)
        {
            // Execute ffmpeg conversion
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = command;
            if (hidden == true)
            {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            else
            {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            }
            process.Start();
            process.WaitForExit();
            // Log exit code
            int exitcode = process.ExitCode;
            if(exitcode != 0)
            {
                return 1;
            } else
            {
                return 0;
            }
        }
    }
}
