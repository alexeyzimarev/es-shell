using System;
using System.Collections.Generic;
using System.Text;

namespace EventStore.Shell.ReadLine
{
    class KeyHandler
    {
        readonly StringBuilder              _text;
        readonly List<string>               _history;
        readonly Dictionary<string, Action> _keyActions;
        readonly IConsole                   _console2;

        int            _cursorPos;
        int            _cursorLimit;
        int            _historyIndex;
        ConsoleKeyInfo _keyInfo;
        string[]       _completions;
        int            _completionStart;
        int            _completionsIndex;

        bool IsStartOfLine() => _cursorPos            == 0;
        bool IsEndOfLine() => _cursorPos              == _cursorLimit;
        bool IsStartOfBuffer() => _console2.CursorLeft == 0;
        bool IsEndOfBuffer() => _console2.CursorLeft   == _console2.BufferWidth - 1;
        bool IsInAutoCompleteMode() => _completions   != null;

        void MoveCursorLeft()
        {
            if (IsStartOfLine()) return;

            if (IsStartOfBuffer())
                _console2.SetCursorPosition(_console2.BufferWidth - 1, _console2.CursorTop - 1);
            else
                _console2.SetCursorPosition(_console2.CursorLeft - 1, _console2.CursorTop);

            _cursorPos--;
        }

        void MoveCursorHome()
        {
            while (!IsStartOfLine()) MoveCursorLeft();
        }

        string BuildKeyInput()
            => (_keyInfo.Modifiers != ConsoleModifiers.Control && _keyInfo.Modifiers != ConsoleModifiers.Shift)
                ? _keyInfo.Key.ToString()
                : $"{_keyInfo.Modifiers}{_keyInfo.Key}";

        void MoveCursorRight()
        {
            if (IsEndOfLine()) return;

            if (IsEndOfBuffer())
                _console2.SetCursorPosition(0, _console2.CursorTop + 1);
            else
                _console2.SetCursorPosition(_console2.CursorLeft + 1, _console2.CursorTop);

            _cursorPos++;
        }

        void MoveCursorEnd()
        {
            while (!IsEndOfLine()) MoveCursorRight();
        }

        void ClearLine()
        {
            MoveCursorEnd();
            while (!IsStartOfLine()) Backspace();
        }

        void WriteNewString(string str)
        {
            ClearLine();
            foreach (var character in str) WriteChar(character);
        }

        void WriteString(string str)
        {
            foreach (var character in str) WriteChar(character);
        }

        void WriteChar() => WriteChar(_keyInfo.KeyChar);

        void WriteChar(char c)
        {
            if (IsEndOfLine())
            {
                _text.Append(c);
                _console2.Write(c.ToString());
                _cursorPos++;
            }
            else
            {
                var left = _console2.CursorLeft;
                var top  = _console2.CursorTop;
                var str  = _text.ToString().Substring(_cursorPos);
                _text.Insert(_cursorPos, c);
                _console2.Write(c + str);
                _console2.SetCursorPosition(left, top);
                MoveCursorRight();
            }

            _cursorLimit++;
        }

        void Backspace()
        {
            if (IsStartOfLine()) return;

            MoveCursorLeft();
            var index = _cursorPos;
            _text.Remove(index, 1);
            var replacement = _text.ToString().Substring(index);
            var left        = _console2.CursorLeft;
            var top         = _console2.CursorTop;

            _console2.Write($"{replacement} ");
            _console2.SetCursorPosition(left, top);
            _cursorLimit--;
        }

        void Delete()
        {
            if (IsEndOfLine()) return;

            var index = _cursorPos;
            _text.Remove(index, 1);
            var replacement = _text.ToString().Substring(index);
            var left        = _console2.CursorLeft;
            var top         = _console2.CursorTop;
            _console2.Write($"{replacement} ");
            _console2.SetCursorPosition(left, top);
            _cursorLimit--;
        }

        void TransposeChars()
        {
            // local helper functions
            bool AlmostEndOfLine() => (_cursorLimit - _cursorPos) == 1;

            static int IncrementIf(Func<bool> expression, int index) => expression() ? index + 1 : index;

            static int DecrementIf(Func<bool> expression, int index) => expression() ? index - 1 : index;

            if (IsStartOfLine())
            {
                return;
            }

            var firstIdx  = DecrementIf(IsEndOfLine, _cursorPos - 1);
            var secondIdx = DecrementIf(IsEndOfLine, _cursorPos);

            var secondChar = _text[secondIdx];
            _text[secondIdx] = _text[firstIdx];
            _text[firstIdx]  = secondChar;

            var left           = IncrementIf(AlmostEndOfLine, _console2.CursorLeft);
            var cursorPosition = IncrementIf(AlmostEndOfLine, _cursorPos);

            WriteNewString(_text.ToString());

            _console2.SetCursorPosition(left, _console2.CursorTop);
            _cursorPos = cursorPosition;

            MoveCursorRight();
        }

