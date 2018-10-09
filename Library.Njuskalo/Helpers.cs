namespace Library.Njuskalo
{
    public static class Helpers
    {
        public static bool HasValue(this object source)
        {
            return source == null ? false : (source is string ? !string.IsNullOrWhiteSpace(source as string) : true);
        }
    }
}
