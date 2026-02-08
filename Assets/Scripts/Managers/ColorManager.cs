using UnityEngine;

public static class ColorManager
{
    public static Color StartNode          = new Color(16f/255f, 185f/255f, 129f/255f);   // #10b981  emerald
    public static Color GoalNode           = new Color(239f/255f, 68f/255f, 68f/255f);    // #ef4444  red
    public static Color UnvisitedNode      = new Color(30f/255f, 41f/255f, 59f/255f);     // #1e293b  slate
    public static Color FrontierNode       = new Color(59f/255f, 130f/255f, 246f/255f);   // #3b82f6  bright blue
    public static Color VisitedNode        = new Color(107f/255f, 114f/255f, 128f/255f);  // #6b7280  cool gray
    public static Color PathNode           = new Color(250f/255f, 204f/255f, 21f/255f);   // #facc15  vivid yellow
    public static Color NormalEdge         = new Color(51f/255f, 65f/255f, 85f/255f);     // #334155  subtle dark
    public static Color PathEdge           = new Color(251f/255f, 191f/255f, 36f/255f);   // #fbbf24  amber glow
    public static Color BetterHighlight   = new Color(0f, 1f, 1f);
}