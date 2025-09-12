namespace SharpTests.Tests.Utilities;

public class TestHelper
{
    public static byte[] HexToBytes(string hex)
    {
        string data = hex.Replace("\n", "").ToUpper();
        byte[] bytes = Enumerable.Range(0, data.Length / 2)
            .Select(x => Convert.ToByte(data.Substring(x * 2, 2), 16))
            .ToArray();
        return bytes;
    }
}