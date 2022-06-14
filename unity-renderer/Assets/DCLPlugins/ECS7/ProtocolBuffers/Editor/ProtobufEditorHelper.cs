using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class ProtobufEditorHelper 
{
    public static void CloneDirectory(string root, string dest)
    {
        foreach (var directory in Directory.GetDirectories(root))
        {
            string dirName = Path.GetFileName(directory);
            if (!Directory.Exists(Path.Combine(dest, dirName)))
            {
                Directory.CreateDirectory(Path.Combine(dest, dirName));
            }
            CloneDirectory(directory, Path.Combine(dest, dirName));
        }

        foreach (var file in Directory.GetFiles(root))
        {
            File.Copy(file, Path.Combine(dest, Path.GetFileName(file)));
        }
    }
        
    public static string ToSnakeCase(this string text)
    {
        if(text == null) {
            throw new ArgumentNullException(nameof(text));
        }
        if(text.Length < 2) {
            return text;
        }
        var sb = new StringBuilder();
        sb.Append(char.ToLowerInvariant(text[0]));
        for(int i = 1; i < text.Length; ++i) {
            char c = text[i];
            if(char.IsUpper(c)) {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            } else {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
