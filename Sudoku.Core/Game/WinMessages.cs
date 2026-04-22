namespace Sudoku.Core.Game;

public static class WinMessages
{
    private static readonly string[] s_messages =
    [
        // Stillness & clarity
        "Inner Calm",
        "Clear Mind",
        "Still Waters",
        "Quiet Strength",
        "Peaceful Mind",
        "Gentle Focus",
        "Deep Stillness",
        "Gently Settled",
        "At Peace",
        "Centered",

        // Presence & mindfulness
        "Fully Present",
        "In the Moment",
        "Mindfully Done",
        "Grounded",
        "Unhurried",
        "Nothing Missed",
        "Wholly Here",
        "With Full Attention",
        "Quietly Awake",
        "Every Number Found",

        // Nature & flow
        "Like Water",
        "Soft Rain",
        "Morning Dew",
        "The River Rests",
        "River Stone",
        "Mountain Air",
        "Gentle Breeze",
        "Open Sky",
        "Still Pond",
        "Sunlit Path",

        // Achievement through calm
        "Quietly Done",
        "Simply Solved",
        "With Ease",
        "Naturally",
        "Effortless",
        "Gracefully",
        "Harmonious",
        "In Balance",
        "Well Placed",
        "Complete",

        // Wisdom & insight
        "Clear Sight",
        "Quiet Wisdom",
        "True Insight",
        "Inner Light",
        "Deep Knowing",
        "Awakened",
        "Enlightened",
        "Understood",
        "Clarity Found",
        "Illuminated",

        // Journey & arrival
        "The Path Clears",
        "Well Walked",
        "The Way Opens",
        "Arrived",
        "The Journey Complete",
        "Home Again",
        "The Gate Passed",
        "The Way Found",
        "Footprints Behind",
        "New Horizon",

        // Zen phrases
        "Beginner's Mind",
        "Empty Cup",
        "Just This",
        "Nothing Extra",
        "As It Should Be",
        "All in Place",
        "The Circle Closes",
        "Perfectly Whole",
        "No Trace",
        "Already There",

        // Warmth & gratitude
        "A Gentle Triumph",
        "Quiet Joy",
        "Warm Glow",
        "Softly Won",
        "A Good Moment",
        "Simple Pleasure",
        "Gently Won",
        "Sweet Silence",
        "Contentment",
        "Satisfaction",

        // Contemplation & care
        "Deeply Felt",
        "Well Considered",
        "Thoughtfully Done",
        "With Intention",
        "Carefully Placed",
        "Slowly, Surely",
        "Nothing Forced",
        "The Last Number",
        "The Last Piece",
        "Whole",

        // Space & release
        "Wide Open",
        "Room to Breathe",
        "Uncluttered",
        "Light as Air",
        "Unburdened",
        "Free and Clear",
        "Weightless",
        "Released",
        "Spacious Mind",
        "Letting Go",
    ];

    public static int Count => s_messages.Length;

    public static string GetRandom() => s_messages[Random.Shared.Next(s_messages.Length)];
}