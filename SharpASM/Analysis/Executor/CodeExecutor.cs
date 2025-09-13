using SharpASM.Models;
using SharpASM.Models.Code;
using SharpASM.Models.Struct.Attribute;

namespace SharpASM.Analysis.Executor;

public class CodeExecutor
{
    public Class Clazz { get; private set; }
    public CodeAttributeStruct Code { get; private set; }
    public List<Code> Codes { get; private set; }

    public CodeExecutor(Class clazz, CodeAttributeStruct code, List<Code> byteCodes)
    {
        Clazz = clazz;
        Code = code;
        Codes = byteCodes;
    }
    
    // TODO...
}