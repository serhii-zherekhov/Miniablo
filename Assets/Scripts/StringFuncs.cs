using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringFuncs
{
    public static string ReverseString(string s)
    {
        char[] arr = s.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }
}
