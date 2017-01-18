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
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using lib;

public class Example : MonoBehaviour
{
    private RunMode mode = RunMode.Stopped;

    private Interpreter interp = new Interpreter();

    public MyCodeEditor editor = null;

    public MsgBox msgbox = null;

    public Dropdown dropdownFile = null;

    public bool pauseOnMsg = true;

    #region Threading

    private static object dataLock = new object();
    private static object msg = null;

    #endregion

    private string FilePath
    {
        get
        {
            string result = Application.persistentDataPath;
            if (!result.EndsWith("\\") && !result.EndsWith("/"))
                result += "/";
            result += string.Format("file_{0}.bas", dropdownFile.value);

            return result;
        }
    }

    private string ConfigFilePath
    {
        get
        {
            string result = Application.persistentDataPath;
            if (!result.EndsWith("\\") && !result.EndsWith("/"))
                result += "/";
            result += "config.txt";

            return result;
        }
    }

    private int LastEdited
    {
        get
        {
            if (!File.Exists(ConfigFilePath))
                return -1;

            using (FileStream fs = new FileStream(ConfigFilePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string l = sr.ReadLine();

                    return int.Parse(l);
                }
            }
        }
        set
        {
            if (!File.Exists(ConfigFilePath))
            {
                using (FileStream fs = new FileStream(ConfigFilePath, FileMode.OpenOrCreate, FileAccess.Write)) { }
            }

            using (FileStream fs = new FileStream(ConfigFilePath, FileMode.Truncate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(dropdownFile.value.ToString());
                }
            }
        }
    }

    private void Start()
    {
        Interpreter.Initlalize();

        interp.Open();
        interp.Stepped += OnStepped;
        interp.ErrorOccured += OnErrorOccured;
        interp.ExecutionFinished += OnExecutionFinished;

        Register(output);

        StartCoroutine(Messaging());

        StartCoroutine(FillLastCode());
    }

    private void OnDestroy()
    {
        interp.Stop();

        while (!interp.Finished)
        {
            System.Threading.Thread.Sleep(10);
        }

        interp.Close();

        Interpreter.Dispose();
    }

    private bool OnStepped(int ln)
    {
        switch (mode)
        {
            case RunMode.Stopped:
                return true;
            case RunMode.Once:
                Debug.Log(">" + ln.ToString());

                editor.ScrollToLine(ln);

                mode = RunMode.Paused;

                return true;
            case RunMode.Always:
                Debug.Log(">" + ln.ToString());

                editor.ScrollToLine(ln);

                return true;
            case RunMode.Paused:
                return false;
        }

        return false;
    }

    private void OnErrorOccured(Error err)
    {
        Debug.LogError(err.full);

        msgbox.Show(err.full);
    }

    private void OnExecutionFinished()
    {
        Debug.Log("OK");

        mode = RunMode.Stopped;
    }

    private void Register(my_basic.mb_func_t func)
    {
        interp.Register(func);
        editor.AddFunction(func.Method.Name.ToLower());
    }

    #region Threading

    private static int output(IntPtr s, ref IntPtr l)
    {
        my_basic.mb_value_t val;

        my_basic.mb_attempt_open_bracket(s, ref l);

        my_basic.mb_pop_value(s, ref l, out val);

        my_basic.mb_attempt_close_bracket(s, ref l);

        lock (dataLock)
        {
            switch (val.type)
            {
                case my_basic.mb_data_e.MB_DT_INT:
                    msg = val.value.integer;

                    break;
                case my_basic.mb_data_e.MB_DT_REAL:
                    msg = val.value.float_point;

                    break;
                case my_basic.mb_data_e.MB_DT_STRING:
                    msg = val.value.String;

                    break;
                default:
                    break;
            }
        }

        return my_basic.MB_FUNC_OK;
    }

    #endregion

    private IEnumerator Messaging()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        while (true)
        {
            object m = null;
            lock (dataLock)
            {
                m = msg;
                msg = null;
            }

            if (m != null)
            {
                if (pauseOnMsg && mode == RunMode.Always)
                {
                    mode = RunMode.Paused;

                    msgbox.Show
                    (
                        m,
                        (_) => { mode = RunMode.Always; }
                    );
                }
                else
                {
                    msgbox.Show(m);
                }
            }

            yield return wait;
        }
    }

    private IEnumerator FillLastCode()
    {
        yield return new WaitForEndOfFrame();

        if (LastEdited == -1)
        {
            editor.Append("a = 22");
            editor.Append("b = 7");
            editor.Append("output(a / b)");
            editor.Append("output(\"Hello World!\")");

            editor.Relayout();
        }
        else
        {
            dropdownFile.value = LastEdited;
            OnLoadClicked();
        }
    }

    public void OnLoadClicked()
    {
        if (msgbox.Shown) return;

        if (!File.Exists(FilePath))
        {
            msgbox.Show("File", "Blank file, not loaded.");

            return;
        }

        editor.Clear();
        using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                string code = sr.ReadToEnd();
                editor.Text = code;
                editor.Relayout();
            }
        }

        LastEdited = dropdownFile.value;
    }

    public void OnSaveClicked()
    {
        if (msgbox.Shown) return;

        if (!File.Exists(FilePath))
        {
            using (FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write)) { }
        }

        using (FileStream fs = new FileStream(FilePath, FileMode.Truncate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(editor.Text);
            }
        }

        LastEdited = dropdownFile.value;
    }

    public void OnInsertClicked()
    {
        if (msgbox.Shown) return;

        editor.Insert(editor.Selected);

        editor.Unselect(editor.Selected);
    }

    public void OnDeleteClicked()
    {
        if (msgbox.Shown) return;

        editor.Remove(editor.Selected);
    }

    public void OnRunClicked()
    {
        if (msgbox.Shown) return;

        RunMode m = mode;

        mode = RunMode.Always;

        if (m == RunMode.Stopped)
        {
            string code = editor.Text;

            interp.Run(this, code);
        }
    }

    public void OnStepClicked()
    {
        if (msgbox.Shown) return;

        RunMode m = mode;

        mode = RunMode.Once;

        if (m == RunMode.Stopped)
        {
            string code = editor.Text;

            interp.Run(this, code);
        }
    }

    public void OnPauseClicked()
    {
        if (msgbox.Shown) return;

        if (mode == RunMode.Always)
            mode = RunMode.Paused;
    }

    public void OnStopClicked()
    {
        if (msgbox.Shown) return;

        if (mode != RunMode.Stopped)
        {
            mode = RunMode.Stopped;

            interp.Stop();
        }
    }
}
