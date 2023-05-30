## Solution
1. Sum OTE payments for each employee grouped by quarter
2. Sum disbursements for each employee grouped by quarter
3. Calculate variance between required disbursements and actual
4. Write to `EmployeeQuarterlySuperReport.csv` (in repo directory)

```
EmployeeQuarterlySuperReport {
    employee_code
    quarter_start
    total_ote_paid
    total_super_payable
    total_super_disbursed
    variance
}
```

## Running
```
cd .\YellowCanaryCodeChallenge
dotnet run '..\Sample Super Data.xlsx'
```

## Assumptions
- `pay_period_from` and `pay_period_to` can be ignored.
    - `payment_made` is used to assign disbursements to the relevant quarterly window
    - This means negative variance is allowed if disbursements > super_payable for a quarter
- sadly, output `quarter_start` is formatted the same as `end` in the Payslips sheet (`M/d/yyyy`)

## Notes
- "Fit for purpose"
    - Barebones solution w/ little fanfare
    - No configurability, hosted service or dependency injection
    - No Typed DataSets or parsing to models
- Optimistic
    - No error handling, validation, logging
    - Assumes excel file contains all sheets, properly formatted
- Included one test case based on the example