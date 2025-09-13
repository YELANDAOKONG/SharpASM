using System;
using System.Collections.Generic;
using System.Text;

namespace SharpASM.Utilities;

/// <summary>
/// Java 类型描述符解析工具
/// </summary>
public static class DescriptorParser
{
    /// <summary>
    /// 解析字段描述符
    /// </summary>
    /// <param name="descriptor">字段描述符</param>
    /// <returns>解析后的类型信息</returns>
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
    /// 解析方法描述符
    /// </summary>
    /// <param name="descriptor">方法描述符</param>
    /// <returns>包含参数类型和返回类型的信息</returns>
    public static MethodTypeInfo ParseMethodDescriptor(string descriptor)
    {
        if (string.IsNullOrEmpty(descriptor))
            throw new ArgumentException("Descriptor cannot be null or empty", nameof(descriptor));

        int index = 0;
            
        // 方法描述符必须以 '(' 开头
        if (descriptor[index] != '(')
            throw new ArgumentException($"Method descriptor must start with '(', got: '{descriptor}'");
            
        index++; // 跳过 '('
            
        // 解析参数类型
        var parameters = new List<FieldTypeInfo>();
        while (index < descriptor.Length && descriptor[index] != ')')
        {
            parameters.Add(ParseFieldType(descriptor, ref index));
        }
            
        // 方法描述符必须以 ')' 结尾
        if (index >= descriptor.Length || descriptor[index] != ')')
            throw new ArgumentException($"Method descriptor must end with ')', got: '{descriptor}'");
            
        index++; // 跳过 ')'
            
        // 解析返回类型
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
    /// 解析字段类型
    /// </summary>
    private static FieldTypeInfo ParseFieldType(string descriptor, ref int index)
    {
        if (index >= descriptor.Length)
            throw new ArgumentException("Unexpected end of descriptor");
            
        char c = descriptor[index];
            
        // 基本类型
        if (c == 'B') { index++; return new FieldTypeInfo { Type = "byte", Descriptor = "B", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'C') { index++; return new FieldTypeInfo { Type = "char", Descriptor = "C", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'D') { index++; return new FieldTypeInfo { Type = "double", Descriptor = "D", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'F') { index++; return new FieldTypeInfo { Type = "float", Descriptor = "F", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'I') { index++; return new FieldTypeInfo { Type = "int", Descriptor = "I", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'J') { index++; return new FieldTypeInfo { Type = "long", Descriptor = "J", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'S') { index++; return new FieldTypeInfo { Type = "short", Descriptor = "S", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'Z') { index++; return new FieldTypeInfo { Type = "boolean", Descriptor = "Z", IsPrimitive = true, ArrayDimensions = 0 }; }
        if (c == 'V') { index++; return new FieldTypeInfo { Type = "void", Descriptor = "V", IsPrimitive = true, ArrayDimensions = 0 }; }
            
        // 数组类型
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
            
        // 对象类型
        if (c == 'L')
        {
            int start = index;
            index++; // 跳过 'L'
                
            // 找到分号
            int end = descriptor.IndexOf(';', index);
            if (end == -1)
                throw new ArgumentException($"Unterminated object descriptor in: '{descriptor}'");
                
            string className = descriptor.Substring(index, end - index);
            index = end + 1; // 跳过 ';'
                
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
    /// 将类型描述符转换为可读名称
    /// </summary>
    public static string DescriptorToReadableName(string descriptor)
    {
        var typeInfo = ParseFieldDescriptor(descriptor);
        return TypeInfoToReadableName(typeInfo);
    }
        
    /// <summary>
    /// 将类型信息转换为可读名称
    /// </summary>
    public static string TypeInfoToReadableName(FieldTypeInfo typeInfo)
    {
        var sb = new StringBuilder();
            
        // 添加数组维度
        for (int i = 0; i < typeInfo.ArrayDimensions; i++)
        {
            sb.Append("[]");
        }
            
        // 添加类型名称
        string typeName = typeInfo.Type;
        if (sb.Length > 0)
        {
            // 如果是数组，将类型名称放在前面
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
    /// 将方法描述符转换为可读签名
    /// </summary>
    public static string MethodDescriptorToReadableSignature(string descriptor)
    {
        var methodInfo = ParseMethodDescriptor(descriptor);
        return MethodInfoToReadableSignature(methodInfo);
    }
        
    /// <summary>
    /// 将方法信息转换为可读签名
    /// </summary>
    public static string MethodInfoToReadableSignature(MethodTypeInfo methodInfo)
    {
        var sb = new StringBuilder();
            
        // 参数列表
        sb.Append("(");
        for (int i = 0; i < methodInfo.Parameters.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(TypeInfoToReadableName(methodInfo.Parameters[i]));
        }
        sb.Append(")");
            
        // 返回类型
        if (methodInfo.ReturnType != null)
        {
            sb.Append(" : ");
            sb.Append(TypeInfoToReadableName(methodInfo.ReturnType));
        }
            
        return sb.ToString();
    }
    
    
    /// <summary>
    /// 字段类型信息
    /// </summary>
    public class FieldTypeInfo
    {
        /// <summary>
        /// 类型名称（已转换为点分格式，如 java.lang.String）
        /// </summary>
        public string Type { get; set; } = String.Empty;
        
        /// <summary>
        /// 原始描述符（如 Ljava/lang/String;）
        /// </summary>
        public string Descriptor { get; set; } = String.Empty;
        
        /// <summary>
        /// 是否为基本类型
        /// </summary>
        public bool IsPrimitive { get; set; }
        
        /// <summary>
        /// 数组维度（0表示不是数组）
        /// </summary>
        public int ArrayDimensions { get; set; }
        
        /// <summary>
        /// 数组元素类型（如果是数组）
        /// </summary>
        public FieldTypeInfo? ComponentType { get; set; }
        
        /// <summary>
        /// 转换为可读字符串
        /// </summary>
        public override string ToString()
        {
            return DescriptorParser.TypeInfoToReadableName(this);
        }
    }

    /// <summary>
    /// 方法类型信息
    /// </summary>
    public class MethodTypeInfo
    {
        /// <summary>
        /// 参数类型列表
        /// </summary>
        public List<FieldTypeInfo> Parameters { get; set; } = new List<FieldTypeInfo>();
        
        /// <summary>
        /// 返回类型（void方法返回null）
        /// </summary>
        public FieldTypeInfo? ReturnType { get; set; }
        
        /// <summary>
        /// 转换为可读字符串
        /// </summary>
        public override string ToString()
        {
            return DescriptorParser.MethodInfoToReadableSignature(this);
        }
    }
}
