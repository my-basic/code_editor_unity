# MY-BASIC Code Editor (Unity)

**Copyright (C) 2017 [Wang Renxin](https://github.com/paladin-t). All rights reserved.**

## Introduction

This is a code editor for [MY-BASIC](https://github.com/paladin-t/my_basic) powered by the Unity3D engine.

![](docs/run.png)

It supports code manipulating, interpreter interacting, and future customization. I used a MY-BASIC DLL for Windows, and Bundle for macOS as demonstration. You can build plugin for other platforms as you wish, or use other interpreters. See the link for more information about [MY-BASIC](https://github.com/paladin-t/my_basic).

![](docs/phone.jpg)

Note this repository doesn't contain any extended libraries of MY-BASIC or any other playable stuffs. It's just a small reusable code editor/shell.

## Configuration

You may configure keywords, reserved words, symbols, opcodes, etc. as well as in which colors to represent them.

The editor also allows to configure how many lines of code is expected to be represented, assuming taking full height of the screen.

![](docs/config.png)

It's also able to add new function words for coloring in code.

~~~~~~~~~~cs
MyCodeEditor editor = ...
...
editor.AddFunction("Your function name");
~~~~~~~~~~

## How to use the editor

Execute the `bin/my_basic_code_editor_unity.exe` to run the editor.

* Click a line to input or edit code

![](docs/edit.png)

* Select a file slot to save and load

![](docs/file.png)

* Select some lines, then make insertion and deletion by clicking the buttons

![](docs/curd.png)

* Click the `RUN` button to run the code top down
* Click the `STEP` button to run the code step by step
* Click the `PAUSE` button to pause a top down execution
* Click the `STOP` button to terminate an execution

![](docs/exe.png)

## How to use the library

Import the `pkg/my_basic_code_editor_unity.unitypackage` to your Unity3D project to use this library.

Have fun :)
