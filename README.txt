Authors: Ethan Andrews and Mary Garfield
Date: December 6th , 2023
University of Utah
Summary:  Server is the second half of a total Snake game. This project allows connections from multiple game clients and 
manages the rules of the Snake game. Each player controls a snake that collects powerups to grow its length. Each snake is
comprised of vertices that define axis-aligned segments. The player controls the snake by typing ‘w’ for up, ‘d’ for left, ‘a’ 
for right, and ‘s’ for down into the text box to the right of the about button. The world contains walls and powerups, and the
player’s snake dies when it hits a wall, another snake, or collides with itself. When the snake dies it is respawned in a random 
location that does not collide with any other object in the world. When a snake collides with a power up the snake will either grow, 
or it will grow and become faster based on the game mode in the setting file which is <SpeedMode>. Each collision with a powerup
increments the player's score. Each snake is always moving, and the snake's body follows along the same path as the segments 
in front of it. When the snake reaches the world’s edge, it is teleported to the opposite edge and continues moving in the 
same orientation. New players can be added into the world at any time and existing players can respawn at any time. When a 
client disconnects the server sends the ‘died’ version of the snake and indicates to the remaining clients that it has disconnected. 

Design Decisions:
-For our extra game mode, we added functionality that makes the snake get faster when a powerup is collected AND the snake’s body grows. This game mode is enabled based on the game mode in the settings file. When <SpeedMode> is true it is in the special game mode, when <SpeedMode> is false, it is in basic game mode. 
--Instead of using GameController we created a separate controller class for the Server. This is because the ServerController and 
it would have gotten convoluted if placed into the same file. 

--We also created a separate Model file called World for the Server. This is because the structure of the Server Model 
would have been very different from the structure of the Client Model. But they do use the same base classes for the objects, 
such as Snake, Wall, and Powerup.

--The View is just the entry point. This is because the Server does not have a set GUI just the console. The controller is responsible 
for printing things to the console. It didn’t seem logical to create a separate View class for exclusively printing things to the 
console. 


Issues:
-Sometimes the program freezes due to information transfer but once inputs are sent it stops freezing.  
--On rare occasions, the AI snakes appear to jitter, but the TA said this is because of lag, and to not worry about it.


November 30th, 2023: 
--Created the necessary files: GameSettings.cs, Server.cs, ServerController.cs, and World.cs. 
--Added very basic functionality.  

December 1st, 2023:
--Added majority of the functionality for networking specifications in ServerController.
--Added majority of the respawn location mechanics that allow snakes to respawn in a random location without colliding with any other world objects. 
--Generated snake body, snake segments, and snake direction. 

December 2nd, 2023:
--Finished creation of snake in unique location mechanics.
--Fixed issues with XML deserialization
--Added functionality for snake movement based on the inputted player commands. 

December 3rd, 2023:
--Completed major restructuring of our program. 
--At this point the snake moves, and the server sends updates.  
--Fixed minor bugs. 
--Added collision logic for all collisions. 
--Fixed some race conditions. 

December 6th, 2023: 
--Restructured the project again, eliminated duplicated methods, and isolated locks.
--Fixed some MVC design issues. 
--Finished the extra game mode, which is <SpeedMode>, makes the snake grow and get faster.


