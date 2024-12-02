namespace HaileysSpreadsheats.Expr;

public struct CompiledExpression(string expr, List<CompExprOp> ops)
{
    public readonly string Expr = expr;
    public readonly List<CompExprOp> Ops = ops;
}

public struct CompExprOp
{
    public enum Op
    {
        Plus,
        Minus,
        Mul,
        Div,

        PushNumber,
        PushCell,
    }

    public Op Kind;
    public double Number;
    public RowCol Cell;
}

public static class CompExpr
{
    public delegate double CellLookup(RowCol cell);

    public static double Evaluate(CompiledExpression expr, CellLookup cellLookup)
    {
        Stack<double> stack = new();
        foreach (var op in expr.Ops)
        {
            switch (op.Kind)
            {
                case CompExprOp.Op.Plus:
                {
                    var a = stack.Pop();
                    var b = stack.Pop();
                    stack.Push(a + b);
                    break;
                }
                case CompExprOp.Op.Minus:
                {
                    var a = stack.Pop();
                    var b = stack.Pop();
                    stack.Push(a - b);
                    break;
                }
                case CompExprOp.Op.Mul:
                {
                    var a = stack.Pop();
                    var b = stack.Pop();
                    stack.Push(a * b);
                    break;
                }
                case CompExprOp.Op.Div:
                {
                    var a = stack.Pop();
                    var b = stack.Pop();
                    stack.Push(a / b);
                    break;
                }
                case CompExprOp.Op.PushNumber:
                    stack.Push(op.Number);
                    break;
                case CompExprOp.Op.PushCell:
                    stack.Push(cellLookup(op.Cell));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return stack.Pop();
    }

    public static CompiledExpression FromString(string expr)
    {
        Lexer l = new(expr);
        Parser p = new(l);
        AstNode root = p.GetRoot();
        Compiler c = new(root);
        var res = c.Compile();
        if (res.error != null) throw new Exception(res.error);
        return new CompiledExpression(expr, res.output!);
    }
}