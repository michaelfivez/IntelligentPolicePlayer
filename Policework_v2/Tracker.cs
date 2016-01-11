using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Policework_v2
{
    // Keeps track of which frames have movement in them
    class Tracker
    {
        int distanceToReset = 10; // 5 seconds
        List<int[]> framesList;
        int[] currElement;
        int started; // keeps track if a first entry has been made or not

        public Tracker(int movementWindow)
        {
            distanceToReset = movementWindow * 2; // in fps
            framesList = new List<int[]>();
            currElement = new int[2];
            started = 0;
        }

        public void addFrame(int framenumber)
        {
            if (started == 0)
            {
                started = 1;
                if(distanceToReset > framenumber)
                {
                    currElement[0] = 0;
                }
                else
                {
                    currElement[0] = framenumber - distanceToReset;
                }
                currElement[1] = framenumber;
            }
            else
            {
                if (currElement[1] + 2*distanceToReset >= framenumber)
                {
                    currElement[1] = framenumber; // continue on previous array
                }
                else
                {
                    currElement[1] += distanceToReset; // move end x seconds forward
                    framesList.Add(currElement);
                    currElement = new int[2];
                    currElement[0] = framenumber - distanceToReset; // move beginning x seconds backward
                    currElement[1] = framenumber;
                }
            }
        }

        public void closeList(int finalframe)
        {
            if (started == 0)
            {
                // no movement in this file
                framesList = null;
                return;
            }
            if (currElement[1] + distanceToReset > finalframe)
            {
                currElement[1] = finalframe;
            }
            else
            {
                currElement[1] += distanceToReset;
            }
            framesList.Add(currElement);
        }

        public List<int[]> getList()
        {
            // return list
            return framesList;
        }
    }
}
