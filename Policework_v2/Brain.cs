using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Policework_v2
{
    class Brain
    {
        // holds the progress for GUI update
        public ProgressStorage progressStorage;
        // Status information
        private Tracker[] trackerlist; // Keeps the list of trackers with the results
        private Convertor[] convertorList; // Keeps all the convertors for later file removal
        public int Status; //Keeps the stage of the processor (0:idle,1:afterconverting,2:afterprocessing)
        // All the 'settings':
        private int threshold;  // used in processing
        private int objectSize;  // used in processing
        private int movementWindow;  // used in processing
        private String convertOutputLocation; // folder of output of converted files
        private String ffmpegLocation; // Location of ffmpeg
        private Rectangle Rect = new Rectangle(); // Area to analyze
        private Boolean hideTerminal = false;
        // Execution information
        private String[] inputFile; // list of input files
        private String resultFolder; // folder where the result will be outputted

        // Constructor
        public Brain() 
        {
            Status = 0;
            progressStorage = new ProgressStorage();
        }

        // give ffmpeglocation and convert output folder
        public int addSeriousSettings(String ffmpegLocationIn, String convertOutputLocationIn)
        {
            if(Status == 0)
            {
                ffmpegLocation = ffmpegLocationIn;
                convertOutputLocation = convertOutputLocationIn;
                return 0;
            } else
            {
                return 1;
            }
        }

        // give object size and movement window
        public int addLessSeriousSettings(int objectSizeIn, int movementWindowIn)
        {
            objectSize = objectSizeIn;
            movementWindow = movementWindowIn;
            return 0;
        }

        // Methods to change settings
        public void doHideTerminal(Boolean input)
        {
            hideTerminal = input;
        }

        public void updateThreshold(int input)
        {
            threshold = input;
        }

        public void setResultFolder(String input)
        {
            resultFolder = input;
        }

        public String getConvertOutput()
        {
            return convertOutputLocation;
        }

        public String getResultFolder()
        {
            return resultFolder;
        }

        public void setRectangle(Rectangle Rectin)
        {
            Rect = Rectin;
        }

        // Get new Files
        public int newFiles(String[] input)
        {
            if (Status == 0)
            {
                inputFile = input;
                return 0;
            }
            else
            {
                return 1; // dont accept, asks for reset first
            }
        }

        // Function to get a preview image (used when 'load' button is pressed)
        public Bitmap getPreview()
        {
            Bitmap result;
            if (Status == 0) // no files converted
            {
                result = null;
            } else
            {
                Processer Temp = new Processer(inputFile[0], convertOutputLocation, threshold, objectSize, movementWindow, Rect);
                result = Temp.getPreview();
            }
            return result;
        }

        // Function to get the length of all files in hours, minutes, seconds (2 fps used for conversion)
        public String[] getVideoLengths()
        {
            String[] result;
            if (Status == 0) // no files converted
            {
                result = null;
            }
            else
            {
                result = new String[inputFile.Length];
                Processer Temp;
                for (int i = 0; i < inputFile.Length; i++)
                {
                    Temp = new Processer(inputFile[i], convertOutputLocation, threshold, objectSize, movementWindow, Rect);
                    int Length = Temp.getLength();
                    result[i] = convertLength(Length);
                }
            }
            return result;
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

        private String getTxtRepresenation()
        {
            String result = "";
            for (int i = 0; i < trackerlist.Length; i++)
            {
                result += "File nr " + i + Environment.NewLine;
                List<int[]> framesList = trackerlist[i].getList();
                // add output from this tracker
                if (framesList == null)
                {
                    result += "No movement in this file" + Environment.NewLine;
                }
                else
                {
                    int nrofelements = framesList.Count;
                    for (int j = 0; j < nrofelements; j++)
                    {
                        result += "Part " + (j + 1) + ": from ";
                        result += convertLength(framesList[j][0]);
                        result += " to " + convertLength(framesList[j][1]);
                        result += Environment.NewLine;
                    }
                }
                result += Environment.NewLine;
            }
            return result;
        }

        // Discard the temporary converted files
        public void discardConvertedFiles()
        {
            int i = convertorList.Length;
            for (int j = 0; j < i; j++)
            {
                convertorList[j].clearFile();
            }
        } 

        // Write to txt file, return 0:succes, 1:wrong place, 2:no output folder, 3:other reason
        public int writeToTxt()
        {
            if(Status != 2)
            {
                return 1; // wrong place
            } else if(resultFolder == null)
            {
                return 2; // no output folder
            } else
            {
                // write to txt. Filename is date and time
                String time = DateTime.Now.Day.ToString("00") + DateTime.Now.Month.ToString("00") + DateTime.Now.Year + "_" + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00");
                String path = resultFolder + "\\output_" + Path.GetFileNameWithoutExtension(inputFile[0]) + "_" + time + ".txt";
                String towrite = getTxtRepresenation();
                System.IO.File.WriteAllText(path, towrite);
            }
            return 0;
        }

        // Write the video file, return 0:succes, 1:wrong place, 2:no output folder, 3:no movement in file, 4:other reason
        public int writeToVideo()
        {
            if (Status != 2)
            {
                return 1; // wrong place
            }
            else if (resultFolder == null)
            {
                return 2; // no output folder
            }
            else
            {
                WriteToVideo Temp = new WriteToVideo(inputFile, trackerlist, resultFolder, ffmpegLocation, hideTerminal);
                int result = Temp.WriteVideo(progressStorage);
                if(result == 0)
                {
                    return 0; // succes
                } else if(result == 2)
                {
                    return 3; // no movement in file
                } else
                {
                    return 4; // the command failed
                }
            }
        }

        // Convert all files, return 0:succes, 1:already converted, 2:wrong settings(no input files), 3:other reason(something wrong with videos)
        public int doConvert()
        {
            if(Status == 0)
            {
                if(inputFile != null)
                {
                    // Code here:
                    progressStorage.setTotFiles(inputFile.Length); // set total files in progressStorage
                    convertorList = new Convertor[inputFile.Length];
                    Convertor Temp;
                    for(int i = 0; i < inputFile.Length; i++)
                    {
                        Temp = new Convertor(inputFile[i],convertOutputLocation, ffmpegLocation);
                        int okay = Temp.doConvert(hideTerminal);
                        convertorList[i] = Temp;
                        if (okay == 1)
                        {
                            // remove all temporary files:
                            for(int j = 0; j < i; j++)
                            {
                                convertorList[j].clearFile();
                            }
                            // clear progress:
                            progressStorage.reset();
                            return 3;
                        }
                        progressStorage.conversionFileFinished(); // a file finished in progressStorage
                    }
                    Status = 1;
                    return 0;
                } else
                {
                    return 2;
                }
            } else
            {
                return 1;
            }
        }

        // Process all files, return 0:succes, 1:convert first, 2:no area selected, 3:other reason, 4:already processed
        public int doProcess()
        {
            if(Status != 0)
            {
                if(Rect.Width == 0 || Rect.Height == 0)
                {
                    return 2;
                } else
                {
                    // Code here:
                    progressStorage.resetProcessing();
                    trackerlist = new Tracker[inputFile.Length];
                    Processer Temp;
                    for (int i = 0; i < inputFile.Length; i++)
                    {
                        Temp = new Processer(inputFile[i], convertOutputLocation, threshold, objectSize, movementWindow, Rect);
                        progressStorage.newProcessingFile(Temp.getLength()); // a file started in progressStorage
                        int okay = Temp.doProcess(progressStorage);
                        if (okay == 1)
                        {
                            progressStorage.resetProcessing();
                            return 3;                          
                        }
                        progressStorage.processingFileFinished(); // the file finished in progressStorage
                        trackerlist[i] = Temp.getTracker();
                    }
                    if(Status == 2)
                    {
                        return 4;
                    } else
                    {
                        Status = 2;
                        return 0;
                    }                   
                }
            } else
            {
                return 1;
            }
        }
    }
}
