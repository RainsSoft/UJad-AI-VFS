using System;

public class XXX
{
  
  public void TT()
  {
    var encoding = System.Text.Encoding.GetEncoding("IBM437");
    Console.Out.WriteLine(encoding.GetBytes("hello world"));
  }
}