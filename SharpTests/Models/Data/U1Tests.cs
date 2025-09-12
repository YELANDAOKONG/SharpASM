using SharpASM.Models.Data;

namespace SharpTests.Models.Data;

public class U1Tests
{
    [Fact]
    public void ToBytes_ShouldReturnSingleByte()
    {
        // Arrange
        var u1 = new U1 { Byte1 = 0xAB };
        
        // Act
        var result = u1.ToBytes();
        
        // Assert
        Assert.Single(result);
        Assert.Equal(0xAB, result[0]);
    }
    
    [Fact]
    public void ToBytes_WithLittleEndian_ShouldReturnSingleByte()
    {
        // Arrange
        var u1 = new U1 { Byte1 = 0xAB };
        
        // Act
        var result = u1.ToBytes(false);
        
        // Assert
        Assert.Single(result);
        Assert.Equal(0xAB, result[0]);
    }
    
    [Fact]
    public void FromBytes_ShouldCreateU1Correctly()
    {
        // Arrange
        byte[] bytes = [0xCD];
        
        // Act
        var result = U1.FromBytes(bytes);
        
        // Assert
        Assert.Equal(0xCD, result.Byte1);
    }
    
    [Fact]
    public void FromBytes_WithInsufficientBytes_ShouldThrowException()
    {
        // Arrange
        byte[] bytes = [];
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => U1.FromBytes(bytes));
    }
    
    [Fact]
    public void ToByte_ShouldReturnCorrectValue()
    {
        // Arrange
        var u1 = new U1 { Byte1 = 0xEF };
        
        // Act
        var result = u1.ToByte();
        
        // Assert
        Assert.Equal(0xEF, result);
    }
    
    [Fact]
    public void FromByte_ShouldCreateU1Correctly()
    {
        // Act
        var result = U1.FromByte(0x12);
        
        // Assert
        Assert.Equal(0x12, result.Byte1);
    }
    
    [Fact]
    public void FromByte_WithLittleEndian_ShouldCreateU1Correctly()
    {
        // Act
        var result = U1.FromByte(0x12, false);
        
        // Assert
        Assert.Equal(0x12, result.Byte1);
    }
}