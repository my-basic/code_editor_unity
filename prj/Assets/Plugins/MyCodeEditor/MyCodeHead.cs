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
using UnityEngine;
using UnityEngine.UI;

public class MyCodeHead : MonoBehaviour
{
    private const float TEXT_OCCUPATION = 0.8f;

    public const float X_OFFSET = 2.0f;
    public const float Y_OFFSET = 2.0f;

    public Toggle toggleSelection = null;

    public Button buttonLineNumber = null;
    public Text textLineNumber = null;

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

            textLineNumber.text = (lineNumber + 1).ToString();
        }
    }

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
            textLineNumber.fontSize = fontSize;

            this.Rect().SetHeight(height);
            buttonLineNumber.Rect().SetHeight(height);
            toggleSelection.Rect().SetSize(new Vector2(height, height));
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
            float result = LineNumberTextWidth;
            result += Mathf.Abs(textLineNumber.Rect().offsetMin.x);
            result += Mathf.Abs(textLineNumber.Rect().offsetMax.x);
            result += toggleSelection.Rect().GetSize().x;

            return result;
        }
    }

    public int LineNumberTextWidth
    {
        get
        {
            int result = 0;
            Font font = textLineNumber.font;
            CharacterInfo characterInfo = new CharacterInfo();
            foreach (char c in textLineNumber.text)
            {
                font.GetCharacterInfo(c, out characterInfo, textLineNumber.fontSize);
                result += characterInfo.advance;
            }

            return result;
        }
    }

    private bool selfProtecting = false;

    public bool SelectionEnabled { get; set; }

    public Action<MyCodeHead> HeadClicked = null;

    private void Start()
    {
        SelectionEnabled = true;
    }

    private void Awake()
    {
        buttonLineNumber.onClick.AddListener(OnButtonHeadClicked);

        toggleSelection.onValueChanged.AddListener(OnSelectionValueChanged);
    }

    private void OnDestroy()
    {
        buttonLineNumber.onClick.RemoveListener(OnButtonHeadClicked);

        toggleSelection.onValueChanged.RemoveListener(OnSelectionValueChanged);
    }

    public void Relayout()
    {
        this.Rect().SetLeftTopPosition(new Vector2(X_OFFSET, -Y_OFFSET - Height * lineNumber));

        toggleSelection.Rect().SetLeftPosition(-this.Rect().GetSize().x / 2.0f);
        buttonLineNumber.Rect().SetSize(new Vector2(this.Rect().GetSize().x - toggleSelection.Rect().GetSize().x, buttonLineNumber.Rect().GetSize().y));
        buttonLineNumber.Rect().SetLeftPosition(-this.Rect().GetSize().x / 2.0f + toggleSelection.Rect().GetSize().x);
    }

    private void OnSelectionValueChanged(bool v)
    {
        if (SelectionEnabled) return;
        if (selfProtecting) return;
        selfProtecting = true;
        toggleSelection.isOn = false;
        selfProtecting = false;
    }

    private void OnButtonHeadClicked()
    {
        if (HeadClicked != null)
            HeadClicked(this);
    }
}
