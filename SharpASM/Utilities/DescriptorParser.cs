using System;
using System.Collections.Generic;
using System.Text;

namespace SharpASM.Utilities;

/// <summary>
/// Java Type Descriptor Parsing Utility
/// </summary>
public static class DescriptorParser
{
    /// <summary>
    /// Parses a field descriptor
    /// </summary>
    /// <param name="descriptor">Field descriptor string</param>
    /// <returns>Parsed type information</returns>
    public static FieldTypeInfo ParseFieldDescriptor(string descriptor)
    {
        if (string.IsNullOrEmpty(descriptor))
            throw new ArgumentException("Descriptor cannot be null or empty", nameof(descriptor));

        int index = 0;
        var result = ParseFieldType(descriptor, ref index);
            
        if (index != descriptor.Length)
            throw new ArgumentException($"Invalid descriptor: '{descriptor}' at position {index}");
                
        return result;
    }

    /// <summary>
    /// Parses a method descriptor
    /// </summary>
    /// <param name="descriptor">Method descriptor string</param>
    /// <returns>Information containing parameter types and return type</returns>
    public static MethodTypeInfo ParseMethodDescriptor(string descriptor)
    {
        if (string.IsNullOrEmpty(descriptor))
            throw new ArgumentException("Descriptor cannot be null or empty", nameof(descriptor));

        int index = 0;
            
        // Method descriptor must start with '('
        if (descriptor[index] != '(')
            throw new ArgumentException($"Method descriptor must start with '(', got: '{descriptor}'");
            
        index++; // Skip '('
            
        // Parse parameter types
        var parameters = new List<FieldTypeInfo>();
        while (index < descriptor.Length && descriptor[index] != ')')
        {
            parameters.Add(ParseFieldType(descriptor, ref index));
        }
            
        // Method descriptor must end with ')'
        if (index >= descriptor.Length || descriptor[index] != ')')
            throw new ArgumentException($"Method descriptor must end with ')', got: '{descriptor}'");
            
        index++; // Skip ')'
            
        // Parse return type
        var returnType = index < descriptor.Length ? ParseFieldType(descriptor, ref index) : null;
            
        if (index != descriptor.Length)
            throw new ArgumentException($"Invalid descriptor: '{descriptor}' at position {index}");
                
        return new MethodTypeInfo
        {
            Parameters = parameters,
            ReturnType = returnType
        };
    }

