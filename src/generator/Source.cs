namespace InfiniteEnumFlags.Generator;

internal class Source
{
    public Source(string fileName, string code)
    {
        FileName = fileName;
        Code = code;
    }

    public string FileName { get; }
    public string Code { get; }
}