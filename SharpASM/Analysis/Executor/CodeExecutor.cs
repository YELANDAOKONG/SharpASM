using SharpASM.Models;
using SharpASM.Models.Code;

namespace SharpASM.Analysis.Executor;

public class CodeExecutor
{
    public Class Clazz { get; private set; }
    public List<Code> Codes { get; private set; }

    public CodeExecutor(Class clazz, List<Code> byteCodes)
    {
        Clazz = clazz;
        Codes = byteCodes;
    }
    
    // TODO...
}