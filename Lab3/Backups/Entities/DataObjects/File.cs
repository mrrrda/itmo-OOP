using System.Text;

namespace Backups.Entities.DataObjects;

public class File : BaseFile
{
    public File(string name, string extension)
        : base(name, extension)
    {
        Data = new MemoryStream();
    }

    public MemoryStream Data { get; }

    public string ReadString()
    {
        Data.Seek(0, SeekOrigin.Begin);

        return new StreamReader(Data).ReadToEnd();
    }

    public byte[] ReadBytes()
    {
        return Data.ToArray();
    }

    public void WriteString(string newData)
    {
        if (newData is null)
            throw new ArgumentNullException(nameof(newData), "Invalid data");

        Data.Seek(0, SeekOrigin.Begin);
        Data.Write(Encoding.Default.GetBytes(newData));
    }

    public void WriteBytes(byte[] newData)
    {
        if (newData.Length == 0)
            throw new ArgumentNullException(nameof(newData), "Empty data");

        Data.Seek(0, SeekOrigin.Begin);
        Data.Write(newData);
    }

    public void AppendString(string newData)
    {
        if (newData is null)
            throw new ArgumentNullException(nameof(newData), "Invalid data");

        Data.Write(Encoding.Default.GetBytes(newData));
    }

    public void AppendBytes(byte[] newData)
    {
        if (newData.Length == 0)
            throw new ArgumentNullException(nameof(newData), "Empty data");

        Data.Write(newData);
    }
}
