using System;

namespace RdsDataExportCliTool.Services {
    internal static class ExceptionHelper {
        public static Exception Unwrap(Exception ex) {
            if(ex is AggregateException agg && agg.InnerException != null)
                return agg.InnerException;
            return ex;
        }
    }
}
