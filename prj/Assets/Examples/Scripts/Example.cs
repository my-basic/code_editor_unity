using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lib;

public class Example : MonoBehaviour
{
    public MyCodeEditor editor = null;

    private IntPtr bas = IntPtr.Zero;

    private void Start()
    {
        my_basic.mb_init();
        my_basic.mb_open(out bas);

        my_basic.mb_set_error_handler(bas, on_error);

        my_basic.mb_register_func(bas, "OUTPUT", output);

        StartCoroutine(test());
    }

    private void OnDestroy()
    {
        my_basic.mb_close(out bas);
        my_basic.mb_dispose();
    }

    private void on_error(IntPtr s, my_basic.mb_error_e e, string m, string f, int p, ushort row, ushort col, int abort_code)
    {
        if (e != my_basic.mb_error_e.SE_NO_ERR)
        {
            if (f != null)
            {
                if (e == my_basic.mb_error_e.SE_RN_WRONG_FUNCTION_REACHED)
                {
                    print(string.Format("ERROR\n  Ln {0}, Func: {1}, Msg: {2}.\n", row, f, m));
                }
                else
                {
                    print(string.Format("ERROR\n  Ln {0}, File: {1}, Msg: {2}.\n", row, f, m));
                }
            }
            else
            {
                print(string.Format("ERROR\n  Ln {0}, Msg: {1}.\n", row, m));
            }
        }
    }

    private int output(IntPtr s, ref IntPtr l)
    {
        my_basic.mb_value_t val;

        my_basic.mb_attempt_open_bracket(s, ref l);

        my_basic.mb_pop_value(s, ref l, out val);

        my_basic.mb_attempt_close_bracket(s, ref l);

        switch (val.type)
        {
            case my_basic.mb_data_e.MB_DT_INT:
                print(val.value.integer);
                break;
            case my_basic.mb_data_e.MB_DT_REAL:
                print(val.value.float_point);
                break;
            case my_basic.mb_data_e.MB_DT_STRING:
                print(val.value.String);
                break;
            default:
                break;
        }

        return my_basic.MB_FUNC_OK;
    }

    private IEnumerator test()
    {
        yield return new WaitForEndOfFrame();

        editor.Append("a = 22");
        editor.Append("b = 7");
        editor.Append("output(a / b)");
        editor.Append("output(\"hello\")");

        editor.Relayout();
    }

    public void OnInsertClicked()
    {
        editor.Insert(editor.Selected);

        editor.Unselect(editor.Selected);
    }

    public void OnDeleteClicked()
    {
        editor.Remove(editor.Selected);
    }

    public void OnRunClicked()
    {
        string code = editor.Text;

        print(code);

        my_basic.mb_reset(out bas);
        my_basic.mb_load_string(bas, code);
        my_basic.mb_run(bas);
    }
}
