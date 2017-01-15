/*
** This source file is part of MY-BASIC Code Editor (Unity)
**
** For the latest info, see https://github.com/paladin-t/my_basic_code_editor_unity/
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
using UnityEngine;
using UnityEngine.UI;

public class MyCodeLine : MonoBehaviour
{
    private const float TEXT_OCCUPATION = 0.8f;

    public const float X_OFFSET = 2.0f;
    public const float Y_OFFSET = 2.0f;

    public Button buttonCode = null;
    public Text textCode = null;

    private int lineNumber = 0;
    public int LineNumber
    {
        get
        {
            return lineNumber;
        }
        set
        {
            lineNumber = value;

            this.Rect().SetLeftTopPosition(new Vector2(X_OFFSET, -Y_OFFSET - Height * lineNumber));
        }
    }

    public string Text { get; private set; }

    private float height = 0.0f;
    public float Height
    {
        get
        {
            return height;
        }
        set
        {
            height = value;

            int fontSize = (int)(height * TEXT_OCCUPATION);
            textCode.fontSize = fontSize;

            this.Rect().SetHeight(height);
            buttonCode.Rect().SetHeight(height);
        }
    }

    public float Width
    {
        get
        {
            return this.Rect().GetSize().x;
        }
        set
        {
            this.Rect().SetSize(new Vector2(value, this.Rect().GetSize().y));
        }
    }

    public float TightWidth
    {
        get
        {
            float result = TextWidth;
            result += Mathf.Abs(textCode.Rect().offsetMin.x);
            result += Mathf.Abs(textCode.Rect().offsetMax.x);

            return result;
        }
    }

    public int TextWidth
    {
        get
        {
            int result = 0;
            Font font = textCode.font;
            CharacterInfo characterInfo = new CharacterInfo();
            foreach (char c in Text)
            {
                font.GetCharacterInfo(c, out characterInfo, textCode.fontSize);
                result += characterInfo.advance;
            }

            return result;
        }
    }

    public Action<MyCodeLine> LineClicked = null;

    private void Start()
    {
        buttonCode.gameObject.SetActive(true);
    }

    private void Awake()
    {
        buttonCode.onClick.AddListener(OnButtonCodeClicked);
    }

    private void OnDestroy()
    {
        buttonCode.onClick.RemoveListener(OnButtonCodeClicked);
    }

    public MyCodeLine SetText(string text, string rich)
    {
        Text = text;

        textCode.text = rich;

        return this;
    }

    public void Relayout()
    {
        this.Rect().SetLeftTopPosition(new Vector2(X_OFFSET, -Y_OFFSET - Height * LineNumber));
    }

    private void OnButtonCodeClicked()
    {
        if (LineClicked != null)
            LineClicked(this);
    }
}
