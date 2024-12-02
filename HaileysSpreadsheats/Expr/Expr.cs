namespace HaileysSpreadsheats.Expr;

public struct Expression
{
    public Expression(AstNode root)
    {
        Root = root;
        Expr = root.ToString();
    }

    public AstNode Root;
    public string Expr;
}


public static class CompExpr
{
    public static Expression FromString(string expr)
    {
        Lexer l = new(expr);
        Parser p = new(l);
        var root = p.GetRoot();
        return new Expression( root);
    }
}