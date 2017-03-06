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

using UnityEngine;
using UnityEngine.UI;

public class MyCodeInput : MonoBehaviour
{
    public MyCodeLine Editing { get; private set; }

    public InputField FieldInput
    {
        get
        {
            InputField i = GetComponent<InputField>();

            return i;
        }
    }

    public bool Shown
    {
        get
        {
            return gameObject.activeSelf;
        }
    }

    public void Show(MyCodeLine ln)
    {
        if (Editing != null)
            Editing.gameObject.SetActive(true);

        gameObject.SetActive(true);

        FieldInput.Rect().SetSize(ln.Rect().GetSize());
        FieldInput.Rect().transform.localPosition = (ln.Rect().transform.localPosition);
        FieldInput.text = ln.Text;
        FieldInput.ActivateInputField();

        ln.gameObject.SetActive(false);

        Editing = ln;
    }

    public void Hide()
    {
        if (Editing != null)
            Editing.gameObject.SetActive(true);

        gameObject.SetActive(false);

        Editing = null;
    }
}
