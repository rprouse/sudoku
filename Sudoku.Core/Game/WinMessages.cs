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
        "Serene",
        "Tranquil",
        "At Peace",
        "Centered",

        // Presence & mindfulness
        "Fully Present",
        "In the Moment",
        "Deep Breath",
        "Mindful",
        "Grounded",
        "Unhurried",
        "Patient",
        "Attentive",
        "Aware",
        "Composed",

        // Nature & flow
        "Like Water",
        "Soft Rain",
        "Morning Dew",
        "Autumn Leaf",
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
        "Understanding",
        "Clarity",
        "Illuminated",

        // Journey & growth
        "One Step Deeper",
        "The Path Clears",
        "Onward",
        "Well Walked",
        "Each Step Counts",
        "The Way Opens",
        "Steady Progress",
        "Growing Stronger",
        "Ever Upward",
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
        "Small Victory",
        "Sweet Silence",
        "Contentment",
        "Satisfaction",

        // Contemplation
        "Deeply Felt",
        "Well Considered",
        "Thoughtfully Done",
        "With Intention",
        "Carefully Placed",
        "Slowly, Surely",
        "Step by Step",
        "Breath by Breath",
        "Piece by Piece",
        "One by One",

        // Space & openness
        "Wide Open",
        "Room to Breathe",
        "Uncluttered",
        "Light as Air",
        "Unburdened",
        "Free and Clear",
        "Weightless",
        "Unbound",
        "Spacious Mind",
        "Letting Go",
    ];

    public static int Count => s_messages.Length;

    public static string GetRandom() => s_messages[Random.Shared.Next(s_messages.Length)];
}
