
using System.Data;
using System.Text;
using ExcelDataReader;

namespace YellowCanaryCodeChallenge
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = args.FirstOrDefault() ?? "";
            var reportingService = new PaymentReportingService();

            try
            {
                var superDataSet = ReadToDataSet(path);
                var resultTable = reportingService.GenerateEmployeeQuarterlySuperReport(superDataSet);

                WriteToCsv(resultTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void WriteToCsv(DataTable resultTable)
        {
            var csvStringBuilder = new StringBuilder();
            csvStringBuilder.AppendLine(string.Join(',', resultTable.Columns.OfType<DataColumn>().Select(c => c.ColumnName)));

            foreach (var row in resultTable.Rows.OfType<DataRow>())
            {
                csvStringBuilder.AppendLine(string.Join(',', row.ItemArray));
            }

            Console.WriteLine(csvStringBuilder.ToString());
            File.WriteAllText($"../{resultTable.TableName}.csv", csvStringBuilder.ToString());
        }

        static DataSet ReadToDataSet(string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            var result = reader.AsDataSet(new()
            {
                ConfigureDataTable = (_) => new()
                {
                    UseHeaderRow = true
                }
            });

            return result;
        }
    }
}

