namespace ProjectMover.Lib.Extensions {
  public static class DateTimeExtensions {
    public static string ToAssemblySuffix (this DateTime dt)
        => dt.ToString ("yyMMdd.HHmmss");
  }
}
