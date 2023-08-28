namespace PlaceholderSoftware.WetStuff.Components
{
    public interface IComponent<T>
    {
        bool IsVisible { get; }

        void OnEnable(T target);

        void Draw();
    }
}