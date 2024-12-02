namespace HaileysSpreadsheats;

public class DrawList()
{
    private readonly List<Terminst> insts = new();
    public void Drawtext(string s) => insts.Add(new Terminst { str = s, kind = Terminst.Tkind.drawtext });
    public void SetForgound(ConsoleColor c) => insts.Add(new Terminst { kind = Terminst.Tkind.change_color_fg, col = c });

    public void SetBackground(ConsoleColor c) => insts.Add(new Terminst { kind = Terminst.Tkind.change_color_bg, col = c });

    public void Move(int l, int t) => insts.Add(new Terminst { l = l, t = t, kind = Terminst.Tkind.move });

    public void ClearBackground() => insts.Add(new Terminst { kind = Terminst.Tkind.clear });

    public void WriteToConsole()
    {
        foreach (var i in insts)
        {
            switch (i.kind)
            {
                case Terminst.Tkind.drawtext: Console.Write(i.str); break;
                case Terminst.Tkind.change_color_bg: Console.BackgroundColor = i.col; break;
                case Terminst.Tkind.change_color_fg: Console.ForegroundColor = i.col; break;
                case Terminst.Tkind.clear: Console.Clear(); break;
                case Terminst.Tkind.move: Console.SetCursorPosition(i.l, i.t); break;
                default: throw new Exception("Oopse !");
            }
        }
    }
    struct Terminst
    {
        public enum Tkind { drawtext, move, clear, change_color_fg, change_color_bg };
        public Tkind kind;
        public int l, t;
        public string str;
        public ConsoleColor col;
    };
}