public static class ArConstants
{
    public const string DefaultAnimatorLayer = "Base Layer";
    public const string ActuatorObjectName = "Atuador";
    public const string AssemblyAnimationPrefix = "animacao_";

    public static string AssemblyAnimationName(string stepNumber)
    {
        return $"{AssemblyAnimationPrefix}{stepNumber}";
    }
}
