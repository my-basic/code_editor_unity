using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    public MyCodeEditor editor = null;

    private void Start()
    {
        System.IntPtr bas;
        lib.my_basic.mb_init();
        lib.my_basic.mb_open(out bas);
        lib.my_basic.mb_register_func(bas, "FOO", foo);
        lib.my_basic.mb_load_string(bas, "print foo(22, 7);");
        lib.my_basic.mb_run(bas);
        lib.my_basic.mb_close(out bas);
        lib.my_basic.mb_dispose();
    }

    private void OnEnable()
    {
        editor.Append("print class if tHEn mod tostring +");
        editor.Append("'print class if tHEn mod tostring +");
        editor.Append("print class 'if tHEn mod tostring +");
        editor.Append("rem print class if tHEn mod tostring +");
        editor.Append("print class rem if tHEn mod tostring +");
        for (int i = 0; i < 16; i++)
        {
            editor.Append("print \"hello world\";");
            editor.Append("print 22/7;");
        }
        editor.Append("print 22/7;print 22/7;print 22/7;print 22/7;print 22/7;print 22/7;print 22/7;print 22/7;print 22/7;print 22/7;");
    }

    private int foo(System.IntPtr s, ref System.IntPtr l)
    {
        int x;
        int y;

        lib.my_basic.mb_attempt_open_bracket(s, ref l);

        lib.my_basic.mb_pop_int(s, ref l, out x);
        lib.my_basic.mb_pop_int(s, ref l, out y);

        lib.my_basic.mb_attempt_close_bracket(s, ref l);

        lib.my_basic.mb_push_real(s, ref l, (float)x / y);

        print((float)x / y);

        return lib.my_basic.MB_FUNC_OK;
    }
}
