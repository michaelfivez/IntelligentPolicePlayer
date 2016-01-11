## Description ##
This program detects movement in camera footage. It works on Windows and needs Microsoft NET4.0 to run.

## Legal ##
This program is created by Michael Fivez in October - January 2015.
It is licensed under GPL-3.0 (http://www.gnu.org/licenses/gpl.html).
This means you are free to do whatever you want with it, but if you use it in your work, you should grant the user the same freedoms.
Included libraries:
- ffmpeg (https://www.ffmpeg.org/)
  licensed mainly under LGPL-2.1, with parts GPL-2.0
- Aforge (http://www.aforgenet.com/)
  licensed under LGPL-3.0 with parts GPL-3.0

## Installation ##
- Download the latest [release](https://github.com/michaelfivez/IntelligentPolicePlayer/releases)
- Unpack the zip to any location, and run Policework_v2 application (create a shortcut if you want easier acces)
- The program needs to acces certain .dll and xml files in the same folder as the .exe, so don't move any files
- ffmpeg: if the location of ffmpeg is not moved in the application, the program should be able to access it
	if there is some problem set the absolute path in the settings manually:
	It needs to refer to 'directory_ffmpeg_is_placed_in'/ffmpeg/bin/ffmpeg
	You can always revert to the default path by pressing 'default' in the settings
- Launch Policework_v2 (the 44KB Application file)

## Workflow of the program ##
- Select the files from the 'open file' or 'open multiple file' menu.
 - Each time you open new file, it will only use those (see the information printed in the console).
- Then press convert and the files will convert to the folder specified in settings (convertor output).
 - All files have to be video files compatible with ffmpeg. If one is not a video file, the conversion will fail (see the information printed in the console). 
 - It supports almost all codecs (if it doesn't it's more likely there is a mistake when copying the ffmpeg, then that ffmpeg doesn't know the codec).
- Afterwards press load and a snapshot taken from the videofiles should appear.
- On this image select a area where you want to see the movement.
 - If there is a time stamp in the video, don't select this in the area, because it's changing will be detected as movement.
- Use the threshold slider to select a sensitivity, the lower the value, the more sensitive the program will be. 
 - The standard threshold of 20 should work on most videos.
- Press process and the converted videos will be analyzed for movement.
- Now select a folder with the 'Choose save location' menu.
 - The output will be generated in this folder. The filename is called output + the date and time.
- You can get the output with the 'get .txt' or 'get video' buttons.
 - The *get txt* button will print all the times between which there is movement for every file in a txt file.
 - The *get video* button will generate a .mp4 video containing only the periods when there is movement (one output file).
- You can use reset to reset the state of the program, and load in a new sequence of files.
  - The converted files will be removed.

## Notes ##
- The console prints helpful information when you make a mistake or when an action is executed.
- A terminal opens minimized to give feedback when converting long files (which can take 2 minutes per hour of video).
- If you accidently press this terminal when it opens and the program gets stuck, press 'escape' to have the program continue.
- The standard folder where the temporary converted files are placed, is inside the program directory. You can change this in settings.
  - If the program is closed while files are converted but not processed, these files are not automatically deleted, delete them manually to free disk space.
- If you put the output or convert-output folder in a administrator protected folder, the program will give errors.
- The order of the videos in the output txt or video dependends on how they are selected when loaded.
  - In the 'File name'-bar before pressing 'Open' it looks like: "testvid0" "testvid1" "testvid2" "testvid3".
  - When shift clicking to select files, it might put the last file in the beginning, manually copy it to the end of the sequence to use the right order.
- Don't change the threshold bar when processing is going on (see the information printed in the console)
- When converting is done, you can only load in new files after pressing 'Reset'.
  - Reset doesn't reset the output folder or any settings.
- The final video generation works a lot faster when the video files are in shorter pieces (1 file being 10 hours will be a lot slower then 20 files of 30 minutes).

## Parameters ##
### Settings for processing ###
These settings are reset each launch of the program
- **Threshold**: The lower this value, the more sensitive the program to movement (default = 20).
- **ObjectSize**: The lower this value, the more sensitive the program to movement (default = 50).
- **MovementWindow**: The amount of seconds before and after each part with movement, that will be kept in the output.

In case you want to understand the settings in more detail, the processing works as follows:
- It processes 2 frames per second of the video, in 320x240 pixel size (converting converts the video to this format).
- A changing background image is used as background, every frame 80% of the previous background is retained, and the remaining 20% comes from the new frame.
- Each frame the following happens (only on the selected pixels of the video):
  - the difference per pixel between the background and the current frame is calculated.
  - A threshold is set over the image, all diferences bigger then the defined threshold (default = 20), becomes a 1, the rest a 0.
  - The total amount of 1's is calculated. If bigger then the defined ObjectSize, this frame is seen as part of movement.
- Add *X* seconds before and after the moving frames is also registered as part of the output (*X* is defined by MovementWindow).

### Settings for folder locations ###
These settings get saved and stay when the program is closed and opened again.
- **ConvertOutputFolder**: In this folder the converted files are placed that will be used for processing (default = Root/ConvertOutput). They get deleted after the processing is done, so if you hardquit the program before this (or it crashes), you should manually delete them. Resetting with the reset button, does delete them.
- **Ffmpeg Location**: see installing notes.

### Other settings ###
By the checkmark 'hide terminal' you can choose to prevent the terminal from showing during the conversion of files, or the generation of the final video
