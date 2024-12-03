namespace HaileysSpreadsheats.Expr;

public struct Token
{
    public double NumberValue;
    public RowCol CellPos;

    public TokenKind Kind;
    public Roll DiceRoll;
}


public enum TokenKind
{
    Plus,
    Minus,
    Mul,
    Div,

    Number, // 123
    Cell, // A1
    CellRange, // A1:B2
    DiceRoll, // 1d20

    SKIP, //used by parts of lexer to communicate
    EOF,
}