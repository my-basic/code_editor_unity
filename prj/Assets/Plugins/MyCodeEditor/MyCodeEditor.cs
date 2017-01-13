using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MyCodeEditor : MonoBehaviour
{
    public Coloring keyword = null;

    public Coloring reserved = null;

    public Coloring symbol = null;

    public Coloring opcode = null;

    public Coloring function = null;

    public Coloring text = null;

    public Coloring comment = null;

    public bool caseSensitive = false;

    public GameObject prefabHead = null;

    public GameObject prefabLine = null;

    public RectTransform transLineRoot = null;

    public int lineCountPerScreen = 18;

    private List<Coloring> colorings = new List<Coloring>();

    private List<MyCodeLine> lines = new List<MyCodeLine>();

    private float textWidth = 0.0f;

    public string Text
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            foreach (MyCodeLine ln in lines)
                sb.AppendLine(ln.Text);

            return sb.ToString();
        }
        set
        {
            Clear();

            string str = value.Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lns = str.Split('\n');
            foreach (string ln in lns)
            {
                Append(ln);
            }
        }
    }

    public int LineCount
    {
        get { return lines.Count; }
    }

    public int[] Selected
    {
        get
        {
            return null;
        }
    }

    private void Start()
    {
        colorings.Clear();
        Coloring[] cs = new Coloring[] { keyword, reserved, symbol, opcode, function };
        foreach (Coloring coloring in cs)
        {
            foreach (string e in coloring.Elements)
            {
                Coloring c = new Coloring();
                c.texts = e;
                c.color = coloring.color;
                colorings.Add(c);
            }
        }
        colorings.Sort((x, y) => y.texts.Length - x.texts.Length);

        textWidth = transLineRoot.GetSize().x;
    }

    private void Relayout(MyCodeLine ln)
    {
        if (ln.TightWidth > textWidth)
        {
            textWidth = ln.TightWidth;
        }

        float h = 0.0f;
        foreach (MyCodeLine l in lines)
        {
            h += l.Rect().GetSize().y;
        }
        transLineRoot.SetSize(new Vector2(textWidth, h));

        foreach (MyCodeLine l in lines)
        {
            l.Width = textWidth;
            l.Relayout();
        }
    }

    private string Color(string str)
    {
        Parts parts = new Parts();

        // Comment.
        foreach (string e in comment.Elements)
        {
            int start = str.IndexOf(e, 0, caseSensitive);
            if (start != -1)
            {
                int end = str.Length - 1;

                string part = str.Substring(start, end - start + 1);
                part = string.Format("<color={1}>{0}</color>", part, comment.ColorHexString);

                parts.Add(new Part(start, end, part));
            }
        }

        // String.
        foreach (string e in text.Elements)
        {
            int start = str.IndexOf(e, 0, caseSensitive);
            while (start != -1)
            {
                int end = str.IndexOf(e, start + 1, caseSensitive);
                if (end != -1)
                {
                    if (!parts.Overlap(start, end))
                    {
                        string part = str.Substring(start, end - start + 1);
                        part = string.Format("<color={1}>{0}</color>", part, text.ColorHexString);

                        parts.Add(new Part(start, end, part));
                    }

                    start = str.IndexOf(e, end + 1, caseSensitive);
                }
            }
        }

        // Tokens.
        foreach (Coloring coloring in colorings)
        {
            foreach (string e in coloring.Elements)
            {
                int start = str.IndexOf(e, 0, caseSensitive);
                while (start != -1)
                {
                    int end = start + e.Length - 1;

                    if (!parts.Overlap(start, end))
                    {
                        string part = str.Substring(start, e.Length);
                        part = string.Format("<color={1}>{0}</color>", part, coloring.ColorHexString);

                        parts.Add(new Part(start, end, part));
                    }

                    start = str.IndexOf(e, end + 1, caseSensitive);
                }
            }
        }

        // Replaces.
        parts.Sort((x, y) => x.Start - y.Start);

        for (int i = parts.Count - 1; i >= 0; --i)
        {
            Part part = parts[i];
            str = str.Remove(part.Start, part.Count);
            str = str.Insert(part.Start, part.Text);
        }

        return str;
    }

    public int Append(string text)
    {
        GameObject objLn = Instantiate(prefabLine, transLineRoot) as GameObject;
        MyCodeLine ln = objLn.GetComponent<MyCodeLine>();
        ln.Height = (float)Screen.height / lineCountPerScreen;
        ln.LineNumber = LineCount;
        ln.SetText(text, Color(text));
        lines.Add(ln);

        Relayout(ln);

        return lines.Count;
    }

    public void Clear()
    {
        foreach (MyCodeLine mcl in lines)
            GameObject.Destroy(mcl.gameObject);
        lines.Clear();

        textWidth = 0.0f;
    }

    public void Select(params int[] indices)
    {
    }

    public void Insert(params int[] indices)
    {
    }

    public void Remove(params int[] indices)
    {
    }
}
