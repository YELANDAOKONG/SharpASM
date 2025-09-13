# Sharp ASM

> 开发中...

使用 C# 解析和编辑 Java Class 字节码。

> 正在开发中，功能会逐步完善，代 ~~（Shi）~~ 码 ~~（Shan）~~ 会慢慢优化......

## 示例

### 字节码解析效果

```text
[@] This Class: LongMain
[@] Super Class: java/lang/Object
[@] - Field: Int I [ Public | Static ]
[@] - Field: Long J [ Public | Static ]
[@] - Field: Float F [ Public | Static ]
[@] - Field: Double D [ Public | Static ]
[@] - Method: <init> ()V [ Public ]
[@] -  - Attribute: Code (47 Bytes)
[@] -  -  - ALOAD_0 []
[@] -  -  - INVOKESPECIAL [0x0001]
[@] -  -  - RETURN []
[@] - Method: main ([Ljava/lang/String;)V [ Public | Static ]
[@] -  - Attribute: Code (122 Bytes)
[@] -  -  - GETSTATIC [0x0007]
[@] -  -  - LDC [0x0D]
[@] -  -  - ICONST_0 []
[@] -  -  - ANEWARRAY [0x0002]
[@] -  -  - INVOKEVIRTUAL [0x000F]
[@] -  -  - POP []
[@] -  -  - ICONST_1 []
[@] -  -  - ISTORE_1 []
[@] -  -  - ILOAD_1 []
[@] -  -  - ICONST_5 []
[@] -  -  - IF_ICMPGT [0x0015]
[@] -  -  - GETSTATIC [0x0007]
[@] -  -  - ILOAD_1 []
[@] -  -  - INVOKEDYNAMIC [0x0015]
[@] -  -  - NOP []
[@] -  -  - NOP []
[@] -  -  - INVOKEVIRTUAL [0x0019]
[@] -  -  - IINC [0x01, 0x01]
[@] -  -  - GOTO [0xFFEC]
[@] -  -  - RETURN []
[@] - Method: <clinit> ()V [ Static ]
[@] -  - Attribute: Code (59 Bytes)
[@] -  -  - LDC [0x1D]
[@] -  -  - PUTSTATIC [0x001E]
[@] -  -  - LDC2_W [0x0024]
[@] -  -  - PUTSTATIC [0x0026]
[@] -  -  - LDC [0x2A]
[@] -  -  - PUTSTATIC [0x002B]
[@] -  -  - LDC2_W [0x002F]
[@] -  -  - PUTSTATIC [0x0031]
[@] -  -  - RETURN []
```

### 字节码解析效果 ( 完整版 )

