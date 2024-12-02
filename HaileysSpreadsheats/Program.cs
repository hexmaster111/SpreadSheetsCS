using HaileysSpreadsheats;

ExpressionCell.CompileExpression("51 + 11");
ExpressionCell.CompileExpression("5 + 11 * 2");
ExpressionCell.CompileExpression("(5 + 11) * 2");
ExpressionCell.CompileExpression("5 + 11 / 2");
ExpressionCell.CompileExpression("5 + 11 - 2");

bool run = true;
DrawList dl = new();

// List<Cell> cells = new();
Dictionary<RowCol, Cell> cells = new();

int currsorRow = 0, currsorCol = 0;

while (run)
{
    dl.ClearBackground();
    DrawCellsBackground(10, 5);
    DrawCellContent();
    dl.Move(0, 10);
    dl.Drawtext(CellName(currsorRow, currsorCol));

    Console.CursorVisible = false;
    dl.WriteToConsole();
    Console.CursorVisible = true;

    MoveCursorToGridPos_NOW(currsorRow, currsorCol);

    var key = Console.ReadKey();
    if (key.Key == ConsoleKey.Escape) run = false;

    switch (key.Key)
    {
        case ConsoleKey.UpArrow:
            currsorRow -= 1;
            break;
        case ConsoleKey.DownArrow:
            currsorRow += 1;
            break;
        case ConsoleKey.LeftArrow:
            currsorCol -= 1;
            break;
        case ConsoleKey.RightArrow:
            currsorCol += 1;
            break;
        case ConsoleKey.Enter:
            var userGivenCell = GetUserValueForCell(currsorRow, currsorCol);
            if (userGivenCell != null)
                cells[new RowCol() { Col = currsorCol, Row = currsorRow }] = userGivenCell.Value;
            else cells[new RowCol() { Col = currsorCol, Row = currsorRow }] = default;


            break;
    }
}

return;

Cell GetCell(int row, int col)
{
    var place = new RowCol() { Row = row, Col = col };
    return cells.TryGetValue(place, out var cell) ? cell : new Cell();
}

string CellName(int row, int col) => $"{(char)(col + 'A')}{row}";

Cell? GetUserValueForCell(int row, int col)
{
    Console.SetCursorPosition(3, Console.BufferHeight);
    Console.Write(CellName(row, col) + " = ");
    var l = Console.ReadLine();
    if (string.IsNullOrEmpty(l)) return null;


    var ret = GetCell(row, col);

    if (double.TryParse(l, out var num))
    {
        ret.Kind = Cell.CKind.Number;
        ret.Number = num;
    }
    else if (l[0] == '=')
    {
        ret.Kind = Cell.CKind.Expr;
    }
    else
    {
        ret.Kind = Cell.CKind.String;
        ret.Str = l;
    }

    return ret;
}

void MoveCursorToGridPos_NOW(int row, int col)
{
    var (l, t) = ToCellPos(row, col);
    Console.SetCursorPosition(l, t);
}

void CursorToGridPos(int row, int col)
{
    var (l, t) = ToCellPos(row, col);
    dl.Move(l, t);
}

(int l, int t) ToCellPos(int row, int col)
{
    row *= 2;
    col *= 11;
    row += 1;
    col += 1;
    return (col, row);
}

void DrawCellContent()
{
    foreach (var cell in cells)
    {
        CursorToGridPos(cell.Key.Row, cell.Key.Col);
        DrawText(cell.Value.ContentString());
    }
}

void DrawCellsBackground(int cols, int rows)
{
    /*    ┼ ┴ ┬ ┤ ├ ┘ └ ┐ ┌ │ ─
     *    ┌──────────┬──────────┬──────────┐
     *    │          │          │          │
     *    ├──────────┼──────────┼──────────┤
     *    │          │          │          │
     *    ├──────────┼──────────┼──────────┤
     *    │          │          │          │
     *    └──────────┴──────────┴──────────┘
     */

    SetCurrsorPos(0, 0);

    DrawText("┌");
    for (int i = 0; i < cols; i++)
    {
        DrawText("──────────");

        if (i != cols - 1)
            DrawText("┬");
    }

    DrawText("┐\r\n");

    do
    {
        for (int i = 0; i < cols; i++)
        {
            DrawText("│          ");
        }

        DrawText("│\r\n");

        if (rows - 1 != 0)
        {
            DrawText("├");
            for (int i = 0; i < cols; i++)
            {
                DrawText("──────────");

                if (i != cols - 1)
                    DrawText("┼");
            }

            DrawText("┤\r\n");
        }
    } while ((rows -= 1) != 0);

    DrawText("└");
    for (int i = 0; i < cols; i++)
    {
        DrawText("──────────");
        if (i != cols - 1)
            DrawText("┴");
    }

    DrawText("┘\r\n");
}


void DrawText(string text)
{
    dl.Drawtext(text);
}

void SetCurrsorPos(int x, int y)
{
    dl.Move(x, y);
}


public struct Cell
{
    public enum CKind
    {
        Blank,
        Expr,
        Number,
        String
    }

    public CKind Kind;
    public string Str;
    public double Number;
    public object Expr;

    public string ContentString() => Kind switch
    {
        CKind.Blank => "",
        CKind.Expr => "TODO",
        CKind.Number => Number.ToString("0.00"),
        CKind.String => Str,
        _ => "INVLD CELL"
    };
}


struct RowCol
{
    public int Row, Col;
    public override int GetHashCode() => HashCode.Combine(Row.GetHashCode(), Col.GetHashCode());
}