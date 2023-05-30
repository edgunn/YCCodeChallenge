using System.Collections.Immutable;
using System.Data;

namespace YellowCanaryCodeChallenge
{
    public class PaymentReportingService
    {
        private const double SuperContributionPercentage = 0.095;

        public DataTable GenerateEmployeeQuarterlySuperReport(DataSet superDataSet)
        {
            var employeeQuarterlyOtePay = GetEmployeeQuarterlyOtePay(superDataSet);
            var employeeQuarterlyDisbursements = GetEmployeeQuarterlyDisbursements(superDataSet);

            var quarterlySuperTable = new DataTable("EmployeeQuarterlySuperReport");
            quarterlySuperTable.Columns.Add(new DataColumn("employee_code", typeof(double)));
            quarterlySuperTable.Columns.Add(new DataColumn("quarter_start", typeof(string)));
            quarterlySuperTable.Columns.Add(new DataColumn("total_ote", typeof(double)));
            quarterlySuperTable.Columns.Add(new DataColumn("total_super_payable", typeof(double)));
            quarterlySuperTable.Columns.Add(new DataColumn("total_disbursed", typeof(double)));
            quarterlySuperTable.Columns.Add(new DataColumn("variance", typeof(double)));

            // Calculate variance for (employee, quarter) groupings
            foreach (var entry in employeeQuarterlyOtePay.OrderBy(e => e.Key))
            {
                var key = entry.Key;
                var otePaid = entry.Value;
                var superPayable = otePaid * SuperContributionPercentage;
                var superDisbursed = employeeQuarterlyDisbursements.GetValueOrDefault(key);
                var variance = superPayable - superDisbursed;

                quarterlySuperTable.Rows.Add(
                    key.empCode, key.quarter.ToString("M/d/yyyy"), Math.Round(otePaid, 2), Math.Round(superPayable, 2), Math.Round(superDisbursed, 2), Math.Round(variance, 2));
            }

            return quarterlySuperTable;
        }

        private Dictionary<(double empCode, DateOnly quarter), double> GetEmployeeQuarterlyOtePay(DataSet dataset)
        {
            // Create hashSet to filter for OTE payments
            var otePayCodesSet = dataset.Tables["PayCodes"].AsEnumerable()
                .Where(row => row.Field<string>("ote_treament") == "OTE")
                .Select(row => row.Field<string>("pay_code")).ToImmutableHashSet();

            // Group OTE pay by (employee_id, quarter)
            var quarterlyGroupedPayslips = dataset.Tables["Payslips"].AsEnumerable()
                .Where(row => otePayCodesSet.Contains(row.Field<string>("code")))
                .GroupBy(row =>
                {
                    var employeeCode = row.Field<double>("employee_code");
                    var date = row.Field<DateTime>("end");
                    var quarter = GetQuarterStartDate(date);

                    return (employeeCode, quarter);
                });

            // Return aggregated quarterly OTE pay for each employee
            return quarterlyGroupedPayslips.ToDictionary(group => group.Key, group =>
                group.Sum(row => row.Field<double>("amount"))
            );
        }

        private Dictionary<(double empCode, DateOnly quarter), double> GetEmployeeQuarterlyDisbursements(DataSet dataset)
        {
            // Group disbursements by (employee_id, quarter)
            var employeeQuarterlyDisbursements = dataset.Tables["Disbursements"].AsEnumerable()
                .GroupBy(row =>
                {
                    var employeeCode = row.Field<double>("employee_code");
                    var dateString = row.Field<string>("payment_made");
                    var date = DateTime.Parse(dateString.ToString()).Subtract(TimeSpan.FromDays(28));
                    var quarter = GetQuarterStartDate(date);

                    return (employeeCode, quarter);
                });

            // Return aggregated quarterly disbursements for each employee
            return employeeQuarterlyDisbursements.ToDictionary(group => group.Key, group =>
                group.Sum(row => row.Field<double>("sgc_amount"))
            );
        }

        private static DateOnly GetQuarterStartDate(DateTime date)
        {
            var quarterNumber = (date.Month - 1) / 3;
            return new DateOnly(date.Year, quarterNumber * 3 + 1, 1);
        }
    }
}