```csharp
[@] Magic: 0xCAFEBABE
[@] Minor Version: 0
[@] Major Version: V21
[@] Constant Pool: 85 Length
[@] Constant Pool: 87 Index
[@] Access Flags: Public | Super
[@] This Class: MainPlus
[@] Super Class: java/lang/Object
[@] Constant Pool: 
[@] - Constant: (Methodref) #2.#3
[@] - Constant: (Class) #4
[@] - Constant: (NameAndType) #5:#6
[@] - Constant: (Utf8) "java/lang/Object"
[@] - Constant: (Utf8) "<init>"
[@] - Constant: (Utf8) "()V"
[@] - Constant: (Fieldref) #8.#9
[@] - Constant: (Class) #10
[@] - Constant: (NameAndType) #11:#12
[@] - Constant: (Utf8) "java/lang/System"
[@] - Constant: (Utf8) "out"
[@] - Constant: (Utf8) "Ljava/io/PrintStream;"
[@] - Constant: (String) #14
[@] - Constant: (Utf8) "Hello and welcome! "
[@] - Constant: (Methodref) #16.#17
[@] - Constant: (Class) #18
[@] - Constant: (NameAndType) #19:#20
[@] - Constant: (Utf8) "java/io/PrintStream"
[@] - Constant: (Utf8) "println"
[@] - Constant: (Utf8) "(Ljava/lang/String;)V"
[@] - Constant: (InvokeDynamic) #0.#22
[@] - Constant: (NameAndType) #23:#24
[@] - Constant: (Utf8) "makeConcatWithConstants"
[@] - Constant: (Utf8) "(I)Ljava/lang/String;"
[@] - Constant: (Integer) 123456
[@] - Constant: (Fieldref) #27.#28
[@] - Constant: (Class) #29
[@] - Constant: (NameAndType) #30:#31
[@] - Constant: (Utf8) "MainPlus"
[@] - Constant: (Utf8) "Int"
[@] - Constant: (Utf8) "I"
[@] - Constant: (Long) 123456789L
[@] - Constant: (Fieldref) #27.#35
[@] - Constant: (NameAndType) #36:#37
[@] - Constant: (Utf8) "Long"
[@] - Constant: (Utf8) "J"
[@] - Constant: (Float) 123.456
[@] - Constant: (Fieldref) #27.#40
[@] - Constant: (NameAndType) #41:#42
[@] - Constant: (Utf8) "Float"
[@] - Constant: (Utf8) "F"
[@] - Constant: (Double) 123.456789D
[@] - Constant: (Fieldref) #27.#46
[@] - Constant: (NameAndType) #47:#48
[@] - Constant: (Utf8) "Double"
[@] - Constant: (Utf8) "D"
[@] - Constant: (Utf8) "IntFinal"
[@] - Constant: (Utf8) "ConstantValue"
[@] - Constant: (Utf8) "LongFinal"
[@] - Constant: (Utf8) "FloatFinal"
[@] - Constant: (Utf8) "DoubleFinal"
[@] - Constant: (Utf8) "StringFinal"
[@] - Constant: (Utf8) "Ljava/lang/String;"
[@] - Constant: (String) #57
[@] - Constant: (Utf8) "Hello, World! "
[@] - Constant: (Utf8) "Code"
[@] - Constant: (Utf8) "LineNumberTable"
[@] - Constant: (Utf8) "LocalVariableTable"
[@] - Constant: (Utf8) "this"
[@] - Constant: (Utf8) "LMainPlus;"
[@] - Constant: (Utf8) "main"
[@] - Constant: (Utf8) "([Ljava/lang/String;)V"
[@] - Constant: (Utf8) "i"
[@] - Constant: (Utf8) "args"
[@] - Constant: (Utf8) "[Ljava/lang/String;"
[@] - Constant: (Utf8) "StackMapTable"
[@] - Constant: (Utf8) "<clinit>"
[@] - Constant: (Utf8) "SourceFile"
[@] - Constant: (Utf8) "MainPlus.java"
[@] - Constant: (Utf8) "BootstrapMethods"
[@] - Constant: (String) #74
[@] - Constant: (Utf8) "i = "
[@] - Constant: (MethodHandle) 6.#76
[@] - Constant: (Methodref) #77.#78
[@] - Constant: (Class) #79
[@] - Constant: (NameAndType) #23:#80
[@] - Constant: (Utf8) "java/lang/invoke/StringConcatFactory"
[@] - Constant: (Utf8) "(Ljava/lang/invoke/MethodHandles$Lookup;Ljava/lang/String;Ljava/lang/invoke/MethodType;Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/invoke/CallSite;"
[@] - Constant: (Utf8) "InnerClasses"
[@] - Constant: (Class) #83
[@] - Constant: (Utf8) "java/lang/invoke/MethodHandles$Lookup"
[@] - Constant: (Class) #85
[@] - Constant: (Utf8) "java/lang/invoke/MethodHandles"
[@] - Constant: (Utf8) "Lookup"
[@] Attributes: 
[@] - Attribute: SourceFile (2 Bytes)
[@] - Attribute: BootstrapMethods (8 Bytes)
[@] - Attribute: InnerClasses (10 Bytes)
[@] Fields: 
[@] - Field: Int I [ Public | Static ]
[@] - Field: IntFinal I [ Public | Static | Final ]
[@] -  - Attribute: ConstantValue (2 Bytes)
[@] -  -  - ConstantValue: (Integer) 123456
[@] - Field: Long J [ Public | Static ]
[@] - Field: LongFinal J [ Public | Static | Final ]
[@] -  - Attribute: ConstantValue (2 Bytes)
[@] -  -  - ConstantValue: (Long) 123456789L
[@] - Field: Float F [ Public | Static ]
[@] - Field: FloatFinal F [ Public | Static | Final ]
[@] -  - Attribute: ConstantValue (2 Bytes)
[@] -  -  - ConstantValue: (Float) 123.456
[@] - Field: Double D [ Public | Static ]
[@] - Field: DoubleFinal D [ Public | Static | Final ]
[@] -  - Attribute: ConstantValue (2 Bytes)
[@] -  -  - ConstantValue: (Double) 123.456789D
[@] - Field: StringFinal Ljava/lang/String; [ Public | Static | Final ]
[@] -  - Attribute: ConstantValue (2 Bytes)
[@] -  -  - ConstantValue: (String) #57
[@] -  -  -  - String Value: "Hello, World! "
[@] Methods: 
[@] - Method: <init> ()V [ Public ]
[@] -  - Attribute: Code (47 Bytes)
[@] -  -  - ALOAD_0 []
[@] -  -  - INVOKESPECIAL [0x0001]
[@] -  -  - RETURN []
[@] - Method: main ([Ljava/lang/String;)V [ Public | Static ]
[@] -  - Attribute: Code (117 Bytes)
[@] -  -  - GETSTATIC [0x0007]
[@] -  -  - LDC [0x0D]
[@] -  -  - INVOKEVIRTUAL [0x000F]
[@] -  -  - ICONST_1 []
[@] -  -  - ISTORE_1 []
[@] -  -  - ILOAD_1 []
[@] -  -  - ICONST_5 []
[@] -  -  - IF_ICMPGT [0x0015]
[@] -  -  - GETSTATIC [0x0007]
[@] -  -  - ILOAD_1 []
[@] -  -  - INVOKEDYNAMIC [0x0015]
[@] -  -  - NOP []
[@] -  -  - NOP []
[@] -  -  - INVOKEVIRTUAL [0x000F]
[@] -  -  - IINC [0x01, 0x01]
[@] -  -  - GOTO [0xFFEC]
[@] -  -  - RETURN []
[@] - Method: <clinit> ()V [ Static ]
[@] -  - Attribute: Code (59 Bytes)
[@] -  -  - LDC [0x19]
[@] -  -  - PUTSTATIC [0x001A]
[@] -  -  - LDC2_W [0x0020]
[@] -  -  - PUTSTATIC [0x0022]
[@] -  -  - LDC [0x26]
[@] -  -  - PUTSTATIC [0x0027]
[@] -  -  - LDC2_W [0x002B]
[@] -  -  - PUTSTATIC [0x002D]
[@] -  -  - RETURN []
```

