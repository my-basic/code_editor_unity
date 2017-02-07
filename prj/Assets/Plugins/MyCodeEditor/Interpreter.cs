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
using System.Threading;
using AOT;
using UnityEngine;

namespace lib
{
    public enum RunMode
    {
        Stopped,
        Once,
        Always,
        Paused
    }

    public sealed class Error
    {
        public IntPtr s = IntPtr.Zero;
        public my_basic.mb_error_e e = my_basic.mb_error_e.SE_NO_ERR;
        public string m = null;
        public string f = null;
        public int p = 0;
        public ushort row = 0;
        public ushort col = 0;
        public int abort_code = 0;
        public string full = null;

        public Error(IntPtr _s, my_basic.mb_error_e _e, string _m, string _f, int _p, ushort _row, ushort _col, int _abort_code, string _full)
        {
            s = _s;
            e = _e;
            m = _m;
            f = _f;
            p = _p;
            row = _row;
            col = _col;
            abort_code = _abort_code;
            full = _full;
        }
    }

    public sealed class Interpreter
    {
        #region Threading

        private static object stopLock = new object();
        private static bool stop = false;

        private static object dataLock = new object();
        private static int line = 0;
        private static bool waiting = false;
        private static Error error = null;

        #endregion

        private Coroutine co = null;

        private Thread thread = null;

        private IntPtr bas = IntPtr.Zero;
        public IntPtr Bas { get { return bas; } }

        public bool Finished
        {
            get
            {
                return co == null || thread == null || !thread.IsAlive;
            }
        }

        public Func<int, bool> Stepped = null;

        public Action<Error> ErrorOccured = null;

        public Action ExecutionFinished = null;

        public static void Initlalize()
        {
            my_basic.mb_init();
        }

        public static void Dispose()
        {
            my_basic.mb_dispose();
        }

        public void Open()
        {
            my_basic.mb_open(out bas);

            my_basic.mb_set_error_handler(bas, on_error);
            my_basic.mb_debug_set_stepped_handler(bas, on_stepped);
        }

        public void Close()
        {
            my_basic.mb_close(out bas);
        }

        #region Threading

        [MonoPInvokeCallback(typeof(my_basic.mb_error_handler_t))]
        private static void on_error(IntPtr s, my_basic.mb_error_e e, string m, string f, int p, ushort row, ushort col, int abort_code)
        {
            lock (dataLock)
            {
                if (e != my_basic.mb_error_e.SE_NO_ERR)
                {
                    string full = null;
                    if (f != null)
                    {
                        if (e == my_basic.mb_error_e.SE_RN_WRONG_FUNCTION_REACHED)
                        {
                            full = string.Format("ERROR\n  Ln {0}, Func: {1}, Msg: {2}.\n", row, f, m);
                        }
                        else
                        {
                            full = string.Format("ERROR\n  Ln {0}, File: {1}, Msg: {2}.\n", row, f, m);
                        }
                    }
                    else
                    {
                        full = string.Format("ERROR\n  Ln {0}, Msg: {1}.\n", row, m);
                    }
                    error = new Error(s, e, m, f, p, row, col, abort_code, full);
                }
            }
        }

        [MonoPInvokeCallback(typeof(my_basic.mb_debug_stepped_handler_t))]
        private static int on_stepped(IntPtr s, ref IntPtr l, string f, int p, ushort row, ushort col)
        {
            lock (dataLock)
            {
                waiting = true;
                line = row;
            }

            bool w = false;
            do
            {
                lock (dataLock)
                {
                    w = waiting;
                }

                lock (stopLock)
                {
                    if (stop)
                        return my_basic.MB_FUNC_BYE;
                }
            } while (w);

            return my_basic.MB_FUNC_OK;
        }

        #endregion

        public void Register(my_basic.mb_func_t func)
        {
            string name = func.Method.Name;
            my_basic.mb_register_func(bas, name.ToUpper(), func);
        }

        public void Register(string name, my_basic.mb_func_t func)
        {
            my_basic.mb_register_func(bas, name.ToUpper(), func);
        }

        public void RemoveReservedFunc(string name)
        {
            my_basic.mb_remove_reserved_func(bas, name.ToUpper());
        }

        private void Thread(object par)
        {
            my_basic.mb_run(bas);
        }

        private IEnumerator StartThread(string code)
        {
            WaitForEndOfFrame wait = new WaitForEndOfFrame();

            while (thread != null && thread.IsAlive)
            {
                Stop();

                yield return wait;
            }
            thread = null;

            stop = false;
            line = 0;
            waiting = false;

            my_basic.mb_reset(ref bas);
            my_basic.mb_load_string(bas, code);

            thread = new Thread(new ParameterizedThreadStart(Thread));
            thread.Start(null);

            while (thread != null && thread.IsAlive)
            {
                lock (dataLock)
                {
                    if (waiting)
                    {
                        if (error != null)
                        {
                            if (ErrorOccured != null)
                                ErrorOccured(error);
                            error = null;
                        }
                        if (Stepped != null)
                        {
                            while (!Stepped(line))
                            {
                                goto _exit;
                            }
                        }
                        waiting = false;
                    }
                }

                _exit:

                yield return wait;
            }
            thread = null;

            co = null;

            if (ExecutionFinished != null)
                ExecutionFinished();
        }

        public void Run(MonoBehaviour owner, string code)
        {
            if (co != null)
                owner.StopCoroutine(co);

            co = owner.StartCoroutine(StartThread(code));
        }

        public void Stop()
        {
            lock (stopLock)
            {
                stop = true;
            }
        }
    }
}
