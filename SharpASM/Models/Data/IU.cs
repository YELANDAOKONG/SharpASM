namespace SharpASM.Models.Data;

public interface IU
{
    byte[] ToBytes(bool bigEndian = true);
}