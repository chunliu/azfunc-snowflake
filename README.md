# azfunc-snowflake

The sample code demonstrates generating snowflake ids with Azure Functions.

## How to run it

- Deploy the code to an Azure Function App.
- Add 2 settings to the configuration of the Function App.
    - `AzureWebJobsWorkerIdsTable` - A connection string with SAS token to a Storage Table account. The code stores the worker id in the table.
    - `DATACENTER_ID` - A number from 0 - 31. If you deploy the code to multiple regions, each region should have different number.

## Note

The code is only for the demonstration purpose. Use it at your own risk.
