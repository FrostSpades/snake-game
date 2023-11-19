Authors: Ethan Andrews and Mary Garfield
Date: November 24th, 2023
University of Utah

Summary: SnakeClient is one-half of a total Snake game, in which each player controls a snake that collects powerups to grow its length. The player controls the snake by typing ‘w’ for up, ‘d’ for left, ‘a’ for right, and ‘s’ 
for down into the text box to the right of the about button. The player begins the game by entering the IP address of the desired server, and by entering the name of their snake, and then by pressing connect. If the connection 
fails, the player is presented with a warning and can change the typed-in IP address. Once connected to the server the connect button is disabled. The world contains walls and powerups, and the player’s snake dies when it hits 
a wall, another snake, or collides with itself. Snake bones are shown upon a player's death and stay in the world until the snake respawns at a random location. 

Design Decisions:
-We added references from the Controller to the View, a Model reference to the Controller, and a Model reference to the View. The Controller communicates to the View using events.
-Instead of explosions, we added snake bones that appear upon a player's death, these bones lay on the screen in the same orientation as the snake at the point of death, and disappear once the snake respawns. 
-The snake colors are determined by the snake ID’s first digit, there are 10 total colors each corresponding to a different digit. The snake's body is rounded at the head and tail.
-The powerups display as red circles. 

Issues:
-Occasionally due to a MAUI bug the application will open with a black screen, the application will need to be restarted and then should work. 
-When localhost is entered as the server address and there is no server open on the computer the client will crash, however, this is the same behavior as the provided client. 
-When the server is closed while the client is open we designed the application to shut down, which is the same behavior as the provided client. 

November 16th, 2023:
Started working on the SnakeClient project. 
-	Created our projects and classes.
-	Completed the “handshake” process. 
-	Created the methods for View, which is the SnakeClient project with WorldPanel and MainPage classes.
-	Created the methods for Controller--our GameController class--and added basic networking behavior. 

November 17th, 2023:
-	Completed some of the functionality for View (SnakeClient) and Model.
-	Added functionality for the Walls. 
-	Added necessary references and delegates.
-	Fixed a bug where the walls were drawn with space in between each segment. 

November 18th, 2023:
-	Completed most of the basic functionality for the entire project, including control commands, SocketState error handling, and changed our implementation for walls, snakes, and powerups from a List to a Dictionary.
-	Completed serialization process for a snake. 
-	Drew the world. Added DisplayAlerts for connection errors. 
-	Completed and drew our snake and powerup implementations.
-	Encountered some bugs where the snake would jitter back and forth on the screen and the first control command sent would not register. 

November 19th, 2023:
-	Completed the majority of the functionality, at this point, there are 8 different colors for snakes, and names and scores are displayed on the head of the snake. 
-	Added custom png’s into our file to display snake bones upon death and embedded those images. 
-	Distinguished between death and disconnection, added functionality so bones only appear when the player has actually died, not when they disconnect. 
-	Finished all the functionality and completed the death process so the body bones and the head bone line up correctly based on orientation. 
-	Finished code comments. 
