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
}