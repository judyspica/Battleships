namespace Aufgabe1_Judy_Kardouh
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            int originalWidth = Console.WindowWidth;
            int originalHeight = Console.WindowHeight;
            CheckConsoleSize();

            int activeIndex = 0;
            int col = 10;
            int row = 10;
            bool manualSecetionModeEnabled = true;

            string[] commands = new string[6] { " Show help", " Change board width", " Change board height", " Toggle set up mode ", " Start game", " Exit game" };
            
            if (row < 10 || row > 20)
            {
                throw new ArgumentOutOfRangeException(nameof(row), "The specified value must be between 10 and 20!");
            }

            if (activeIndex < 0 || activeIndex > commands.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(activeIndex), "The specified value must not be smallwe than zero or greater than the number of commands!");
            }

            ConsoleKey input;
            do
            {
                CheckConsoleSize();
                DrawGameHeader();
                ShowGameModeInMenu(manualSecetionModeEnabled, commands);
                PrintMenu(commands, activeIndex);

                input = Console.ReadKey().Key;

                switch (input)
                {
                    case ConsoleKey.UpArrow: 
                        MoveUpInMenu(ref activeIndex, commands); 
                        break;
                    case ConsoleKey.DownArrow: 
                        MoveDownInMenu(ref activeIndex, commands); 
                        break;
                    case ConsoleKey.Enter: 
                        ExecuteCommand(activeIndex, ref row, ref col, ref manualSecetionModeEnabled, originalHeight, originalWidth); 
                        break;
                }
            } 
            while (true);
        }

        public static void ShowGameModeInMenu(bool manualSecetionModeEnabled, string[] commands)
        {
            if (manualSecetionModeEnabled)
            {
                commands[3] = " Toggle set up mode: now Manual";
            }
            else
            {
                commands[3] = " Toggle set up mode: now Automatic";
            }
        }

        public static void DrawGameHeader()
        {
            CheckConsoleSize();
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("                ____________/___                    ");
            Console.WriteLine("                //        \\   \\                   ^^");
            Console.WriteLine("       ============================            ^^   ");
            Console.WriteLine("       \\    FHWN   o o o o         |                ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(" ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  \n\n");
            Console.ResetColor();
            Console.WriteLine("BATTLESHIPS\n___________\n\n Please pick a command! \n");
        }

        public static void ExecuteCommand(int activeIndex, ref int col, ref int row, ref bool manualSecetionModeEnabled, int originalHeight, int originalWidth)
        {
            switch (activeIndex)
            {
                case 0: 
                    ShowHelp(); 
                    break;
                case 1: 
                    ChangeBoardWidth(ref col); 
                    break;
                case 2: 
                    ChangeBoardHeight(ref row); 
                    break;
                case 3: ToggleSetupMode(ref manualSecetionModeEnabled); 
                    break;
                case 4: 
                    StartGame(ref row, ref col, ref manualSecetionModeEnabled);
                    break;
                case 5: AskIfPlayerWantsToExit(originalHeight, originalWidth); 
                    break;
            }
        }

        public static void StartGame(ref int row, ref int col, ref bool manualSecetionModeEnabled)
        {
            CheckConsoleSize();
            Console.Clear();
            char[,] humanPlayerGrid = CreateGrid(row, col);
            char[,] computerPlayersGrid = CreateGrid(row, col);

            AddGameUnits(row, col, manualSecetionModeEnabled, humanPlayerGrid, computerPlayersGrid);
            Random rand = new Random();
            //// this random will take an integer value between greater than or equal to zero and max value, with mudolo 2 it will see if its a single or double number. This determines who starts shooting first.
            bool computerFirstToStart = rand.Next() % 2 == 0;

            char[,] playerFiredPositions = new char[row, col];
            bool isGameRunning = true;
            do
            {
                // number of chars in each cell, 4 in our case (| + SPACE + CELL + SPACE)  
                // + Extra cell sperator in the end of the line  
                // + distance we want between the 2 grids = 15
                int gridStartLocationOffset = (4 * col) + 1 + 15;

                DrawGrid(row, col, humanPlayerGrid, 0, "P   L   A   Y   E   R");
                DrawGrid(row, col, computerPlayersGrid, gridStartLocationOffset, "O   P   P   O   N   E   N   T");

                if (computerFirstToStart)
                {
                    FireAutomaticallyAtPlayer(row, col, humanPlayerGrid, playerFiredPositions);
                    FireMauallyAtEnemy(row, col, computerPlayersGrid, gridStartLocationOffset);
                }
                else
                {
                    FireMauallyAtEnemy(row, col, computerPlayersGrid, gridStartLocationOffset);
                    FireAutomaticallyAtPlayer(row, col, humanPlayerGrid, playerFiredPositions);
                }

                isGameRunning = GridHasUnHitShipCells(row, col, humanPlayerGrid) && GridHasUnHitShipCells(row, col, computerPlayersGrid);
            } 
            while (isGameRunning);

            CheckWhoWon(row, col, humanPlayerGrid, computerPlayersGrid);
        }

        public static void CheckWhoWon(int row, int col, char[,] humanPlayerGrid, char[,] computerPlayersGrid)
        {
            CheckConsoleSize();
            bool playerWon = GridHasUnHitShipCells(row, col, humanPlayerGrid);
            bool enemyWon = GridHasUnHitShipCells(row, col, computerPlayersGrid);

            if (playerWon)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("WINNER ALERT!\n\nyou my master have won!! arrr");
                Console.ResetColor();
                Console.ReadKey();
            }

            if (enemyWon)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("SAD WINNER ALERT\n\ncomputer won, better luck next time. arrrr :/");
                Console.ResetColor();
                Console.ReadKey();
            }
        }

        public static void AddGameUnits(int row, int col, bool manualSecetionModeEnabled, char[,] humanPlayerGrid, char[,] computerPlayersGrid)
        {
            if (manualSecetionModeEnabled)
            {
                PlacePlayerUnits(row, col, humanPlayerGrid, manualSecetionModeEnabled);
            }
            else
            {
                SetUnitsAutomatically(row, col, humanPlayerGrid);
            }

            SetUnitsAutomatically(row, col, computerPlayersGrid);
        }

        public static bool GridHasUnHitShipCells(int row, int col, char[,] grid)
        {
            bool stillHasUnHitShipCells = false;

            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < col; y++)
                {
                    stillHasUnHitShipCells = stillHasUnHitShipCells || grid[x, y] == 'S';
                }
            }

            return stillHasUnHitShipCells;
        }

        public static void FireAutomaticallyAtPlayer(int row, int col, char[,] humanPlayerGrid, char[,] firedAtPositions)
        {
            Random random = new Random();

            int randomSelectedX = -1;
            int randomSelectedY = -1;

            bool ongoingTurn = true;
            while (ongoingTurn)
            {
                CheckIfGridIsFull(row, col, firedAtPositions);
                if (ongoingTurn && randomSelectedX != -1 && randomSelectedY != -1)
                {
                    randomSelectedX = random.Next(randomSelectedX - 1, randomSelectedX + 1);
                    if (randomSelectedX < 0)
                    {
                        randomSelectedX = 0;
                    }

                    if (randomSelectedX > row)
                    {
                        randomSelectedX = row;
                    }

                    randomSelectedY = random.Next(randomSelectedY - 1, randomSelectedY + 1);
                    if (randomSelectedY < 0)
                    {
                        randomSelectedY = 0;
                    }

                    if (randomSelectedY > col)
                    {
                        randomSelectedY = col;
                    }
                }
                else
                {
                    CheckIfGridIsFull(row, col, firedAtPositions);
                    randomSelectedX = random.Next(0, row);
                    randomSelectedY = random.Next(0, col);
                }

                if (firedAtPositions[randomSelectedX, randomSelectedY] == 'X')
                {
                    CheckIfGridIsFull(row, col, firedAtPositions);
                    randomSelectedX = -1;
                    randomSelectedY = -1;
                    ongoingTurn = true;
                }
                else
                {
                    CheckIfGridIsFull(row, col, firedAtPositions);
                    ongoingTurn = UpdateHitPosition(randomSelectedX, randomSelectedY, humanPlayerGrid, 0);
                    firedAtPositions[randomSelectedX, randomSelectedY] = 'X';
                }
            }
        }

        public static void CheckIfGridIsFull(int row, int col, char[,] firedAtPositions)
        {
            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < col; y++)
                {
                    if (firedAtPositions[x, y] == 'X')
                    {
                        break;
                    }
                }
            }
        }

        public static char[,] CreateGrid(int row, int col)
        {
            char[,] grid = new char[row, col];
            InitGrid(row, col, grid);

            return grid;
        }

        public static void SetUnitsAutomatically(int row, int col, char[,] computerPlayersGridgrid)
        {
            //// Tried to put bigger ships first, because other wise the program might not find places for the last pieces and it will keep trying to find a place.
            PlaceBattleCruisers(row, col, computerPlayersGridgrid, false);
            PlaceCruisers(row, col, computerPlayersGridgrid, false);
            PlaceDestroyers(row, col, computerPlayersGridgrid, false);
            PlaceSubmarines(row, col, computerPlayersGridgrid, false);
        }

        public static void PlacePlayerUnits(int row, int col, char[,] grid, bool manualSecetionModeEnabled)
        {
            PlaceSubmarines(row, col, grid, manualSecetionModeEnabled);
            PlaceCruisers(row, col, grid, manualSecetionModeEnabled);
            PlaceDestroyers(row, col, grid, manualSecetionModeEnabled);
            PlaceBattleCruisers(row, col, grid, manualSecetionModeEnabled);
        }

        public static void PlaceSubmarines(int row, int col, char[,] grid, bool manualSecetionModeEnabled)
        {
            int submarineCount = 5;
            int submarineSize = 2;
            for (int index = 0; index < submarineCount; index++)
            {
                if (manualSecetionModeEnabled)
                {
                    PlaceShipManually(row, col, grid, submarineSize);
                }
                else
                {
                    PlaceShipRandomly(row, col, grid, submarineSize);
                }
            }
        }

        public static void PlaceCruisers(int row, int col, char[,] grid, bool manualSecetionModeEnabled)
        {
            int cruiserCount = 3;
            int cruiserSize = 4;
            for (int index = 0; index < cruiserCount; index++)
            {
                if (manualSecetionModeEnabled)
                {
                    PlaceShipManually(row, col, grid, cruiserSize);
                }
                else
                {
                    PlaceShipRandomly(row, col, grid, cruiserSize);
                }
            }
        }

        public static void PlaceDestroyers(int row, int col, char[,] grid, bool manualSecetionModeEnabled)
        {
            int destroyerCount = 3;
            int destroyerSize = 3;
            for (int index = 0; index < destroyerCount; index++)
            {
                if (manualSecetionModeEnabled)
                {
                    PlaceShipManually(row, col, grid, destroyerSize);
                }
                else
                {
                    PlaceShipRandomly(row, col, grid, destroyerSize);
                }
            }
        }

        public static void PlaceBattleCruisers(int row, int col, char[,] grid, bool manualSecetionModeEnabled)
        {
            int battleCruiserCount = 1;
            int battleCruiserSize = 5;
            for (int index = 0; index < battleCruiserCount; index++)
            {
                if (manualSecetionModeEnabled)
                {
                    PlaceShipManually(row, col, grid, battleCruiserSize);
                }
                else
                {
                    PlaceShipRandomly(row, col, grid, battleCruiserSize);
                }
            }
        }

        public static void InitGrid(int row, int col, char[,] grid)
        {
            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < col; y++)
                {
                    grid[x, y] = ' ';
                }
            }
        }

        public static void PlaceShipManually(int row, int col, char[,] grid, int size)
        {
            CheckConsoleSize();
            DrawGrid(row, col, grid, 0, "PLACE YOUR SHIPS");
            bool horizontal = true;
            int userSlectedX = 0;
            int userSelectedY = 0;

            bool shipPlaced = false;
            while (!shipPlaced)
            {
                //// Bounces according to how far my cells are apart from each other.
                Console.SetCursorPosition((userSelectedY * 4) + 4, (userSlectedX * 2) + 5);
                switch (Console.ReadKey(true).Key)
                {
                    //// Does the selected n equals something? do this, otherwise do this. and break.
                    case ConsoleKey.UpArrow: 
                        userSlectedX = userSlectedX == 0 ? row - 1 : userSlectedX - 1; 
                        break;
                    case ConsoleKey.DownArrow: 
                        userSlectedX = userSlectedX == row - 1 ? 0 : userSlectedX + 1; 
                        break;
                    case ConsoleKey.LeftArrow: 
                        userSelectedY = userSelectedY == 0 ? col - 1 : userSelectedY - 1; 
                        break;
                    case ConsoleKey.RightArrow: 
                        userSelectedY = userSelectedY == col - 1 ? 0 : userSelectedY + 1; 
                        break;
                    case ConsoleKey.R: 
                        horizontal = false; 
                        break;
                    case ConsoleKey.Enter: 
                        shipPlaced = TryPlaceShip(row, col, userSlectedX, userSelectedY, grid, size, horizontal); 
                        break;
                }
            }
        }

        public static bool TryPlaceShip(int row, int col, int slectedX, int slectedY, char[,] grid, int size, bool horizontal)
        {
            if (IsShipPositionFree(row, col, slectedX, slectedY, grid, size, horizontal))
            {
                SetShipPosition(slectedX, slectedY, size, grid, horizontal);
                return true;
            }

            return false;
        }

        public static void PlaceShipRandomly(int row, int col, char[,] grid, int size)
        {
            Random random = new Random();

            bool shipPlaced = false;
            while (!shipPlaced)
            {
                int randomSelectedX = random.Next(0, row);
                int randomSelectedY = random.Next(0, col);
                //// Gets a number randomly and checks if one can devide it by two (single or double), if yes then the whole thing is true.
                bool horizontal = random.Next() % 2 == 0;

                shipPlaced = TryPlaceShip(row, col, randomSelectedX, randomSelectedY, grid, size, horizontal);
            }
        }

        public static bool IsShipPositionFree(int row, int col, int selectedX, int selectedY, char[,] grid, int size, bool horizontal)
        {
            bool isShipPositionFree = true;

            if (horizontal)
            {
                for (int index = 0; index < size; index++)
                {
                    isShipPositionFree = isShipPositionFree && row > selectedX + index && IsPositionAndAdjacentPositionsFree(row, col, selectedX + index, selectedY, grid);
                }
            }
            else
            {
                for (int index = 0; index < size; index++)
                {
                    isShipPositionFree = isShipPositionFree && col > selectedY + index && IsPositionAndAdjacentPositionsFree(row, col, selectedX, selectedY + index, grid);
                }
            }

            return isShipPositionFree;
        }

        public static bool IsPositionAndAdjacentPositionsFree(int row, int col, int selectedX, int selectedY, char[,] grid)
        {
            bool isPositionFree = true;

            isPositionFree = isPositionFree && grid[selectedX, selectedY] == ' ';

            if (selectedX >= 1)
            {
                isPositionFree = isPositionFree && grid[selectedX - 1, selectedY] == ' ';
            }

            if (selectedX < row - 1)
            {
                isPositionFree = isPositionFree && grid[selectedX + 1, selectedY] == ' ';
            }

            if (selectedY >= 1)
            {
                isPositionFree = isPositionFree && grid[selectedX, selectedY - 1] == ' ';
            }

            if (selectedY < col - 1)
            {
                isPositionFree = isPositionFree && grid[selectedX, selectedY + 1] == ' ';
            }

            return isPositionFree;
        }

        public static void SetShipPosition(int shipStartHorizontal, int shipStartVertical, int shipSize, char[,] grid, bool horizontal)
        {
            if (horizontal)
            {
                for (int index = 0; index < shipSize; index++)
                {
                    grid[shipStartHorizontal + index, shipStartVertical] = 'S';
                }
            }
            else
            {
                for (int index = 0; index < shipSize; index++)
                {
                    grid[shipStartHorizontal, shipStartVertical + index] = 'S';
                }
            }
        }

        public static void DrawGrid(int row, int col, char[,] grid, int gridStartLocationOffset, string header)
        {
            CheckConsoleSize();
            //// int lineSeperatorLength = 4 * col + 1;
            Console.SetCursorPosition(gridStartLocationOffset, 2);
            Console.WriteLine("    " + header);

            Console.SetCursorPosition(gridStartLocationOffset, 3);
            Console.WriteLine(BuildHeader(col));

            Console.SetCursorPosition(gridStartLocationOffset, 4);
            PrintHorizontelLine(col);

            char[] columnsIndexer = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            for (int x = 0; x < row; x++)
            {
                Console.CursorLeft = gridStartLocationOffset;
                Console.Write(columnsIndexer[x] + " ");
                for (int y = 0; y < col; y++)
                {
                    Console.Write($"│ ");
                    DrawCellBasedOnContent(grid[x, y], gridStartLocationOffset == 0);
                    Console.Write($" ");
                }

                Console.WriteLine("│");
                Console.CursorLeft = gridStartLocationOffset;
                PrintHorizontelLine(col);
            }
        }

        public static void DrawCellBasedOnContent(char cell, bool playerGrid)
        {
            if (playerGrid)
            {
                if (cell == ' ')
                {
                    Console.BackgroundColor = ConsoleColor.DarkCyan;
                    Console.Write(' ');
                }
                
                if (cell == 'X')
                {
                    Console.BackgroundColor = ConsoleColor.DarkCyan;
                    Console.Write('O');
                }

                if (cell == 'S')
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.Write('S');
                }
                //// Letter H stands for "hit".
                if (cell == 'H')
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write('S');
                }
            }
            else
            {
                if (cell == ' ')
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.Write(' ');
                }

                if (cell == 'X')
                {
                    Console.BackgroundColor = ConsoleColor.DarkCyan;
                    Console.Write(' ');
                }

                if (cell == 'S')
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.Write(' ');
                }

                if (cell == 'H')
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write('S');
                }
            }

            Console.ResetColor();
        }

        public static void FireMauallyAtEnemy(int row, int col, char[,] grid, int offset)
        {
            CheckConsoleSize();
            int x = 0;
            int y = 0;

            bool ongoingTurn = true;
            while (ongoingTurn)
            {
                Console.SetCursorPosition((y * 4) + 4 + offset, (x * 2) + 5);
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow: 
                        x = x == 0 ? row - 1 : x - 1; 
                        break;
                    case ConsoleKey.DownArrow: 
                        x = x == row - 1 ? 0 : x + 1; 
                        break;
                    case ConsoleKey.LeftArrow: 
                        y = y == 0 ? col - 1 : y - 1; 
                        break;
                    case ConsoleKey.RightArrow: 
                        y = y == col - 1 ? 0 : y + 1; 
                        break;
                    case ConsoleKey.Enter: 
                         ongoingTurn = UpdateHitPosition(x, y, grid, offset);
                         break;
                }
            }
        }

        public static bool UpdateHitPosition(int x, int y, char[,] grid, int offset)
        {
            CheckConsoleSize();
            Console.SetCursorPosition((y * 4) + 4 + offset, (x * 2) + 5);
            char currentState = grid[x, y];
            if (currentState == 'S' || currentState == 'H')
            {
                grid[x, y] = 'H';
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Write('S');
            }
            else
            {
                grid[x, y] = 'X';
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.Write('X');
            }

            Console.ResetColor();
            return currentState == 'S';
        }

        public static string BuildHeader(int col)
        {
            string result = "    ";

            for (int index = 0; index < col; index++)
            {
                if (index < 10)
                {
                    result += index + "   ";
                }
                else
                {
                    result += index + "  ";
                }
            }

            return result;
        }

        public static void PrintHorizontelLine(int col)
        {
            Console.Write("  ");
            for (int i = 0; i < col; i++)
            {
                Console.Write("────");
            }

            Console.WriteLine();
        }

        public static bool ToggleSetupMode(ref bool manual)
        {
            manual = !manual;
            return manual;
        }

        public static int ChangeBoardHeight(ref int boardHeight)
        {
            CheckConsoleSize();
            Console.Clear();
            Console.WriteLine("please Choose the game board height you would like to play with");
            bool parseSuccessful;
            do
            {
                parseSuccessful = int.TryParse(Console.ReadLine(), out boardHeight);
                if (boardHeight < 10 || boardHeight > 20)
                {
                    parseSuccessful = false;
                }

                if (!parseSuccessful)
                {
                    Console.WriteLine("invalid input please only pick number between 10 and 20");
                }
            } 
            while (!parseSuccessful);
            
            return boardHeight;
        }

        public static int ChangeBoardWidth(ref int boardWidth)
        {
            CheckConsoleSize();
            Console.Clear();
            Console.WriteLine("please Choose the game board width you would like to play with (you can only chose number between 10 and 20)");
            bool parseSuccessful;
            do
            {
                parseSuccessful = int.TryParse(Console.ReadLine(), out boardWidth);

                if (boardWidth < 10 || boardWidth > 20)
                {
                    parseSuccessful = false;
                }

                if (!parseSuccessful)
                {
                    Console.WriteLine("invalid input please only pick number between 10 and 20");
                }
            } 
            while (!parseSuccessful);
            return boardWidth;
        }

        public static bool AskIfPlayerWantsToExit(int originalHeight, int originalWidth)
        {
            CheckConsoleSize();
            Console.Clear();
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Are you sure you want to exit the game? Just write yes\n\nIf not just press enter to go back to main menu ;) \n\n");
                Console.ResetColor();
                string answer = Console.ReadLine().ToUpper();
                if (answer == "YES")
                {
                    Console.SetWindowSize(originalWidth, originalHeight);
                    Environment.Exit(0);
                }
                else
                {
                    return false;
                }
            }
        }

        public static void ShowHelp()
        {
            CheckConsoleSize();
            Console.Clear();
            Console.WriteLine("   =========================================\n   BATTLE SHIP GAME - HELP AND GENERAL INFO\n   ========================================= \n\n\n" +
                "The point of the game is to destroy your apponent's ships. to do so there are few things you need to know!" +
                "\nyou get to chose the height and width of the boards, you can toggle between manual and automatic playing modes\n" +
                "manual lets you place your own ships, and automatic creates and places the ships on your and the opponents board\n\n" +
                "the keys you can use to play are: the up, down ,left, right arrows, R to rotate ships horizontally or vertically\n" +
                "and last but not least: enter. to confirm the placement of the ship and then to shoot once you start playing \\m/\n\n" +
                "Game will end once one of the players hit all ships on the opponents field, first to start shooting is random! \n\nBe prepared ARRRR\n\n");
            Console.WriteLine("                                            ( )");
            Console.WriteLine("                                    ##       |");
            Console.WriteLine("                               -----##-----------");
            Console.WriteLine("                  _____     = =|                |= =       ____");
            Console.WriteLine("             = = |     |    _________________________     |    |= =");
            Console.WriteLine("       /--------------------|                       |-----------------/");
            Console.WriteLine("       \\      .  .  .  .  .                       o  o  o  o  o      /");
            Console.WriteLine("        \\___________________________________________________________/\n\n");
            Console.WriteLine("press any key to go back to main menu! and enjoy playing!");
            Console.ReadKey();
        }

        public static void MoveUpInMenu(ref int activeIndex, string[] commands)
        {
            activeIndex--;
            if (activeIndex < 0)
            {
                activeIndex = commands.Length - 1;
            }
        }

        public static void MoveDownInMenu(ref int activeIndex, string[] commands)
        {
            activeIndex++;
            if (activeIndex > commands.Length - 1)
            {
                activeIndex = 0;
            }
        }

        public static void PrintMenu(string[] commands, int activeIndex)
        {
            CheckConsoleSize();
            for (int i = 0; i < commands.Length; i++)
            {
                if (activeIndex == i)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(">");
                    Console.WriteLine(commands[i]);
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(commands[i]);
                }
            }
        }

        public static void CheckConsoleSize()
        {
            if (Console.WindowWidth < Console.LargestWindowWidth && Console.WindowHeight < Console.LargestWindowHeight)
            {
                ResetConsoleSizeToLargest();
            }
        }

        public static void ResetConsoleSizeToLargest()
        {
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
        }
    }
}