### 构建字节码 ( 字节码 API )

```csharp
// ...

// 构建字节码
var codes = new List<Code>();
codes.Add(Code.GetStatic(systemOutFieldRefIndex));
codes.Add(new Code(OperationCode.LDC, [Operand.Index((byte)helloWorldStrIndex)]));
codes.Add(Code.InvokeVirtual(printlnMethodRefIndex));
codes.Add(Code.GetStatic(systemOutFieldRefIndex));
codes.Add(new Code(OperationCode.LDC, [Operand.Index((byte)helloCSharpStrIndex)]));
codes.Add(Code.InvokeVirtual(printlnMethodRefIndex));
codes.Add(new Code(OperationCode.RETURN)); 

var codeBytes = ByteCodeParser.Serialize(codes);

// ...
```

### 凭空构建类 ( 高级 API )

```csharp
var clazz = new Class()
{
    Magic = ClassStruct.ClassMagic,
    MinorVersion = 0,
    MajorVersion = ClassFileVersion.V17,
    AccessFlags = ClassAccessFlags.Public,
    ThisClass = "HelloWorld",
    SuperClass = "java/lang/Object"
};
var helper = clazz.GetConstantPoolHelper();

// 添加常量项
ushort systemClassIndex = helper.NewClass("java/lang/System");
ushort outFieldNameIndex = helper.NewUtf8("out");
ushort outFieldDescriptorIndex = helper.NewUtf8("Ljava/io/PrintStream;");
ushort outNameAndTypeIndex = helper.NewNameAndType(outFieldNameIndex, outFieldDescriptorIndex);
ushort systemOutFieldRefIndex = helper.NewFieldref(systemClassIndex, outNameAndTypeIndex);
ushort printStreamClassIndex = helper.NewClass("java/io/PrintStream");
ushort printlnMethodNameIndex = helper.NewUtf8("println");
ushort printlnDescriptorIndex = helper.NewUtf8("(Ljava/lang/String;)V");
ushort printlnNameAndTypeIndex = helper.NewNameAndType(printlnMethodNameIndex, printlnDescriptorIndex);
ushort printlnMethodRefIndex = helper.NewMethodref(printStreamClassIndex, printlnNameAndTypeIndex);
ushort helloWorldStrIndex = helper.NewString("Hello, World! ");
ushort helloCSharpStrIndex = helper.NewString("Hello, C#! ");

// 创建 "main" 方法
var mainMethod = new Method
{
    AccessFlags = MethodAccessFlags.Public | MethodAccessFlags.Static,
    Name = "main",
    Descriptor = "([Ljava/lang/String;)V"
};

// 构建字节码
var codeBytes = new List<byte>();
// getstatic System.out
codeBytes.Add((byte)OperationCode.GETSTATIC);
codeBytes.AddRange(BitConverter.GetBytes(systemOutFieldRefIndex).Reverse().Take(2));
// ldc "Hello, World! "
codeBytes.Add((byte)OperationCode.LDC);
codeBytes.Add((byte)helloWorldStrIndex);
// invokevirtual PrintStream.println
codeBytes.Add((byte)OperationCode.INVOKEVIRTUAL);
codeBytes.AddRange(BitConverter.GetBytes(printlnMethodRefIndex).Reverse().Take(2));
// getstatic System.out
codeBytes.Add((byte)OperationCode.GETSTATIC);
codeBytes.AddRange(BitConverter.GetBytes(systemOutFieldRefIndex).Reverse().Take(2));
// ldc "Hello, C#! "
codeBytes.Add((byte)OperationCode.LDC);
codeBytes.Add((byte)helloCSharpStrIndex);
// invokevirtual PrintStream.println
codeBytes.Add((byte)OperationCode.INVOKEVIRTUAL);
codeBytes.AddRange(BitConverter.GetBytes(printlnMethodRefIndex).Reverse().Take(2));
// return
codeBytes.Add((byte)OperationCode.RETURN);

// 创建 "Code" 属性
var codeAttribute = new Attribute
{
    Name = "Code",
    Info = new CodeAttributeStruct
    {
        MaxStack = 2,
        MaxLocals = 1,
        Code = codeBytes.ToArray()
    }.ToBytesWithoutIndexAndLength()
};
mainMethod.Attributes.Add(codeAttribute);
clazz.Methods.Add(mainMethod);

// 保存常量池更改
clazz.ConstantPool = helper.ToList();

// 生成字节码文件
var clazzStruct = clazz.ToStruct();
Console.WriteLine(clazzStruct);
var clazzData = ClassParser.Serialize(clazzStruct);
File.WriteAllBytes("HelloWorld.class", clazzData);
return;
```

