using SharpASM.Models.Data;

namespace SharpTests.Models.Data;

public class U2Tests
{
    [Fact]
    public void ToBytes_WithBigEndian_ShouldReturnBytesInCorrectOrder()
    {
        // Arrange
        var u2 = new U2 { Byte1 = 0xAB, Byte2 = 0xCD };
        
        // Act
        var result = u2.ToBytes();
        
        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal(0xAB, result[0]);
        Assert.Equal(0xCD, result[1]);
    }
    
    [Fact]
    public void ToBytes_WithLittleEndian_ShouldReturnBytesInReverseOrder()
    {
        // Arrange
        var u2 = new U2 { Byte1 = 0xAB, Byte2 = 0xCD };
        
        // Act
        var result = u2.ToBytes(false);
        
        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal(0xCD, result[0]);
        Assert.Equal(0xAB, result[1]);
    }
    
    [Fact]
    public void FromBytes_WithBigEndian_ShouldCreateU2Correctly()
    {
        // Arrange
        byte[] bytes = [0x12, 0x34];
        
        // Act
        var result = U2.FromBytes(bytes);
        
        // Assert
        Assert.Equal(0x12, result.Byte1);
        Assert.Equal(0x34, result.Byte2);
    }
    
    [Fact]
    public void FromBytes_WithLittleEndian_ShouldCreateU2Correctly()
    {
        // Arrange
        byte[] bytes = [0x34, 0x12];
        
        // Act
        var result = U2.FromBytes(bytes, false);
        
        // Assert
        Assert.Equal(0x12, result.Byte1);
        Assert.Equal(0x34, result.Byte2);
    }
    
    [Fact]
    public void FromBytes_WithInsufficientBytes_ShouldThrowException()
    {
        // Arrange
        byte[] bytes = [0x01];
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => U2.FromBytes(bytes));
    }
    
    [Fact]
    public void ToUInt16_WithBigEndian_ShouldReturnCorrectValue()
    {
        // Arrange
        var u2 = new U2 { Byte1 = 0x12, Byte2 = 0x34 };
        
        // Act
        var result = u2.ToUInt16();
        
        // Assert
        Assert.Equal(0x1234, result);
    }
    
    [Fact]
    public void ToUInt16_WithLittleEndian_ShouldReturnCorrectValue()
    {
        // Arrange
        var u2 = new U2 { Byte1 = 0x34, Byte2 = 0x12 };
        
        // Act
        var result = u2.ToUInt16(false);
        
        // Assert
        Assert.Equal(0x1234, result);
    }
    
    [Fact]
    public void FromUInt16_WithBigEndian_ShouldCreateU2Correctly()
    {
        // Act
        var result = U2.FromUInt16(0x1234);
        
        // Assert
        Assert.Equal(0x12, result.Byte1);
        Assert.Equal(0x34, result.Byte2);
    }
    
    [Fact]
    public void FromUInt16_WithLittleEndian_ShouldCreateU2Correctly()
    {
        // Act
        var result = U2.FromUInt16(0x1234, false);
        
        // Assert
        Assert.Equal(0x34, result.Byte1);
        Assert.Equal(0x12, result.Byte2);
    }
}
