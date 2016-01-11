using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Policework_v2
{
    // holds the information about the progressBar for conversion and processing
    // the max for each bar defined in the main is 100, so this gives the progress relative to 100
    // each file is 1/x'th of the progressbar and the progress within a file is updated every 200 frames (= 01:40 minuts)
    class ProgressStorage
    {
        // Accesable for gui
        public int totFiles = 0;
        public int currentFileConverting = 0;
        public int currentFileProcessing = 0;
        public int currentFileGenerating = 0; // will go one over totFiles, to show when final video is generating
        public int ConvertingProgress = 0;
        public int ProcessingProgress = 0;
        public int GeneratingProgress = 0;

        // private labels
        private int totFrames = 0;
        private int currFrame = 0;

        // Updates Gui labels, called by GUI before taking variables out
        public void updateGui()
        {
            if(totFiles != 0)
            {
                ConvertingProgress = (int)(100 * ((double)currentFileConverting / (double)totFiles));
                if (ConvertingProgress > 100) { ConvertingProgress = 100; }
                GeneratingProgress = (int)(100 * ((double)currentFileGenerating / (double)(totFiles + 1)));
                if (GeneratingProgress > 100) { GeneratingProgress = 100; }
                double onefilePart = (100.0 / (double)totFiles);
                ProcessingProgress = (int)(100 * ((double)currentFileProcessing / (double)totFiles));
                if(totFrames != 0)
                {
                    ProcessingProgress += (int)(onefilePart * ((double)currFrame / (double)totFrames));
                    if (ProcessingProgress > 100) { ProcessingProgress = 100; }
                }
            }
            else
            {
                ConvertingProgress = 0;
                ProcessingProgress = 0;
                GeneratingProgress = 0;
            }
        }

        // Called when setting a number of files (the moment you press convert)
        public void setTotFiles(int totFilesin)
        {
            totFiles = totFilesin;
        }
        
        // Reset progress (when error occurs in converting)
        public void reset()
        {
            totFiles = 0;
            currentFileConverting = 0;
            ConvertingProgress = 0;
            currentFileProcessing = 0;
            ProcessingProgress = 0;
            currentFileGenerating = 0;
            GeneratingProgress = 0;
        }

        // Reset progress on processing (when error occurs)
        public void resetProcessing()
        {
            currentFileProcessing = 0;
            ProcessingProgress = 0;
        }

        // Reset progress on generating (when error occurs or when making the video again)
        public void resetGeneration()
        {
            currentFileGenerating = 0;
            GeneratingProgress = 0;
        }

        // Increment number of conversions done
        // Called when finishing conversion on a new file
        public void conversionFileFinished()
        {
            currentFileConverting += 1;
        }

        // Increment number of generations done
        // Called when finishing generation on a new file
        public void generationFileFinished()
        {
            currentFileGenerating += 1;
        }

        // Start logging one processing 'go'
        // Called when starting processing on a new file
        public void newProcessingFile(int totFramesin)
        {
            totFrames = totFramesin;
            currFrame = 0;
        }

        // called every ~200 frames to increment the progress of processing
        public void incrementProgress(int incurrFrame)
        {
            currFrame = incurrFrame;
        }

        // Called when finished with processing the file
        public void processingFileFinished()
        {
            currentFileProcessing += 1;
        }
    }
}