```text
@Class {
    Magic: 0xCAFEBABE,
    MinorVersion: 0,
    MajorVersion: 61,
    ConstantPoolCount: 24,
    ConstantPool: [
        {
            Tag: 1 (Utf8),
            Info: 0x00106A6176612F6C616E672F53797374656D,
        },
        {
            Tag: 7 (Class),
            Info: 0x0001,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00036F7574,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00154C6A6176612F696F2F5072696E7453747265616D3B,
        },
        {
            Tag: 12 (NameAndType),
            Info: 0x00030004,
        },
        {
            Tag: 9 (Fieldref),
            Info: 0x00020005,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00136A6176612F696F2F5072696E7453747265616D,
        },
        {
            Tag: 7 (Class),
            Info: 0x0007,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00077072696E746C6E,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x0015284C6A6176612F6C616E672F537472696E673B2956,
        },
        {
            Tag: 12 (NameAndType),
            Info: 0x0009000A,
        },
        {
            Tag: 10 (Methodref),
            Info: 0x0008000B,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x000E48656C6C6F2C20576F726C642120,
        },
        {
            Tag: 8 (String),
            Info: 0x000D,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x000B48656C6C6F2C2043232120,
        },
        {
            Tag: 8 (String),
            Info: 0x000F,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x000A48656C6C6F576F726C64,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00106A6176612F6C616E672F4F626A656374,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00046D61696E,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x0016285B4C6A6176612F6C616E672F537472696E673B2956,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x0004436F6465,
        },
        {
            Tag: 7 (Class),
            Info: 0x0011,
        },
        {
            Tag: 7 (Class),
            Info: 0x0012,
        },
    ],
    AccessFlags: 1 ( Public ),
    ThisClass: 22,
    SuperClass: 23,
    InterfacesCount: 0,
    Interfaces: [
    ],
    FieldsCount: 0,
    Fields: [
    ],
    MethodsCount: 1,
    Methods: [
        {
            AccessFlags: 9 ( Public | Static ),
            NameIndex: 19,
            DescriptorIndex: 20,
            AttributesCount: 1,
            Attributes: [
                {
                    AttributeNameIndex: 21,
                    AttributeLength: 29,
                    Info: 0x0002000100000011B20006120EB6000CB200061210B6000CB100000000,
                }
            ],
        },
    ],
    AttributesCount: 0,
    Attributes: [
    ],
}
```

