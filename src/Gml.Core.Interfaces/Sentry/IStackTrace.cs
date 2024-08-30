namespace GmlCore.Interfaces.Sentry;

public interface IStackTrace
{
    public string Filename { get; set; }
    public string Function { get; set; }
    public int Lineno { get; set; }
    public int Colno { get; set; }
    public string AbsPath { get; set; }
    public bool InApp { get; set; }
    public string Package { get; set; }
    public string InstructionAddr { get; set; }
    public string AddrMode { get; set; }
    public string FunctionId { get; set; }
}
