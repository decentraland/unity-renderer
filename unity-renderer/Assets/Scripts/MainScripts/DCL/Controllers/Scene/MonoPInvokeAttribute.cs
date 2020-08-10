using System;

public class MonoPInvokeCallbackAttribute : System.Attribute
{
	public Type type;
	public MonoPInvokeCallbackAttribute( Type t ) { type = t; }
}