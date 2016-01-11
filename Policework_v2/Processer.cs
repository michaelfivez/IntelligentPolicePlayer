using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AForge.Video.FFMPEG;
using AForge.Imaging.Filters;

namespace Policework_v2
{
    // finds the movement in a video file in the standard format (320x240, 2 fps, .mp4 x264 coding)
    class Processer
    {
        int threshold;
        int objectsize;
        int movementWindow;
        String file;
        private Rectangle area; // holds the area where to analyze
        private Bitmap[] imageStack; // holds currently loaded stack of bitmaps for analyzing
        private VideoFileReader reader; // the reader that extracts frames from file
        private Bitmap backgroundFrame; // holds the backgroundframe for processing
        private int currentFrame; // holds the number of the current frame that is analyzed
        private Tracker tracker; // keeps track of which frames have movement

        public Processer(String filein, String convertOutputLocation, int thresholdin, int objectSizein, int movementWindowin, Rectangle areain) // more settings later
        {
            file = convertOutputLocation + "/" + Path.GetFileNameWithoutExtension(filein) + "Converted.mp4";
            threshold = thresholdin;
            objectsize = objectSizein;
            movementWindow = movementWindowin;
            area = areain;
        }

        // Function to get one image from the file (if null returned it failed)
        public Bitmap getPreview()
        {
            Bitmap preview = null;
            VideoFileReader Temp = new VideoFileReader();
            try
            {
                Temp.Open(file);
            }
            catch
            {
                return preview;
            }
            // try if possible to extract a frame (use this one, to return for drawing)
            try
            {
                int frametotake = (int)Temp.FrameCount;
                for (int i = 0; i < (frametotake - 1) && i < 10; i++)
                {
                    Temp.ReadVideoFrame();
                }
                preview = Temp.ReadVideoFrame();
            }
            catch
            {
                return preview;
            }
            return preview;
        }

        // Function to get the length (in frames) from the file (if 0 returned it failed)
        public int getLength()
        {
            VideoFileReader Temp = new VideoFileReader();
            int result = 0;
            try
            {
                Temp.Open(file);
            }
            catch
            {
                return result;
            }
            // try if possible to extract a frame (use this one, to return for drawing)
            try
            {
                result = (int)Temp.FrameCount;
            }
            catch
            {
                return result;
            }
            return result;
        }

        // Function returns the tracker for brain, so he can store it
        public Tracker getTracker()
        {
            return tracker;
        }


        // Function to process the file
        public int doProcess(ProgressStorage progressStorage)
        {
            currentFrame = 0;
            reader = new VideoFileReader();
            try
            {
                reader.Open(file);
            }
            catch
            {
                return 1;
            }
            int frameCount = (int)reader.FrameCount;
            tracker = new Tracker(movementWindow);
            int nrofiterations = frameCount / 200;
            int i = 0;
            int nrofframes = 0;
            while (i < nrofiterations)
            {
                nrofframes = 200;
                loadinPart(nrofframes, area);
                if(i == 0) // store first frame as background frame
                {
                    backgroundFrame = imageStack[0].Clone(new Rectangle(0, 0, imageStack[0].Width, imageStack[0].Height), imageStack[0].PixelFormat);
                }
                processFilePart();
                progressStorage.incrementProgress(currentFrame);
                i++;
            }
            nrofframes = frameCount - currentFrame;
            loadinPart(nrofframes, area);
            if (nrofiterations == 0) // store first frame as background frame if nrofiterations is 0
            {
                backgroundFrame = imageStack[0].Clone(new Rectangle(0, 0, imageStack[0].Width, imageStack[0].Height), imageStack[0].PixelFormat);
            }
            processFilePart();
            progressStorage.incrementProgress(currentFrame);
            tracker.closeList(currentFrame);
            reader.Close(); // close the file
            return 0;
        }

        // Process max 200 frames (5 min) in 320x240 resolution. So 76KB memory per frame (grayscale). 1200 frames is max 93 MB of RAM (normally less because of area)
        private void processFilePart()
        {
            int nrofframes = imageStack.Length;
            int i;
            int sum;
            // create filters
            Morph morphFilter = new Morph(); // filter for adapting background
            morphFilter.SourcePercent = 0.8;
            Difference differenceFilter = new Difference(); // filter for subtracting two frames
            Threshold thresholdFilter = new Threshold(); // filter for thresholding
            FiltersSequence filters = new FiltersSequence(); // all filters in one
            filters.Add(morphFilter);
            filters.Add(differenceFilter);
            filters.Add(thresholdFilter);
            thresholdFilter.ThresholdValue = threshold;
            // Process here
            for (i = 0; i < nrofframes; i++)
            {
                // move background towards current frame
                morphFilter.OverlayImage = imageStack[i];
                Bitmap Temp = morphFilter.Apply(backgroundFrame);
                backgroundFrame = Temp.Clone(new Rectangle(0, 0, Temp.Width, Temp.Height), Temp.PixelFormat);
                Temp.Dispose();
                // apply rest of the filters
                differenceFilter.OverlayImage = imageStack[i];
                Bitmap Temp2 = filters.Apply(backgroundFrame);
                sum = 0;
                // Calculate sum of white pixels
                for (int j = 0; j < Temp2.Width; j++)
                {
                    for (int k = 0; k < Temp2.Height; k++)
                    {
                        if (Temp2.GetPixel(j, k) != Color.FromArgb(255, 0, 0, 0))
                        {
                            sum += 1;
                        }
                    }
                }
                Temp2.Dispose();
                if (sum > objectsize)
                {
                    tracker.addFrame(currentFrame);
                }
                currentFrame += 1;
            }
            // Discard Array
            for (i = 0; i < nrofframes; i++)
            {
                imageStack[i].Dispose();
            }
        }

        // Load in nrofframes, only the selected area, and it is at 4 fps by the preprocessing
        private void loadinPart(int nrofframes, Rectangle area)
        {
            imageStack = new Bitmap[nrofframes];
            Grayscale filter = new Grayscale(0.2125, 0.7154, 0.0721);
            for (int i = 0; i < nrofframes; i++)
            {
                // Read frame
                Bitmap Temp = reader.ReadVideoFrame();
                // Crop frame
                Bitmap Temp2 = new Bitmap(area.Width, area.Height);
                using (Graphics gph = Graphics.FromImage(Temp2))
                {
                    gph.DrawImage(Temp, new Rectangle(0, 0, Temp2.Width, Temp2.Height), area, GraphicsUnit.Pixel);
                }
                Temp.Dispose();
                // Convert to grayscale
                Bitmap Temp3 = filter.Apply(Temp2);
                Temp2.Dispose();
                imageStack[i] = Temp3;
            }
        }
    }
}
