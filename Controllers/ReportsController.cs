using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AtlasPremierProperties.Services;
using Newtonsoft.Json;

namespace AtlasPremierProperties.Controllers
{
    public class ReportsController : Controller
    {
        // Managers at or above this occupancy rate are highlighted as top performers.
        public const decimal TopPerformerRate = 90m;

        private readonly ReportService _reportService;

        public ReportsController()
        {
            _reportService = new ReportService();
        }

        // Defaults to the last 12 months so the report shows something as soon as the page opens.
        public ActionResult CoHostPerformance(DateTime? startDate, DateTime? endDate)
        {
            var end = (endDate ?? DateTime.Today).Date;
            var start = (startDate ?? end.AddYears(-1)).Date;
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            if (end < start)
            {
                ModelState.AddModelError("", "The end date must be on or after the start date.");
                return View(new List<CoHostStats>());
            }

            var data = _reportService.GetCoHostPerformance(start, end);

            // Chart.js needs separate arrays, not a list of objects
            ViewBag.ChartJson = JsonConvert.SerializeObject(new
            {
                labels = data.Select(d => d.ManagerName),
                rates = data.Select(d => d.OccupancyRate),
                colors = data.Select(d => d.OccupancyRate >= TopPerformerRate ? "#d4a340" : "#0f1d3d")
            });

            return View(data);
        }
    }
}
