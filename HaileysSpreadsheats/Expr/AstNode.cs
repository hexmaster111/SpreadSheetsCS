﻿namespace HaileysSpreadsheats.Expr;

public class AstNode
{
    public TokenKind Kind;
    public double Value;
    public AstNode Left;
    public AstNode Right;

    public override string ToString()
    {
        return Kind switch
        {
            TokenKind.Number => Value.ToString(),
            TokenKind.Plus => $"({Left} + {Right})",
            TokenKind.Minus => $"({Left} - {Right})",
            TokenKind.Mul => $"({Left} * {Right})",
            TokenKind.Div => $"({Left} / {Right})",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}