﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomVisionApp.StreetFighter
{
    public static class ExecuteWhen
    {
        private static string _current = "";
        private static int _count = 0;

        public static void SameValueNTimes(int n, string value, Action action)
        {
            if (_current != value)
            {
                _count = 1;
                _current = value;
                return;
            }

            _count++;

            if (_count == n)
            {
                action.Invoke();
                _count = 0;
            }
        }

        public static void SameValueThreeTimes(string value, Action action)
        {
            SameValueNTimes(3, value, action);
        }
    }
}

