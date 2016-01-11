using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Policework_v2
{
    // Uses ffmpeg to convert a video file to the standard format (320x240, 2 fps, .mp4 x264 coding)
    class Convertor
    {
        private String file;
        private String outputfolder;
        private String outputfile; // where the tmeporary converted file is for later removal
        private String ffmpegLocation;
        public Convertor(String filein, String outputfolderin, String ffmpegLocationin)
        {
            file = filein;
            outputfolder = outputfolderin;
            ffmpegLocation = ffmpegLocationin;
        }

        // Function to convert one file
        public int doConvert(Boolean hidden)
        {
            // Calculate outputfile:
            outputfile = outputfolder + "\\" + Path.GetFileNameWithoutExtension(file) + "Converted.mp4";
            String command = "";
            command = "/C "; // quit terminal after executing
            command += "cd \"" + Path.GetDirectoryName(ffmpegLocation) + "\" && ";
            command += Path.GetFileName(ffmpegLocation);
            command += " -i " + "\"" + file + "\""; // add file location
            command += " -vf scale=320:240 -r 2 -an -c:v libx264 -preset veryfast "; // rest of settings
            command += "\"" + outputfile + "\""; // add outputfile
            // clear file if it exists and run command:
            clearFile();
            int succes = runCommand(command, hidden);
            return succes;
        }

        public void clearFile()
        {
            // clearFile file from folder in case it already exists, so it doesn't block the command
            // and for later removal
            if (File.Exists(outputfile))
            {
                File.Delete(outputfile);
            }
        }


        private int runCommand(String command, Boolean hidden)
        {
            // Execute ffmpeg conversion
            Console.WriteLine(command);
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = command;
            if(hidden == true)
            {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            } else
            {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            }
            process.Start();
            process.WaitForExit();
            // Log exit code
            int exitcode = process.ExitCode;
            return exitcode;
        }

    }
}
