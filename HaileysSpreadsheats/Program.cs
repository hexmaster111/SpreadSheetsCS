using System.Xml;
using HaileysSpreadsheats;
using HaileysSpreadsheats.Expr;


DrawList dl = new();
Dictionary<RowCol, Cell> cells = new();
RowCol cursor = new();
ConsoleKeyInfo last = default;
List<RollLogEntry> rollLog = new();

var run = true;
var redraw = true;
var needRedrawAfterMove = true;

while (run)
{
    if (redraw)
    {
        dl.ClearBackground();
        DrawCellsBackground();
        DrawRollLog();
        DrawCellsLabels();
        DrawCellContent();
        redraw = false;
        needRedrawAfterMove = false;
    }


    dl.Move(1, 12);
    dl.DrawText(cursor.ToString());
    if (cells.TryGetValue(cursor, out var currentSelectedCell))
    {
        dl.Move(0, 13);

        if (currentSelectedCell.Kind == Cell.CKind.Expr)
        {
            dl.DrawText(" = " + currentSelectedCell.Expr.Expr);
            needRedrawAfterMove = true;
        }
    }

    Console.CursorVisible = false;
    dl.WriteToConsole();
    // Console.SetCursorPosition(5, 11);
    // Console.Write($"{last.KeyChar}");
    MoveCursorToGridPos_NOW(cursor.Row, cursor.Col);
    Console.CursorVisible = true;

    var key = last = Console.ReadKey();

    if (key.Key == ConsoleKey.Escape) run = false;

    switch (key.KeyChar)
    {
        case '=':
        {
            GetUserValueWithErrorReportingLoop('=');
            break;
        }
    }

    switch (key.Key)
    {
        case ConsoleKey.UpArrow:
            cursor.Row -= 1;
            if (needRedrawAfterMove) redraw = true;
            break;
        case ConsoleKey.DownArrow:
            cursor.Row += 1;
            if (needRedrawAfterMove) redraw = true;
            break;
        case ConsoleKey.LeftArrow:
            cursor.Col -= 1;
            if (needRedrawAfterMove) redraw = true;
            break;
        case ConsoleKey.RightArrow:
            cursor.Col += 1;
            if (needRedrawAfterMove) redraw = true;
            break;

        case ConsoleKey.Enter:
            GetUserValueWithErrorReportingLoop();
            break;

        case ConsoleKey.Delete:
            cells.Remove(cursor);
            redraw = true;
            break;

        case ConsoleKey.R:
            RecomputeAllCells();
            redraw = true;
            break;
    }
}

return;

void DrawRollLog()
{
    int lz = 5 * 12;
    int offset = 0;
    dl.Move(lz, offset++);
    dl.DrawText("Dice Roll Log");
    dl.Move(lz, offset++);

    foreach (var entry in rollLog)
    {
        dl.Move(lz, offset++);
        dl.DrawText($"{entry.When:HH:mm:ss} {entry.Where} {entry.Roll} = {entry.Result}");
    }
}

void GetUserValueWithErrorReportingLoop(char pretype = (char)0)
{
    retry:
    try
    {
        var userGivenCell = GetUserValueForCell(cursor, pretype);
        if (userGivenCell != null)
        {
            cells[cursor] = userGivenCell;
            RecomputeAllCells();
        }
    }
    catch (Exception e)
    {
        Console.SetCursorPosition(3, Console.BufferHeight - 2);
        Console.Write("Invalid Value for cell");
        goto retry;
    }

    redraw = true;
}

Cell? GetCellAt(RowCol pos) => cells.GetValueOrDefault(pos);

double GetCellValue(RowCol pos)
{
    var c = GetCellAt(pos);
    if (c == null) return 0; // Blank cells are 0
    if (c.Kind != Cell.CKind.Number && c.Kind != Cell.CKind.Expr) throw new Exception($"invalid cell kind {c.Kind}");
    if (c.ComputedValue != null) return c.ComputedValue.Value; // computed value is cached
    return c.Number;
}

void RecomputeAllCells()
{
    foreach (var (_, value) in cells)
    {
        value.ComputedValue = null;
    }

    foreach (var (key, value) in cells)
    {
        if (value.Kind == Cell.CKind.Expr)
        {
            value.ComputedValue = CompExpr.Evaluate(value.Expr, GetCellValue, RollTheDice, key);
        }
    }
}

