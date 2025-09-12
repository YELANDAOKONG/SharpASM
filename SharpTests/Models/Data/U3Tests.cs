using SharpASM.Models.Data;

namespace SharpTests.Models.Data;

public class U3Tests
{
    [Fact]
    public void ToBytes_WithBigEndian_ShouldReturnBytesInCorrectOrder()
    {
        // Arrange
        var u3 = new U3 { Byte1 = 0xAB, Byte2 = 0xCD, Byte3 = 0xEF };
        
        // Act
        var result = u3.ToBytes();
        
        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal(0xAB, result[0]);
        Assert.Equal(0xCD, result[1]);
        Assert.Equal(0xEF, result[2]);
    }
    
    [Fact]
    public void ToBytes_WithLittleEndian_ShouldReturnBytesInReverseOrder()
    {
        // Arrange
        var u3 = new U3 { Byte1 = 0xAB, Byte2 = 0xCD, Byte3 = 0xEF };
        
        // Act
        var result = u3.ToBytes(false);
        
        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal(0xEF, result[0]);
        Assert.Equal(0xCD, result[1]);
        Assert.Equal(0xAB, result[2]);
    }
    
    [Fact]
    public void FromBytes_WithBigEndian_ShouldCreateU3Correctly()
    {
        // Arrange
        byte[] bytes = [0x12, 0x34, 0x56];
        
        // Act
        var result = U3.FromBytes(bytes);
        
        // Assert
        Assert.Equal(0x12, result.Byte1);
        Assert.Equal(0x34, result.Byte2);
        Assert.Equal(0x56, result.Byte3);
    }
    
    [Fact]
    public void FromBytes_WithLittleEndian_ShouldCreateU3Correctly()
    {
        // Arrange
        byte[] bytes = [0x56, 0x34, 0x12];
        
        // Act
        var result = U3.FromBytes(bytes, false);
        
        // Assert
        Assert.Equal(0x12, result.Byte1);
        Assert.Equal(0x34, result.Byte2);
        Assert.Equal(0x56, result.Byte3);
    }
    
    [Fact]
    public void FromBytes_WithInsufficientBytes_ShouldThrowException()
    {
        // Arrange
        byte[] bytes = [0x01, 0x02];
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => U3.FromBytes(bytes));
    }
}
