namespace PlaceholderSoftware.WetStuff.Components
{
    /// <summary>
    /// An editor component which has a failure indicator. If it is not ok then subsequent sections in an editor may be disabled
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOkComponent<T>
        : IComponent<T>
    {
        bool Ok { get; }
    }
}