double RollTheDice(Roll spec, RowCol where)
{
    var r = new Random();
    var sum = 0;

    for (int i = 0; i < spec.Rolls; i++)
    {
        int ro = r.Next(1, spec.Sides + 1);
        sum += ro;
    }

    LogRollResults(where, sum, spec);
    return sum;
}

void LogRollResults(RowCol inCell, int resault, Roll roll)
{
    rollLog.Add(new RollLogEntry
    {
        When = DateTime.Now,
        Where = inCell,
        Roll = roll,
        Result = resault
    });

    if (rollLog.Count > 10) rollLog.RemoveAt(0);

    redraw = true;
}

Cell GetCellOrNew(RowCol place) => cells.TryGetValue(place, out var cell) ? cell : new Cell();

Cell? GetUserValueForCell(RowCol pos, char v = (char)0)
{
    Console.SetCursorPosition(3, Console.BufferHeight - 1);
    for (int i = 3; i < Console.BufferWidth; i++)
    {
        Console.Write(' ');
    }

    Console.SetCursorPosition(3, Console.BufferHeight - 1);
    Console.Write(pos + " : ");

    var l = "";

    if (v != 0)
    {
        l += v;
        Console.Write(v);
    }

    while (true)
    {
        var k = Console.ReadKey();
        if (k.Key == ConsoleKey.Escape) return null;
        if (k.Key == ConsoleKey.Backspace && l != "")
        {
            l = l[..^1];
            Console.Write(' ');
            Console.Write('\b');
        }
        else if (k.Key == ConsoleKey.Backspace) Console.Write(' ');
        else if (k.Key == ConsoleKey.Enter) break;
        else if (char.IsBetween(k.KeyChar, (char)32, (char)128)) l += k.KeyChar;
    }

    if (string.IsNullOrEmpty(l)) return null;


    var ret = GetCellOrNew(pos);

    if (double.TryParse(l, out var num))
    {
        ret.Kind = Cell.CKind.Number;
        ret.Number = num;
    }
    else if (l[0] == '=')
    {
        ret.Kind = Cell.CKind.Expr;
        ret.Expr = CompExpr.FromString(l.Substring(1));
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
    row += 2;
    col += 3;
    return (col, row);
}

void DrawCellContent()
{
    foreach (var cell in cells)
    {
        CursorToGridPos(cell.Key.Row, cell.Key.Col);
        dl.DrawText(cell.Value.ContentString());
    }
}

void DrawCellsLabels()
{
    for (int i = 1; i < 6; i++)
    {
        dl.Move((i * 11) - 4, 0);
        dl.DrawText(((char)('A' + i - 1)).ToString());
    }

    for (int i = 1; i < 6; i++)
    {
        dl.Move(0, i * 2);
        dl.DrawText(i.ToString("00"));
    }
}

void DrawCellsBackground(int cols = 5, int rows = 5)
{
    dl.Move(2, 1);
    dl.DrawText("┌");
    for (int i = 0; i < cols; i++)
    {
        dl.DrawText("──────────");

        if (i != cols - 1)
            dl.DrawText("┬");
    }

    dl.DrawText("┐\r\n  ");


    do
    {
        for (int i = 0; i < cols; i++)
        {
            dl.DrawText("│          ");
        }

        dl.DrawText("│\r\n  ");

        if (rows - 1 != 0)
        {
            dl.DrawText("├");
            for (int i = 0; i < cols; i++)
            {
                dl.DrawText("──────────");

                if (i != cols - 1)
                    dl.DrawText("┼");
            }

            dl.DrawText("┤\r\n  ");
        }
    } while ((rows -= 1) != 0);

    dl.DrawText("└");
    for (int i = 0; i < cols; i++)
    {
        dl.DrawText("──────────");
        if (i != cols - 1)
            dl.DrawText("┴");
    }

    dl.DrawText("┘\r\n");
}

public struct RollLogEntry
{
    public DateTime When;
    public RowCol Where;
    public Roll Roll;
    public int Result;
}

namespace HaileysSpreadsheats
{
    public class Cell
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
        public CompiledExpression Expr;

        public double? ComputedValue;

        public string ContentString() => Kind switch
        {
            CKind.Blank => "",
            CKind.Expr => ComputedValue?.ToString("0.00") ?? "!ERROR!",
            CKind.Number => Number.ToString("0.00"),
            CKind.String => Str,
            _ => "INVLD CELL"
        };
    }
}