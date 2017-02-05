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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MsgBox : MonoBehaviour
{
    private struct MsgInfo
    {
        public string title;

        public object msg;

        public ClickedEventHandler callback;

        public MsgInfo(string t, object m, ClickedEventHandler c)
        {
            title = t;
            msg = m;
            callback = c;
        }
    }

    public Text textTitle = null;

    public Text textMsg = null;

    private Stack<MsgInfo> stack = new Stack<MsgInfo>();

    public bool Shown { get { return stack.Count != 0; } }

    public delegate void ClickedEventHandler(MsgBox mb);

    public void Show(string title, object msg, ClickedEventHandler callback = null)
    {
        gameObject.SetActive(true);

        stack.Push(new MsgInfo(title, msg, callback));

        textTitle.text = title;
        textMsg.text = msg.ToString();
    }

    public void Show(object msg, ClickedEventHandler callback = null)
    {
        gameObject.SetActive(true);

        stack.Push(new MsgInfo(null, msg, callback));

        textTitle.text = string.Empty;
        textMsg.text = msg.ToString();
    }

    public void OnOkClicked()
    {
        if (stack.Peek().callback != null)
            stack.Peek().callback(this);

        stack.Pop();
        if (stack.Count == 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            textTitle.text = stack.Peek().title;
            textMsg.text = stack.Peek().msg.ToString();
        }
    }
}
