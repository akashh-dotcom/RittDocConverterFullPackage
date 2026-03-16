#region

using System;
using System.IO;
using System.Text;
using R2V2.Core.OrderRelay;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.WindowsService.Infrastructure.FileTransfer;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.OrderRelay
{
    public class PreludeOrderService
    {
        public const char Delimiter = '|';
        public const string LineBreak = "\r\n";

        private static readonly string[] HeaderFieldNames =
        {
            "CUSTOMER NUMBER",
            "CONTACT",
            "E-MAIL ADDRESS",
            "CUSTOMER PO#",
            "SHIP TO NUMBER",
            "BLIND SHIPMENT FLAG",
            "MARCIVE CARDS FLAG",
            "FOLLETT CORPORATE FLAG",
            "RESIDENTIAL FLAG",
            "ORDER TYPE",
            "JOB NUMBER",
            "SHIP VIA",
            "FREIGHT TERMS",
            "SHIPPER NUMBER",
            "SHIP TYPE",
            "PAYMENT TERMS",
            "WRITTEN BY",
            "ORDER DATE",
            "REQUIRED DATE",
            "ORDER SOURCE",
            "ACKNOWLEDGE FLAG",
            "AUTO CANCEL DATE",
            "ADD DISCOUNT %",
            "SHIP TO NAME",
            "SHIP TO ATTN",
            "SHIP TO ADDRESS ONE",
            "SHIP TO ADDRESS TWO",
            "SHIP TO ADDRESS THREE",
            "COUNTRY",
            "CITY",
            "STATE",
            "ZIP",
            "PHONE",
            "TAX JURISDICTION",
            "INSTRUCTIONS LINE 1",
            "INSTRUCTIONS LINE 2",
            "INSTRUCTIONS LINE 3",
            "HEADER NOTES",
            "CREDIT CARD TYPE",
            "CREDIT CARD NUMBER",
            "CREDIT CARD HOLDER NAME",
            "CREDIT CARD EXP DATE",
            "QUOTATION NUMBER",
            "ADMIN HOLD FLAG",
            "ADMIN HOLD MANAGER",
            "PROMOTION CODE",
            "PAYTRACE TRANSACTION ID",
            "INTERPAYMENTS TRANSACTION ID",
            "INTERPAYMENTS TRANSACTION FEE",
            "INTERPAYMENTS SERVICE FEE",
            "TRANSACTION FEE INCLUDED"
        };

        private static readonly string[] ItemFieldNames =
        {
            "PRODUCT NUMBER",
            "QUANTITY",
            "LINE PO NUMBER",
            "LINE NOTES",
            "LINE DISCOUNT %",
            "LINE LOCATION",
            "FUND CODE",
            "GIFT CERTIFICATE NUMBER",
            "GIFT CERTIFICATE NAME",
            "GIFT CERTIFICATE AMOUNT"
        };

        private static readonly string[] PaymentFieldNames = { "PAY TYPE", "PAYMENT AMOUNT" };

        private static readonly string[] FooterFieldNames =
        {
            "TERMS CODE",
            "OUTBOUND FREIGHT AMOUNT",
            "TAX AMOUNT",
            "GIFT CERTIFICATE NUMBER",
            "GIFT CERTIFICATE AMOUNT"
        };

        private readonly EmailQueueService _emailQueueService;
        private readonly ILog<PreludeOrderService> _log;
        private readonly WindowsServiceSettings _windowsServiceSettings;

        public PreludeOrderService(ILog<PreludeOrderService> log
            , WindowsServiceSettings windowsServiceSettings
            , EmailQueueService emailQueueService
        )
        {
            _log = log;
            _windowsServiceSettings = windowsServiceSettings;
            _emailQueueService = emailQueueService;
        }

        public string GenerateOrderFile(OrderMessage order)
        {
            var orderFile = new StringBuilder();

            // ****************************************
            // HEADER
            // ****************************************
            orderFile.Append("H").Append(Delimiter);
            // customer account number
            AppendNextValue(orderFile, order.AccountNumber, 6, Delimiter, "account number");

            // contact
            AppendNextValue(orderFile, order.ContactName, 30, Delimiter, "contact name");

            // email address
            AppendNextValue(orderFile, order.ConfirmationEmailAddress, 100, Delimiter.ToString(), "email address",
                true);

            // purchase order number
            AppendNextValue(orderFile, order.PurchaseOrderNumber, 20, Delimiter, "customer po #");

            // ship to number - M = manual (changes made to address), D = default (bill to address) or numeric value from ship to file.
            //SquishList #799
            AppendNextValue(orderFile,
                string.IsNullOrWhiteSpace(order.ShipToNumber)
                    ? _windowsServiceSettings.OrderRelayShipToNumber
                    : order.ShipToNumber, 6, Delimiter, "ship to number");

            // blind shipping flag
            AppendNextValue(orderFile, _windowsServiceSettings.OrderRelayBlindShipFlag, 1, Delimiter,
                "blind shipment flag");

            // marcive flag
            AppendNextValue(orderFile, "", 1, Delimiter, "marcive flag");

            // follett corp flag - Y = yes, N = no, currently not needed for web orders, default to N for now
            AppendNextValue(orderFile, _windowsServiceSettings.OrderRelayFollettCorpFlag, 1, Delimiter,
                "follett corp flag");

            // residential flag - Y = yes, N = no, currently not used, but leave in for future use, set to N
            AppendNextValue(orderFile, _windowsServiceSettings.OrderRelayResidentFlag, 1, Delimiter,
                "residential flag");

            // order type - 01 = regular, 04 = drop ship, 06 = direct to invoice (R2 sales), set to 01
            AppendNextValue(orderFile, _windowsServiceSettings.OrderRelayOrderType, 2, Delimiter, "order type");

            // job number - web order number
            AppendNextValue(orderFile, order.WebOrderNumber, 36, Delimiter, "job number");

            // ship via - user supplied, 01 = Standard, 02 = 2-day, 03 = Overnight
            AppendNextValue(orderFile, _windowsServiceSettings.OrderRelayShipVia, 2, Delimiter, "ship via");

            // freight terms -  01 = free freight, 10 = flat rate, if standard, 01, other 10, except guest user pass 10, (A new customer level flag needs to be passed from Prelude indicating free freight for standard.)
            AppendNextValue(orderFile, string.Empty, 2, Delimiter, "freight terms");

            // shipper number supplied by user
            AppendNextValue(orderFile, string.Empty, 10, Delimiter, "shipper number");

            // ship type - set to COL if shipper number is supplied
            AppendNextValue(orderFile, string.Empty, 3, Delimiter, "ship type");

            // payment terms - 01 = net 30, 03 = credit card (default payment terms need to be added to the customer file)
            AppendNextValue(orderFile, order.PaymentTerm, 2, Delimiter, "payment terms");

            // written by - always set to WEB
            AppendNextValue(orderFile, _windowsServiceSettings.OrderRelayWrittenBy, 3, Delimiter, "written by");

            // order date - Current date, format YYYYMMDD
            AppendNextValue(orderFile, $"{order.OrderDate:yyyyMMdd}", 8, Delimiter, "order date");

            // required date - user supplied, required shipping date, format YYYYMMDD
            AppendNextValue(orderFile, $"{order.RequiredDate:yyyyMMdd}", 8, Delimiter, "required date");

            // order source - W, triggers addition 1% in Prelude
            AppendNextValue(orderFile, order.OrderSource.ToString(), 1, Delimiter, "order source");

            // achnowledge flag - E=Prelude emails invoice
            AppendNextValue(orderFile, _windowsServiceSettings.OrderRelayAcknowledgeFlag, 1, Delimiter,
                "achnowledge flag");

            // auto cancel date - auto order cancel date equals current date + 365 or user supplied, format YYYYMMDD
            AppendNextValue(orderFile, $"{order.AutoCancelDate:yyyyMMdd}", 8, Delimiter, "auto cancel date");

            // additiona discount percentage - Percentage discount (Only used right of spring, octoberfest, etc.) -- Removed 11/8/2013 per Glenn's request
            AppendNextValue(orderFile, order.AdditionalDiscountPercentage > 0
                ? $"{order.AdditionalDiscountPercentage:0#}"
                : "", 2, Delimiter, "additiona discount percentage");

            // R2 doesn't really need ship to address since the product is not shipped, it is an online product
            AppendNextValue(orderFile, string.Empty, 30, Delimiter, "ship to name");
            AppendNextValue(orderFile, string.Empty, 30, Delimiter, "ship to attention");
            AppendNextValue(orderFile, string.Empty, 30, Delimiter, "ship to address 1");
            AppendNextValue(orderFile, string.Empty, 30, Delimiter, "ship to address 2");
            AppendNextValue(orderFile, string.Empty, 30, Delimiter, "ship to address 3");
            AppendNextValue(orderFile, string.Empty, 3, Delimiter, "ship to country");
            AppendNextValue(orderFile, string.Empty, 20, Delimiter, "ship to city");
            AppendNextValue(orderFile, string.Empty, 5, Delimiter, "ship to state");
            AppendNextValue(orderFile, string.Empty, 10, Delimiter, "ship to zip code");
            AppendNextValue(orderFile, string.Empty, 16, Delimiter, "ship to phone");

            // tax jurisdiction - Y=taxable, N=exempt (Need to get tax info from Prelude via ship to file.  All guest order will be Y which will force manual.)
            AppendNextValue(orderFile, string.Empty, 6, Delimiter, "tax jurisdiction");

            // instructions line 1 - printed on picking document, not used at this point
            AppendNextValue(orderFile, order.Instruction1, 40, Delimiter, "instructions line 1");

            // instructions line 2 - printed on picking document, not used at this point
            AppendNextValue(orderFile, order.Instruction2, 40, Delimiter, "instructions line 2");

            // instructions line 3 - printed on picking document, not used at this point
            AppendNextValue(orderFile, order.Instruction3, 40, Delimiter, "instructions line 3");

            // header notes - User supplied (Any orders with notes goto the manual queue.)
            AppendNextValue(orderFile, order.HeaderNotes, 255, Delimiter.ToString(), "header notes", true);

            // credit card type - User supplied (VISA=Visa, M/C=Master Card, AMEX=American Express)
            AppendNextValue(orderFile, string.Empty, 4, Delimiter, "credit card type");

            // credit card number - currently NOT encrypted but sent via sFTP
            AppendNextValue(orderFile, string.Empty, 20, Delimiter, "credit card number");

            // credit card holder's name
            AppendNextValue(orderFile, string.Empty, 30, Delimiter, "credit card holder's name");

            // credit card expire date
            AppendNextValue(orderFile, string.Empty, 5, Delimiter, "credit card expire date");

            //// element express token
            //AppendNextValue(orderFile, string.Empty, 50, Delimiter, "element express token");

            //// element express reference number
            //AppendNextValue(orderFile, string.Empty, 50, Delimiter, "element express reference number");

            // quotation number - Quotation number for performa accounts, future use
            AppendNextValue(orderFile, order.QuotationNumber, 8, Delimiter, "quotation number");

            // admin hold flag - Y/N for placing orders on admin hold in Prelude.  Set to Y for initial launch.
            AppendNextValue(orderFile, _windowsServiceSettings.OrderRelayAdminHoldFlag, 1, Delimiter,
                "admin hold flag");

            // admin hold mgr - Admin Hold Manager for web orders.  Set to 21 for initial launch.
            AppendNextValue(orderFile, _windowsServiceSettings.OrderRelayAdminHoldManager, 2, Delimiter,
                "admin hold mgr");

            // promotion code - Promotion Code to pass in orders for review (Rites of Spring, Oktoberfest, etc.)  Code will come from new Promotion table.  Entered by user at time or order placement.
            AppendNextValue(orderFile, order.PromotionCode, 20, Delimiter, "promotion code");

            // PayTrace Transaction Id
            AppendNextValue(orderFile, string.Empty, 50, Delimiter, "paytrace transaction id");
            // InterPayments Transaction Id
            AppendNextValue(orderFile, string.Empty, 50, Delimiter, "interpayments transaction id");
            // InterPayments Transaction Fee
            AppendNextValue(orderFile, string.Empty, 10, Delimiter, "interpayments transaction fee");
            // InterPayments Service Fee
            AppendNextValue(orderFile, string.Empty, 10, Delimiter, "interpayments service fee");
            // Transaction Fee Included
            AppendNextValue(orderFile, "N", 1, LineBreak, "transaction fee included");


            // ****************************************
            // ITEMS
            // ****************************************
            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    orderFile.Append("L").Append(Delimiter);

                    // product number - sku
                    AppendNextValue(orderFile, item.Sku, 20, Delimiter, "product number");

                    // quantity
                    AppendNextValue(orderFile, $"{item.Quantity}", 4, Delimiter, "quantity");

                    // line PO number
                    AppendNextValue(orderFile, string.Empty, 20, Delimiter, "line PO number");

                    // line remarks
                    AppendNextValue(orderFile, item.LineNotes, 255, Delimiter, "line remarks");

                    //AppendNextValue(orderFile, (item.PromotionDiscount > 0) ? string.Format("{0}", item.PromotionDiscount) : "", 2, Delimiter); // line discount percentage, only set it promotion like rites of sprinf or oktoberfest
                    // line discount percentage, only set it promotion like rites of spring or oktoberfest
                    if ((string.IsNullOrEmpty(order.PromotionCode) && !item.IsSpecialDiscount) ||
                        item.DiscountPercentage == decimal.Zero)
                    {
                        AppendNextValue(orderFile, "", 4, Delimiter, "line discount percentage");
                    }
                    else
                    {
                        AppendNextValue(orderFile, $"{item.DiscountPercentage * 100m:####}", 4, Delimiter,
                            "line discount percentage");
                    }

                    // line location - library only
                    AppendNextValue(orderFile, string.Empty, 10, Delimiter, "line location");

                    // fund code - library only
                    AppendNextValue(orderFile, string.Empty, 10, Delimiter, "fund code");

                    // gift certificate number - User supplied, verified against certificate table (AZ only)
                    AppendNextValue(orderFile, string.Empty, 15, Delimiter, "gift certificate number");

                    // gift certificate name - User supplied, verified against certificate table (AZ only)
                    AppendNextValue(orderFile, string.Empty, 30, Delimiter, "gift certificate name");

                    // gift certificate amount - User supplied, verified against certificate table (AZ only)
                    AppendNextValue(orderFile, string.Empty, 10, LineBreak, "gift certificate amount");
                }
            }

            // per the instructions from Glenn's email on 3/22/2010, just add empty values
            orderFile.AppendFormat("P{0}{0}{1}", Delimiter, LineBreak);


            // ****************************************
            // FOOTER
            // ****************************************
            orderFile.Append("F").Append(Delimiter);

            // terms code - Same values as Header Payment Terms (01=net 30, 03=credit card (completion screen))
            AppendNextValue(orderFile, string.Empty, 2, Delimiter, "terms code");

            // outbound freight amount - flat rate value or blank
            AppendNextValue(orderFile, string.Empty, 10, Delimiter, "outbound freight amount");

            // tax amount - blank, future
            AppendNextValue(orderFile, string.Empty, 10, Delimiter, "tax amount");

            // gift certificate number - blank, future - redemption sales sites
            AppendNextValue(orderFile, string.Empty, 15, Delimiter, "gift certificate number");

            // gift certificate amount - blank, future - redemption sales sites
            AppendNextValue(orderFile, string.Empty, 10, LineBreak, "gift certificate amount");

            var orderFileText = orderFile.ToString();

            return orderFileText;
        }

        private void AppendNextValue(StringBuilder sb, string value, int maxLength, char delimiter,
            string debugOnlyFieldName)
        {
            AppendNextValue(sb, value, maxLength, delimiter.ToString(), debugOnlyFieldName, false);
        }


        private void AppendNextValue(StringBuilder sb, string value, int maxLength, string delimiter,
            string debugOnlyFieldName)
        {
            AppendNextValue(sb, value, maxLength, delimiter, debugOnlyFieldName, false);
        }

        private void AppendNextValue(StringBuilder sb, string value, int maxLength, string delimiter,
            string debugOnlyFieldName, bool truncateIfTooLong)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Length > maxLength)
                {
                    if (debugOnlyFieldName == "credit card number")
                    {
                        var creditCardNumber = string.IsNullOrEmpty(value) ? "" :
                            value.Length >= 4 ? value :
                            new string('*', value.Length - 4) + value.Substring(value.Length - 4);
                        _log.DebugFormat("{0} - value: {1}, maxLength: {2}, file: \n{3}", debugOnlyFieldName,
                            creditCardNumber, maxLength, sb);
                        throw new Exception(
                            $"Invalid order file value, value is too long. value: {creditCardNumber}, maxLength: {maxLength}");
                    }

                    if (!truncateIfTooLong)
                    {
                        _log.DebugFormat("{0} - value: {1}, maxLength: {2}, file: \n{3}", debugOnlyFieldName, value,
                            maxLength, sb);
                        throw new Exception(
                            $"Invalid order file value, value is too long. value: {value}, maxLength: {maxLength}");
                    }

                    var truncatedValue = value.Substring(0, maxLength);

                    var errorMsg = new StringBuilder();
                    errorMsg.AppendFormat(
                            "Invalid order file value, value is too long, value is truncated to {0} characters.",
                            maxLength)
                        .AppendLine();
                    errorMsg.AppendFormat("Original {0} value: {1}", debugOnlyFieldName, value).AppendLine();
                    errorMsg.AppendFormat("Truncated {0} value: {1}", debugOnlyFieldName, truncatedValue).AppendLine();
                    _log.Error(errorMsg);

                    value = truncatedValue;
                }

                // strip carriage returns and line feeds
                var cleanedValue = value.Replace("\r", " ").Replace("\n", " ").Replace("|", "/");

                sb.AppendFormat("{0}{1}", cleanedValue, delimiter);
                return;
            }

            sb.AppendFormat("{0}", delimiter);
        }

        public bool SendOrderFileToPrelude(string orderFileText, OrderMessage orderMessage)
        {
            try
            {
                var orderFileName =
                    $"{_windowsServiceSettings.OrderRelayOrderFileNamePrefix}{orderMessage.WebOrderNumber}.txt";
                _log.DebugFormat("orderFileName: {0}, orderFileText: {1}", orderFileName, orderFileText);
                _log.DebugFormat("OrderRelaySendOrderFileToPrelude: {0}",
                    _windowsServiceSettings.OrderRelaySendOrderFileToPrelude);

                if (!_windowsServiceSettings.OrderRelaySendOrderFileToPrelude)
                {
                    _log.Warn("********************************************************************************");
                    _log.Warn("********************************************************************************");
                    _log.Warn("NO ORDER FILES ARE BEING SENT TO PRELUDE!!!");
                    _log.Warn("APPLICATION IS IN DEBUG MODE!!!");
                    _log.Warn("OrderRelaySendOrderFileToPrelude is set to False");
                    _log.Warn("********************************************************************************");
                    _log.Warn("********************************************************************************");

                    return true;
                }

                var ok = SendWebOrderFileText(orderFileName, orderFileText, out var errorMessage);
                //bool ok = SendWebOrderFile(orderFileName, localFilePath, out errorMessage);
                orderMessage.PreviousSendErrorMessage = errorMessage;
                return ok;
            }
            catch (Exception ex)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine("Error send order to Prelude!");
                errorMessage.AppendLine(orderFileText);
                errorMessage.AppendLine(ex.Message);
                _log.Error(errorMessage.ToString(), ex);
                orderMessage.PreviousSendErrorMessage = errorMessage.ToString();
                return false;
            }
        }

        public string LogOrderFileToDisk(string orderFileText, OrderMessage orderMessage)
        {
            try
            {
                var filename =
                    $"{orderMessage.WebOrderNumber}_{orderMessage.AccountNumber}_{orderMessage.OrderDate:yyyyMMdd-HHmmss}.txt";
                var path = Path.Combine(_windowsServiceSettings.OrderRelayLogDirectory, filename);

                var logFileText = new StringBuilder();
                logFileText.AppendLine(orderFileText);

                //Need remove the extra information.
                //The file on disk is what is being sent to S3 for Prelude Pickup.

                //logFileText.AppendLine();
                //logFileText.AppendLine(GenerateOrderFileReadable(orderFileText));

                using (var file = new StreamWriter(path, false))
                {
                    file.Write(logFileText.ToString());
                }

                return filename;
            }
            catch (Exception ex)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine("Error logging order file to disk!");
                errorMessage.AppendLine(orderFileText);
                errorMessage.AppendLine(ex.Message);
                _log.Error(errorMessage.ToString(), ex);
                return null;
            }
        }

        public bool SendInternalOrderEmail(string orderFileText, OrderMessage orderMessage)
        {
            try
            {
                var emailMessageHtml = GenerateOrderFileHtml(orderFileText);
                var emailMessage = CreateOrderEmail(emailMessageHtml, orderMessage);
                return _emailQueueService.QueueEmailMessage(emailMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine("Error sending internal order email!");
                errorMessage.AppendLine(orderFileText);
                errorMessage.AppendLine(ex.Message);
                _log.Error(errorMessage.ToString(), ex);
                return false;
            }
        }

        private string GenerateOrderFileHtml(string orderFileText)
        {
            var parsedOrderFile = new StringBuilder();

            var lines = orderFileText.Replace("\r", "").Split('\n');

            foreach (var line in lines)
            {
                _log.DebugFormat("line.Length: {0}", line.Length);
                if (line.Length > 0)
                {
                    var fieldValues = line.Split(Delimiter);

                    switch (fieldValues[0])
                    {
                        case "H":
                            AppendValuesAsHtml(parsedOrderFile, fieldValues, HeaderFieldNames, "HEADER LINE:", line);
                            break;
                        case "F":
                            AppendValuesAsHtml(parsedOrderFile, fieldValues, FooterFieldNames, "FOOTER LINE:", line);
                            break;
                        case "L":
                            AppendValuesAsHtml(parsedOrderFile, fieldValues, ItemFieldNames, "ITEM LINE:", line);
                            break;
                        case "P":
                            AppendValuesAsHtml(parsedOrderFile, fieldValues, PaymentFieldNames, "PAYMENT LINE:", line);
                            break;
                        default:
                            parsedOrderFile.AppendFormat("INVALID LINE!{0}", LineBreak);
                            break;
                    }

                    parsedOrderFile.AppendFormat("{0}<br />{0}", LineBreak);
                }
            }

            _log.Debug(parsedOrderFile.ToString());
            return parsedOrderFile.ToString();
        }


        private static void AppendValuesAsHtml(StringBuilder sb, string[] values, string[] fieldNames, string header,
            string line)
        {
            sb.AppendFormat("<div class=\"lineHeader\">{0}</div>{1}", header, LineBreak);
            sb.AppendFormat("<div class=\"fileTextLine\">{0}</div>{1}", line, LineBreak);
            sb.AppendFormat("<table border=\"0\" cellpadding=\"0\" cellspacing=\"1\">{0}", LineBreak);
            for (var i = 1; i < values.Length; i++)
            {
                sb.AppendFormat(
                    "\t<tr><td align=\"right\"><div class=\"valueLabel\">&nbsp;&nbsp;&nbsp;{0}:&nbsp;</div></td><td><div class=\"valueText\">{1}</div></td>{2}",
                    fieldNames[i - 1], values[i], LineBreak);
            }

            sb.AppendFormat("</table>{0}", LineBreak);
        }

        public string GenerateOrderFileReadable(string orderFileText)
        {
            var parsedOrderFile = new StringBuilder();

            var lines = orderFileText.Replace("\r", "").Split('\n');

            foreach (var line in lines)
            {
                _log.DebugFormat("line.Length: {0}", line.Length);
                if (line.Length > 0)
                {
                    var fieldValues = line.Split(Delimiter);

                    switch (fieldValues[0])
                    {
                        case "H":
                            AppendValuesAsText(parsedOrderFile, fieldValues, HeaderFieldNames, "HEADER LINE:", line);
                            break;
                        case "F":
                            AppendValuesAsText(parsedOrderFile, fieldValues, FooterFieldNames, "FOOTER LINE:", line);
                            break;
                        case "L":
                            AppendValuesAsText(parsedOrderFile, fieldValues, ItemFieldNames, "ITEM LINE:", line);
                            break;
                        case "P":
                            AppendValuesAsText(parsedOrderFile, fieldValues, PaymentFieldNames, "PAYMENT LINE:", line);
                            break;
                        default:
                            parsedOrderFile.AppendLine("INVALID LINE!");
                            break;
                    }

                    parsedOrderFile.AppendLine();
                }
            }

            _log.Debug(parsedOrderFile.ToString());
            return parsedOrderFile.ToString();
        }


        private static void AppendValuesAsText(StringBuilder sb, string[] values, string[] fieldNames, string header,
            string line)
        {
            sb.AppendLine().AppendLine(header);
            sb.AppendLine(line);
            for (var i = 1; i < values.Length; i++)
            {
                sb.AppendFormat("{0} = {1}", fieldNames[i - 1], values[i]).AppendLine();
            }
        }

        private bool SendWebOrderFileText(string orderFileName, string orderFileText, out string errorMessage)
        {
            try
            {
                _log.InfoFormat("Attempting to sftp file {0}.", orderFileName);

                var lines = orderFileText.Split('\n');
                foreach (var line in lines)
                {
                    var fields = line.Split('|');
                    _log.DebugFormat("{0} fields: {1}", fields[0], fields.Length);
                }

                //Transmit order file via SFTP
                var sftp = new SFtp
                {
                    Hostname = _windowsServiceSettings.OrderRelaySFtpHost,
                    Username = _windowsServiceSettings.OrderRelaySFtpUsername,
                    Password = _windowsServiceSettings.OrderRelaySFtpPassword
                };

                if (sftp.WriteStringAsFile(orderFileText, orderFileName))
                {
                    _log.InfoFormat("sFTP successful for file {0}.", orderFileName);
                    errorMessage = null;
                    return true;
                }

                _log.WarnFormat("sFTP ERROR!!!");
                errorMessage = sftp.ErrorMessage;
                return false;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                errorMessage = ex.Message;
                return false;
            }
        }

        private EmailMessage
            CreateOrderEmail(string orderMessageHtml, OrderMessage orderMessage) //, bool wasSuccessful, bool isRetry)
        {
            var subject = string.IsNullOrWhiteSpace(orderMessage.PreviousSendErrorMessage)
                ? $"Order {orderMessage.WebOrderNumber} was successfully reprocessed."
                : $"Unable to {(orderMessage.SendAttemptCount > 0 ? "re" : "")}process order {orderMessage.WebOrderNumber} at this time.";

            subject = $"{subject} - {Environment.MachineName}";

            var htmlMessageBody = new StringBuilder()
                .AppendLine("<html><head><title>R2 Order File Results</title>")
                .AppendLine("<style type=\"text/css\">")
                .AppendLine("body { font-family:Courier New; font-size:12; font-weight:bold; }")
                .AppendLine(".fileTextLine { }")
                .AppendLine(
                    ".lineHeader { font-family: Arial, Helvetica, sans-serif; font-size: 12px; font-weight:bold; }")
                .AppendLine(".valueText { font-size:12; font-weight:bold; }")
                .AppendLine(".valueLabel { font-family: Arial, Helvetica, sans-serif; font-size:11; }")
                .AppendLine(
                    ".errorMsg { font-family: Arial, Helvetica, sans-serif; font-size: 12px; font-weight:bold; font-color:#a00; }")
                .AppendLine("</style>")
                .AppendLine("</head>")
                .AppendLine("<body>");

            try
            {
                if (!string.IsNullOrEmpty(orderMessage.PreviousSendErrorMessage))
                {
                    htmlMessageBody.AppendFormat("</div class=\"errorMsg\">{0}</div><br/>",
                        orderMessage.PreviousSendErrorMessage);
                }

                htmlMessageBody.Append(orderMessageHtml);
            }
            catch (Exception ex)
            {
                _log.Warn("Exception creating email message");
                _log.Error(ex.Message, ex);
                htmlMessageBody.AppendFormat("</div class=\"errorMsg\">ERORR CREATING ORDER FILE: {0}</div>",
                    ex.Message);
            }

            htmlMessageBody.Append("</body></html>");

            var email = new EmailMessage
            {
                Subject = subject,
                Body = htmlMessageBody.ToString(),
                IsHtml = true,
                FromAddress = _windowsServiceSettings.OrderRelayInternalOrderEmailFromAddress,
                FromDisplayName = "R2 Library Auto Order Process"
            };

            email.AddToRecipients(_windowsServiceSettings.OrderRelayInternalOrderEmailToAddresses, ',');

            return email;
        }
    }
}