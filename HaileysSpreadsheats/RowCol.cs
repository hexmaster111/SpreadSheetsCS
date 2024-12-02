namespace HaileysSpreadsheats;

public struct RowCol : IEquatable<RowCol>
{
    public int Row, Col;

    public RowCol(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public override int GetHashCode() => HashCode.Combine(Row.GetHashCode(), Col.GetHashCode());


    public bool Equals(RowCol other)
    {
        return Row == other.Row && Col == other.Col;
    }

    public override bool Equals(object? obj)
    {
        return obj is RowCol other && Equals(other);
    }

    public string ToString() => $"{(char)(Col + 'A')}{Row+1}";
    public static RowCol FromCellNotation(string s)
    {
        int col = s[0] - 'A';
        int row = int.Parse(s.Substring(1)) - 1;
        return new RowCol(row, col);
    }
}