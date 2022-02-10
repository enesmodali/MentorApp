﻿using Microsoft.AspNetCore.Mvc;

namespace MentorApp.Alerts
{
    public static class AlertExtensions
    {
        public static IActionResult WithError(this IActionResult result,
                                            string message,
                                            string debugInfo = null)
        {
            return Alert(result, "danger", message, debugInfo);
        }

        public static IActionResult WithSuccess(this IActionResult result,
                                            string message,
                                            string debugInfo = null)
        {
            return Alert(result, "success", message, debugInfo);
        }

        public static IActionResult WithInfo(this IActionResult result,
                                            string message,
                                            string debugInfo = null)
        {
            return Alert(result, "info", message, debugInfo);
        }

        private static IActionResult Alert(IActionResult result,
                                        string type,
                                        string message,
                                        string debugInfo)
        {
            return new WithAlertResult(result, type, message, debugInfo);
        }
    }
}