### 凭空构建类

```csharp
var classStruct = new ClassStruct
{
    Magic = ClassStruct.ClassMagic,
    MinorVersion = 0,
    MajorVersion = (ushort)ClassFileVersion.V17,
    AccessFlags = (ushort)ClassAccessFlags.Public,
    ThisClass = 0, // 稍后设置
    SuperClass = 0, // 稍后设置
    InterfacesCount = 0,
    Interfaces = new ushort[0],
    FieldsCount = 0,
    Fields = new FieldInfoStruct[0],
    MethodsCount = 1,
    Methods = new MethodInfoStruct[1],
    AttributesCount = 0,
    Attributes = new AttributeInfoStruct[0]
};

// 初始化常量池列表
var constantPoolList = new List<ConstantPoolInfo>();
var helper = new ConstantPoolHelper(constantPoolList);

// 添加常量项
ushort mainClassUtf8Index = helper.NewUtf8("Main");
ushort objectClassUtf8Index = helper.NewUtf8("java/lang/Object");
ushort mainClassIndex = helper.NewClass(mainClassUtf8Index);
ushort objectClassIndex = helper.NewClass(objectClassUtf8Index);

classStruct.ThisClass = mainClassIndex;
classStruct.SuperClass = objectClassIndex;

ushort mainMethodUtf8Index = helper.NewUtf8("main");
ushort mainDescriptorUtf8Index = helper.NewUtf8("([Ljava/lang/String;)V");
ushort mainNameAndTypeIndex = helper.NewNameAndType(mainMethodUtf8Index, mainDescriptorUtf8Index);

ushort systemClassUtf8Index = helper.NewUtf8("java/lang/System");
ushort systemClassIndex = helper.NewClass(systemClassUtf8Index);
ushort outFieldUtf8Index = helper.NewUtf8("out");
ushort outDescriptorUtf8Index = helper.NewUtf8("Ljava/io/PrintStream;");
ushort outNameAndTypeIndex = helper.NewNameAndType(outFieldUtf8Index, outDescriptorUtf8Index);
ushort systemOutFieldRefIndex = helper.NewFieldref(systemClassIndex, outNameAndTypeIndex);

ushort printStreamClassUtf8Index = helper.NewUtf8("java/io/PrintStream");
ushort printStreamClassIndex = helper.NewClass(printStreamClassUtf8Index);
ushort printlnMethodUtf8Index = helper.NewUtf8("println");
ushort printlnDescriptorUtf8Index = helper.NewUtf8("(Ljava/lang/String;)V");
ushort printlnNameAndTypeIndex = helper.NewNameAndType(printlnMethodUtf8Index, printlnDescriptorUtf8Index);
ushort printlnMethodRefIndex = helper.NewMethodref(printStreamClassIndex, printlnNameAndTypeIndex);

ushort helloWorldUtf8Index = helper.NewUtf8("Hello World");
ushort helloWorldStringIndex = helper.NewString(helloWorldUtf8Index);

// 创建 "Code" 属性
ushort codeAttributeNameIndex = helper.NewUtf8("Code");

using var codeStream = new MemoryStream();
ByteUtils.WriteUInt16(2, codeStream); // max_stack
ByteUtils.WriteUInt16(1, codeStream); // max_locals
ByteUtils.WriteUInt32(10, codeStream); // code_length

// 字节码指令
codeStream.WriteByte(0xB2); // getstatic
ByteUtils.WriteUInt16(systemOutFieldRefIndex, codeStream);
codeStream.WriteByte(0x13); // ldc_w
ByteUtils.WriteUInt16(helloWorldStringIndex, codeStream);
codeStream.WriteByte(0xB6); // invokevirtual
ByteUtils.WriteUInt16(printlnMethodRefIndex, codeStream);
codeStream.WriteByte(0xB1); // return

ByteUtils.WriteUInt16(0, codeStream); // exception_table_length
ByteUtils.WriteUInt16(0, codeStream); // attributes_count

var codeAttributeBytes = codeStream.ToArray();
var codeAttribute = new AttributeInfoStruct
{
    AttributeNameIndex = codeAttributeNameIndex,
    AttributeLength = (uint)codeAttributeBytes.Length,
    Info = codeAttributeBytes
};

// 创建 "main" 方法
var mainMethod = new MethodInfoStruct
{
    AccessFlags = (ushort)(MethodAccessFlags.Public | MethodAccessFlags.Static),
    NameIndex = mainMethodUtf8Index,
    DescriptorIndex = mainDescriptorUtf8Index,
    AttributesCount = 1,
    Attributes = new[] { codeAttribute }
};
classStruct.Methods[0] = mainMethod;

// 设置常量池
classStruct.ConstantPool = helper.ToArray();
classStruct.ConstantPoolCount = helper.ConstantPoolIndexCount;

// 序列化并写入文件
byte[] classData = ClassParser.Serialize(classStruct);
File.WriteAllBytes("Main.class", classData);

Console.WriteLine("Main.class generated successfully.");
return;
```

