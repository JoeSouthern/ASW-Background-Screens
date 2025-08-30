# ASW Background Screens

![Main screen of app](docs/Screenshot Main.png)

Allows separate background images for each monitor in Windows (Up to 4).  
Supports:
- Different wallpapers per monitor (Windows 10/11)
- Automatic wallpaper rotation per monitor
- Optional recursive subfolder scanning
- User-customizable monitor order
- Saving reference(s) to favorite images for later printing or processing

## Features
- Set unique wallpaper style (Fill, Fit, Span, etc.)
- Timer-based automatic rotation
- Right-click or press F9 to save a reference to a favorite image
- Config file to store preferences
- Works on multi-monitor setups

## How to Use
1. Launch the app.
2. Choose an image folder for each monitor.
3. Click **Start** to begin wallpaper rotation.
4. Use **Stop** to pause rotation.
5. Right-click anywhere or press **F9** to save an image reference for printing.

## Other operational information
- There is a 'Required' MyConfig.cfg file in the exe folder that holds 
	-- Screen directories to us
	-- Duration in between changes
	-- Font name and size
	-- Debug mode -- which outputs history list of files that have been shown.
	-- Saves form locations
	-- Saves your screen order
	-- Preference on including Subfolders

-- There is a print_later.csv file in the exe folder that holds
	-- References to files you've marked via an F9. 
	-- This file won't exist if you haven't marked any files yet.
	-- This is a continuous file -- added items are appended.
![Sample Save Screen](docs/Screenshot Sample Save data.png)
![Sample Save Screen Part 2](docs/Screenshot Save for Print Step 2.png)
![Sample Save Screen Data Saved](docs/Screenshot Sample Save data.png)

-- There is a DebugData.csv file in the exe folder that holds
	-- References to all files selected in each rotation. 
	-- This file won't exist if you haven't marked any files yet.
	-- This is a continuous file -- added items are appended.
	-- Basically it's just show what files it has been picking.

-- When picking folders 
	-- If the Include Subdirectories is checked 
		-- then it will include all subfolders under the directory you picked.
		-- This program will probably support up to thousands of files 
			in its list of available choices for each monitor. 
			Hundreds of thousands -- it might need to be refined.
	-- If not checked it is limited to the folder you selected.  
	-- If you pick a folder with no available files then it will alert you.
	-- It does recognize .heic files as valid image files. 
		You can easially add support for .heic files in Windows natively.
		Go to the Windows store and install the free HEIF image extension package.
		I probably should have made that more configurable (:
	
-- Making changes. 
	-- If you want to change the Duration time just change it.
	-- If you change the My Screen Order the change is immediate.
		The My Screen Order is to help you identify monitors.
		For example, Windows has mine as 1,2,3.  But they are 
		arranged by me as 2,1,3. That's all this section is. 
		An attempt to keep your sanity.
	-- If you want to change directories for any monitors 
		-- Press Stop Button first. Make Changes. Press Start.	
	-- If you want to change it to include subdirectories
		-- Press Stop Button first. 
		-- Check the Include Subfolders check box. 
		-- Click the Mon button for each monitor and 
			change your directory if needed.
			But even if unchanged -- open it 
			and then accept it so the system 
			will recognize the change to include subdirectories
			and your file count will change.
		-- Press Start.


## Requirements
- Windows 10/11
- .NET Framework 4.72 +
- A desire to see more than one image on your monitors.

## License
MIT License © 2025 Joe Poff
