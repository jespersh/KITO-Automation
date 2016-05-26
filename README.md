# KITO-Automation
Simple zip encryption automation tool
![alt text](http://download.kitopen.com/images/zip-encryption-automation.png "Example")

It works by being given a list of Zip files to pass through the xor array for obscufation.
We read in chunks of 65536bytes (64KB) for maximum speed.

## Usage
1. Zip your files and select input zip files,
2. Select an output folder.
3. Set some fitting File Extension or leave it alone.
4. Input your xor key array from GameConfig.h in ENCRYPTION_XORS
  Current line 15 in GameConfig.h Example:

  Code:
  ```
      #define ENCRYPTION_XORS { 11, 22, 33, 144, 57, 63, 69, 96, 240, 128 }
  ```
  
  The numbers
  
  Code:
  ```
      11, 22, 33, 144, 57, 63, 69, 96, 240, 128
  ```
  
  are what you'll add to the program.

5. DO IT button to do the thing.
6. To get the game to load the resources add something like this to resources.cfg

  Code:
  ```
      KITOZIP=media/materials.kzip
  ```



Check and uncheck in the Do column of the table if you wish to skip a file. Altho this is hella fast.
