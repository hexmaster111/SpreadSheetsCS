namespace HaileysSpreadsheats;

public struct RowCol(int row, int col) : IEquatable<RowCol>
{
    public int Row = row, Col = col;

    public override int GetHashCode() => HashCode.Combine(Row.GetHashCode(), Col.GetHashCode());


    public bool Equals(RowCol other)
    {
        return Row == other.Row && Col == other.Col;
    }

    public override bool Equals(object? obj)
    {
        return obj is RowCol other && Equals(other);
    }

    public override string ToString() => $"{(char)(Col + 'A')}{Row + 1}";
    public static bool operator ==(RowCol left, RowCol right) => left.Equals(right);
    public static bool operator !=(RowCol left, RowCol right) => !(left == right);

    public static RowCol FromCellNotation(string s)
    {
        s = s.ToUpper();
        int col = s[0] - 'A';
        int row = int.Parse(s.Substring(1)) - 1;
        return new RowCol(row, col);
    }
}