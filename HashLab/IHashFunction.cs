namespace HashLab
{
    interface IHashFunction
    {
        string Name { get; }
        string Authors { get; }

        byte[] GetHash(string message);
    }
}