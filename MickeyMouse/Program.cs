using System.Numerics;

namespace MickeyMouse
{
    internal class Program
    {
        const int PlayerNameMaxLength = 3;

        static int NoPlayers = -1;
        static int NoNeededToScore = 3;

        static bool GameWon = false;
        
        static List<int>[] Scores;
        static Dictionary<string, int>[] Scored;

        static string[] PlayerNames;
        static int PlayerUp = 0;

        static int CurrentHandScore = 0;

        static string[] places = new string[] { "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "D", "T", "25" };

        static void Main(string[] args)
        {
            bool Valid;
            do
            {
                try
                {
                    Console.Write("Enter how many players are playing: ");
                    string input = Console.ReadLine();

                    int.TryParse(input, out NoPlayers);

                    if (NoPlayers <= 1)
                        throw new ArgumentException();

                    Valid = true;
                }
                catch
                {
                    Valid = false;
                    Console.WriteLine("Input should be an integer more than 1");
                    Console.WriteLine();
                }
            } while (!Valid);

            Console.WriteLine();

            PlayerNames = new string[NoPlayers];
            for (int i = 0; i < NoPlayers; i++)
            {
                do
                {
                    try
                    {
                        Console.Write($"Enter the name for player {i + 1}: ");
                        string input = Console.ReadLine();

                        if (input.Length == 0 || input.Length > PlayerNameMaxLength || PlayerNames.Contains(input))
                            throw new ArgumentException();

                        PlayerNames[i] = input;

                        Valid = true;
                    }
                    catch
                    {
                        Valid = false;
                        Console.WriteLine($"Name should be between 0 and {PlayerNameMaxLength} characters in length and must be unique");
                        Console.WriteLine();
                    }
                } while (!Valid);
            }

            Scores = new List<int>[NoPlayers];
            for (int i = 0; i < NoPlayers; i++)
                Scores[i] = new List<int>() { 0 };

            Scored = new Dictionary<string, int>[NoPlayers];
            for (int i = 0; i < NoPlayers; i++)
            {
                Scored[i] = new Dictionary<string, int>()
                {
                    { "20", NoNeededToScore },
                    { "19", NoNeededToScore },
                    { "18", NoNeededToScore },
                    { "17", NoNeededToScore },
                    { "16", NoNeededToScore },
                    { "15", NoNeededToScore },
                    { "14", NoNeededToScore },
                    { "13", NoNeededToScore },
                    { "12", NoNeededToScore },
                    { "11", NoNeededToScore },
                    { "10", NoNeededToScore },
                    { "D", NoNeededToScore },
                    { "T", NoNeededToScore },
                    { "25", NoNeededToScore },
                };
            }


            while (!GameWon)
            {
                Console.Clear();

                OutputGameBoard();

                Console.WriteLine($"\n{PlayerNames[PlayerUp]} is up\n");

                for (int i = 0; i < 3; i++)
                {
                    string Hitplace = "";
                    do
                    {
                        try
                        {
                            Console.Write($"Enter dart {i + 1}: ");
                            string input = Console.ReadLine().ToUpper();

                            if (input.Length == 0)
                                input = "N0";

                            char modifier = input[0];
                            int number = int.Parse(new string(input.Skip(1).ToArray()));

                            if (input == "T25" || modifier != 'S' && modifier != 'D' && modifier != 'T' && modifier != 'N' && (number < 1 || number > 20 && number != 25))
                                throw new ArgumentException();

                            Hitplace = input;

                            Valid = true;
                        }
                        catch
                        {
                            Valid = false;
                            Console.WriteLine("Darts need to be given in the correct form");
                            Console.WriteLine();
                        }
                    } while (!Valid);

                    ProcessDart(Hitplace);
                }

                if (CurrentHandScore > 0)
                    if (Scores[PlayerUp].Count == 0)
                        Scores[PlayerUp].Add(CurrentHandScore);
                    else
                        Scores[PlayerUp].Add(Scores[PlayerUp].Last() + CurrentHandScore);
                CurrentHandScore = 0;

                PlayerUp++;
                if (PlayerUp == NoPlayers)
                    PlayerUp = 0;


                bool NoMoreToScoreOn = !places.Any(x => Scored.Count(y => y[x] == 0) < 2);

                int Player = -1;
                bool OnePlayerHasAllTheScoreables = false;
                for (int i = 0; i < NoPlayers; i++)
                {
                    if (places.Sum(x => Scored.Count(y => y[x] == 0) > 1 ? 0 : Scored[i][x]) == 0)
                    {
                        Player = i;
                        OnePlayerHasAllTheScoreables = true;
                    }
                }

                bool ThatPlayerHasMorePoints = false;
                if (Player >= 0)
                {
                    List<int> MostPointsPlayers = new List<int>();
                    for (int i = 0; i < NoPlayers; i++)
                    {
                        if (Scores.Max(x => x.Last()) == Scores[i].Last())
                            MostPointsPlayers.Add(i);
                    }

                    ThatPlayerHasMorePoints = MostPointsPlayers.Count == 1 && MostPointsPlayers.First() == Player;
                }

                GameWon = NoMoreToScoreOn || OnePlayerHasAllTheScoreables && ThatPlayerHasMorePoints;
            }

            List<int> PlayerWon = new List<int>();
            for (int i = 0; i < NoPlayers; i++)
            {
                if (Scores.Max(x => x.Last()) == Scores[i].Last())
                    PlayerWon.Add(i);
            }

            Console.Clear();
            OutputGameBoard();

            if (PlayerWon.Count == 1)
                Console.WriteLine($"{PlayerNames[PlayerWon.First()]} has won");
            else if (PlayerWon.Count == NoPlayers)
            {
                if (Scores.Max(x => x.Last()) == 0)
                    Console.WriteLine("Nobody scored, everybody loses");
                else
                    Console.WriteLine("It's a tie");
            }
            else
            {
                string OutText = "";
                OutText += "It's a draw between ";
                for (int i = 0; i < PlayerWon.Count; i++)
                {
                    OutText += PlayerNames[PlayerWon[i]];

                    if (i == PlayerWon.Count - 2)
                        OutText += ", and ";
                    else
                        OutText += ", ";
                }

                Console.WriteLine(OutText.Take(OutText.Length - 2));
            }

            Console.ReadLine();
        }

        static void ProcessDart(string Hitplace)
        {
            /*
             *   Doubles or triple interchangably
             *   Centre bull counts as double bull
             *   Bull counts as >=10
             *   
             *   Hit nothing
             *   - Nothing
             * 
             *   Hit a single <10
             *   - Nothing
             *  
             *   Hit a single >=10
             *   - If that number not scratched off
             *       - If that number not filled then fill it
             *       - If that number filled then score
             * 
             *   Hit a double <10
             *   - If doubles are filled and not scratched off then score
             *       - else fill doubles by 1
             *
             *   Hit a double >=10
             *   - If both doubles and that number is scratched off then nothing
             *   - If neither doubles nor that number is scratched off
             *      - If neither that number nor doubles is filled then give choice to fill that number or fill doubles
             *      - If both that number and doubles is filled then score
             *      - If that number is filled and not doubles not filled then give the choice to score or fill doubles
             *      - If doubles is filled and not that number not filled then give the choice to score or fill that number
             *   - If doubles is scratched off and not that number
             *      - If that number is filled then score
             *      - If that number is not filled then fill it
             *   - If that number is scratched off and not doubles
             *      - If doubles is filled then score
             *      - If doubles is not filled then fill it
             * 
             */

            string modifier = Hitplace[0].ToString();
            int numberHit = int.Parse(new string(Hitplace.Skip(1).ToArray()));

            if (modifier == "N")
                return;

            if (numberHit < 10)
            {
                if (modifier == "S")
                    return;

                bool modifiedScratchedOff = Scored.Count(x => x[modifier] == 0) > 1;
                bool modifiedFilled = Scored[PlayerUp][modifier] == 0;

                if (numberHit < 10)
                {
                    if (modifiedFilled && !modifiedScratchedOff)
                        CurrentHandScore += numberHit * (modifier == "D" ? 2 : 3);
                    else
                        FillModified(modifier);
                }

                return;
            }

            bool numberScratchedOff = Scored.Count(x => x[numberHit.ToString()] == 0) > 1;
            bool numberFilled = Scored[PlayerUp][numberHit.ToString()] == 0;

            if (modifier == "S" && !numberScratchedOff)
            {
                if (!numberFilled)
                    FillNumber(numberHit);
                else
                    CurrentHandScore += numberHit;
            }

            if (modifier == "D" || modifier == "T")
            {
                bool modifiedScratchedOff = Scored.Count(x => x[modifier] == 0) > 1;
                bool modifiedFilled = Scored[PlayerUp][modifier] == 0;

                if (modifiedScratchedOff && numberScratchedOff)
                    return;

                if (!modifiedScratchedOff && !numberScratchedOff)
                {
                    bool Valid;

                    if (numberFilled && modifiedFilled)
                        CurrentHandScore += numberHit * (modifier == "D" ? 2 : 3);

                    else if (!numberFilled && !modifiedFilled)
                    {
                        int choice = -1;
                        do
                        {
                            try
                            {
                                Console.Write($"Take dart as {(modifier == "D" ? "2" : "3")} {numberHit}'s [0] or a {(modifier == "D" ? "double" : "triple")} [1]: ");
                                string input = Console.ReadLine();

                                choice = int.Parse(input);

                                if (choice != 0 && choice != 1)
                                    throw new ArgumentException();

                                Valid = true;
                            }
                            catch
                            {
                                Valid = false;
                                Console.WriteLine("Input needs to be either 0 or 1");
                                Console.WriteLine();
                            }
                        } while (!Valid);

                        if (choice == 0)
                            for (int i = 0; i < (modifier == "D" ? 2 : 3); i++)
                            {
                                if (Scored[PlayerUp][numberHit.ToString()] == 0)
                                    CurrentHandScore += numberHit;
                                else
                                    FillNumber(numberHit);
                            }
                        else if (choice == 1)
                            FillModified(modifier);
                    }
                    else if (numberFilled && !modifiedFilled)
                    {
                        int choice = -1;
                        do
                        {
                            try
                            {
                                Console.Write($"Take dart as score [0] or a {(modifier == "D" ? "double" : "triple")} [1]: ");
                                string input = Console.ReadLine();

                                choice = int.Parse(input);

                                if (choice != 0 && choice != 1)
                                    throw new ArgumentException();

                                Valid = true;
                            }
                            catch
                            {
                                Valid = false;
                                Console.WriteLine("Input needs to be either 0 or 1");
                                Console.WriteLine();
                            }
                        } while (!Valid);

                        if (choice == 0)
                            CurrentHandScore += numberHit * (modifier == "D" ? 2 : 3);
                        else if (choice == 1)
                            FillModified(modifier);
                    }
                    else if (modifiedFilled && !numberFilled)
                    {
                        int choice = -1;
                        do
                        {
                            try
                            {
                                Console.Write($"Take dart as score [0] or as {(modifier == "D" ? "2" : "3")} {numberHit}'s [1]: ");
                                string input = Console.ReadLine();

                                choice = int.Parse(input);

                                if (choice != 0 && choice != 1)
                                    throw new ArgumentException();

                                Valid = true;
                            }
                            catch
                            {
                                Valid = false;
                                Console.WriteLine("Input needs to be either 0 or 1");
                                Console.WriteLine();
                            }
                        } while (!Valid);

                        if (choice == 0)
                            CurrentHandScore += numberHit * (modifier == "D" ? 2 : 3);
                        else if (choice == 1)
                            FillModified(modifier);
                            
                    }
                }

                else if (modifiedScratchedOff && !numberScratchedOff)
                {
                    if (numberFilled)
                        CurrentHandScore += numberHit * (modifier == "D" ? 2 : 3);
                    else
                        for (int i = 0; i < (modifier == "D" ? 2 : 3); i++)
                        {
                            if (Scored[PlayerUp][numberHit.ToString()] == 0)
                                CurrentHandScore += numberHit;
                            else
                                FillNumber(numberHit);
                        }
                }

                else if (numberScratchedOff && !modifiedScratchedOff)
                {
                    if (modifiedFilled)
                        CurrentHandScore += numberHit * (modifier == "D" ? 2 : 3);
                    else
                        FillModified(modifier);
                }
            }

            for (int i = 0; i < NoPlayers; i++)
            {
                for (int j = 0; j < places.Length; j++)
                {
                    if (Scored[i][places[j]] < 0)
                        Scored[i][places[j]] = 0;
                }
            }
        }

        static void FillNumber(int numberHit)
        {
            Scored[PlayerUp][numberHit.ToString()] = Scored[PlayerUp][numberHit.ToString()] - 1;
        }

        static void FillModified(string modifier)
        {
            Scored[PlayerUp][modifier] = Scored[PlayerUp][modifier] - 1;
        }

        static void OutputGameBoard()
        {
            /*
             *       | Zac | JR  
             * ------|-----|-----
             *   20  |     |     
             *   19  |     |     
             *   18  |     |     
             *   17  |     |     
             *   16  |     |     
             *   15  |     |     
             *   14  |     |     
             *   13  |     |     
             *   12  |     |     
             *   11  |     |     
             *   10  |     |     
             *   D   |     |     
             *   T   |     |     
             *  Bull |     |     
             * 
             */

            Console.Write("      | ");
            for (int i = 0; i < NoPlayers; i++)
            {
                Console.Write(PlayerNames[i]);
                for (int j = 0; j < PlayerNameMaxLength - PlayerNames[i].Length; j++)
                    Console.Write(" ");

                if (i < NoPlayers - 1)
                    Console.Write(" | ");
            }
            Console.WriteLine(" ");

            Console.Write("------");
            for (int i = 0; i < NoPlayers; i++)
                Console.Write("|-----");
            Console.WriteLine();

            for (int i = 20; i >= 10; i--)
            {
                if (Scored.Count(x => x[i.ToString()] == 0) > 1)
                    continue;

                Console.Write($"  {i}  ");
                for (int j = 0; j < NoPlayers; j++)
                {
                    Console.Write($"| {(Scored[j][i.ToString()] >= 3 ? " " : "X")}{(Scored[j][i.ToString()] >= 2 ? " " : "X")}{(Scored[j][i.ToString()] >= 1 ? " " : "X")} ");
                }
                Console.WriteLine();
            }

            if (Scored.Count(x => x["D"] == 0) < 2)
            {
                Console.Write($"  D   ");
                for (int i = 0; i < NoPlayers; i++)
                {
                    Console.Write($"| {(Scored[i]["D"] >= 3 ? " " : "X")}{(Scored[i]["D"] >= 2 ? " " : "X")}{(Scored[i]["D"] >= 1 ? " " : "X")} ");
                }
                Console.WriteLine();
            }

            if (Scored.Count(x => x["T"] == 0) < 2)
            {
                Console.Write($"  T   ");
                for (int i = 0; i < NoPlayers; i++)
                {
                    Console.Write($"| {(Scored[i]["T"] >= 3 ? " " : "X")}{(Scored[i]["T"] >= 2 ? " " : "X")}{(Scored[i]["T"] >= 1 ? " " : "X")} ");
                }
                Console.WriteLine();
            }

            if (Scored.Count(x => x["25"] == 0) < 2)
            {
                Console.Write($" Bull ");
                for (int i = 0; i < NoPlayers; i++)
                {
                    Console.Write($"| {(Scored[i]["25"] >= 3 ? " " : "X")}{(Scored[i]["25"] >= 2 ? " " : "X")}{(Scored[i]["25"] >= 1 ? " " : "X")} ");
                }
                Console.WriteLine();
            }


            Console.WriteLine("\n");


            /* 
             * 
             *  Zac | JR
             * -----|-----
             *      |
             * 
             */

            Console.Write(" ");
            for (int i = 0; i < NoPlayers; i++)
            {
                Console.Write(PlayerNames[i]);
                for (int j = 0; j < PlayerNameMaxLength - PlayerNames[i].Length; j++)
                    Console.Write(" ");

                if (i < NoPlayers - 1)
                    Console.Write(" | ");
            }
            Console.WriteLine(" ");

            Console.Write("-----");
            for (int i = 1; i < NoPlayers; i++)
                Console.Write("|-----");
            Console.WriteLine();


            int NoScoreLinesNeeded = 0;
            foreach (List<int> Score in Scores)
                if (NoScoreLinesNeeded < Score?.Count - 1)
                    NoScoreLinesNeeded = Score.Count - 1;

            for (int i = 0; i < NoScoreLinesNeeded; i++)
            {
                Console.Write(" ");
                for (int j = 0; j < NoPlayers; j++)
                {
                    if (Scores[j].Skip(1).Count() > i)
                    {
                        Console.Write(Scores[j].Skip(1).ElementAt(i));
                        for (int k = 0; k < 3 - Scores[j].Skip(1).ElementAt(i).ToString().Length; k++)
                            Console.Write(" ");
                    }
                    else
                        Console.Write("   ");

                    if (j < NoPlayers - 1)
                        Console.Write(" | ");
                }
                Console.WriteLine(" ");
            }

            Console.WriteLine($"     |     ");


            Console.WriteLine();
        }
    }
}
