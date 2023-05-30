using YellowCanaryCodeChallenge;
using System.Data;

namespace YellowCanaryCodeChallenge.Test
{
    public class PaymentReportingServiceTests
    {
        private PaymentReportingService reportingService;

        [SetUp]
        public void Setup()
        {
            reportingService = new PaymentReportingService();
        }

        [Test]
        public void TestReturnsExpectedResults()
        {
            var dataset = setupDataSet();
            var employeeCode = 12345;

            var payslipsTable = dataset.Tables["Payslips"];
            var payslips = new List<object?[]> {
                new object[] {"1", new DateTime(2023, 1, 1), employeeCode, "Salary", 5000 },
                new object[] {"1", new DateTime(2023, 1, 1), employeeCode, "Overtime - Weekend", 1500 },
                new object[] {"1", new DateTime(2023, 1, 1), employeeCode, "Super Withheld", 475 },
                new object[] {"2", new DateTime(2023, 2, 1), employeeCode, "Salary", 5000 },
                new object[] {"2", new DateTime(2023, 2, 1), employeeCode, "Super Withheld", 475 },
                new object[] {"3", new DateTime(2023, 3, 1), employeeCode, "Salary", 5000 },
                new object[] {"3", new DateTime(2023, 3, 1), employeeCode, "Super Withheld", 475 },
            };
            payslips.ForEach((payslip) => payslipsTable.Rows.Add(payslip));

            var disbursementsTable = dataset.Tables["Disbursements"];
            var disbursements = new List<object?[]> {
                new object[] { 500, new DateTime(2023, 2, 27).ToString(), null, null, employeeCode },
                new object[] { 500, new DateTime(2023, 3, 30).ToString(), null, null, employeeCode },
                new object[] { 500, new DateTime(2023, 4, 30).ToString(), null, null, employeeCode },
            };
            disbursements.ForEach((disbursement) => disbursementsTable.Rows.Add(disbursement));

            var resultTable = reportingService.GenerateEmployeeQuarterlySuperReport(dataset);
            var rows = resultTable.Rows.OfType<DataRow>().ToList();

            Assert.That(rows.Count, Is.EqualTo(1));

            var row = rows.First();
            Assert.Multiple(() =>
            {
                Assert.That(row["employee_code"], Is.EqualTo(employeeCode));
                Assert.That(row["quarter_start"], Is.EqualTo("1/1/2023"));
                Assert.That(row["total_ote"], Is.EqualTo(15000));
                Assert.That(row["total_super_payable"], Is.EqualTo(1425));
                Assert.That(row["total_disbursed"], Is.EqualTo(1000));
                Assert.That(row["variance"], Is.EqualTo(425));
            });
        }

        private DataSet setupDataSet()
        {

            var dataset = new DataSet();

            var payslipsTable = new DataTable("Payslips");
            payslipsTable.Columns.Add(new DataColumn("payslip_id", typeof(string)));
            payslipsTable.Columns.Add(new DataColumn("end", typeof(DateTime)));
            payslipsTable.Columns.Add(new DataColumn("employee_code", typeof(double)));
            payslipsTable.Columns.Add(new DataColumn("code", typeof(string)));
            payslipsTable.Columns.Add(new DataColumn("amount", typeof(double)));
            dataset.Tables.Add(payslipsTable);

            var disbursementsTable = new DataTable("Disbursements");
            disbursementsTable.Columns.Add(new DataColumn("sgc_amount", typeof(double)));
            disbursementsTable.Columns.Add(new DataColumn("payment_made", typeof(string)));
            disbursementsTable.Columns.Add(new DataColumn("pay_period_from", typeof(string)));
            disbursementsTable.Columns.Add(new DataColumn("pay_period_to", typeof(string)));
            disbursementsTable.Columns.Add(new DataColumn("employee_code", typeof(double)));
            dataset.Tables.Add(disbursementsTable);

            var paycodesTable = new DataTable("PayCodes");
            paycodesTable.Columns.Add(new DataColumn("pay_code", typeof(string)));
            paycodesTable.Columns.Add(new DataColumn("ote_treament", typeof(string)));
            dataset.Tables.Add(paycodesTable);

            var paycodesMap = new Dictionary<string, string>()
            {
                ["Salary"] = "OTE",
                ["Site Allowance"] = "OTE",
                ["Overtime - Weekend"] = "Not OTE",
                ["Super Withheld"] = "Not OTE",
            };

            foreach (var entry in paycodesMap)
            {
                paycodesTable.Rows.Add(entry.Key, entry.Value);
            }

            return dataset;
        }
    }
}