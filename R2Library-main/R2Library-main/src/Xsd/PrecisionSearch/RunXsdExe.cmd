"..\..\..\Libs\cURL\curl.exe" "http://www.medinfonow.com/wsmedinfonow2/xsd/PrecisionSearchCreateCustomerRetrieveRequest.xsd"  -o ".\CreateCustomerRequest.xsd"
"..\..\..\Libs\cURL\curl.exe" "http://www.medinfonow.com/wsmedinfonow2/xsd/CreateCustomerResponse.xsd"  -o ".\CreateCustomerResponse.xsd"

"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\NETFX 4.0 Tools\x64\xsd.exe" CreateCustomerRequest.xsd /c /outputdir:..\..\R2V2.WindowsService\WebServices
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\NETFX 4.0 Tools\x64\xsd.exe" CreateCustomerResponse.xsd /c /outputdir:..\..\R2V2.WindowsService\WebServices
pause

rem "..\..\..\Libs\cURL\curl.exe" "http://www.medinfonow.com/wsmedinfonow2/xsd/CreateCustomer.xsd"  -o ".\CreateCustomerRequest.xsd"
rem    [XmlAttribute(AttributeName = "noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
rem     public string noNamespaceSchemaLocation = @"http://www.medinfonow.com/wsmedinfonow2/xsd/PrecisionSearchCreateCustomerRetrieveRequest.xsd";

rem cURL --> http://www.paehl.com/open_source/?CURL_7.30.0
