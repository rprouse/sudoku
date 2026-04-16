namespace Sudoku.Core.Game;

public static class WinMessages
{
    private static readonly string[] s_messages =
    [
        // Classic praise
        "Stupendous",
        "Magnificent",
        "Brilliant",
        "Outstanding",
        "Spectacular",
        "Extraordinary",
        "Remarkable",
        "Exceptional",
        "Superb",
        "Impressive",
        "Splendid",
        "Marvellous",
        "Masterful",
        "Astounding",
        "Astonishing",
        "Staggering",
        "Phenomenal",
        "Incredible",
        "Fantastic",
        "Wonderful",

        // Humorous / playful
        "Big Brain",
        "Galaxy Brain",
        "Smarty Pants",
        "Sudoku Ninja",
        "Number Wizard",
        "Digit Whisperer",
        "Puzzle Crusher",
        "Grid Dominator",
        "Certified Genius",
        "Absolute Legend",
        "Mad Skills",
        "Too Easy",
        "Nailed It",
        "Boom Shakalaka",
        "No Sweat",
        "Like a Boss",
        "Mind Blown",
        "Drop the Mic",
        "Crushed It",
        "Victory Dance",

        // Over-the-top
        "Supreme Overlord",
        "Unstoppable Force",
        "Sudoku Royalty",
        "Number Cruncher Supreme",
        "Master of the Grid",
        "Puzzle Prodigy",
        "Brainiac",
        "Human Calculator",
        "Pencil Pusher Extraordinaire",
        "Logic Legend",

        // Silly / fun
        "Holy Smokes",
        "Wowza",
        "Hot Diggity",
        "Well Well Well",
        "Look at You Go",
        "Who Needs a Calculator",
        "Einstein Who",
        "Sudoku Sorcerer",
        "Grid Whisperer",
        "Number Ninja",
        "Brainy McBrainface",
        "Captain Sudoku",
        "Professor Puzzle",
        "The Chosen One",
        "Puzzle Picasso",

        // Exclamatory
        "Oh Snap",
        "Ka-Pow",
        "Bazinga",
        "Shazam",
        "Bingo Bango",
        "Abracadabra",
        "Hocus Focus",
        "Ta-Da",
        "Voilà",
        "Eureka",

        // Confident / cheeky
        "Show Off",
        "Flex Worthy",
        "Not Even Close",
        "Child's Play",
        "Piece of Cake",
        "Easy Peasy",
        "Walk in the Park",
        "Smooth Operator",
        "Cool as a Cucumber",
        "Zero Doubt",

        // Wholesome
        "You Did It",
        "So Proud",
        "Gold Star",
        "Top Marks",
        "Well Played",

        // Nerdy / witty
        "Sudoku Savant",
        "The Grid Awakens",
        "Return of the Solver",
        "A New Hope for Numbers",
        "May the Nines Be With You",
        "Elementary, My Dear",
        "Game, Set, Solved",
        "Check and Mate",
        "Flawless Victory",
        "Achievement Unlocked",
    ];

    public static int Count => s_messages.Length;

    public static string GetRandom() => s_messages[Random.Shared.Next(s_messages.Length)];
}
