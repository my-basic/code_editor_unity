/*
** This source file is part of MY-BASIC Code Editor (Unity)
**
** For the latest info, see https://github.com/my-basic/my_basic_code_editor_unity/
**
** Copyright (C) 2017 Wang Renxin
**
** Permission is hereby granted, free of charge, to any person obtaining a copy of
** this software and associated documentation files (the "Software"), to deal in
** the Software without restriction, including without limitation the rights to
** use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
** the Software, and to permit persons to whom the Software is furnished to do so,
** subject to the following conditions:
**
** The above copyright notice and this permission notice shall be included in all
** copies or substantial portions of the Software.
**
** THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
** IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
** FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
** COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
** IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
** CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MyCodeEditor : MonoBehaviour
{
    public Coloring keyword = null;

    public Coloring reserved = null;

    public Coloring symbol = null;

    public Coloring opcode = null;

    public Coloring function = null;

    public Coloring text = null;

    public Coloring comment = null;

    public Coloring discarded = null;

    public bool caseSensitive = false;

    public GameObject prefabHead = null;

    public GameObject prefabLine = null;

    public ScrollRect scrollHeadCol = null;

    public ScrollRect scrollLineCol = null;

    public RectTransform transHeadRoot = null;

    public RectTransform transLineRoot = null;

    public MyCodeInput codeInput = null;

    public int lineCountPerScreen = 18;

    public Material fontMaterial = null;

    private List<Coloring> colorings = new List<Coloring>();

    private List<MyCodeHead> heads = new List<MyCodeHead>();

    private List<MyCodeLine> lines = new List<MyCodeLine>();

    private List<string> userFuncs = new List<string>();

    private float headNumberWidth = 0.0f;

    private float lineTextWidth = 0.0f;

    public string Text
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lines.Count; ++i)
            {
                MyCodeLine ln = lines[i];
                if (i == lines.Count - 1 && string.IsNullOrEmpty(ln.Text))
                    continue;
                sb.AppendLine(ln.Text);
            }

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

    public IEnumerable<int> Selected
    {
        get
        {
            List<int> lst = new List<int>();
            for (int i = 0; i < LineCount; ++i)
            {
                MyCodeHead h = heads[i];
                if (h.toggleSelection.isOn)
                    lst.Add(i);
            }

            return lst;
        }
    }

    public IEnumerable<string> SelectedText
    {
        get
        {
            foreach (int i in Selected)
            {
                MyCodeLine ln = lines[i];

                yield return ln.Text;
            }
        }
    }

    public Action<int> HeadClicked = null;

    private void Start()
    {
        fontMaterial.mainTexture.filterMode = FilterMode.Point;

        RefillColoring();
    }

    private void OnDestroy()
    {
        Clear();
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

        // Discarded.
        foreach (string e in discarded.Elements)
        {
            int start = str.IndexOf(e, 0, caseSensitive);
            while (start != -1)
            {
                int end = start + e.Length - 1;

                if (!parts.Overlap(start, end))
                {
                    string part = str.Substring(start, e.Length);
                    part = string.Format("<color={1}>{0}</color>", part, discarded.ColorHexString);

                    parts.Add(new Part(start, end, part));
                }

                start = str.IndexOf(e, end + 1, caseSensitive);
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

    private void RefillColoring()
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
        foreach (string f in userFuncs)
        {
            Coloring c = new Coloring();
            c.texts = f;
            c.color = function.color;
            colorings.Add(c);
        }
        colorings.Sort((x, y) => y.texts.Length - x.texts.Length);
    }

    public void AddFunction(string func)
    {
        userFuncs.Add(func);
    }

    public void AddFunction(IEnumerable<string> funcs)
    {
        userFuncs.AddRange(funcs);
    }

    public void ClearFunction()
    {
        userFuncs.Clear();
    }

    private void EnsureLastLineEditable()
    {
        if (LineCount == 0 || !string.IsNullOrEmpty(lines.Last().Text))
        {
            Append(string.Empty);
        }
    }

    private IEnumerator RelayoutProc(bool reserveScrollValue)
    {
        yield return new WaitForEndOfFrame();

        float height = MyCodeLine.Y_OFFSET * 2.0f;

        // Heads.
        headNumberWidth = 0.0f;
        foreach (MyCodeHead h in heads)
        {
            height += h.Rect().GetSize().y;
            if (h.TightWidth > headNumberWidth)
                headNumberWidth = h.TightWidth;
        }
        scrollHeadCol.Rect().offsetMin = new Vector2(0.0f, scrollHeadCol.Rect().offsetMin.y);
        scrollHeadCol.Rect().SetSize(new Vector2(headNumberWidth + MyCodeHead.X_OFFSET * 2.0f, scrollHeadCol.Rect().GetSize().y));
        transHeadRoot.SetSize(new Vector2(headNumberWidth + MyCodeHead.X_OFFSET * 2.0f, height));
        transHeadRoot.SetLeftPosition(0.0f);

        foreach (MyCodeHead h in heads)
        {
            h.Width = headNumberWidth;
            h.Relayout();
        }
        yield return new WaitForEndOfFrame();

        // Lines.
        Vector2 locPos = transLineRoot.localPosition;
        float oldVal = scrollLineCol.verticalScrollbar.value;
        scrollLineCol.Rect().offsetMin = new Vector2(scrollHeadCol.Rect().GetSize().x, scrollLineCol.Rect().offsetMin.y);
        scrollLineCol.Rect().SetSize(new Vector2(this.Rect().GetSize().x - scrollHeadCol.Rect().GetSize().x, scrollLineCol.Rect().GetSize().y));

        lineTextWidth = scrollLineCol.Rect().GetSize().x - 5;
        foreach (MyCodeLine l in lines)
        {
            if (l.TightWidth > lineTextWidth)
                lineTextWidth = l.TightWidth;
        }
        transLineRoot.SetSize(new Vector2(lineTextWidth + MyCodeLine.X_OFFSET * 2.0f, height));
        transLineRoot.SetLeftPosition(scrollLineCol.verticalScrollbar.Rect().GetSize().x / 2.0f);
        yield return new WaitForEndOfFrame();

        foreach (MyCodeLine l in lines)
        {
            l.Width = lineTextWidth;
            l.Relayout();
        }
        yield return new WaitForEndOfFrame();

        transLineRoot.localPosition = locPos;

        if (reserveScrollValue)
        {
            scrollLineCol.verticalScrollbar.value = 1.0f;
            yield return new WaitForEndOfFrame();
            scrollLineCol.verticalScrollbar.value = oldVal;
        }
        else
        {
            transLineRoot.localPosition = locPos;
        }
    }

    public void Relayout(bool toBottom = false)
    {
        EnsureLastLineEditable();

        StartCoroutine(RelayoutProc(toBottom));
    }

    public int Append(string text)
    {
        GameObject objLn = Instantiate(prefabLine, transLineRoot) as GameObject;
        MyCodeLine ln = objLn.GetComponent<MyCodeLine>();
        ln.Height = (float)Screen.height / lineCountPerScreen;
        ln.LineNumber = LineCount;
        ln.SetText(text, Color(text));
        ln.LineClicked += OnLineClicked;
        lines.Add(ln);

        GameObject objHd = Instantiate(prefabHead, transHeadRoot) as GameObject;
        MyCodeHead hd = objHd.GetComponent<MyCodeHead>();
        hd.Height = (float)Screen.height / lineCountPerScreen;
        hd.LineNumber = ln.LineNumber;
        hd.HeadClicked += OnHeadClicked;
        heads.Add(hd);

        return lines.Count;
    }

    public bool Insert(int index)
    {
        if (index < 0 || index >= LineCount)
            return false;

        string text = string.Empty;

        GameObject objLn = Instantiate(prefabLine, transLineRoot) as GameObject;
        MyCodeLine ln = objLn.GetComponent<MyCodeLine>();
        ln.Height = (float)Screen.height / lineCountPerScreen;
        ln.LineNumber = index;
        ln.SetText(text, Color(text));
        ln.LineClicked += OnLineClicked;
        lines.Insert(index, ln);

        GameObject objHd = Instantiate(prefabHead, transHeadRoot) as GameObject;
        MyCodeHead hd = objHd.GetComponent<MyCodeHead>();
        hd.Height = (float)Screen.height / lineCountPerScreen;
        hd.LineNumber = ln.LineNumber;
        hd.HeadClicked += OnHeadClicked;
        heads.Insert(index, hd);

        for (int i = index + 1; i < LineCount; ++i)
        {
            ++lines[i].LineNumber;
            ++heads[i].LineNumber;
        }

        return true;
    }

    public int Insert(IEnumerable<int> indices)
    {
        int n = 0;
        foreach (int i in indices)
        {
            Insert(i + n);
            ++n;
        }

        Relayout();

        return n;
    }

    public bool Remove(int index)
    {
        if (index < 0 || index >= LineCount)
            return false;

        GameObject.Destroy(lines[index].gameObject);
        GameObject.Destroy(heads[index].gameObject);
        lines.RemoveAt(index);
        heads.RemoveAt(index);
        for (int i = index; i < LineCount; ++i)
        {
            --lines[i].LineNumber;
            --heads[i].LineNumber;
        }

        return true;
    }

    public int Remove(IEnumerable<int> indices)
    {
        int n = 0;
        foreach (int i in indices)
        {
            Remove(i - n);
            ++n;
        }

        Relayout(true);

        return n;
    }

    public void Clear()
    {
        foreach (MyCodeHead hd in heads)
            GameObject.Destroy(hd.gameObject);
        heads.Clear();

        foreach (MyCodeLine mcl in lines)
            GameObject.Destroy(mcl.gameObject);
        lines.Clear();

        headNumberWidth = 0.0f;
        lineTextWidth = 0.0f;
    }

    public bool Unselect(int index)
    {
        if (index < 0 || index >= LineCount)
            return false;

        MyCodeHead h = heads[index];
        h.toggleSelection.isOn = false;

        return true;
    }

    public void Unselect(IEnumerable<int> indices)
    {
        foreach (int i in indices)
        {
            if (i >= 0 && i < LineCount)
            {
                MyCodeHead h = heads[i];
                h.toggleSelection.isOn = false;
            }
        }
    }

    public void Select(int index)
    {
        if (index >= 0 && index < LineCount)
        {
            MyCodeHead h = heads[index];
            h.toggleSelection.isOn = true;
        }
    }

    public void Select(IEnumerable<int> indices)
    {
        foreach (int i in indices)
        {
            if (i >= 0 && i < LineCount)
            {
                MyCodeHead h = heads[i];
                h.toggleSelection.isOn = true;
            }
        }
    }

    public void ScrollToLine(int index)
    {
        if (index < 0) index = 0;
        else if (index >= LineCount) index = LineCount - 1;

        float val = 1.0f - (float)index / LineCount;
        scrollLineCol.verticalScrollbar.value = val;
    }

    public void OnScrollLineColValueChanged(Vector2 val)
    {
        scrollHeadCol.verticalScrollbar.value = val.y;

        if (codeInput.Shown)
            codeInput.Hide();
    }

    private void OnLineClicked(MyCodeLine ln)
    {
        codeInput.Show(ln);
    }

    private void OnHeadClicked(MyCodeHead hd)
    {
        if (HeadClicked != null)
            HeadClicked(hd.LineNumber);
    }

    public void OnInputEdited()
    {
        bool relayout = false;

        if (codeInput.Editing != null)
        {
            codeInput.Editing.SetText(codeInput.FieldInput.text, Color(codeInput.FieldInput.text));

            relayout = codeInput.Editing == lines.Last();
        }

        codeInput.Hide();

        if (relayout)
        {
            Relayout(true);
        }
    }
}
