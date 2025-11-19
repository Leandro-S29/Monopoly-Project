# Programming Fundamentals Project - Monopoly

This project involves the implementation of a variant of the Monopoly game for the curricular units Fundamentals of Programming and Structure of Logical Thinking in Computer Engineering.

## 📋 Requirements:
- .NET 8.0 SDK or higher

## 👥 Group members of the project:
- Leandro Santos
- Guilherme Soares
- Henrique Carvalho
- Henrique Metelo

## Organization of the project files:
* **`Program.cs`**: Handles only console I/O (Input/Output) and command parsing.
* **`Game.cs`**: Acts as the main controller, managing the game state, player list, and turn logic. Includes bankruptcy and end-game logic (player elimination and statistics).
* **`Board.cs`**: Models the 7x7 board and its initialization.
* **`Player.cs`**: Stores the state of each player (both registered and currently in-game).
* **`Space.cs`**: Stores the state of each board space.
* **`Enums.cs` / `Dice.cs`**: Utility files for constants and random number generation.