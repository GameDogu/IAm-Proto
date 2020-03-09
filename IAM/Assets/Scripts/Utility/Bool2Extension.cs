using Unity.Mathematics;

public static class Bool2Extension
{
    public static bool ElemtAnd(this bool2 b)
    {
        return b.x && b.y;
    }

    public static bool ElemrOr(this bool2 b)
    {
        return b.x || b.y;
    }
}