        void StartAutoComplete()
        {
            while (_cursorPos > _completionStart) Backspace();

            _completionsIndex = 0;

            WriteString(_completions[_completionsIndex]);
        }

        void NextAutoComplete()
        {
            while (_cursorPos > _completionStart) Backspace();

            _completionsIndex++;

            if (_completionsIndex == _completions.Length) _completionsIndex = 0;

            WriteString(_completions[_completionsIndex]);
        }

        void PreviousAutoComplete()
        {
            while (_cursorPos > _completionStart) Backspace();

            _completionsIndex--;

            if (_completionsIndex == -1) _completionsIndex = _completions.Length - 1;

            WriteString(_completions[_completionsIndex]);
        }

        void PrevHistory()
        {
            if (_historyIndex <= 0) return;

            _historyIndex--;
            WriteNewString(_history[_historyIndex]);
        }

        void NextHistory()
        {
            if (_historyIndex >= _history.Count) return;

            _historyIndex++;

            if (_historyIndex == _history.Count)
                ClearLine();
            else
                WriteNewString(_history[_historyIndex]);
        }

        void ResetAutoComplete()
        {
            _completions      = null;
            _completionsIndex = 0;
        }

        public string Text => _text.ToString();

        public KeyHandler(IConsole console, List<string> history, IAutoCompleteHandler autoCompleteHandler)
        {
            _console2 = console;

            _history      = history ?? new List<string>();
            _historyIndex = _history.Count;
            _text         = new StringBuilder();

            _keyActions = new Dictionary<string, Action>
            {
                ["LeftArrow"]  = MoveCursorLeft,
                ["Home"]       = MoveCursorHome,
                ["End"]        = MoveCursorEnd,
                ["ControlA"]   = MoveCursorHome,
                ["ControlB"]   = MoveCursorLeft,
                ["RightArrow"] = MoveCursorRight,
                ["ControlF"]   = MoveCursorRight,
                ["ControlE"]   = MoveCursorEnd,
                ["Backspace"]  = Backspace,
                ["Delete"]     = Delete,
                ["ControlD"]   = Delete,
                ["ControlH"]   = Backspace,
                ["ControlL"]   = ClearLine,
                ["Escape"]     = ClearLine,
                ["UpArrow"]    = PrevHistory,
                ["ControlP"]   = PrevHistory,
                ["DownArrow"]  = NextHistory,
                ["ControlN"]   = NextHistory,
                ["ControlU"] = () =>
                {
                    while (!IsStartOfLine()) Backspace();
                },
                ["ControlK"] = () =>
                {
                    int pos = _cursorPos;
                    MoveCursorEnd();
                    while (_cursorPos > pos) Backspace();
                },
                ["ControlW"] = () =>
                {
                    while (!IsStartOfLine() && _text[_cursorPos - 1] != ' ') Backspace();
                },
                ["ControlT"] = TransposeChars,
                ["Tab"] = () =>
                {
                    if (IsInAutoCompleteMode())
                    {
                        NextAutoComplete();
                    }
                    else
                    {
                        if (autoCompleteHandler == null || !IsEndOfLine()) return;

                        var text = _text.ToString();

                        _completionStart = text.LastIndexOfAny(autoCompleteHandler.Separators);
                        _completionStart = _completionStart == -1 ? 0 : _completionStart + 1;

                        _completions = autoCompleteHandler.GetSuggestions(text, _completionStart);
                        _completions = _completions?.Length == 0 ? null : _completions;

                        if (_completions == null) return;

                        StartAutoComplete();
                    }
                },
                ["ShiftTab"] = () =>
                {
                    if (IsInAutoCompleteMode())
                    {
                        PreviousAutoComplete();
                    }
                }
            };
        }

        public void Handle(ConsoleKeyInfo keyInfo)
        {
            _keyInfo = keyInfo;

            // If in auto complete mode and Tab wasn't pressed
            if (IsInAutoCompleteMode() && _keyInfo.Key != ConsoleKey.Tab) ResetAutoComplete();

            var input = BuildKeyInput();
            _keyActions.TryGetValue(input, out var action);
            action ??= WriteChar;
            action.Invoke();
        }
    }
}