### 读取并解析类

```csharp
byte[] classData = ...; // 字节码
var clazz = ClassParser.Parse(classData);
Console.WriteLine(clazz);
```

```text
@Class {
    Magic: 0xCAFEBABE,
    MinorVersion: 0,
    MajorVersion: 65,
    ConstantPoolCount: 60,
    ConstantPool: [
        {
            Tag: 10 (Methodref),
            Info: 0x00020003,
        },
        {
            Tag: 7 (Class),
            Info: 0x0004,
        },
        {
            Tag: 12 (NameAndType),
            Info: 0x00050006,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00106A6176612F6C616E672F4F626A656374,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00063C696E69743E,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x0003282956,
        },
        {
            Tag: 9 (Fieldref),
            Info: 0x00080009,
        },
        {
            Tag: 7 (Class),
            Info: 0x000A,
        },
        {
            Tag: 12 (NameAndType),
            Info: 0x000B000C,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00106A6176612F6C616E672F53797374656D,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00036F7574,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00154C6A6176612F696F2F5072696E7453747265616D3B,
        },
        {
            Tag: 8 (String),
            Info: 0x000E,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x001248656C6C6F20616E642077656C636F6D6521,
        },
        {
            Tag: 10 (Methodref),
            Info: 0x00100011,
        },
        {
            Tag: 7 (Class),
            Info: 0x0012,
        },
        {
            Tag: 12 (NameAndType),
            Info: 0x00130014,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00136A6176612F696F2F5072696E7453747265616D,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00067072696E7466,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x003C284C6A6176612F6C616E672F537472696E673B5B4C6A6176612F6C616E672F4F626A6563743B294C6A6176612F696F2F5072696E7453747265616D3B,
        },
        {
            Tag: 18 (InvokeDynamic),
            Info: 0x00000016,
        },
        {
            Tag: 12 (NameAndType),
            Info: 0x00170018,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00176D616B65436F6E63617457697468436F6E7374616E7473,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00152849294C6A6176612F6C616E672F537472696E673B,
        },
        {
            Tag: 10 (Methodref),
            Info: 0x0010001A,
        },
        {
            Tag: 12 (NameAndType),
            Info: 0x001B001C,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00077072696E746C6E,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x0015284C6A6176612F6C616E672F537472696E673B2956,
        },
        {
            Tag: 7 (Class),
            Info: 0x001E,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00044D61696E,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x0004436F6465,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x000F4C696E654E756D6265725461626C65,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00124C6F63616C5661726961626C655461626C65,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x000474686973,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00064C4D61696E3B,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00046D61696E,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x0016285B4C6A6176612F6C616E672F537472696E673B2956,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x000169,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x000149,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x000461726773,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00135B4C6A6176612F6C616E672F537472696E673B,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x000D537461636B4D61705461626C65,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x000A536F7572636546696C65,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00094D61696E2E6A617661,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x0010426F6F7473747261704D6574686F6473,
        },
        {
            Tag: 8 (String),
            Info: 0x002F,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x000569203D2001,
        },
        {
            Tag: 15 (MethodHandle),
            Info: 0x060031,
        },
        {
            Tag: 10 (Methodref),
            Info: 0x00320033,
        },
        {
            Tag: 7 (Class),
            Info: 0x0034,
        },
        {
            Tag: 12 (NameAndType),
            Info: 0x00170035,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00246A6176612F6C616E672F696E766F6B652F537472696E67436F6E636174466163746F7279,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x0098284C6A6176612F6C616E672F696E766F6B652F4D6574686F6448616E646C6573244C6F6F6B75703B4C6A6176612F6C616E672F537472696E673B4C6A6176612F6C616E672F696E766F6B652F4D6574686F64547970653B4C6A6176612F6C616E672F537472696E673B5B4C6A6176612F6C616E672F4F626A6563743B294C6A6176612F6C616E672F696E766F6B652F43616C6C536974653B,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x000C496E6E6572436C6173736573,
        },
        {
            Tag: 7 (Class),
            Info: 0x0038,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00256A6176612F6C616E672F696E766F6B652F4D6574686F6448616E646C6573244C6F6F6B7570,
        },
        {
            Tag: 7 (Class),
            Info: 0x003A,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x001E6A6176612F6C616E672F696E766F6B652F4D6574686F6448616E646C6573,
        },
        {
            Tag: 1 (Utf8),
            Info: 0x00064C6F6F6B7570,
        },
    ],
    AccessFlags: 33 ( Public | Super ),
    ThisClass: 29,
    SuperClass: 2,
    InterfacesCount: 0,
    Interfaces: [
    ],
    FieldsCount: 0,
    Fields: [
    ],
    MethodsCount: 2,
    Methods: [
        {
            AccessFlags: 1 ( Public ),
            NameIndex: 5,
            DescriptorIndex: 6,
            AttributesCount: 1,
            Attributes: [
                {
                    AttributeNameIndex: 31,
                    AttributeLength: 47,
                    Info: 0x00010001000000052AB70001B10000000200200000000600010000000300210000000C000100000005002200230000,
                }
            ],
        },
        {
            AccessFlags: 9 ( Public | Static ),
            NameIndex: 36,
            DescriptorIndex: 37,
            AttributesCount: 1,
            Attributes: [
                {
                    AttributeNameIndex: 31,
                    AttributeLength: 122,
                    Info: 0x0003000200000027B20007120D03BD0002B6000F57043C1B08A30015B200071BBA00150000B60019840101A7FFECB100000003002000000016000500000007000D00090014000C002000090026000E0021000000160002000F001700260027000100000027002800290000002A000000090002FC000F01FA0016,
                }
            ],
        },
    ],
    AttributesCount: 3,
    Attributes: [
        {
            AttributeNameIndex: 43
            AttributeLength: 2,
            Info: 0x002C,
        },
        {
            AttributeNameIndex: 45
            AttributeLength: 8,
            Info: 0x000100300001002E,
        },
        {
            AttributeNameIndex: 54
            AttributeLength: 10,
            Info: 0x000100370039003B0019,
        },
    ],
}
```