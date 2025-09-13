namespace SharpASM.Models.Struct.Union;

public class StackMapFrameStruct
{
    /*
     * union stack_map_frame {
           same_frame;
           same_locals_1_stack_item_frame;
           same_locals_1_stack_item_frame_extended;
           chop_frame;
           same_frame_extended;
           append_frame;
           full_frame;
       }
     */

    public SameFrameStruct? SameFrame { get; set; } = null;
    public SameLocals1StackItemFrameStruct? SameLocals1StackItemFrame { get; set; } = null;
    public SameLocals1StackItemFrameExtendedStruct? SameLocals1StackItemFrameExtended { get; set; } = null;
    public ChopFrameStruct? ChopFrame { get; set; } = null;
    public SameFrameExtendedStruct? SameFrameExtended { get; set; } = null;
    public AppendFrameStruct? AppendFrame { get; set; } = null;
    public FullFrameStruct? FullFrame { get; set; } = null;
    
    public byte[] ToBytes()
    {
        if (SameFrame != null)
        {
            return SameFrame.ToBytes();
        }
        if (SameLocals1StackItemFrame != null)
        {
            return SameLocals1StackItemFrame.ToBytes();
        }
        if (SameLocals1StackItemFrameExtended != null)
        {
            return SameLocals1StackItemFrameExtended.ToBytes();
        }
        if (ChopFrame != null)
        {
            return ChopFrame.ToBytes();
        }
        if (SameFrameExtended != null)
        {
            return SameFrameExtended.ToBytes();
        }
        if (AppendFrame != null)
        {
            return AppendFrame.ToBytes();
        }
        if (FullFrame != null)
        {
            return FullFrame.ToBytes();
        }
        throw new InvalidOperationException("No stack map frame set");
    }
}