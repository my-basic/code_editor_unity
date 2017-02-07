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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Coloring
{
    [Multiline]
    public string texts = null;

    public Color color = Color.black;

    private string[] elements = null;
    public string[] Elements
    {
        get
        {
            if (elements == null)
            {
                elements =
                (
                    from t in texts.Split(';')
                    where !string.IsNullOrEmpty(t.Trim())
                    select t.Trim()
                ).ToArray();
            }

            return elements;
        }
    }

    private string colorHexString = null;
    public string ColorHexString
    {
        get
        {
            if (colorHexString == null)
            {
                uint hex = 0;
                hex |= (uint)(color.r * 255) << 24;
                hex |= (uint)(color.g * 255) << 16;
                hex |= (uint)(color.b * 255) << 8;
                hex |= 0xff;

                colorHexString = "#" + hex.ToString("X8");
            }

            return colorHexString;
        }
    }
}

public class Part
{
    public int Start { get; private set; }

    public int End { get; private set; }

    public int Count { get { return End - Start + 1; } }

    public string Text { get; private set; }

    public Part(int start, int end, string text)
    {
        Start = start;
        End = end;
        Text = text;
    }

    public bool Overlap(int start, int end)
    {
        if (start >= Start && start <= End)
            return true;
        if (end >= Start && end <= End)
            return true;
        if (start <= Start && end >= End)
            return true;

        return false;
    }
}

public class Parts : List<Part>
{
    public bool Overlap(int start, int end)
    {
        foreach(Part part in this)
        {
            if (part.Overlap(start, end))
                return true;
        }

        return false;
    }
}

internal static class StringExtensions
{
    public static int IndexOf(this string str, string sub, int start, bool caseSensitive)
    {
        if (caseSensitive)
            return str.IndexOf(sub, start);

        return str.ToLower().IndexOf(sub.ToLower(), start);
    }
}

internal static class RectTransformExtensions
{
    public static void SetDefaultScale(this RectTransform trans)
    {
        trans.localScale = new Vector3(1, 1, 1);
    }

    public static void SetPivotAndAnchors(this RectTransform trans, Vector2 aVec)
    {
        trans.pivot = aVec;
        trans.anchorMin = aVec;
        trans.anchorMax = aVec;
    }

    public static Vector2 GetSize(this RectTransform trans)
    {
        return trans.rect.size;
    }

    public static float GetWidth(this RectTransform trans)
    {
        return trans.rect.width;
    }

    public static float GetHeight(this RectTransform trans)
    {
        return trans.rect.height;
    }

    public static void SetPositionOfPivot(this RectTransform trans, Vector2 newPos)
    {
        trans.localPosition = new Vector3(newPos.x, newPos.y, trans.localPosition.z);
    }

    public static void SetLeftPosition(this RectTransform trans, float newPos)
    {
        trans.localPosition = new Vector3(newPos + (trans.pivot.x * trans.rect.width), trans.localPosition.y, trans.localPosition.z);
    }

    public static void SetRightPosition(this RectTransform trans, float newPos)
    {
        trans.localPosition = new Vector3(newPos - ((1f - trans.pivot.x) * trans.rect.width), trans.localPosition.y, trans.localPosition.z);
    }

    public static void SetLeftBottomPosition(this RectTransform trans, Vector2 newPos)
    {
        trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
    }

    public static void SetLeftTopPosition(this RectTransform trans, Vector2 newPos)
    {
        trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
    }

    public static void SetRightBottomPosition(this RectTransform trans, Vector2 newPos)
    {
        trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
    }

    public static void SetRightTopPosition(this RectTransform trans, Vector2 newPos)
    {
        trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
    }

    public static void SetSize(this RectTransform trans, Vector2 newSize)
    {
        Vector2 oldSize = trans.rect.size;
        Vector2 deltaSize = newSize - oldSize;
        trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
        trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
    }

    public static void SetWidth(this RectTransform trans, float newSize)
    {
        SetSize(trans, new Vector2(newSize, trans.rect.size.y));
    }

    public static void SetHeight(this RectTransform trans, float newSize)
    {
        SetSize(trans, new Vector2(trans.rect.size.x, newSize));
    }

    public static RectTransform Rect(this MyCodeEditor ui)
    {
        return ui.GetComponent<RectTransform>();
    }

    public static RectTransform Rect(this MyCodeHead ui)
    {
        return ui.GetComponent<RectTransform>();
    }

    public static RectTransform Rect(this MyCodeLine ui)
    {
        return ui.GetComponent<RectTransform>();
    }

    public static RectTransform Rect(this ScrollRect ui)
    {
        return ui.GetComponent<RectTransform>();
    }

    public static RectTransform Rect(this Scrollbar ui)
    {
        return ui.GetComponent<RectTransform>();
    }

    public static RectTransform Rect(this Button ui)
    {
        return ui.GetComponent<RectTransform>();
    }

    public static RectTransform Rect(this Toggle ui)
    {
        return ui.GetComponent<RectTransform>();
    }

    public static RectTransform Rect(this InputField ui)
    {
        return ui.GetComponent<RectTransform>();
    }

    public static RectTransform Rect(this Text ui)
    {
        return ui.GetComponent<RectTransform>();
    }
}
