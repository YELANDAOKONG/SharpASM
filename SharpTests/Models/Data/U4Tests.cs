using SharpASM.Models.Data;

namespace SharpTests.Models.Data;

public class U4Tests
{
    [Fact]
    public void ToBytes_WithBigEndian_ShouldReturnBytesInCorrectOrder()
    {
        // Arrange
        var u4 = new U4 { Byte1 = 0xAB, Byte2 = 0xCD, Byte3 = 0xEF, Byte4 = 0x12 };
        
        // Act
        var result = u4.ToBytes();
        
        // Assert
        Assert.Equal(4, result.Length);
        Assert.Equal(0xAB, result[0]);
        Assert.Equal(0xCD, result[1]);
        Assert.Equal(0xEF, result[2]);
        Assert.Equal(0x12, result[3]);
    }
    
    [Fact]
    public void ToBytes_WithLittleEndian_ShouldReturnBytesInReverseOrder()
    {
        // Arrange
        var u4 = new U4 { Byte1 = 0xAB, Byte2 = 0xCD, Byte3 = 0xEF, Byte4 = 0x12 };
        
        // Act
        var result = u4.ToBytes(false);
        
        // Assert
        Assert.Equal(4, result.Length);
        Assert.Equal(0x12, result[0]);
        Assert.Equal(0xEF, result[1]);
        Assert.Equal(0xCD, result[2]);
        Assert.Equal(0xAB, result[3]);
    }
    
    [Fact]
    public void FromBytes_WithBigEndian_ShouldCreateU4Correctly()
    {
        // Arrange
        byte[] bytes = [0x12, 0x34, 0x56, 0x78];
        
        // Act
        var result = U4.FromBytes(bytes);
        
        // Assert
        Assert.Equal(0x12, result.Byte1);
        Assert.Equal(0x34, result.Byte2);
        Assert.Equal(0x56, result.Byte3);
        Assert.Equal(0x78, result.Byte4);
    }
    
    [Fact]
    public void FromBytes_WithLittleEndian_ShouldCreateU4Correctly()
    {
        // Arrange
        byte[] bytes = [0x78, 0x56, 0x34, 0x12];
        
        // Act
        var result = U4.FromBytes(bytes, false);
        
        // Assert
        Assert.Equal(0x12, result.Byte1);
        Assert.Equal(0x34, result.Byte2);
        Assert.Equal(0x56, result.Byte3);
        Assert.Equal(0x78, result.Byte4);
    }
    
    [Fact]
    public void FromBytes_WithInsufficientBytes_ShouldThrowException()
    {
        // Arrange
        byte[] bytes = [0x01, 0x02, 0x03];
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => U4.FromBytes(bytes));
    }
    
    [Fact]
    public void ToUInt32_WithBigEndian_ShouldReturnCorrectValue()
    {
        // Arrange
        var u4 = new U4 { Byte1 = 0x12, Byte2 = 0x34, Byte3 = 0x56, Byte4 = 0x78 };
        
        // Act
        var result = u4.ToUInt32();
        
        // Assert
        Assert.Equal(0x12345678u, result);
    }
    
    [Fact]
    public void ToUInt32_WithLittleEndian_ShouldReturnCorrectValue()
    {
        // Arrange
        var u4 = new U4 { Byte1 = 0x78, Byte2 = 0x56, Byte3 = 0x34, Byte4 = 0x12 };
        
        // Act
        var result = u4.ToUInt32(false);
        
        // Assert
        Assert.Equal(0x12345678u, result);
    }
    
    [Fact]
    public void FromUInt32_WithBigEndian_ShouldCreateU4Correctly()
    {
        // Act
        var result = U4.FromUInt32(0x12345678);
        
        // Assert
        Assert.Equal(0x12, result.Byte1);
        Assert.Equal(0x34, result.Byte2);
        Assert.Equal(0x56, result.Byte3);
        Assert.Equal(0x78, result.Byte4);
    }
    
    [Fact]
    public void FromUInt32_WithLittleEndian_ShouldCreateU4Correctly()
    {
        // Act
        var result = U4.FromUInt32(0x12345678, false);
        
        // Assert
        Assert.Equal(0x78, result.Byte1);
        Assert.Equal(0x56, result.Byte2);
        Assert.Equal(0x34, result.Byte3);
        Assert.Equal(0x12, result.Byte4);
    }
}
