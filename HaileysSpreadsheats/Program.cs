﻿using HaileysSpreadsheats;
using HaileysSpreadsheats.Expr;


bool run = true;
DrawList dl = new();
Dictionary<RowCol, Cell> cells = new();
RowCol cursor = new();

while (run)
{
    dl.ClearBackground();
    DrawCellsBackground(10, 5);
    DrawCellContent();
    dl.Move(0, 11);
    dl.Drawtext(cursor.ToString());

    if (cells.TryGetValue(cursor, out var currentSelectedCell))
    {
        dl.Move(0, 12);

        if (currentSelectedCell.Kind == Cell.CKind.Expr)
        {
            dl.Drawtext(" = " + currentSelectedCell.Expr.Expr);
        }
    }

    Console.CursorVisible = false;
    dl.WriteToConsole();
    Console.CursorVisible = true;

    MoveCursorToGridPos_NOW(cursor.Row, cursor.Col);

    var key = Console.ReadKey();
    if (key.Key == ConsoleKey.Escape) run = false;

    switch (key.Key)
    {
        case ConsoleKey.UpArrow:
            cursor.Row -= 1;
            break;
        case ConsoleKey.DownArrow:
            cursor.Row += 1;
            break;
        case ConsoleKey.LeftArrow:
            cursor.Col -= 1;
            break;
        case ConsoleKey.RightArrow:
            cursor.Col += 1;
            break;

        case ConsoleKey.Enter:
            var userGivenCell = GetUserValueForCell(cursor);
            if (userGivenCell != null)
            {
                cells[cursor] = userGivenCell;
                RecomputeAllCells(cells);
            }

            break;

        case ConsoleKey.Delete:
            cells.Remove(cursor);
            break;

        case ConsoleKey.R:
            RecomputeAllCells(cells);
            break;
    }
}

return;

Cell? GetCellAt(RowCol pos) => cells.GetValueOrDefault(pos);

double GetCellValue(RowCol pos)
{
    var c = GetCellAt(pos);
    if (c == null) throw new Exception($"invalid cell pos {pos}");
    if (c.Kind != Cell.CKind.Number && c.Kind != Cell.CKind.Expr) throw new Exception($"invalid cell kind {c.Kind}");
    return c.Number;
}

void RecomputeAllCells(Dictionary<RowCol, Cell> allCellsOnSheet)
{
    foreach (var (_, value) in allCellsOnSheet)
    {
        value.ComputedValue = null;
    }

    foreach (var (key, value) in allCellsOnSheet)
    {
        if (value.Kind == Cell.CKind.Expr)
        {
            value.ComputedValue = CompExpr.Evaluate(value.Expr, GetCellValue);
        }
    }
}

Cell GetCell(RowCol place)
{
    return cells.TryGetValue(place, out var cell) ? cell : new Cell();
}

Cell? GetUserValueForCell(RowCol pos)
{
    Console.SetCursorPosition(3, Console.BufferHeight - 1);
    Console.Write(pos.ToString() + " : ");
    var l = Console.ReadLine();
    if (string.IsNullOrEmpty(l)) return null;


    var ret = GetCell(pos);

    if (double.TryParse(l, out var num))
    {
        ret.Kind = Cell.CKind.Number;
        ret.Number = num;
    }
    else if (l[0] == '=')
    {
        ret.Kind = Cell.CKind.Expr;
        ret.Expr = CompExpr.FromString(l.Substring(1)); //Todo : Handle errors
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

    SetCursorPos(0, 0);

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

void SetCursorPos(int x, int y)
{
    dl.Move(x, y);
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