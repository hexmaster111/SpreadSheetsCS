namespace HaileysSpreadsheats.Expr;

public struct Token
{
    public double Value;
    public TokenKind Kind;
}

public enum TokenKind
{
    Plus,
    Minus,
    Mul,
    Div,

    Number,
    SKIP, //used by parts of lexer to communicate
    EOF,
}