    /// <summary>
    /// Parses a field type from the descriptor
    /// </summary>
    private static FieldTypeInfo ParseFieldType(string descriptor, ref int index)
    {
        if (index >= descriptor.Length)
            throw new ArgumentException("Unexpected end of descriptor");
            
        char c = descriptor[index];
            
        // Primitive types
        if (c == 'B') { index++; return new FieldTypeInfo { Type = "byte", Descriptor = "B", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'C') { index++; return new FieldTypeInfo { Type = "char", Descriptor = "C", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'D') { index++; return new FieldTypeInfo { Type = "double", Descriptor = "D", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'F') { index++; return new FieldTypeInfo { Type = "float", Descriptor = "F", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'I') { index++; return new FieldTypeInfo { Type = "int", Descriptor = "I", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'J') { index++; return new FieldTypeInfo { Type = "long", Descriptor = "J", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'S') { index++; return new FieldTypeInfo { Type = "short", Descriptor = "S", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'Z') { index++; return new FieldTypeInfo { Type = "boolean", Descriptor = "Z", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'V') { index++; return new FieldTypeInfo { Type = "void", Descriptor = "V", IsPrimitive = true, ArrayDimensions = 0 }; }
            
        // Array types
        if (c == '[')
        {
            int dimensions = 0;
            while (index < descriptor.Length && descriptor[index] == '[')
            {
                dimensions++;
                index++;
            }
                
            var elementType = ParseFieldType(descriptor, ref index);
                
            return new FieldTypeInfo
            {
                Type = elementType.Type,
                Descriptor = new string('[', dimensions) + elementType.Descriptor,
                IsPrimitive = elementType.IsPrimitive,
                ArrayDimensions = dimensions,
                ComponentType = elementType
            };
        }
            
        // Object types
        if (c == 'L')
        {
            int start = index;
            index++; // Skip 'L'
                
            // Find semicolon
            int end = descriptor.IndexOf(';', index);
            if (end == -1)
                throw new ArgumentException($"Unterminated object descriptor in: '{descriptor}'");
                
            string className = descriptor.Substring(index, end - index);
            index = end + 1; // Skip ';'
                
            return new FieldTypeInfo
            {
                Type = className.Replace('/', '.'),
                Descriptor = descriptor.Substring(start, index - start),
                IsPrimitive = false,
                ArrayDimensions = 0
            };
        }
            
        throw new ArgumentException($"Invalid type descriptor character: '{c}' in '{descriptor}' at position {index}");
    }
        
    /// <summary>
    /// Converts a type descriptor to a human-readable name
    /// </summary>
    public static string DescriptorToReadableName(string descriptor)
    {
        var typeInfo = ParseFieldDescriptor(descriptor);
        return TypeInfoToReadableName(typeInfo);
    }
        
    /// <summary>
    /// Converts type information to a human-readable name
    /// </summary>
    public static string TypeInfoToReadableName(FieldTypeInfo typeInfo)
    {
        var sb = new StringBuilder();
            
        // Add array dimensions
        for (int i = 0; i < typeInfo.ArrayDimensions; i++)
        {
            sb.Append("[]");
        }
            
        // Add type name
        string typeName = typeInfo.Type;
        if (sb.Length > 0)
        {
            // For arrays, place type name first
            typeName = typeName + sb.ToString();
            sb.Clear();
            sb.Append(typeName);
        }
        else
        {
            sb.Append(typeName);
        }
            
        return sb.ToString();
    }
        
    /// <summary>
    /// Converts a method descriptor to a human-readable signature
    /// </summary>
    public static string MethodDescriptorToReadableSignature(string descriptor)
    {
        var methodInfo = ParseMethodDescriptor(descriptor);
        return MethodInfoToReadableSignature(methodInfo);
    }
        
    /// <summary>
    /// Converts method information to a human-readable signature
    /// </summary>
    public static string MethodInfoToReadableSignature(MethodTypeInfo methodInfo)
    {
        var sb = new StringBuilder();
            
        // Parameter list
        sb.Append("(");
        for (int i = 0; i < methodInfo.Parameters.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(TypeInfoToReadableName(methodInfo.Parameters[i]));
        }
        sb.Append(")");
            
        // Return type
        if (methodInfo.ReturnType != null)
        {
            sb.Append(" : ");
            sb.Append(TypeInfoToReadableName(methodInfo.ReturnType));
        }
            
        return sb.ToString();
    }
    
    
    /// <summary>
    /// Field type information
    /// </summary>
    public class FieldTypeInfo
    {
        /// <summary>
        /// Type name (converted to dot notation, e.g., java.lang.String)
        /// </summary>
        public string Type { get; set; } = String.Empty;
        
        /// <summary>
        /// Original descriptor (e.g., Ljava/lang/String;)
        /// </summary>
        public string Descriptor { get; set; } = String.Empty;
        
        /// <summary>
        /// Whether the type is a primitive
        /// </summary>
        public bool IsPrimitive { get; set; }
        
        /// <summary>
        /// Array dimensions (0 indicates not an array)
        /// </summary>
        public int ArrayDimensions { get; set; }
        
        /// <summary>
        /// Array element type (if applicable)
        /// </summary>
        public FieldTypeInfo? ComponentType { get; set; }
        
        /// <summary>
        /// Converts to a human-readable string
        /// </summary>
        public override string ToString()
        {
            return DescriptorParser.TypeInfoToReadableName(this);
        }
    }

    /// <summary>
    /// Method type information
    /// </summary>
    public class MethodTypeInfo
    {
        /// <summary>
        /// List of parameter types
        /// </summary>
        public List<FieldTypeInfo> Parameters { get; set; } = new List<FieldTypeInfo>();
        
        /// <summary>
        /// Return type (null for void methods)
        /// </summary>
        public FieldTypeInfo? ReturnType { get; set; }
        
        /// <summary>
        /// Converts to a human-readable string
        /// </summary>
        public override string ToString()
        {
            return DescriptorParser.MethodInfoToReadableSignature(this);
        }
    }
}
