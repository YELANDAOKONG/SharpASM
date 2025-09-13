using System;
using System.IO;
using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Struct.Union;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct.Attribute;

public class StackMapTableAttributeStruct : IAttributeStruct
{
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public StackMapFrameStruct[] Entries { get; set; } = [];

    public byte[] ToBytes()
    {
        return ToStructInfo().ToBytes();
    }

    public byte[] ToBytesWithoutIndexAndLength()
    {
        using (var stream = new MemoryStream())
        {
            ByteUtils.WriteUInt16((ushort)Entries.Length, stream);
            foreach (var entry in Entries)
            {
                var entryBytes = entry.ToBytes();
                stream.Write(entryBytes, 0, entryBytes.Length);
            }
            return stream.ToArray();
        }
    }

    public AttributeInfoStruct ToStructInfo()
    {
        var infoBytes = ToBytesWithoutIndexAndLength();
        return new AttributeInfoStruct
        {
            AttributeNameIndex = AttributeNameIndex,
            AttributeLength = (uint)infoBytes.Length,
            Info = infoBytes
        };
    }

    public static StackMapTableAttributeStruct FromStructInfo(AttributeInfoStruct info)
    {
        var stackMapTableAttr = new StackMapTableAttributeStruct
        {
            AttributeNameIndex = info.AttributeNameIndex,
            AttributeLength = info.AttributeLength
        };

        int offset = 0;
        byte[] bytes = info.Info;

        // Read number_of_entries (2 bytes)
        ushort numberOfEntries = ByteUtils.ReadUInt16(bytes, ref offset);

        // Read entries[number_of_entries]
        stackMapTableAttr.Entries = new StackMapFrameStruct[numberOfEntries];
        for (int i = 0; i < numberOfEntries; i++)
        {
            stackMapTableAttr.Entries[i] = ReadStackMapFrame(bytes, ref offset);
        }

        return stackMapTableAttr;
    }

    private static StackMapFrameStruct ReadStackMapFrame(byte[] bytes, ref int offset)
    {
        var frame = new StackMapFrameStruct();
        byte frameType = bytes[offset++];

        if (frameType >= 0 && frameType <= 63)
        {
            frame.SameFrame = new SameFrameStruct { FrameType = frameType };
        }
        else if (frameType >= 64 && frameType <= 127)
        {
            frame.SameLocals1StackItemFrame = new SameLocals1StackItemFrameStruct
            {
                FrameType = frameType,
                Stack = new VerificationTypeInfoStruct[1]
            };
            frame.SameLocals1StackItemFrame.Stack[0] = ReadVerificationTypeInfo(bytes, ref offset);
        }
        else if (frameType == 247)
        {
            frame.SameLocals1StackItemFrameExtended = new SameLocals1StackItemFrameExtendedStruct
            {
                FrameType = frameType,
                OffsetDelta = ByteUtils.ReadUInt16(bytes, ref offset),
                Stack = new VerificationTypeInfoStruct[1]
            };
            frame.SameLocals1StackItemFrameExtended.Stack[0] = ReadVerificationTypeInfo(bytes, ref offset);
        }
        else if (frameType >= 248 && frameType <= 250)
        {
            frame.ChopFrame = new ChopFrameStruct
            {
                FrameType = frameType,
                OffsetDelta = ByteUtils.ReadUInt16(bytes, ref offset)
            };
        }
        else if (frameType == 251)
        {
            frame.SameFrameExtended = new SameFrameExtendedStruct
            {
                FrameType = frameType,
                OffsetDelta = ByteUtils.ReadUInt16(bytes, ref offset)
            };
        }
        else if (frameType >= 252 && frameType <= 254)
        {
            int localsCount = frameType - 251;
            frame.AppendFrame = new AppendFrameStruct
            {
                FrameType = frameType,
                OffsetDelta = ByteUtils.ReadUInt16(bytes, ref offset),
                Locals = new VerificationTypeInfoStruct[localsCount]
            };
            for (int j = 0; j < localsCount; j++)
            {
                frame.AppendFrame.Locals[j] = ReadVerificationTypeInfo(bytes, ref offset);
            }
        }
        else if (frameType == 255)
        {
            frame.FullFrame = new FullFrameStruct
            {
                FrameType = frameType,
                OffsetDelta = ByteUtils.ReadUInt16(bytes, ref offset),
                NumberOfLocals = ByteUtils.ReadUInt16(bytes, ref offset),
                Locals = new VerificationTypeInfoStruct[0],
                NumberOfStackItems = ByteUtils.ReadUInt16(bytes, ref offset),
                Stack = new VerificationTypeInfoStruct[0]
            };

            frame.FullFrame.Locals = new VerificationTypeInfoStruct[frame.FullFrame.NumberOfLocals];
            for (int j = 0; j < frame.FullFrame.NumberOfLocals; j++)
            {
                frame.FullFrame.Locals[j] = ReadVerificationTypeInfo(bytes, ref offset);
            }

            frame.FullFrame.Stack = new VerificationTypeInfoStruct[frame.FullFrame.NumberOfStackItems];
            for (int j = 0; j < frame.FullFrame.NumberOfStackItems; j++)
            {
                frame.FullFrame.Stack[j] = ReadVerificationTypeInfo(bytes, ref offset);
            }
        }
        else
        {
            throw new InvalidOperationException($"Invalid frame type: {frameType}");
        }

        return frame;
    }

    private static VerificationTypeInfoStruct ReadVerificationTypeInfo(byte[] bytes, ref int offset)
    {
        var info = new VerificationTypeInfoStruct();
        byte tag = bytes[offset++];

        switch (tag)
        {
            case 0: // ITEM_Top
                info.TopVariableInfo = new TopVariableInfoStruct { Tag = tag };
                break;
            case 1: // ITEM_Integer
                info.IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = tag };
                break;
            case 2: // ITEM_Float
                info.FloatVariableInfo = new FloatVariableInfoStruct { Tag = tag };
                break;
            case 3: // ITEM_Double
                info.DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = tag };
                break;
            case 4: // ITEM_Long
                info.LongVariableInfo = new LongVariableInfoStruct { Tag = tag };
                break;
            case 5: // ITEM_Null
                info.NullVariableInfo = new NullVariableInfoStruct { Tag = tag };
                break;
            case 6: // ITEM_UninitializedThis
                info.UninitializedThisVariableInfo = new UninitializedThisVariableInfoStruct { Tag = tag };
                break;
            case 7: // ITEM_Object
                info.ObjectVariableInfo = new ObjectVariableInfoStruct
                {
                    Tag = tag,
                    CPoolIndex = ByteUtils.ReadUInt16(bytes, ref offset)
                };
                break;
            case 8: // ITEM_Uninitialized
                info.UninitializedVariableInfo = new UninitializedVariableInfoStruct
                {
                    Tag = tag,
                    Offset = ByteUtils.ReadUInt16(bytes, ref offset)
                };
                break;
            default:
                throw new InvalidOperationException($"Invalid verification type tag: {tag}");
        }

        return info;
    }
}
