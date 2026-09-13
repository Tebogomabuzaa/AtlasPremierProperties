using System;
using System.Web.Mvc;
using AtlasPremierProperties.Services;
using Newtonsoft.Json;

namespace AtlasPremierProperties.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ReportService _reportService;

        public ReportsController()
        {
            _reportService = new ReportService();
        }

        public ActionResult CoHostPerformance()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CoHostPerformance(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                ModelState.AddModelError("", "The end date must be on or after the start date.");
                return View();
            }

            var data = _reportService.GetCoHostPerformance(startDate, endDate);

            var labels = new System.Collections.Generic.List<string>();
            var rates = new System.Collections.Generic.List<decimal>();
            var colors = new System.Collections.Generic.List<string>();

            // Chart.js needs separate arrays, not a list of objects
            foreach (var item in data)
            {
                labels.Add(item.ManagerName);
                rates.Add(item.OccupancyRate);

                // Green if occupancy is healthy (70%+), red if it's low
                colors.Add(item.OccupancyRate >= 70 ? "#28a745" : "#dc3545");
            }

            ViewBag.ChartJson = JsonConvert.SerializeObject(
                new { labels, rates, colors });

            ViewBag.StartDate = startDate.ToString("dd MMM yyyy");
            ViewBag.EndDate = endDate.ToString("dd MMM yyyy");

            return View(data);
        }
    }
}
